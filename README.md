# UiManager

Centralized UI panel manager for Unity.  
Manages show/hide of registered panels, supports modal overlays, JSON-driven panel definitions for modding, and exposes events for cross-module integration via bridge components.


## Features

- **ShowPanel / HidePanel / HideAll** — unified panel lifecycle API
- **Modal overlays** — panels flagged as modal prevent interaction below; `hideHud` panels auto-hide the HUD
- **Prefab instantiation** — panels are loaded from `Resources/` paths and `DontDestroyOnLoad`-ed automatically
- **JSON / Modding** — define panels in `StreamingAssets/ui_panels/`; merged by `id` on top of Inspector data
- **Events** — `OnPanelShown`, `OnPanelHidden` for reactive integration
- **StateManager integration** — auto-show/hide panels mapped to each `AppState` (activated via `UIMANAGER_STM`)
- **CutsceneManager integration** — auto-hide HUD during sequences; restore on complete/skip (activated via `UIMANAGER_CSM`)
- **DialogueManager integration** — show/hide dialogue panel on dialogue start/complete (activated via `UIMANAGER_DM`)
- **MapLoaderFramework integration** — show loading panel on chapter change; hide on load (activated via `UIMANAGER_MLF`)
- **MiniGameManager integration** — show/hide mini-game HUD on start/complete/abort (activated via `UIMANAGER_MGM`)
- **LocalizationManager integration** — refresh visible panels' text on language change (activated via `UIMANAGER_LM`)
- **EventManager integration** — broadcast `ui.panel.shown/hidden` events (activated via `UIMANAGER_EM` or `EVENTMANAGER_UIM`)
- **Custom Inspector** — live panel controls, visible panel list, and registered panel catalogue in Play Mode
- **DOTween Pro integration** — `CanvasGroup.DOFade` and `transform.DOPunchScale` provide polished panel show/hide animations instead of instant visibility toggles (activated via `UIMANAGER_DOTWEEN`)
- **Odin Inspector integration** — `SerializedMonoBehaviour` base for full Inspector serialization of complex types; runtime-display fields marked `[ReadOnly]` in Play Mode (activated via `ODIN_INSPECTOR`)


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/UiManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/UiManager.git Assets/UiManager
```

### Option C — npm / postinstall

```bash
cd Assets/UiManager
npm install
```


## Scene Setup

1. Create a persistent manager GameObject (or reuse your existing manager object).
2. Attach `UiManager`.
3. Set `hudPanelId` to the id of your HUD panel (default: `"hud"`).
4. Add panel definitions in the Inspector or via JSON files in `StreamingAssets/ui_panels/`.
5. Attach any bridge components (see Bridge Components below).


## Quick Start

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `panels` | *(empty)* | Built-in panel definitions |
| `hudPanelId` | `"hud"` | Id of the HUD panel (auto-hidden when a `hideHud` panel is shown) |
| `loadFromJson` | `false` | Merge definitions from `ui_panels/` |
| `jsonPath` | `"ui_panels/"` | Folder relative to `StreamingAssets/` containing `.json` files to merge. Falls back to single-file mode if the value points to an existing file. |
| `verboseLogging` | `false` | Log all show/hide calls to Console |

### UiPanelDefinition fields

| Field | Description |
| ----- | ----------- |
| `id` | Unique id, e.g. `"hud"`, `"dialogue"`, `"inventory"` |
| `displayName` | Human-readable label |
| `prefabPath` | Resources-relative path to the panel prefab |
| `modal` | When true, blocks interaction below this panel |
| `hideHud` | When true, auto-hides the HUD panel while this panel is visible |
| `category` | Tag, e.g. `"overlay"`, `"menu"` |

### Code usage

```csharp
var ui = FindFirstObjectByType<UiManager.Runtime.UiManager>();

ui.ShowPanel("inventory");
ui.HidePanel("inventory");
ui.ShowModal("options");
ui.HideAll();

bool visible = ui.IsPanelVisible("hud");

