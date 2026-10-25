<div align="center">

<img src=".github/logo.png" alt="C2ML" width="96">

# C2ML SDK

[![Website](https://img.shields.io/badge/website-c2ml.l3g3nd.dev-e6e600?style=flat-square)](https://c2ml.l3g3nd.dev)
[![Docs](https://img.shields.io/badge/docs-online-blue?style=flat-square)](https://c2ml.l3g3nd.dev/docs)
[![Release](https://img.shields.io/github/v/release/l3g3nd2410/C2ML-SDK?style=flat-square&label=latest%20release)](https://github.com/l3g3nd2410/C2ML-SDK/releases)
[![Loader](https://img.shields.io/badge/loader-CarX2ModLoader-111111?style=flat-square)](https://github.com/l3g3nd2410/CarX2ModLoader)
[![License](https://img.shields.io/badge/license-see%20below-lightgrey?style=flat-square)](#license)

**[Website](https://c2ml.l3g3nd.dev) · [Docs online](https://c2ml.l3g3nd.dev/docs) ·
[Getting started](docs/en/01-getting-started.md) · [Changelog](CHANGELOG.md) ·
[Loader repo](https://github.com/l3g3nd2410/CarX2ModLoader)**

</div>

---

Everything you need to develop a mod for **C2ML**, the mod loader for CarX Drift Racing Online 2.

## Getting the SDK

Download the latest `C2ML-SDK.zip` from this repository's
[Releases](https://github.com/l3g3nd2410/C2ML-SDK/releases) page and extract it. It is the same package offered
for download from your [developer space](https://c2ml.l3g3nd.dev/dev) once your access is approved.

## Contents

```
lib/       C2ML.API.dll (public API) + 0Harmony.dll (HarmonyX, only needed if you patch game code)
docs/      Full guide set, English (en/) primary and French (fr/)
examples/  C2ML.ExampleMod - reference mod using the WHOLE API (source + published .c2m)
```

## Guides

The same guides below are also available online, rendered as HTML with syntax-highlighted code and a copy
button, on the site's [Documentation section](https://c2ml.l3g3nd.dev/docs) - English by default, with a French
toggle.

| Guide | EN | FR |
|---|---|---|
| Install, first mod, local testing | [01-getting-started](docs/en/01-getting-started.md) | [01-getting-started](docs/fr/01-getting-started.md) |
| `BaseMod`, `[C2MLMeta]`, lifecycle, sub-pages | [02-mod-lifecycle](docs/en/02-mod-lifecycle.md) | [02-mod-lifecycle](docs/fr/02-mod-lifecycle.md) |
| Full reference for every menu widget | [03-menu-widgets](docs/en/03-menu-widgets.md) | [03-menu-widgets](docs/fr/03-menu-widgets.md) |
| Player-configurable keybinds | [04-keybinds](docs/en/04-keybinds.md) | [04-keybinds](docs/fr/04-keybinds.md) |
| Persistent settings | [05-config](docs/en/05-config.md) | [05-config](docs/fr/05-config.md) |
| Writing to `loader.log` | [06-logging](docs/en/06-logging.md) | [06-logging](docs/fr/06-logging.md) |
| Patching game code with HarmonyX | [07-harmony-patching](docs/en/07-harmony-patching.md) | [07-harmony-patching](docs/fr/07-harmony-patching.md) |
| Publishing, updates, test versions | [08-publishing](docs/en/08-publishing.md) | [08-publishing](docs/fr/08-publishing.md) |

`C2ML.ExampleMod` is a demo mod, not a real one: every API function has a minimal example there you can
copy-paste directly. Its source (`examples/C2ML.ExampleMod/ExampleMod.cs`) is the most complete reference, meant
to be read alongside the guides.

## License

`C2ML.API.dll` is distributed here solely so third parties can build mods against it; the API itself is closed
source, all rights reserved. `0Harmony.dll` is [HarmonyX](https://github.com/BepInEx/HarmonyX) by the BepInEx
team, © BepInEx, included unmodified under its own MIT license.
