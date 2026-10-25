# Persistent settings (`Config`)

Every mod has access to `Config` (a property inherited from `BaseMod`, of type `ModConfig`): key/value settings
saved automatically to an encrypted file in the `C2ML\config\` folder, read back on the next launch.

## Reading a value (with a default)

```csharp
public override void OnLoad()
{
    int counter = Config.GetInt("counter", 0);
    float sensitivity = Config.GetFloat("sensitivity", 1.0f);
    bool optionEnabled = Config.GetBool("option_enabled", false);
    string nickname = Config.GetString("nickname", "");
}
```

Each `GetXxx` returns the saved value if it exists, otherwise the default given as the second parameter - never
throws, never needs a key-existence check beforehand.

## Writing a value

```csharp
Config.Set("counter", 42);
Config.Set("sensitivity", 1.5f);
Config.Set("option_enabled", true);
Config.Set("nickname", "YourName");
```

Writes happen in memory immediately, then the file is **saved automatically within the following second** -
nothing to call after a `Set` in the common case.

## Immediate save

```csharp
Config.Save();
```

Forces the write to disk right away, without waiting for the automatic delay. Useful just before a sensitive
moment (for example right before `OnUnload()`, if you want to be sure a last-minute change is persisted before
the mod is unloaded).

## Checking whether a key exists

```csharp
if (Config.Has("first_time"))
{
    // already configured in the past
}
else
{
    Config.Set("first_time", true);
    // first time this mod runs for this player: show a welcome message, for example
}
```

## Common pattern: widget + config in one line

```csharp
if (ImGuiAPI.Checkbox("Option", ref _flag))
    Config.Set("option", _flag);
```

`ImGuiAPI` widgets return `true` only on the frame their value changes: combined with `Config.Set`, this saves
automatically as soon as the player touches the setting, with no unnecessary write every frame.

## Where is the data stored?

In an encrypted file per mod, in `C2ML\config\`. The format and encryption are handled entirely by the loader -
your mod never has to worry about the exact path or on-disk format, only about the keys and values it chooses
itself.

## Raw data (advanced)

For a format `ModConfig` doesn't cover (binary data, a complex structure you serialize yourself),
`C2ML.API.Config.ConfigManager` exposes more direct access:

```csharp
using C2ML.API.Config;

byte[] data = System.Text.Encoding.UTF8.GetBytes("any content at all");
ConfigManager.SaveConfig(Meta.Name, data);

byte[]? readBack = ConfigManager.LoadConfig(Meta.Name);
```

In the vast majority of cases, `Config.GetXxx`/`Config.Set` (via `ModConfig`, shown above) is plenty and much
simpler to use.
