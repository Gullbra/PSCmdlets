using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetShortcut;
/*
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
 */
