using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text.RegularExpressions;



namespace PSCmdlets.GetWifiPwdCmdlet;

[Cmdlet(VerbsCommon.Get, "WifiPwd")]
[OutputType(typeof(string), typeof(PSObject))]
public partial class GetWifiPwdCommand : PSCmdlet 
{
  [GeneratedRegex(@"Key Content\s+:\s+(.*)", RegexOptions.IgnoreCase)]
  private static partial Regex KeyContentRegex();

  [GeneratedRegex(@"All User Profile\s+:\s+(.*)", RegexOptions.IgnoreCase)]
  private static partial Regex ProfileRegex();


  [Parameter(
	Mandatory = true, 
	Position = 0, 
	ValueFromPipeline = true, 
	ParameterSetName = "ByName")]
  public string? Name { get; set; }

  [Parameter(
	Mandatory = true, 
	ParameterSetName = "List")]
  public SwitchParameter List { get; set; }


  protected override void ProcessRecord()
  {
	if (List.IsPresent)
	{
	  ListAllNetworksWithPasswords();
	}
	else
	{
	  GetPasswordForNetwork(Name);
	}
  }


  private void ListAllNetworksWithPasswords()
  {
	var profiles = GetAllProfiles();

	foreach (var profile in profiles)
	{
	  try
	  {
		string? password = GetNetworkPassword(profile);

		var result = new PSObject();
		result.Properties.Add(new PSNoteProperty("NetworkName", profile));
		result.Properties.Add(new PSNoteProperty("Password", password ?? "(No password or open network)"));

		WriteObject(result);
	  }
	  catch (Exception ex)
	  {
		WriteWarning($"Could not retrieve password for '{profile}': {ex.Message}");
	  }
	}
  }


  private List<string> GetAllProfiles()
  {
	var profiles = new List<string>();
	var matches = ProfileRegex().Matches(ExecuteNetshCommand("wlan show profiles"));

	foreach (Match match in matches)
	{
	  if (match.Success)
	  {
		profiles.Add(match.Groups[1].Value.Trim());
	  }
	}

	return profiles;
  }


  private void GetPasswordForNetwork(string? networkName)
  {
	if (string.IsNullOrWhiteSpace(networkName))
	{
	  WriteError(new ErrorRecord(
		new ArgumentException("Network name cannot be null or empty"),
		"InvalidNetworkName",
		ErrorCategory.InvalidArgument,
		networkName));
	  return;
	}

	try
	{
	  string? password = GetNetworkPassword(networkName);

	  if (password != null)
	  {
		WriteObject(password);
	  }
	  else
	  {
		WriteError(new ErrorRecord(
		  new Exception($"Could not find password for network: {networkName}"),
		  "PasswordNotFound",
		  ErrorCategory.ObjectNotFound,
		  networkName));
	  }
	}
	catch (Exception ex)
	{
	  WriteError(new ErrorRecord(
		ex,
		"CommandExecutionFailed",
		ErrorCategory.NotSpecified,
		networkName));
	}
  }


  private string? GetNetworkPassword(string networkName)
  {
	Match match = KeyContentRegex()
	  .Match(ExecuteNetshCommand($"wlan show profile name=\"{networkName}\" key=clear"));

	return match.Success ? match.Groups[1].Value.Trim() : null;
  }


  private string ExecuteNetshCommand(string arguments)
  {
	ProcessStartInfo psi = new()
	{
	  FileName = "netsh",
	  Arguments = arguments,
	  RedirectStandardOutput = true,
	  RedirectStandardError = true,
	  UseShellExecute = false,
	  CreateNoWindow = true
	};

	using Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start netsh process");
	string output = process.StandardOutput.ReadToEnd();
	string error = process.StandardError.ReadToEnd();
	process.WaitForExit();

	if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
	{
	  throw new InvalidOperationException($"netsh command failed: {error}");
	}

	return output;
  }
}


//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Management.Automation;
//using System.Text.RegularExpressions;


//namespace PSCmdlets.GetWifiPwdCmdlet;

