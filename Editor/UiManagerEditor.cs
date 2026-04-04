using UnityEditor;
using UnityEngine;

namespace UiManager.Editor
{
    [CustomEditor(typeof(UiManager.Runtime.UiManager))]
    public class UiManagerEditor : UnityEditor.Editor
    {
        private string _previewId = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

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
}
