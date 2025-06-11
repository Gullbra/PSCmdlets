namespace GetShortcut;

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;


[Cmdlet(VerbsCommon.Get, "Shortcut")]
[Alias("goto")]
public class GetShortcut : PSCmdlet
{
  internal static readonly Dictionary<string, string> folderShortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
  {
    { "desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
    { "documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
    { "downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") },
    { "pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) },
    { "music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
    { "videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) },
    { "temp", Path.GetTempPath() },
    { "home", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
    { "programfiles", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) },
    { "startup", Environment.GetFolderPath(Environment.SpecialFolder.Startup) },
  };
  private string? location = null;


  [Parameter(
    Mandatory = false,
    Position = 0,
    HelpMessage = "Folder shortcut name or full path")]
  [ArgumentCompleter(typeof(ShortcutCompleter))]
  public string? Location { get => location; set => location = value; }

  [Parameter(
    Mandatory = false,
    HelpMessage = "List all available shortcuts")]
  public SwitchParameter List { get; set; }


  protected override void ProcessRecord()
  {
    try
    {
      if (List)
      {
        Host.UI.WriteLine(ConsoleColor.Green, Host.UI.RawUI.BackgroundColor, $"\n  {"Shortcut",-15} path");
        Host.UI.WriteLine(ConsoleColor.Green, Host.UI.RawUI.BackgroundColor, $"  {new String('-',8), -15} {new String('-',4)}");
        //var cliList = new StringBuilder("\nAvailable folder shortcuts:\n");
        var cliList = new StringBuilder();
        foreach (var kvp in folderShortcuts)
          cliList.AppendLine($"  {kvp.Key,-15} {kvp.Value}");
        foreach (var kvp in GetEnvShortcuts())
          cliList.AppendLine($"  {kvp.Key,-15} {kvp.Value}");
        WriteObject( cliList.ToString() );
        return;
      }

      if (String.IsNullOrWhiteSpace(Location))
      {
        WriteWarning($"Parameter Location is either null or empty.");
        return;
      }


      string? targetPath;
      if (folderShortcuts.TryGetValue(Location, out targetPath) || GetEnvShortcuts().TryGetValue(Location, out targetPath))
      {
        if (!Directory.Exists(targetPath))
        {
          WriteWarning($"Shortcut '{Location}' points to '{targetPath}' which doesn't exist.");
          return;
        }
      }
      else
      {
        WriteWarning($"Shortcut '{Location}' doesn't exist.");
        return;
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


  internal static Dictionary<string, string> GetEnvShortcuts()
  {
    var envShortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var userEnvVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

    if (userEnvVars == null)
      return envShortcuts;

    foreach (System.Collections.DictionaryEntry envVar in userEnvVars)
    {
      string? name = envVar.Key.ToString(), value = envVar.Value?.ToString();

      if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(name) || envShortcuts.ContainsKey(name) )
        continue;

      if (!Regex.IsMatch(name, "^dir_", RegexOptions.IgnoreCase))
        continue; 

      try
      {
        string expandedPath = Environment.ExpandEnvironmentVariables(value);
        if (Directory.Exists(expandedPath))
        {
          envShortcuts[name[4..].ToLower()] = expandedPath;
        }
      }
      catch
      {
        continue;
      }
    }

    return envShortcuts;
  }
}


class ShortcutCompleter : IArgumentCompleter
{
  public IEnumerable<CompletionResult> CompleteArgument(
      string commandName,
      string parameterName,
      string wordToComplete,
      CommandAst commandAst,
      IDictionary fakeBoundParameters)
  {
    var completions = new List<CompletionResult>();

    foreach (var kvp in GetShortcut.folderShortcuts.Concat(GetShortcut.GetEnvShortcuts()))
    {
      completions.Add(new CompletionResult(
        kvp.Key,
        "test",
        CompletionResultType.Text,
        $"Description for {kvp.Key}"));
    }

    return completions;
  }
}
