# Getting started with the C2ML API

C2ML is the mod loader for CarX Drift Racing Online 2. This guide walks you through setting up a mod project
from scratch, all the way to seeing it run in game.

## What's in the SDK

```
C2ML-SDK/
  lib/
    C2ML.API.dll        Public API - reference this in ALL your mods
    0Harmony.dll          HarmonyX - only needed if you patch game code (see 07-harmony-patching.md)
  docs/                 these guides
  examples/
    C2ML.ExampleMod/     Reference mod that uses the WHOLE API, section by section
```

`C2ML.ExampleMod` isn't a real mod: it's a collection of examples to read alongside these guides. Every widget,
every API function has a minimal example there that you can copy-paste straight into your own mod.

## Prerequisites

- **.NET SDK 6.0** (`dotnet --version` should print `6.x`)
- The game installed with C2ML (a `C2ML\` folder must exist next to the game executable)
- Visual Studio, Rider, or just a text editor + `dotnet build`

## Create a mod project

```bash
dotnet new classlib -n MyMod -f net6.0
cd MyMod
```

Edit `MyMod.csproj` to reference `C2ML.API.dll` (adjust the path to your own install):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="C2ML.API">
      <HintPath>path\to\C2ML-SDK\lib\C2ML.API.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

`<Private>false</Private>` matters: it keeps `C2ML.API.dll` from being copied next to your own DLL in the build
output (the loader already provides its own copy at runtime - copying a second one serves no purpose and could
be confusing if the versions ever drift apart).

If your mod needs to call into the game itself (Unity types, game classes...), also add the game's interop
DLLs, found in its `C2ML\cache\unity-libs\` folder and generated automatically on the loader's first launch:

```xml
<Reference Include="UnityEngine">
  <HintPath>...\CarX Drift Racing Online 2\C2ML\cache\unity-libs\UnityEngine.dll</HintPath>
  <Private>false</Private>
</Reference>
<Reference Include="UnityEngine.CoreModule">
  <HintPath>...\CarX Drift Racing Online 2\C2ML\cache\unity-libs\UnityEngine.CoreModule.dll</HintPath>
  <Private>false</Private>
</Reference>
```

## The bare minimum

```csharp
using C2ML.API.Attributes;
using C2ML.API.Core;
using C2ML.API.Menu;

namespace MyMod
{
    [C2MLMeta("My Mod", "1.0.0", "YourName")]
    public class MyMod : BaseMod
    {
        public override void OnLoad()
        {
            Logger.Log("My mod has started!");
        }

        public override void ShowMenu()
        {
            ImGuiAPI.Text("Hello from my mod!");
        }
    }
}
```

That's all it takes for a valid mod: a class that inherits from `BaseMod`, decorated with `[C2MLMeta(...)]`.
The rest (`OnLoad`, `ShowMenu`, etc.) is covered in [02-mod-lifecycle.md](02-mod-lifecycle.md).

## Testing your mod

C2ML is built so the loader only ever runs code that went through the server: an encrypted `.c2m` package
that was actually published. Dropping a plain `.dll` straight into `C2ML\mods\` is **not** a supported way to
test it - whether that loads at all depends on the server's mod-allowlist policy, and on a server with that
policy enforced (as it should be) an unrecognized DLL is simply ignored, exactly like it would be for a player
trying to sideload something unapproved. That protection is the whole point of C2ML, so don't rely on this path
and never suggest it to anyone as a way to run a mod - it's not a sanctioned distribution route.

The supported way to iterate quickly, even on a mod that isn't finished yet, is a **test version**: publish it
through `/dev` with the "Test version" box checked (see [08-publishing.md](08-publishing.md)). It's encrypted
like any other release, but never distributed to other players until you explicitly promote it, and
republishing overwrites the previous test build - so you can iterate almost as fast as a local build, without
ever going near the plain-DLL path.

Once your mod is ready for other players, publish a real (non-test) version the same way, and optionally list
it in the Mod DB (see [08-publishing.md](08-publishing.md) for visibility and listing).

## Where to go next

| Guide | Covers |
|---|---|
| [02-mod-lifecycle.md](02-mod-lifecycle.md) | `BaseMod`, `[C2MLMeta]`, lifecycle, sub-pages, icon |
| [03-menu-widgets.md](03-menu-widgets.md) | Full reference for every menu widget (`ImGuiAPI`) |
| [04-keybinds.md](04-keybinds.md) | Player-configurable keybinds |
| [05-config.md](05-config.md) | Persisting settings |
| [06-logging.md](06-logging.md) | Writing to `loader.log` |
| [07-harmony-patching.md](07-harmony-patching.md) | Patching game code with Harmony |
| [08-publishing.md](08-publishing.md) | Publishing, updates, test versions |
