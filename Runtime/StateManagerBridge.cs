#if UIMANAGER_STM
using System.Collections.Generic;
using UnityEngine;
using StateManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and StateManager.
    /// Enable define <c>UIMANAGER_STM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Shows and hides registered panels automatically when the app state changes.
    /// Each mapping specifies which panel to show and which to hide for a given <see cref="AppState"/>.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/State Manager Bridge")]
    [DisallowMultipleComponent]
    public class StateManagerBridge : MonoBehaviour
    {
        [System.Serializable]
        public class StatePanelMapping
        {
            [Tooltip("Application state that triggers this mapping.")]
            public AppState state;

            [Tooltip("Panel ids to show when this state becomes active.")]
            public List<string> showPanels = new List<string>();

            [Tooltip("Panel ids to hide when this state becomes active.")]
            public List<string> hidePanels = new List<string>();
        }

        [SerializeField] private List<StatePanelMapping> stateMappings = new List<StatePanelMapping>();

        private UiManager _ui;
        private StateManager.Runtime.StateManager _state;

        private void Awake()
        {
            _ui    = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _state = GetComponent<StateManager.Runtime.StateManager>()
                     ?? FindFirstObjectByType<StateManager.Runtime.StateManager>();

            if (_ui    == null) Debug.LogWarning("[UiManager/StateManagerBridge] UiManager not found.");
            if (_state == null) Debug.LogWarning("[UiManager/StateManagerBridge] StateManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null) _state.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (_state != null) _state.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_ui == null) return;
            foreach (var mapping in stateMappings)
            {
                if (mapping.state != next) continue;
                foreach (var id in mapping.showPanels) _ui.ShowPanel(id);
                foreach (var id in mapping.hidePanels) _ui.HidePanel(id);
                return;
            }
        }
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_STM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/State Manager Bridge")]
    public class StateManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/StateManagerBridge] Bridge disabled — add UIMANAGER_STM to Scripting Define Symbols.");
    }
}
#endif
