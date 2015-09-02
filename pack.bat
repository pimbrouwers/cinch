nuget restore Cinch.net40.sln
nuget restore Cinch.net45.sln

%FrameworkDir%%FrameworkVersion%\msbuild.exe Cinch.net40.sln /t:Build /p:Configuration=Release /m
%FrameworkDir%%FrameworkVersion%\msbuild.exe Cinch.net45.sln /t:Build /p:Configuration=Release /m

nuget pack Cinch.nuspec

%FrameworkDir%%FrameworkVersion%\msbuild.exe Cinch.net40.sln /t:Build /p:Configuration=Debug /m
%FrameworkDir%%FrameworkVersion%\msbuild.exe Cinch.net45.sln /t:Build /p:Configuration=Debug /m

nuget pack Cinch.symbols.nuspec -symbols