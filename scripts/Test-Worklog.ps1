param([string]$Image = 'postgres:18.3')
$ErrorActionPreference = 'Stop'
$workspacePath = Split-Path -Parent $PSScriptRoot
$containerName = 'worklog-tests-' + [Guid]::NewGuid().ToString('N')
$testPassword = [Guid]::NewGuid().ToString('N')
$previousConnection = $env:WORKLOG_TEST_CONNECTION
$started = $false
Push-Location $workspacePath
try {
    docker run --detach --rm --name $containerName --publish 127.0.0.1::5432 --env "POSTGRES_PASSWORD=$testPassword" --env POSTGRES_DB=worklog_tests $Image | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Não foi possível iniciar o PostgreSQL de teste.' }
    $started = $true
    $binding = docker port $containerName 5432
    $testPort = ($binding -split ':')[-1]
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        docker exec $containerName pg_isready -U postgres -d worklog_tests *> $null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw 'PostgreSQL de teste não ficou pronto.' }
    $env:WORKLOG_TEST_CONNECTION = "Host=127.0.0.1;Port=$testPort;Database=worklog_tests;Username=postgres;Password=$testPassword"
    dotnet test --configuration Release --logger 'console;verbosity=minimal'
    if ($LASTEXITCODE -ne 0) { throw 'Há testes com falha.' }
} finally {
    $env:WORKLOG_TEST_CONNECTION = $previousConnection
    if ($started) { docker stop $containerName | Out-Null }
    Pop-Location
}

