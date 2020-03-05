#!/bin/pwsh

[CmdletBinding(PositionalBinding = $false)]
Param (
    [parameter(Position=1)]
    [string] $Task
)

cd $PSScriptRoot  # All paths are relative.
$artifactsBaseDir = "./artifacts"
$artifactsDir = $artifactsBaseDir + "/" + (Get-Date -f 'yyyyMMdd_HHmmss')
$scriptInvocation = $MyInvocation

function _EnsureEnvironment 
{
    if (-not (test-path $artifactsDir)) { new-item -path $artifactsDir -type Directory > $null }
    $artifactsDir = (Resolve-Path $artifactsDir).Path
    set-item Env:/BIKINGAPP_ARTIFACTSDIR -Value $artifactsDir

@"
BikingApp build run. $( get-date )
$( $scriptInvocation | out-string )
Host:
$( $Host | out-string )
"@ > $artifactsDir/readme.txt


    # Development environment settings:
    $env:ASPNETCORE_Kestrel__Certificates__Default__Password = "1234"
    $env:ASPNETCORE_Kestrel__Certificates__Default__Path = (Resolve-Path "./localhost.pfx").Path
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $cdDir = (Resolve-Path .).Path

    $env:BIKINGAPP_API_URLS = "https://localhost:5000;http://localhost:5001"

    $env:BIKINGAPP_ISERVER_URLS = "https://localhost:5002;http://localhost:5003"
}

function Clean 
{
    Write-Host "Cleaning..."
    remove-Item $artifactsBaseDir/* -Force -Recurse -Verbose
}

function Provision {
    param($script = $null)
    & _EnsureEnvironment
    $runArgs = @{
        PassThru = $true
        FilePath = "dotnet"
        NoNewWindow  = $true
    }

    $apiPArgs = @{
        Args = "run --project BA.WebAPI" 
        RedirectStandardOutput = "$artifactsDir/apiserver.logs.txt"
    }
    $apiP = Start-Process @runArgs @apiPArgs
    write-host "API started
    URLs: $($env:BIKINGAPP_API_URLS)
    Logs: $($apiPArgs.RedirectStandardOutput)"

    $iserverArgs = @{
        Args = "run --project BA.IServer" 
        RedirectStandardOutput = "$artifactsDir/iserver.logs.txt"
    }
    $iserverP = Start-Process @runArgs @iserverArgs
    write-host "IdentityServer (OAuth) started
    URLs: $($env:BIKINGAPP_ISERVER_URLS)
    Logs: $($iserverArgs.RedirectStandardOutput)"

    if ($script) 
    {
        sleep 2  # To wait servers to start.
        & $script
    }

    read-host “Press ENTER to stop the API...”

    Stop-Process -Id $apiP.Id
    Stop-Process -Id $iserverP.Id

    # Ensure the servers killed. 'dotnet' starts forked processes.
    Get-Process -Name BA.WebAPI,BA.IServer -ErrorAction SilentlyContinue | Stop-Process
}

function UnitTest {
    & _EnsureEnvironment
    Write-Host "`n============= UNIT TESTS ============`n"
    dotnet watch --project BA.WebAPI.UnitTests test 
}

function E2ETest {
    & _EnsureEnvironment
    Write-Host "`n============= E2E TESTS ============`n"

    & pytest BA.E2ETests
}

function Test {
    & E2ETest
}

function Build {
    & _EnsurePythonClientsContextCreated
    & dotnet build
}

function Demo {
    & Build
    & Provision -Script {
        python3 ./BA.CliClient/run.py
    }
}

function _EnsurePythonClientsContextCreated
{
    $pythonContextFile = (Resolve-Path ./BA.E2ETests/context.py).Path
    $pythonContextFileLink = (Resolve-Path ./BA.CliClient/context.py).Path
    if (-not (test-path $pythonContextFileLink ))
    {
        new-item -Path $pythonContextFileLink -ItemType HardLink -Value $pythonContextFile
    }
}

if ($Task)
{
    & "$Task" 
}
else 
{
    & Build
}
