# Spec Delta

## Purpose

Starting and supervising external operating-system-processes through `ExternalProgramExecutor`, including the synchronous and asynchronous owned waiting-states and a detached fire-and-forget variant that lets a started process outlive the executor.

## ADDED Requirements

### Requirement: Synchronous execution returns the process-result

The system SHALL, when the waiting-state is synchronous, start the external process, wait until it terminates, and make its exit-code, standard-output and standard-error available to the caller.

#### Scenario: Synchronous run reports exit-code and output

- **WHEN** a program is executed with the synchronous waiting-state
- **THEN** the call returns only after the program has terminated
- **AND** the exit-code, the standard-output-lines and the standard-error-lines of the program are available afterwards

### Requirement: Owned asynchronous execution supervises the process

The system SHALL, when the waiting-state is asynchronous and not detached, start the external process, return control to the caller while the process keeps running, and continue to supervise it: capture its standard-streams and exit-code, enforce a configured timeout, and terminate the process (including its child-processes) when the executor is disposed.

#### Scenario: Owned asynchronous run keeps the result available

- **WHEN** a program is executed with the asynchronous, non-detached waiting-state
- **THEN** control returns to the caller before the program terminates
- **AND** after the program terminates its exit-code and output are available through the executor

#### Scenario: Disposing the executor terminates an owned asynchronous process

- **WHEN** an owned asynchronous process is still running and its executor is disposed
- **THEN** the process and its child-processes are terminated

### Requirement: Detached asynchronous execution outlives the executor

The system SHALL, when the waiting-state is asynchronous and detached, start the external process so that the operating-system does not treat it as a child-process of the executor, and the process SHALL keep running independently after the executor is disposed and after the starting process ends.

#### Scenario: Detached process is not a child of the executor

- **WHEN** a program is executed with the asynchronous detached waiting-state
- **THEN** the started process is not registered as a child-process of the process that runs the executor

#### Scenario: Detached process survives the executor

- **WHEN** a detached process is running and the executor is disposed or the starting process ends
- **THEN** the detached process keeps running and is not terminated by that disposal or end

### Requirement: Detached execution is fire-and-forget

The system SHALL, for a detached execution, not redirect the standard-streams, not capture the exit-code, not enforce a timeout and not terminate the process on dispose. After the start the system SHALL make the process-id available and SHALL NOT expose a result that depends on the process having terminated.

#### Scenario: Only the process-id is available for a detached process

- **WHEN** a detached process has been started
- **THEN** its process-id is available
- **AND** querying an execution-result that requires termination (exit-code, captured output, execution-duration) is reported as not available for a detached execution

### Requirement: Detached execution is supported on Windows, Linux and macOS

The system SHALL provide the detached start-path on Windows, Linux and macOS, using a per-platform mechanism that releases the process from the executor's process-ownership.

#### Scenario: Detached start on a supported platform

- **WHEN** a detached execution is requested on Windows, Linux or macOS
- **THEN** the process is started using that platform's detaching mechanism
- **AND** the start does not fail because of the platform

### Requirement: Detached-selection defaults to disabled

The asynchronous waiting-state SHALL expose the detached-selection as a boolean that defaults to disabled, so that an asynchronous execution is owned unless detachment is explicitly requested.

#### Scenario: Default asynchronous execution is owned

- **WHEN** an asynchronous execution is requested without explicitly enabling detachment
- **THEN** the execution is the owned asynchronous execution, not a detached one
