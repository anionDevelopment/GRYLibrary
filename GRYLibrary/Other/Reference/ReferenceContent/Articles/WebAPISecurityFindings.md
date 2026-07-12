# WebAPI security findings

## Scope

This article documents security findings in the WebAPI-server framework provided by GRYLibrary
(namespace `GRYLibrary.Core.APIServer`). This is the code that concrete backends (for example OpenDMS,
ConSurv, CamMate, SimpleOCR) host through their `Program.cs` by configuring middlewares, services and
routes. The review focused on how a user can, through crafted requests or inputs, leak information or
trigger behaviour that the developer obviously did not intend.

The findings below describe the framework-level weaknesses. Where a concrete backend is mentioned, it
is used as an example of how the framework is wired up in practice; the underlying cause is in the
framework and should be fixed there.

Severity uses a simple High/Medium/Low scale reflecting exploitability and impact.

## Summary

| Id | Title | Severity |
|----|-------|----------|
| F1 | Plaintext credentials and sensitive data written to the request log | High |
| F2 | Unsalted single-round SHA-256 password hashing | High |
| F3 | Synchronous-over-asynchronous pipeline enables thread-pool and memory exhaustion | Medium-High |
| F4 | Security middlewares (WAF, obfuscation) are silent no-ops | Medium |
| F5 | User enumeration, non-constant-time login and no default throttling | Medium |
| F6 | OpenAPI specification exposed without authentication in production | Low |
| F7 | Unanchored regular expressions in authentication and logging allow-lists | Low |
| F8 | Partial API-key value written to the log | Low |

Positive observations are listed at the end.

## F1 — Plaintext credentials and sensitive data written to the request log (High)

### Description

The request-logging middleware captures the complete request body and the complete response body of
every request and writes them to the request log file.

- `Tools.ExecuteNextMiddlewareAndGetRequestAndResponseBody` reads the entire request body into a byte
  array and replaces the response stream with a `MemoryStream` so that the full response body can be
  captured.
- `RequestLoggingMiddleware.Invoke` passes both byte arrays to the concrete logger.
- `DRequestLoggingMiddleware.ShouldLogEntireRequestContentInLogFile` returns `true` unconditionally
  (both branches of the method `return true;`), so the full body is always written to the log file.
- There is no redaction of secret fields (no password, token, cookie or authorization filtering).

The logging middleware is registered in `specialMiddlewares1`, which runs **before** the authentication
and authorization middlewares (registered in `specialMiddlewares2`). Consequently the body of the
unauthenticated login request — which contains the user name and the plaintext password — is written
to the log, as are response bodies that can contain access tokens, personal data or document content.

The default body-length cap is 4000 bytes (`DRequestLoggingConfiguration.MaximalLengthofRequestBodies`);
a backend such as OpenDMS lowers it to 500, which is still more than enough to contain a full password.

### Impact

Anyone with read access to the log files (operators, backup archives, log-shipping/SIEM pipelines)
obtains valid user credentials and session tokens in cleartext. This turns a low-privilege log-read
capability into full account takeover.

### Affected code

- `APIServer/Utilities/Tools.cs` — `ExecuteNextMiddlewareAndGetRequestAndResponseBody`, `GetRequestBody`
- `APIServer/MidT/RLog/RequestLoggingMiddleware.cs`
- `APIServer/Mid/M05DLog/DRequestLoggingMiddleware.cs` — `ShouldLogEntireRequestContentInLogFile`, `Log`, `FormatBody`

### Recommendation

- Never log full request/response bodies unconditionally. Restrict full-body logging to error cases and
  make it opt-in.
- Redact known-sensitive routes and fields before writing to the log (login/registration/password-change
  bodies, `Set-Cookie`/authorization headers, token-bearing responses).
- Provide a per-route "do not capture body" list that is honoured before the body is read, and default
  authentication endpoints into it.

**Resolution:** Accepted as by-design and will not be changed. Logging the full request- and response-body
(including the plaintext login-body) is the intended behaviour of the request-logging middleware. The
mitigating assumption is that access to the log-files is restricted to trusted operators. No change.

