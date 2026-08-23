# ADR-0001: Provider SPI location — `portic-sdk` (Apache-2.0)

- Status: **Accepted / implemented**
- Date: 2026-08-02

## Context

The provider SPI (`IChatProvider` and the normalized `ChatRequest`/`ChatCompletion` contracts) is the
permanent AI abstraction every model call goes through (AGENTS.md; handbook `10`). Two facts pull on
where it should live:

- This runtime repo is **AGPL-3.0**. Client SDKs / provider SPI / contracts are designated
  **Apache-2.0** in `portic-sdk` (AGENTS.md "Licensing").
- Third-party integrators need to implement provider adapters **without** taking an AGPL dependency.
  If the SPI ships only from the AGPL runtime, every external adapter inherits AGPL — which defeats
  the "providers are disposable, the abstraction is permanent, integrators are welcome" intent.

## Decision

The SPI and the normalized wire contracts should live in the Apache-2.0 **`portic-sdk`** package, and
this runtime should depend on that package rather than defining the interface locally.

This decision is now implemented: the runtime consumes `Portic.Sdk` from nuget.org, and the local
stubbed copies in `Portic.Core` have been removed. The move stayed deliberately small — one
interface plus four records — so it remained a namespace/assembly extraction, not a redesign.

## Consequences

- **Now:** `IChatProvider`, `ChatRequest`, `ChatCompletion`, `ChatMessage`, and `TokenUsage` come from
  `Portic.Sdk`; the runtime depends on the public Apache-2.0 package while remaining AGPL-3.0.
  Adapter authors reference only the Apache-2.0 package.

## Result

`portic-sdk` now owns the public SPI and normalized contracts. `portic-community` consumes those
published types directly and no longer carries a parallel local copy.
