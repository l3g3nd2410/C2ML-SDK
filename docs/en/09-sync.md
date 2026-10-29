# Talking to other players' mods (`Sync`)

Every mod gets a `Sync` property (on `BaseMod`, already scoped to your own mod - see
[02-mod-lifecycle.md](02-mod-lifecycle.md)) for three things: sending small binary messages to other players
running the same mod, reading who's in the current room, and reading/writing small "user variables" visible to
everyone in the room. All three work through the game's own SmartFox connection for the room you're currently
in - no server of your own needed, and nothing to set up beyond calling these methods.

Full working example: `ShowSyncPage()` in `C2ML.ExampleMod\ExampleMod.cs`.

## Contents

- [Messages between mods](#messages-between-mods)
- [Who's in the room](#whos-in-the-room)
- [User variables](#user-variables)

## Messages between mods

Register a handler for an action, then send to the whole room or to specific players:

```csharp
public override void OnLoad()
{
    Sync.Register("ping", OnPingReceived);
}

private void OnPingReceived(byte[] payload, int senderNetPlayerId)
{
    int value = BitConverter.ToInt32(payload, 0);
    Logger.Log("Ping from " + senderNetPlayerId + ": " + value);
}

private void SendPing(int value)
{
    Sync.Send("ping", BitConverter.GetBytes(value));                              // whole room
    Sync.SendTo("ping", BitConverter.GetBytes(value), targetId1, targetId2);      // only these players
}
```

- `action` is a string you make up (`"ping"` above): it scopes your handler so it only ever receives messages
  *your own mod* sent with that exact action - never another mod's traffic, even though every mod shares the
  same underlying game channel.
- `payload` is raw bytes - serialize whatever you need (`BitConverter`, a small struct, JSON...).
- `senderNetPlayerId` on the handler is a SmartFox id (room-scoped), usable straight back into `SendTo` or
  `Sync.GetUserVariable`.
- **Only players running this exact mod receive anything** - nothing is sent to or interpreted by anyone else.
- The game's binary channel has no concept of a native unicast: `SendTo` is really a broadcast that each client
  filters locally. Don't rely on it for anything secret - any player could in theory see the raw bytes.

## Who's in the room

```csharp
int count = Sync.GetRoomPlayerCount();      // reliable, correct even for players already in the room when you joined
RoomPlayer[] players = Sync.GetRoomPlayers();   // id + name for each
int[] ids = Sync.GetRoomPlayerIds();        // ids only
```

`RoomPlayer` is `{ int Id; string Name; }`. `Name` can contain the game's own color tags
(`<color=#rrggbb>...</color>`) - see [03-menu-widgets.md#colored-text](03-menu-widgets.md#colored-text) to render
them with `ImGuiAPI.ColoredText`, or strip them with `ImGuiAPI.StripColorTags` for a plain label (for example
inside a `Dropdown`, which can't render per-segment colors).

Each of these makes a real native call: fine occasionally (e.g. once when a menu page is drawn), but avoid
calling them every single frame for a long player list without caching the result yourself - see the warning
below.

## User variables

A small key/value flag or counter attached to *your own* player, visible to anyone who asks for it. Useful for
"who else has this mod" style checks: other mods' variables (and the game's own) are never mixed in, you only
ever see the ones your own mod set.

```csharp
// once, e.g. in OnLoad - the loader resends it automatically on every room you join, even with the menu closed
Sync.DeclareUserVariable("has_mod", true);

// read someone else's (or your own) variable - bool/int/double/string
object value = Sync.GetUserVariable(somePlayerId, "has_mod");
bool hasMod = value is bool b && b;

// everything THIS mod has declared for that player
IReadOnlyDictionary<string, object> all = Sync.GetUserVariables(somePlayerId);
```

- **`DeclareUserVariable`** is the one to reach for in almost every case: call it once (bool/int/double/string
  overloads), and the loader keeps it set across rooms on its own - sent again on every room you join, and
  resent automatically if it ever notices it's missing (desync, late connection...). You never have to call it
  again yourself; calling it again with a new value just updates what gets kept in sync.
- **`SetUserVariable`** (same four overloads) sends once, immediately, with no "keep this set" guarantee -
  `DeclareUserVariable` covers that case too (and more), so there's rarely a reason to use `SetUserVariable`
  directly instead.
- Supported value types: `bool`, `int`, `double`, `string`. A value set with another type, or never set at all,
  reads back as `null`.
- There's a short, normal network delay between setting a variable and other players (including yourself)
  seeing the new value - it isn't instant.
- Reads (`GetUserVariable`/`GetUserVariables`) make a native call each time, same as the room roster above.

**Performance warning**: `GetRoomPlayers()`, `GetUserVariable(s)` and friends each make a real call into the
game's networking layer. Calling them every single frame (for example directly inside `ShowMenu()` for a long
player list) is the kind of pattern that has caused real, hard crashes in testing (not just a slowdown). Cache
the result yourself and refresh it every second or two instead - see `_hasModCache`/`_hasModRefreshTimer` in
`ShowSyncPage()` (`C2ML.ExampleMod\ExampleMod.cs`) for the exact pattern to copy.

## Where to go next

| Guide | Covers |
|---|---|
| [03-menu-widgets.md](03-menu-widgets.md) | `ImGuiAPI.ColoredText`/`StripColorTags` for player names |
| [10-advanced-native-access.md](10-advanced-native-access.md) | Last-resort low-level field access (`NativeApi`) |
