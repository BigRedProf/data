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

The script uses APT, Microsoft's package feed for .NET and PowerShell, and
Task's official package feed. It requests `sudo` only for package operations.

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
