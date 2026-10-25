using System.IO;
using System.Reflection;
using C2ML.API.Attributes;
using C2ML.API.Core;
using C2ML.API.Keybinds;
using C2ML.API.Menu;
using HarmonyLib;
using UnityEngine;

namespace C2ML.ExampleMod
{
    // =====================================================================================================================
    // C2ML.ExampleMod - mod de reference qui utilise TOUTE l'API C2ML, section par section. Il ne fait rien d'utile en
    // jeu : c'est un recueil d'exemples a copier-coller, pas un vrai mod. Pour la documentation complete de chaque
    // fonction utilisee ici, voir les guides du SDK (docs\01-getting-started.md et suivants).
    //
    // [C2MLMeta(nom, version, auteur)] est le SEUL attribut requis sur ta classe de mod. "version" n'est utilise que
    // pour un mod non protege (.dll directement dans le dossier mods, sans passer par le serveur) : une fois ce mod
    // publie et installe depuis la Mod DB, le loader affiche la version/description connues du serveur a la place.
    // =====================================================================================================================
    [C2MLMeta("Example Mod", "1.0.0", "TonPseudo")]
    public class ExampleMod : BaseMod
    {
        // Identifiant unique du mod, utilise par le loader pour eviter de le charger deux fois. Par defaut
        // (si tu ne surcharges pas GetModGUID), c'est le nom complet de la classe .NET (espace de noms inclus) :
        // suffisant tant que tu ne renommes jamais ta classe. Si tu veux un identifiant stable meme apres un
        // renommage de code, surcharge GetModGUID() avec une chaine fixe choisie par toi (ex. un GUID).
        public override string GetModGUID() => "com.example.c2ml.examplemod";

        // -----------------------------------------------------------------------------------------------------------
        // Harmony : patch "a l'affichage" d'une methode Unity existante (ici le SETTER de Time.timeScale), pour
        // observer/modifier le comportement du jeu sans toucher a ses fichiers. 0Harmony.dll est fourni dans lib\ du
        // SDK. Voir docs\07-harmony-patching.md pour les regles ([HarmonyPatch], Prefix/Postfix, quand utiliser quoi).
        // -----------------------------------------------------------------------------------------------------------
        private Harmony? _harmony;
        private static int _timeScaleChanges;

        public override void OnLoad()
        {
            // Logger propre a ce mod (prefixe automatiquement "[Example Mod] " dans loader.log) : utilise-le pour
            // tout ce qui se passe DANS une methode d'instance de ton mod (OnLoad, ShowMenu, tes propres methodes...).
            Logger.Log("OnLoad: chargement du mod d'exemple");

            // ---- Config : valeurs cle/valeur persistees automatiquement (fichier chiffre dans C2ML\config) ----
            // Voir docs\05-config.md. GetXxx(cle, valeurParDefaut) renvoie la valeur sauvegardee ou le defaut si elle
            // n'existe pas encore ; Set(cle, valeur) l'ecrit en memoire (sauvegarde automatique dans la seconde qui
            // suit, ou immediate avec Config.Save()).
            _counter = Config.GetInt("counter", 0);
            _checkboxDemo = Config.GetBool("checkbox_demo", false);
            _nameDemo = Config.GetString("name_demo", "");

            // ---- Keybinds : touche configurable par le joueur, persistee automatiquement ----
            // Voir docs\04-keybinds.md. RegisterKeybind doit etre appele une fois (ici dans OnLoad) avant de pouvoir
            // lire la touche actuelle avec GetKeybind/IsKeyPressed. defaultKey n'est utilise que la toute premiere
            // fois (aucun fichier de touches existant pour ce mod) ; ensuite la valeur sauvegardee prend le dessus,
            // y compris si le joueur l'a changee depuis le menu Keybinds du loader.
            KeybindManager.RegisterKeybind(Meta.Name, "Exemple d'action", (int)VirtualKey.F6);

            // ---- Harmony : PatchAll() scanne cet assembly et applique toutes les classes [HarmonyPatch] qu'il
            // trouve (ici TimeScalePatch, plus bas dans ce fichier). L'identifiant passe a Harmony doit etre unique
            // (convention : le meme que GetModGUID()). Toujours desenregistrer dans OnUnload (voir plus bas) : sinon
            // le patch resterait actif meme apres que le joueur ait desactive ton mod.
            _harmony = new Harmony(GetModGUID());
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.Log("Harmony: patch de Time.timeScale installe");
        }

