[CmdletBinding()]
param(
	[Alias("check-only")]
	[switch] $CheckOnly,

	[switch] $Yes,

	[Alias("skip-verify")]
	[switch] $SkipVerify
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$bootstrapRoot = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $bootstrapRoot "..\..")).Path
$toolchainPath = Join-Path $bootstrapRoot "toolchain.env"

function Get-ToolchainRequirements
{
	$requirements = @{}

	foreach ($line in [System.IO.File]::ReadAllLines($toolchainPath))
	{
		if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith("#"))
		{
			continue
		}

		$separatorIndex = $line.IndexOf('=')
		if ($separatorIndex -lt 1)
		{
			throw "Invalid toolchain requirement: $line"
		}

		$name = $line.Substring(0, $separatorIndex).Trim()
		$value = $line.Substring($separatorIndex + 1).Trim()
		$requirements[$name] = $value
	}

	return $requirements
}

function Test-CommandExists
{
	param(
		[Parameter(Mandatory = $true)]
		[string] $Name
	)

	return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Get-CommandVersion
{
	param(
		[Parameter(Mandatory = $true)]
		[string] $Name,

		[Parameter(Mandatory = $true)]
		[string[]] $Arguments
	)

	if (-not (Test-CommandExists $Name))
	{
		return $null
	}

	$output = (& $Name @Arguments 2>$null | Out-String).Trim()
	if ($LASTEXITCODE -ne 0)
	{
		return $null
	}

	$match = [regex]::Match($output, '\d+\.\d+\.\d+')
	if (-not $match.Success)
	{
		return $null
	}

	return [version]$match.Value
}

function Get-DotNetVersion
{
	if (-not (Test-CommandExists "dotnet"))
	{
		return $null
	}

	Push-Location $repoRoot
	try
	{
		$output = (& dotnet --version 2>$null | Out-String).Trim()
		if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($output))
		{
			return $null
		}

		return [version]$output
	}
	finally
	{
		Pop-Location
	}
}

function Invoke-PackageCommand
{
	param(
		[Parameter(Mandatory = $true)]
		[string] $Command,

		[Parameter(Mandatory = $true)]
		[string[]] $Arguments
	)

	& $Command @Arguments
	if ($LASTEXITCODE -ne 0)
	{
		throw "Package command failed with exit code ${LASTEXITCODE}: $Command $($Arguments -join ' ')"
	}
}

function Install-WinGetPackage
{
	param(
		[Parameter(Mandatory = $true)]
		[string] $Id
	)

	$arguments = @(
		"install",
		"--id", $Id,
		"--exact",
		"--source", "winget",
		"--accept-package-agreements",
		"--accept-source-agreements"
	)
	Invoke-PackageCommand "winget" $arguments
}

function Install-ChocolateyPackage
{
	param(
		[Parameter(Mandatory = $true)]
		[string] $Name
	)

	Invoke-PackageCommand "choco" @("upgrade", $Name, "--yes")
}

function Update-ProcessPath
{
	$machinePath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
	$userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
	$env:PATH = "$machinePath;$userPath"
}

if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT)
{
	throw "windows.ps1 supports Windows only. On Ubuntu, run ./script/bootstrap/ubuntu.sh."
}

$requirements = Get-ToolchainRequirements
$requiredTaskVersion = [version]$requirements["TASK_MIN_VERSION"]
$requiredPwshVersion = [version]$requirements["PWSH_MIN_VERSION"]
$globalJson = Get-Content -Raw (Join-Path $repoRoot "global.json") | ConvertFrom-Json
$requiredDotNetVersion = [version]$globalJson.sdk.version

$dotnetVersion = Get-DotNetVersion
$gitVersion = Get-CommandVersion "git" @("--version")
$taskVersion = Get-CommandVersion "task" @("--version")
$pwshVersion = Get-CommandVersion "pwsh" @("-NoProfile", "-Command", '$PSVersionTable.PSVersion.ToString()')