// Subscribe to events
ui.OnPanelShown  += id => Debug.Log($"Shown: {id}");
ui.OnPanelHidden += id => Debug.Log($"Hidden: {id}");
```


## Bridge Components

| Component | Define | Effect |
| --------- | ------ | ------ |
| `StateManagerBridge` | `UIMANAGER_STM` | Auto show/hide panels mapped to `AppState` changes |
| `CutsceneManagerBridge` | `UIMANAGER_CSM` | Hide HUD on sequence start; restore on complete/skip |
| `DialogueManagerBridge` | `UIMANAGER_DM` | Show dialogue panel on start; hide on complete |
| `MapLoaderBridge` | `UIMANAGER_MLF` | Show loading panel on chapter change; hide on map loaded |
| `MiniGameManagerBridge` | `UIMANAGER_MGM` | Show mini-game HUD on start; hide on complete/abort |
| `LocalizationManagerBridge` | `UIMANAGER_LM` | Refresh visible panel text on language change |
| `EventManagerBridge` | `UIMANAGER_EM` | Fire `ui.panel.shown/hidden` via EventManager |

EventManager can also re-broadcast UiManager events using `UiEventBridge` (define: `EVENTMANAGER_UIM`).


## JSON / Modding

Place one or more `.json` files in `StreamingAssets/ui_panels/` (path is configurable).
All `*.json` files in the folder are loaded and merged by `id` at startup.

**Example:** `StreamingAssets/ui_panels/main.json`

```json
{
  "panels": [
    {
      "id": "shop",
      "displayName": "Shop",
      "prefabPath": "UI/ShopPanel",
      "modal": true,
      "hideHud": true,
      "category": "overlay"
    }
  ]
}
```

JSON entries are **merged by id** — mods can add new panels or override Inspector definitions without reimporting.


## Optional Integrations

| Define | Integration |
| ------ | ----------- |
| `UIMANAGER_STM` | UiManager ←→ StateManager |
| `UIMANAGER_CSM` | UiManager ←→ CutsceneManager |
| `UIMANAGER_DM` | UiManager ←→ DialogueManager |
| `UIMANAGER_MLF` | UiManager ←→ MapLoaderFramework |
| `UIMANAGER_MGM` | UiManager ←→ MiniGameManager |
| `UIMANAGER_LM` | UiManager ←→ LocalizationManager |
| `UIMANAGER_EM` | UiManager → EventManager (fire events) |
| `EVENTMANAGER_UIM` | EventManager ← UiManager (re-broadcast) |
| `ODIN_INSPECTOR` | UiManager ↔→ Odin Inspector (`SerializedMonoBehaviour` + `[ReadOnly]`) |


## Editor Tools — Prefab Generation

`UiManagerEditor.cs` in `Editor/` doubles as a prefab generator.
`UiPrefabHelper` reads `*.json` files from `StreamingAssets/ui_panels/` and outputs one prefab per `UiPanelDefinition` into `Assets/Resources/Prefabs/UI/`.

**Manual**

- **Generate Prefabs → UI Panels** in the Unity menu bar
- **Generate Prefabs → All** (`Ctrl+Shift+G`) — regenerates all registered prefab generators in one step

**Automatic**
Saving any `*.json` file in `ui_panels/` triggers `UiPrefabPostprocessor` via `AssetPostprocessor.OnPostprocessAllAssets`.

**What is generated per prefab**

| Component | Details |
| --------- | ------- |
| `RectTransform` | Anchors stretch full screen (anchorMin 0,0 → anchorMax 1,1) |
| `CanvasGroup` | `alpha = 0` (1 for `hud`); `blocksRaycasts` mirrors `modal` |
| `Image` | Added for modal / fullscreen panels; `fade_screen` gets `Color.black`, others semi-transparent |

> Generated prefabs are starting points. Add child layouts, text components, and button callbacks before shipping.

**ODIN Inspector compatibility**
When `ODIN_INSPECTOR` is defined, `UiManagerEditor` inherits `OdinEditor` so the full ODIN property tree is rendered. The prefab generation helper `UiPrefabHelper` is a plain static class and is completely ODIN-independent.


## JSON Editor Window

Open via **JSON Editors → UI Manager** in the Unity menu bar, or via the **Open JSON Editor** button in the UiManager Inspector.

| Action | Result |
| ------ | ------ |
| **Load** | Reads all `*.json` from `StreamingAssets/ui_panels/`; creates the folder if missing |
| **Edit** | Add / remove / reorder entries using the Inspector list |
| **Save** | Writes each entry as `<id>.json` to `StreamingAssets/ui_panels/`; entries without an `id` are skipped. Calls `AssetDatabase.Refresh()` |

With **ODIN_INSPECTOR** active, the list uses Odin's enhanced drawer (drag-to-sort, collapsible entries).
