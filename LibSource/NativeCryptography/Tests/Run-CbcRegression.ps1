param(
    [string]$NativeDll = "$PSScriptRoot\..\bin\x64\Release\NativeCryptography.dll",
    [string]$OutputDirectory = "$PSScriptRoot\bin"
)
$ErrorActionPreference = 'Stop'
$nativePath = (Resolve-Path -LiteralPath $NativeDll).Path
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path
Copy-Item -LiteralPath $nativePath -Destination (Join-Path $outputPath 'NativeCryptography.dll')
Copy-Item -LiteralPath "$PSScriptRoot\CbcRegression.cs" -Destination $outputPath
Push-Location $outputPath
try {
    & "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /platform:x64 /out:CbcRegression.exe /reference:NativeCryptography.dll CbcRegression.cs
    if ($LASTEXITCODE -ne 0) { throw 'C# regression test compilation failed' }
    & .\CbcRegression.exe
    if ($LASTEXITCODE -ne 0) { throw 'Native CBC regression tests failed' }
}
finally {
    Pop-Location
}