## F2 — Unsalted single-round SHA-256 password hashing (High)

### Description

Password hashing is a single unsalted SHA-256 over the UTF-8 password, hex-encoded:

```csharp
public string Hash(string password)
{
    return GUtilities.ByteArrayToHexString(new SHA256().Hash(GUtilities.StringToByteArray(password)));
}
```

This pattern is used both in the transient (test) authentication service in GRYLibrary
(`TransientAuthenticationService.Hash`) and in the production persistent authentication service that
backends ship (for example `OpenDMSBackend.Core.Services.PersistentAuthenticationService.Hash`).

SHA-256 is a fast, general-purpose hash. With no per-user salt and no key-stretching:

- identical passwords produce identical stored hashes (visible directly in the user table);
- precomputed rainbow tables apply directly;
- offline brute-force runs at billions of guesses per second on commodity GPUs.

### Impact

If the user table leaks (SQL injection, backup exposure, insider access), the vast majority of passwords
are recovered quickly, enabling account takeover and credential-stuffing against other systems.

### Affected code

- `APIServer/Services/Trans/TransientAuthenticationService.cs` — `Hash`, `Login`
- Backend persistent authentication services deriving from the same pattern.

### Recommendation

Use a memory-hard / stretched password KDF — Argon2id (preferred), scrypt, bcrypt, or at minimum
PBKDF2 with a high iteration count — with a unique per-user random salt and a versioned hash format so
parameters can be upgraded over time.

**Resolution:** Tracked as an open point (see `OpenIssues.md`). Not fixed yet because it spans the persistent
authentication-services of every backend and requires a data-model-change and a database-migration.

## F3 — Synchronous-over-asynchronous pipeline enables thread-pool and memory exhaustion (Medium-High)

### Description

The custom middleware pipeline blocks synchronously on asynchronous work on every request:

- `ExceptionManagerMiddleware.Invoke` calls `this._Next(context).Wait();`.
- `Tools.ExecuteNextMiddlewareAndGetRequestAndResponseBody` calls `next(context).Wait();` and
  `memStream2.CopyToAsync(originalBody).Wait();`.
- Several middlewares call `.Wait()` / `.WaitAndGetResult()` on `WriteAsync` and other async calls.
- Kestrel is configured with `AllowSynchronousIO = true` to make this possible.

Because each request is processed by blocking a thread-pool thread for the whole request duration, a
burst of concurrent or deliberately slow requests exhausts the thread pool and stalls the server.

Independently, the logging path buffers the entire response into a `MemoryStream` before it is sent to
the client. For a document-management backend that serves large files this fully materialises large
payloads in memory (memory-pressure denial of service) and defeats response streaming.

### Impact

Availability: a modest number of concurrent slow requests, or a few large-response requests, can
degrade or stall the service without any authentication.

### Affected code

- `APIServer/MidT/Exception/ExceptionManagerMiddleware.cs`
- `APIServer/Utilities/Tools.cs`
- `APIServer/APIServer.cs` — `kestrelOptions.AllowSynchronousIO = true`

### Recommendation

- Make the middleware pipeline genuinely asynchronous (`await this._Next(context)`), remove the
  `.Wait()` calls, and remove `AllowSynchronousIO = true`.
- Do not buffer whole responses in memory; if request/response logging is needed, cap the captured size
  strictly and stream the remainder straight through.

**Resolution:** Tracked as an open point together with F5 (see `OpenIssues.md`). Not fixed yet because it is
an architectural change that affects every middleware in the framework and in the backends.

## F4 — Security middlewares (WAF, obfuscation) are silent no-ops (Medium)

### Description

Two middlewares are named as security controls but do nothing:

- `WebApplicationFirewallMiddleware.Invoke` only calls `this._Next(context)`; the entire intended
  behaviour (blocking suspicious payloads, rejecting invalid or entity-expanding XML, enforcing response
  size limits) is a TODO comment.
