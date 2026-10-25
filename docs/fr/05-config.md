# Réglages persistants (`Config`)

Chaque mod a accès à `Config` (propriété héritée de `BaseMod`, type `ModConfig`) : des valeurs clé/valeur
sauvegardées automatiquement dans un fichier chiffré du dossier `C2ML\config\`, relues au prochain lancement.

## Lire une valeur (avec valeur par défaut)

```csharp
public override void OnLoad()
{
    int compteur = Config.GetInt("compteur", 0);
    float sensibilite = Config.GetFloat("sensibilite", 1.0f);
    bool optionActivee = Config.GetBool("option_activee", false);
    string pseudo = Config.GetString("pseudo", "");
}
```

Chaque `GetXxx` renvoie la valeur sauvegardée si elle existe, sinon la valeur par défaut donnée en deuxième
paramètre - jamais d'exception, jamais besoin de vérifier si la clé existe avant de la lire.

## Écrire une valeur

```csharp
Config.Set("compteur", 42);
Config.Set("sensibilite", 1.5f);
Config.Set("option_activee", true);
Config.Set("pseudo", "TonPseudo");
```

L'écriture se fait en mémoire immédiatement, puis le fichier est **sauvegardé automatiquement dans la seconde
qui suit** un changement - pas besoin d'appeler quoi que ce soit après un `Set` dans le cas général.

## Sauvegarde immédiate

```csharp
Config.Save();
```

Force l'écriture sur disque tout de suite, sans attendre le délai automatique. Utile juste avant un moment
sensible (par exemple juste avant `OnUnload()`, si tu veux être sûr qu'un dernier changement est bien persisté
avant que le mod ne soit déchargé).

## Vérifier qu'une clé existe

```csharp
if (Config.Has("premiere_fois"))
{
    // déjà configuré par le passé
}
else
{
    Config.Set("premiere_fois", true);
    // première fois que ce mod tourne pour ce joueur : afficher un message de bienvenue, par exemple
}
```

## Motif courant : widget + config en une ligne

```csharp
if (ImGuiAPI.Checkbox("Option", ref _flag))
    Config.Set("option", _flag);
```

Les widgets de `ImGuiAPI` renvoient `true` uniquement le frame où leur valeur change : combiné avec `Config.Set`,
ça sauvegarde automatiquement dès que le joueur touche au réglage, sans écriture inutile à chaque frame.

## Où sont stockées les données ?

Dans un fichier chiffré par mod, dans `C2ML\config\`. Le format et le chiffrement sont gérés entièrement par le
loader - ton mod n'a jamais à se soucier du chemin exact ni du format sur disque, seulement des clés et valeurs
qu'il choisit lui-même.

## Données brutes (avancé)

Pour un format que `ModConfig` ne couvre pas (données binaires, structure complexe sérialisée toi-même),
`C2ML.API.Config.ConfigManager` expose un accès plus direct :

```csharp
using C2ML.API.Config;

byte[] donnees = System.Text.Encoding.UTF8.GetBytes("n'importe quel contenu");
ConfigManager.SaveConfig(Meta.Name, donnees);

byte[]? relu = ConfigManager.LoadConfig(Meta.Name);
```

Dans la grande majorité des cas, `Config.GetXxx`/`Config.Set` (via `ModConfig`, montré plus haut) suffit
largement et est plus simple à utiliser.
