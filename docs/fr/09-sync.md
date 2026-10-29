# Communiquer avec les mods des autres joueurs (`Sync`)

Chaque mod dispose d'une propriété `Sync` (sur `BaseMod`, déjà liée à ton propre mod - voir
[02-mod-lifecycle.md](02-mod-lifecycle.md)) pour trois choses : envoyer de petits messages binaires aux autres
joueurs qui ont ce même mod, savoir qui est dans la room courante, et lire/écrire de petites "variables
utilisateur" visibles par tout le monde dans la room. Les trois passent par la connexion SmartFox du jeu pour la
room où tu es actuellement - pas besoin de ton propre serveur, rien à configurer en dehors de ces appels.

Exemple complet et fonctionnel : `ShowSyncPage()` dans `C2ML.ExampleMod\ExampleMod.cs`.

## Sommaire

- [Messages entre mods](#messages-entre-mods)
- [Qui est dans la room](#qui-est-dans-la-room)
- [Variables utilisateur](#variables-utilisateur)

## Messages entre mods

Enregistre un gestionnaire pour une action, puis envoie à toute la room ou à des joueurs précis :

```csharp
public override void OnLoad()
{
    Sync.Register("ping", OnPingReceived);
}

private void OnPingReceived(byte[] payload, int senderNetPlayerId)
{
    int valeur = BitConverter.ToInt32(payload, 0);
    Logger.Log("Ping de " + senderNetPlayerId + " : " + valeur);
}

private void EnvoyerPing(int valeur)
{
    Sync.Send("ping", BitConverter.GetBytes(valeur));                              // toute la room
    Sync.SendTo("ping", BitConverter.GetBytes(valeur), idCible1, idCible2);        // seulement ces joueurs
}
```

- `action` est une chaîne que tu choisis (`"ping"` ci-dessus) : elle limite ton gestionnaire pour qu'il ne reçoive
  QUE les messages envoyés par *ton propre mod* avec exactement cette action - jamais le trafic d'un autre mod,
  même si tous les mods partagent le même canal du jeu en coulisses.
- `payload` ce sont des octets bruts - sérialise ce dont tu as besoin (`BitConverter`, une petite structure,
  JSON...).
- `senderNetPlayerId` sur le gestionnaire est un id SmartFox (limité à la room), réutilisable directement dans
  `SendTo` ou `Sync.GetUserVariable`.
- **Seuls les joueurs qui ont exactement ce mod reçoivent quoi que ce soit** - rien n'est envoyé à, ni interprété
  par, personne d'autre.
- Le canal binaire du jeu n'a pas de notion d'envoi ciblé natif : `SendTo` est en réalité une diffusion que chaque
  client filtre lui-même localement. Ne compte pas dessus pour quelque chose de secret - n'importe quel joueur
  pourrait en théorie voir les octets bruts.

## Qui est dans la room

```csharp
int nombre = Sync.GetRoomPlayerCount();      // fiable, correct meme pour les joueurs deja presents en arrivant
RoomPlayer[] joueurs = Sync.GetRoomPlayers();   // id + nom de chacun
int[] ids = Sync.GetRoomPlayerIds();         // ids seuls
```

`RoomPlayer` est `{ int Id; string Name; }`. `Name` peut contenir les balises de couleur du jeu
(`<color=#rrggbb>...</color>`) - voir [03-menu-widgets.md#texte-coloré](03-menu-widgets.md#texte-coloré) pour les
afficher avec `ImGuiAPI.ColoredText`, ou retire-les avec `ImGuiAPI.StripColorTags` pour un libellé simple (par
exemple dans un `Dropdown`, qui ne peut pas afficher une couleur par segment).

Chacun de ces appels fait un vrai appel natif : d'accord ponctuellement (par exemple une fois par dessin d'une
page de menu), mais évite de les appeler à chaque frame pour une longue liste de joueurs sans mettre le résultat
en cache toi-même - voir l'avertissement plus bas.

## Variables utilisateur

Un petit indicateur ou compteur clé/valeur attaché à *ton propre* joueur, visible par quiconque le demande.
Pratique pour savoir "qui d'autre a ce mod" : les variables des autres mods (et celles du jeu) ne sont jamais
mélangées, tu ne vois que celles posées par ton propre mod.

```csharp
// une seule fois, ex. dans OnLoad - le loader la renvoie automatiquement a chaque room rejointe, meme menu ferme
Sync.DeclareUserVariable("has_mod", true);

// lit la variable d'un autre joueur (ou de toi-meme) - bool/int/double/string
object valeur = Sync.GetUserVariable(idJoueur, "has_mod");
bool aCeMod = valeur is bool b && b;

// tout ce que CE mod a declare pour ce joueur
IReadOnlyDictionary<string, object> tout = Sync.GetUserVariables(idJoueur);
```

- **`DeclareUserVariable`** est celle à utiliser dans presque tous les cas : appelle-la une fois (surcharges
  bool/int/double/string), et le loader la garde posée à travers les rooms tout seul - renvoyée à chaque room
  rejointe, et renvoyée automatiquement s'il remarque qu'elle n'y est plus (désync, connexion tardive...). Pas
  besoin de la rappeler toi-même ; un appel ultérieur avec une nouvelle valeur met juste à jour ce qui est
  maintenu synchronisé.
- **`SetUserVariable`** (mêmes quatre surcharges) envoie une seule fois, immédiatement, sans garantie de
  "maintenir posée" - `DeclareUserVariable` couvre aussi ce cas (et plus), donc il y a rarement une raison
  d'utiliser `SetUserVariable` directement.
- Types de valeur supportés : `bool`, `int`, `double`, `string`. Une valeur posée avec un autre type, ou jamais
  posée, se relit comme `null`.
- Il y a un court délai réseau normal entre le moment où une variable est posée et celui où les autres joueurs
  (toi-même inclus) voient la nouvelle valeur - ce n'est pas instantané.
- Les lectures (`GetUserVariable`/`GetUserVariables`) font un appel natif à chaque fois, comme le roster de la
  room ci-dessus.

**Avertissement de performance** : `GetRoomPlayers()`, `GetUserVariable(s)` et compagnie font chacun un vrai
appel dans la couche réseau du jeu. Les appeler à chaque frame (par exemple directement dans `ShowMenu()` pour
une longue liste de joueurs) est exactement le genre de schéma qui a causé de vrais crashs en test (pas
seulement un ralentissement). Mets le résultat en cache toi-même et rafraîchis-le toutes les une ou deux
secondes - voir `_hasModCache`/`_hasModRefreshTimer` dans `ShowSyncPage()` (`C2ML.ExampleMod\ExampleMod.cs`) pour
le schéma exact à copier.

## Pour aller plus loin

| Guide | Contenu |
|---|---|
| [03-menu-widgets.md](03-menu-widgets.md) | `ImGuiAPI.ColoredText`/`StripColorTags` pour les pseudos des joueurs |
| [10-advanced-native-access.md](10-advanced-native-access.md) | Accès bas niveau en dernier recours (`NativeApi`) |