//[Cmdlet(VerbsCommon.Get, "WifiPwd")]
//[OutputType(typeof(string), typeof(PSObject))]
//public partial class GetWifiPwdCommand : PSCmdlet
//{
//  private const string KeyContentPattern = @"Key Content\s+:\s+(.*)";
//  private const string ProfilePattern = @"All User Profile\s+:\s+(.*)";

//  [Parameter(
//	Mandatory = true, 
//	Position = 0, 
//	ValueFromPipeline = true, 
//	ParameterSetName = "ByName")]
//  public string? Name { get; set; }

//  [Parameter(
//	Mandatory = true, 
//	ParameterSetName = "List")]
//  public SwitchParameter List { get; set; }


//  protected override void ProcessRecord()
//  {
//	if (List.IsPresent)
//	{
//	  ListAllNetworksWithPasswords();
//	}
//	else
//	{
//	  GetPasswordForNetwork(Name);
//	}
//  }


//  private void ListAllNetworksWithPasswords()
//  {
//	var profiles = GetAllProfiles();

//	foreach (var profile in profiles)
//	{
//	  try
//	  {
//		string password = GetNetworkPassword(profile);

//		var result = new PSObject();
//		result.Properties.Add(new PSNoteProperty("NetworkName", profile));
//		result.Properties.Add(new PSNoteProperty("Password", password ?? "(No password or open network)"));

//		WriteObject(result);
//	  }
//	  catch (Exception ex)
//	  {
//		WriteWarning($"Could not retrieve password for '{profile}': {ex.Message}");
//	  }
//	}
//  }


//  private List<string> GetAllProfiles()
//  {
//	var profiles = new List<string>();
//	string output = ExecuteNetshCommand("wlan show profiles");

//	var matches = Regex.Matches(output, ProfilePattern, RegexOptions.IgnoreCase);
//	foreach (Match match in matches)
//	{
//	  if (match.Success)
//	  {
//		profiles.Add(match.Groups[1].Value.Trim());
//	  }
//	}

//	return profiles;
//  }


//  private void GetPasswordForNetwork(string? networkName)
//  {
//	if (string.IsNullOrWhiteSpace(networkName))
//	{
//	  WriteError(new ErrorRecord(
//		new ArgumentException("Network name cannot be null or empty"),
//		"InvalidNetworkName",
//		ErrorCategory.InvalidArgument,
//		networkName));
//	  return;
//	}

//	try
//	{
//	  string? password = GetNetworkPassword(networkName);

//	  if (password != null)
//	  {
//		WriteObject(password);
//	  }
//	  else
//	  {
//		WriteError(new ErrorRecord(
//		  new Exception($"Could not find password for network: {networkName}"),
//		  "PasswordNotFound",
//		  ErrorCategory.ObjectNotFound,
//		  networkName));
//	  }
//	}
//	catch (Exception ex)
//	{
//	  WriteError(new ErrorRecord(
//		ex,
//		"CommandExecutionFailed",
//		ErrorCategory.NotSpecified,
//		networkName));
//	}
//  }


//  private string? GetNetworkPassword(string networkName)
//  {
//	Match match = MyRegex()
//	  .Match(ExecuteNetshCommand($"wlan show profile name=\"{networkName}\" key=clear"));

//	return match.Success ? match.Groups[1].Value.Trim() : null;
//  }


//  private string ExecuteNetshCommand(string arguments)
//  {
//	ProcessStartInfo psi = new()
//	{
//	  FileName = "netsh",
//	  Arguments = arguments,
//	  RedirectStandardOutput = true,
//	  RedirectStandardError = true,
//	  UseShellExecute = false,
//	  CreateNoWindow = true
//	};

//	using Process process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start netsh process");
//	string output = process.StandardOutput.ReadToEnd();
//	string error = process.StandardError.ReadToEnd();
//	process.WaitForExit();

//	if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
//	{
//	  throw new InvalidOperationException($"netsh command failed: {error}");
//	}

//	return output;
//  }


//  [GeneratedRegex(@"Key Content\s+:\s+(.*)", RegexOptions.IgnoreCase)]
//  private static partial Regex KeyContentRegex();


//  [GeneratedRegex(@"All User Profile\s+:\s+(.*)", RegexOptions.IgnoreCase)]
//  private static partial Regex ProfileRegex();
//}