$needed = [System.Collections.Generic.List[string]]::new()
if ($null -eq $dotnetVersion)
{
	$needed.Add(".NET SDK compatible with global.json ($requiredDotNetVersion)")
}
# Presence only, no version floor -- MinVer needs git on PATH to derive the
# build version and names no minimum. See the Git block in script/doctor.ps1.
if ($null -eq $gitVersion)
{
	$needed.Add("Git (MinVer reads the version from git history during the build)")
}
if ($null -eq $taskVersion -or $taskVersion -lt $requiredTaskVersion)
{
	$needed.Add("Task >= $requiredTaskVersion")
}
if ($null -eq $pwshVersion -or $pwshVersion -lt $requiredPwshVersion)
{
	$needed.Add("PowerShell >= $requiredPwshVersion")
}

Write-Host "BigRedProf.Data development bootstrap"
Write-Host ""
Write-Host (" .NET SDK   : " + $(if ($null -eq $dotnetVersion) { "missing or incompatible" } else { $dotnetVersion }))
Write-Host (" Git        : " + $(if ($null -eq $gitVersion) { "missing" } else { $gitVersion }))
Write-Host (" Task       : " + $(if ($null -eq $taskVersion) { "missing" } else { $taskVersion }))
Write-Host (" PowerShell : " + $(if ($null -eq $pwshVersion) { "missing" } else { $pwshVersion }))

if ($needed.Count -eq 0)
{
	Write-Host ""
	Write-Host "Toolchain is already healthy."
}
else
{
	Write-Host ""
	Write-Host "Required changes:"
	foreach ($item in $needed)
	{
		Write-Host " - $item"
	}
}

if ($CheckOnly)
{
	if ($needed.Count -gt 0)
	{
		throw "Bootstrap check failed. Install the items listed above."
	}

	exit 0
}

if ($needed.Count -gt 0)
{
	if (-not $Yes)
	{
		$answer = Read-Host "Continue with installation? [y/N]"
		if ($answer -notmatch '^(y|yes)$')
		{
			throw "Bootstrap cancelled."
		}
	}

	$packageManager = $null
	if (Test-CommandExists "winget")
	{
		$packageManager = "winget"
	}
	elseif (Test-CommandExists "choco")
	{
		$packageManager = "choco"
	}
	else
	{
		throw "Neither WinGet nor Chocolatey is installed. Install WinGet (App Installer), then rerun this script."
	}

	if ($packageManager -eq "choco")
	{
		$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
		$principal = [Security.Principal.WindowsPrincipal]::new($identity)
		if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
		{
			throw "Chocolatey requires an elevated shell. Rerun Windows PowerShell as Administrator."
		}
	}

	if ($null -eq $dotnetVersion)
	{
		if ($packageManager -eq "winget")
		{
			Install-WinGetPackage "Microsoft.DotNet.SDK.8"
		}
		else
		{
			Install-ChocolateyPackage "dotnet-8.0-sdk"
		}
	}

	if ($null -eq $gitVersion)
	{
		if ($packageManager -eq "winget")
		{
			Install-WinGetPackage "Git.Git"
		}
		else
		{
			Install-ChocolateyPackage "git"
		}
	}

	if ($null -eq $taskVersion -or $taskVersion -lt $requiredTaskVersion)
	{
		if ($packageManager -eq "winget")
		{
			Install-WinGetPackage "Task.Task"
		}
		else
		{
			Install-ChocolateyPackage "go-task"
		}
	}

	if ($null -eq $pwshVersion -or $pwshVersion -lt $requiredPwshVersion)
	{
		if ($packageManager -eq "winget")
		{
			Install-WinGetPackage "Microsoft.PowerShell"
		}
		else
		{
			Install-ChocolateyPackage "powershell-core"
		}
	}

	Update-ProcessPath
}

Push-Location $repoRoot
try
{
	& task doctor
	if ($LASTEXITCODE -ne 0)
	{
		throw "task doctor failed with exit code $LASTEXITCODE."
	}

	if (-not $SkipVerify)
	{
		& task verify
		if ($LASTEXITCODE -ne 0)
		{
			throw "task verify failed with exit code $LASTEXITCODE."
		}
	}
}
finally
{
	Pop-Location
}

Write-Host ""
Write-Host "Bootstrap completed successfully."
