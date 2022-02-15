# Get Latest Roslyn Version

[![MIT Licensed](https://img.shields.io/github/license/infectedlibraries/clangsharp.pathogen?style=flat-square)](LICENSE.txt)

This repository houses a tiny C# tool for fetching the version of latest daily build of `Microsoft.Net.Compilers.Toolset` and printing it to the console and (on Windows) copying it to the clipboard.

This program doesn't exactly embody best practices, I mainly created it to solve a very specific need and as an excuse to explore C# 9 features (which were still in preview at the time.)

## Building

Building requires .NET 6 and can be built by simply using `dotnet build` or Visual Studio 2022.

## Frequently Anticipated Questions

### Why?

I was sick of checking the Azure DevOps page. (Also checking from Visual Studio just locks things up.)

### Why is there no error checking in the JSON parsing but tons in the clipboard section?

I plead the fifth. (Also I tend to write a lot more Win32 interop code than I do web stuff, so it was out of habit.)

### No seriously, the clipboard code is over half the program!

Yup. Gotta love "handing" errors via return codes.

### Your mistreatment of the `System.Text.Json` API is atrotious!

Not a question, but also...Sorry 😬

If there's a succinct way to traverse the Json documents here, I wouldn't mind hearing about them!

### Does it work on Linux?

Yup, but it won't copy the latest version to the clipboard.

### Can I put this into a mission critical scenario?

Please don't! It was written very lazily. It does not handle certain situations gracefully or at all. (The reason I made it print the days since the package was published was so I'll notice if Azure DevOps ever starts ordering the results in a different way.)
