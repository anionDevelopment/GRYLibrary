# Open issues

This article collects known open points in the WebAPI-server framework provided by GRYLibrary
(namespace `GRYLibrary.Core.APIServer`) which are acknowledged but not yet resolved. Each open point is
described under its own heading. See also `WebAPISecurityFindings.md` for the underlying security review.

## Stronger password hashing (was finding F2)

Password hashing currently is a single unsalted SHA-256 over the UTF-8 password, hex-encoded
(`TransientAuthenticationService.Hash` and the analogous persistent authentication services shipped by the
backends). SHA-256 is a fast, general-purpose hash; without a per-user salt and without key-stretching,
identical passwords produce identical stored hashes, precomputed rainbow tables apply directly and offline
brute-force runs at billions of guesses per second.

What should happen:

- Replace the hash with a memory-hard / stretched password-KDF: Argon2id (preferred), scrypt, bcrypt, or
  at minimum PBKDF2 with a high iteration-count.
- Use a unique per-user random salt.
- Use a versioned hash-format so parameters can be upgraded over time and old hashes can be re-hashed on the
  next successful login.

Why it is still open: the code-change itself is small, but it spans the persistent authentication-services of
every backend and requires a change to the `User`-data-model (salt / hash-format), a database-migration and an
upgrade-on-login strategy for existing hashes. This needs to be planned as a cross-repository change.

## Fully-asynchronous pipeline and login-hardening (was findings F3 and F5)

These two findings are grouped here because they share the same theme (resource-exhaustion / abuse-resistance
of the request-pipeline) and the rate-limiter is the control that connects them. They are, however, otherwise
independent: F3 is a broad architectural refactor of the middleware-pipeline, whereas F5 is a set of
login-specific hardening steps. They can be worked on separately.

### F3 — Synchronous-over-asynchronous pipeline and response-buffering

The custom middleware-pipeline blocks synchronously on asynchronous work on every request
(`this._Next(context).Wait()` in `ExceptionManagerMiddleware`, `next(context).Wait()` and
`memStream2.CopyToAsync(originalBody).Wait()` in `Tools.ExecuteNextMiddlewareAndGetRequestAndResponseBody`,
several `.Wait()` / `.WaitAndGetResult()` calls in other middlewares), which is only possible because Kestrel
is configured with `AllowSynchronousIO = true`. Because each request blocks a thread-pool thread for its whole
duration, a burst of concurrent or deliberately slow requests can exhaust the thread-pool. Independently, the
logging-path buffers the entire response into a `MemoryStream` before it is sent, which fully materialises
large payloads in memory and defeats response-streaming.

What should happen:

- Make the middleware-pipeline genuinely asynchronous (`await this._Next(context)`), remove the `.Wait()`
  calls and remove `AllowSynchronousIO = true`. This touches `AbstractMiddleware` and every middleware
  (including the backend-specific subclasses).
- Do not buffer whole responses in memory; if request/response-logging is needed, cap the captured size
  strictly and stream the remainder straight through.

Why it is still open: this is the only genuinely architectural change and is regression-prone because it
affects all middlewares in the framework and in every backend.

### F5 — Login-hardening (user-enumeration, non-constant-time comparison, default throttling)

The login-flow leaks information and is not throttled by default:

- `Login` returns immediately for unknown users but computes the password-hash when the user exists (a
  username-validity timing-oracle).
- A locked account throws a distinct `NotAuthorizedException("User 'X' is locked.")` (403), disclosing
  account existence and lock-state.
- The password-comparison uses `!=` on the hex-hash strings, which short-circuits and is not constant-time.
- `RateLimitingMiddleware` exists but is abstract and opt-in; it is not wired into the login-path by default,
  so credential-stuffing and brute-force against the login are unthrottled out of the box.

What should happen:

- Return a single generic "invalid credentials" error regardless of whether the user exists or is locked,
  and keep the code-path timing-independent (always perform a hash-comparison against a dummy-hash for
  unknown users).
- Use a fixed-time comparison for secrets.
- Register a rate-limiter on the authentication-endpoints by default and add account-lockout / backoff after
  repeated failures.

Why it is still open: the login-logic lives in the persistent authentication-service of every backend, and
providing a sensible default rate-limiter plus wiring it into the pipeline by default is a framework-change
that has to be coordinated with the backends.

## Fixed test-port for the in-process test-server (no dynamic free port)

When the server is started in test-mode (the `--TestRun` execution-mode), it always binds a fixed port
(`HTTP.DefaultPort`, currently 80). A dynamically-chosen free port for the in-process test-server is
currently **not supported**.

The practical consequence for the backends is that two integration-tests which each start an in-process
server must never run at the same time — they would both try to bind the same fixed port and fail. As long
as no free-port selection is available, the backends work around this by serializing all such tests with a
single shared, process-wide lock (a static semaphore acquired in `TestInitialize` and released in
`TestCleanup` by every integration-test-class that starts a server). A port that is still occupied while
that lock is held therefore belongs to something outside the test-run (a leftover server or a second
test-run in another process), and the test-framework fails fast with that diagnosis instead of cascading.

What should happen:

- Let the test-mode bind a free ephemeral port (bind to port 0 and read back the actually-assigned port),
  and expose that port to the test-framework so it can build the base-url from it.
- With a unique port per server-instance the shared serialization-lock would no longer be required, and even
  parallel test-runs (in separate processes) would stop colliding.

Why it is still open: the port is baked into the test-mode of the server-framework (`TestServer` /
`GetExecutionMode` bind `HTTP.DefaultPort`), and every backend's test-framework derives its base-url from
that same constant, so introducing a dynamic port is a framework-change that has to be coordinated with the
backends.
