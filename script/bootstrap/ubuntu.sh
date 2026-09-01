#!/usr/bin/env bash

set -euo pipefail

check_only=false
assume_yes=false
skip_verify=false

usage()
{
	echo "Usage: bash ./script/bootstrap/ubuntu.sh [--check-only] [--yes] [--skip-verify]"
}

while (($# > 0)); do
	case "$1" in
		--check-only)
			check_only=true
			;;
		--yes)
			assume_yes=true
			;;
		--skip-verify)
			skip_verify=true
			;;
		--help|-h)
			usage
			exit 0
			;;
		*)
			echo "Unknown option: $1" >&2
			usage >&2
			exit 2
			;;
	esac
	shift
done

bootstrap_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$bootstrap_root/../.." && pwd)"

# shellcheck disable=SC1091
source "$bootstrap_root/toolchain.env"
# shellcheck disable=SC1091
source /etc/os-release

if [[ "${ID:-}" != "ubuntu" ]]; then
	echo "ubuntu.sh supports Ubuntu only. On Windows, run script/bootstrap/windows.ps1." >&2
	exit 1
fi

version_at_least()
{
	local actual="$1"
	local required="$2"
	[[ "$(printf '%s\n%s\n' "$required" "$actual" | sort -V | head -n 1)" == "$required" ]]
}

command_version()
{
	local command_name="$1"
	shift

	if ! command -v "$command_name" >/dev/null 2>&1; then
		return 1
	fi

	"$command_name" "$@" 2>/dev/null | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+' | head -n 1
}

# Prints the SDK version that global.json actually resolves to, or nothing.
#
# The subtlety is that `dotnet --version` reports failure in a way that reads
# like success. Run from the repository root with only an incompatible SDK
# installed, it exits 145 and prints its complaint -- the list of SDKs it does
# have -- to STDOUT rather than stderr: "8.0.130 [/usr/lib/dotnet/sdk]". Capture
# the output alone and you store a non-empty string that even begins with a
# version number, and the bootstrap concludes the SDK is fine, installs nothing,
# and fails its own `task doctor` seconds later. The EXIT CODE is what tells the
# two apart, so this keys on that.
resolved_dotnet_version()
{
	local output
	output="$(cd "$repo_root" && dotnet --version 2>/dev/null)" || return 1
	[[ "$output" =~ ^[0-9]+\.[0-9]+\.[0-9]+[-0-9A-Za-z.]*$ ]] || return 1
	echo "$output"
}

# The .NET SDK does NOT come from a package manager, and that is the whole point
# of this function.
#
# global.json pins 8.0.403 with rollForward: latestFeature, which means the SDK
# must be in feature band 4xx or newer. Ubuntu 24.04 offers dotnet-sdk-8.0 at
# 8.0.104 and 8.0.130 -- band 1xx, and no amount of apt pinning changes that,
# because Microsoft's own packages.microsoft.com feed carries no dotnet-sdk-8.0
# for 24.04 at all. It defers to Ubuntu's archive. So `apt-get install
# dotnet-sdk-8.0` installs an SDK that CANNOT satisfy this repository and then
# fails the `task doctor` run at the end of this script -- issue #79.
#
# dotnet-install.sh with --jsonfile reads global.json and installs exactly the
# pinned version, so what lands is what the repository asked for rather than
# whatever the distribution happened to ship. Installing the exact pin always
# satisfies rollForward, whose policies only ever permit NEWER SDKs than the pin.
install_dotnet_sdk()
{
	# /usr/share/dotnet is Microsoft's own documented location for a scripted
	# install, and deliberately NOT /usr/lib/dotnet: that tree belongs to dpkg, and
	# a hand-installed SDK inside it is something a later apt upgrade or remove
	# would half-clobber. An apt-installed dotnet may therefore still exist at
	# /usr/bin/dotnet; the symlink below outranks it, and the check at the end of
	# this function is what proves it did.
	local install_dir="/usr/share/dotnet"

	local install_script="$bootstrap_temp/dotnet-install.sh"
	curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$install_script"

	echo "Installing the .NET SDK pinned by global.json into $install_dir"
	sudo bash "$install_script" --jsonfile "$repo_root/global.json" --install-dir "$install_dir" --no-path

	# -f because a stale link from an earlier install would otherwise be left
	# pointing at an SDK that no longer satisfies global.json, and -n so an
	# existing link to a directory is replaced rather than followed into.
	sudo ln -sfn "$install_dir/dotnet" /usr/local/bin/dotnet

	# Bash caches the full path of commands it has looked up, and dotnet was looked
	# up above -- before the symlink existed.
	hash -r

	# The check that makes this honest. `dotnet --version` run from the repository
	# root resolves through global.json and FAILS when no installed SDK satisfies
	# it, so this reports success only if the contract is actually met -- not
	# merely because some 8.0.x is now present.
	local installed_version
	installed_version="$(resolved_dotnet_version || true)"
	if [[ -z "$installed_version" ]]; then
		echo "Installed .NET SDK $required_dotnet_version into $install_dir, but the dotnet on PATH still does not satisfy global.json." >&2
		echo "The dotnet being resolved is $(command -v dotnet). Check that $install_dir precedes any distribution-packaged .NET on PATH." >&2
		(cd "$repo_root" && dotnet --version) >&2 || true
		exit 1
	fi

	echo "Installed .NET SDK $installed_version"
}

