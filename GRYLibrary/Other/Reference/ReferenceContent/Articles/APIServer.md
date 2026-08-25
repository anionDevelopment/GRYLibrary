# APIServer

The namespace `GRYLibrary.Core.APIServer` contains the parts with which an application of this family becomes a
web-api-server: the folders it works in, the middlewares it runs, the routes it always has and the configuration it
persists. This article states the things which an application has to know about it and which are not visible in the
signature of a single method.

## Where the folders of an application are

Every application has one base-folder; its data-folder, its configuration-folder and its log-folder lie below it. Where
that base-folder is depends on where the application runs:

| Situation | Base-folder |
| --------- | ----------- |
| In a container | `/Workspace` |
| Locally | `<folder of the program>/../../Workspace` |
| In a test-run | a folder of its own below the temp-folder |

Whether the application runs in a container is decided by the environment-variable `ISRUNNINGINCONTAINER`: it states
that when its value is `true`. **The name of that variable is compared without case**, so an image which sets it as
`IsRunningInContainer` states the same thing as one which sets it as `ISRUNNINGINCONTAINER`.

That comparison matters more than it looks: on linux a lookup which respects the case answers "no container" for an
image which wrote the name differently, and the application then puts every one of its folders somewhere else
(`/Workspace/Workspace` instead of `/Workspace`) - without any error, because there is nothing wrong with that folder.
Everything which expects a file at its documented place then fails, and the reason is nowhere near the symptom. An
image should write the name in upper case, which is the usual form of an environment-variable.

## The common routes

An application which hosts the common routes (`HostCommonRoutes`) offers three routes which redirect to an address of
that application:

| Route | Redirects to |
| ----- | ------------ |
| `<api-prefix>/Other/Resources/Information/TermsOfService` | `ICommonRoutesInformation.TermsOfServiceLink` |
| `<api-prefix>/Other/Resources/Information/Contact` | `ICommonRoutesInformation.ContactLink` |
| `<api-prefix>/Other/Resources/Information/License` | `ICommonRoutesInformation.LicenseLink` |

**A link which is null means that the application has no such information, and then that route is not hosted at all.**
It is not hosted and answering with a redirect to nothing: an application which does not state a contact should answer
"this route does not exist" and not "this route exists but leads nowhere". The api-specification of the application
names only the routes which exist either.

```csharp
applicationConfiguration.CommonRoutesInformation = new CommonRoutesInformation()
{
    LicenseLink = "https://information.example.com/License",
    // this application has no terms-of-service and no contact-route, so these two links stay null
};
```

Which routes exist is decided while the application starts (`CommonRoutesConvention`), so a link which is set later has
no effect on it.

## Values which have to be hard to guess

`Utilities.GenerateSecureRandomValue(int amountOfCharacters = 32)` creates a hexadecimal value whose bytes come from the
random-generator of the operating-system. It is the function to use for a password, a token or anything else which
somebody must not be able to compute:

```csharp
string token = Utilities.GenerateSecureRandomValue();      // 32 characters, which state 16 bytes
string shortValue = Utilities.GenerateSecureRandomValue(7); // an odd amount works as well
```

`Random` is deliberately not used for this: it is meant to be fast and reproducible, which is the opposite of what a
password needs - whoever knows a few of its values can compute the following ones, and a value which can be computed is
no secret.
