using System.Management.Automation;

namespace TestCmdlet;

[Cmdlet(VerbsCommunications.Send, "Greeting")]  // The name: Verb-Noun
[OutputType(typeof(string))]
public class TestCmdlet : Cmdlet
{
  [Parameter(Mandatory = true, Position = 0)]   // Declare the parameters for the cmdlet.
  public string? Name { get; set; }

  [Alias("t")]                             // Alias, still just guessing how it works.
  [Parameter(Mandatory = false)]
  public string? Title { get; set; }

  [Parameter(
    Mandatory = false,
    HelpMessage = "Make the greeting formal")]
  public SwitchParameter Formal { get; set; }   // Testing Switch parameter




  // Override the ProcessRecord method to process
  // the supplied user name and write out a
  // greeting to the user by calling the WriteObject
  // method.
  protected override void ProcessRecord()
  {
    try
    {
      var title = string.IsNullOrWhiteSpace(Title) ? "" : Title.Trim() + " ";
      var greeting = Formal 
        ? $"Good day, {title}{Name}. It is a pleasure to meet you."
        : $"Hello, {title}{Name}!";

      WriteObject(greeting);
    }
    catch (Exception ex)
    {
      WriteError(new ErrorRecord(ex, "GreetingError", ErrorCategory.NotSpecified, Name));
    }
  }
}