dotnet_version=""
if command -v dotnet >/dev/null 2>&1; then
	dotnet_version="$(resolved_dotnet_version || true)"
fi

task_version="$(command_version task --version || true)"
pwsh_version="$(command_version pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()' || true)"
required_dotnet_version="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([0-9][0-9.]*\)".*/\1/p' "$repo_root/global.json" | head -n 1)"

needed=()
packages=()
install_dotnet=false

if [[ -z "$dotnet_version" ]]; then
	needed+=(".NET SDK compatible with global.json ($required_dotnet_version)")
	install_dotnet=true
fi
if [[ -z "$task_version" ]] || ! version_at_least "$task_version" "$TASK_MIN_VERSION"; then
	needed+=("Task >= $TASK_MIN_VERSION")
	packages+=("task")
fi
if [[ -z "$pwsh_version" ]] || ! version_at_least "$pwsh_version" "$PWSH_MIN_VERSION"; then
	needed+=("PowerShell >= $PWSH_MIN_VERSION")
	packages+=("powershell")
fi

echo "BigRedProf.Data development bootstrap"
echo
echo " .NET SDK   : ${dotnet_version:-missing or incompatible}"
echo " Task       : ${task_version:-missing}"
echo " PowerShell : ${pwsh_version:-missing}"

if ((${#needed[@]} == 0)); then
	echo
	echo "Toolchain is already healthy."
else
	echo
	echo "Required changes:"
	printf ' - %s\n' "${needed[@]}"
fi

if [[ "$check_only" == true ]]; then
	if ((${#needed[@]} > 0)); then
		echo "Bootstrap check failed. Install the items listed above." >&2
		exit 1
	fi
	exit 0
fi

if ((${#needed[@]} > 0)); then
	if [[ "$assume_yes" != true ]]; then
		read -r -p "Continue with installation? [y/N] " answer
		if [[ ! "$answer" =~ ^[Yy]([Ee][Ss])?$ ]]; then
			echo "Bootstrap cancelled." >&2
			exit 1
		fi
	fi

	if ! command -v sudo >/dev/null 2>&1; then
		echo "sudo is required to install system packages." >&2
		exit 1
	fi

	# One scratch directory for every download below, removed on any exit. The
	# alternative -- a mktemp and a trap per file -- silently loses cleanups,
	# because each new trap REPLACES the previous one rather than adding to it.
	bootstrap_temp="$(mktemp -d)"
	trap 'rm -rf "$bootstrap_temp"' EXIT

	sudo apt-get update
	sudo apt-get install -y ca-certificates curl wget apt-transport-https software-properties-common

	# Microsoft's feed is here for PowerShell alone. It is deliberately NOT the
	# source of the .NET SDK any more -- see install_dotnet_sdk for why.
	if [[ " ${packages[*]} " == *" powershell "* ]]; then
		if ! dpkg-query -W -f='${Status}' packages-microsoft-prod 2>/dev/null | grep -q "install ok installed"; then
			microsoft_package="$bootstrap_temp/packages-microsoft-prod.deb"
			wget -q "https://packages.microsoft.com/config/ubuntu/$VERSION_ID/packages-microsoft-prod.deb" -O "$microsoft_package"
			sudo dpkg -i "$microsoft_package"
		fi
	fi

	if [[ " ${packages[*]} " == *" task "* ]]; then
		task_setup="$bootstrap_temp/task-setup.deb.sh"
		curl -1sLf 'https://dl.cloudsmith.io/public/task/task/setup.deb.sh' -o "$task_setup"
		sudo -E bash "$task_setup"
	fi

	# Guarded because the SDK no longer travels in this array, so it can now be
	# empty while there is still work to do.
	if ((${#packages[@]} > 0)); then
		sudo apt-get update
		sudo apt-get install -y "${packages[@]}"
	fi

	if [[ "$install_dotnet" == true ]]; then
		install_dotnet_sdk
	fi
fi

cd "$repo_root"
task doctor

if [[ "$skip_verify" != true ]]; then
	task verify
fi

echo
echo "Bootstrap completed successfully."
