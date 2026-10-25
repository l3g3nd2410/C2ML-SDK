# Publier ton mod

Publie via `/dev` pour chiffrer ton mod pour le loader (jamais en `.dll` brute, voir
[01-getting-started.md](01-getting-started.md)) - en version de test pendant que tu itères, ou en vraie release
une fois prêt pour les autres joueurs, mis à jour automatiquement et listé dans la Mod DB du loader si tu le
souhaites.

## Devenir développeur

Va sur `/dev` (connexion Steam), et envoie une demande d'accès avec un court message expliquant ce que tu
comptes publier. Un administrateur valide la demande manuellement - ça peut prendre un moment, ce n'est pas
automatique.

## Créer un mod

Une fois approuvé, "Nouveau mod" sur `/dev` demande :

| Champ | Rôle |
|---|---|
| **Nom** | Affiché partout (3 à 40 caractères) |
| **Description** | Affichée sur la page About du mod dans le loader une fois publié |
| **Visibilité** | `Public` (installable par n'importe qui, voir Listé ci-dessous pour la découvrabilité) ou `Privé` (invisible et non installable sauf pour toi et une liste d'accès que tu choisis) |
| **Listé dans la Mod DB** | Séparé de la visibilité : un mod `Public` n'apparaît dans la Mod DB du loader que s'il est en plus **listé**. Un mod public non listé reste installable par quiconque connaît déjà son id/lien, juste pas visible en parcourant la Mod DB - `Public` seul ne suffit **pas** à y apparaître |
| **Mise à jour automatique** | Les joueurs qui ont ce mod reçoivent automatiquement ta dernière version dès que tu la publies sur le serveur |
| **Liste d'accès** (mods privés) | SteamID64 des joueurs autorisés à voir/installer ce mod, en plus de toi |

## Publier une version

Sur la fiche de ton mod, section Versions : donne un numéro de version (ex. `1.0.0`), sélectionne ta DLL
compilée, clique sur Envoyer. Le chiffrement se fait entièrement dans ton navigateur - le serveur ne voit
jamais la DLL en clair, seulement le paquet `.c2m` déjà chiffré (voir plus bas pour l'inspection par un admin,
qui est une exception explicite et journalisée).

Dès la première version publiée, ton mod devient installable depuis la Mod DB du loader (si public et listé) ou
pour les joueurs de ta liste d'accès (si privé).

## Versions de test

Pour publier une version sans la distribuer à tout le monde tout de suite (travail en cours, feature à tester
avant une vraie sortie) : coche **"Version de test"** au moment d'envoyer - pas besoin de choisir un numéro,
un seul emplacement de test est réservé par mod. Republier une nouvelle version de test **écrase** la
précédente au lieu d'accumuler des essais.

Une version de test n'est **jamais** distribuée par la Mod DB ni la mise à jour automatique tant qu'elle n'est
pas promue. Pour la rendre active :

**Promouvoir en active** (bouton sur la ligne de la version de test) - elle est renommée automatiquement au
numéro suivant ta dernière version active (ex. `1.2.0` active → promue devient `1.2.1`) et devient la version
réellement distribuée.

## Supprimer une version

Une version qui n'est **pas** la version active (ancienne version jamais nettoyée, ou version de test que tu
ne veux plus garder) peut être supprimée définitivement depuis `/dev` - fichiers chiffrés compris. La version
active ne peut pas être supprimée directement : promeus d'abord une autre version pour la remplacer.

Seules les 3 dernières versions sont de toute façon conservées automatiquement (les plus anciennes sont
nettoyées tout seules à chaque nouvelle publication, sauf la version active qui est toujours protégée même si
elle est plus ancienne que les 3 dernières).

## Révoquer une version

Si une version publiée pose problème (bug grave, régression), **Révoquer** la coupe immédiatement pour tous les
joueurs (plus de clé de déchiffrement distribuée) sans la supprimer - réversible, utile le temps de publier un
correctif.

## Mods privés

Un mod privé n'apparaît dans aucun catalogue public et n'est installable que par toi et les joueurs de ta liste
d'accès (SteamID64). Pratique pour un mod en bêta fermée, ou réservé à un petit groupe.

## Inspection par un administrateur

Les administrateurs du serveur peuvent, pour n'importe quel mod publié (y compris privé), télécharger la DLL en
clair à des fins de modération (vérifier qu'un mod signalé ne fait rien de malveillant) ainsi que le paquet
`.c2m` final pour le tester en jeu directement - chaque inspection est journalisée. C'est la seule exception au
chiffrement de bout en bout décrit plus haut.
