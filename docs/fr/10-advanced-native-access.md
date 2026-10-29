# Avancé : accès bas niveau à un champ (`NativeApi`)

**Dernier recours.** Une bonne partie de l'état interne du jeu est sur des champs privés sans propriété publique,
événement ou méthode qui les couvre - la réflexion classique (`GetField`/`GetValue`) ne fonctionne pas non plus
dessus (les champs privés IL2CPP ne sont pas réfléchissables de la manière habituelle). `C2ML.API.Native.NativeApi`
les lit directement, en contournant complètement le système de types IL2CPP.

**Lis toute cette page avant de t'en servir.** Contrairement au reste de l'API, `NativeApi` ne fait **aucune
validation** : un mauvais type pour un champ fait planter le jeu (accès mémoire invalide) au lieu de lever une
exception récupérable. Vérifie toujours d'abord si une API publique couvre déjà ce dont tu as besoin - une
propriété, un événement, une méthode sur une interface exposée par le jeu - avant de passer par ici.

## Trouver le bon champ

Il te faut le nom exact du champ et le type exact qui le déclare. Le dump Cpp2IL fourni avec l'outillage de
développement du loader est le moyen fiable de trouver les deux ; deviner un nom à l'aveugle, c'est la meilleure
façon de finir par lire le mauvais champ. Si tu sais seulement à peu près où chercher, `DumpFieldNames` liste
tous les vrais noms de champs natifs d'un type :

```csharp
string[] champs = NativeApi.DumpFieldNames(unType);
foreach (string c in champs) Logger.Log(c);
```

## Lire un champ

**Champ objet/référence** (une classe ou une interface) :

```csharp
UnType valeur = NativeApi.GetObjectField<UnType>(instance, typeof(TypeDeclarant), "m_unChamp");
```

**Champ valeur** (`int`, `bool`, `float`, `double`, `IntPtr`, une struct blittable...) :

```csharp
if (NativeApi.TryGetValueField<int>(instance, typeof(TypeDeclarant), "m_unChamp", out int valeur))
{
    // valeur n'est valide que si TryGetValueField a renvoyé true
}
```

- `instance` est l'instance sur laquelle se trouve le champ (un objet déjà emballé par IL2CPP que tu as déjà,
  issu d'une propriété, d'une valeur de retour, d'un événement...).
- `typeof(TypeDeclarant)` doit être le type qui **déclare réellement** le champ - pas une sous-classe, pas une
  interface qu'il implémente. Pour un champ déclaré sur une classe de base, passe le type de la classe de base,
  pas celui de l'instance concrète.
- `GetObjectField<T>` renvoie `null` si le champ n'existe pas, vaut `null` côté jeu, ou si `T` ne correspond pas
  réellement au vrai type du champ (échec silencieux, pas d'exception - voir l'avertissement ci-dessus).
- `TryGetValueField<T>` renvoie `false` (et laisse `valeur` à `default`) dans les mêmes cas - vérifie toujours la
  valeur de retour, ne fais jamais confiance à `valeur` seule.

## Pointeur d'une instance

```csharp
IntPtr ptr = NativeApi.GetPointer(uneInstanceIl2Cpp);   // 0 si ce n'est pas un objet IL2CPP valide
```

Rarement utile directement - surtout pratique en débogage (logger un pointeur pour comparer deux instances...).

## Pour aller plus loin

| Guide | Contenu |
|---|---|
| [09-sync.md](09-sync.md) | Messages, liste des joueurs et variables utilisateur entre joueurs |