- `ObfuscationMiddleware.Invoke` computes whether the response body should be cleared and whether the
  status code should be normalised in the `Productive` environment, but the body is never actually
  cleared (`//TODO`), and the status-code rewrite is applied after the downstream response has already
  started — the code itself carries `//TODO check why this does not work properly`.

### Impact

An integrator who enables the WAF and obfuscation middlewares reasonably believes that request filtering
and error-detail obfuscation are active, while in reality neither provides any protection. This is a
"false sense of security" that can lead to weaker compensating controls elsewhere.

### Affected code

- `APIServer/MidT/WAF/WebApplicationFirewallMiddleware.cs`
- `APIServer/MidT/Obfuscation/ObfuscationMiddleware.cs`

### Recommendation

- Either implement these middlewares or clearly mark them as non-functional stubs and log a prominent
  warning when they are enabled, so operators are not misled.
- If obfuscation of error responses is required, perform it before the response starts (for example by
  buffering only error responses, or by setting the status/body inside the exception-handling
  middleware).

**Resolution:** Fixed. The `ObfuscationMiddleware` now normalizes the response-status-code in the productive
environment (every 2xx becomes 200, every 4xx/5xx becomes 400) and does nothing else; the rewrite is
registered as an `OnStarting`-callback so it reliably takes effect before the response is sent. The
`WebApplicationFirewallMiddleware` is now an abstract middleware which provides the block-and-log mechanism
(`Invoke`, `CheckRequest`, `GetRequestBody`, `WebApplicationFirewallResult`); the concrete firewall-rules must
be provided by the integrator in a concrete subclass.

## F5 — User enumeration, non-constant-time login and no default throttling (Medium)

### Description

The login flow leaks information and is not throttled by default:

- `Login` checks `UserWithNameExists` first and returns immediately for unknown users, but computes the
  password hash when the user exists. This timing difference is a username-validity oracle.
- When the credentials are correct but the account is locked, a distinct
  `NotAuthorizedException("User 'X' is locked.")` (403) is thrown, disclosing account existence and lock
  state.
- The password comparison uses `!=` on the hex hash strings, which short-circuits and is not
  constant-time.
- The `RateLimitingMiddleware` is abstract and opt-in (the application must subclass it to define client
  classification and register it). It is not wired into the login path by default, and for example
  OpenDMS's `Program.cs` does not register it, so credential-stuffing and brute-force against login are
  unthrottled out of the box.

### Impact

Attackers can enumerate valid user names and mount unthrottled online password-guessing attacks.

### Affected code

- Backend persistent authentication service `Login` (same shape as
  `TransientAuthenticationService.Login`).
- `APIServer/MidT/RateLimit/RateLimitingMiddleware.cs` (present but not enabled by default).

### Recommendation

- Return a single generic error for "invalid credentials" regardless of whether the user exists or is
  locked, and keep the code path timing-independent (always perform a hash comparison against a dummy
  hash for unknown users).
- Use a fixed-time comparison for secrets.
- Register a rate-limiter on authentication endpoints by default, and add account lockout/backoff after
  repeated failures.

**Resolution:** Tracked as an open point together with F3 (see `OpenIssues.md`). Not fixed yet because the
login-logic lives in the persistent authentication-service of every backend and a default rate-limiter has to
be coordinated with them.

## F6 — OpenAPI specification exposed without authentication in production (Low)

### Description

Backends can host the OpenAPI/Swagger specification in non-development environments
(`HostAPISpecificationForInNonDevelopmentEnvironment = true`) and allow-list the specification route for
unauthenticated access (`^/API/Other/Resources/APISpecification/*`). The full API document — every route,
parameter and schema — is then retrievable without credentials.

### Impact

This is primarily reconnaissance value: it hands an attacker a complete map of the attack surface. It is
often acceptable for public APIs, but for internal services it needlessly widens exposure.

### Recommendation

For non-public deployments, require authentication for the specification route, or disable
specification hosting in production.

