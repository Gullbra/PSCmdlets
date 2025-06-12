using namespace System.Management.Automation

[Cmdlet(VerbsCommon.Open, "IDE")]
[OutputType([void])]
param()

class OpenIDECommand : PSCmdlet {
    [Parameter(Mandatory = $true, Position = 0, HelpMessage = "Specify the IDE to open")]
    [ValidateSet("PyCharm", "VSCode", "IntelliJ", "VisualStudio")]
    [string]$IDE

    [Parameter(Mandatory = $false, Position = 1, HelpMessage = "Path to the folder to open in the IDE")]
    [ValidateScript({
        if ($_ -and -not (Test-Path $_ -PathType Container)) {
            throw "The specified path '$_' does not exist or is not a directory."
        }
        return $true
    })]
    [string]$Path

    [void] ProcessRecord() {
        try {
            $executablePath = $this.GetIDEExecutablePath($this.IDE)
            $arguments = $this.GetIDEArguments($this.IDE, $this.Path)
            
            $this.WriteVerbose("Opening $($this.IDE) with executable: $executablePath")
            if ($this.Path) {
                $this.WriteVerbose("Opening folder: $($this.Path)")
            }
            
            $processInfo = [System.Diagnostics.ProcessStartInfo]::new()
            $processInfo.FileName = $executablePath
            $processInfo.Arguments = $arguments
            $processInfo.UseShellExecute = $true
            
            $process = [System.Diagnostics.Process]::Start($processInfo)
            
            if ($null -eq $process) {
                $this.WriteError([ErrorRecord]::new(
                    [System.InvalidOperationException]::new("Failed to start $($this.IDE)"),
                    "ProcessStartFailed",
                    [ErrorCategory]::InvalidOperation,
                    $this.IDE
                ))
            } else {
                $this.WriteInformation("Successfully opened $($this.IDE)", @("Process"))
            }
        }
        catch {
            $this.WriteError([ErrorRecord]::new(
                $_.Exception,
                "IDEOpenError",
                [ErrorCategory]::InvalidOperation,
                $this.IDE
            ))
        }
    }

    [string] GetIDEExecutablePath([string]$ideType) {
        switch ($ideType) {
            "PyCharm" {
                $envPath = [Environment]::GetEnvironmentVariable("IDE_PYCHARM", "Machine")
                if (-not $envPath) {
                    $envPath = [Environment]::GetEnvironmentVariable("IDE_PYCHARM", "User")
                }
                if (-not $envPath) {
                    throw "Environment variable IDE_PYCHARM is not set. Please set it to the PyCharm bin directory path."
                }
                $execPath = Join-Path $envPath "pycharm64.exe"
                if (-not (Test-Path $execPath)) {
                    $execPath = Join-Path $envPath "pycharm.exe"
                }
                if (-not (Test-Path $execPath)) {
                    throw "PyCharm executable not found in $envPath. Please verify the IDE_PYCHARM environment variable points to the correct bin directory."
                }
                return $execPath
            }
            "VSCode" {
                $commonPaths = @(
                    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\Code.exe",
                    "${env:ProgramFiles}\Microsoft VS Code\Code.exe",
                    "${env:ProgramFiles(x86)}\Microsoft VS Code\Code.exe"
                )
                
                foreach ($path in $commonPaths) {
                    if (Test-Path $path) {
                        return $path
                    }
                }
                
                # Try to find in PATH
                $codeCmd = Get-Command "code" -ErrorAction SilentlyContinue
                if ($codeCmd) {
                    return $codeCmd.Source
                }
                
                throw "VSCode executable not found. Please ensure VSCode is installed and accessible via PATH or in standard locations."
            }
            "IntelliJ" {
                $envPath = [Environment]::GetEnvironmentVariable("IDE_INTELLIJ", "Machine")
                if (-not $envPath) {
                    $envPath = [Environment]::GetEnvironmentVariable("IDE_INTELLIJ", "User")
                }
                if (-not $envPath) {
                    throw "Environment variable IDE_INTELLIJ is not set. Please set it to the IntelliJ bin directory path."
                }
                $execPath = Join-Path $envPath "idea64.exe"
                if (-not (Test-Path $execPath)) {
                    $execPath = Join-Path $envPath "idea.exe"
                }
                if (-not (Test-Path $execPath)) {
                    throw "IntelliJ executable not found in $envPath. Please verify the IDE_INTELLIJ environment variable points to the correct bin directory."
                }
                return $execPath
            }
            "VisualStudio" {
                # Try to find Visual Studio via vswhere
                $vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
                if (Test-Path $vswherePath) {
                    $vsPath = & $vswherePath -latest -property installationPath 2>$null
                    if ($vsPath) {
                        $devenvPath = Join-Path $vsPath "Common7\IDE\devenv.exe"
                        if (Test-Path $devenvPath) {
                            return $devenvPath
                        }
                    }
                }
                
                # Fallback to common paths
                $commonPaths = @(
                    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
                    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
                    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
                    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe",
                    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe",
                    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
                )
                
                foreach ($path in $commonPaths) {
                    if (Test-Path $path) {
                        return $path
                    }
                }
                
                throw "Visual Studio executable not found. Please ensure Visual Studio is installed in a standard location."
            }
            default {
                throw "Unsupported IDE: $ideType"
            }
        }
    }

    [string] GetIDEArguments([string]$ideType, [string]$folderPath) {
        if (-not $folderPath) {
            return ""
        }
        
        # Convert to absolute path
        $absolutePath = (Resolve-Path $folderPath -ErrorAction SilentlyContinue).Path
        if (-not $absolutePath) {
            $absolutePath = $folderPath
        }
        
        switch ($ideType) {
            "PyCharm" { return "`"$absolutePath`"" }
            "VSCode" { return "`"$absolutePath`"" }
            "IntelliJ" { return "`"$absolutePath`"" }
            "VisualStudio" { return "`"$absolutePath`"" }
            default { return "" }
        }
    }
}