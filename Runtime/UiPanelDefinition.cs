using System;
using System.Collections.Generic;
using UnityEngine;

namespace UiManager.Runtime
{
    /// <summary>
    /// Defines a single UI panel managed by <see cref="UiManager"/>.
    /// </summary>
    [Serializable]
    public class UiPanelDefinition
    {
        [Tooltip("Unique identifier for this panel.")]
        public string id;

        [Tooltip("Human-readable display name.")]
        public string displayName;

        [Tooltip("Resources-relative path to the panel prefab.")]
        public string prefabPath;

        [Tooltip("When true, this panel is treated as a modal overlay (blocks interaction below).")]
        public bool modal;

        [Tooltip("When true, the HUD panel is hidden while this panel is visible.")]
        public bool hideHud;

        [Tooltip("Category tag, e.g. 'hud', 'overlay', 'menu'.")]
        public string category;
    }

    /// <summary>
    /// JSON root wrapper used when loading panel definitions from StreamingAssets.
    /// </summary>
    [Serializable]
    internal class UiPanelManifestJson
    {
        public List<UiPanelDefinition> panels = new List<UiPanelDefinition>();
    }
}
