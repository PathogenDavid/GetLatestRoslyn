# Get Latest Roslyn Version

[![MIT Licensed](https://img.shields.io/github/license/infectedlibraries/clangsharp.pathogen?style=flat-square)](LICENSE.txt)

This repository houses a tiny C#9 tool for fetching the version of latest daily build of [`Microsoft.Net.Compilers.Toolset`](https://dev.azure.com/dnceng/public/_packaging?_a=package&feed=dotnet-tools&package=Microsoft.Net.Compilers.Toolset&version=3.8.0-4.20460.4&protocolType=NuGet) and printing it to the console and (on Windows) copying it to the clipboard.

In the spirit of the bleeding edge this project was intended to aid, this project uses:

* The latest Roslyn daily
* [Local function attributes and extern local funcions](https://github.com/dotnet/csharplang/blob/8cb28ad5601b090a9bf4f0c622cab7e620339286/proposals/csharp-9.0/local-function-attributes.md)
* [Native-sized integers](https://github.com/dotnet/csharplang/blob/8cb28ad5601b090a9bf4f0c622cab7e620339286/proposals/csharp-9.0/native-integers.md)
* [Target-typed `new()`](https://github.com/dotnet/csharplang/blob/8cb28ad5601b090a9bf4f0c622cab7e620339286/proposals/csharp-9.0/target-typed-new.md)
* [Top-level statements](https://github.com/dotnet/csharplang/blob/8cb28ad5601b090a9bf4f0c622cab7e620339286/proposals/csharp-9.0/top-level-statements.md)
* [`AnalysisLevel` AKA warning waves](https://devblogs.microsoft.com/dotnet/automatically-find-latent-bugs-in-your-code-with-net-5/) (Not that it's currently doing anything)

That being said, this program doesn't exactly embody best practices.

## Building

Building requires .NET 5 (I used 5.0.100-preview.8.20417.9) and can be built by simply using `dotnet build` or Visual Studio 2019. (You may get Intellisense errors without Visual Studio 2019 Preview.)

## Frequently Anticipated Questions

### Why?

I was sick of checking the Azure DevOps page.

### Why is there no error checking in the JSON parsing but tons in the clipboard section?

I plead the fifth. (Also I tend to write a lot more Win32 interop code than I do web stuff, so it was out of habit.)

### No seriously, the clipboard code is over half the program!

Yup. Gotta love "handing" errors via return codes.

### Your mistreatment of the `System.Text.Json` API is atrotious!

Not a question, but also...Sorry 😬

If there's a succinct way to traverse the Json documents here, I wouldn't mind hearing about them!

### Does it work on Linux?

It works on WSL! (You might have trouble building until [roslyn#46772](https://github.com/dotnet/roslyn/issues/46772) is fixed though.)

### Should I be using `Microsoft.Net.Compilers.Toolset`

Probably not! [See the official documentation for more info.](https://github.com/dotnet/roslyn/blob/91e949823f7d75f5764b6a0928b6bb11eea614d0/docs/compilers/Compiler%20Toolset%20NuPkgs.md)

### Can I put this into a mission critical scenario?

Please don't! It was written very lazily. It does not handle certain situations gracefully or at all. (The reason I made it print the days since the package was published was so I'll notice if Azure DevOps ever starts ordering the results in a different way.)
