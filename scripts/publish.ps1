$Host.UI.RawUI.WindowTitle = "Cursor Scheme Inverter Compiler"

Write-Output "Starting Release Compilation..."
Write-Output ""

# 1. Resolve absolute paths dynamically relative to this script's location
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Convert-Path (Join-Path $ScriptDir "..\source")
$PublishDir = Join-Path $SourceDir "bin\Release\net9.0\win-x64\publish\"

# 2. Change location to the source directory
Set-Location -Path $SourceDir

# 3. Run the native AOT publishing command
dotnet publish -c Release -r win-x64

# 4. Check if the compilation succeeded
if ($LASTEXITCODE -ne 0) {
    Write-Output ""
    Write-Error "Compilation failed! Check the log messages above."
    Write-Output ""

    Read-Host "Press any key to exit..."
    Exit $LASTEXITCODE
}

Write-Output ""
Write-Output "Compilation Successful! Opening output directory..."

# 5. Open build output directory
if (Test-Path $PublishDir) {
    Invoke-Item $PublishDir
} else {
    Write-Warning "Publish directory not found at: $PublishDir"
}

Write-Output ""

Write-Output ""
Write-Host "Processing complete." -ForegroundColor Green
Read-Host "Press any key to close this window..."