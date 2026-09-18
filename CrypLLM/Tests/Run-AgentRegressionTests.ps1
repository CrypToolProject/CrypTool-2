param(
    [string]$Configuration = 'Release',
    [string]$CompilerPath
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$buildDirectory = Join-Path $repository "CrypBuild/$Configuration"
if (!$CompilerPath) {
    $installation = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -prerelease -products '*' -property installationPath
    if ($installation) {
        $CompilerPath = Join-Path $installation 'MSBuild/Current/Bin/Roslyn/csc.exe'
    } else {
        $CompilerPath = Get-ChildItem "${env:ProgramFiles}/Microsoft Visual Studio" -Recurse -Filter csc.exe |
            Where-Object { $_.FullName -like '*MSBuild*Bin*Roslyn*' } | Select-Object -First 1 -ExpandProperty FullName
    }
}
$dependencyNames = @(
    'CrypLLM', 'CrypPluginBase', 'Microsoft.SemanticKernel.Abstractions',
    'Microsoft.SemanticKernel.Agents.Core', 'Microsoft.SemanticKernel.Agents.Abstractions',
    'Microsoft.SemanticKernel.Connectors.OpenAI', 'Microsoft.SemanticKernel.Core',
    'Microsoft.Extensions.AI.Abstractions', 'Microsoft.Extensions.DependencyInjection.Abstractions',
    'Microsoft.Extensions.DependencyInjection', 'Microsoft.Bcl.AsyncInterfaces',
    'System.Threading.Tasks.Extensions', 'Newtonsoft.Json',
    'Microsoft.Extensions.Logging.Abstractions', 'System.Text.Json',
    'WorkspaceManagerModel', 'WorkspaceManager', 'TextInput', 'TextOutput'
)
$references = foreach ($name in $dependencyNames) {
    $candidate = @("$buildDirectory/$name.dll", "$buildDirectory/Lib/$name.dll") | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if (!$candidate) { throw "Missing $name.dll. Build CrypLLM in $Configuration/x64 first." }
    "/reference:$candidate"
}
$netstandard = "${env:ProgramFiles(x86)}/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2/Facades/netstandard.dll"
$output = Join-Path $buildDirectory 'AgentRegressionTests.exe'
$wpfReferences = @('WindowsBase', 'PresentationCore', 'PresentationFramework', 'System.Xaml') | ForEach-Object {
    "/reference:${env:ProgramFiles(x86)}/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.7.2/$_.dll"
}
& $CompilerPath /nologo /target:exe /langversion:9.0 "/out:$output" /reference:System.Net.Http.dll /reference:System.Configuration.dll "/reference:$netstandard" $wpfReferences $references (Join-Path $PSScriptRoot 'AgentRegressionTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression test compilation failed.' }
# Use the application's redirects and probing paths for the same dependency versions.
$applicationConfig = Join-Path $buildDirectory 'CrypWin.exe.config'
if (!(Test-Path -LiteralPath $applicationConfig)) { $applicationConfig = Join-Path $repository 'CrypWin/app.config' }
Copy-Item -LiteralPath $applicationConfig -Destination "$output.config"
& $output
if ($LASTEXITCODE -ne 0) { throw 'Agent regression tests failed.' }
