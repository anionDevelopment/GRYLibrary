# APIServer

The namespace `GRYLibrary.Core.APIServer` contains the parts with which an application of this family becomes a web-api-server: the folders it works in, the middlewares it runs, the routes it always has and the configuration it persists.
This article states the things which an application has to know about it and which are not visible in the signature of a single method.

## Where the folders of an application are

Every application has one base-folder; its data-folder, its configuration-folder and its log-folder lie below it.
Where that base-folder is depends on where the application runs:

| Situation      | Base-folder                               |
| -------------- | ----------------------------------------- |
| In a container | `/Workspace`                              |
| Locally        | `<folder of the program>/../../Workspace` |
| In a test-run  | a folder of its own below the temp-folder |

Whether the application runs in a container is decided by the environment-variable `ISRUNNINGINCONTAINER`: it states that when its value is `true`.
**The name of that variable is compared without case**, so an image which sets it as `IsRunningInContainer` states the same thing as one which sets it as `ISRUNNINGINCONTAINER`.

That comparison matters more than it looks: on linux a lookup which respects the case answers "no container" for an image which wrote the name differently, and the application then puts every one of its folders somewhere else (`/Workspace/Workspace` instead of `/Workspace`) - without any error, because there is nothing wrong with that folder.
Everything which expects a file at its documented place then fails, and the reason is nowhere near the symptom.
An image should write the name in upper case, which is the usual form of an environment-variable.

## The common routes

An application which hosts the common routes (`HostCommonRoutes`) offers three routes which redirect to an address of that application:

| Route                                                     | Redirects to                                  |
| --------------------------------------------------------- | --------------------------------------------- |
| `<api-prefix>/Other/Resources/Information/TermsOfService` | `ICommonRoutesInformation.TermsOfServiceLink` |
| `<api-prefix>/Other/Resources/Information/Contact`        | `ICommonRoutesInformation.ContactLink`        |
| `<api-prefix>/Other/Resources/Information/License`        | `ICommonRoutesInformation.LicenseLink`        |

**A link which is null means that the application has no such information, and then that route is not hosted at all.**
It is not hosted and answering with a redirect to nothing: an application which does not state a contact should answer "this route does not exist" and not "this route exists but leads nowhere".
The api-specification of the application names only the routes which exist either.

```csharp
applicationConfiguration.CommonRoutesInformation = new CommonRoutesInformation()
{
    LicenseLink = "https://information.example.com/License",
    // this application has no terms-of-service and no contact-route, so these two links stay null
};
```

Which routes exist is decided while the application starts (`CommonRoutesConvention`), so a link which is set later has no effect on it.

## Values which have to be hard to guess

`Utilities.GenerateSecureRandomValue(int amountOfCharacters = 32)` creates a hexadecimal value whose bytes come from the random-generator of the operating-system.
It is the function to use for a password, a token or anything else which somebody must not be able to compute:

```csharp
string token = Utilities.GenerateSecureRandomValue();      // 32 characters, which state 16 bytes
string shortValue = Utilities.GenerateSecureRandomValue(7); // an odd amount works as well
```

`Random` is deliberately not used for this: it is meant to be fast and reproducible, which is the opposite of what a password needs - whoever knows a few of its values can compute the following ones, and a value which can be computed is no secret.

## Login-throttling

`IAuthenticationService.Login(userName, password, clientIPAddress)` throttles logins to prevent online-password-guessing.
The throttling is done by an `ILoginThrottlingService` (default-implementation: `LoginThrottlingService`), which must be registered as singleton:

```csharp
services.AddSingleton<ILoginThrottlingConfiguration>(new LoginThrottlingConfiguration());
services.AddSingleton<ILoginThrottlingService, LoginThrottlingService>();
```

The client-ip-address is available in the controller as `(IPAddress?)HttpContext.Items["ClientIPAddress"]`.

The throttling works per user-name and per ip-address:

- From `AmountOfFailedLoginsPerUserNameUntilLock` (default: 5) failed logins for a user-name on, further logins for this user-name are locked, regardless of the ip-address.
- From `AmountOfFailedLoginsPerIPAddressUntilLock` (default: 20) failed logins from an ip-address on, further logins from this address are locked, regardless of the user-name. The threshold is higher because several users can share one ip-address. Ipv6-addresses are throttled per /64-network, and all clients with an unknown ip-address share one counter.
- The first lock lasts `InitialLockDuration` (default: 1 minute). Every further failed login doubles the duration up to `MaximalLockDuration` (default: 15 minutes).
- After `DurationUntilFailedLoginsAreForgotten` (default: 1 hour) without failed login the counter is forgotten.
- A successful login resets only the counter of the user-name. The counter of the ip-address is not reset, otherwise an attacker could reset it by logging in with an own account.
- A locked login is rejected with http-status 429 before the password is verified, also if the password is correct.
- Failed logins for user-names which do not exist are counted as well, so the throttling does not reveal whether a user-name exists.

The counters are kept in memory. They are lost when the application restarts and are not shared between several instances of the application. This is accepted by design.

## Migrations of the configuration-file

When a persisted property is renamed, moved or removed, existing configuration-files have to be migrated before they are loaded. How GRYLibrary does that and how an application adds its own migrations is described in the article [ConfigurationFileMigrations](./ConfigurationFileMigrations.md).
