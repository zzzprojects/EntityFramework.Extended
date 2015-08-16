%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Source\EntityFramework.Extended\EntityFramework.Extended.net40.csproj /target:Clean
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Source\EntityFramework.Extended\EntityFramework.Extended.net40.csproj /P:Configuration=Release

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Source\EntityFramework.Extended\EntityFramework.Extended.net45.csproj /target:Clean
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Source\EntityFramework.Extended\EntityFramework.Extended.net45.csproj /P:Configuration=Release


tools\nuget.exe pack Source\EntityFramework.Extended\EntityFramework.Extended.nuspec