$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & dotnet build IdeaUnstuck.sln -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    New-Item -ItemType Directory -Path 'release' -Force | Out-Null
    Copy-Item -Path 'IdeaUnstuck.AddIn\bin\Release\net48\IdeaUnstuck.*.dll' -Destination 'release' -Force
    Copy-Item -Path 'IdeaUnstuck.SelfTest\bin\Release\net48\IdeaUnstuck.SelfTest.exe*' -Destination 'release' -Force
} finally { Pop-Location }
