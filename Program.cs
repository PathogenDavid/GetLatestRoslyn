using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

const string packageSource = @"https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json";
const string packageName = "Microsoft.Net.Compilers.Toolset";

JsonDocumentOptions jsonOptions = new()
{
    AllowTrailingCommas = true,
    CommentHandling = JsonCommentHandling.Skip
};

using HttpClient http = new();

// Query the index for the RegistrationsBaseUrl service
string indexJson = await http.GetStringAsync(packageSource);
JsonDocument index = JsonDocument.Parse(indexJson, jsonOptions);
string registrationsBaseUrl = index.RootElement.GetProperty("resources")
    .EnumerateArray().First(resource => resource.GetProperty("@type").GetString() == "RegistrationsBaseUrl/3.6.0")
    .GetProperty("@id").GetString();

// Query the package
string packageInfoJson = await http.GetStringAsync(registrationsBaseUrl + packageName.ToLowerInvariant() + "/index.json");
JsonDocument packageInfo = JsonDocument.Parse(packageInfoJson, jsonOptions);

// We assume the first catalog entry is the newest and that Azure Devops returns items in the first request
// (Technically the NuGet API spec allows the request to be forwarded, in which case there are no items.)
JsonElement catalogEntry = packageInfo.RootElement.GetProperty("items")[0].GetProperty("items")[0].GetProperty("catalogEntry");

// Write out latest version
string version = catalogEntry.GetProperty("version").GetString();
Console.Write($"Latest version: {version}");

// Write out the time since the version was published
string publishedOnString = catalogEntry.GetProperty("published").GetString();

if (DateTime.TryParse(publishedOnString, out DateTime publishedOn))
{
    TimeSpan timeSincePublish = DateTime.Now - publishedOn;

    Console.Write(" (Published ");
    Console.Write((int)Math.Floor(timeSincePublish.TotalDays) switch
    {
        0 => "today",
        1 => "yesterday",
        int other => $"{other} days ago"
    });
    Console.Write(")");
}

// Finish the line
Console.WriteLine();

// Copy the latest version to the clipboard
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
#pragma warning disable CS0219 // Workaround for Roslyn bug https://github.com/dotnet/roslyn/issues/47619
    const string kernel32 = "Kernel32.dll";
    const string user32 = "User32.dll";
#pragma warning restore CS0219
    [DllImport(kernel32)] static extern IntPtr GetConsoleWindow();

    [DllImport(user32, SetLastError = true)] static extern bool OpenClipboard(IntPtr newOwner);
    [DllImport(user32, SetLastError = true)] static extern bool EmptyClipboard();
    [DllImport(user32, SetLastError = true)] static extern IntPtr SetClipboardData(uint format, IntPtr memoryHandle);
    [DllImport(user32, SetLastError = true)] static extern bool CloseClipboard();

    [DllImport(kernel32, SetLastError = true)] static extern IntPtr GlobalAlloc(uint flags, nint bytes);
    [DllImport(kernel32, SetLastError = true)] static extern IntPtr GlobalLock(IntPtr handle);
    [DllImport(kernel32, SetLastError = true)] static extern bool GlobalUnlock(IntPtr handle);
    [DllImport(kernel32, SetLastError = true)] static extern IntPtr GlobalFree(IntPtr handle);

    const uint CF_TEXT = 1;
    const uint GMEM_MOVEABLE = 0x0002;
    const uint GMEM_ZEROINIT = 0x0040;

    void SetClipboardString(string data)
    {
        void Win32Error(string message)
            => Console.Error.WriteLine($"{message}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");

        // Open and clear the clipboard
        IntPtr consoleWindowHandle = GetConsoleWindow();
        if (consoleWindowHandle == IntPtr.Zero)
        {
            Console.Error.WriteLine("Failed to get the console window handle.");
            return;
        }

        if (!OpenClipboard(consoleWindowHandle))
        {
            Win32Error("Failed to open the clipboard");
            return;
        }

        IntPtr dataHandle = IntPtr.Zero;

        try
        {
            if (!EmptyClipboard())
            {
                Win32Error("Failed to empty the clipboard");
                return;
            }

            // Convert the data to an ASCII string
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            // Allocate memory for the clipboard contents
            // +1 for null terminator
            // We zero-initialize the memory so we don't have to bother with setting the null terminator
            dataHandle = GlobalAlloc(GMEM_MOVEABLE | GMEM_ZEROINIT, dataBytes.Length + 1);
            if (dataHandle == IntPtr.Zero)
            {
                Win32Error("Failed to allocate clipboard memory");
                return;
            }

            // Populate the clipboard contents
            IntPtr dataPtr = GlobalLock(dataHandle);
            if (dataPtr == IntPtr.Zero)
            {
                Win32Error("Failed to lock the clipboard memory");
                return;
            }

            Marshal.Copy(dataBytes, 0, dataPtr, dataBytes.Length);

            if (!GlobalUnlock(dataHandle) && Marshal.GetLastWin32Error() != 0)
            {
                Win32Error("Failed to unlock the clipboard memory");
                return;
            }

            // Set the clipboard contents
            if (SetClipboardData(CF_TEXT, dataHandle) == IntPtr.Zero)
            {
                Win32Error("Failed to set the clipboard data");
                return;
            }
        }
        finally
        {
            // If the dataPtr isn't cleared, free the memory
            if (dataHandle != IntPtr.Zero)
            {
                if (GlobalFree(dataHandle) != IntPtr.Zero)
                { Win32Error("Failed to free clipboard memory"); }
            }

            if (!CloseClipboard())
            { Win32Error("Failed to close the clipboard"); }
        }
    }

    SetClipboardString(version);
}
