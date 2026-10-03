# GRYLibrary

![Coverage](./GRYLibrary/Other/Resources/TestCoverageBadges/badge_shieldsio_linecoverage_blue.svg)

GRYLibrary is a collection with some useful .NET classes and functions which are very easy (re)usable.

The GRYLibrary follows the declarative-programming-paradigm where possible:

You should say what you want to do, and not how to do it. This paradigm results in code which is easy to understand and can be written very quickly without loosing the overview of your code.

## Getting Started

### Usage

[![NuGet](https://img.shields.io/nuget/v/GRYLibrary.svg?color=green)](https://www.nuget.org/packages/GRYLibrary) ![Nuget](https://img.shields.io/nuget/dt/GRYLibrary.svg)

Install the GRYLibrary as NuGet-package using the Package Manager Console:

```bash
Install-Package GRYLibrary
```

## Reference

The GRYLibrary-reference can be found [here](https://aniondev.github.io/GRYLibraryReference).

## OWASP-Top-10-analysis

This section records the result of a security-analysis of this library against the
[OWASP Top 10:2025](https://owasp.org/Top10/2025/), which is the current official release of that list. Every one of the ten
categories is assessed explicitly.

The analysed version is 2.1.3. The analysis is a review of the source-code; it is not a penetration-test, so a statement which
could not be decided from the code is marked as an assumption instead of being claimed. The findings are stated from the point of
view of a consumer: this library is not an application but the framework which its consumers build their api-server on, so a weak
default here becomes a weakness in every product which uses it, and a security-control which the library does not offer is a
control which no consumer has.

Each finding additionally carries a "Fixable without breaking changes"-line. It states whether the finding can be remediated in a
way which is definitely not a breaking change for a consumer - that is: no behaviour-change when the library is used as intended,
no additional configuration required and no interface-change. The verdict is one of "Yes", "No" or "Partial" ("Partial" means
that one part of the recommended remediation is breaking-change-free while another part is not). It describes only the
breaking-change-risk, not the severity or the effort of the fix.

Each finding also carries a "State"-line which records how the finding is currently handled. Its value is one of "open" (not
yet addressed), "fixed" (remediated in the meantime) or "accepted" (the risk is knowingly accepted and will not be fixed). The
same state is repeated in the "Status"-column of the summary-table at the end of the section.

### Scope

The analysis concentrates on the parts which an api-server-consumer really runs, because those are the ones with an attack-
surface:

| Area | Files | Relevance |
| --- | --- | --- |
| Pipeline-composition and hosting | `APIServer/APIServer.cs`, `APIServer/Settings/Configuration/ServerConfiguration.cs` | decides which middleware runs in which order, and configures kestrel and tls |
| Authentication | `APIServer/MidT/Auth/AuthenticationMiddleware.cs`, `APIServer/Mid/AuthS/*` | decides for every request whether it is authenticated |
| Authorization | `APIServer/MidT/Aut/AuthorizationMiddleware.cs`, `APIServer/Mid/AutS/*` | decides whether the caller may perform the operation |
| Credential-transport | `APIServer/Services/CredH/*` | extracts the access-token from the request |
| Authentication-services | `APIServer/Services/Trans/TransientAuthenticationService.cs`, `APIServer/CommonDBTypes/User.cs`, `APIServer/CommonAuthenticationTypes/AccessToken.cs` | the reference-implementation of login, password-handling and token-handling |
| OpenID-Connect | `APIServer/Services/OIDC/*` | validates tokens of an external identity-provider |
| Request-logging | `APIServer/Mid/M05DLog/*`, `APIServer/Mid/General/GeneralMiddleware.cs` | decides what of every request is written to a log-file |
| Exception-handling | `APIServer/MidT/Exception/*`, `APIServer/Mid/Ex/*` | decides which status-code and which body a failed request gets |
| Threat-protection | `APIServer/MidT/RateLimit/*`, `APIServer/MidT/WAF/*`, `APIServer/MidT/Obfuscation/*`, `APIServer/MidT/Captcha/*` | the security-controls the library offers |
| Maintenance-routes | `APIServer/MaintenanceRoutes/*` | endpoints which every consumer exposes |
| Data-access | `APIServer/Services/Database/*` | builds and executes the sql of its consumers |
| Cryptography | `Crypto/*` | the primitives a consumer picks from |
| Logging | `Logging/GRYLogger/*` | the facility a consumer builds its audit-log on |

### A01:2025 Broken Access Control

**GRY-01 - Authentication is opt-in per endpoint, so an unannotated route is public** (High, confirmed)

- State: open

- Fixable without breaking changes: No. Requiring authentication by default turns every currently-public unannotated route into a 401 and forces consumers to add allow-anonymous entries (behaviour-change plus new configuration). Only the additive start-up log of the reachable routes would be non-breaking.

- Affected component: `APIServer/MidT/Auth/AuthenticationMiddleware.cs` (`AuthenticationIsRequired`)
- Attack-surface: every endpoint of every consumer
- Evidence: the method returns `true` only when the action carries an `AuthenticateAttribute` or an `AuthorizeAttribute`.
  An action which carries neither is treated as a route for which authentication is not required.
- Impact: forgetting one attribute on one action publishes that action to the internet, and nothing in the build or at start-up
  points that out. The failure-mode of the central access-control-decision is therefore "open", and a consumer can only notice
  it by testing every single route.
- Recommendation: require authentication by default and make the exception explicit (an `AllowAnonymousAttribute` plus the
  route-allowlist), and log at start-up which routes are reachable without authentication so that the result is visible.

**GRY-02 - The allowlist of unauthenticated routes is matched with unanchored regular expressions** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Compiling each pattern once and null-checking `Path.Value` are behaviour-preserving. Anchoring, rejecting unanchored patterns at start-up and switching to case-insensitive matching change which routes match and would break existing consumer-patterns, so they are not breaking-change-free.

- Affected component: `APIServer/MidT/Auth/AuthenticationMiddleware.cs` (`AuthenticationIsRequired`), `APIServer/Mid/M05DLog/DRequestLoggingMiddleware.cs` (`IsIgnored`)
- Attack-surface: every endpoint of every consumer
- Evidence: the patterns of `RoutesWhereUnauthenticatedAccessIsAllowed` are applied with `new Regex(pattern).IsMatch(path)`.
  Nothing requires a pattern to be anchored, so a pattern without `$` also matches every route which merely starts with it. The
  comparison is case-sensitive while the routing of asp.net is not, a new `Regex` is compiled for every pattern on every request,
  and `context.Request.Path.Value` is passed without a null-check.
- Impact: a pattern which was meant for one route silently covers routes which are added later. A real example from a consumer of
  this library is the pattern `^/API/Other/Resources/APISpecification/*`, which because of the missing `$` also matches every
  route whose path starts with that text.
- Recommendation: anchor every pattern (or match against the route-template of the resolved endpoint instead of against the raw
  path), compare case-insensitively, compile the patterns once, and reject a pattern which is not anchored at both ends.

**GRY-03 - Authorization re-reads the token from the request and does not re-validate it** (High, confirmed)

- State: open

- Fixable without breaking changes: Partial. Authorizing from the already-established principal and dropping the duplicate lookup can be behaviour-neutral on the normal path, but adding token-re-validation and rejecting the allowlist-plus-authorize combination at start-up change the behaviour (or abort the start-up) for a consumer who uses that combination.

- Affected component: `APIServer/Mid/AutS/AutSRMiddleware.cs`, `APIServer/Mid/AutS/AutSAMiddleware.cs` (`IsAuthorized`)
- Attack-surface: every route which carries an `AuthorizeAttribute`
- Evidence: `IsAuthorized` does not use the `ClaimsPrincipal` which the authentication-middleware put into the context. It takes
  the token out of the request again and calls `GetUserByAccessToken`, without calling `AccessTokenIsValid`. Whether the token is
  expired is therefore never checked here.
- Impact: as long as the authentication-middleware ran before it and rejected the request, the effect is only a duplicated
  database-lookup. But a route which is listed in `RoutesWhereUnauthenticatedAccessIsAllowed` and at the same time carries an
  `AuthorizeAttribute` passes the authentication-middleware untouched (see GRY-01), and the authorization then accepts an expired
  token, because the token only has to exist. The library allows that combination and does not warn about it.
- Recommendation: authorize from the principal which the authentication-middleware established, refuse to authorize a request
  which was not authenticated, and reject the combination of an allowlisted route with an authorize-attribute at start-up.

**GRY-04 - An authorize-attribute without groups disables the authorization-check** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. The fix (an empty group-set must fail at start-up or deny) breaks any consumer which currently ships such an attribute (start-up-failure or newly-denied requests).

- Affected component: `APIServer/MidT/Aut/AuthorizationMiddleware.cs` (`AuthorizationIsRequired`)
- Attack-surface: every route which carries an `AuthorizeAttribute` without a group
- Evidence: `AuthorizationIsRequired` returns `authorizeAttribute.Groups.Any()`, so an empty group-set means that no
  authorization is required at all.
- Impact: an annotation which looks like a protection is none. The route still requires authentication, so the effective
  permission is "every authenticated user", which is regularly not what the author of the annotation intended.
- Recommendation: treat an authorize-attribute without a group as a configuration-error and fail at start-up, so that the
  intention has to be written down explicitly.

**GRY-05 - The maintenance-routes can not be protected** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. An opt-in per-endpoint auth-control can be added with the default left at today's behaviour (that part is non-breaking), but actually closing the finding needs new configuration and a changed default.

- Affected component: `APIServer/MaintenanceRoutes/MaintenanceRoutesController.cs`
- Attack-surface: internet-exposed in every consumer which enables one of the endpoints
- Evidence: none of the actions carries an `AuthenticateAttribute` or an `AuthorizeAttribute`, so by GRY-01 they are always
  reachable without authentication. The only control is one boolean per endpoint. `ShowAllEndpoints` returns every route of the
  application together with the controller-type and the method-name, `CurrentVersion` returns the exact version and `Metrics`
  returns the complete prometheus-registry.
- Impact: a consumer can only choose between "public" and "off". The three endpoints named above are exactly the information an
  attacker collects first: the complete route-list, the version for matching known vulnerabilities, and operational metrics.
- Recommendation: let the consumer decide per endpoint whether it requires authentication (and which role), and keep the default
  for everything except a plain availability-check at "off".

### A02:2025 Security Misconfiguration

**GRY-06 - The library sets no security-response-headers** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. A header-middleware can only be added non-breaking if it is off by default; enabling the headers (`Content-Security-Policy`, …) or deriving the hsts-decision from the public protocol needs new configuration and can change client-visible behaviour.

- Affected component: `APIServer/APIServer.cs`
- Attack-surface: internet-exposed in every consumer
- Evidence: the pipeline contains no middleware which sets `Content-Security-Policy`, `X-Content-Type-Options`,
  `Referrer-Policy`, `Permissions-Policy` or `Cross-Origin-Opener-Policy`, and the library offers none. `UseHsts` is called only
  when `ServerConfiguration.Protocol` is `HTTPS`.
- Impact: every consumer has to solve this itself in its reverse-proxy, and a consumer which runs the api behind a
  tls-terminating proxy (which is the documented deployment of the products which use this library) gets no
  `Strict-Transport-Security` at all, because the application itself then speaks plain http.
- Recommendation: offer a configurable security-header-middleware with a safe default-set, and derive the hsts-decision from the
  publicly reachable protocol instead of from the protocol of the internal listener.

**GRY-07 - A certificate-problem in a productive environment is only a warning** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. Making a certificate-problem an error by default stops deployments which start today; it can only be added as a configurable option with the default left at "warning", which does not close the finding.

- Affected component: `APIServer/APIServer.cs` (kestrel-configuration)
- Attack-surface: internet-exposed, when the application terminates tls itself
- Evidence: a self-signed certificate in a `Productive`-environment and a certificate whose dns-name differs from the configured
  domain are both written to the log with `LogLevel.Warning`, and the server starts and serves regardless.
- Impact: a deployment-error which breaks the authenticity of the tls-connection does not stop the deployment and is only
  visible to somebody who reads the start-up-log.
- Recommendation: let the consumer decide whether such a finding is a warning or an error, and make "error" the default for a
  productive environment.

**GRY-08 - Synchronous io is enabled for the whole server** (Low on its own, confirmed)

- State: open

- Fixable without breaking changes: No. Removing `AllowSynchronousIO` can throw for any consumer-code which performs synchronous stream-io; it is only safe once GRY-16 is done and no consumer relies on synchronous io.

- Affected component: `APIServer/APIServer.cs` (`kestrelOptions.AllowSynchronousIO = true`)
- Attack-surface: internet-exposed in every consumer
- Evidence: the flag is set unconditionally, which asp.net disables by default for a reason.
- Impact: on its own it only permits synchronous reads and writes on the request- and response-stream. Its real weight comes from
  the fact that the pipeline makes use of it everywhere, which is GRY-16.
- Recommendation: remove the flag together with the blocking calls described in GRY-16.

### A03:2025 Software Supply Chain Failures

The positive part first: every nuget-dependency is pinned to an exact version with the bracket-notation, and the project keeps a
lock-file (`RestorePackagesWithLockFile`), so a restore resolves reproducibly.

~~**GRY-09 - A deprecated data-access-package is referenced**~~ (Medium, confirmed)

- State: fixed (the unused `System.Data.SqlClient`-reference was removed)

- Fixable without breaking changes: Yes. The package is referenced but never used: `SQLServerDatabaseInteractor` throws in every method and imports only `System.Data.Common`, and no public type exposes a `SqlClient`-type. Dropping the reference removes an unused dependency with no consumer-impact. (Implementing sql-server on `Microsoft.Data.SqlClient` is separate, additive work.)

- Affected component: `GRYLibrary/GRYLibrary/GRYLibrary.csproj` (`System.Data.SqlClient` 4.9.1)
- Attack-surface: build and every consumer, because a library-dependency is transitive
- Evidence: `System.Data.SqlClient` is referenced. That package is the legacy sql-server-client which is superseded by
  `Microsoft.Data.SqlClient`; it receives no feature-work and its advisories are fixed only in the successor.
- Impact: every consumer of this library inherits the package, including a consumer which does not use sql-server at all.
- Recommendation: migrate `SQLServerDatabaseInteractor` to `Microsoft.Data.SqlClient` and drop the old package.

~~**GRY-10 - A vulnerability in a dependency does not break the build**~~ (Low, confirmed)

- State: fixed (`NuGetAuditMode` is set to `all` and `NU1901`-`NU1904` are promoted to errors; the build currently reports no vulnerable dependency)

- Fixable without breaking changes: Yes. The `NuGetAudit`-settings affect only GRYLibrary's own build-pipeline, not any consumer.

- Affected component: `GRYLibrary/GRYLibrary/GRYLibrary.csproj`
- Attack-surface: build-pipeline
- Evidence: `WarningsAsErrors` contains only `NU1605`, and no `NuGetAudit`-settings are made, so an audit-warning about a known
  vulnerable package stays a warning.
- Impact: a known vulnerable dependency can be released without anybody having to acknowledge it, and because this is a library
  it is then handed on to every consumer.
- Recommendation: set `NuGetAuditMode` to `all` and promote the audit-warnings `NU1901` to `NU1904` to errors.

### A04:2025 Cryptographic Failures

**GRY-11 - The reference authentication-service hashes passwords with an unsalted single-round sha-256** (Critical, confirmed)

- State: open

- Fixable without breaking changes: No. A salted key-derivation-function can not keep the current `Hash(password) == storedHash` equality-contract, so it needs a verify-method (a changed or relocated api); `Login` compares with `!=` today. The in-memory, test-only nature of this service removes the stored-hash-migration-problem, but the public `Hash`-contract still changes. (See GRY-12 for the additive primitive.)

- Affected component: `APIServer/Services/Trans/TransientAuthenticationService.cs` (`Hash`)
- Attack-surface: every stored password of every consumer which uses or copies this service
- Evidence: `Hash` returns the hex-representation of one `SHA256`-pass over the password, without a salt and without
  key-stretching, and `Login` compares the result with `!=`.
- Impact: sha-256 is built to be fast, so a leaked user-table can be attacked with rainbow-tables and with billions of guesses
  per second; without a salt two users with the same password are visibly identical and one cracked hash opens every account
  which shares it. The weight of the finding is that this is the reference-implementation of the framework: a consumer which
  needs a persistent authentication-service writes its own by following this one, and then has the same defect. At least one
  product which uses this library does exactly that.
- Recommendation: hash with a memory-hard algorithm and a per-user salt (argon2id, or pbkdf2-hmac-sha256 with a high
  iteration-count as the variant which the platform brings with it), store the algorithm and its parameters next to the hash so
  that they can be raised later, compare in constant time, and re-hash transparently on the next successful login. Because this
  is the pattern every consumer copies, it is worth offering the whole password-verification as a service of the library instead
  of only the primitive.

~~**GRY-12 - The library offers no usable password-hashing-primitive**~~ (High, confirmed)

- State: fixed (a dependency-free `PasswordHasher` on PBKDF2-HMAC-SHA256 with a per-password salt, a stored iteration-count and a constant-time verify was added in `Crypto/PasswordHasher.cs`; the `Argon2`-stub was kept as decided)

- Fixable without breaking changes: Yes. `Argon2.Hash` currently throws `NotImplementedException`, so no working caller can exist; implementing it and adding a new `PasswordHasher` are additive. (Removing the class would be breaking, so take the implement-path.)

- Affected component: `Crypto/Argon2.cs`, `Crypto/GRYBCryptoSystem.cs`
- Attack-surface: every consumer which looks for the right primitive
- Evidence: `Argon2.Hash` throws a `NotImplementedException` and only `GetIdentifier` is implemented; `GRYBCryptoSystem` contains
  a `NotImplementedException` as well. No pbkdf2-, bcrypt- or scrypt-implementation and no key-derivation-helper exists in
  `Crypto`. The only working hash-primitives are `SHA256` and `SHA256PureCSharp`, which are the wrong tool for a password.
- Impact: a consumer which deliberately searches the library for the correct primitive finds a class with the right name which
  does not work, and then falls back to `SHA256` - which is how GRY-11 propagates.
- Recommendation: implement `Argon2` or remove the class, and in either case offer an explicit `PasswordHasher` with a
  verify-method, so that the correct way is the shortest one.

**GRY-13 - Access-tokens are guids and are stored in plaintext as the key of the token-table** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Generating the token via `RandomNumberGenerator` (still an opaque string) and making the lifetime configurable with the default left at one day are behaviour-compatible. Storing only the token-hash changes the `AccessToken`-primary-key/schema and breaks persistent consumer-stores.

- Affected component: `APIServer/CommonAuthenticationTypes/AccessToken.cs`, `APIServer/Services/Trans/TransientAuthenticationService.cs`
- Attack-surface: the token-store of every consumer
- Evidence: a token is `Guid.NewGuid().ToString()`, the type carries `[PrimaryKey(nameof(Value))]`, so the token-value itself is
  the primary key and is stored as it is. The lifetime is hard-coded to one day with a `//TODO make this configurable`, and
  `IsValid` only compares the expiry-moment.
- Impact: a version-4-guid carries 122 random bits from a cryptographically strong source on the supported platforms, so guessing
  is not practical; the finding is that the security-property depends on a platform-detail instead of on an explicit decision,
  and that read-access to the token-table (a backup, a log of a query, a sql-injection in a consumer) yields directly usable
  tokens.
- Recommendation: generate the token explicitly from `RandomNumberGenerator`, store only its hash and look it up by that hash,
  and make the lifetime configurable.

**GRY-14 - The oidc-authority is not required to use https** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. Requiring https rejects consumers which currently use an http-authority; restoring that needs a new development-opt-in (behaviour-change plus new configuration).

- Affected component: `APIServer/Services/OIDC/OIDCService.cs` (`FetchDiscoveryAsync`, `FetchJwksAsync`)
- Attack-surface: outgoing connection to the identity-provider
- Evidence: the discovery-url is built as `provider.Authority + "/.well-known/openid-configuration"` without checking the scheme,
  and the `jwks_uri` taken from the discovery-document is fetched as it is.
- Impact: with a plaintext authority (or a discovery-document which names a plaintext `jwks_uri`) an attacker on the network-path
  can replace the signing-keys and thereby mint tokens which this library accepts as valid. That is a full
  authentication-bypass, reachable through a misconfiguration which the library does not prevent although the specification
  requires https for an issuer.
- Recommendation: require https for the authority and for every endpoint taken from the discovery-document, and allow an
  exception only through an explicit opt-in which is meant for a development-environment.

### A05:2025 Injection

No sql-injection was found in the data-access-layer: a command is built from a sql-text plus real `DbParameter`-objects which the
concrete interactor creates (`GetParameter`), and the queries which the library executes itself against `information_schema` are
constant texts with bound parameters.

~~**GRY-15 - The migration-statements are assembled by string-interpolation**~~ (Low, confirmed)

- State: fixed (the migration-table-name and the migration-name are validated against a strict pattern before interpolation; binding the name as a parameter was deliberately not used, because a bound parameter in the same command as the arbitrary migration-DDL would break migrations which legitimately use `@`, which would itself be a breaking change)

- Fixable without breaking changes: Yes. Binding the migration-name as a parameter and validating the table-name is internal to the migration-execution; the values are build-time constants which already satisfy a strict identifier, so valid migrations behave identically and no consumer-api changes.

- Affected component: `APIServer/Services/Database/PostgreSQLDatabaseInteractor.cs`, `.../MariaDBDatabaseInteractor.cs`, `.../OracleDatabaseInteractor.cs`, `.../SQLServerDatabaseInteractor.cs` (`GetSQLStatementForRunningMigration`, `CreateSQLStatementForCreatingMigrationMaintenanceTableIfNotExist`, `GetSQLStatementForSelectMigrationMaintenanceTableContent`)
- Attack-surface: build-time content (the migration-resources of a consumer), not a request
- Evidence: the migration-name is interpolated into a quoted sql-literal and the table-name into a quoted identifier.
- Impact: none today, because both values come from the embedded resources of the consumer and not from a request. It is a
  hardening-finding: the pattern invites passing a value which is not developer-controlled, and a migration-name which contains
  a quote breaks the statement.
- Recommendation: bind the migration-name as a parameter and validate the table-name against a strict identifier-pattern.

### A06:2025 Insecure Design

**GRY-16 - The pipeline is synchronous-over-asynchronous** (High, confirmed)

- State: open

- Fixable without breaking changes: Partial. The overridable method is already `Task Invoke(HttpContext)`, so converting the library's own middlewares from `.Wait()` to `await` keeps the signature and the happy-path-result identical. What a custom `HandleException` receives changes from an `AggregateException` to the original exception, so it is not guaranteed behaviour-identical for a consumer's exception-handler.

- Affected component: `APIServer/MidT/Exception/ExceptionManagerMiddleware.cs`, `APIServer/Mid/Ex/DefaultExceptionHandlerMiddleware.cs`, `APIServer/Mid/AuthS/AuthSMiddleware.cs`, `APIServer/MaintenanceRoutes/MaintenanceRoutesController.cs`
- Attack-surface: internet-exposed in every consumer
- Evidence: the exception-middleware - which wraps the entire rest of the pipeline - executes it with
  `this._Next(context).Wait()` and then returns `Task.CompletedTask`; the response is written with `WriteAsync(...).Wait()`; the
  oidc-token-validation is executed with `GetAwaiter().GetResult()`; the maintenance-routes use `.Wait()` and
  `WaitAndGetResult()`. `AllowSynchronousIO` (GRY-08) is what makes this work at all.
- Impact: every request in flight occupies one thread-pool-thread for its whole duration instead of releasing it while it waits
  for the database or for an external service. The server therefore stops answering at the point where the thread-pool is
  exhausted, which a moderate number of slow requests reaches - and a request which waits for an external service is exactly the
  normal case here. An attacker does not need a flood for that, only a slow dependency. It also turns every exception into an
  `AggregateException`, which the exception-middleware then has to unwrap again.
- Recommendation: make the middlewares `async` and await the next one, remove `AllowSynchronousIO`, and await the outgoing
  http-calls instead of blocking on them.

**GRY-17 - Rate-limiting can not be switched on** (High, confirmed)

- State: open

- Fixable without breaking changes: Yes. Adding a concrete rate-limiting-middleware and an `ISupportRateLimitingMiddleware`-hook is purely additive; existing consumers have no rate-limit today and stay unaffected, new consumers can opt in, and that already closes "can not be switched on". (A failed-login-counter or lockout which is active by default would change login-behaviour, so keep it opt-in.)

- Affected component: `APIServer/MidT/RateLimit/RateLimitingMiddleware.cs`, `APIServer/APIServer.cs`
- Attack-surface: every consumer, especially its login-route
- Evidence: `RateLimitingMiddleware` is an `abstract class`, there is no concrete implementation in the library, there is no
  `ISupportRateLimitingMiddleware`-interface and `APIServer.cs` never adds it to the pipeline. The same holds for
  `WebApplicationFirewallMiddleware` and `ObfuscationMiddleware`, which are abstract as well; of the four controls in the region
  "General Threat-Protection" only the captcha has a concrete implementation (`Mid/M04CC/CCaptchaMiddleware`). There is also no
  counter of failed logins and no lockout-mechanism anywhere in the library.
- Impact: no consumer of this library has a rate-limit, an account-lockout or a web-application-firewall unless it writes the
  implementation itself, and the standard bootstrap gives it no place to plug a rate-limiter in. Password-guessing against a
  product built on this library is therefore unlimited by default. The counting-logic itself is already there and is sound apart
  from the points below, so what is missing is a concrete class and three lines in the pipeline.
- Recommendation: add a concrete rate-limiting-middleware and an `ISupportRateLimitingMiddleware`-hook, add it to the pipeline
  before the authentication-middleware, count failed logins per account in the authentication-service, and take the client-
  identity of an anonymous request from the forwarded-header-handling (`GeneralMiddleware`) instead of from
  `Connection.RemoteIpAddress`, which behind a reverse-proxy is the proxy and therefore puts every anonymous client into one
  bucket.

**GRY-18 - Multi-factor-authentication is a model-only stub** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. Making `IsActicated` enforce a second factor changes the login-flow and needs the caller to supply a code; removing the model is an api-removal. Neither is breaking-change-free. (Generating the totp-secret from a random byte-sequence for newly-created users is an internal improvement, but alone does not close the finding.)

- Affected component: `APIServer/MFA/TOTP.cs`, `APIServer/MFA/IMFAMethod.cs`, `APIServer/CommonDBTypes/User.cs`
- Attack-surface: every consumer which believes the flag has an effect
- Evidence: `TOTP` holds a `SecretKey` and an `IsActicated`-flag and nothing else; no code anywhere in the library verifies a
  one-time-code, and the login-path does not look at the flag. `User.CreateNewUser` fills the secret with
  `Guid.NewGuid().ToString("N")`.
- Impact: a consumer which sets `IsActicated` gets no second factor, which is worse than having no mfa-model at all, because the
  database then states a protection which does not exist. The secret is additionally derived from a guid instead of from a
  random byte-sequence of the length the totp-standard expects.
- Recommendation: either implement the verification (including the secret-generation, the time-window and the replay-protection)
  or remove the model, so that no consumer can configure a control which does nothing.

**GRY-19 - The deactivation-flag of a user has no effect and the lock is checked too late** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Honouring `UserIsActivated` only affects a consumer which deliberately sets it to false (its documented purpose), but reordering the lock-check and unifying the locked-account-message/status changes client-visible responses.

- Affected component: `APIServer/Services/Trans/TransientAuthenticationService.cs` (`Login`), `APIServer/CommonDBTypes/User.cs`
- Attack-surface: every consumer
- Evidence: `User.UserIsActivated` exists and defaults to `true`, but no code in the library ever reads it in a decision. `Login`
  checks `UserIsLocked` only after the password-comparison succeeded, and answers a locked account with a message which names
  the user.
- Impact: deactivating a user does not prevent the login, so an administrative measure which looks effective is not. The order
  of the lock-check additionally tells a caller who guessed a password correctly that the account exists and is locked.
- Recommendation: evaluate `UserIsActivated` in the login, check the lock before the password-comparison, and answer both cases
  with the same message as an invalid credential.

**GRY-20 - The first group of consumer-middlewares runs before authentication** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Clarifying the documentation of the property is fully safe; renaming the middleware-group-property is an api-change and is therefore breaking.

- Affected component: `APIServer/APIServer.cs`
- Attack-surface: every consumer which registers a custom middleware
- Evidence: `CustomMiddlewares1` is added to `businessMiddlewares1`, which is registered before `specialMiddlewares2` - the group
  which contains the authentication- and the authorization-middleware.
- Impact: a consumer-middleware in that group sees an unauthenticated context, with no user and no token in `context.Items`. A
  middleware which assumes otherwise either fails or, worse, performs its work for an unauthenticated caller. The two groups are
  distinguished only by their name, so the distinction is easy to get wrong.
- Recommendation: name the two groups after what distinguishes them (before and after authentication) and state it in the
  documentation of the property.

**GRY-21 - The oidc-discovery-document and the signing-keys are fetched per validation** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Caching the discovery-document and the key-set per provider (with a refresh on an unknown key-id) and giving the default `HttpClient` a timeout are transparent for correct usage. Injecting the `HttpClient` through a new constructor-parameter is an interface-change, and selecting the provider by the issuer-claim changes internal behaviour.

- Affected component: `APIServer/Services/OIDC/OIDCService.cs`, `APIServer/Mid/AuthS/AuthSMiddleware.cs`
- Attack-surface: internet-exposed, every request which carries an oidc-token
- Evidence: `ValidateAccessTokenAsync` calls `FetchDiscoveryAsync` and `FetchJwksAsync` on every call; nothing is cached.
  `AuthSMiddleware.TryGetOIDCAuthentication` loops over every configured provider and validates against each until one accepts,
  blocking on each call. The default constructor of `OIDCService` creates its own `HttpClient` without a timeout.
- Impact: one incoming request causes up to two outbound requests per configured provider, synchronously (see GRY-16). The
  identity-provider thereby becomes a dependency on the hot path of every request, its rate-limit becomes reachable by normal
  traffic, and a slow provider blocks threads of the consumer.
- Recommendation: cache the discovery-document and the key-set per provider with a sensible lifetime and refresh them on an
  unknown key-id, select the provider from the issuer-claim of the token instead of trying all of them, and inject a configured
  `HttpClient` with a timeout.

**GRY-22 - The resource-owner-password-credentials-grant is offered** (Low, confirmed)

- State: open

- Fixable without breaking changes: Yes. Marking `LoginWithPasswordAsync` with `[Obsolete]` emits a compiler-warning only; the method keeps working, with no behaviour- or interface-change. (A consumer who treats warnings as errors would see a build-warning, but that is its own build-configuration.)

- Affected component: `APIServer/Services/OIDC/OIDCService.cs` (`LoginWithPasswordAsync`)
- Attack-surface: a consumer which uses that method
- Evidence: the method implements `grant_type=password` and forwards the user-name and the password of the user to the
  token-endpoint.
- Impact: the grant is removed from current oauth-guidance, because it makes the application handle the credentials of the
  identity-provider and rules out every protection which happens at the provider (second factor, risk-based checks, consent).
  Offering it in a library makes it a plausible choice for a consumer.
- Recommendation: mark the method as obsolete with a reference to the authorization-code-flow with pkce, which the same class
  already implements correctly.

### A07:2025 Authentication Failures

**GRY-23 - The audience of an oidc-access-token is validated only when it happens to be configured** (High, confirmed)

- State: open

- Fixable without breaking changes: No. Requiring the audience rejects tokens for a consumer which did not configure one (behaviour-change plus new mandatory configuration).

- Affected component: `APIServer/Services/OIDC/OIDCService.cs` (`ValidateJwtAndParseClaimsAsync`, `ValidateAccessTokenAsync`)
- Attack-surface: internet-exposed, every request which carries an oidc-token
- Evidence: `bool validateAudience = !string.IsNullOrWhiteSpace(validAudience);` and
  `ValidateAudience = validateAudience`. `ValidateAccessTokenAsync` passes `provider.Audience`, for which no default is set.
  When a consumer does not fill it - which the consumer this analysis started from does not - the audience is not validated at
  all, while the issuer, the signature and the lifetime are.
- Impact: any token which the same identity-provider issued for any other client is then accepted as a valid access-token of
  this application. In an organisation with one central provider and several applications that is a realistic path: a token
  which a user legitimately holds for an unrelated application authenticates them here. The id-token-path is not affected,
  because it passes the client-id as the audience.
- Recommendation: require the audience for access-token-validation and refuse to validate without it, instead of silently
  switching the check off.

**GRY-24 - The oidc-principal carries only the subject, so subjects of different providers collapse** (High, confirmed)

- State: open

- Fixable without breaking changes: Partial. Adding the issuer as an additional claim is additive and non-breaking (the existing `NameIdentifier` stays). Changing `NameIdentifier` itself or switching the provider-selection to issuer-based changes behaviour.

- Affected component: `APIServer/Mid/AuthS/AuthSMiddleware.cs` (`TryGetOIDCAuthentication`)
- Attack-surface: internet-exposed, a consumer with more than one configured provider
- Evidence: the method accepts the first provider for which the token validates and builds a `ClaimsPrincipal` whose
  `NameIdentifier` is only `result.Subject`. The issuer is not part of the principal, and the exception of a provider which
  rejects the token is swallowed.
- Impact: the subject of an identity-provider is unique only within that provider. Two providers can issue the same subject, and
  the identity which the application then works with is the same for both. A consumer which maps a user by
  (provider, subject) - which is the correct mapping and the one the examined consumer implements - can not do so from this
  principal at all.
- Recommendation: put the issuer into the principal as its own claim, select the provider by the issuer-claim of the token
  instead of by trying all of them, and build the user-identity from both values.

**GRY-25 - The oidc-bearer-path ends in an exception instead of in an authenticated request** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Yes (for the discarded-lookup). Removing the unconditional `GetUserByAccessToken` whose result is discarded removes an unnecessary query and the spurious exception on the oidc-path; on the regular path the result was thrown away, so correct usage is unchanged. (Mapping the external subject to a local user is a separate feature.)

- Affected component: `APIServer/MidT/Auth/AuthenticationMiddleware.cs` (`IsAuthenticatedInternal`), `APIServer/Utilities/Tools.cs` (`GetUser`)
- Attack-surface: a consumer which enables oidc-token-authentication
- Evidence: after `TryGetAuthentication` succeeded, `IsAuthenticatedInternal` unconditionally calls
  `this._AuthenticationService.GetUserByAccessToken(accessToken)` and discards the result. For a token which was accepted by the
  oidc-path that token is not an access-token of the application, so the lookup can not find it. `Tools.GetUser`, which the
  controllers of a consumer use, resolves the local user by the `NameIdentifier`-claim, which on that path is the subject of the
  provider and therefore never a local user-id.
- Impact: the oidc-bearer-authentication which `AuthSMiddleware` advertises does not lead to a usable request. The caller gets an
  internal error instead of either an authenticated request or a clean 401, and the discarded lookup is an unnecessary
  database-query on every authenticated request of the regular path as well.
- Recommendation: remove the unused lookup, and map an externally authenticated subject to a local user before the request
  continues (or state explicitly that the oidc-path only works together with a consumer-side mapping).

**GRY-26 - The contract of the credential-header is inconsistent** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. The path which works today is "send the raw token", so any format-/parsing-alignment or rejecting a duplicated header changes observable behaviour for existing clients.

- Affected component: `APIServer/Services/CredH/HeaderService.cs`, `APIServer/Services/CredH/HeaderTools.cs`, `APIServer/Mid/AuthS/AuthSFilter.cs`
- Attack-surface: internet-exposed in every consumer
- Evidence: `HeaderTools.GetAccessTokenHeader` produces the value `User=<name>;AccessToken=<token>` for the header
  `X-AccessToken`, while `HeaderService.ExtractSecret` returns the whole header-value as the token; nothing parses that format.
  `HeaderService.TryGetHeaderValue` converts a `StringValues` with several entries implicitly to a string, which joins them with
  a comma, whereas `AuthSFilter.TryGetAcessToken` accepts the header only when it has exactly one value.
- Impact: a client which builds its header the way the library offers it can not authenticate, and two components of the same
  library disagree about what a duplicated credential-header means. A disagreement about how a credential is parsed is the kind
  of thing a proxy-chain can be used against, and the generated api-specification describes the one format while the server
  accepts the other.
- Recommendation: define one format, parse it in one place, reject a request which carries the credential-header more than once,
  and cover the round-trip with a test.

**GRY-27 - The oidc-code-flow sends no nonce** (Low, confirmed)

- State: open

- Fixable without breaking changes: Yes. Adding a `nonce` to the authorization-request and validating it against the id-token is a specification-compliant, additive change to a flow the library manages internally; the provider echoes the nonce and the state is kept library-side next to the verifier, so correct usage is unchanged.

- Affected component: `APIServer/Services/OIDC/OIDCService.cs` (`InitiateLoginAsync`)
- Attack-surface: the login-flow
- Evidence: the authorization-url contains `state`, `code_challenge` and `code_challenge_method=S256`, but no `nonce`, and the
  validation of the id-token does not check one.
- Impact: low, because pkce with s256 already covers the code-injection the nonce is meant to prevent, and the state is
  generated from a cryptographically strong source. It remains a deviation from what the specification asks for.
- Recommendation: send a nonce, keep it next to the code-verifier and compare it against the claim of the id-token.

Beyond the individual findings: the library has no password-policy, no password-change- or reset-flow, no failed-login-counter
and no re-authentication for a sensitive operation. Every consumer therefore has to build those itself, which is why the products
built on it regularly do not have them.

### A08:2025 Software or Data Integrity Failures

**GRY-28 - The logging-facility offers no integrity-protection** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Yes. A hash-chaining/append-only log-target is a new, additive option; the existing log-targets stay untouched.

- Affected component: `Logging/GRYLogger/*`
- Attack-surface: internal, whoever reaches the log-files
- Evidence: a log-target writes plain text into a file (or to the console). There is no append-only-mode, no signature, no
  hash-chaining of the entries and no built-in transfer to an external sink. This is the facility on which consumers build their
  audit-log, including products which have to keep one for regulatory reasons.
- Impact: an audit-log built on it can be changed afterwards by anybody with write-access to the file, so its evidential value
  is limited to the case in which the host itself stayed intact. The library does not offer an alternative for the case in which
  the log itself has to be trustworthy.
- Recommendation: offer a log-target which chains every entry with the hash of its predecessor and which can write to an
  append-only or external destination, so that a consumer which needs a tamper-evident log does not have to build it from
  scratch.

**GRY-29 - The assemblies carry no valid strong-name-signature** (Low, confirmed)

- State: open

- Fixable without breaking changes: Yes. Completing the signature keeps the same public key (hence the same strong-name-identity/public-key-token), so consumer-binding is unchanged; it only makes the existing signature verify. Publishing a bill-of-materials is additive. (It needs the signing-key; dropping the strong name instead would change the identity and be breaking.)

- Affected component: `GRYLibrary/GRYLibrary/GRYLibrary.csproj`
- Attack-surface: distribution of the nuget-package
- Evidence: `SignAssembly` is `true` while `DelaySign` is `true` as well, so only the public key is embedded and the signature is
  not completed.
- Impact: the strong name does not verify, so it gives no integrity-guarantee for the delivered assembly. A consumer which checks
  it gains nothing.
- Recommendation: complete the signing in the release-pipeline or stop declaring it, and publish a bill-of-materials next to the
  package.

### A09:2025 Security Logging and Alerting Failures

The good part first: request-headers are logged only when a consumer lists them explicitly in `LoggedHTTPRequeustHeader`, which
is empty by default. The credential-header and a `password`-header are therefore not written to the log by default.

**GRY-30 - The complete request- and response-body is written to the log-file for every request** (High, confirmed)

- State: open

- Fixable without breaking changes: Partial. Adding a redaction-configuration and excluding the authentication-routes' bodies/tokens is a security-fix no correct consumer depends on, but making body-logging opt-in removes log-content which a consumer may rely on, so it changes the logging-behaviour.

- Affected component: `APIServer/Mid/M05DLog/DRequestLoggingMiddleware.cs` (`ShouldLogEntireRequestContentInLogFile`, `FormatLogEntryFull`)
- Attack-surface: internal, the log-files and everything they are forwarded to
- Evidence: `ShouldLogEntireRequestContentInLogFile` returns `true` in every case (its body reads
  `if (request.ResponseStatusCode / 100 == 5) { return true; } return true;`), so the file-target always gets the full entry.
  Request- and response-body are written truncated to `MaximalLengthofRequestBodies` respectively
  `MaximalLengthOfResponseBodies` (both 4000 characters by default) with no redaction of any kind; only the console-target gets
  the short summary.
- Impact: the response of a login contains the issued access-token, and it is far shorter than the truncation-limit, so the
  token of every login is written to `Requests.log` in plaintext. Read-access to the log is therefore equivalent to
  account-takeover. The same applies to every body which carries personal or confidential data, for example an uploaded document
  in a document-management-system.
- Recommendation: offer a redaction-configuration (routes whose body is not logged at all, and json-paths which are replaced),
  exclude the authentication-routes from body-logging by default, and make the body-logging itself opt-in instead of always-on.

**GRY-31 - Verbose mode ignores the list of not-logged routes** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Providing a separate "log really everything"-switch is additive; keeping the exclusion-list effective in verbose mode changes what verbose mode logs today, so it is a behaviour-change.

- Affected component: `APIServer/Mid/M05DLog/DRequestLoggingMiddleware.cs` (`ShouldBeLogged`)
- Attack-surface: internal, the log-files
- Evidence: `if (this._CommandlineParameter.EnforceVerbose) { return true; }` stands before the check of `NotLoggedRoutes`, so
  verbose mode logs exactly the routes which a consumer excluded.
- Impact: a consumer excludes a route because its content must not be logged. Switching on verbose output to analyse a problem
  then logs precisely that content, which is the opposite of what the exclusion was for.
- Recommendation: keep the exclusion-list effective in verbose mode, and provide a separate switch for a consumer which really
  wants everything.

**GRY-32 - The client decides the request-id which appears in every log-line** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Partial. Escaping and length-limiting the value (which prevents the log-forging through line-breaks) is behaviour-preserving for a well-formed id; ignoring the client-supplied `X-RequestId` or only trusting it from a proxy changes the correlation-behaviour for a consumer which relies on it.

- Affected component: `APIServer/Mid/General/GeneralMiddleware.cs`
- Attack-surface: internet-exposed in every consumer
- Evidence: when the request carries an `X-RequestId`-header, its value is adopted unchanged as the request-id and is written
  into the request-log and into the authorization- and exception-log-entries. The value is neither validated nor length-limited
  nor escaped.
- Impact: a caller can choose the correlation-id of their own requests, which makes it possible to make requests look like
  somebody else's, and a value which contains line-breaks forges additional lines in a plain-text log. That is exactly the kind
  of record an investigation afterwards relies on.
- Recommendation: either generate the id always, or accept a supplied one only from a trusted proxy and only when it matches a
  strict pattern, and escape the value before writing it.

**GRY-33 - The authentication-middleware swallows every exception without logging it** (Medium, confirmed)

- State: open

- Fixable without breaking changes: Yes (for the logging). Logging the caught exception is additive and keeps the fail-closed outcome for correct usage. (Distinguishing "credentials are wrong" from "the check could not be performed" and answering with a service-unavailable would change responses, so keep that separate.)

- Affected component: `APIServer/Mid/AuthS/AuthSMiddleware.cs` (`TryGetAuthentication`, `TryGetOIDCAuthentication`)
- Attack-surface: internet-exposed in every consumer
- Evidence: both methods contain a `catch` with a comment and no logging. Every failure - a database which is not reachable, a
  broken token-store, an identity-provider which answers with an error - is turned into "not authenticated".
- Impact: the behaviour fails closed, which is right, but it is invisible: an outage of the token-store looks like a wave of
  wrong credentials, and a broken oidc-provider looks like users who send invalid tokens. Nobody can distinguish an attack from
  a defect.
- Recommendation: log the caught exception (at least at debug-level, and at warning-level for an infrastructure-error) and
  distinguish "credentials are wrong" from "the check could not be performed", so that the second case can answer with a
  service-unavailable.

The library also offers no notion of a security-event and no alerting-hook: a failed login, a refused authorization and a blocked
request all end up in the ordinary log, so a consumer which wants to alert on them has to parse text.

### A10:2025 Mishandling of Exceptional Conditions

The default is sound in the most important respect: `DefaultExceptionHandlerMiddleware.GetExceptionResponceContent` returns an
empty body, so no stack-trace and no exception-message reaches the client in any environment. A consumer which overrides it has
to take care of that itself.

**GRY-34 - A security-decision is signalled as an exception and mapped by a separate, optional middleware** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. Letting the middlewares write the status-code and short-circuit themselves changes the control-flow and the 401/403-behaviour for a consumer which relies on its own exception-middleware for that mapping.

- Affected component: `APIServer/MidT/Auth/AuthenticationMiddleware.cs`, `APIServer/MidT/Aut/AuthorizationMiddleware.cs`, `APIServer/MidT/Exception/ExceptionManagerMiddleware.cs`, `APIServer/APIServer.cs`
- Attack-surface: internet-exposed in every consumer
- Evidence: a missing authentication is a `BadRequestException(401)` and a missing authorization a `BadRequestException(403)`;
  which status-code the caller really gets is decided by the exception-middleware, which `AddDefinedMiddleware` only adds when
  the consumer configured one. `AutSRMiddleware` additionally throws an `InternalAlgorithmException` when the attribute it
  expects is missing, and uses `AssertCondition` to state that credentials are present.
- Impact: the outcome of an access-control-decision is spread over two components which are configured independently of each
  other. A consumer without an exception-middleware answers an authorization-failure with an unhandled exception, and a consumer
  with its own one decides the semantics of 401 and 403 by accident. The assertion turns a configuration-mistake into a 500
  instead of into a clear refusal.
- Recommendation: let the middlewares write the status-code themselves and short-circuit the pipeline, and keep the
  exception-middleware for unexpected errors only.

**GRY-35 - The health-check reports a healthy service while it is still initializing** (Medium, confirmed)

- State: open

- Fixable without breaking changes: No. Changing `Initializing` from `Healthy` to `Degraded`/`Unhealthy` changes the readiness-signal an orchestrator acts on; that observable change is the very point of the fix, and a probe which currently depends on "healthy during init" would see it.

- Affected component: `APIServer/Utilities/Tools.cs` (`InitializationStateVisitor.Handle(Initializing)`)
- Attack-surface: the orchestrator of every consumer
- Evidence: the visitor answers `Uninitialized` with `Degraded` and `InitializationFailed` with `Unhealthy`, but `Initializing`
  with `HealthCheckResult.Healthy("Initializing")`.
- Impact: a container-orchestrator or a load-balancer which trusts the health-check routes traffic to an instance whose
  initialization - which includes the database-migration - is still running. The requests which arrive then work on a schema
  which is being changed.
- Recommendation: answer `Initializing` with `Degraded` (or `Unhealthy`), so that readiness really means ready.

**GRY-36 - A forwarded-for header with several addresses makes every request fail** (Low, confirmed)

- State: open

- Fixable without breaking changes: Partial. Falling back to the connection-address for a malformed/multi-value header (instead of throwing) only affects today's crash-path and leaves correct single-proxy usage unchanged; restricting the trust to a configured proxy-list is new configuration. `TrustForwardedHeader` defaults to false, so default consumers are unaffected either way.

- Affected component: `APIServer/Mid/General/GeneralMiddleware.cs` (`GetIPAddress`)
- Attack-surface: a consumer which sets `TrustForwardedHeader`
- Evidence: the whole header-value is passed to `IPAddress.Parse`, which throws for the comma-separated list that a chain of
  proxies produces (and that any client can send). The header is also trusted without checking that the request really came from
  a trusted proxy.
- Impact: with the option switched on, a second proxy in the chain - or a single client which sends its own header - makes the
  outermost-but-one middleware throw for every request. When it does work, the logged client-address is a value the client
  chose, which spoofs the log and any ip-based decision built on it. The default of `TrustForwardedHeader` is `false`, which is
  the right default, but then the logged address is always the proxy and the log can not attribute a request to a client at all.
- Recommendation: parse only the entry which the trusted proxy appended, accept the header only from a configured list of
  trusted proxy-addresses, and handle a malformed value by falling back to the connection-address instead of by throwing.

**GRY-37 - Smaller robustness-defects in the same area** (Low, confirmed)

- State: open

- Fixable without breaking changes: Partial. The null-check on `Path.Value`, using `FirstOrDefault` for the `nameidentifier`-claim, routing the error-handling-failure to the log, and removing the dead code are behaviour-preserving robustness-fixes; changing the invalid-credentials-status from 400 to 401 is a client-visible contract-change and is therefore breaking.

- Affected component: `APIServer/Services/Trans/TransientAuthenticationService.cs`, `APIServer/Mid/M05DLog/DRequestLoggingMiddleware.cs`, `APIServer/MidT/Auth/AuthenticationMiddleware.cs`, `APIServer/MidT/Exception/ExceptionManagerMiddleware.cs`
- Attack-surface: internal
- Evidence and impact, each small on its own: invalid credentials are answered with the status-code 400 instead of 401, so a
  client can not distinguish a wrong password from a malformed request. `ShouldLogEntireRequestContentInLogFile` contains dead
  code which shows that an intended distinction between an error-response and a normal one was lost.
  `AuthenticationIsRequired` passes `context.Request.Path.Value` to `Regex.IsMatch` without a null-check.
  `IsAuthenticatedInternal` reads the `nameidentifier`-claim with `First()`, which throws when it is absent.
  `IsAuthenticatedInternal` also still contains an unused external-provider-block behind `bool externalProviderEnabled = false`.
  A failure while handling an error is written only to `Console.Error` and reaches no log.
- Recommendation: work these off together with GRY-34, since they all sit on the same path.

### Validation and test-coverage

No test was found for any of the paths above: not for the decision of `AuthenticationIsRequired`, not for the route-allowlist,
not for the authorization-middleware, not for the password-hashing or the login, not for the token-validity, not for the
oidc-validation (audience, issuer, signature, expiry) and not for the body-logging. For a library whose purpose is to make these
decisions for its consumers, that is the most important gap of this analysis: every finding above would be cheap to hold shut
with a test, and without such tests a consumer has no way to tell which behaviour is intended and which is an accident.

What is already sound and should be kept: the authorization-code-flow with pkce and s256, the state- and verifier-generation from
`RandomNumberGenerator`, the full validation of issuer, signature and lifetime of a jwt against the discovery-document, the
parameterized data-access-layer, the empty exception-response-body, the fact that request-headers are not logged by default, the
secure default of `TrustForwardedHeader`, `AddServerHeader = false`, the thread-safe resource-loader which reads only embedded
resources, and the exact pinning of every dependency together with a lock-file.

### Prioritized remediation plan

1. Replace the password-handling: a real key-derivation-function with a salt, offered as a service of the library, and remove or
   implement the argon2-stub (GRY-11, ~~GRY-12~~). Everything a consumer copies starts here.
2. Make the oidc-validation strict: require the audience, require https for the authority, and carry the issuer in the principal
   (GRY-23, GRY-14, GRY-24).
3. Stop logging bodies unredacted, and exclude the authentication-routes by default (GRY-30, GRY-31).
4. Make the access-control fail closed: authentication required by default, anchored allowlist-patterns, authorization from the
   established principal, and no silent disabling through an empty group-set (GRY-01, GRY-02, GRY-03, GRY-04).
5. Deliver the threat-protection the library already outlines: a concrete rate-limiting-middleware with a pipeline-hook, plus a
   failed-login-counter and a lockout (GRY-17).
6. Remove the synchronous-over-asynchronous pipeline and the `AllowSynchronousIO`-flag (GRY-16, GRY-08).
7. Fix the oidc-bearer-path and the credential-header-contract, and cache the discovery-document and the key-set (GRY-25,
   GRY-26, GRY-21).
8. Let the consumer protect the maintenance-routes, and offer a security-header-middleware (GRY-05, GRY-06).
9. Make the remaining robustness-findings behave correctly (GRY-34, GRY-35, GRY-36, GRY-37), decide what happens with the
   mfa-stub and the deactivation-flag (GRY-18, GRY-19), and work off the integrity- and supply-chain-findings (GRY-28, GRY-29,
   ~~GRY-09~~, ~~GRY-10~~, GRY-13, ~~GRY-15~~, GRY-20, GRY-22, GRY-27, GRY-32, GRY-33).
10. Add tests for every path named in the previous section.

### Summary of all findings

| Id | OWASP-category | Affected component | Criticality | Confidence | Status |
| --- | --- | --- | --- | --- | --- |
| GRY-11 | A04 Cryptographic Failures | `TransientAuthenticationService.Hash` | Critical | confirmed | open |
| GRY-01 | A01 Broken Access Control | `AuthenticationMiddleware.AuthenticationIsRequired` | High | confirmed | open |
| GRY-03 | A01 Broken Access Control | `AutSRMiddleware.IsAuthorized`, `AutSAMiddleware.IsAuthorized` | High | confirmed | open |
| GRY-12 | A04 Cryptographic Failures | `Crypto/Argon2`, `Crypto/GRYBCryptoSystem` | High | confirmed | fixed |
| GRY-16 | A06 Insecure Design | `ExceptionManagerMiddleware`, `AuthSMiddleware`, `MaintenanceRoutesController` | High | confirmed | open |
| GRY-17 | A06 Insecure Design | `MidT/RateLimit`, `MidT/WAF`, `MidT/Obfuscation`, `APIServer.cs` | High | confirmed | open |
| GRY-23 | A07 Authentication Failures | `OIDCService.ValidateJwtAndParseClaimsAsync` | High | confirmed | open |
| GRY-24 | A07 Authentication Failures | `AuthSMiddleware.TryGetOIDCAuthentication` | High | confirmed | open |
| GRY-30 | A09 Security Logging and Alerting Failures | `DRequestLoggingMiddleware` | High | confirmed | open |
| GRY-02 | A01 Broken Access Control | `AuthenticationMiddleware`, `DRequestLoggingMiddleware.IsIgnored` | Medium | confirmed | open |
| GRY-04 | A01 Broken Access Control | `AuthorizationMiddleware.AuthorizationIsRequired` | Medium | confirmed | open |
| GRY-05 | A01 Broken Access Control | `MaintenanceRoutesController` | Medium | confirmed | open |
| GRY-06 | A02 Security Misconfiguration | `APIServer.cs` | Medium | confirmed | open |
| GRY-07 | A02 Security Misconfiguration | `APIServer.cs` (kestrel-configuration) | Medium | confirmed | open |
| GRY-09 | A03 Software Supply Chain Failures | `GRYLibrary.csproj` (`System.Data.SqlClient`) | Medium | confirmed | fixed |
| GRY-13 | A04 Cryptographic Failures | `AccessToken`, `TransientAuthenticationService` | Medium | confirmed | open |
| GRY-14 | A04 Cryptographic Failures | `OIDCService.FetchDiscoveryAsync`, `.FetchJwksAsync` | Medium | confirmed | open |
| GRY-18 | A06 Insecure Design | `MFA/TOTP`, `User.CreateNewUser` | Medium | confirmed | open |
| GRY-19 | A06 Insecure Design | `TransientAuthenticationService.Login`, `User` | Medium | confirmed | open |
| GRY-20 | A06 Insecure Design | `APIServer.cs` (middleware-order) | Medium | confirmed | open |
| GRY-21 | A06 Insecure Design | `OIDCService`, `AuthSMiddleware` | Medium | confirmed | open |
| GRY-25 | A07 Authentication Failures | `AuthenticationMiddleware.IsAuthenticatedInternal`, `Tools.GetUser` | Medium | confirmed | open |
| GRY-26 | A07 Authentication Failures | `HeaderService`, `HeaderTools`, `AuthSFilter` | Medium | confirmed | open |
| GRY-28 | A08 Software or Data Integrity Failures | `Logging/GRYLogger` | Medium | confirmed | open |
| GRY-31 | A09 Security Logging and Alerting Failures | `DRequestLoggingMiddleware.ShouldBeLogged` | Medium | confirmed | open |
| GRY-32 | A09 Security Logging and Alerting Failures | `GeneralMiddleware` | Medium | confirmed | open |
| GRY-33 | A09 Security Logging and Alerting Failures | `AuthSMiddleware` | Medium | confirmed | open |
| GRY-34 | A10 Mishandling of Exceptional Conditions | `AuthenticationMiddleware`, `AuthorizationMiddleware`, `ExceptionManagerMiddleware` | Medium | confirmed | open |
| GRY-35 | A10 Mishandling of Exceptional Conditions | `Tools.InitializationStateVisitor` | Medium | confirmed | open |
| GRY-08 | A02 Security Misconfiguration | `APIServer.cs` (`AllowSynchronousIO`) | Low | confirmed | open |
| GRY-10 | A03 Software Supply Chain Failures | `GRYLibrary.csproj` | Low | confirmed | fixed |
| GRY-15 | A05 Injection | the database-interactors (migration-statements) | Low | confirmed | fixed |
| GRY-22 | A06 Insecure Design | `OIDCService.LoginWithPasswordAsync` | Low | confirmed | open |
| GRY-27 | A07 Authentication Failures | `OIDCService.InitiateLoginAsync` | Low | confirmed | open |
| GRY-29 | A08 Software or Data Integrity Failures | `GRYLibrary.csproj` | Low | confirmed | open |
| GRY-36 | A10 Mishandling of Exceptional Conditions | `GeneralMiddleware.GetIPAddress` | Low | confirmed | open |
| GRY-37 | A10 Mishandling of Exceptional Conditions | several files on the authentication- and logging-path | Low | confirmed | open |

## Build

This product requires to use `scbuildcodeunits` implemented/provided by [ScriptCollection](https://github.com/anionDev/ScriptCollection) to build the project.

## Changelog

See the [Changelog-folder](./Other/Resources/Changelog).

## Contribute

Contributions are always welcome.

See [Contributing.md](./Contributing.md) for information about that.

## Repository-structure

This product uses the [CommonProjectStructure](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/RepositoryStructure/CommonProjectStructure/CommonProjectStructure.md) as repository-structure.

## Branching-system

This product follows the [GitFlowSimplified](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/BranchingSystem/GitFlowSimplified/GitFlowSimplified.md)-branching-system.

## Versioning

This product follows the [SemVerPractise](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/Versioning/SemVerPractise/SemVerPractise.md)-versioning-system.

## License

There are the following licenses for the GRYLibrary available

- The GRYLibrary is generally and commonly licensed under the terms of GRYL. The concrete license-text can be found [here](https://raw.githubusercontent.com/anionDev/GRYLibrary/main/License.txt). This license-text does obviously not apply to the other following licenses.
- There are some special licenses for certain scopes:
  - [epew](https://github.com/anionDev/ExternalProgramExecutionWrapper) is allowed to use the [Nuget-release](https://www.nuget.org/packages/GRYLibrary) of the GRYLibrary under the Terms of the MIT-license for executing programs as main-purpose of epew and for nothing else.
  - [ReliablePlayer](https://github.com/anionDev/ReliablePlayer) is allowed to use the [Nuget-release](https://www.nuget.org/packages/GRYLibrary) of the GRYLibrary under the Terms of the MIT-license to help implement features provided in the official repository and release of ReliablePlayer and for nothing else.
- If you need another license-type or you want to use the GRYLibrary in your company then please contact the owner of the GRYLibrary.
