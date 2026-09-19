# Composing a gateway from Portic.Core

`Portic.Core` is the packable .NET 10 runtime (the gateway domain: routing, orchestration,
governance, observability ports). `Portic.Sdk` remains the separate Apache-2.0 contract/provider
SPI. **Only the Core project is packable**; the HTTP host, stub adapter and tests are not shipped
as packages. Package metadata declares `AGPL-3.0-only` per
[ADR-0005](./adr/0005-license-policy.md); the commercial arm of the dual license is granted
out-of-band ([ADR-0006](./adr/0006-dual-licensing-the-runtime.md)). Packaging changes no license
terms and no runtime behaviour.

A host composes a working gateway by registering the runtime and supplying its own providers and
identity configuration:

```csharp
services.AddPorticCore(configuration);
services.AddSingleton<IChatProvider, MyProvider>(); // a Portic.Sdk adapter
// ... then map your transport to IMessageGateway.SendAsync(...)
```

No concrete model SDK, provider credential, private feed, HTTP application or content logging is
added by packaging.

## Verify the package before release

```sh
dotnet test Portic.sln -c Release
dotnet format Portic.sln --verify-no-changes
pwsh scripts/Test-Package.ps1
```

`Test-Package.ps1` packs the real assembly, checks the nuspec metadata (id `Portic.Core`,
`AGPL-3.0-only`, README present, runtime dependencies present, no `MinVer` leak), then creates a
consumer **outside the checkout** with an empty package cache: only `Portic.Core` comes from the
temporary built-package feed (via `packageSourceMapping`), every other dependency restores from
nuget.org. The consumer registers a provider through `Portic.Sdk` and executes `IMessageGateway`.

This is an **artifact test**, not proof that a package has been published. CI runs it on every PR.

## Versioning & release

Versions are derived by MinVer from full git history and `v` tags — never hand-edit a `Version`.
`dotnet pack` produces a pre-release until a release tag exists, so CI checks out full history
(`fetch-depth: 0`). Releasing is the one-button **Actions → release** (`publish.yml`): it verifies
main + green CI, pauses on the protected `release` environment for approval, tags `vX.Y.Z`, and
publishes to nuget.org via OIDC Trusted Publishing. The first publish is a one-time maintainer
bootstrap (NuGet ownership + Trusted Publisher policy + `NUGET_USER` variable) — see the
`publish.yml` header. Packages are immutable: fix a released bug with a later version, never by
replacing one.
