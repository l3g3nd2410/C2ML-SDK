# Patcher du code du jeu avec Harmony

Les widgets de menu et la config couvrent l'interface et les réglages de ton mod, mais pour changer le
**comportement du jeu lui-même** (modifier une valeur calculée, intercepter un événement, empêcher une action...),
il faut patcher une méthode existante du jeu. C'est ce que fait [Harmony](https://harmony.pardeike.net/)
(`0Harmony.dll`, fourni dans `lib\` du SDK).

## Pourquoi ça marche sur un jeu IL2CPP

CarX Drift Racing Online 2 est un jeu Unity compilé en IL2CPP (code natif, pas de simple assembly .NET à lire
avec un décompilateur classique). Le loader génère au premier lancement des **assemblies d'interop** (dans
`C2ML\cache\unity-libs\` et `C2ML\interop\`) : des types C# qui ont la même forme que le code du jeu et qui
relaient chaque appel vers le vrai code natif. Harmony patche ces types d'interop normalement, comme n'importe
quel autre assembly .NET - la seule différence avec un jeu Mono classique est que le type patché n'est pas le
"vrai" code, mais un relais transparent vers lui.

## Référencer Harmony dans ton projet

```xml
<Reference Include="0Harmony">
  <HintPath>chemin\vers\C2ML-SDK\lib\0Harmony.dll</HintPath>
  <Private>false</Private>
</Reference>
```

## Poser et retirer les patches

```csharp
using HarmonyLib;
using System.Reflection;

public override void OnLoad()
{
    _harmony = new Harmony(GetModGUID());        // identifiant unique, voir 02-mod-lifecycle.md
    _harmony.PatchAll(Assembly.GetExecutingAssembly());
}

public override void OnUnload()
{
    _harmony?.UnpatchSelf();   // retire tous les patches posés par CETTE instance de Harmony
}

private Harmony? _harmony;
```

`PatchAll(assembly)` scanne ton assembly à la recherche de classes marquées `[HarmonyPatch(...)]` et applique
chacune automatiquement - pas besoin d'appeler `Patch()` manuellement pour chaque méthode.

**`OnUnload()` doit toujours retirer les patches posés.** Sans ça, le patch resterait actif dans le jeu même
après que le joueur ait désactivé ton mod depuis Manage mods, ce qui peut provoquer des comportements
inattendus ou des conflits avec d'autres mods.

## Écrire un patch

Une classe statique imbriquée par méthode patchée, avec `[HarmonyPatch(type, nomDeMembre)]` :

```csharp
[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.timeScale), MethodType.Setter)]
private static class TimeScalePatch
{
    private static void Postfix()
    {
        // s'exécute APRÈS que Time.timeScale ait été changé par le jeu : on observe, sans rien bloquer
        C2ML.API.Logging.Logger.Log("[Mon Mod] timeScale a changé");
    }
}
```

- `MethodType.Setter` cible le "set" d'une propriété (`MethodType.Getter` pour le "get"). Omets ce paramètre
  pour cibler une méthode normale par son nom.
- Une classe de patch est **statique** : elle n'a pas accès à une instance de ton mod. Pour logger ou toucher
  l'état de ton mod depuis un patch, utilise le logger global (voir [06-logging.md](06-logging.md)) et stocke
  l'état partagé dans un champ `static` de ta classe de mod.

### `Postfix` vs `Prefix`

| | S'exécute | Peut empêcher l'original | Usage typique |
|---|---|---|---|
| `Postfix` | Après la méthode d'origine | Non | Observer un résultat, le modifier légèrement (`ref __result`) |
| `Prefix` | Avant la méthode d'origine | Oui (`return false`) | Bloquer complètement un comportement, remplacer une valeur avant calcul |

```csharp
[HarmonyPatch(typeof(MaClasseDuJeu), "MaMethode")]
private static class MonPatch
{
    // Bloque l'exécution de l'originale si elle renvoie false
    private static bool Prefix()
    {
        return true;   // laisse l'original s'exécuter normalement
    }

    // Peut lire/modifier le résultat de l'originale
    private static void Postfix(ref bool __result)
    {
        // __result contient ce que l'originale a renvoyé ; le modifier ici change ce que les appelants reçoivent
    }
}
```

Une classe peut avoir `Prefix` et `Postfix` en même temps si besoin.

## Trouver la bonne méthode à patcher

Les assemblies d'interop générées par le loader (`C2ML\cache\unity-libs\`, `C2ML\interop\`) sont des DLL .NET
normales : ouvre-les avec [dnSpy](https://github.com/dnSpyEx/dnSpy) ou [ILSpy](https://github.com/icsharpcode/ILSpy)
pour explorer les classes et méthodes du jeu disponibles, exactement comme pour un mod Mono classique. C'est en
dehors du périmètre de ce SDK (spécifique au code du jeu, pas à l'API C2ML elle-même), mais c'est le point de
départ : repère le type et la méthode qui t'intéressent dans le décompilateur, puis écris le patch comme montré
ci-dessus avec ce type/cette méthode.

## Exemple complet

Voir `TimeScalePatch` dans `C2ML.ExampleMod\ExampleMod.cs` - un patch fonctionnel et sans danger (il observe
juste les changements de `Time.timeScale`, ne modifie rien) que tu peux lancer en jeu pour voir le mécanisme
marcher de bout en bout avant d'écrire ton propre patch sur une vraie méthode du jeu.
