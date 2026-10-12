@echo off
setlocal
set "SquadRushEditor=%ProgramFiles%\Unity 6000.3.26f1\Editor\Unity.exe"
if not exist "%SquadRushEditor%" (
  echo Unity Editor was not found at the installed location.
  pause
  exit /b 1
)
start "" "%SquadRushEditor%" -projectPath "%~dp0SquadRush" -executeMethod SquadRush.Editor.PrototypeLauncher.PlayPrototype -logFile "%~dp0SquadRush\Logs\editor-session.log"
if errorlevel 1 (
  pause
  exit /b 1
)
endlocal
