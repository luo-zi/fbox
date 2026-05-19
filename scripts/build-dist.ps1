param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [switch]$SelfContained = $true
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$DistDir = Join-Path $ProjectRoot "dist"
$PublishArgs = @(
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", $SelfContained.ToString().ToLower()
)

Write-Host "=== Building fbox distribution package ===" -ForegroundColor Cyan
Write-Host "Runtime: $Runtime"
Write-Host "Configuration: $Configuration"
Write-Host "Self-contained: $SelfContained"
Write-Host ""

# Clean dist
if (Test-Path $DistDir) {
    Remove-Item -Path "$DistDir\*" -Recurse -Force -ErrorAction SilentlyContinue
}

# Publish CLI
Write-Host ">>> Publishing CLI (fbox.exe)..." -ForegroundColor Yellow
$cliArgs = $PublishArgs + @(
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "/p:ForceRebuild=true"
)
dotnet publish (Join-Path $ProjectRoot "src\Fbox.Cli\Fbox.Cli.csproj") `
    -o (Join-Path $DistDir "cli") @cliArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "CLI publish failed!" -ForegroundColor Red
    exit 1
}
Copy-Item (Join-Path $DistDir "cli\fbox.exe") (Join-Path $DistDir "fbox.exe") -Force
Remove-Item (Join-Path $DistDir "cli") -Recurse -Force

Write-Host "  -> fbox.exe ($((Get-Item (Join-Path $DistDir "fbox.exe")).Length / 1MB -as [int]) MB)"

# Publish GUI
Write-Host ">>> Publishing GUI (fbox-gui.exe)..." -ForegroundColor Yellow
$guiArgs = $PublishArgs + @(
    "-p:PublishSingleFile=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
    "/p:ForceRebuild=true"
)
dotnet publish (Join-Path $ProjectRoot "src\Fbox.Gui\Fbox.Gui.csproj") `
    -o (Join-Path $DistDir "gui") @guiArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "GUI publish failed!" -ForegroundColor Red
    exit 1
}
Copy-Item (Join-Path $DistDir "gui\Fbox.Gui.exe") (Join-Path $DistDir "fbox-gui.exe") -Force
Remove-Item (Join-Path $DistDir "gui") -Recurse -Force

Write-Host "  -> fbox-gui.exe ($((Get-Item (Join-Path $DistDir "fbox-gui.exe")).Length / 1MB -as [int]) MB)"

# Copy skill
Write-Host ">>> Copying skill..." -ForegroundColor Yellow
$skillDir = Join-Path $DistDir "skill"
New-Item -ItemType Directory -Path $skillDir -Force | Out-Null
$skillSource = Join-Path $ProjectRoot "skills\fbox\SKILL.md"
Copy-Item $skillSource (Join-Path $skillDir "SKILL.md") -ErrorAction SilentlyContinue
if (-not (Test-Path (Join-Path $skillDir "SKILL.md"))) {
    Write-Host "  (skill not found at $skillSource, skipping)" -ForegroundColor DarkYellow
}

# Copy setup script and install guide
Copy-Item (Join-Path $ProjectRoot "scripts\setup.ps1") (Join-Path $DistDir "setup.ps1") -Force
Copy-Item (Join-Path $ProjectRoot "scripts\INSTALL.md") (Join-Path $DistDir "INSTALL.md") -Force

# Create zip
$zipPath = Join-Path $DistDir "fbox-dist.zip"
Compress-Archive -Path "$DistDir\*" -DestinationPath $zipPath -Force

# Summary
Write-Host ""
Write-Host "=== Distribution package ready ===" -ForegroundColor Green
Write-Host "Directory: $DistDir"
Write-Host "Zip: $zipPath ($((Get-Item $zipPath).Length / 1MB -as [int]) MB)"
Get-ChildItem $DistDir -File -Exclude "*.zip" | ForEach-Object {
    Write-Host "  $($_.Name) ($($_.Length / 1KB -as [int]) KB)"
}
