# PS Cmdlet "Recipe"

## Module (C#)

### Setup

add package PowerShellStandard.Library (PowerShell SDK) to a class lib

```powershell
dotnet new classlib -n <ModuleName> && cd <ModuleName> && dotnet add package PowerShellStandard.Library
```


### Dev

[Overview Docs](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/cmdlet-overview?view=powershell-7.5)
[PS SDK](https://learn.microsoft.com/en-us/powershell/scripting/developer/windows-powershell-reference?view=powershell-7.5)
[Class Dec](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/cmdlet-class-declaration?view=powershell-7.5)
[Verb Names](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands?view=powershell-7.5)
[Cmdlet Attributes](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/cmdlet-attribute-declaration?view=powershell-7.5)
[Example](https://learn.microsoft.com/en-us/powershell/scripting/developer/cmdlet/how-to-write-a-simple-cmdlet?view=powershell-7.5)

#### Cmdlet attributes:

* ``[Cmdlet(Verb, Noun)]`` - Defines the cmdlet name
* ``[Parameter()]`` - Defines parameters with various options
* ``[OutputType()]`` - Specifies what the cmdlet returns

#### Base classes:

* ``PSCmdlet`` - Full-featured base class with access to PowerShell runtime
* ``Cmdlet`` - Simpler base class for basic cmdlets

#### Important methods:

* ``ProcessRecord()`` - Called for each input object
* ``BeginProcessing()`` - Called once at the start
* ``EndProcessing()`` - Called once at the end

#### Output methods:

* ``WriteObject()`` - Send objects to the pipeline
* ``WriteError()`` - Write error messages
* ``WriteWarning()`` - Write warning messages
* ``WriteVerbose()`` - Write verbose output


### Build and Import

[MsDocs](https://learn.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-binary-module?view=powershell-7.5)

```
dotnet build --configuration Release


$ModulePath = "$env:USERPROFILE\Documents\PowerShell\Modules\<ModuleName>"

# Copy files
New-Item -ItemType Directory -Path $ModulePath -Force
Copy-Item "bin\Release\<dotnetVersion>\<ModuleName>.dll" $ModulePath
Copy-Item "<ModuleName>.psd1" $ModulePath

Import-Module <ModuleName>

Get-Module -name <ModuleName> | Select-Object Name, Version, Path
```


## Versioning

Follow the MAJOR.MINOR.PATCH format:

MAJOR: Breaking changes (cmdlet removed, parameter removed, behavior changed)
MINOR: New features (new cmdlets, new parameters, new functionality)
PATCH: Bug fixes and small improvements

Examples:

1.0.0 → 1.0.1 (bug fix)
1.0.1 → 1.1.0 (added new cmdlet)
1.1.0 → 2.0.0 (removed a parameter, breaking change)


## Manifest

[MsDocs](https://learn.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-7.5)

```powershell
@{
    RootModule = 'MyCustomCmdlet.dll'                   # DON'T FORGET
    ModuleVersion = '1.0.0'
    GUID = '12345678-1234-1234-1234-123456789012'       # Generate a new GUID
    Author = 'Your Name'
    CompanyName = 'Your Company'
    Copyright = '(c) 2025 Your Company. All rights reserved.'
    Description = 'Custom PowerShell cmdlets'
    PowerShellVersion = '7.0'
    RequiredAssemblies = @('MyCustomCmdlets.dll')
    CmdletsToExport = @('Verb-Noun1', 'Verb-Noun2')     # Cmdlets in module (one class each)
    AliasesToExport = @('vn')
    FunctionsToExport = @()
    VariablesToExport = @()
}
```


## Uninstall

```
Get-Module <ModuleName> | uninstall-module
```
