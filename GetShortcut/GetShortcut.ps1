@{
    ModuleVersion = '1.0.1'
    GUID = '1a5d1225-7963-43bb-a15b-6f000ba217e5'  # Generate a new GUID
    Author = 'Martin'
    Description = 'Navigation Shortcuts'
    PowerShellVersion = '7.0'
    RequiredAssemblies = @('MyCustomCmdlets.dll')
    CmdletsToExport = @('Go-ToFolder')
    AliasesToExport = @('goto')
    FunctionsToExport = @()
    VariablesToExport = @()
    AliasesToExport = @()
}
