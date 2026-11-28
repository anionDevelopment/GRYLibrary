# Tasks

## 1. Waiting-state API

- [x] 1.1 Add `public bool RunDetached { get; set; } = false;` to `RunAsynchronously` and verify the project builds and existing `ExternalProgramExecutorTest` tests stay green.

## 2. Native detached process-creation

- [x] 2.1 Add internal abstraction `IDetachedProcessStarter` (input: resolved program, argument, working-directory; output: process-id) and a platform-selecting factory using `RuntimeInformation.IsOSPlatform`; verify a unit-test resolves the correct implementation per current platform.
- [x] 2.2 Implement `WindowsDetachedProcessStarter` via P/Invoke `CreateProcessW` with `DETACHED_PROCESS | CREATE_NEW_PROCESS_GROUP | CREATE_BREAKAWAY_FROM_JOB` (`CREATE_NO_WINDOW` is omitted because Win32 ignores it together with `DETACHED_PROCESS`), retrying once without `CREATE_BREAKAWAY_FROM_JOB` on `ERROR_ACCESS_DENIED`; verify on Windows that a started process has a valid process-id and runs detached. (Verified: detached integration-tests pass on Windows.)
- [ ] 2.3 Implement `UnixDetachedProcessStarter` via P/Invoke libc `posix_spawn` with `POSIX_SPAWN_SETSID` (flag value `0x80` on Linux, `0x400` on macOS); verify on Linux/macOS that the started process is a session-leader and detached from the starter's process-group. (Code implemented; runtime-verification on Linux/macOS still pending because the development-machine is Windows.)
- [x] 2.4 Verify argument/working-directory quoting (spaces and embedded quotes) in the native starters with per-platform tests. (`CommandLineArgumentParser` unit-tests pass on Windows.)

## 3. Detached start-path in `ExternalProgramExecutor`

- [x] 3.1 Branch `WaitingStateRunVisitor.Handle(RunAsynchronously)` on `RunDetached`: `false` keeps `StartAsynchronously()`; `true` calls a new `StartDetached()` that runs `Prepare()`, starts the process through `IDetachedProcessStarter`, sets `ProcessId`, sets an internal `_IsDetached` flag and a terminal-equivalent state, and does not create the `RunningHandler`, redirect streams or begin output-readers; verify a detached run returns immediately and exposes a valid `ProcessId`.
- [x] 3.2 Make `ExitCode`, `AllStdOutLines`, `AllStdErrLines`, `ExecutionDuration` and `ProcessWasAbortedDueToTimeout` throw an `InvalidOperationException` stating they are not available for a detached execution; verify with a test that each throws for a detached run while `ProcessId` succeeds.
- [x] 3.3 Make `Dispose()` and `Terminate()` not kill a detached process (release only the executor's own resources for `_IsDetached`); verify with a test that a detached process is still running after its executor is disposed, then clean it up in the test.
- [x] 3.4 Make a detached execution combined with a non-null `User` fail fast with a clear exception; verify with a test.

## 4. epew-argument rendering

- [x] 4.1 Update `GetWaitingStateCreateEpewArgumentStringVisitor.Handle(RunAsynchronously)` to render the `RunDetached`-value as the value of `--NotSynchronous` (for example `--NotSynchronous True`); verify with a test that `CreateEpewArgumentString` produces the expected token for both `RunDetached = true` and `RunDetached = false`.

## 5. Full verification

- [ ] 5.1 Run the repository's build-, test- and linting-pipeline and verify all specs for `external-program-execution` are satisfied and all tests are green.

## Workflow follow-up

- Downstream (`epew`-repository, separate change): change `-n`/`--NotSynchronous` from a switch to an option requiring `True`/`False` (value = `RunDetached`, default `False`), map it onto `RunAsynchronously.RunDetached`, adjust the async-mode handling so a detached run is fire-and-forget, update the help-text, and update the `epew` tests and documentation.
- Archive this GRYLibrary change after review and after the downstream `epew` change is coordinated.
