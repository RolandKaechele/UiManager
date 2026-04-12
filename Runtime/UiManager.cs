using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UiManager.Runtime
{
    /// <summary>
    /// Centralized UI panel manager for Unity.
    /// Manages show/hide of registered panels, supports modal overlays, JSON-driven
    /// panel definitions for modding, and exposes events for cross-module integration.
    /// </summary>
    [AddComponentMenu("Managers/UI Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class UiManager : SerializedMonoBehaviour
#else
    public class UiManager : MonoBehaviour
#endif
    {
        // ──────────────────────────────────────────────────────────
        // Inspector fields
        // ──────────────────────────────────────────────────────────

        [Header("Panels")]
        [Tooltip("Built-in panel definitions. JSON entries are merged on top by id.")]
        [SerializeField] private List<UiPanelDefinition> panels = new List<UiPanelDefinition>();

        [Header("HUD")]
        [Tooltip("Id of the HUD panel. Auto-hidden when a panel with hideHud=true is shown.")]
        [SerializeField] private string hudPanelId = "hud";

        [Header("JSON / Modding")]
        [Tooltip("Load additional panel definitions from StreamingAssets/<jsonPath>.")]
        [SerializeField] private bool loadFromJson;

        [Tooltip("Path relative to StreamingAssets/.")]
        [SerializeField] private string jsonPath = "ui_panels/";

        [Header("Debug")]
        [Tooltip("Log all show/hide calls to the Console.")]
        [SerializeField] private bool verboseLogging;

        // ──────────────────────────────────────────────────────────
        // Events
        // ──────────────────────────────────────────────────────────

        /// <summary>Fired when a panel becomes visible. Parameter is the panel id.</summary>
        public event Action<string> OnPanelShown;

        /// <summary>Fired when a panel is hidden. Parameter is the panel id.</summary>
        public event Action<string> OnPanelHidden;

        // ──────────────────────────────────────────────────────────
        // State
        // ──────────────────────────────────────────────────────────

        private readonly Dictionary<string, UiPanelDefinition> _map = new Dictionary<string, UiPanelDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _instances  = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _visiblePanels             = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ──────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────

        /// <summary>All registered panel definitions, keyed by id.</summary>
        public IReadOnlyDictionary<string, UiPanelDefinition> Panels => _map;

        /// <summary>Returns true if the panel with the given id is currently visible.</summary>
        public bool IsPanelVisible(string id) => !string.IsNullOrEmpty(id) && _visiblePanels.Contains(id);

        /// <summary>Shows the panel with the given id.</summary>
        public void ShowPanel(string id)
        {
            if (!TryGetDefinition(id, out var def)) return;
            if (_visiblePanels.Contains(id)) return;

            ShowInternal(def);
        }

        /// <summary>Hides the panel with the given id.</summary>
        public void HidePanel(string id)
        {
            if (!_visiblePanels.Contains(id)) return;
            HideInternal(id);
        }

        /// <summary>Shows a modal panel, optionally hiding the HUD.</summary>
        public void ShowModal(string id)
        {
            if (!TryGetDefinition(id, out _)) return;
            ShowPanel(id);
        }

        /// <summary>Hides a modal panel.</summary>
        public void HideModal(string id) => HidePanel(id);

        /// <summary>Hides all currently visible panels.</summary>
        public void HideAll()
        {
            var copy = new List<string>(_visiblePanels);
            foreach (var id in copy) HideInternal(id);
        }

        /// <summary>Returns a snapshot of currently visible panel ids.</summary>
        public IReadOnlyCollection<string> VisiblePanels => _visiblePanels;

        // ──────────────────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────────────────

        private void Awake()
        {
            BuildMap();
            if (loadFromJson) LoadJson();
        }

        // ──────────────────────────────────────────────────────────
        // Internal helpers
        // ──────────────────────────────────────────────────────────

        private void ShowInternal(UiPanelDefinition def)
        {
            var instance = GetOrCreateInstance(def);
            if (instance != null) instance.SetActive(true);

            _visiblePanels.Add(def.id);

            if (def.hideHud && !string.IsNullOrEmpty(hudPanelId) && def.id != hudPanelId)
                HidePanel(hudPanelId);

            if (verboseLogging)
                Debug.Log($"[UiManager] Shown: {def.id}");

            OnPanelShown?.Invoke(def.id);
        }

        private void HideInternal(string id)
        {
            if (_instances.TryGetValue(id, out var instance) && instance != null)
                instance.SetActive(false);

            _visiblePanels.Remove(id);

            if (verboseLogging)
                Debug.Log($"[UiManager] Hidden: {id}");

            OnPanelHidden?.Invoke(id);
        }

        private GameObject GetOrCreateInstance(UiPanelDefinition def)
        {
            if (_instances.TryGetValue(def.id, out var existing) && existing != null)
                return existing;

            if (string.IsNullOrEmpty(def.prefabPath)) return null;

            var prefab = Resources.Load<GameObject>(def.prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[UiManager] Prefab not found at Resources/{def.prefabPath}");
                return null;
            }

            var go = Instantiate(prefab);
            go.name = $"UI_{def.id}";
            DontDestroyOnLoad(go);
            _instances[def.id] = go;
            return go;
        }

        private void BuildMap()
        {
            _map.Clear();
            foreach (var def in panels)
            {
                if (string.IsNullOrEmpty(def.id)) continue;
                _map[def.id] = def;
            }
        }

        private void LoadJson()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, jsonPath);
            if (Directory.Exists(fullPath))
            {
                foreach (var file in Directory.GetFiles(fullPath, "*.json", SearchOption.TopDirectoryOnly))
                    MergeUiPanelsFromFile(file);
            }
            else if (File.Exists(fullPath))
            {
                MergeUiPanelsFromFile(fullPath);
            }
            else
            {
                Debug.LogWarning($"[UiManager] JSON not found: {fullPath}");
            }
        }

        private void MergeUiPanelsFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var manifest = JsonUtility.FromJson<UiPanelManifestJson>(json);
                if (manifest?.panels == null) return;
                foreach (var def in manifest.panels)
                {
                    if (string.IsNullOrEmpty(def.id)) continue;
                    _map[def.id] = def;
                }
                if (verboseLogging)
                    Debug.Log($"[UiManager] Merged from {path}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UiManager] Failed to load JSON: {ex.Message}");
            }
        }

        private bool TryGetDefinition(string id, out UiPanelDefinition def)
        {
            if (string.IsNullOrEmpty(id) || !_map.TryGetValue(id, out def))
            {
                Debug.LogWarning($"[UiManager] Panel not found: '{id}'");
                def = null;
                return false;
            }
            return true;
        }
    }
}
