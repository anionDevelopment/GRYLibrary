# OIDC

## Description

The GRYLibrary provides functionality to add [OpenID Connect (OIDC)](https://openid.net/developers/how-connect-works/)
as an alternative login-mechanism to an application that is built as a web-API with the GRYLibrary.

The idea is a clean separation of responsibilities:

- The application only supplies the *configuration* of the OIDC-provider (for example a [Keycloak](https://www.keycloak.org/)-server).
- The GRYLibrary implements the actual protocol: it talks to the provider, performs the login and validates the tokens.

The functionality is exposed by the injectable service `IOIDCService`
(namespace `GRYLibrary.Core.APIServer.Services.OIDC`) and its default-implementation `OIDCService`.

## Provider-configuration

A single OIDC-provider is described by an `OIDCProviderConfiguration`:

```csharp
OIDCProviderConfiguration provider = new OIDCProviderConfiguration
{
    Id = "keycloak",                                              // internal identifier for this entry
    DisplayName = "Company-Login",                               // label that can be shown on a login-button
    Authority = "https://login.example.com/realms/myrealm",      // the OIDC-authority (issuer base-url)
    ClientId = "my-application",                                 // the client-id registered in the provider
    ClientSecret = "…",                                          // optional: only for confidential clients
    Scope = "openid profile email",                             // optional: defaults to "openid profile email"
    RedirectUri = "https://app.example.com/oidc-callback",       // only used by the authorization-code-flow
    Audience = null,                                             // optional: expected audience of incoming access-tokens
};
```

The application typically reads these values from its own configuration-file so that a deployment can point the
application at its own OIDC-provider without any code-change.

## Registration

Register the OIDC-service in the dependency-injection-container:

```csharp
services.AddOIDC(); // registers IOIDCService -> OIDCService
```

The application keeps ownership of the `OIDCProviderConfiguration`-values and passes them to the service where needed.

## The two login-flows

The GRYLibrary supports two different ways to log a user in. They solve different use-cases.

### 1. Authorization-code-flow with PKCE (recommended)

This is the standard, recommended browser-based flow. The user's password is entered *at the provider* and is never
seen by the application. It supports multi-factor-authentication, consent-screens and social-logins, because the whole
interactive login happens on the provider's login-page.

The flow works like this:

1. The application calls `InitiateLoginAsync(provider)`. This returns an `OIDCAuthorizationRequest` that contains the
   `AuthorizationUrl` (to which the browser must be redirected), a `State` and a `CodeVerifier`.
2. The application stores `State` and `CodeVerifier` server-side (keyed by `State`) and redirects the browser to the
   `AuthorizationUrl`.
3. The user logs in at the provider. The provider redirects the browser back to the configured `RedirectUri` with a
   `code` and the `state`.
4. The application looks up the stored `CodeVerifier` for the returned `state` and calls
   `ExchangeCodeAsync(provider, code, codeVerifier)`. This exchanges the code for tokens, validates the id-token and
   returns an `OIDCTokenResult` with the user's claims (`Subject`, `PreferredUsername`, `Email`, `Name`, `Claims`).

```csharp
OIDCAuthorizationRequest request = await oidcService.InitiateLoginAsync(provider);
// store request.State + request.CodeVerifier, then redirect the browser to request.AuthorizationUrl
// …later, on the callback:
OIDCTokenResult result = await oidcService.ExchangeCodeAsync(provider, code, codeVerifier);
```

Use this flow whenever the login happens in a browser and you can redirect the user to the provider.

### 2. Password-flow (resource-owner-password-credentials)

The password-flow lets an application keep its existing classic `username`/`password`-login-form but delegate the
credential-check to the OIDC-provider. When OIDC is used, the application's own login-implementation forwards the
received credentials to the GRYLibrary, which authenticates them against the provider and returns the resulting token:

```csharp
OIDCPasswordLoginResult result = await oidcService.LoginWithPasswordAsync(provider, username, password);
// result.AccessToken is the token issued by the provider; return it to the client.
```

`OIDCPasswordLoginResult` contains the `AccessToken`, optionally a `RefreshToken` and `IdToken`, the lifetime
(`ExpiresInSeconds`) and — if an id-token was returned — the validated `Subject` and further claims.

A login-implementation can therefore branch like this:

```csharp
public AccessToken Login(string userName, string password)
{
    if (this.OIDCIsEnabled)
    {
        OIDCPasswordLoginResult oidcResult = this._OIDCService.LoginWithPasswordAsync(this._Provider, userName, password).GetAwaiter().GetResult();
        return new AccessToken
        {
            Value = oidcResult.AccessToken,
            OwnerUserId = oidcResult.Subject,
            ExpiredMoment = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().AddSeconds(oidcResult.ExpiresInSeconds),
        };
    }
    else
    {
        // …the application's built-in local login that issues its own token…
    }
}
```

## What is the resource-owner-password-credentials-flow (ROPC) and why is it special?

The password-flow above is the OAuth-2.0 "resource-owner-password-credentials"-grant, usually abbreviated **ROPC**.

**What it is:** Instead of redirecting the user to the provider's login-page, the application collects the `username`
and `password` itself and sends them directly to the provider's token-endpoint (`grant_type=password`). The provider
checks the credentials and, if they are correct, returns the tokens.

**Which use-case it is for:** It exists for the situation where an application already has a classic username/password
login-form and wants to delegate the credential-check to a central identity-provider **without changing that form or
adding a browser-redirect**. In other words: the user-experience stays exactly the same (enter username and password in
the application), but the identity is verified by the OIDC-provider instead of a local user-database. This is exactly
the case this article's password-flow addresses.

**Consequences you must be aware of:**

- Because the application receives the plain-text password (it has to, in order to forward it), it defeats one of the
  main benefits of OIDC, where the application normally never sees the password. Only use ROPC when the application is
  fully trusted with the credentials (typically a first-party application).
- ROPC does **not** support multi-factor-authentication, consent-screens or social-/external-logins, because there is no
  interactive provider-page involved. If you need any of those, use the authorization-code-flow instead.
- ROPC is considered **deprecated** in the current OAuth-best-practices (OAuth 2.1) and should be avoided for new
  designs where a browser-redirect is possible.
- The provider's client must explicitly allow this grant. In Keycloak this is the setting **"Direct Access Grants
  Enabled"** on the client. For a confidential client you additionally have to configure the `ClientSecret`.

In short: prefer the authorization-code-flow. Use the password-flow only when you deliberately want to keep a classic
login-form and delegate only the credential-verification to the provider, and when the application is trusted with the
password.

## Authenticating subsequent requests with an OIDC-token

After a login via OIDC, the client sends the provider's access-token (a JWT) with each subsequent request. The
authentication-middleware (`AuthSMiddleware`) can validate such a token in addition to application-local tokens.

This is enabled by registering an `IOIDCAuthenticationConfiguration` (besides `AddOIDC()`), which lists the providers
whose tokens are accepted:

```csharp
services.AddOIDC();
services.AddSingleton<IOIDCAuthenticationConfiguration>(new OIDCAuthenticationConfiguration
{
    Providers = new List<OIDCProviderConfiguration> { provider },
});
```

With this registration in place, `AuthSMiddleware` behaves as follows for every incoming request:

1. First it checks whether the presented token is a valid application-local token (the existing behaviour).
2. If it is not, it tries to validate the token as an OIDC-token via `IOIDCService.ValidateAccessTokenAsync`. The token's
   signature is verified against the provider's published keys (JWKS) and its issuer and lifetime are checked (and its
   audience, if `OIDCProviderConfiguration.Audience` is set). On success the request is authenticated and a principal is
   built from the token's claims (the `sub`-claim becomes the user-identifier).

If neither an `IOIDCService` nor an `IOIDCAuthenticationConfiguration` is registered, only application-local tokens are
accepted and the behaviour is unchanged. This makes the OIDC-token-validation strictly opt-in.

You can also call `ValidateAccessTokenAsync` directly if you need to validate an OIDC-token yourself:

```csharp
OIDCTokenResult result = await oidcService.ValidateAccessTokenAsync(provider, incomingAccessToken);
```

### Mapping the OIDC-identity to a local user

Authentication (who the user is) is handled by the GRYLibrary. Authorization (what the user is allowed to do) usually
depends on the application's own roles and user-records. Because the GRYLibrary cannot know how an external identity maps
to a local user, the application stays responsible for that mapping. The recommended approach is:

- On the first successful OIDC-login, provision a local user that is keyed by the OIDC-`Subject` (the `sub`-claim), and
  assign it the roles the application needs.
- Make the application's authentication-service resolve that local user for the OIDC-token so that role-based
  authorization keeps working the same way as for local users.

This keeps the whole authorization-model of the application unchanged while OIDC is used only as an alternative way to
prove the user's identity.
