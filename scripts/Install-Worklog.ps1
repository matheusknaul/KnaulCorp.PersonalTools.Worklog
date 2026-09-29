$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path -Parent $PSScriptRoot
Push-Location $workspacePath
try {
    dotnet pack src/KnaulCorp.PersonalTools.Worklog.Cli -c Release -o artifacts/packages
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o pacote.' }
    $installed = dotnet tool list --global
    if ($installed -match 'knaulcorp.personaltools.worklog') {
        dotnet tool uninstall --global KnaulCorp.PersonalTools.Worklog
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao remover a versão anterior de Worklog.' }
    }
    dotnet tool install --global KnaulCorp.PersonalTools.Worklog --source artifacts/packages --version 1.0.0 --no-http-cache
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao instalar Worklog.' }
} finally { Pop-Location }
