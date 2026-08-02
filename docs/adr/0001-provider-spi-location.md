# ADR-0001: Provider SPI location — `portic-sdk` (Apache-2.0)

- Status: **Proposed**
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

## Decision (proposed)

The SPI and the normalized wire contracts should live in the Apache-2.0 **`portic-sdk`** package, and
this runtime should depend on that package rather than defining the interface locally.

Until `portic-sdk` exists / is wired up, the SPI is **stubbed locally** in `Portic.Core` (namespaces
`Portic.Core.Providers` and `Portic.Core.Contracts`) with a `TODO(ADR-0001)` on `IChatProvider`. This
is deliberately the smallest surface — one interface plus four records — so the later move is a
namespace change, not a redesign.

## Consequences

- **Now:** no external dependency; contributors can build immediately. The boundary is still enforced
  by the fitness test regardless of where the SPI type is declared.
- **When adopted:** move `IChatProvider`, `ChatRequest`, `ChatCompletion`, `ChatMessage`, `TokenUsage`
  into `portic-sdk`; replace the local types with a package reference; keep the runtime AGPL and the
  contract Apache-2.0. Adapter authors reference only the Apache-2.0 package.

## Open question

Does `portic-sdk` already define equivalents we must conform to? If so, this runtime conforms to
those names rather than the placeholders here. Do not fork a second contract.
