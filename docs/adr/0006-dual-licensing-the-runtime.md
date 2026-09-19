# ADR-0006: The community runtime is dual-licensed (AGPL-3.0 + commercial)

- Status: **Accepted** (maintainer decision, 2026-09-19)
- Date: 2026-09-19
- Amends [ADR-0005](./0005-license-policy.md): the runtime's *open* license stays AGPL-3.0
  exactly as ADR-0005 requires; this ADR adds VEV's parallel **commercial** license for the
  same runtime. Builds on [ADR-0003](./0003-license-file-discrepancy.md) (AGPL `LICENSE`
  text) and [ADR-0001](./0001-provider-spi-location.md) (Apache SPI).

## Context

Portic ships as an open-core product family:

- `portic-sdk` — provider SPI, client SDK and contracts — **Apache-2.0**
  (permissive integration surface, ADR-0001 here / portic-sdk ADR-0001).
- `portic-community` — the runtime — **AGPL-3.0** (this repo, ADR-0003 / ADR-0005).
- `portic-enterprise` — paid modules and VEV's hosted **Portic Cloud** — **proprietary**.

AGPL-3.0 is strong copyleft and is viral on linking. That created a real conflict for the
intended architecture: a **proprietary** enterprise/cloud host cannot link the AGPL runtime
without becoming AGPL-encumbered itself (including AGPL §13's network-use source-offer
obligation for a hosted service). Working around that by process-separating the proprietary
layer from the runtime over HTTP added complexity that existed only to avoid the license
clash.

VEV owns 100% of the copyright to the runtime (solo authorship, no external contributions,
no prior CLA). ADR-0005 already forbids relicensing the *open* runtime to Apache/BSL; it did
not address VEV offering the same code commercially in parallel. That copyright ownership
makes a cleaner resolution available without weakening ADR-0005.

## Decision

**The community runtime is dual-licensed: AGPL-3.0 OR a commercial license from VEV.**

- The root `LICENSE` stays the verbatim AGPL-3.0 text (unchanged; ADR-0003/0005). Public
  users get the runtime under AGPL-3.0, and packaging metadata keeps `AGPL-3.0-only` as its
  open license expression (ADR-0005). The commercial option is granted out-of-band, not by
  changing the package's declared open license.
- VEV additionally licenses the *same* runtime commercially. VEV's own `portic-enterprise`
  modules and hosted Portic Cloud consume the runtime under that **commercial** grant, not
  under AGPL — so proprietary code may link the runtime without AGPL obligations. This is
  the MongoDB/GitLab/Sentry open-core pattern.
- To keep dual-licensing legally sound, **all contributions require a CLA** granting VEV the
  right to license the contribution under both terms. Added: `LICENSING.md`, `CLA.md`,
  `CONTRIBUTING.md`, `NOTICE`.
- The permissive `portic-sdk` (Apache-2.0) is unchanged: proprietary code can always depend
  on the SDK/contracts freely. Only the runtime is dual-licensed.

This does **not** fork the codebase. There is one runtime; "community" and "cloud/enterprise"
are two *distributions* of it under two licenses, not two codebases (open-core, not fork).

## Consequences

- **Enables** the paid cloud/enterprise editions to build on the exact community runtime
  without AGPL infection, and without an HTTP hop introduced solely to dodge the license.
- ADR-0005 is preserved: the open runtime is still AGPL-3.0 and still must not be presented
  as Apache/BSL. This ADR only adds the parallel commercial grant that ADR-0005 left
  implicitly to "the maintainers".
- The AGENTS.md "Do not relicense" / "Do NOT touch LICENSE/NOTICE/CLA" guardrail is updated:
  the dual-license model is now the established decision; the AGPL `LICENSE` text still must
  not be altered, and the commercial arm is VEV's to grant.
- **Contribution intake now depends on the CLA.** The first external contribution accepted
  *without* a CLA would end VEV's ability to license the runtime commercially — so the CLA
  gate is mandatory, not optional.
- The community edition remains **production-worthy, not crippled** (AGENTS.md): the
  free/paid line is drawn on *operational and scale* features in `portic-enterprise` (multi-
  tenant, managed keys, signed spend/quota entitlements, SSO, SLA), never by degrading the
  free single-tenant gateway.
- These license/CLA texts are **templates pending legal review**; adopting them does not
  substitute for counsel before commercial reliance.

## Alternatives considered

- **Two separate codebases (fork)** — rejected: doubles maintenance/security/patching and
  causes feature drift. The perceived "easier to maintain" benefit actually comes from
  dual-licensing, not from forking.
- **AGPL-only, keep proprietary layer in a separate process** — rejected as the default:
  adds architecture complexity purely to avoid the license clash that dual-licensing
  removes. (Still available as a deployment option for third parties who do not hold a
  commercial license.)
- **Relicense the runtime to a permissive license (Apache/BSL)** — rejected and forbidden by
  ADR-0005: loses the copyleft that protects the free edition from being captured into closed
  forks.
