# Journalisation (`Logger`)

Tout ce que ton mod écrit dans ses logs se retrouve dans `loader.log` (dossier `C2ML\` du jeu), utile pour
diagnostiquer un problème signalé par un joueur ou pendant ton propre développement.

## Le logger de ton mod (`this.Logger`)

Depuis une méthode d'instance de ta classe de mod (`OnLoad`, `ShowMenu`, une méthode à toi...), utilise la
propriété `Logger` héritée de `BaseMod` - chaque ligne est automatiquement préfixée avec le nom de ton mod :

```csharp
public override void OnLoad()
{
    Logger.Log("Message normal");
    Logger.LogSuccess("Chargement terminé avec succès");
    Logger.LogWarning("Un réglage semble incohérent, valeur par défaut utilisée");
    Logger.LogError("Impossible de lire le fichier attendu");
}
```

Dans `loader.log`, ça donne par exemple :

```
[Mon Mod] Message normal
[Mon Mod] Chargement terminé avec succès
[Mon Mod] Un réglage semble incohérent, valeur par défaut utilisée
[Mon Mod] Impossible de lire le fichier attendu
```

| Méthode | Niveau | Quand l'utiliser |
|---|---|---|
| `Log` | normal | Information générale, suivi du déroulement |
| `LogSuccess` | succès | Une opération s'est bien terminée (chargement, sauvegarde...) |
| `LogWarning` | avertissement | Quelque chose d'anormal mais non bloquant (valeur par défaut utilisée, fonctionnalité optionnelle indisponible...) |
| `LogError` | erreur | Une opération a réellement échoué |

## Le logger global (`C2ML.API.Logging.Logger`)

Depuis un contexte **statique** - typiquement une classe de patch Harmony, qui n'a pas d'instance de ton mod
(voir [07-harmony-patching.md](07-harmony-patching.md)) - `this.Logger` n'existe pas. Utilise à la place le
logger global, avec la même API mais sans préfixe automatique (ajoute le tien à la main si tu veux identifier
la source) :

```csharp
using C2ML.API.Logging;

private static class MonPatch
{
    private static void Postfix()
    {
        Logger.Log("[Mon Mod] Patch déclenché");
    }
}
```

## Résumé

- Dans une méthode d'instance de ton mod → `Logger.Log(...)` (celui hérité de `BaseMod`, préfixé automatiquement)
- Dans une classe statique (patch Harmony, méthode utilitaire statique...) → `C2ML.API.Logging.Logger.Log(...)`
  (préfixe-toi-même si besoin, par exemple `"[Mon Mod] ..."`)
