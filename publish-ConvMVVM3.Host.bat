@echo off
setlocal EnableExtensions DisableDelayedExpansion
rem Publish only ConvMVVM3.Host. Version and dependencies come from its csproj.
rem Set NUGET_API_KEY in the calling shell. Never store the key in this file.
rem Optional: set NUGET_SOURCE to override the nuget.org push endpoint.
rem --pack-only builds and creates packages without requiring a key or uploading.

set "PACKAGE_ID=ConvMVVM3.Host"
set "PROJECT=%~dp0ConvMVVM3\%PACKAGE_ID%\%PACKAGE_ID%.csproj"
set "PACK_ONLY=0"
if /i "%~1"=="--help" goto :help
if /i "%~1"=="--pack-only" set "PACK_ONLY=1"
if not "%~1"=="" if /i not "%~1"=="--pack-only" goto :usage_error
if not "%~2"=="" goto :usage_error

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: Install the .NET SDK required by the project.
    exit /b 1
)
if not exist "%PROJECT%" (
    echo ERROR: Project not found: "%PROJECT%"
    exit /b 1
)
if "%PACK_ONLY%"=="0" if not defined NUGET_API_KEY (
    echo ERROR: Set the NUGET_API_KEY environment variable before publishing.
    echo Use --pack-only to build packages without uploading.
    exit /b 1
)
if not defined NUGET_SOURCE set "NUGET_SOURCE=https://api.nuget.org/v3/index.json"

rem A new directory prevents accidentally uploading packages from earlier builds.
set "OUTPUT=%~dp0ConvMVVM3\%PACKAGE_ID%\bin\Release\nuget-publish\%RANDOM%-%RANDOM%-%RANDOM%"
if exist "%OUTPUT%" (
    echo ERROR: Output directory already exists. Run this script again.
    exit /b 1
)
pushd "%~dp0"
if errorlevel 1 exit /b 1

echo Building and packing %PACKAGE_ID% in Release...
rem Override GeneratePackageOnBuild to allow pack to build before packaging.
dotnet pack "%PROJECT%" --configuration Release --output "%OUTPUT%" -p:GeneratePackageOnBuild=false
if errorlevel 1 goto :failed

set "PACKAGE_COUNT=0"
set "PACKAGE_FILE="
for %%P in ("%OUTPUT%\%PACKAGE_ID%.*.nupkg") do if exist "%%~fP" (
    set /a PACKAGE_COUNT+=1 >nul
    set "PACKAGE_FILE=%%~fP"
)
if not "%PACKAGE_COUNT%"=="1" (
    echo ERROR: Expected exactly one main NuGet package in "%OUTPUT%".
    popd
    exit /b 1
)
if "%PACK_ONLY%"=="1" (
    echo Package created: "%PACKAGE_FILE%"
    echo Upload skipped.
    popd
    exit /b 0
)

echo Publishing "%PACKAGE_FILE%"...
rem The adjacent .snupkg is handled by NuGet's symbol-package publishing behavior.
rem Existing package versions fail: increment Version in the csproj before retrying.
dotnet nuget push "%PACKAGE_FILE%" --source "%NUGET_SOURCE%" --api-key "%NUGET_API_KEY%" --timeout 300
if errorlevel 1 goto :failed

echo Publish request completed for %PACKAGE_ID%.
echo Wait for NuGet validation and indexing before consuming a new version.
popd
exit /b 0

:failed
set "RESULT=%ERRORLEVEL%"
echo ERROR: %PACKAGE_ID% failed. See the dotnet output above.
echo Any generated packages are in "%OUTPUT%".
popd
exit /b %RESULT%

:usage_error
echo ERROR: Unsupported arguments.
call :help
exit /b 1

:help
echo Usage: %~nx0 [--pack-only ^| --help]
echo   No arguments: build, pack, and publish %PACKAGE_ID%.
echo   --pack-only: build and pack without uploading.
echo   Version and dependency versions are read from the project file.
echo   Publishing requires NUGET_API_KEY in the environment.
echo   NUGET_SOURCE is optional and defaults to nuget.org.
exit /b 0
