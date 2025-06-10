namespace OpenIDE;

using System;
using System.Management.Automation;


[Cmdlet(VerbsCommon.Open, "IDE")]
public class OpenIDE
{
  [Parameter(
    Mandatory = true,
    Position = 0,
    HelpMessage = "The IDE to open.")]
  public string Name { get; set; }


  [Parameter(
    Mandatory = true,
    Position = 0,
    HelpMessage = "The IDE to open.")]
  public string Name { get; set; }




}
