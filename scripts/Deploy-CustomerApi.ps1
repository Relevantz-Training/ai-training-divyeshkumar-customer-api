[CmdletBinding()]
param(
    [string]$ProjectPath = ".\src\CustomerApi\CustomerApi.csproj",
    [string]$Configuration = "Release",
    [string]$PublishDirectory = ".\artifacts\publish\CustomerApi",
    [string]$SiteName = "CustomerApi",
    [string]$AppPoolName = "CustomerApiAppPool",
    [string]$PhysicalPath = "C:\inetpub\CustomerApi",
    [string]$BindingInformation = "*:8080:",
    [string]$EnvironmentName = "Production",
    [switch]$CreateOrUpdateIisSite,
    [switch]$SkipTests,
    [switch]$SelfContained,
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)

    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Assert-CommandExists {
    param([string]$CommandName)

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "Required command '$CommandName' was not found. Install it and try again."
    }
}

function Resolve-FullPath {
    param([string]$Path)

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Test-IsAdministrator {
    $currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($currentIdentity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Initialize-IisManagement {
    if (-not (Test-IsAdministrator)) {
        throw "IIS site creation or update requires an elevated PowerShell session. Re-run PowerShell as Administrator."
    }

    $webAdministrationModule = Get-Module -ListAvailable -Name WebAdministration | Select-Object -First 1
    if ($webAdministrationModule) {
        Import-Module WebAdministration
        return
    }

    $appCmdPath = Join-Path $env:windir "System32\inetsrv\appcmd.exe"
    if (Test-Path $appCmdPath) {
        throw "The IIS appcmd tool is available at '$appCmdPath', but this script currently requires the WebAdministration PowerShell module. Install the IIS Management Scripts and Tools feature, or run the script without -CreateOrUpdateIisSite to publish only."
    }

    throw "IIS management tooling was not found on this machine. Install IIS plus the Management Scripts and Tools feature, then re-run the script. On Windows Server, add the 'Web-Scripting-Tools' feature. On Windows client editions, enable 'Internet Information Services > Web Management Tools > IIS Management Scripts and Tools'. You can still run the script without -CreateOrUpdateIisSite to publish the API only."
}

Assert-CommandExists -CommandName "dotnet"

$projectFullPath = Resolve-FullPath -Path $ProjectPath
$publishFullPath = Resolve-FullPath -Path $PublishDirectory

if (-not (Test-Path $projectFullPath)) {
    throw "Project file not found: $projectFullPath"
}

Write-Step "Restoring solution dependencies"
dotnet restore $projectFullPath

if (-not $SkipTests) {
    $solutionPath = Resolve-FullPath -Path ".\CustomerApi.sln"
    if (Test-Path $solutionPath) {
        Write-Step "Running automated tests before deployment"
        dotnet test $solutionPath --configuration $Configuration --no-restore
    }
}

$publishArguments = @(
    "publish",
    $projectFullPath,
    "--configuration", $Configuration,
    "--output", $publishFullPath,
    "--no-restore"
)

if ($SelfContained) {
    $publishArguments += @("--self-contained", "true", "-r", $RuntimeIdentifier)
}
else {
    $publishArguments += @("--self-contained", "false")
}

Write-Step "Publishing API to $publishFullPath"
dotnet @publishArguments

if (-not $CreateOrUpdateIisSite) {
    Write-Step "Publish completed"
    Write-Host "Published files are available at: $publishFullPath" -ForegroundColor Green
    Write-Host "Use -CreateOrUpdateIisSite to copy the output to IIS and configure the site." -ForegroundColor Yellow
    return
}

Initialize-IisManagement

Write-Step "Preparing IIS physical path"
if (-not (Test-Path $PhysicalPath)) {
    New-Item -Path $PhysicalPath -ItemType Directory -Force | Out-Null
}

Write-Step "Copying published files to IIS path"
Copy-Item -Path (Join-Path $publishFullPath "*") -Destination $PhysicalPath -Recurse -Force

Write-Step "Creating or updating IIS application pool"
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-WebAppPool -Name $AppPoolName | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.identityType -Value "ApplicationPoolIdentity"

Write-Step "Creating or updating IIS site"
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName -BindingInformation $BindingInformation | Out-Null
}
else {
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
}

Write-Step "Setting ASP.NET Core environment variable on IIS site"
$siteFilter = "system.webServer/aspNetCore"
$environmentVariablePath = "IIS:\Sites\$SiteName"

try {
    Remove-WebConfigurationProperty -PSPath $environmentVariablePath -Filter $siteFilter -Name ".environmentVariables.[name='ASPNETCORE_ENVIRONMENT']" -ErrorAction SilentlyContinue
}
catch {
}

Add-WebConfigurationProperty -PSPath $environmentVariablePath -Filter $siteFilter -Name "." -Value @{ environmentVariables = @{ name = "ASPNETCORE_ENVIRONMENT"; value = $EnvironmentName } } -ErrorAction SilentlyContinue

Write-Step "Restarting IIS site"
Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
Start-Website -Name $SiteName

Write-Host "`nDeployment complete." -ForegroundColor Green
Write-Host "Site Name: $SiteName"
Write-Host "App Pool: $AppPoolName"
Write-Host "Physical Path: $PhysicalPath"
Write-Host "Binding: $BindingInformation"
Write-Host "Environment: $EnvironmentName"
