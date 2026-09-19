# Composing a gateway from Portic.Core

`Portic.Core` is the packable .NET 10 runtime. `Portic.Sdk` remains the separate
Apache-2.0 contract/provider SPI. Only the Core project is packable; the HTTP host,
stub adapter and tests are not shipped as additional packages. Runtime metadata
uses AGPL-3.0-only per ADR-0005; no license terms or runtime behavior change.

The package README describes registration. Hosts supply their own providers and
identity configuration. No concrete model SDK, provider credential, private feed,
HTTP application or content logging is added by packaging.

## Verify before release

```sh
dotnet test Portic.sln -c Release
dotnet format Portic.sln --verify-no-changes
pwsh scripts/Test-Package.ps1
```

The last command packs the real assembly, checks license/dependency metadata,
and creates a consumer outside the checkout with an empty package cache. Only
Portic.Core comes from the temporary built-package feed; other dependencies
restore from nuget.org. The consumer registers a provider through Portic.Sdk and
executes IMessageGateway. This is an artifact test, not proof that a package has
already been published. Temporary test directories remain available for inspection.

MinVer derives versions from full history and `v` tags. Never edit a Version or
VersionPrefix to release. CI checks out full history. `dotnet pack` produces a
prerelease until a release tag exists. Runtime API compatibility is unchanged;
future breaking changes to packaged APIs require the normal ADR/migration process.

## First registry setup and release

The maintainer must establish ownership of `Portic.Core` on NuGet, configure
Trusted Publishing for this repository's `publish.yml`, set repository variable
`NUGET_USER` to the policy creator's username, and configure required reviewers
on the `release` environment. NuGet first-package bootstrap is separate from
building a package. Do not put API keys in the repository or a local feed into
another application's production configuration.

After main CI passes, run **Actions → release**, choose the bump and optionally
dry-run. The workflow checks the exact main commit, and actual publication refuses
to tag without the publishing setup and protected release environment. The job
uses the environment approval, builds/tests again and creates the tag. A dry-run
builds/packs only and still follows the environment's configured review policy.

The signed-release workflow reuses the organization's SBOM/provenance machinery.
The publisher dispatches it explicitly for the tag because a GITHUB_TOKEN-created
tag does not itself trigger another push workflow. It waits for success, downloads
the versioned release package and verifies its attestation before NuGet upload.
No secret is required for OIDC beyond the configured publisher identity. The
reusable release workflow and registry operations need their normal permissions.

An interrupted publishing job can be rerun against the same tag/commit; existing
tags pointing elsewhere are rejected. Packages are immutable. Fix a released bug
with a later release, never by replacing the package at the same version.

Completion of the packaging issue does not assert a real cloud/local completion,
a production identity posture or a successful registry bootstrap. Check the
published version on nuget.org before adding it to another application.
