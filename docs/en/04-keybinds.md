# Configurable keybinds (`KeybindManager`)

`C2ML.API.Keybinds.KeybindManager` gives your mod a key the player can reconfigure themselves from the loader's
**Keybinds** menu, without you having to draw anything for it.

## Registering a key

Once, in `OnLoad()`:

```csharp
using C2ML.API.Keybinds;

public override void OnLoad()
{
    KeybindManager.RegisterKeybind(Meta.Name, "Activate nitro", (int)VirtualKey.F6);
}
```

- `Meta.Name`: use your mod's name as the first parameter - naturally groups your actions under your mod in the
  loader's Keybinds menu.
- `"Activate nitro"`: the action's name, shown to the player. A mod with several actions calls
  `RegisterKeybind` multiple times with a different name each time.
- `(int)VirtualKey.F6`: default key, **only used the very first time** (no key saved yet for this action). After
  that, the player's chosen value takes over automatically, including across sessions.

`VirtualKey` is a convenience enum of the most common keys (letters, F1-F12, Space, Enter, Tab, Escape, left and
right Ctrl/Shift/Alt, mouse buttons...). You can also pass any Windows virtual-key code directly as an `int` if
you need a key that isn't listed there.

## Reading the current key and detecting a press

```csharp
public override void ShowMenu()
{
    int key = KeybindManager.GetKeybind(Meta.Name, "Activate nitro");
    ImGuiAPI.Text("Current key: VK " + key);

    if (KeybindManager.IsKeyPressed(key))
    {
        ImGuiAPI.Badge("Pressed!", BadgeKind.Ok);
    }
}
```

`IsKeyPressed` returns `true` for as long as the key is held down (not just on the first press) - the usual
pattern for "hold to activate X". It doesn't depend on the menu being open by itself, but you can only call it
from code that runs every frame on the menu side (`ShowMenu()`): see the `OnMenuUpdate()` limitation in
[02-mod-lifecycle.md](02-mod-lifecycle.md) for menu-independent detection (use a Harmony patch on a game method
that already runs every frame instead).

## What the player sees

As soon as `RegisterKeybind` is called, the action automatically appears in your mod's **Keybinds** tab (loader
menu), with a button for the player to pick their own key. You don't need to draw anything more for that -
`GetKeybind`/`IsKeyPressed` always reflect the player's choice.

## Advanced functions (rarely needed)

- `KeybindManager.SetKeybind(modName, action, key)` - changes the key programmatically. This is what the
  loader's own Keybinds screen calls when the player picks a key; a mod doesn't normally need to call it (let
  the player configure it via the standard screen, more consistent across all mods).
- `KeybindManager.LoadKeybinds(modName)` / `SaveKeybinds(modName)` - explicit reload/write of the keybinds file.
  Rarely useful in practice: `RegisterKeybind` already reads the saved value automatically on load, and any
  change via the Keybinds screen is saved on its own as soon as it happens.
