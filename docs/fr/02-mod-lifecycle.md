# BaseMod et le cycle de vie d'un mod

Toute classe de mod hérite de `C2ML.API.Core.BaseMod` et porte l'attribut `[C2MLMeta(...)]`.

## `[C2MLMeta]`

```csharp
[C2MLMeta("Mon Mod", "1.0.0", "TonPseudo")]
public class MonMod : BaseMod { ... }
```

| Paramètre | Rôle |
|---|---|
| `name` | Nom affiché dans le menu du loader (Manage mods, About, Mod DB...) |
| `version` | Utilisée **seulement** pour un mod non protégé (`.dll` directement dans le dossier `mods`, sans passer par le serveur). Une fois le mod publié et installé depuis la Mod DB, le loader affiche la version connue du **serveur** à la place - pas besoin de la synchroniser manuellement à chaque publication. |
| `author` | Pseudo affiché à côté du nom ("by TonPseudo"). Libre, non vérifié. |

La **description** et le **SteamID vérifié de l'auteur** ne se déclarent pas ici : ils viennent automatiquement
du catalogue du serveur une fois le mod publié (voir [08-publishing.md](08-publishing.md)), remplis depuis ta
fiche de mod sur `/dev`. Pour un mod testé uniquement en local (jamais publié), le loader affiche simplement
"No description."

## Membres hérités de `BaseMod`

| Membre | Rôle |
|---|---|
| `Logger` | Logger préfixé automatiquement avec le nom du mod (voir [06-logging.md](06-logging.md)) |
| `Config` | Réglages persistants clé/valeur (voir [05-config.md](05-config.md)) |
| `Meta` | L'attribut `[C2MLMeta]` lu en mémoire (`Meta.Name`, `Meta.Author`...) |
| `ModAssembly` | L'assembly .NET de ton mod (`Assembly.GetExecutingAssembly()` équivalent) |
| `CurrentSubPage` | Sous-page actuellement affichée - voir plus bas |

## Méthodes à surcharger

Toutes sont `virtual` : ne surcharge que celles dont tu as besoin.

### `OnLoad()`

Appelée une fois, quand le mod est chargé (au démarrage du jeu, ou immédiatement s'il est activé depuis Manage
mods pendant que le jeu tourne). C'est l'endroit pour lire ta config, enregistrer tes touches, poser tes patches
Harmony.

```csharp
public override void OnLoad()
{
    _counter = Config.GetInt("counter", 0);
    KeybindManager.RegisterKeybind(Meta.Name, "Mon action", (int)VirtualKey.F6);
    Logger.Log("Chargé, compteur = " + _counter);
}
```

### `OnUnload()`

Appelée quand le mod est désactivé pendant que le jeu tourne (depuis Manage mods), ou que l'accès au loader est
coupé par le serveur. **Pas appelée à la fermeture normale du jeu.** Désenregistre ici tout ce qui doit l'être
explicitement - typiquement tes patches Harmony (voir [07-harmony-patching.md](07-harmony-patching.md)).

```csharp
public override void OnUnload()
{
    _harmony?.UnpatchSelf();
}
```

### `ShowMenu()`

Appelée à chaque frame pendant que la page de ton mod est affichée dans le menu du loader (joueur sur l'onglet
de ton mod, menu ouvert). C'est ici que tu dessines ton interface avec `ImGuiAPI` (voir
[03-menu-widgets.md](03-menu-widgets.md)).

```csharp
public override void ShowMenu()
{
    ImGuiAPI.Text("Compteur : " + _counter);
    if (ImGuiAPI.Button("Incrémenter"))
    {
        _counter++;
        Config.Set("counter", _counter);
    }
}
```

Le loader affiche déjà l'en-tête (icône, nom, auteur, statut) et les onglets "About"/"Keybinds" tout seul :
`ShowMenu()` ne dessine QUE le contenu propre à ton mod, pas d'en-tête à répéter.

### `OnMenuUpdate()`

Déclarée dans l'API pour un usage futur, **pas encore appelée par le loader actuellement** - ne t'y fie pas pour
l'instant. Pour une logique qui doit tourner à chaque frame indépendamment du menu (même quand le joueur n'est
pas dessus), utilise un patch Harmony sur une méthode du jeu qui tourne déjà chaque frame (voir
[07-harmony-patching.md](07-harmony-patching.md)), ou vérifie une touche avec `KeybindManager.IsKeyPressed`
directement dans `ShowMenu()`.

### `GetModIcon()`

Icône affichée dans le menu (Manage mods, About, Mod DB). Renvoie les octets d'un PNG, ou `null` (ou ne la
surcharge pas du tout) pour l'icône par défaut du loader.

```csharp
public override byte[] GetModIcon()
{
    using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MonMod.icon.png");
    if (s == null) return null!;
    using var ms = new MemoryStream();
    s.CopyTo(ms);
    return ms.ToArray();
}
```

Le PNG doit être embarqué dans la DLL (`<EmbeddedResource Include="Resources\icon.png"
LogicalName="MonMod.icon.png" />` dans le `.csproj`), pas livré comme fichier séparé.

### `GetModGUID()`

Identifiant unique utilisé par le loader pour éviter de charger le même mod deux fois. Par défaut, c'est le nom
complet de ta classe .NET (`GetType().FullName`) - suffisant tant que tu ne renommes jamais ta classe ou ton
namespace. Si tu veux un identifiant stable même après un renommage de code (recommandé si tu publies ton mod),
surcharge-la avec une chaîne fixe choisie par toi :

```csharp
public override string GetModGUID() => "com.tonpseudo.monmod";
```

C'est aussi l'identifiant conventionnellement utilisé comme `id` Harmony (voir
[07-harmony-patching.md](07-harmony-patching.md)).

## Sous-pages (`CurrentSubPage`)

Un mod avec plusieurs pages (réglages de base + réglages avancés, par exemple) n'a pas besoin de dessiner son
propre système d'onglets : le loader s'en charge via `CurrentSubPage`. Chaque sous-page est juste une autre
méthode C# ordinaire.

```csharp
public override void ShowMenu()
{
    if (CurrentSubPage == "Advanced")
    {
        ShowAdvancedPage();
        return;
    }

    ImGuiAPI.Text("Page principale");
    if (ImGuiAPI.NavButton("Réglages avancés ->"))
        CurrentSubPage = "Advanced";
}

private void ShowAdvancedPage()
{
    ImGuiAPI.Text("Page séparée, poussée depuis la page principale.");
    // Pas besoin de redessiner un en-tête ou un bouton retour : le loader affiche déjà
    // "Mon Mod - Advanced" avec un bouton "< Mon Mod" tout seul, à partir de CurrentSubPage.
}
```

Le loader remet `CurrentSubPage` à `null` automatiquement quand le joueur clique sur son propre bouton retour -
pas besoin de le gérer toi-même.

Pour plus de deux pages, une valeur de chaîne différente par page (`"Advanced"`, `"Stats"`, `"Credits"`...) et
un `switch` au début de `ShowMenu()` suffisent (voir `C2ML.ExampleMod` pour un exemple à trois sous-pages).
