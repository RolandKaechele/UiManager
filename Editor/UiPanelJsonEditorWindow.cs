#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UiManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace UiManager.Editor
{
    // ────────────────────────────────────────────────────────────────────────────
    // UI Panels JSON Editor Window
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Editor window for creating and editing <c>ui_panels.json</c> in StreamingAssets.
    /// Open via <b>JSON Editors → UI Manager</b> or via the Manager Inspector button.
    /// </summary>
    public class UiPanelJsonEditorWindow : EditorWindow
    {
        private const string JsonFolderName = "ui_panels";

        private UiPanelEditorBridge      _bridge;
        private UnityEditor.Editor       _bridgeEditor;
        private Vector2                  _scroll;
        private string                   _status;
        private bool                     _statusError;

        [MenuItem("JSON Editors/UI Manager")]
        public static void ShowWindow() =>
            GetWindow<UiPanelJsonEditorWindow>("UI Panels JSON");

        private void OnEnable()
        {
            _bridge = CreateInstance<UiPanelEditorBridge>();
            Load();
        }

        private void OnDisable()
        {
            if (_bridgeEditor != null) DestroyImmediate(_bridgeEditor);
            if (_bridge      != null) DestroyImmediate(_bridge);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);

            if (_bridge == null) return;
            if (_bridgeEditor == null)
                _bridgeEditor = UnityEditor.Editor.CreateEditor(_bridge);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _bridgeEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(
                $"StreamingAssets/{JsonFolderName}/",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
            try
            {
                var list = new List<UiPanelDefinition>();
                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        var w = JsonUtility.FromJson<UiPanelEditorWrapper>(File.ReadAllText(file));
                        if (w?.panels != null) list.AddRange(w.panels);
                    }
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.Refresh();
                }
                _bridge.panels = list;
                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }
                _status = $"Loaded {list.Count} UI panels from {JsonFolderName}/.";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Load error: {e.Message}"; _statusError = true; }
        }

        private void Save()
        {
            try
            {
                string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                int saved = 0;
                foreach (var entry in _bridge.panels)
                {
                    if (string.IsNullOrEmpty(entry.id)) continue;
                    var w = new UiPanelEditorWrapper { panels = new[] { entry } };
                    File.WriteAllText(Path.Combine(folderPath, $"{entry.id}.json"), JsonUtility.ToJson(w, true));
                    saved++;
                }
                AssetDatabase.Refresh();
                _status = $"Saved {saved} UI panel file(s) to {JsonFolderName}/";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Save error: {e.Message}"; _statusError = true; }
        }
    }

    // ── ScriptableObject bridge ──────────────────────────────────────────────
    internal class UiPanelEditorBridge : ScriptableObject
    {
        public List<UiPanelDefinition> panels = new List<UiPanelDefinition>();
    }

    // ── Local wrapper mirrors the internal UiPanelManifestJson ───────────────
    [Serializable]
    internal class UiPanelEditorWrapper
    {
        public UiPanelDefinition[] panels = Array.Empty<UiPanelDefinition>();
    }
}
#endif
