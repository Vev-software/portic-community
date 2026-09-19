# Licensing

Portic Community Edition (`portic-community`) is **dual-licensed**. You may use, copy,
modify and distribute the runtime in this repository under **either**:

1. the **GNU Affero General Public License, version 3.0** (AGPL-3.0) — the terms in
   [`LICENSE`](./LICENSE); **or**
2. a **commercial license** from VEV Software ApS.

You choose which set of terms applies to your use. If you have not signed a commercial
agreement with VEV, the AGPL-3.0 applies.

## Which license do I need?

AGPL-3.0 is a strong copyleft license, and its network-use clause (§13) is what matters
most for a gateway that is normally run as a service:

- **AGPL-3.0 is enough when** you self-host Portic for your own use, **or** you distribute
  or operate a modified Portic and are willing to offer the complete corresponding source
  of your version — your modifications and anything you combine into the same program — to
  its users, under AGPL-3.0.
- **You need a commercial license when** you want to build Portic into a proprietary or
  closed-source product, embed it in an application you do not want to release under
  AGPL-3.0, or offer it as a hosted service without meeting AGPL-3.0's source-offer
  obligations.

VEV's own hosted **Portic Cloud** and the private modules in `portic-enterprise` use this
runtime under the **commercial** license, not the AGPL. Because VEV holds the copyright to
the runtime (see *Contributions* below), it can offer it under both licenses at once.

## The client SDK and provider SPI are permissive — depend on those freely

The contracts you integrate against — `Portic.Sdk` (provider SPI + normalized message
contracts) and `Portic.Client` — live in the separate
**[`portic-sdk`](https://github.com/Vev-software/portic-sdk)** repository under
**Apache-2.0**. You can depend on those from proprietary code with no AGPL obligation.
Only the **runtime** in this repository is AGPL/commercial dual-licensed.

| Component | Repository | License |
| --- | --- | --- |
| Provider SPI, client SDK, normalized contracts | `portic-sdk` (public) | Apache-2.0 |
| Community runtime (`Portic.Core`, gateway host, stub adapter) | `portic-community` (public) | AGPL-3.0 **or** commercial |
| Enterprise modules, hosted management, Portic Cloud | `portic-enterprise` (private) | Proprietary (commercial) |

The direction of dependencies is one-way: the runtime and the enterprise modules depend on
the permissive SDK/contracts, never the other way around.

## Commercial licensing

For commercial terms — proprietary embedding, closed-source distribution, or hosting
without AGPL source-offer obligations — contact VEV Software ApS via the address published
on the [`Vev-software`](https://github.com/Vev-software) organization profile, or open a
GitHub issue labelled `licensing`.

## Contributions

Because Portic Community is dual-licensed, every contribution must be made under terms that
let VEV offer it under **both** the AGPL-3.0 and the commercial license. Contributors
therefore agree to the [Contributor License Agreement](./CLA.md); see
[`CONTRIBUTING.md`](./CONTRIBUTING.md). This keeps the dual-license model intact: if even
one contribution were AGPL-only, VEV could no longer license the whole runtime commercially.

---

> This document explains the licensing model; it is **not itself a license and not legal
> advice**. The binding terms are in [`LICENSE`](./LICENSE) (AGPL-3.0) and in any commercial
> agreement you sign with VEV. The CLA and commercial-license templates in this repository
> should be reviewed by counsel before you rely on them.