**Resolution:** Accepted as by-design and will not be changed. Whether the OpenAPI-specification is hosted in
a non-development environment and allow-listed for unauthenticated access is a deliberate per-backend
configuration choice; the framework only provides the option. No change.

## F7 — Unanchored regular expressions in authentication and logging allow-lists (Low)

### Description

The set of routes where unauthenticated access is allowed, and the set of routes excluded from logging,
are matched with `new Regex(pattern).IsMatch(path)`:

- `AuthenticationMiddleware.AuthenticationIsRequired` iterates
  `RoutesWhereUnauthenticatedAccessIsAllowed` and returns "authentication not required" on any match.
- `DRequestLoggingMiddleware.IsIgnored` iterates `NotLoggedRoutes`.

The configured patterns are prefix-anchored (`^...`) but frequently not end-anchored. An entry such as
`^/API/Other/Maintenance/Metrics` (without a trailing `$`) also matches
`/API/Other/Maintenance/MetricsSomethingElse`. The currently shipped OpenDMS patterns do use `$` for the
sensitive maintenance routes, so this is not exploitable there today, but the mechanism is fragile: a
future or looser allow-list entry silently widens unauthenticated exposure.

### Impact

Latent risk of an authentication or logging bypass introduced by a seemingly harmless allow-list edit.

### Recommendation

- Anchor allow-list patterns fully (`^...$`) or match against the resolved endpoint/route pattern rather
  than the raw request path.
- Validate allow-list entries at start-up and prefer exact-match sets over regular expressions where
  possible.

**Resolution:** Documented. The concrete allow-list patterns are configured per backend, not in this library.
The library ships no default patterns for these allow-lists (`AuthenticationConfiguration.RoutesWhereUnauthenticatedAccessIsAllowed`
and `DRequestLoggingConfiguration.NotLoggedRoutes` both default to an empty set), so there is no affected
default value to change here. Anchoring/validating the patterns in the matching-code remains a possible future
hardening step.

## F8 — Partial API-key value written to the log (Low)

### Description

`APIKeyValidatorMiddleware.IsAuthorized` logs the first five characters of the presented API key:

```csharp
this._Log.Log($"Provided API-Key \"{apiKey.Substring(0, 5)}...\" is" + ... , LogLevel.Trace);
```

### Impact

Low: the message is at `Trace` level and only a prefix is written, but it is still secret material in
logs and reduces the effective entropy an attacker must guess if logs leak.

### Recommendation

Do not log any portion of key material. Log a non-reversible identifier (for example a hash prefix) if a
correlation handle is needed.

**Resolution:** Accepted and documented. Logging the first five characters of the API-key is intentional for
troubleshooting. It is confirmed to be written only to debug-logs: `APIKeyValidatorMiddleware.IsAuthorized`
logs the prefix at `LogLevel.Trace`, which is the most verbose (debug-only) level and does not appear in
normal production logs. No change.

## Positive observations

The following were checked and found to be implemented correctly:

- The authentication cookie is issued with `HttpOnly`, `Secure` and `SameSite=Strict`
  (`CookieTools.GetCookieWithSpecificExpiredDate`), which mitigates cross-site request forgery and
  script access to the session cookie.
- Kestrel is configured with `AddServerHeader = false`, reducing server-banner information disclosure.
- `app.UseRouting()` runs before the authentication and authorization middlewares, so the
  attribute-based checks (`[Authenticate]` / `[Authorize]`) that rely on `context.GetEndpoint()` see the
  resolved endpoint. Authentication therefore fails safe rather than being bypassed when routing has not
  run.
- The default exception handler does not write exception messages or stack traces into the HTTP response
  body (`GetExceptionResponceContent` returns an empty body), so internal error details are not leaked to
  clients.

## Prioritisation

Address F1 and F2 first: together they mean that a single log-file or database exposure yields usable
plaintext credentials. F3 (availability) and F4/F5 (missing/ineffective controls) should follow. F6–F8
are hardening items.
