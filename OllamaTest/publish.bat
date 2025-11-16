@echo off
setlocal

REM Configuration
set CONFIGURATION=Release
set PROJECT=Backend.csproj

REM Runtimes and Architectures
set RUNTIMES=win-x64 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64

echo ========================================
echo Publishing %PROJECT% in self-contained mode...
echo Configuration: %CONFIGURATION%
echo ========================================

for %%R in (%RUNTIMES%) do (
    echo.
    echo Publishing for runtime: %%R
    dotnet publish %PROJECT% ^
        --configuration %CONFIGURATION% ^
        --runtime %%R ^
        /p:IncludeNativeLibrariesForSelfExtract=true ^
        --output bin\publish\%%R

    IF %ERRORLEVEL% NEQ 0 (
        echo ERROR: Failed to publish for %%R
        exit /b 1
    )
)

echo.
echo ========================================
echo Publishing complete!
echo Output folders:
for %%R in (%RUNTIMES%) do (
    echo   bin\publish\%%R
)
echo ========================================

endlocal
pause
