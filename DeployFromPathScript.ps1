param(
    [Parameter(Mandatory = $true, HelpMessage = "Path to the C# project root directory")]
    [ValidateScript({
        if (-not (Test-Path $_ -PathType Container)) {
            throw "The specified path '$_' does not exist or is not a directory."
        }
        return $true
    })]
    [string]$ProjectPath,
    
    [Parameter(Mandatory = $false, HelpMessage = "Name of the module (will be auto-detected if not specified)")]
    [string]$ModuleName,
    
    [Parameter(Mandatory = $false, HelpMessage = "Destination path for the module (defaults to PSModulePath)")]
    [string]$ModulePath
)


Write-Host "`r`nProjPath: $ProjectPath`r`nModulename: $ModuleName`r`nModPath: $ModulePath`r`n"

# Change to the project directory
$originalLocation = Get-Location
Set-Location $ProjectPath

try {
    # Auto-detect module name if not provided
    if (-not $ModuleName) {
        # Try to find .csproj file
        $csprojFiles = Get-ChildItem -Path . -Filter "*.csproj"
        if ($csprojFiles.Count -eq 1) {
            $ModuleName = [System.IO.Path]::GetFileNameWithoutExtension($csprojFiles[0].Name)
            Write-Host "Auto-detected module name: $ModuleName" -ForegroundColor Yellow
        } elseif ($csprojFiles.Count -gt 1) {
            Write-Error "Multiple .csproj files found. Please specify -ModuleName parameter."
            return
        } else {
            Write-Error "No .csproj file found in the specified directory. Please specify -ModuleName parameter."
            return
        }
    }

    Write-Host "`r`nProjPath: $ProjectPath`r`nModulename: $ModuleName`r`nModPath: $ModulePath`r`n"
    
    # Set default module path if not provided
    if (-not $ModulePath) {
        $psModulePaths = $env:PSModules -split [System.IO.Path]::PathSeparator
        $userModulePath = $psModulePaths | Where-Object { $_ -like "*$env:USERNAME*" -or $_ -like "*Documents*" } | Select-Object -First 1
        if (-not $userModulePath) {
            $userModulePath = $psModulePaths[0]
        }
        $ModulePath = Join-Path $userModulePath $ModuleName
        Write-Host "Using module path: $ModulePath" -ForegroundColor Yellow
    }

    Write-Host "`r`nProjPath: $ProjectPath`r`nModulename: $ModuleName`r`nModPath: $ModulePath`r`n"
    
    # Verify manifest file exists
    $manifestPath = "$ModuleName.psd1"
    if (-not (Test-Path $manifestPath)) {
        Write-Error "Module manifest '$manifestPath' not found in project directory."
        return
    }
    
    # Build the project
    Write-Host "Building project..." -ForegroundColor Green
    $buildResult = dotnet build --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed!"
        return
    }
    
    # Find the built DLL
    $dllPath = "bin\Release\net8.0\$ModuleName.dll"
    if (-not (Test-Path $dllPath)) {
        # Try to find DLL in any .NET version folder
        $possibleDlls = Get-ChildItem -Path "bin\Release" -Filter "$ModuleName.dll" -Recurse
        if ($possibleDlls.Count -gt 0) {
            $dllPath = $possibleDlls[0].FullName
            Write-Host "Found DLL at: $dllPath" -ForegroundColor Yellow
        } else {
            Write-Error "Could not find built DLL: $ModuleName.dll"
            return
        }
    }

    Write-Host "`r`ndllPath: $dllPath`r`n"
    
    # Remove old module if loaded
    Write-Host "Removing old module..." -ForegroundColor Green
    Remove-Module $ModuleName -Force -ErrorAction SilentlyContinue
    
    # Copy files
    Write-Host "Copying files..." -ForegroundColor Green
    if (!(Test-Path $ModulePath)) {
        New-Item -ItemType Directory -Path $ModulePath -Force | Out-Null
    }
    
    Copy-Item $dllPath $ModulePath -Force
    Copy-Item $manifestPath $ModulePath -Force
    
    # Copy any additional files (like help files, etc.)
    # $additionalFiles = @("*.xml", "*.txt", "README.md")
    # foreach ($pattern in $additionalFiles) {
    #     $files = Get-ChildItem -Path . -Filter $pattern -ErrorAction SilentlyContinue
    #     if ($files) {
    #         Copy-Item $files $ModulePath -Force
    #         Write-Host "Copied additional files: $($files.Name -join ', ')" -ForegroundColor Cyan
    #     }
    # }
    
    # Import updated module
    Write-Host "Importing updated module..." -ForegroundColor Green
    Import-Module $ModuleName -Force
    
    # # Verify import
    # $importedModule = Get-Module $ModuleName
    # if ($importedModule) {
    #     Write-Host "Module updated successfully!" -ForegroundColor Green
    #     Write-Host "Module version: $($importedModule.Version)" -ForegroundColor Cyan
    #     Write-Host "Exported commands: $($importedModule.ExportedCommands.Keys -join ', ')" -ForegroundColor Cyan
    # } else {
    #     Write-Warning "Module import may have failed. Please check for errors."
    # }
}
catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
}
finally {
    # Return to original location
    Set-Location $originalLocation
}
