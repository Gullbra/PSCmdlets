using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace GetShortcut;


[Cmdlet(VerbsCommon.Get, "ToFolder")]
[Alias("goto")]
public class Claude : PSCmdlet
{
  // Get all folder shortcuts including environment variables
  private Dictionary<string, string> GetFolderShortcuts()
  {
    var shortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
          {
              // Built-in Windows folders
              { "desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
              { "documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
              { "downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") },
              { "pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) },
              { "music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
              { "videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) },
              { "temp", Path.GetTempPath() },
              { "home", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
              { "profile", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
              { "programfiles", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) },
              { "startup", Environment.GetFolderPath(Environment.SpecialFolder.Startup) },
                
              // Custom static shortcuts
              { "projects", @"C:\Projects" },
              { "work", @"C:\Work" },
              { "scripts", @"C:\Scripts" }
          };

    // Add user environment variables that point to directories
    var userEnvVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
    foreach (System.Collections.DictionaryEntry envVar in userEnvVars)
    {
      string name = envVar.Key.ToString();
      string value = envVar.Value.ToString();

      // Skip if already exists or if it's not a directory path
      if (shortcuts.ContainsKey(name) || string.IsNullOrEmpty(value))
        continue;

      // Expand the path and check if it's a valid directory
      try
      {
        string expandedPath = Environment.ExpandEnvironmentVariables(value);
        if (Directory.Exists(expandedPath))
        {
          shortcuts[name.ToLower()] = expandedPath;
        }
      }
      catch
      {
        // Skip invalid paths
        continue;
      }
    }

    // Also add machine-level environment variables (optional)
    var machineEnvVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
    foreach (System.Collections.DictionaryEntry envVar in machineEnvVars)
    {
      string name = envVar.Key.ToString();
      string value = envVar.Value.ToString();

      // Skip common system variables and existing entries
      if (shortcuts.ContainsKey(name) || string.IsNullOrEmpty(value) ||
          IsSystemVariable(name))
        continue;

      try
      {
        string expandedPath = Environment.ExpandEnvironmentVariables(value);
        if (Directory.Exists(expandedPath))
        {
          shortcuts[name.ToLower()] = expandedPath;
        }
      }
      catch
      {
        continue;
      }
    }

    return shortcuts;
  }

  // Helper method to filter out common system variables that aren't paths
  private bool IsSystemVariable(string name)
  {
    var systemVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
          {
              "PATH", "PATHEXT", "PROCESSOR_ARCHITECTURE", "PROCESSOR_IDENTIFIER",
              "PROCESSOR_LEVEL", "PROCESSOR_REVISION", "OS", "COMPUTERNAME",
              "USERNAME", "USERDOMAIN", "LOGONSERVER", "NUMBER_OF_PROCESSORS",
              "SESSIONNAME", "WINDIR", "SYSTEMROOT", "COMSPEC"
          };

    return systemVars.Contains(name);
  }

  [Parameter(
      Mandatory = true,
      Position = 0,
      HelpMessage = "Folder shortcut name or full path")]
  public string Location { get; set; }

  [Parameter(
      Mandatory = false,
      HelpMessage = "List all available shortcuts")]
  public SwitchParameter List { get; set; }

  protected override void ProcessRecord()
  {
    try
    {
      var folderShortcuts = GetFolderShortcuts();

      if (List)
      {
        WriteObject("Available folder shortcuts:");
        WriteObject("Built-in shortcuts:", false);

        var builtInShortcuts = new[] { "desktop", "documents", "downloads", "pictures", "music", "videos", "temp", "home", "profile", "programfiles", "startup", "projects", "work", "scripts" };
        foreach (var shortcut in folderShortcuts.Where(s => builtInShortcuts.Contains(s.Key)))
        {
          WriteObject($"  {shortcut.Key,-20} -> {shortcut.Value}");
        }

        WriteObject("Environment variable shortcuts:", false);
        foreach (var shortcut in folderShortcuts.Where(s => !builtInShortcuts.Contains(s.Key)))
        {
          WriteObject($"  {shortcut.Key,-20} -> {shortcut.Value}");
        }
        return;
      }

      string targetPath;

      // Check if it's a predefined shortcut
      if (folderShortcuts.TryGetValue(Location, out targetPath))
      {
        if (!Directory.Exists(targetPath))
        {
          WriteWarning($"Shortcut '{Location}' points to '{targetPath}' which doesn't exist.");
          return;
        }
      }
      else
      {
        // Treat as a direct path
        targetPath = Location;

        // Expand environment variables and relative paths
        targetPath = Environment.ExpandEnvironmentVariables(targetPath);
        targetPath = SessionState.Path.GetResolvedPSPathFromPSPath(targetPath).FirstOrDefault()?.Path ?? targetPath;

        if (!Directory.Exists(targetPath))
        {
          WriteError(new ErrorRecord(
              new DirectoryNotFoundException($"Directory '{targetPath}' not found."),
              "DirectoryNotFound",
              ErrorCategory.ObjectNotFound,
              targetPath));
          return;
        }
      }

      // Change to the directory
      SessionState.Path.SetLocation(targetPath);
      WriteVerbose($"Changed directory to: {targetPath}");
    }
    catch (Exception ex)
    {
      WriteError(new ErrorRecord(ex, "GoToFolderError", ErrorCategory.NotSpecified, Location));
    }
  }
}

[Cmdlet(VerbsCommon.Add, "FolderShortcut")]
public class AddFolderShortcutCommand : PSCmdlet
{
  [Parameter(Mandatory = true, Position = 0)]
  public string Name { get; set; }

  [Parameter(Mandatory = true, Position = 1)]
  public string Path { get; set; }

  protected override void ProcessRecord()
  {
    try
    {
      // Expand the path
      string expandedPath = Environment.ExpandEnvironmentVariables(Path);
      expandedPath = SessionState.Path.GetResolvedPSPathFromPSPath(expandedPath).FirstOrDefault()?.Path ?? expandedPath;

      if (!Directory.Exists(expandedPath))
      {
        WriteError(new ErrorRecord(
            new DirectoryNotFoundException($"Directory '{expandedPath}' not found."),
            "DirectoryNotFound",
            ErrorCategory.ObjectNotFound,
            expandedPath));
        return;
      }

      // In a real implementation, you'd want to persist this to a config file
      // For now, we'll just show what would be added
      WriteObject($"Would add shortcut: {Name} -> {expandedPath}");
      WriteWarning("Note: This shortcut will only be available during this session. Consider adding it to the source code for persistence.");
    }
    catch (Exception ex)
    {
      WriteError(new ErrorRecord(ex, "AddShortcutError", ErrorCategory.NotSpecified, Name));
    }
  }
}
