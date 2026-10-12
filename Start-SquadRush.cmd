@echo off
setlocal
set "SquadRushCli=%LOCALAPPDATA%\Unity\bin\unity.exe"
if not exist "%SquadRushCli%" (
  echo Unity CLI was not found. Open the SquadRush folder in Unity Hub.
  pause
  exit /b 1
)
"%SquadRushCli%" open "%~dp0SquadRush" --editor-version 6000.3.26f1
if errorlevel 1 (
  pause
  exit /b 1
)
endlocal
