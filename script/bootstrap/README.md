# Development Bootstrap

The bootstrap scripts provision the machine-level tools required to build this
repository: the .NET SDK selected by `global.json`, Task, and PowerShell 7.
They do not replace Task as the repository's build orchestration layer.

## Windows 10/11

Run from the repository root in Windows PowerShell 5.1 or newer:

```powershell
powershell -ExecutionPolicy Bypass -File .\script\bootstrap\windows.ps1
```

WinGet is preferred. An existing Chocolatey installation is supported as a
fallback; Chocolatey requires an elevated shell.

## Ubuntu

Run from the repository root:

```bash
bash ./script/bootstrap/ubuntu.sh
```

The script uses APT for PowerShell (from Microsoft's package feed) and Task
(from Task's official package feed). It requests `sudo` only for package
operations.

The .NET SDK deliberately does **not** come from APT. `global.json` pins a
feature band, and the `dotnet-sdk-8.0` package available on Ubuntu 24.04 is in a
lower one, so installing it produces an SDK that cannot satisfy this repository.
Bootstrap instead runs Microsoft's `dotnet-install.sh` against `global.json`,
installing exactly the pinned version into `/usr/share/dotnet` and linking it at
`/usr/local/bin/dotnet`. It then confirms the result actually satisfies
`global.json` before continuing, rather than assuming the install was enough.

## Options

| Option          | Behavior                                                        |
| --------------- | --------------------------------------------------------------- |
| `--check-only`  | Report tool status without changing the machine                 |
| `--yes`         | Accept the displayed installation plan without prompting        |
| `--skip-verify` | Run `task doctor` after setup, but skip the final `task verify`  |

Without options, bootstrap displays its plan, asks once for confirmation,
installs only missing or incompatible tools, runs `task doctor`, and finishes
with `task verify`. Re-running it on a healthy machine is safe.

`toolchain.env` contains shared minimum versions for Task and PowerShell. The
.NET SDK requirement remains in `global.json`, which is its native source of
truth.
