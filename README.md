# C2ML SDK

Everything you need to develop a mod for **C2ML**, the mod loader for CarX Drift Racing Online 2.

➡️ **Website:** [c2ml.l3g3nd.dev](https://c2ml.l3g3nd.dev) - publish your mod, browse the Mod DB
📖 **Docs online:** [c2ml.l3g3nd.dev/docs](https://c2ml.l3g3nd.dev/docs) - same guides as below, rendered as HTML
🔧 **The loader itself:** [CarX2ModLoader](https://github.com/l3g3nd2410/CarX2ModLoader) - native loader this SDK
targets

## Getting the SDK

Download the latest `C2ML-SDK.zip` from this repository's
[Releases](https://github.com/l3g3nd2410/C2ML-SDK/releases) page and extract it. It is the same package offered
for download from your [developer space](https://c2ml.l3g3nd.dev/dev) once your access is approved.

## Contents

```
lib/
  C2ML.API.dll        Public API - reference this in every mod
  0Harmony.dll          HarmonyX - only needed if you patch game code
docs/
  en/                  English guides (primary)
  fr/                  Guides en français
    01-getting-started.md       Install, first mod, local testing
    02-mod-lifecycle.md         BaseMod, [C2MLMeta], lifecycle, sub-pages
    03-menu-widgets.md          Full reference for every menu widget
    04-keybinds.md              Player-configurable keybinds
    05-config.md                Persistent settings
    06-logging.md               Writing to loader.log
    07-harmony-patching.md      Patching game code
    08-publishing.md            Publishing, updates, test versions
examples/
  C2ML.ExampleMod/            Reference mod using the WHOLE API (source + published .c2m)
```

Start with [docs/en/01-getting-started.md](docs/en/01-getting-started.md) (or
[docs/fr/01-getting-started.md](docs/fr/01-getting-started.md)).

`C2ML.ExampleMod` is a demo mod, not a real one: every API function has a minimal example there you can
copy-paste directly. Its source (`examples/C2ML.ExampleMod/ExampleMod.cs`) is the most complete reference, meant
to be read alongside the guides.

These same guides are also available online, rendered as HTML with syntax-highlighted code, on the C2ML site's
Documentation section (`/docs`) - English by default, with a French toggle.

## License

`C2ML.API.dll` is distributed here solely so third parties can build mods against it; the API itself is closed
source, all rights reserved. `0Harmony.dll` is [HarmonyX](https://github.com/BepInEx/HarmonyX) by the BepInEx
team, © BepInEx, included unmodified under its own MIT license.
