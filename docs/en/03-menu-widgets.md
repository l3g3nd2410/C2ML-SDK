# Menu widgets reference (`ImGuiAPI`)

Everything is drawn from `ShowMenu()` (or a sub-page, see [02-mod-lifecycle.md](02-mod-lifecycle.md)) by calling
static methods on `C2ML.API.Menu.ImGuiAPI`. Styling (colors, rounding, entrance animations) is applied
automatically by the loader to match the site's theme - you don't have to do anything for that.

Common rule for every `BeginXxx`/`EndXxx` pair (cards, groups, lists, windows): **`EndXxx()` must always be
called**, even if `BeginXxx()` returned false. Never put it inside an `if`, or the menu's internal stack gets
desynced and the rest of the page draws incorrectly.

```csharp
bool open = ImGuiAPI.BeginCard("##my_card");
if (open)
{
    ImGuiAPI.Text("Content");
}
ImGuiAPI.EndCard();   // always, not inside the "if"
```

Full code for every widget below: see `ShowWidgetsPage()` in `C2ML.ExampleMod\ExampleMod.cs`.

## Contents

- [Text](#text)
- [Buttons](#buttons)
- [Checkboxes, toggles, sliders](#checkboxes-toggles-sliders)
- [Text and numeric fields](#text-and-numeric-fields)
- [Color](#color)
- [Lists and dropdowns](#lists-and-dropdowns)
- [Badges and loading](#badges-and-loading)
- [Grouping (cards, groups)](#grouping-cards-groups)
- [Layout](#layout)
- [Navigation](#navigation)
- [Standalone floating window](#standalone-floating-window)
- [Colored text](#colored-text)
- [Raw ImGui colors](#raw-imgui-colors-advanced)

## Text

```csharp
ImGuiAPI.Text("Plain text");
ImGuiAPI.TextWrapped("Long text that wraps on its own to fit the available width - useful for a description.");
```

## Buttons

Five styles, plus a button that stays pressed. `width`/`height` omitted or `<= 0` means the full available
width; a positive value means an exact pixel size. Each returns `true` on the frame it's clicked.

```csharp
if (ImGuiAPI.Button("Ghost (default)")) { }
if (ImGuiAPI.PrimaryButton("Primary")) { }         // accent color, main action
if (ImGuiAPI.SecondaryButton("Secondary")) { }      // visible outline, transparent background
if (ImGuiAPI.DangerButton("Danger")) { }            // red, for a destructive action
if (ImGuiAPI.LinkButton("Link")) { }                // text only, underlined on hover

bool active = false;
ImGuiAPI.ToggleButton(active ? "On##id" : "Off##id", ref active, 100f);   // stays pressed, behaves like a bool
```

For several buttons on the same line, split the available width yourself (`GetContentRegionAvailX()`, see
below) and use `SameLine()` between each one.

## Checkboxes, toggles, sliders

```csharp
bool flag = false;
ImGuiAPI.Checkbox("Checkbox", ref flag);
ImGuiAPI.ToggleSwitch("##unique_id", ref flag);   // same bool value, "toggle switch" look instead of a checkbox

int intVal = 50;
ImGuiAPI.SliderInt("Integer slider", ref intVal, 0, 100);

float floatVal = 0.5f;
ImGuiAPI.SliderFloat("Float slider", ref floatVal, 0f, 1f);
```

`ToggleSwitch` takes an `id` (can start with `##` to not display anything next to the widget - handy when the
label is already shown separately with `Text()` + `SameLine()`).

## Text and numeric fields

```csharp
string text = "";
ImGuiAPI.InputText("##a_field", ref text, maxLength: 256);

string longText = "";
ImGuiAPI.InputTextMultiline("##several_lines", ref longText, maxLength: 1024, width: 0f, height: 80f);

int i = 5;
ImGuiAPI.InputInt("##integer", ref i, step: 1);

float f = 1.5f;
ImGuiAPI.InputFloat("##float", ref f, step: 0.5f);
```

All of them return `true` on the frame the value changes (handy for saving to `Config` only when needed, see
[05-config.md](05-config.md)).

## Color

Two variants, pick whichever you like visually - both open a small popup with a hex field. `r`/`g`/`b` are
`float` values between 0 and 1 (not 0-255).

```csharp
float r = 0.29f, g = 0.87f, b = 0.50f;
ImGuiAPI.ColorEdit3("##color_sliders", ref r, ref g, ref b);      // three R/G/B sliders
ImGuiAPI.ColorWheel3("##color_wheel", ref r, ref g, ref b);       // full HSV color wheel
```

## Lists and dropdowns

**Simple dropdown** (single choice) and **multi-select dropdown** - `items` is a string with one label per line
(separated by `\n`):

```csharp
int choice = 0;
ImGuiAPI.Dropdown("##difficulty", "Easy\nNormal\nHard", ref choice);

bool[] selection = new bool[4];   // one slot per item, same order as "items"
ImGuiAPI.DropdownMulti("##colors", "Red\nGreen\nBlue\nYellow", selection);
```

**List box** (fixed, scrollable area), filled with `ListItem` (single choice) or `ListItemCheck` (multiple
choices, one checkbox per row):

```csharp
string[] items = { "Apple", "Banana", "Cherry" };
int selected = 0;

bool open = ImGuiAPI.BeginListBox("##my_list", width: 0f, height: 90f);
if (open)
{
    for (int i = 0; i < items.Length; i++)
        if (ImGuiAPI.ListItem(items[i] + "##item_" + i, selected == i))
            selected = i;
}
ImGuiAPI.EndListBox();   // always, even if "open" is false
```

```csharp
bool[] checked_ = new bool[items.Length];
bool open2 = ImGuiAPI.BeginListBox("##my_multi_list", 0f, 110f);
if (open2)
{
    for (int i = 0; i < items.Length; i++)
        ImGuiAPI.ListItemCheck(items[i] + "##mitem_" + i, ref checked_[i]);
}
ImGuiAPI.EndListBox();
```

`"##..."` trick: the part after `##` in a label is **not** displayed, but gives the widget a unique id
(essential inside a `for` loop, otherwise every item shares the same internal state).

## Badges and loading

```csharp
ImGuiAPI.Badge("Info", BadgeKind.Info);
ImGuiAPI.Badge("Ok", BadgeKind.Ok);
ImGuiAPI.Badge("Warning", BadgeKind.Warning);
ImGuiAPI.Badge("Problem", BadgeKind.Problem);

ImGuiAPI.Spinner("##loading");   // small animated loading indicator
```

Common pattern: a button that turns into a spinner + "Saving..." during an operation, then goes back on its own.
See `ShowWidgetGallery` in `C2ML.ExampleMod` for the full example with a timer.

## Grouping (cards, groups)

**Card**: rounded sub-panel with its own border (visually groups a row, a list entry...):

```csharp
if (ImGuiAPI.BeginCard("##a_card"))
{
    ImGuiAPI.Text("Card content");
}
ImGuiAPI.EndCard();
```

**Titled group box**: like a card, with a discreet title at the top:

```csharp
if (ImGuiAPI.BeginGroupBox("##my_group", "Advanced settings"))
{
    ImGuiAPI.Checkbox("Option 1", ref flag);
}
ImGuiAPI.EndGroupBox();
```

**Frameless group** (`BeginGroup`/`EndGroup`): treats several widgets as a single block for layout purposes
(useful with `SameLine()` to place an icon next to several lines of text):

```csharp
ImGuiAPI.BeginGroup();
ImGuiAPI.Text("Line 1");
ImGuiAPI.Text("Line 2");
ImGuiAPI.EndGroup();
```

## Layout

```csharp
ImGuiAPI.Separator();                          // horizontal separator line
ImGuiAPI.Spacing();                            // small vertical gap
ImGuiAPI.SameLine(offsetFromStartX: 0, spacing: -1);   // places the next widget on the same line

float availWidth = ImGuiAPI.GetContentRegionAvailX();   // remaining width on the current line
ImGuiAPI.AlignTextToFramePadding();   // aligns a Text() that precedes a button/checkbox on the same line
```

Place `AlignTextToFramePadding()` right before a `Text()` followed by a "framed" widget (button, checkbox,
badge...) on the same line via `SameLine()`: without it, the text appears higher than the widget next to it
(framed widgets are sized to a button's height, taller than a plain line of text).

```csharp
ImGuiAPI.AlignTextToFramePadding();
ImGuiAPI.Text("Option:");
ImGuiAPI.SameLine(120f, -1f);
ImGuiAPI.ToggleSwitch("##option", ref flag);
```

## Navigation

```csharp
if (ImGuiAPI.NavButton("Advanced settings ->"))
    CurrentSubPage = "Advanced";
```

See [02-mod-lifecycle.md](02-mod-lifecycle.md) for the full sub-page mechanism (`CurrentSubPage`).

`ImGuiAPI.PageHeader(backLabel, title)` also exists (centered title + back button + separator), but you
normally don't need it: the loader already draws its own header from `CurrentSubPage`. Only use it if you're
building an entirely custom navigation system outside of that mechanism.

## Standalone floating window

`BeginWindow`/`EndWindow` open an ImGui window **completely independent** of the C2ML panel (for example an
always-visible HUD, even with the menu closed). Unlike everything else in this API, it does **not** have the
loader's visual theme - it's plain default ImGui.

```csharp
bool open = true;
if (open)
{
    bool shown = ImGuiAPI.BeginWindow("Window title", ref open);
    if (shown)
    {
        ImGuiAPI.Text("Content independent of the C2ML panel");
    }
    ImGuiAPI.EndWindow();   // always, even if "shown" is false
}
```

`open` flips back to `false` on its own if the player clicks the window's `[x]` - read it back afterwards to
keep your own state in sync (see `ShowWindowPage` in `C2ML.ExampleMod`).

Reserve this for cases where you genuinely need something outside the C2ML panel: for everything else, draw
directly inside `ShowMenu()`.

## Colored text

Renders `<color=#RRGGBB>...</color>` tags (or the shorthand `<#RRGGBB>...</color>`, RRGGBB/RRGGBBAA, or the
3-digit shorthand `<#f0c>`) with the actual color instead of showing the raw tags - the same format the game
itself uses, for example for player names (see [09-sync.md](09-sync.md), `RoomPlayer.Name`):

```csharp
ImGuiAPI.ColoredText("Hello <color=#ff6666>world</color>!");
ImGuiAPI.ColoredText("<#0f0>Short form</color> works too");
```

Behaves exactly like `Text()` when the string has no tags - safe to use everywhere instead of `Text()` if you're
not sure. Nested tags are supported (color stack).

A `Dropdown`/`DropdownMulti` label is a single native widget: it can't render per-segment colors, so use
`ImGuiAPI.StripColorTags(text)` there instead to get a plain, clean label (removes the tags without interpreting
them):

```csharp
string label = ImGuiAPI.StripColorTags(player.Name) + " (" + player.Id + ")";
```

## Raw ImGui colors (advanced)

```csharp
ImGuiAPI.PushStyleColor(ImGuiCol.Text, 0xFF66CCFFu);   // ABGR, like native ImGui
ImGuiAPI.Text("Colored text");
ImGuiAPI.PopStyleColor();   // count (default 1) must match the number of Push calls stacked since the last Pop
```

Rarely needed: the custom widgets above already handle the site's theme. Useful only for a very specific color
need on a basic ImGui widget.
