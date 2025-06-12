@{
	RootModule = 'GetShortcut.dll'
	ModuleVersion = '1.0.1'
	GUID = '1a5d1225-7963-43bb-a15b-6f000ba217e5'
	Author = 'Gullbra'
	Description = 'Navigation Shortcuts'
	PowerShellVersion = '7.0'
	RequiredAssemblies = @('./GetShortcut.dll')
	CmdletsToExport = @('Get-Shortcut')
	AliasesToExport = @('goto')
	FunctionsToExport = @()
	VariablesToExport = @()
}
