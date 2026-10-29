# Référence des widgets du menu (`ImGuiAPI`)

Tout se dessine depuis `ShowMenu()` (ou une sous-page, voir [02-mod-lifecycle.md](02-mod-lifecycle.md)) en
appelant des méthodes statiques de `C2ML.API.Menu.ImGuiAPI`. Le style (couleurs, arrondis, animations d'entrée)
est appliqué automatiquement par le loader pour matcher le thème du site - tu n'as rien à faire pour ça.

Règle commune à tous les `BeginXxx`/`EndXxx` (cartes, groupes, listes, fenêtres) : **`EndXxx()` doit toujours
être appelé**, même si `BeginXxx()` a renvoyé faux. Ne le place jamais à l'intérieur d'un `if`, sinon la pile
interne du menu se désynchronise et le reste de la page se dessine n'importe comment.

```csharp
bool open = ImGuiAPI.BeginCard("##ma_carte");
if (open)
{
    ImGuiAPI.Text("Contenu");
}
ImGuiAPI.EndCard();   // toujours, pas dans le "if"
```

Code complet de chaque widget ci-dessous : voir `ShowWidgetsPage()` dans `C2ML.ExampleMod\ExampleMod.cs`.

## Sommaire

- [Texte](#texte)
- [Boutons](#boutons)
- [Cases, interrupteurs, curseurs](#cases-interrupteurs-curseurs)
- [Champs de texte et numériques](#champs-de-texte-et-numériques)
- [Couleur](#couleur)
- [Listes et menus déroulants](#listes-et-menus-déroulants)
- [Badges et chargement](#badges-et-chargement)
- [Regroupement (cartes, groupes)](#regroupement-cartes-groupes)
- [Mise en page](#mise-en-page)
- [Navigation](#navigation)
- [Fenêtre flottante indépendante](#fenêtre-flottante-indépendante)
- [Texte coloré](#texte-coloré)
- [Couleurs ImGui brutes](#couleurs-imgui-brutes-avancé)

## Texte

```csharp
ImGuiAPI.Text("Texte simple");
ImGuiAPI.TextWrapped("Texte long qui revient à la ligne tout seul pour tenir dans la largeur disponible - utile pour une description.");
```

## Boutons

Cinq styles, plus un bouton qui reste enfoncé. `width`/`height` omis ou `<= 0` = toute la largeur disponible ;
une valeur positive = taille exacte en pixels. Chacun renvoie `true` le frame où il est cliqué.

```csharp
if (ImGuiAPI.Button("Ghost (par défaut)")) { }
if (ImGuiAPI.PrimaryButton("Primary")) { }         // accent, action principale
if (ImGuiAPI.SecondaryButton("Secondary")) { }      // contour visible, fond transparent
if (ImGuiAPI.DangerButton("Danger")) { }            // rouge, pour une action destructrice
if (ImGuiAPI.LinkButton("Lien")) { }                // texte seul, souligné au survol

bool actif = false;
ImGuiAPI.ToggleButton(actif ? "On##id" : "Off##id", ref actif, 100f);   // reste enfoncé, se comporte comme un bool
```

Pour plusieurs boutons sur la même ligne, divise toi-même la largeur disponible (`GetContentRegionAvailX()`,
voir plus bas) et utilise `SameLine()` entre chacun.

## Cases, interrupteurs, curseurs

```csharp
bool flag = false;
ImGuiAPI.Checkbox("Case à cocher", ref flag);
ImGuiAPI.ToggleSwitch("##id_unique", ref flag);   // même valeur bool, habillage "interrupteur" au lieu d'une case

int entier = 50;
ImGuiAPI.SliderInt("Curseur entier", ref entier, 0, 100);

float flottant = 0.5f;
ImGuiAPI.SliderFloat("Curseur flottant", ref flottant, 0f, 1f);
```

`ToggleSwitch` prend un `id` (peut commencer par `##` pour ne rien afficher à côté du widget - utile quand le
label est déjà affiché séparément avec `Text()` + `SameLine()`).

## Champs de texte et numériques

```csharp
string texte = "";
ImGuiAPI.InputText("##un_champ", ref texte, maxLength: 256);

string long_texte = "";
ImGuiAPI.InputTextMultiline("##plusieurs_lignes", ref long_texte, maxLength: 1024, width: 0f, height: 80f);

int i = 5;
ImGuiAPI.InputInt("##entier", ref i, step: 1);

float f = 1.5f;
ImGuiAPI.InputFloat("##flottant", ref f, step: 0.5f);
```

Tous renvoient `true` le frame où la valeur change (pratique pour sauvegarder dans `Config` uniquement quand
nécessaire, voir [05-config.md](05-config.md)).

## Couleur

Deux variantes, au choix selon ce que tu préfères visuellement - les deux ouvrent une petite popup avec un champ
hexadécimal. `r`/`g`/`b` sont des `float` entre 0 et 1 (pas 0-255).

```csharp
float r = 0.29f, g = 0.87f, b = 0.50f;
ImGuiAPI.ColorEdit3("##couleur_curseurs", ref r, ref g, ref b);      // trois curseurs R/V/B
ImGuiAPI.ColorWheel3("##couleur_roue", ref r, ref g, ref b);         // roue de couleurs HSV complète
```

## Listes et menus déroulants

**Menu déroulant simple** (un seul choix) et **à choix multiples** - `items` est une chaîne avec un libellé par
ligne (séparés par `\n`) :

```csharp
int choix = 0;
ImGuiAPI.Dropdown("##difficulte", "Facile\nNormal\nDifficile", ref choix);

bool[] selection = new bool[4];   // une case par item, même ordre que "items"
ImGuiAPI.DropdownMulti("##couleurs", "Rouge\nVert\nBleu\nJaune", selection);
```

**Liste déroulante** (zone fixe qui défile), remplie avec `ListItem` (un seul choix) ou `ListItemCheck`
(plusieurs choix, une case à cocher par ligne) :

```csharp
string[] items = { "Pomme", "Banane", "Cerise" };
int selectionne = 0;

bool open = ImGuiAPI.BeginListBox("##ma_liste", width: 0f, height: 90f);
if (open)
{
    for (int i = 0; i < items.Length; i++)
        if (ImGuiAPI.ListItem(items[i] + "##item_" + i, selectionne == i))
            selectionne = i;
}
ImGuiAPI.EndListBox();   // toujours, même si "open" est faux
```

```csharp
bool[] coches = new bool[items.Length];
bool open2 = ImGuiAPI.BeginListBox("##ma_liste_multi", 0f, 110f);
if (open2)
{
    for (int i = 0; i < items.Length; i++)
        ImGuiAPI.ListItemCheck(items[i] + "##mitem_" + i, ref coches[i]);
}
ImGuiAPI.EndListBox();
```

Astuce `"##..."` : la partie après `##` dans un libellé n'est PAS affichée, mais sert à donner un identifiant
unique au widget (indispensable dans une boucle `for`, sinon tous les éléments partagent le même état interne).

## Badges et chargement

```csharp
ImGuiAPI.Badge("Info", BadgeKind.Info);
ImGuiAPI.Badge("Ok", BadgeKind.Ok);
ImGuiAPI.Badge("Attention", BadgeKind.Warning);
ImGuiAPI.Badge("Problème", BadgeKind.Problem);

ImGuiAPI.Spinner("##chargement");   // petit indicateur de chargement animé
```

Motif courant : bouton qui devient un spinner + "Saving..." pendant une opération, puis revient tout seul.
Voir `ShowWidgetGallery` dans `C2ML.ExampleMod` pour l'exemple complet avec un minuteur.

## Regroupement (cartes, groupes)

**Carte** : sous-panneau arrondi avec sa propre bordure (regroupe visuellement une ligne, une entrée de liste...) :

```csharp
if (ImGuiAPI.BeginCard("##une_carte"))
{
    ImGuiAPI.Text("Contenu de la carte");
}
ImGuiAPI.EndCard();
```

**Groupe titré** : comme une carte, avec un titre discret en haut :

```csharp
if (ImGuiAPI.BeginGroupBox("##mon_groupe", "Réglages avancés"))
{
    ImGuiAPI.Checkbox("Option 1", ref flag);
}
ImGuiAPI.EndGroupBox();
```

**Groupe sans cadre visible** (`BeginGroup`/`EndGroup`) : traite plusieurs widgets comme un seul bloc pour
l'alignement (utile avec `SameLine()` pour mettre une icône à côté de plusieurs lignes de texte) :

```csharp
ImGuiAPI.BeginGroup();
ImGuiAPI.Text("Ligne 1");
ImGuiAPI.Text("Ligne 2");
ImGuiAPI.EndGroup();
```

## Mise en page

```csharp
ImGuiAPI.Separator();                          // ligne de séparation horizontale
ImGuiAPI.Spacing();                            // petit espace vertical
ImGuiAPI.SameLine(offsetFromStartX: 0, spacing: -1);   // place le widget suivant sur la même ligne

float largeurDispo = ImGuiAPI.GetContentRegionAvailX();   // largeur restante sur la ligne courante
ImGuiAPI.AlignTextToFramePadding();   // aligne un Text() qui précède un bouton/une case sur la même ligne
```

`AlignTextToFramePadding()` est à placer juste avant un `Text()` suivi d'un widget "encadré" (bouton, case,
badge...) sur la même ligne via `SameLine()` : sans ça, le texte paraît plus haut que le widget à côté (les
widgets encadrés sont calés sur la hauteur d'un bouton, plus grande qu'une simple ligne de texte).

```csharp
ImGuiAPI.AlignTextToFramePadding();
ImGuiAPI.Text("Option :");
ImGuiAPI.SameLine(120f, -1f);
ImGuiAPI.ToggleSwitch("##option", ref flag);
```

## Navigation

```csharp
if (ImGuiAPI.NavButton("Réglages avancés ->"))
    CurrentSubPage = "Advanced";
```

Voir [02-mod-lifecycle.md](02-mod-lifecycle.md) pour le mécanisme complet des sous-pages (`CurrentSubPage`).

`ImGuiAPI.PageHeader(backLabel, title)` existe aussi (titre centré + bouton retour + séparateur), mais tu n'en
as normalement pas besoin : le loader affiche déjà son propre en-tête à partir de `CurrentSubPage`. Ne l'utilise
que si tu construis un système de navigation entièrement personnalisé en dehors de ce mécanisme.

## Fenêtre flottante indépendante

`BeginWindow`/`EndWindow` ouvrent une fenêtre ImGui **complètement indépendante** du panneau C2ML (par exemple
un HUD toujours visible, même menu fermé). Contrairement à tout le reste de cette API, elle n'a **pas** le thème
visuel du loader - c'est de l'ImGui par défaut.

```csharp
bool ouverte = true;
if (ouverte)
{
    bool shown = ImGuiAPI.BeginWindow("Titre de la fenêtre", ref ouverte);
    if (shown)
    {
        ImGuiAPI.Text("Contenu indépendant du panneau C2ML");
    }
    ImGuiAPI.EndWindow();   // toujours, même si "shown" est faux
}
```

`ouverte` repasse à `false` tout seul si le joueur clique sur le `[x]` de la fenêtre - relis-la après l'appel
pour garder ton propre état synchronisé (voir `ShowWindowPage` dans `C2ML.ExampleMod`).

À réserver aux cas où tu as vraiment besoin d'un affichage hors du panneau C2ML : pour tout le reste, dessine
directement dans `ShowMenu()`.

## Texte coloré

Affiche les balises `<color=#RRGGBB>...</color>` (ou le raccourci `<#RRGGBB>...</color>`, RRGGBB/RRGGBBAA, ou le
format court à 3 chiffres `<#f0c>`) avec la vraie couleur au lieu de montrer les balises brutes - le même format
que celui utilisé par le jeu lui-même, par exemple pour les pseudos des joueurs (voir [09-sync.md](09-sync.md),
`RoomPlayer.Name`) :

```csharp
ImGuiAPI.ColoredText("Bonjour <color=#ff6666>monde</color> !");
ImGuiAPI.ColoredText("<#0f0>Format court</color> fonctionne aussi");
```

Se comporte exactement comme `Text()` quand la chaîne ne contient aucune balise - utilisable partout à la place
de `Text()` en cas de doute. Les balises imbriquées sont supportées (pile de couleurs).

Un libellé de `Dropdown`/`DropdownMulti` est un seul widget natif : impossible d'y afficher une couleur par
segment, utilise plutôt `ImGuiAPI.StripColorTags(text)` pour obtenir un libellé propre (retire les balises sans
les interpréter) :

```csharp
string libelle = ImGuiAPI.StripColorTags(joueur.Name) + " (" + joueur.Id + ")";
```

## Couleurs ImGui brutes (avancé)

```csharp
ImGuiAPI.PushStyleColor(ImGuiCol.Text, 0xFF66CCFFu);   // ABGR, comme ImGui natif
ImGuiAPI.Text("Texte coloré");
ImGuiAPI.PopStyleColor();   // count (défaut 1) doit correspondre au nombre de Push empilés depuis le dernier Pop
```

Rarement nécessaire : les widgets custom ci-dessus gèrent déjà le thème du site. Utile seulement pour un besoin
de couleur très spécifique sur un widget ImGui de base.
