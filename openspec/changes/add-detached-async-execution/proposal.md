# Proposal

## Why

`ExternalProgramExecutor` always owns the process it starts: even in the asynchronous waiting-state it redirects the standard-streams, waits for the process to end on a background-thread, captures the exit-code and kills the process (including its child-processes) on dispose. Callers who want to start a process that outlives the executor and is not tied to it as a child-process (fire-and-forget) have no way to do so. `epew` exposes this executor through its `-n`/`--NotSynchronous` option but can therefore only offer the owned variant.

## What Changes

- Add a `RunDetached` boolean property to the `RunAsynchronously` waiting-state (default `false`), which selects between the existing owned asynchronous execution (`false`) and a new detached fire-and-forget execution (`true`).
- Add a detached start-path to `ExternalProgramExecutor` for `RunAsynchronously` with `RunDetached = true`:
  - the process is started so that the operating-system does not treat it as a child-process of the executor (Windows: detached process-creation; Linux and macOS: new session via `setsid`),
  - no standard-stream-redirection, no exit-code-capture, no timeout-enforcement and no kill-on-dispose are applied to a detached process,
  - only the process-id is made available after the start.
- Keep the existing behavior unchanged for `RunDetached = false` and for `RunSynchronously`.
- Update `CreateEpewArgumentString` so that it renders the `RunDetached`-value for the `epew`-argument.
- **BREAKING** (`epew`, implemented in the `epew`-repository, not here): `-n`/`--NotSynchronous` changes from a boolean switch to an option that requires an explicit `True`/`False` value carrying `RunDetached`, defaulting to `False`.

## Capabilities

### New Capabilities
- `external-program-execution`: Starting and supervising external operating-system-processes through `ExternalProgramExecutor`, including the synchronous and asynchronous waiting-states and the detached fire-and-forget variant.

### Modified Capabilities
<!-- none: no existing capability covers external program execution yet -->

## Impact

- Code: `GRYLibrary/GRYLibrary/ExecutePrograms/WaitingStates/RunAsynchronously.cs`, `GRYLibrary/GRYLibrary/ExecutePrograms/ExternalProgramExecutor.cs` (new detached start-path and platform-specific process-creation), `CreateEpewArgumentString`.
- Platforms: Windows, Linux and macOS each need their own detached process-creation mechanism.
- Tests: `GRYLibrary/GRYLibraryTests/Testcases/ExternalProgramExecutorTest.cs`.
- Downstream: the `epew`-repository consumes this capability and must wire the new `RunDetached`-value into its `-n`/`--NotSynchronous` CLI-option (tracked and implemented there, out of scope for this GRYLibrary-change).
