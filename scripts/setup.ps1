<#
.SYNOPSIS
    Install fbox (文件收集箱) - file reference collector for AI workflow
.DESCRIPTION
    Installs fbox CLI and GUI to the local machine, adds to PATH,
    and installs the opencode companion skill.
#>

$InstallDir = Join-Path $env:LOCALAPPDATA "fbox"
$SkillDir = Join-Path $env:USERPROFILE ".config\opencode\skills\fbox"

Write-Host "=== fbox 安装程序 ===" -ForegroundColor Cyan
Write-Host ""

# --- 1. Copy executables ---
Write-Host "[1/4] 安装程序文件..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
Copy-Item ".\fbox.exe" $InstallDir -Force -ErrorAction Stop
Copy-Item ".\fbox-gui.exe" $InstallDir -Force -ErrorAction Stop
Write-Host "  -> $InstallDir" -ForegroundColor Green

# --- 2. Add to PATH ---
Write-Host "[2/4] 添加 fbox 到 PATH..." -ForegroundColor Yellow
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($userPath -notlike "*$InstallDir*") {
    $newPath = if ($userPath) { "$userPath;$InstallDir" } else { $InstallDir }
    [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    $env:Path = [Environment]::GetEnvironmentVariable("Path", "User") + ";$env:Path"
    Write-Host "  -> 已添加 (需重启终端生效)" -ForegroundColor Green
} else {
    Write-Host "  -> 已在 PATH 中，跳过" -ForegroundColor DarkYellow
}

# --- 3. Install skill ---
Write-Host "[3/4] 安装 opencode 技能..." -ForegroundColor Yellow
$skillSource = ".\skill\SKILL.md"
if (Test-Path $skillSource) {
    New-Item -ItemType Directory -Path $SkillDir -Force | Out-Null
    Copy-Item $skillSource $SkillDir -Force
    Write-Host "  -> $SkillDir" -ForegroundColor Green
} else {
    Write-Host "  -> SKILL.md 未找到，跳过" -ForegroundColor DarkYellow
}

# --- 4. Create shortcuts ---
Write-Host "[4/4] 创建快捷方式..." -ForegroundColor Yellow
$shell = New-Object -ComObject WScript.Shell
$startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\fbox"
New-Item -ItemType Directory -Path $startMenu -Force | Out-Null

$shortcut = $shell.CreateShortcut((Join-Path $startMenu "fbox-gui.lnk"))
$shortcut.TargetPath = Join-Path $InstallDir "fbox-gui.exe"
$shortcut.Description = "fbox - 文件收集箱"
$shortcut.WorkingDirectory = $InstallDir
$shortcut.Save()

# Also create an uninstall shortcut
$uninstallScript = @"
# fbox uninstaller
`$InstallDir = "$InstallDir"
`$SkillDir = "$SkillDir"

Write-Host "Uninstalling fbox..." -ForegroundColor Yellow

# Remove from PATH
`$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
if (`$userPath -like "*`$InstallDir*") {
    `$newPath = (`$userPath -split ';' | Where-Object { `$_ -ne `$InstallDir }) -join ';'
    [Environment]::SetEnvironmentVariable("Path", `$newPath, "User")
}

# Remove files
if (Test-Path `$InstallDir) { Remove-Item `$InstallDir -Recurse -Force }
if (Test-Path `$SkillDir) { Remove-Item `$SkillDir -Recurse -Force }

# Remove shortcuts
`$startMenu = Join-Path `$env:APPDATA "Microsoft\Windows\Start Menu\Programs\fbox"
if (Test-Path `$startMenu) { Remove-Item `$startMenu -Recurse -Force }

Write-Host "fbox has been uninstalled." -ForegroundColor Green
"@
$uninstallerPath = Join-Path $startMenu "uninstall.ps1"
Set-Content -Path $uninstallerPath -Value $uninstallScript -Encoding utf8

Write-Host "  -> 开始菜单: $startMenu" -ForegroundColor Green

# --- Done ---
Write-Host ""
Write-Host "=== 安装完成 ===" -ForegroundColor Green
Write-Host ""
Write-Host "使用方法:"
Write-Host "  fbox list             查看全部文件"
Write-Host "  fbox list --new       查看新文件"
Write-Host "  fbox count            查看统计"
Write-Host "  fbox-gui              启动桌面窗口"
Write-Host ""
Write-Host "按任意键退出..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