        public override void OnUnload()
        {
            // Retire tous les patches poses par CETTE instance de Harmony (celle creee dans OnLoad, avec notre
            // propre identifiant). Indispensable : sans ca, le patch resterait actif dans le jeu meme apres la
            // desactivation du mod depuis Manage mods.
            _harmony?.UnpatchSelf();
            Logger.Log("OnUnload: patch Harmony retire");
        }

        // Classe de patch Harmony : convention HarmonyX habituelle, une classe statique imbriquee par methode
        // patchee. [HarmonyPatch(type, nomDeMembre, MethodType.Setter)] cible le "set" d'une propriete (ici
        // UnityEngine.Time.timeScale) ; pour une methode normale, omets MethodType.Setter et donne son nom.
        // Postfix() s'execute APRES la methode d'origine (le jeu change bien timeScale normalement, on observe
        // juste) ; un Prefix() s'executerait AVANT et peut meme empecher l'original de s'executer (return false).
        //
        // Static : une classe de patch Harmony n'a pas d'instance de ton mod. Pour logger depuis ici, utilise le
        // logger GLOBAL C2ML.API.Logging.Logger (pas "this.Logger", qui n'existe que sur une instance de BaseMod).
        [HarmonyPatch(typeof(Time), nameof(Time.timeScale), MethodType.Setter)]
        private static class TimeScalePatch
        {
            private static void Postfix()
            {
                _timeScaleChanges++;
                C2ML.API.Logging.Logger.Log("[ExampleMod/Harmony] Time.timeScale a change (" + _timeScaleChanges + " fois)");
            }
        }

        // =====================================================================================================================
        // Menu : page principale (ShowMenu). CurrentSubPage (sur BaseMod) dit au loader quel sous-titre/bouton retour
        // afficher dans SON PROPRE en-tete : voir docs\02-mod-lifecycle.md. Chaque sous-page est juste une autre
        // methode C# ordinaire, affichee quand CurrentSubPage vaut la valeur que tu lui as donnee via NavButton.
        // =====================================================================================================================
        private int _counter;
        private bool _checkboxDemo;
        private string _nameDemo = "";

        public override void ShowMenu()
        {
            switch (CurrentSubPage)
            {
                case "Widgets": ShowWidgetsPage(); return;
                case "Window": ShowWindowPage(); return;
                case "Harmony": ShowHarmonyPage(); return;
            }

            ImGuiAPI.Text("C2ML.ExampleMod - page principale");
            ImGuiAPI.TextWrapped("Ce mod n'a aucun effet en jeu : c'est un recueil d'exemples d'utilisation de l'API C2ML, a lire a cote du code source (ExampleMod.cs) et des guides du SDK.");
            ImGuiAPI.Spacing();
            ImGuiAPI.Separator();
            ImGuiAPI.Spacing();

            // ---- Logger : les 4 niveaux (voir docs\06-logging.md) ----
            ImGuiAPI.Text("Logger (ecrit dans loader.log) :");
            float rowW = ImGuiAPI.GetContentRegionAvailX();
            float btnW = (rowW - 3f * 6f) / 4f;
            if (ImGuiAPI.Button("Log", btnW)) Logger.Log("Message normal");
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.SecondaryButton("Success", btnW)) Logger.LogSuccess("Message de succes");
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Warning", btnW)) Logger.LogWarning("Message d'avertissement");
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.DangerButton("Error", btnW)) Logger.LogError("Message d'erreur");
            ImGuiAPI.Spacing();

