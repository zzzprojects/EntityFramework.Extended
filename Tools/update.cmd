@echo off

del MSBuildTasks /q /f /s
rd MSBuildTasks /S /Q
NuGet.exe install MSBuildTasks -OutputDirectory .\ -ExcludeVersion -NonInteractive


del xunit.runners /q /f /s
rd xunit.runners /S /Q
NuGet.exe install xunit.runners -OutputDirectory .\ -ExcludeVersion -NonInteractive