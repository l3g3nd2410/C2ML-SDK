# Logging (`Logger`)

Anything your mod writes to its logs ends up in `loader.log` (the game's `C2ML\` folder), useful for debugging
an issue reported by a player, or during your own development.

## Your mod's logger (`this.Logger`)

From an instance method of your mod class (`OnLoad`, `ShowMenu`, one of your own methods...), use the `Logger`
property inherited from `BaseMod` - every line is automatically prefixed with your mod's name:

```csharp
public override void OnLoad()
{
    Logger.Log("Normal message");
    Logger.LogSuccess("Loading finished successfully");
    Logger.LogWarning("A setting looks inconsistent, falling back to default");
    Logger.LogError("Could not read the expected file");
}
```

In `loader.log`, this produces for example:

```
[My Mod] Normal message
[My Mod] Loading finished successfully
[My Mod] A setting looks inconsistent, falling back to default
[My Mod] Could not read the expected file
```

| Method | Level | When to use it |
|---|---|---|
| `Log` | normal | General information, tracking what's happening |
| `LogSuccess` | success | An operation finished well (load, save...) |
| `LogWarning` | warning | Something abnormal but non-blocking (default value used, optional feature unavailable...) |
| `LogError` | error | An operation genuinely failed |

## The global logger (`C2ML.API.Logging.Logger`)

From a **static** context - typically a Harmony patch class, which has no instance of your mod (see
[07-harmony-patching.md](07-harmony-patching.md)) - `this.Logger` doesn't exist. Use the global logger instead,
same API but without an automatic prefix (add your own if you want to identify the source):

```csharp
using C2ML.API.Logging;

private static class MyPatch
{
    private static void Postfix()
    {
        Logger.Log("[My Mod] Patch triggered");
    }
}
```

## Summary

- Inside an instance method of your mod → `Logger.Log(...)` (the one inherited from `BaseMod`, auto-prefixed)
- Inside a static class (Harmony patch, static utility method...) → `C2ML.API.Logging.Logger.Log(...)`
  (prefix yourself if needed, e.g. `"[My Mod] ..."`)
