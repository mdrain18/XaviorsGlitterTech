@echo off
setlocal enabledelayedexpansion

:: Go up two levels from this script location to find the mod root
set "CURRENT_DIR=%~dp0"
cd /d "%CURRENT_DIR%"
cd ../..
set "MOD_ROOT=%CD%"

:: Extract folder name as mod name
for %%F in ("%MOD_ROOT%") do set "MOD_NAME=%%~nxF"

:: Set RimWorld Mods directory target
set "TARGET_DIR=D:\Steam\steamapps\common\RimWorld\Mods\%MOD_NAME%"

echo.
echo ===============================
echo Mod root: %MOD_ROOT%
echo Mod name: %MOD_NAME%
echo Target Dir: %TARGET_DIR%
echo ===============================
echo.

:: Delete existing mod folder
if exist "%TARGET_DIR%" (
    echo Deleting existing folder at %TARGET_DIR%
    rmdir /s /q "%TARGET_DIR%"
)

:: Copy everything except Source, hidden folders, and .pdb files
echo Copying files to RimWorld Mods folder...
robocopy "%MOD_ROOT%" "%TARGET_DIR%" /E /XD "Source" ".git" ".vs" ".idea" /XF ".gitignore" ".gitattributes" *.pdb

echo.
echo Deployment complete!
echo.

endlocal
exit /b 0