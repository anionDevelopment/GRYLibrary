# Design

## Context

See proposal.md - Why. `ExternalProgramExecutor` has a single start-path (`StartProgram`) that always redirects the standard-streams, always starts a `RunningHandler` background-task that calls `WaitForProcessEnd` / `process.WaitForExit`, and whose `Dispose`/`Terminate` kills the process (`Kill(true)`). The waiting-state (`RunSynchronously` / `RunAsynchronously`) only selects whether the caller waits for that handler; the ownership of the process is identical in both. `ProcessStartInfo` does not expose process-creation flags, so true OS-level detachment cannot be expressed through it.

## Goals / Non-Goals

**Goals:**
- Add a detached fire-and-forget start-path selected by `RunAsynchronously.RunDetached = true`.
- Start the process on Windows, Linux and macOS so the OS does not keep it as a child/owned process of the executor's process.
- Leave the existing owned-asynchronous and synchronous paths byte-for-byte behaviorally unchanged.

**Non-Goals:**
- Capturing standard-output, standard-error, exit-code or execution-duration of a detached process (impossible once ownership is released; explicitly out of scope).
- Enforcing a timeout or killing a detached process via `Dispose`/`Terminate`.
- Running a detached process as a different user (`User`/`Password`): the owned path uses `ProcessStartInfo.UserName`; the detached path uses native process-creation and would need `CreateProcessAsUser`/credential-handling. Out of scope for this change; combining detached with a user SHALL fail fast.
- The `epew` CLI wiring of `-n`/`--NotSynchronous` to `True`/`False` (lives in the `epew`-repository).

## Decisions

### Decision 1: Separate detached start-path, selected in the waiting-state visitor

`WaitingStateRunVisitor.Handle(RunAsynchronously)` branches on `RunDetached`: `false` keeps the current `StartAsynchronously()`; `true` calls a new `StartDetached()`. `StartDetached()` reuses `Prepare()` (path-resolution, logging, start-guard) but then calls a new native start instead of `StartProgram()`, and does NOT create the `RunningHandler` task, does NOT redirect streams and does NOT begin the output-readers.

Rationale: keeps the owned path untouched (lowest regression-risk) and localizes all detached specifics. Alternative considered: a single parameterized `StartProgram` with many `if (detached)` branches - rejected, it would entangle the fire-and-forget lifecycle with the owned lifecycle and raise the regression-risk on the existing path.

### Decision 2: Native process-creation per platform

- **Windows**: P/Invoke `CreateProcessW` with creation-flags `DETACHED_PROCESS | CREATE_NEW_PROCESS_GROUP | CREATE_BREAKAWAY_FROM_JOB`. `CREATE_BREAKAWAY_FROM_JOB` releases the process from a Job the executor may live in (otherwise the OS would kill it with the Job). When the Job forbids breakaway, `CreateProcessW` fails with `ERROR_ACCESS_DENIED`; the implementation retries once without `CREATE_BREAKAWAY_FROM_JOB`. No handles are inherited (no stream-redirection). `CREATE_NO_WINDOW` is deliberately not set: Win32 ignores it when it is combined with `DETACHED_PROCESS`, which already detaches the process from the executor's console.
- **Linux and macOS**: P/Invoke libc `posix_spawn` with a spawn-attribute that sets `POSIX_SPAWN_SETSID`, so the child starts as leader of a new session (detached from the executor's process-group / controlling terminal). `POSIX_SPAWN_SETSID` is available on glibc >= 2.26 and macOS >= 10.15.

Rationale: `posix_spawn` + `POSIX_SPAWN_SETSID` is the one primitive that detaches cleanly on both Linux and macOS without `fork()` (which is unsafe in the managed runtime). On Windows the creation-flags are the only way to express detachment, which `ProcessStartInfo` cannot.

Alternatives considered:
- Shelling out to the `setsid` command on Unix - rejected: stock macOS does not ship a `setsid` binary, so it would not be portable.
- `fork()` + `setsid()` + `execvp()` via P/Invoke - rejected: `fork()` in a .NET process is unsafe (runtime threads/locks are not duplicated correctly).
- Windows `UseShellExecute = true` via `Process.Start` - rejected: it does not guarantee Job-breakaway and gives no control over the creation-flags.

### Decision 3: Encapsulate native creation behind an internal abstraction

Introduce an internal `IDetachedProcessStarter` with `WindowsDetachedProcessStarter` and `UnixDetachedProcessStarter` implementations, selected via `RuntimeInformation.IsOSPlatform`. Each takes resolved program, argument and working-directory and returns the started process-id. This keeps the native P/Invoke code out of `ExternalProgramExecutor`'s main body and makes each platform testable in isolation.

### Decision 4: State, result-access and lifecycle for detached

- A detached execution sets an internal `_IsDetached` flag and, after a successful start, a terminal-equivalent state so the object is "done" from the caller's view.
- `ProcessId` remains available (it already only requires state != `NotStarted`).
- `ExitCode`, `AllStdOutLines`, `AllStdErrLines`, `ExecutionDuration`, `ProcessWasAbortedDueToTimeout` SHALL throw an `InvalidOperationException` whose message states they are not available for a detached execution.
- `Dispose()` and `Terminate()` SHALL NOT kill a detached process: for `_IsDetached` they only release the executor's own resources.

### Decision 5: `CreateEpewArgumentString` renders the detached-value

`GetWaitingStateCreateEpewArgumentStringVisitor.Handle(RunAsynchronously)` renders the `RunDetached`-value as the value of the epew `--NotSynchronous` argument (for example `--NotSynchronous True`), matching the new `epew` CLI contract. The exact token must stay consistent with the `epew`-side option.

## Risks / Trade-offs

- [`POSIX_SPAWN_SETSID` unavailable on an old glibc/macOS] → Minimum versions (glibc 2.26 / macOS 10.15) are from 2017/2019; the detached start fails fast with a clear error on older systems rather than silently running attached.
- [Windows Job forbids `CREATE_BREAKAWAY_FROM_JOB`] → Retry without the flag; the process is then still detached from console/process-group but may be bound to the Job's lifetime. The limitation is logged.
- [Marshalling/quoting of program and arguments in native calls diverges from `ProcessStartInfo` behavior] → Reuse the already path-resolved program and the existing single `Argument` string; cover quoting (spaces, embedded quotes) with tests on each platform.
- [A detached process breaks if it writes to the inherited std-handles after the executor ends] → Detached creation does not inherit the executor's std-handles; on Unix the new session detaches from the controlling terminal.
- [Behavior-regression on the owned path] → The owned path is not modified; detached is an additive branch, covered by keeping existing tests green plus new detached tests.

## Migration Plan

- The GRYLibrary API change is additive (`RunDetached` defaults to `false`), so existing library-callers are unaffected.
- The breaking part is the downstream `epew` CLI (`-n` now requires `True`/`False`); it is implemented and released in the `epew`-repository and coordinated with the GRYLibrary release that contains `RunDetached`.
