@echo off
setlocal enableextensions enabledelayedexpansion
set OD=%CD%
cd /d %~dp0
set DESTINATION=C:\Tools

set BUILD_NUMBER=1.0.0.5

:## Make msbuild available on the Path.
set "Path=%Path%;%ProgramFiles(x86)%\MSBuild\12.0\bin\amd64;%CD%\.nuget"

:BUILD
REM ### Build the library
echo ...compiling...
msbuild /m /nologo /p:Configuration=Release /p:Platform="Any CPU" Boinst.NAntExtensions.sln

REM ### Pack nuget for Boinst.NAntExtensions
cd /d "%~dp0./Boinst.NAntExtensions"
del /f /q *.nupkg
nuget.exe pack -Prop Configuration=Release -NonInteractive -Version %BUILD_NUMBER% -Verbosity normal -IncludeReferencedProjects || goto :ERROR

REM ### Pack nuget for Boinst.NAntExtensions.TeamCity
cd /d "%~dp0./Boinst.NAntExtensions.TeamCity"
del /f /q *.nupkg
nuget.exe pack -Prop Configuration=Release -NonInteractive -Version %BUILD_NUMBER% -Verbosity normal -IncludeReferencedProjects || goto :ERROR

goto :EXIT

:ERROR
echo Build FAILED with exit code %ERRORLEVEL% && cd /d "%~dp0." && endlocal && exit /b %ERRORLEVEL%

:EXIT
cd /d %OD%
endlocal
exit /b 0

:EOF