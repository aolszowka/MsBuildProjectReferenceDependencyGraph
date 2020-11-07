@ECHO OFF

SET PACKAGE_NAMES=msbuildprojectreferencedependencygraph,processdependencygraph,processparallelability
SET BUILD_CONFIGURATION=Debug

pushd ..

FOR %%P IN (%PACKAGE_NAMES%) DO ( CALL :ClearExistingPackage %%P )

ECHO.
ECHO Delete Existing Packs
IF EXIST nupkg (
    RMDIR /q /s nupkg
)

ECHO.
ECHO Restore
dotnet restore
dotnet tool restore --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json

ECHO.
ECHO Format / Standardize Files
dotnet format --fix-style warn --fix-analyzers warn

ECHO.
ECHO Build
dotnet build -graphBuild -maxCpuCount --configuration %BUILD_CONFIGURATION%

ECHO.
ECHO Test
dotnet test --configuration %BUILD_CONFIGURATION%

IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Cannot build a package on error; Fix the tests.
    GOTO :Finally
)

ECHO.
ECHO Packing
dotnet pack --configuration %BUILD_CONFIGURATION% --version-suffix "dev"

IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Cannot deploy on package On error; Fix the build.
    GOTO :Finally
)

FOR %%P IN (%PACKAGE_NAMES%) DO ( CALL :InstallPackage %%P )

:Finally
    popd
    EXIT /B
GOTO :EOF

REM Clear any packages out of the local nuget package cache
REM This is because `dotnet tool install --no-cache` appears to be broken?
:ClearExistingPackage
    SET PACKAGE_CACHE_FOLDER=%userprofile%\.nuget\packages\%1
    ECHO Attempting to Clean Existing Package Cache From %PACKAGE_CACHE_FOLDER%
    IF EXIST "%PACKAGE_CACHE_FOLDER%" (
        RMDIR /Q /S "%PACKAGE_CACHE_FOLDER%"
    )

    ECHO.
    ECHO Uninstall Existing Tooling
    dotnet tool uninstall %1
GOTO :EOF

REM Install the latest pre-release packages
:InstallPackage
    ECHO.
    ECHO Install the Latest Prerelease
    dotnet tool install --add-source=nupkg --no-cache %1 --version=*-*
GOTO :EOF
