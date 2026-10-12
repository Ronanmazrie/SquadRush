param([switch]$NoOpen)
$ErrorActionPreference = 'Stop'
$taskVersion = '6000.3.26f1'
$taskCli = Join-Path $env:LOCALAPPDATA 'Unity/bin/unity.exe'
$taskParent = Split-Path -Parent $PSScriptRoot
$taskProject = Join-Path $taskParent 'SquadRush'
if (-not (Test-Path -LiteralPath $taskCli)) { throw 'Unity CLI is missing. Install the official Unity CLI first.' }

function Invoke-UnityChecked {
    param([string[]]$Arguments)
    & $taskCli @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Unity command failed with exit code $LASTEXITCODE. See the output above." }
}

$taskEditorsText = & $taskCli editors --installed --format json
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect installed Unity editors.' }
$taskEditors = $taskEditorsText | ConvertFrom-Json
if (-not $taskEditors.success) { throw 'Unity editor inspection failed.' }
if (-not ($taskEditors.data | Where-Object { $_.version -eq $taskVersion })) {
    $taskCandidates = @(
        (Join-Path $env:LOCALAPPDATA "Unity/Editors/$taskVersion/Editor/Unity.exe"),
        (Join-Path $env:ProgramFiles "Unity/Hub/Editor/$taskVersion/Editor/Unity.exe"),
        (Join-Path $env:ProgramFiles "Unity $taskVersion/Editor/Unity.exe"),
        (Join-Path $env:ProgramFiles 'Unity/Editor/Unity.exe')
    )
    $taskCandidate = $taskCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if ($taskCandidate) {
        Invoke-UnityChecked -Arguments @('editors', 'add', $taskCandidate, '--format', 'json')
    } else { throw "Install Unity $taskVersion, then run this script again. The downloaded installer is in AppData/Roaming/UnityHub/downloads." }
}

if (-not (Test-Path -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt'))) {
    if ((Test-Path -LiteralPath $taskProject) -and (Get-ChildItem -LiteralPath $taskProject -Force | Select-Object -First 1)) {
        throw 'SquadRush exists and is not a recognized Unity project. Rename that folder before continuing.'
    }
    Invoke-UnityChecked -Arguments @('projects', 'create', 'SquadRush', '--path', $taskParent, '--editor-version', $taskVersion,
        '--template', 'com.unity.template.3d', '--non-interactive', '--format', 'json')
}
$taskRecordedVersion = Get-Content -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt')
if (-not ($taskRecordedVersion | Where-Object { $_ -eq "m_EditorVersion: $taskVersion" })) {
    throw "The existing project uses a different Unity version. Expected $taskVersion."
}

$taskScripts = Join-Path $taskProject 'Assets/Scripts'
$taskEditor = Join-Path $taskProject 'Assets/Editor/SquadRush'
$taskFonts = Join-Path $taskProject 'Assets/Fonts'
New-Item -ItemType Directory -Force -Path $taskScripts,$taskEditor,$taskFonts,(Join-Path $taskProject 'Logs') | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Sources/SquadRushGame.cs'),(Join-Path $PSScriptRoot 'Sources/SquadRushHud.cs') -Destination $taskScripts
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Editor/PrototypeBuilder.cs') -Destination $taskEditor
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Editor/RuntimeVerifier.cs') -Destination $taskEditor
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Editor/PrototypeLauncher.cs') -Destination $taskEditor
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Fonts/NotoSansCJKjp-Regular.otf'),(Join-Path $PSScriptRoot 'Fonts/LICENSE.txt') -Destination $taskFonts

Write-Host 'The official 3D template provides com.unity.ugui and com.unity.inputsystem. No extra packages are installed.'
$taskEditorPathText = & $taskCli editors path $taskVersion --format json
if ($LASTEXITCODE -ne 0) { throw 'Could not resolve the Unity Editor executable.' }
$taskEditorPath = ($taskEditorPathText | ConvertFrom-Json).data.path
if (Test-Path -LiteralPath $taskEditorPath -PathType Container) { $taskEditorPath = Join-Path $taskEditorPath 'Editor/Unity.exe' }
$taskImportArguments = @('-batchmode', '-projectPath', ('"' + $taskProject + '"'), '-executeMethod',
    'SquadRush.Editor.PrototypeBuilder.ImportTextResources', '-logFile', ('"' + (Join-Path $taskProject 'Logs/text-resources.log') + '"'))
$taskImportProcess = Start-Process -FilePath $taskEditorPath -ArgumentList $taskImportArguments -WindowStyle Hidden -PassThru -Wait
if ($taskImportProcess.ExitCode -ne 0) { throw 'TMP text resource import failed. See Logs/text-resources.log.' }
Invoke-UnityChecked -Arguments @('run', $taskProject, '--editor-version', $taskVersion, '--', '-executeMethod', 'SquadRush.Editor.PrototypeBuilder.Generate',
    '-logFile', (Join-Path $taskProject 'Logs/generate.log'))
Invoke-UnityChecked -Arguments @('run', $taskProject, '--editor-version', $taskVersion, '--', '-executeMethod', 'SquadRush.Editor.PrototypeBuilder.Verify',
    '-logFile', (Join-Path $taskProject 'Logs/verify.log'))
$taskRuntimeArguments = @('-batchmode', '-projectPath', ('"' + $taskProject + '"'), '-executeMethod',
    'SquadRush.Editor.RuntimeVerifier.Run', '-logFile', ('"' + (Join-Path $taskProject 'Logs/runtime-verify.log') + '"'))
$taskRuntimeProcess = Start-Process -FilePath $taskEditorPath -ArgumentList $taskRuntimeArguments -WindowStyle Hidden -PassThru -Wait
if ($taskRuntimeProcess.ExitCode -ne 0) { throw 'Play mode verification failed. See Logs/runtime-verify.log.' }
Write-Host "Prototype prepared; gameplay and Play mode scenarios verified: $taskProject"
if (-not $NoOpen) { Invoke-UnityChecked -Arguments @('open', $taskProject, '--editor-version', $taskVersion) }

