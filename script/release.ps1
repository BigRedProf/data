<#
.SYNOPSIS
	Releases a version by tagging it and pushing the tag.

.DESCRIPTION
	Publishing is CI's job, and deliberately so: nothing local can push a package. What this
	does is create the `v*` tag that makes CI publish, which is the step that is easy to get
	wrong and impossible to take back.

	nuget.org versions are immutable and effectively permanent. Tagging the wrong commit, a
	stale main, a dirty tree, or a typo'd version cannot be undone by deleting the tag, because
	by then the package is public. So this checks all of that first, shows what it is about to
	do, and asks.

.PARAMETER Version
	The version to release, with or without the leading `v`: 0.9.0, v0.9.0, 0.9.0-rc.5.

.PARAMETER Yes
	Skip the confirmation. For non-interactive use; think before reaching for it.

.PARAMETER SkipVerify
	Skip `task verify`. CI runs it too, so this only trades a local minute for a slower
	discovery of the same failure -- after the tag exists.
#>

[CmdletBinding()]
param(
	[Parameter(Mandatory = $true, Position = 0)]
	[string] $Version,

	[switch] $Yes,

	[switch] $SkipVerify
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/common.ps1"

$repoRoot = Get-RepoRoot
Push-Location $repoRoot
try
{
	# Everything below throws a plain sentence on refusing; the catch at the bottom prints it
	# as one line. A wall of PowerShell error formatting would bury the reason.

	# ---- the version itself -------------------------------------------------------------
	$version = $Version.Trim()
	if ($version.StartsWith('v'))
	{
		$version = $version.Substring(1)
	}

	# major.minor.patch, optionally a prerelease of dot-separated alphanumerics.
	$semVer = '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z]+(\.[0-9A-Za-z]+)*)?$'
	if ($version -notmatch $semVer)
	{
		throw "'$Version' is not a version. Expected major.minor.patch, optionally -prerelease: 0.9.0, 0.9.0-rc.5."
	}

	$tag = "v$version"
	$isPrerelease = $version.Contains('-')

	# ---- the working copy ---------------------------------------------------------------
	Write-Step "Checking the working copy"

	$branch = (git rev-parse --abbrev-ref HEAD).Trim()
	if ($branch -ne 'main')
	{
		throw "Releases are tagged on main, and this is '$branch'. A release names a commit everyone has."
	}

	if (@(git status --porcelain).Count -gt 0)
	{
		# Untracked files count. An uncommitted source file compiles locally and not in CI,
		# so a verify that passed here would say nothing about the commit being tagged.
		throw "The working tree has changes, including untracked files. Tag a commit that is exactly what CI will build."
	}

	Invoke-Checked -Command 'git' -Arguments @('fetch', '--quiet', '--tags', 'origin')

	$localHead = (git rev-parse HEAD).Trim()
	$remoteHead = (git rev-parse origin/main).Trim()
	if ($localHead -ne $remoteHead)
	{
		throw "Local main is not origin/main. Pull (or push) first, so the tag names the commit others will see."
	}

	if (@(git tag --list $tag).Count -gt 0)
	{
		throw "Tag $tag already exists locally."
	}

	$remoteTag = @(git ls-remote --tags origin "refs/tags/$tag")
	if ($remoteTag.Count -gt 0)
	{
		throw "Tag $tag already exists on origin. nuget.org versions are immutable, so this version is spent; pick the next one."
	}

	# ---- does it actually build ---------------------------------------------------------
	if ($SkipVerify)
	{
		Write-Host "[release] Skipping verify at your request. CI will still run it."
	}
	else
	{
		Write-Step "Verifying before tagging (CI runs this too, but after the tag exists)"
		Invoke-Checked -Command 'task' -Arguments @('verify')
	}

	# ---- confirm ------------------------------------------------------------------------
	$subject = (git log -1 --pretty=%s).Trim()
	$shortSha = (git rev-parse --short HEAD).Trim()

	Write-Step "Ready to release"
	Write-Host "  tag:      $tag"
	Write-Host "  version:  $version$(if ($isPrerelease) { '   (prerelease)' })"
	Write-Host "  commit:   $shortSha  $subject"
	Write-Host ""
	Write-Host "  Pushing this tag publishes BigRedProf.Data.Core, BigRedProf.Data.PackRatCompiler"
	Write-Host "  and BigRedProf.Data.Tape to nuget.org. Published versions are permanent: $version"
	Write-Host "  can never be republished, even if it turns out to be wrong."

	if (-not $Yes)
	{
		Write-Host ""
		# Read-Host returns null at end of input, which is what a non-interactive caller gets.
		# That is a decline, not a crash.
		$answer = Read-Host "  Type the version to confirm"
		if ($null -eq $answer -or $answer.Trim() -ne $version)
		{
			throw "Not confirmed; nothing was tagged."
		}
	}

	# ---- do it --------------------------------------------------------------------------
	Write-Step "Tagging and pushing"
	Invoke-Checked -Command 'git' -Arguments @('tag', '-a', $tag, '-m', "$version")
	try
	{
		Invoke-Checked -Command 'git' -Arguments @('push', 'origin', $tag)
	}
	catch
	{
		# Leaving a local tag behind would make a retry fail for the wrong reason.
		git tag -d $tag | Out-Null
		throw
	}

	Write-Host ""
	Write-Host "[release] OK: $tag pushed. CI is building and will publish to nuget.org."
	Write-Host "[release] Watch it with: gh run list --limit 1"
	Write-Host "[release] nuget.org takes a few minutes to index, and its registration index"
	Write-Host '[release]   lags the flat container, so `dotnet add package` may not see it'
	Write-Host '[release]   for a little while after the run goes green.'
}
catch
{
	Write-Host ""
	Write-Host "[release] REFUSED: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}
finally
{
	Pop-Location
}
