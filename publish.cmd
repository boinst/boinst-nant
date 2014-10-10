@echo off
setlocal enableextensions enabledelayedexpansion
set OD=%CD%
cd /d %~dp0

call "%~dp0./build.cmd"

set BUILD_NUMBER=1.0.0.5

:PUBLISH
nuget push "%~dp0./Boinst.NAntExtensions/Boinst.NAntExtensions.%BUILD_NUMBER%.nupkg" || goto :ERROR
REM ## nuget push "%~dp0./Boinst.NAntExtensions/Boinst.NAntExtensions.%BUILD_NUMBER%.symbols.nupkg" || goto :ERROR
nuget push "%~dp0./Boinst.NAntExtensions.TeamCity/Boinst.NAntExtensions.TeamCity.%BUILD_NUMBER%.nupkg" || goto :ERROR
REM ## nuget push "%~dp0./Boinst.NAntExtensions.TeamCity/Boinst.NAntExtensions.TeamCity.%BUILD_NUMBER%.symbols.nupkg" || goto :ERROR

:EXIT
cd /d %OD%
endlocal
exit /b 0

:EOF