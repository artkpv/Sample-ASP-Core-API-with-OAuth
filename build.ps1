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
}

function Clean 
{
    Write-Host "Cleaning..."
    remove-Item $artifactsBaseDir/* -Force -Recurse -Verbose
}

function Provision {
    param ($script = $null)
    & _EnsureEnvironment
    # Development environment settings:
    $env:ASPNETCORE_Kestrel__Certificates__Default__Password = "1234"
    $env:ASPNETCORE_Kestrel__Certificates__Default__Path = (Resolve-Path "./localhost.pfx").Path
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $cdDir = (Resolve-Path .).Path

    $runArgs = @{
        PassThru = $true
        FilePath = "dotnet"
        NoNewWindow  = $true
    }

    $apiP = Start-Process @runArgs -Args "run --project BA.WebAPI" -RedirectStandardOutput $artifactsDir/apiserver.logs.txt
    $iserverP = Start-Process @runArgs -Args "run --project BA.IServer" -RedirectStandardOutput $artifactsDir/iserver.logs.txt

    if ($script)
    {
        sleep 3  # Better make ping...
        & $script
    }

    read-host “Servers started. Press ENTER to stop...”

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
    & dotnet build
}

if ($Task)
{
    & "$Task" 
}
else 
{
    & Build
}