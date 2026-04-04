# UiManager

Centralized UI panel manager for Unity.  
Manages show/hide of registered panels, supports modal overlays, JSON-driven panel definitions for modding, and exposes events for cross-module integration via bridge components.


## Features

- **ShowPanel / HidePanel / HideAll** — unified panel lifecycle API
- **Modal overlays** — panels flagged as modal prevent interaction below; `hideHud` panels auto-hide the HUD
- **Prefab instantiation** — panels are loaded from `Resources/` paths and `DontDestroyOnLoad`-ed automatically
- **JSON / Modding** — define panels in `StreamingAssets/ui_panels.json`; merged by `id` on top of Inspector data
- **Events** — `OnPanelShown`, `OnPanelHidden` for reactive integration
- **StateManager integration** — auto-show/hide panels mapped to each `AppState` (activated via `UIMANAGER_STM`)
- **CutsceneManager integration** — auto-hide HUD during sequences; restore on complete/skip (activated via `UIMANAGER_CSM`)
- **DialogueManager integration** — show/hide dialogue panel on dialogue start/complete (activated via `UIMANAGER_DM`)
- **MapLoaderFramework integration** — show loading panel on chapter change; hide on load (activated via `UIMANAGER_MLF`)
- **MiniGameManager integration** — show/hide mini-game HUD on start/complete/abort (activated via `UIMANAGER_MGM`)
- **LocalizationManager integration** — refresh visible panels' text on language change (activated via `UIMANAGER_LM`)
- **EventManager integration** — broadcast `ui.panel.shown/hidden` events (activated via `UIMANAGER_EM` or `EVENTMANAGER_UIM`)
- **Custom Inspector** — live panel controls, visible panel list, and registered panel catalogue in Play Mode


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
4. Add panel definitions in the Inspector or via `ui_panels.json`.
5. Attach any bridge components (see Bridge Components below).


## Quick Start

### Inspector Fields

| Field | Default | Description |
| ----- | ------- | ----------- |
| `panels` | *(empty)* | Built-in panel definitions |
| `hudPanelId` | `"hud"` | Id of the HUD panel (auto-hidden when a `hideHud` panel is shown) |
| `loadFromJson` | `false` | Merge definitions from `ui_panels.json` |
| `jsonPath` | `"ui_panels.json"` | Path relative to `StreamingAssets/` |
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

Place `ui_panels.json` in `StreamingAssets/` (path is configurable):

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
