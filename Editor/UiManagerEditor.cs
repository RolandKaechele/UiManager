using System;
using System.Collections.Generic;
using System.IO;
using UiManager.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace UiManager.Editor
{
    // ODIN Inspector: when ODIN_INSPECTOR is defined the manager extends
    // SerializedMonoBehaviour. The custom editor must derive from OdinEditor
    // so ODIN's full property tree is drawn correctly.
#if ODIN_INSPECTOR
    [CustomEditor(typeof(UiManager.Runtime.UiManager))]
    public class UiManagerEditor : OdinEditor
#else
    [CustomEditor(typeof(UiManager.Runtime.UiManager))]
    public class UiManagerEditor : UnityEditor.Editor
#endif
    {
        private string _previewId = string.Empty;

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI() renders the full ODIN property tree when
            // inheriting from OdinEditor, or the standard Unity inspector otherwise.
            base.OnInspectorGUI();

            var manager = (UiManager.Runtime.UiManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use live controls.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visible Panels", EditorStyles.boldLabel);
            foreach (var id in manager.VisiblePanels)
                EditorGUILayout.LabelField("  •", id);

            EditorGUILayout.Space();
            _previewId = EditorGUILayout.TextField("Panel Id", _previewId);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Panel"))  manager.ShowPanel(_previewId);
            if (GUILayout.Button("Hide Panel"))  manager.HidePanel(_previewId);
            if (GUILayout.Button("Hide All"))    manager.HideAll();
            EditorGUILayout.EndHorizontal();

            if (manager.Panels.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Registered Panels", EditorStyles.boldLabel);
                foreach (var kvp in manager.Panels)
                {
                    bool visible = manager.IsPanelVisible(kvp.Key);
                    EditorGUILayout.LabelField($"  {(visible ? "✔" : "○")} {kvp.Key}", kvp.Value.displayName ?? kvp.Key);
                }
            }

            Repaint();
        }
    }

    // ── Prefab generation ──────────────────────────────────────────────────────

    [Serializable]
    internal class UiManifestProxy
    {
        public List<UiPanelDefinition> panels = new List<UiPanelDefinition>();
    }

    /// <summary>
    /// Generates UI panel prefabs from ui_panels.json. Lives in UiManager.Editor so it
    /// has direct access to all UiPanelDefinition fields without a separate assembly.
    /// </summary>
    internal static class UiPrefabHelper
    {
        internal const string UiJson = "Assets/StreamingAssets/ui_panels.json";
        private  const string OutDir = "Assets/Resources/Prefabs/UI";

        [MenuItem("Generate Prefabs/UI Panels", priority = 102)]
        public static void GenerateUiPrefabs()
        {
            var path = Path.Combine(Application.dataPath, "StreamingAssets", "ui_panels.json");
            if (!File.Exists(path)) { Debug.LogError("[UiPrefabHelper] ui_panels.json not found."); return; }

            var manifest = JsonUtility.FromJson<UiManifestProxy>(File.ReadAllText(path));
            if (manifest?.panels == null || manifest.panels.Count == 0)
            { Debug.LogWarning("[UiPrefabHelper] ui_panels.json contained no entries."); return; }

            EnsureDirectory(OutDir);

            int n = 0;
            foreach (var panel in manifest.panels)
            {
                if (string.IsNullOrEmpty(panel.id)) continue;
                var go = BuildUiGo(panel);
                var fname = !string.IsNullOrEmpty(panel.prefabPath)
                    ? Path.GetFileName(panel.prefabPath)
                    : panel.id;
                SavePrefab(go, $"{OutDir}/{fname}.prefab");
                UnityEngine.Object.DestroyImmediate(go);
                n++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UiPrefabHelper] Generated {n} UI prefabs \u2192 {OutDir}");
        }

        private static GameObject BuildUiGo(UiPanelDefinition panel)
        {
            var go = new GameObject(panel.id);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha         = panel.id == "hud" ? 1f : 0f;
            cg.interactable  = true;
            cg.blocksRaycasts = panel.modal;

            bool fullscreen = panel.modal
                || panel.id is "load_screen" or "fade_screen"
                or "game_over_screen" or "victory_screen" or "chapter_complete";

            if (fullscreen)
            {
                var img = go.AddComponent<Image>();
                img.color         = panel.id == "fade_screen" ? Color.black : new Color(0f, 0f, 0f, 0.75f);
                img.raycastTarget = panel.modal;
            }

            return go;
        }

        private static void SavePrefab(GameObject go, string assetPath)
        {
            bool ex = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null;
            PrefabUtility.SaveAsPrefabAsset(go, assetPath, out bool ok);
            if (!ok) Debug.LogWarning($"[PrefabGen] \u2717 {assetPath}");
            else     Debug.Log(ex ? $"[PrefabGen] \u21ba {assetPath}" : $"[PrefabGen] \u2713 {assetPath}");
        }

        private static void EnsureDirectory(string assetPath)
        {
            Directory.CreateDirectory(Path.Combine(
                Path.GetDirectoryName(Application.dataPath)!,
                assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        [UnityEditor.InitializeOnLoadMethod]
        static void RegisterWithPrefabGenerator()
        {
            PrefabGenerator.Register("UI Panels", GenerateUiPrefabs);
        }
    }

    internal class UiPrefabPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var p in imported)
            {
                if (p == UiPrefabHelper.UiJson)
                {
                    UiPrefabHelper.GenerateUiPrefabs();
                    return;
                }
            }
        }
    }
}
