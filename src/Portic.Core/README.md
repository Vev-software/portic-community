# Portic.Core

The .NET 10 provider-neutral gateway runtime: routing, policy enforcement,
message orchestration, and content-free audit/telemetry seams.

Runtime package metadata is **AGPL-3.0-only**, consistent with the repository's
license policy. See the [repository](https://github.com/Vev-software/portic-community)
for the established dual-license terms. This does not change licensing.

Hosts register `services.AddPorticCore(configuration)` from
`Portic.Core.DependencyInjection`, configure `Portic:DefaultProvider`, register
logging, and register their own `IChatProvider` implementations. Resolve
`Portic.Core.IMessageGateway` and call `SendAsync` using the normalized chat
contracts from `Portic.Sdk`.

The package includes no HTTP host, concrete provider, model credentials or
production identity setup. The default request context is single-tenant and the
default entitlement evaluator denies paid capabilities. A host must configure
its own identity/governance posture; composition does not grant capabilities.

Client and adapter authors who only need wire contracts or the provider SPI
should reference the separate, Apache-2.0 `Portic.Sdk` instead of this runtime.
Versions derive from repository release tags. Packaging does not change the
existing `/v1/messages` HTTP contract or add OpenAI wire compatibility.
