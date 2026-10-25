# Touches configurables (`KeybindManager`)

`C2ML.API.Keybinds.KeybindManager` donne à ton mod une touche que le joueur peut reconfigurer lui-même depuis
le menu **Keybinds** du loader, sans que tu aies à dessiner quoi que ce soit pour ça.

## Enregistrer une touche

Une fois, dans `OnLoad()` :

```csharp
using C2ML.API.Keybinds;

public override void OnLoad()
{
    KeybindManager.RegisterKeybind(Meta.Name, "Activer le nitro", (int)VirtualKey.F6);
}
```

- `Meta.Name` : utilise le nom de ton mod comme premier paramètre - regroupe naturellement tes actions sous ton
  mod dans le menu Keybinds du loader.
- `"Activer le nitro"` : nom de l'action, affiché au joueur. Un mod avec plusieurs actions appelle
  `RegisterKeybind` plusieurs fois avec un nom différent à chaque fois.
- `(int)VirtualKey.F6` : touche par défaut, **utilisée seulement la toute première fois** (aucune touche
  sauvegardée pour cette action). Ensuite, la valeur choisie par le joueur prend le dessus automatiquement, y
  compris entre deux sessions.

`VirtualKey` est une énumération pratique des touches les plus courantes (lettres, F1-F12, Espace, Entrée,
Tab, Échap, Ctrl/Shift/Alt gauche et droite, boutons de souris...). Tu peux aussi passer n'importe quel code de
touche virtuelle Windows directement en `int` si tu as besoin d'une touche qui n'y figure pas.

## Lire la touche actuelle et détecter l'appui

```csharp
public override void ShowMenu()
{
    int touche = KeybindManager.GetKeybind(Meta.Name, "Activer le nitro");
    ImGuiAPI.Text("Touche actuelle : VK " + touche);

    if (KeybindManager.IsKeyPressed(touche))
    {
        ImGuiAPI.Badge("Pressée !", BadgeKind.Ok);
    }
}
```

`IsKeyPressed` renvoie `true` tant que la touche est maintenue enfoncée (pas seulement au moment de l'appui) -
classique pour un "appuie pour activer X". Elle ne dépend pas du menu ouvert ou non en elle-même, mais tu ne
peux l'appeler que depuis du code qui s'exécute chaque frame côté menu (`ShowMenu()`) : voir la limite
d'`OnMenuUpdate()` dans [02-mod-lifecycle.md](02-mod-lifecycle.md) pour une détection indépendante du menu
(utilise alors un patch Harmony sur une méthode du jeu qui tourne déjà chaque frame).

## Ce que le joueur voit

Dès `RegisterKeybind` appelé, l'action apparaît automatiquement dans l'onglet **Keybinds** de ton mod (menu du
loader), avec un bouton pour que le joueur choisisse sa propre touche. Tu n'as rien de plus à dessiner toi-même
pour ça - `GetKeybind`/`IsKeyPressed` reflètent toujours le choix du joueur.

## Fonctions avancées (rarement nécessaires)

- `KeybindManager.SetKeybind(modName, action, key)` - change la touche par programme. C'est ce que l'écran
  Keybinds du loader appelle lui-même quand le joueur choisit une touche ; un mod n'a normalement pas besoin de
  l'appeler (laisse le joueur configurer via l'écran standard, plus cohérent pour tous les mods).
- `KeybindManager.LoadKeybinds(modName)` / `SaveKeybinds(modName)` - relecture/écriture immédiate du fichier de
  touches. Inutile en pratique : `RegisterKeybind` lit déjà la valeur sauvegardée automatiquement au chargement,
  et tout changement via l'écran Keybinds est sauvegardé tout seul dès qu'il a lieu.
