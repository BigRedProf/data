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

dotnet_version=""
if command -v dotnet >/dev/null 2>&1; then
	dotnet_version="$(cd "$repo_root" && dotnet --version 2>/dev/null || true)"
fi

task_version="$(command_version task --version || true)"
pwsh_version="$(command_version pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()' || true)"
required_dotnet_version="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([0-9][0-9.]*\)".*/\1/p' "$repo_root/global.json" | head -n 1)"
dotnet_channel="${required_dotnet_version%.*}"

needed=()
packages=()

if [[ -z "$dotnet_version" ]]; then
	needed+=(".NET SDK compatible with global.json ($required_dotnet_version)")
	packages+=("dotnet-sdk-$dotnet_channel")
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

	sudo apt-get update
	sudo apt-get install -y ca-certificates curl wget apt-transport-https software-properties-common

	if [[ " ${packages[*]} " == *" powershell "* || " ${packages[*]} " == *" dotnet-sdk-"* ]]; then
		if ! dpkg-query -W -f='${Status}' packages-microsoft-prod 2>/dev/null | grep -q "install ok installed"; then
			microsoft_package="$(mktemp --suffix=.deb)"
			trap 'rm -f "$microsoft_package"' EXIT
			wget -q "https://packages.microsoft.com/config/ubuntu/$VERSION_ID/packages-microsoft-prod.deb" -O "$microsoft_package"
			sudo dpkg -i "$microsoft_package"
		fi
	fi

	if [[ " ${packages[*]} " == *" task "* ]]; then
		task_setup="$(mktemp)"
		trap 'rm -f "${microsoft_package:-}" "${task_setup:-}"' EXIT
		curl -1sLf 'https://dl.cloudsmith.io/public/task/task/setup.deb.sh' -o "$task_setup"
		sudo -E bash "$task_setup"
	fi

	sudo apt-get update
	sudo apt-get install -y "${packages[@]}"
fi

cd "$repo_root"
task doctor

if [[ "$skip_verify" != true ]]; then
	task verify
fi

echo
echo "Bootstrap completed successfully."
