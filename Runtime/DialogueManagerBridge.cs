#if UIMANAGER_DM
using UnityEngine;
using DialogueManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and DialogueManager.
    /// Enable define <c>UIMANAGER_DM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Shows the dialogue panel when <see cref="DialogueManager.Runtime.DialogueManager.OnDialogueStarted"/> fires
    /// and hides it when <see cref="DialogueManager.Runtime.DialogueManager.OnDialogueCompleted"/> fires.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Dialogue Manager Bridge")]
    [DisallowMultipleComponent]
    public class DialogueManagerBridge : MonoBehaviour
    {
        [Tooltip("Id of the dialogue panel to show/hide.")]
        [SerializeField] private string dialoguePanelId = "dialogue";

        private UiManager _ui;
        private DialogueManager.Runtime.DialogueManager _dm;

        private void Awake()
        {
            _ui = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _dm = GetComponent<DialogueManager.Runtime.DialogueManager>()
                  ?? FindFirstObjectByType<DialogueManager.Runtime.DialogueManager>();

            if (_ui == null) Debug.LogWarning("[UiManager/DialogueManagerBridge] UiManager not found.");
            if (_dm == null) Debug.LogWarning("[UiManager/DialogueManagerBridge] DialogueManager not found.");
        }

        private void OnEnable()
        {
            if (_dm != null)
            {
                _dm.OnDialogueStarted   += OnDialogueStarted;
                _dm.OnDialogueCompleted += OnDialogueCompleted;
            }
        }

        private void OnDisable()
        {
            if (_dm != null)
            {
                _dm.OnDialogueStarted   -= OnDialogueStarted;
                _dm.OnDialogueCompleted -= OnDialogueCompleted;
            }
        }

        private void OnDialogueStarted(string sequenceId)   => _ui?.ShowPanel(dialoguePanelId);
        private void OnDialogueCompleted(string sequenceId) => _ui?.HidePanel(dialoguePanelId);
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_DM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Dialogue Manager Bridge")]
    public class DialogueManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/DialogueManagerBridge] Bridge disabled — add UIMANAGER_DM to Scripting Define Symbols.");
    }
}
#endif
