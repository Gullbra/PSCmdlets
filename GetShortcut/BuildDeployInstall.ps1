param(
  [string]$ModuleName = "GetShortcut",
  [string]$ModulePath = "$env:PSModules\$ModuleName"
)

# Build the project
Write-Host "Building project..." -ForegroundColor Green
dotnet build --configuration Release

# Remove old module if loaded
Write-Host "Removing old module..." -ForegroundColor Green
Remove-Module $ModuleName -Force -ErrorAction SilentlyContinue

# Copy files
Write-Host "Copying files..." -ForegroundColor Green
if (!(Test-Path $ModulePath)) {
    New-Item -ItemType Directory -Path $ModulePath -Force
}

Copy-Item "bin\Release\net8.0\$ModuleName.dll" $ModulePath -Force
Copy-Item "$ModuleName.psd1" $ModulePath -Force

# Import updated module
Write-Host "Importing updated module..." -ForegroundColor Green
Import-Module $ModuleName -Force

Write-Host "Module updated successfully!" -ForegroundColor Green
