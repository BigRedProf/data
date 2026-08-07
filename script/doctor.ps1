$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

. "$PSScriptRoot\common.ps1"

$requirements = @{}
$toolchainPath = Join-Path $PSScriptRoot "bootstrap\toolchain.env"
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

$requiredTaskVersion = [version]$requirements["TASK_MIN_VERSION"]
$requiredPwshVersion = [version]$requirements["PWSH_MIN_VERSION"]
$repoRoot = Get-RepoRoot
$globalJson = Get-Content -Raw (Join-Path $repoRoot "global.json") | ConvertFrom-Json
$requiredDotNetVersion = [version]$globalJson.sdk.version

Write-Step "BigRedProf toolchain diagnostics"

$ok = $true

Write-Host ""
Write-Host "==============================================================="
Write-Host "                 BIGREDPROF DEVELOPMENT ENVIRONMENT"
Write-Host "==============================================================="
Write-Host (" Repository  : data")
Write-Host (" Machine     : " + [Environment]::MachineName)
Write-Host (" PowerShell  : " + $PSVersionTable.PSVersion + " (" + $PSVersionTable.PSEdition + ")")
Write-Host (" OS          : " + [System.Environment]::OSVersion.VersionString)

if ($PSVersionTable.PSVersion -lt $requiredPwshVersion)
{
	Write-Host "   -> PowerShell is older than required version $requiredPwshVersion. Run the development bootstrap."
	$ok = $false
}

# --- .NET SDK ---------------------------------------------------------------
if (Test-CommandExists "dotnet")
{
	Push-Location $repoRoot
	try
	{
		$dotnetVersion = (& dotnet --version 2>$null | Out-String).Trim()
		if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($dotnetVersion))
		{
			Write-Host (" .NET SDK    : " + $dotnetVersion + " (global.json >= " + $requiredDotNetVersion + ")")
		}
		else
		{
			Write-Host (" .NET SDK    : <incompatible with global.json " + $requiredDotNetVersion + ">")
			$ok = $false
		}
	}
	finally
	{
		Pop-Location
	}
}
else
{
	Write-Host " .NET SDK    : <missing>"
	$ok = $false
}

# --- Task -------------------------------------------------------------------
if (Test-CommandExists "task")
{
	$taskVersionRaw = (& task --version).Trim()
	Write-Host (" Task        : " + $taskVersionRaw + " (required >= $requiredTaskVersion)")

	$match = [regex]::Match($taskVersionRaw, '\d+\.\d+\.\d+')
	if (-not $match.Success)
	{
		Write-Host "   -> Could not parse the Task version. Run the development bootstrap."
		$ok = $false
	}
	elseif ([version]$match.Value -lt $requiredTaskVersion)
	{
		Write-Host "   -> Task is older than the required version. Run the development bootstrap."
		$ok = $false
	}
}
else
{
	Write-Host " Task        : <missing> — run the development bootstrap"
	$ok = $false
}

# NOTE: no Docker check. This repository ships a library, not a service -- there
# is no Dockerfile and no image task, so reporting Docker would be noise.

Write-Host "==============================================================="

# --- .env encoding ----------------------------------------------------------
# NOTE: when doctor runs via `task doctor` this always passes, because Task
# parses the dotenv files at startup and would already have failed. It earns its
# keep when doctor is invoked DIRECTLY:
#
#   pwsh -NoProfile -ExecutionPolicy Bypass -File script/doctor.ps1
#
# which is the way to diagnose a repository whose .env files stop Task running
# at all. See Test-DotEnvEncoding in common.ps1.
if (-not (Test-DotEnvEncoding))
{
	$ok = $false
}

Write-Host ""

if (-not $ok)
{
	throw "Toolchain diagnostics failed. Resolve the items marked above."
}

Write-Host "[doctor] OK: toolchain looks healthy."
