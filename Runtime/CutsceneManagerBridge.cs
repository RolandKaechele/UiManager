#if UIMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and CutsceneManager.
    /// Enable define <c>UIMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Hides the HUD panel when a cutscene sequence starts and restores it when the
    /// sequence completes or is skipped.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Cutscene Manager Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneManagerBridge : MonoBehaviour
    {
        [Tooltip("Id of the HUD panel to hide during cutscenes.")]
        [SerializeField] private string hudPanelId = "hud";

        private UiManager _ui;
        private CutsceneManager.Runtime.CutsceneManager _csm;
        private bool _hudWasVisible;

        private void Awake()
        {
            _ui  = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _csm = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                   ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_ui  == null) Debug.LogWarning("[UiManager/CutsceneManagerBridge] UiManager not found.");
            if (_csm == null) Debug.LogWarning("[UiManager/CutsceneManagerBridge] CutsceneManager not found.");
        }

        private void OnEnable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   += OnSequenceStarted;
                _csm.OnSequenceCompleted += OnSequenceEnded;
                _csm.OnSequenceSkipped   += OnSequenceEnded;
            }
        }

        private void OnDisable()
        {
            if (_csm != null)
            {
                _csm.OnSequenceStarted   -= OnSequenceStarted;
                _csm.OnSequenceCompleted -= OnSequenceEnded;
                _csm.OnSequenceSkipped   -= OnSequenceEnded;
            }
        }

        private void OnSequenceStarted(string sequenceId)
        {
            if (_ui == null || string.IsNullOrEmpty(hudPanelId)) return;
            _hudWasVisible = _ui.IsPanelVisible(hudPanelId);
            if (_hudWasVisible) _ui.HidePanel(hudPanelId);
        }

        private void OnSequenceEnded(string sequenceId)
        {
            if (_ui == null || string.IsNullOrEmpty(hudPanelId)) return;
            if (_hudWasVisible) _ui.ShowPanel(hudPanelId);
        }
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_CSM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Cutscene Manager Bridge")]
    public class CutsceneManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/CutsceneManagerBridge] Bridge disabled — add UIMANAGER_CSM to Scripting Define Symbols.");
    }
}
#endif
