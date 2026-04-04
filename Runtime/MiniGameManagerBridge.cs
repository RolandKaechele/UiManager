#if UIMANAGER_MGM
using UnityEngine;
using MiniGameManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and MiniGameManager.
    /// Enable define <c>UIMANAGER_MGM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Shows a mini-game HUD panel when a mini-game starts and hides it when the game
    /// completes or is aborted.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Mini Game Manager Bridge")]
    [DisallowMultipleComponent]
    public class MiniGameManagerBridge : MonoBehaviour
    {
        [Tooltip("Id of the mini-game HUD panel.")]
        [SerializeField] private string miniGamePanelId = "minigame_hud";

        private UiManager _ui;
        private MiniGameManager.Runtime.MiniGameManager _mgm;

        private void Awake()
        {
            _ui  = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _mgm = GetComponent<MiniGameManager.Runtime.MiniGameManager>()
                   ?? FindFirstObjectByType<MiniGameManager.Runtime.MiniGameManager>();

            if (_ui  == null) Debug.LogWarning("[UiManager/MiniGameManagerBridge] UiManager not found.");
            if (_mgm == null) Debug.LogWarning("[UiManager/MiniGameManagerBridge] MiniGameManager not found.");
        }

        private void OnEnable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   += OnMiniGameStarted;
                _mgm.OnMiniGameCompleted += OnMiniGameEnded;
                _mgm.OnMiniGameAborted   += OnMiniGameAborted;
            }
        }

        private void OnDisable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   -= OnMiniGameStarted;
                _mgm.OnMiniGameCompleted -= OnMiniGameEnded;
                _mgm.OnMiniGameAborted   -= OnMiniGameAborted;
            }
        }

        private void OnMiniGameStarted(string id)             => _ui?.ShowPanel(miniGamePanelId);
        private void OnMiniGameEnded(MiniGameResult result)   => _ui?.HidePanel(miniGamePanelId);
        private void OnMiniGameAborted(string id)             => _ui?.HidePanel(miniGamePanelId);
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_MGM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Mini Game Manager Bridge")]
    public class MiniGameManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/MiniGameManagerBridge] Bridge disabled — add UIMANAGER_MGM to Scripting Define Symbols.");
    }
}
#endif
