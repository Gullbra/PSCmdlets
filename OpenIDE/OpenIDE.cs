namespace PSCmdlets.OpenIDECmdlet;

using System;
using System.Management.Automation;

using System.Diagnostics;
using System.IO;


[Cmdlet(VerbsCommon.Open, "IDE")]
[OutputType(typeof(void))]
[Alias("IDE")]
public class OpenIDECommand : PSCmdlet
{
  [Parameter(Mandatory = true, Position = 0, HelpMessage = "Specify the IDE to open")]
  [ValidateSet("PyCharm", "VSCode", "IntelliJ", "VisualStudio")]
  public string? IDE { get; set; }

  [Parameter(Mandatory = false, Position = 1, HelpMessage = "Path to the folder to open in the IDE")]
  [PathValidator]
  public string? Path { get; set; }


  protected override void ProcessRecord()
  {
    try
    {
      if (string.IsNullOrEmpty(IDE))
        throw new ArgumentException($"Unsupported IDE: {IDE}");

      string executablePath = GetIDEExecutablePath(IDE);
      string arguments = GetIDEArguments(IDE, Path);

      WriteVerbose($"Opening {IDE} with executable: {executablePath}");
      if (!string.IsNullOrEmpty(Path))
      {
        WriteVerbose($"Opening folder: {Path}");
      }

      var processInfo = new ProcessStartInfo
      {
        FileName = executablePath,
        Arguments = arguments,
        UseShellExecute = true
      };

      var process = Process.Start(processInfo);

      if (process == null)
      {
        WriteError(new ErrorRecord(
            new InvalidOperationException($"Failed to start {IDE}"),
            "ProcessStartFailed",
            ErrorCategory.InvalidOperation,
            IDE));
      }
      else
      {
        WriteInformation($"Successfully opened {IDE}", new[] { "Process" });
      }

    }
    catch (Exception ex)
    {
      WriteError(new ErrorRecord(
          ex,
          "IDEOpenError",
          ErrorCategory.InvalidOperation,
          IDE));
    }
  }

  private string GetIDEExecutablePath(string ideType)
  {
    return ideType.ToLower() switch
    {
      "pycharm" => GetPyCharmPath(),
      "vscode" => GetVSCodePath(),
      "intellij" => GetIntelliJPath(),
      "visualstudio" => GetVisualStudioPath(),
      _ => throw new ArgumentException($"Unsupported IDE: {ideType}")
    };
  }

  private string GetPyCharmPath()
  {
    string? envPath = Environment.GetEnvironmentVariable("IDE_PYCHARM", EnvironmentVariableTarget.Machine) ??
                     Environment.GetEnvironmentVariable("IDE_PYCHARM", EnvironmentVariableTarget.User);

    if (string.IsNullOrEmpty(envPath))
    {
      throw new InvalidOperationException("Environment variable IDE_PYCHARM is not set. Please set it to the PyCharm bin directory path.");
    }

    string execPath = System.IO.Path.Combine(envPath, "pycharm64.exe");
    if (!File.Exists(execPath))
    {
      execPath = System.IO.Path.Combine(envPath, "pycharm.exe");
    }

    if (!File.Exists(execPath))
    {
      throw new FileNotFoundException($"PyCharm executable not found in {envPath}. Please verify the IDE_PYCHARM environment variable points to the correct bin directory.");
    }

    return execPath;
  }

  private string GetVSCodePath()
  {
    string[] commonPaths = {
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft VS Code", "Code.exe")
            };

    foreach (string path in commonPaths)
    {
      if (File.Exists(path))
      {
        return path;
      }
    }

    // Try to find in PATH
    try
    {
      var codeCmd = SessionState.InvokeCommand.GetCommand("code", CommandTypes.Application);
      if (codeCmd != null && codeCmd is ApplicationInfo appInfo)
      {
        return appInfo.Path;
      }
    }
    catch
    {
      // Ignore errors when trying to find in PATH
    }

    throw new FileNotFoundException("VSCode executable not found. Please ensure VSCode is installed and accessible via PATH or in standard locations.");
  }

  private string GetIntelliJPath()
  {
    string? envPath = Environment.GetEnvironmentVariable("IDE_INTELLIJ", EnvironmentVariableTarget.Machine) ??
                     Environment.GetEnvironmentVariable("IDE_INTELLIJ", EnvironmentVariableTarget.User);

    if (string.IsNullOrEmpty(envPath))
    {
      throw new InvalidOperationException("Environment variable IDE_INTELLIJ is not set. Please set it to the IntelliJ bin directory path.");
    }

    string execPath = System.IO.Path.Combine(envPath, "idea64.exe");
    if (!File.Exists(execPath))
    {
      execPath = System.IO.Path.Combine(envPath, "idea.exe");
    }

    if (!File.Exists(execPath))
    {
      throw new FileNotFoundException($"IntelliJ executable not found in {envPath}. Please verify the IDE_INTELLIJ environment variable points to the correct bin directory.");
    }

    return execPath;
  }

  private string GetVisualStudioPath()
  {
    // Try to find Visual Studio via vswhere
    string vswherePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "Installer", "vswhere.exe");

    if (File.Exists(vswherePath))
    {
      try
      {
        var vswhereProcess = new ProcessStartInfo
        {
          FileName = vswherePath,
          Arguments = "-latest -property installationPath",
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        };

        using var process = Process.Start(vswhereProcess);
        if (process != null)
        {
          string? vsPath = process.StandardOutput.ReadToEnd().Trim();
          process.WaitForExit();

          if (!string.IsNullOrEmpty(vsPath))
          {
            string devenvPath = System.IO.Path.Combine(vsPath, "Common7", "IDE", "devenv.exe");
            if (File.Exists(devenvPath))
            {
              return devenvPath;
            }
          }
        }
      }
      catch
      {
        // Fall through to manual search if vswhere fails
      }
    }

    // Fallback to common paths
    string[] commonPaths = {
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Visual Studio", "2022", "Enterprise", "Common7", "IDE", "devenv.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Visual Studio", "2022", "Professional", "Common7", "IDE", "devenv.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Visual Studio", "2022", "Community", "Common7", "IDE", "devenv.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "2019", "Enterprise", "Common7", "IDE", "devenv.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "2019", "Professional", "Common7", "IDE", "devenv.exe"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "2019", "Community", "Common7", "IDE", "devenv.exe")
            };

    foreach (string path in commonPaths)
    {
      if (File.Exists(path))
      {
        return path;
      }
    }

    throw new FileNotFoundException("Visual Studio executable not found. Please ensure Visual Studio is installed in a standard location.");
  }

  private string GetIDEArguments(string ideType, string? folderPath)
  {
    if (string.IsNullOrEmpty(folderPath))
    {
      return string.Empty;
    }

    // Convert to absolute path
    string absolutePath;
    try
    {
      absolutePath = System.IO.Path.GetFullPath(folderPath);
    }
    catch
    {
      absolutePath = folderPath;
    }

    return ideType switch
    {
      "PyCharm" => $"\"{absolutePath}\"",
      "VSCode" => $"\"{absolutePath}\"",
      "IntelliJ" => $"\"{absolutePath}\"",
      "VisualStudio" => $"\"{absolutePath}\"",
      _ => string.Empty
    };
  }
}

    

    // Custom validator class for path validation
public class PathValidatorAttribute : ValidateArgumentsAttribute
{
  protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
  {
    if (arguments is string path && !string.IsNullOrEmpty(path))
    {
      if (!Directory.Exists(path))
      {
        throw new ValidationMetadataException($"The specified path '{path}' does not exist or is not a directory.");
      }
    }
  }
}

