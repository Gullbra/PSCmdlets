@{
    ModuleVersion = '1.0.0'
    GUID = '12345678-1234-1234-1234-123456789012'  # Generate a new GUID
    Author = 'Gullbra'
    Description = 'Testing PS Cmdlets'
    PowerShellVersion = '7.0'
    RequiredAssemblies = @('TestCmdlet.dll')
    CmdletsToExport = @('Send-Greeting')
    FunctionsToExport = @()
    VariablesToExport = @()
    AliasesToExport = @()
}