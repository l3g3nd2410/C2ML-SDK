# BaseMod and a mod's lifecycle

Every mod class inherits from `C2ML.API.Core.BaseMod` and carries the `[C2MLMeta(...)]` attribute.

## `[C2MLMeta]`

```csharp
[C2MLMeta("My Mod", "1.0.0", "YourName")]
public class MyMod : BaseMod { ... }
```

| Parameter | Role |
|---|---|
| `name` | Name shown in the loader's menu (Manage mods, About, Mod DB...) |
| `version` | Used **only** for an unprotected mod (`.dll` dropped directly into the `mods` folder, never going through the server). Once the mod is published and installed from the Mod DB, the loader shows the version known by the **server** instead - no need to keep it in sync by hand on every publish. |
| `author` | Name shown next to the mod ("by YourName"). Free text, not verified. |

The **description** and the author's **verified SteamID** aren't declared here: they come automatically from the
server's catalog once the mod is published (see [08-publishing.md](08-publishing.md)), filled in from your mod's
page on `/dev`. For a mod only ever tested locally (never published), the loader just shows "No description."

## Members inherited from `BaseMod`

| Member | Role |
|---|---|
| `Logger` | Logger automatically prefixed with your mod's name (see [06-logging.md](06-logging.md)) |
| `Config` | Persistent key/value settings (see [05-config.md](05-config.md)) |
| `Meta` | The `[C2MLMeta]` attribute read into memory (`Meta.Name`, `Meta.Author`...) |
| `ModAssembly` | Your mod's .NET assembly (equivalent to `Assembly.GetExecutingAssembly()`) |
| `CurrentSubPage` | Currently shown sub-page - see below |

## Methods to override

All of them are `virtual`: only override the ones you need.

### `OnLoad()`

Called once, when the mod is loaded (at game startup, or immediately if it's enabled from Manage mods while the
game is running). This is where you read your config, register your keybinds, and set up your Harmony patches.

```csharp
public override void OnLoad()
{
    _counter = Config.GetInt("counter", 0);
    KeybindManager.RegisterKeybind(Meta.Name, "My action", (int)VirtualKey.F6);
    Logger.Log("Loaded, counter = " + _counter);
}
```

### `OnUnload()`

Called when the mod is disabled while the game is running (from Manage mods), or when the loader's access is
revoked by the server. **Not called on a normal game shutdown.** Unregister here anything that needs it
explicitly - typically your Harmony patches (see [07-harmony-patching.md](07-harmony-patching.md)).

```csharp
public override void OnUnload()
{
    _harmony?.UnpatchSelf();
}
```

### `ShowMenu()`

Called every frame while your mod's page is shown in the loader's menu (player on your mod's tab, menu open).
This is where you draw your interface with `ImGuiAPI` (see [03-menu-widgets.md](03-menu-widgets.md)).

```csharp
public override void ShowMenu()
{
    ImGuiAPI.Text("Counter: " + _counter);
    if (ImGuiAPI.Button("Increment"))
    {
        _counter++;
        Config.Set("counter", _counter);
    }
}
```

The loader already draws the header (icon, name, author, status) and the "About"/"Keybinds" tabs on its own:
`ShowMenu()` only draws content specific to your mod, no header to repeat.

### `OnMenuUpdate()`

Declared in the API for future use, **not currently called by the loader** - don't rely on it yet. For logic
that needs to run every frame regardless of the menu being open, use a Harmony patch on a game method that
already runs every frame (see [07-harmony-patching.md](07-harmony-patching.md)), or check a key with
`KeybindManager.IsKeyPressed` directly inside `ShowMenu()`.

### `GetModIcon()`

Icon shown in the menu (Manage mods, About, Mod DB). Returns the bytes of a PNG, or `null` (or don't override it
at all) for the loader's default icon.

```csharp
public override byte[] GetModIcon()
{
    using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyMod.icon.png");
    if (s == null) return null!;
    using var ms = new MemoryStream();
    s.CopyTo(ms);
    return ms.ToArray();
}
```

The PNG must be embedded in the DLL (`<EmbeddedResource Include="Resources\icon.png"
LogicalName="MyMod.icon.png" />` in the `.csproj`), not shipped as a separate file.

### `GetModGUID()`

Unique identifier the loader uses to avoid loading the same mod twice. By default, it's your .NET class's full
name (`GetType().FullName`) - good enough as long as you never rename your class or namespace. If you want an
identifier that stays stable even after a code rename (recommended if you publish your mod), override it with a
fixed string of your choosing:

```csharp
public override string GetModGUID() => "com.yourname.mymod";
```

It's also the identifier conventionally used as the Harmony `id` (see
[07-harmony-patching.md](07-harmony-patching.md)).

## Sub-pages (`CurrentSubPage`)

A mod with several pages (basic settings + advanced settings, for example) doesn't need to draw its own tab
system: the loader handles it through `CurrentSubPage`. Each sub-page is just another ordinary C# method.

```csharp
public override void ShowMenu()
{
    if (CurrentSubPage == "Advanced")
    {
        ShowAdvancedPage();
        return;
    }

    ImGuiAPI.Text("Main page");
    if (ImGuiAPI.NavButton("Advanced settings ->"))
        CurrentSubPage = "Advanced";
}

private void ShowAdvancedPage()
{
    ImGuiAPI.Text("A separate page, pushed from the main page.");
    // No need to redraw a header or back button: the loader already shows
    // "My Mod - Advanced" with a "< My Mod" back button on its own, from CurrentSubPage.
}
```

The loader resets `CurrentSubPage` back to `null` automatically when the player clicks its own back button - no
need to handle that yourself.

For more than two pages, a different string value per page (`"Advanced"`, `"Stats"`, `"Credits"`...) and a
`switch` at the start of `ShowMenu()` are enough (see `C2ML.ExampleMod` for an example with three sub-pages).
