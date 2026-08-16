---
name: product-knowledge
description: What GRYLibrary is, how this repository is structured and which mechanisms exist for building and testing it. Use this before fixing a defect or developing a feature in this repository, to know where things belong and how to verify a change.
---

# GRYLibrary

GRYLibrary is the dotnet-library which the dotnet-products of these repositories are built on. Two parts of it
matter most:

- **`APIServer`** - the api-server-framework: the middlewares (authentication, authorization, rate-limiting,
  exception-handling, request-logging, maintenance-site, captcha), the maintenance-routes, the configuration-
  and initialization-handling and the commandline-verbs. Every backend here (SRM, ConSurv, OpenDMS,
  RSSArchivist, ClientInformation, GRYProxy, SimpleOCRService, ...) starts through it.
- **`Misc` / `Logging`** - the general utilities, the logger (`IGRYLog`) and the console-application-overhead
  (`GRYConsoleApplication`, `VerbParser`).

This is the widest-reaching code of all the dotnet-products, so a change here has to be made with the
consumers in mind.

## Structure of the repository

The repository follows the "common project structure": all sourcecode lives in code-units, and every code-unit
has its own `Other`-folder with its build-, quality-check- and reference-files. Use the
`work-with-common-project-structure`-skill when you need the details of that structure.

## Two properties which the consumers depend on

- **The nuget-package declares no dependencies.** Whoever uses GRYLibrary has to reference everything it needs
  by themselves (EF Core, Npgsql, MySqlConnector, ExtendedXmlSerializer, ...). That is why a consumer can
  compile fine and then fail at runtime with "Could not load file or assembly" - the assembly-version which
  GRYLibrary was built against has to be referenced by the consumer. When the version of such a dependency is
  raised here, the consumers have to follow.
- **A rename in the api reaches every consumer.** The namespaces were reorganized (`Miscellaneous` became
  `Misc`, the logging moved to `Logging.GRYLogger` / `Logging.GeneralPurposeLogger`) and signatures changed
  (for example `GRYConsoleApplication`, which now takes a `ParserBase` and a log). Every such change means
  work in all consuming repositories, so it is worth asking whether the old shape can be kept working.

## Registered and not registered

In the dependency-injection-container of the api-server, `IServerLog` is registered; `IGRYLog` and
`IGeneralLogger` are **not**. A service which asks for one of the latter compiles and then fails at startup
with "Unable to resolve service for type ...". When a service of a product needs the log, it takes
`IServerLog`.

## Building and testing

`scbuildcodeunits` builds everything: compile, tests, linting and security-checks. The task `task bb`
(`BaseBuildAllCodeunits`) does the same.

The `runsettings.xml` of this repository is the reference for all other dotnet-code-units: it uses the
collector `XPlat Code Coverage` with the format `cobertura`, which is what the pipeline expects. The old
collector `Code Coverage` does not produce such a file and makes the pipeline fail while evaluating the
coverage.