            // ---- Config : compteur persiste entre deux lancements du jeu ----
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("Config.GetInt / Set - compteur : " + _counter);
            ImGuiAPI.SameLine(0f, 10f);
            if (ImGuiAPI.PrimaryButton("+1##counter_btn", 60f))
            {
                _counter++;
                Config.Set("counter", _counter);   // ecrit en memoire, sauvegarde automatique sous 1 seconde
            }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.Button("Save now##counter_save", 90f))
                Config.Save();   // ecriture immediate, utile juste avant un moment sensible (ex. avant un OnUnload)

            if (ImGuiAPI.Checkbox("Config.GetBool / Set - case cochee", ref _checkboxDemo))
                Config.Set("checkbox_demo", _checkboxDemo);

            ImGuiAPI.Text("Config.GetString / Set - texte :");
            if (ImGuiAPI.InputText("##name_demo", ref _nameDemo))
                Config.Set("name_demo", _nameDemo);
            ImGuiAPI.Spacing();

            // ---- Keybinds ----
            int key = KeybindManager.GetKeybind(Meta.Name, "Exemple d'action");
            ImGuiAPI.Text("Touche actuelle (modifiable dans le menu Keybinds du loader) : VK " + key);
            if (KeybindManager.IsKeyPressed(key))
            {
                ImGuiAPI.SameLine(0f, 10f);
                ImGuiAPI.Badge("Pressee !", BadgeKind.Ok);
            }
            ImGuiAPI.Spacing();
            ImGuiAPI.Separator();
            ImGuiAPI.Spacing();

            // ---- Navigation vers les autres pages de cet exemple (NavButton + CurrentSubPage, voir plus haut) ----
            if (ImGuiAPI.NavButton("Galerie de widgets ->"))
                CurrentSubPage = "Widgets";
            if (ImGuiAPI.NavButton("Fenetre flottante ->"))
                CurrentSubPage = "Window";
            if (ImGuiAPI.NavButton("Etat du patch Harmony ->"))
                CurrentSubPage = "Harmony";
        }

        // =====================================================================================================================
        // Sous-page : galerie de TOUS les widgets de l'API (docs\03-menu-widgets.md). Chaque bloc est independant :
        // copie juste celui dont tu as besoin dans ton propre mod.
        // =====================================================================================================================
        private bool _toggleDemo = true;
        private bool _toggleBtnDemo;
        private int _intDemo = 5;
        private float _floatDemo = 1.5f;
        private int _sliderIntDemo = 50;
        private float _sliderFloatDemo = 0.5f;
        private float _colorR = 0.29f, _colorG = 0.87f, _colorB = 0.50f;
        private float _colorR2 = 0.9f, _colorG2 = 0.6f, _colorB2 = 0.2f;
        private int _dropdownDemo;
        private readonly bool[] _dropdownMultiDemo = new bool[4];
        private int _listDemo;
        private readonly bool[] _listMultiDemo = new bool[5];
        private string _textDemo = "";
        private string _multilineDemo = "";

        private void ShowWidgetsPage()
        {
            ImGuiAPI.TextWrapped("Chaque widget custom de l'API C2ML, un a la fois. Le style (couleurs, arrondis, animations) est applique automatiquement par le loader : tu n'as rien a faire pour que ca matche le theme du site.");
            ImGuiAPI.Spacing();

            // Badges de statut : 4 couleurs fixes (voir BadgeKind). AlignTextToFramePadding() avant un Text() suivi
            // d'un widget "encadre" (bouton, badge...) sur la meme ligne : sinon le texte parait plus haut.
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("Badge:");
            ImGuiAPI.SameLine(0f, 10f);
            ImGuiAPI.Badge("Info", BadgeKind.Info);
            ImGuiAPI.SameLine(0f, 6f);
            ImGuiAPI.Badge("Ok", BadgeKind.Ok);
            ImGuiAPI.SameLine(0f, 6f);
            ImGuiAPI.Badge("Warning", BadgeKind.Warning);
            ImGuiAPI.SameLine(0f, 6f);
            ImGuiAPI.Badge("Problem", BadgeKind.Problem);
            ImGuiAPI.Spacing();

            // Interrupteur (on/off visuel) vs Checkbox (case a cocher classique) : meme valeur bool, juste un autre
            // habillage - choisis celui qui correspond au mieux au reglage.
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("ToggleSwitch:");
            ImGuiAPI.SameLine(140f, -1f);
            ImGuiAPI.ToggleSwitch("##toggle_demo", ref _toggleDemo);
            ImGuiAPI.Spacing();

            // Boutons : Ghost (Button, par defaut), Primary, Secondary, Danger, Link. width/height omis ou <= 0 =
            // toute la largeur disponible ; une valeur positive = largeur exacte en pixels.
            float rowW = ImGuiAPI.GetContentRegionAvailX();
            float btnW = (rowW - 3f * 6f) / 4f;
            if (ImGuiAPI.Button("Button", btnW)) { }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.PrimaryButton("PrimaryButton", btnW)) { }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.SecondaryButton("SecondaryButton", btnW)) { }
            ImGuiAPI.SameLine(0f, 6f);
            if (ImGuiAPI.DangerButton("DangerButton", btnW)) { }
            if (ImGuiAPI.LinkButton("LinkButton")) { }
            ImGuiAPI.Spacing();

            // Bouton qui reste enfonce (meme forme qu'un bouton, se comporte comme un bool) : utile pour un mode
            // on/off plus visible qu'une case a cocher au milieu d'une liste de reglages.
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("ToggleButton:");
            ImGuiAPI.SameLine(140f, -1f);
            ImGuiAPI.ToggleButton(_toggleBtnDemo ? "On##tbtn" : "Off##tbtn", ref _toggleBtnDemo, 100f);
            ImGuiAPI.Spacing();

            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("Spinner:");
            ImGuiAPI.SameLine(140f, -1f);
            ImGuiAPI.Spinner("##spinner_demo");
            ImGuiAPI.Spacing();

            // Champs numeriques et curseurs : chacun prend toute la largeur disponible par defaut (une ligne chacun).
            ImGuiAPI.Text("InputInt:");
            ImGuiAPI.InputInt("##int_demo", ref _intDemo);
            ImGuiAPI.Text("InputFloat:");
            ImGuiAPI.InputFloat("##float_demo", ref _floatDemo, 0.5f);
            ImGuiAPI.SliderInt("SliderInt", ref _sliderIntDemo, 0, 100);
            ImGuiAPI.SliderFloat("SliderFloat", ref _sliderFloatDemo, 0f, 1f);
            ImGuiAPI.Spacing();

            // Couleur : ColorEdit3 (curseurs R/V/B) ou ColorWheel3 (roue HSV). Les deux ouvrent une popup avec un
            // champ hexadecimal. r/g/b sont des float 0..1 (pas 0..255).
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("ColorEdit3:");
            ImGuiAPI.ColorEdit3("##color_demo", ref _colorR, ref _colorG, ref _colorB);
            ImGuiAPI.AlignTextToFramePadding();
            ImGuiAPI.Text("ColorWheel3:");
            ImGuiAPI.ColorWheel3("##colorwheel_demo", ref _colorR2, ref _colorG2, ref _colorB2);
            ImGuiAPI.Spacing();

            // Menus deroulants : simple (un choix) et multiple (plusieurs cases a cocher dans la liste).
            // "items" : libelles separes par '\n'.
            ImGuiAPI.Text("Dropdown:");
            ImGuiAPI.Dropdown("##dropdown_demo", "Facile\nNormal\nDifficile", ref _dropdownDemo);
            ImGuiAPI.Text("DropdownMulti:");
            ImGuiAPI.DropdownMulti("##dropdownmulti_demo", "Rouge\nVert\nBleu\nJaune", _dropdownMultiDemo);
            ImGuiAPI.Spacing();

            // Listes deroulantes (zone fixe qui defile). IMPORTANT : EndListBox()/EndGroupBox()/EndCard() doivent
            // TOUJOURS etre appeles, meme si le Begin...() correspondant a renvoye faux - jamais a l'interieur d'un
            // "if", sinon la pile interne du menu se desynchronise.
            ImGuiAPI.Text("BeginListBox / ListItem:");
            bool listOpen = ImGuiAPI.BeginListBox("##list_demo", 0f, 90f);
            if (listOpen)
            {
                string[] items = { "Pomme", "Banane", "Cerise", "Datte", "Kiwi" };
                for (int i = 0; i < items.Length; i++)
                    if (ImGuiAPI.ListItem(items[i] + "##litem_" + i, _listDemo == i))
                        _listDemo = i;
            }
            ImGuiAPI.EndListBox();
            ImGuiAPI.Spacing();

            ImGuiAPI.Text("BeginListBox / ListItemCheck (multi-select):");
            bool listMultiOpen = ImGuiAPI.BeginListBox("##listmulti_demo", 0f, 110f);
            if (listMultiOpen)
            {
                string[] items = { "Rouge", "Vert", "Bleu", "Jaune", "Violet" };
                for (int i = 0; i < items.Length; i++)
                    ImGuiAPI.ListItemCheck(items[i] + "##lmitem_" + i, ref _listMultiDemo[i]);
            }
            ImGuiAPI.EndListBox();
            ImGuiAPI.Spacing();

            // Champs de texte : une ligne, puis plusieurs lignes.
            ImGuiAPI.Text("InputText:");
            ImGuiAPI.InputText("##text_demo", ref _textDemo);
            ImGuiAPI.Text("InputTextMultiline:");
            ImGuiAPI.InputTextMultiline("##multiline_demo", ref _multilineDemo, 1024, 0f, 80f);
            ImGuiAPI.Spacing();

            // Carte : sous-panneau arrondi avec sa propre bordure (regroupe visuellement plusieurs widgets).
            if (ImGuiAPI.BeginCard("##card_demo"))
            {
                ImGuiAPI.Text("Contenu a l'interieur d'une carte (BeginCard/EndCard).");
                ImGuiAPI.Text("Pratique pour une ligne de liste (un mod, une entree...).");
            }
            ImGuiAPI.EndCard();
            ImGuiAPI.Spacing();

            // Groupe titre : comme une carte, mais avec un titre discret en haut.
            if (ImGuiAPI.BeginGroupBox("##groupbox_demo", "Reglages groupes"))
            {
                ImGuiAPI.Text("Tout ce qui est ici est visuellement lie.");
                ImGuiAPI.Checkbox("Option imbriquee", ref _toggleDemo);
            }
            ImGuiAPI.EndGroupBox();
            ImGuiAPI.Spacing();

            // BeginGroup/EndGroup : regroupe plusieurs widgets (pas de cadre visible, contrairement a BeginCard/
            // BeginGroupBox) comme un seul bloc pour la mise en page - utile pour aligner une icone a cote de
            // plusieurs lignes de texte avec SameLine().
            ImGuiAPI.BeginGroup();
            ImGuiAPI.Text("Premiere ligne d'un BeginGroup/EndGroup");
            ImGuiAPI.Text("Deuxieme ligne, traitee comme un seul bloc avec la premiere");
            ImGuiAPI.EndGroup();
            ImGuiAPI.Spacing();

            // PushStyleColor/PopStyleColor : acces direct aux couleurs ImGui de base (rarement necessaire - les
            // widgets custom ci-dessus geren deja le theme). count sur PopStyleColor doit correspondre au nombre
            // de PushStyleColor empiles depuis le dernier Pop.
            ImGuiAPI.PushStyleColor(ImGuiCol.Text, 0xFF66CCFFu);   // ABGR (comme ImGui) : ici un orange clair
            ImGuiAPI.Text("Texte colore via PushStyleColor/PopStyleColor");
            ImGuiAPI.PopStyleColor();
        }

        // =====================================================================================================================
        // Sous-page : fenetre flottante independante (BeginWindow/EndWindow = ImGui::Begin/End bruts). A reserver
        // aux cas ou tu as vraiment besoin d'une fenetre EN DEHORS du panneau C2ML (ex. un HUD toujours visible) :
        // contrairement au reste de l'API, elle n'a PAS le theme visuel du loader (ImGui par defaut).
        // =====================================================================================================================
        private bool _floatingWindowOpen;

        private void ShowWindowPage()
        {
            ImGuiAPI.TextWrapped("BeginWindow/EndWindow ouvrent une fenetre ImGui independante du panneau C2ML (utile pour un HUD, par exemple). Elle n'a pas le theme du site : c'est de l'ImGui par defaut.");
            ImGuiAPI.Spacing();
            ImGuiAPI.ToggleSwitch("Afficher la fenetre flottante##floating_toggle", ref _floatingWindowOpen);

            if (_floatingWindowOpen)
            {
                // Le "ref bool open" se met a false tout seul si le joueur clique sur le [x] de la fenetre :
                // on relit _floatingWindowOpen ensuite pour garder le ToggleSwitch ci-dessus synchronise.
                bool shown = ImGuiAPI.BeginWindow("C2ML.ExampleMod - fenetre flottante", ref _floatingWindowOpen);
                if (shown)
                {
                    ImGuiAPI.Text("Cette fenetre existe independamment du panneau C2ML.");
                    ImGuiAPI.Text("Compteur partage avec la page principale : " + _counter);
                }
                ImGuiAPI.EndWindow();   // toujours appele, meme si shown est faux (meme regle que les autres Begin/End)
            }
        }

        // =====================================================================================================================
        // Sous-page : etat du patch Harmony installe dans OnLoad (voir plus haut, TimeScalePatch). Change le
        // timeScale de la simulation d'un peu pour voir le compteur augmenter (ne fait rien de destructeur).
        // =====================================================================================================================
        private void ShowHarmonyPage()
        {
            ImGuiAPI.TextWrapped("Patch Harmony installe au chargement (OnLoad) sur UnityEngine.Time.timeScale (voir TimeScalePatch dans ExampleMod.cs). Le bouton ci-dessous modifie timeScale juste pour declencher le patch - inoffensif, revient a 1 tout seul.");
            ImGuiAPI.Spacing();
            ImGuiAPI.Text("Nombre de changements detectes par le patch : " + _timeScaleChanges);
            ImGuiAPI.Spacing();
            if (ImGuiAPI.PrimaryButton("Declencher le patch (timeScale = 0.5 puis 1)", 320f))
            {
                Time.timeScale = 0.5f;
                Time.timeScale = 1f;
            }
        }

        // Icone affichee dans le menu du loader (Manage mods, About...) : ressource embarquee dans la DLL. Renvoyer
        // null (ou ne pas surcharger GetModIcon du tout) utilise l'icone par defaut du loader - entierement facultatif.
        public override byte[] GetModIcon()
        {
            using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream("C2ML.ExampleMod.icon.png");
            if (s == null) return null!;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
