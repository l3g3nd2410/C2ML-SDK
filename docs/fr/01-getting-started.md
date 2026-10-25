# Démarrer avec l'API C2ML

C2ML est le loader de mods pour CarX Drift Racing Online 2. Ce guide couvre la mise en place d'un projet de mod
de zéro, jusqu'à le voir tourner en jeu.

## Contenu du SDK

```
C2ML-SDK/
  lib/
    C2ML.API.dll        API publique - référence-la dans TOUS tes mods
    0Harmony.dll         HarmonyX - uniquement si tu patches du code du jeu (voir 07-harmony-patching.md)
  docs/                 ces guides
  examples/
    C2ML.ExampleMod/     mod de référence qui utilise TOUTE l'API, section par section
```

`C2ML.ExampleMod` n'est pas un vrai mod : c'est un recueil d'exemples à côté duquel lire ces guides. Chaque
widget, chaque fonction de l'API y a un exemple minimal que tu peux copier-coller directement dans ton propre mod.

## Pré-requis

- **.NET SDK 6.0** (`dotnet --version` doit afficher `6.x`)
- Le jeu installé avec C2ML (le dossier `C2ML\` doit exister à côté de l'exécutable du jeu)
- Visual Studio, Rider, ou simplement un éditeur de texte + `dotnet build`

## Créer un projet de mod

```bash
dotnet new classlib -n MonMod -f net6.0
cd MonMod
```

Édite `MonMod.csproj` pour référencer `C2ML.API.dll` (adapte les chemins à ton installation) :

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="C2ML.API">
      <HintPath>chemin\vers\C2ML-SDK\lib\C2ML.API.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>

</Project>
```

`<Private>false</Private>` est important : il évite que `C2ML.API.dll` soit copié à côté de ta propre DLL dans le
dossier de build (le loader fournit déjà sa propre copie à l'exécution - en copier une deuxième ne sert à rien
et peut semer la confusion si les versions divergent un jour).

Si ton mod a besoin d'appeler des méthodes du jeu lui-même (types Unity, classes du jeu...), ajoute aussi les
DLL d'interop du jeu, présentes dans son dossier `C2ML\cache\unity-libs\` et générées automatiquement au premier
lancement du loader :

```xml
<Reference Include="UnityEngine">
  <HintPath>...\CarX Drift Racing Online 2\C2ML\cache\unity-libs\UnityEngine.dll</HintPath>
  <Private>false</Private>
</Reference>
<Reference Include="UnityEngine.CoreModule">
  <HintPath>...\CarX Drift Racing Online 2\C2ML\cache\unity-libs\UnityEngine.CoreModule.dll</HintPath>
  <Private>false</Private>
</Reference>
```

## Le strict minimum

```csharp
using C2ML.API.Attributes;
using C2ML.API.Core;
using C2ML.API.Menu;

namespace MonMod
{
    [C2MLMeta("Mon Mod", "1.0.0", "TonPseudo")]
    public class MonMod : BaseMod
    {
        public override void OnLoad()
        {
            Logger.Log("Mon mod a démarré !");
        }

        public override void ShowMenu()
        {
            ImGuiAPI.Text("Bonjour depuis mon mod !");
        }
    }
}
```

C'est tout ce qu'il faut pour un mod valide : une classe qui hérite de `BaseMod`, décorée de `[C2MLMeta(...)]`.
Le reste (`OnLoad`, `ShowMenu`, etc.) est détaillé dans [02-mod-lifecycle.md](02-mod-lifecycle.md).

## Tester ton mod

C2ML est conçu pour que le loader n'exécute jamais que du code passé par le serveur : un paquet `.c2m` chiffré
réellement publié. Déposer une `.dll` brute directement dans `C2ML\mods\` n'est **pas** une méthode de test
fiable - que ça charge ou non dépend de la politique de liste blanche du serveur, et sur un serveur où cette
politique est appliquée (comme elle devrait l'être), une DLL non reconnue est tout simplement ignorée, exactement
comme pour un joueur qui tenterait de charger quelque chose de non approuvé. Cette protection est tout l'intérêt
de C2ML : ne compte pas sur ce chemin, et ne le présente jamais à qui que ce soit comme une façon de faire
tourner un mod - ce n'est pas une voie de distribution reconnue.

La façon prévue d'itérer rapidement, même sur un mod pas encore fini, c'est une **version de test** : publie-la
via `/dev` en cochant "Version de test" (voir [08-publishing.md](08-publishing.md)). Elle est chiffrée comme
n'importe quelle autre version, mais jamais distribuée aux autres joueurs tant que tu ne l'as pas explicitement
promue, et republier écrase le précédent essai de test - tu peux donc itérer presque aussi vite qu'avec une
compilation locale, sans jamais passer par le chemin de la DLL en clair.

Une fois ton mod prêt pour d'autres joueurs, publie une vraie version (pas de test) de la même façon, et
liste-la dans la Mod DB si tu le souhaites (voir [08-publishing.md](08-publishing.md) pour la visibilité et le
listage).

## Pour aller plus loin

| Guide | Contenu |
|---|---|
| [02-mod-lifecycle.md](02-mod-lifecycle.md) | `BaseMod`, `[C2MLMeta]`, cycle de vie, sous-pages, icône |
| [03-menu-widgets.md](03-menu-widgets.md) | Référence complète de chaque widget du menu (`ImGuiAPI`) |
| [04-keybinds.md](04-keybinds.md) | Touches configurables par le joueur |
| [05-config.md](05-config.md) | Sauvegarde de réglages persistants |
| [06-logging.md](06-logging.md) | Écrire dans `loader.log` |
| [07-harmony-patching.md](07-harmony-patching.md) | Patcher du code du jeu avec Harmony |
| [08-publishing.md](08-publishing.md) | Publier, mettre à jour, versions de test |
