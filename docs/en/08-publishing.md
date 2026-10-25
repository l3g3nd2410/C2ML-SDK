# Publishing your mod

Publish through `/dev` to encrypt your mod for the loader (never as a plain `.dll`, see
[01-getting-started.md](01-getting-started.md)) - as a test version while you're iterating, or as a real release
once it's ready to reach other players, auto-updated and optionally listed in the loader's Mod DB.

## Becoming a developer

Go to `/dev` (sign in with Steam) and send an access request with a short message explaining what you plan to
publish. An administrator reviews the request manually - it may take a moment, it isn't automatic.

## Creating a mod

Once approved, "New mod" on `/dev` asks for:

| Field | Role |
|---|---|
| **Name** | Shown everywhere (3 to 40 characters) |
| **Description** | Shown on the mod's About page in the loader once published |
| **Visibility** | `Public` (installable by anyone, see Listed below for discoverability) or `Private` (invisible and uninstallable except for you and an access list you choose) |
| **Listed in the Mod DB** | Separate from visibility: a `Public` mod is only shown in the loader's Mod DB when also **listed**. An unlisted public mod stays installable by anyone who already knows its id/link, just not browsable - `Public` alone does **not** put it in the Mod DB |
| **Automatic updates** | Players who have this mod automatically receive your latest published version as soon as you publish it on the server |
| **Access list** (private mods) | SteamID64s of the players allowed to see/install this mod, in addition to you |

## Publishing a version

On your mod's page, Versions section: give a version number (e.g. `1.0.0`), pick your compiled DLL, click
Upload. Encryption happens entirely in your browser - the server never sees the DLL in plain form, only the
already-encrypted `.c2m` package (see below for admin inspection, an explicit and logged exception to this).

From the first published version, your mod becomes installable from the loader's Mod DB (if public and listed)
or for players on your access list (if private).

## Test versions

To publish a version without distributing it to everyone right away (work in progress, a feature to test before
a real release): check **"Test version"** when uploading - no need to pick a number, a single test slot is
reserved per mod. Publishing a new test version **overwrites** the previous one instead of piling up attempts.

A test version is **never** distributed by the Mod DB or automatic updates until it's promoted. To make it the
active one:

**Promote to active** (button on the test version's row) - it's automatically renamed to the version right
after your current active one (e.g. active `1.2.0` → promoted becomes `1.2.1`) and becomes the version actually
distributed.

## Deleting a version

A version that is **not** the active one (an old version you never cleaned up, or a test version you no longer
want) can be permanently deleted from `/dev` - encrypted files included. The active version can't be deleted
directly: promote another version to replace it first.

Only the last 3 versions are kept automatically anyway (older ones are cleaned up on their own on every new
publish, except the active version, always protected even if it's older than the last 3).

## Revoking a version

If a published version causes a problem (serious bug, regression), **Revoke** cuts it off immediately for every
player (no decryption key distributed anymore) without deleting it - reversible, useful while you publish a fix.

## Private mods

A private mod doesn't appear in any public catalog and can only be installed by you and the players on your
access list (SteamID64). Handy for a closed beta, or something reserved for a small group.

## Admin inspection

Server administrators can, for any published mod (private ones included), download the plain DLL for moderation
purposes (checking that a reported mod doesn't do anything malicious), as well as the final `.c2m` package to
test it directly in game - every inspection is logged. This is the only exception to the end-to-end encryption
described above.
