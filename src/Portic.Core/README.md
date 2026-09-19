# Portic.Core

The **community runtime** of [Portic](https://github.com/Vev-software/portic-community), VEV's
provider-neutral AI gateway. `Portic.Core` is the gateway *domain*: routing, orchestration,
governance and the observability ports (`IMessageGateway` and friends). It has **no dependency
on any concrete AI-provider SDK and no dependency on ASP.NET** — you compose it with a
`Portic.Sdk` provider adapter and a host to build a working gateway.

## What it is for

Use `Portic.Core` when you are **building a gateway host** (self-hosted or managed) and want the
same routing/orchestration the Portic community gateway uses, rather than re-implementing it.
Register the runtime, add one or more `IChatProvider` adapters (from `Portic.Sdk`), and map your
transport to `IMessageGateway`.

If you only need to *call* a Portic gateway, use **`Portic.Client`** instead. If you are writing a
provider adapter or integrating against the message contracts, use **`Portic.Sdk`**. Both are
Apache-2.0.

## Licensing

`Portic.Core` is **dual-licensed: AGPL-3.0 or a commercial license from VEV Software ApS**. The
package's declared open license is `AGPL-3.0-only`. Self-hosting under AGPL-3.0 is free; embedding
it in proprietary or closed-source software, or offering it as a hosted service without meeting
AGPL-3.0's source-offer obligations, requires a commercial license. See
[`LICENSING.md`](https://github.com/Vev-software/portic-community/blob/main/LICENSING.md).

## Links

- Source & docs: https://github.com/Vev-software/portic-community
- SDK / provider SPI (Apache-2.0): https://github.com/Vev-software/portic-sdk
