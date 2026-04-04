#if UIMANAGER_LM
using UnityEngine;
using LocalizationManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and LocalizationManager.
    /// Enable define <c>UIMANAGER_LM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires <see cref="UiManager.OnPanelShown"/> for the currently-visible panels whenever
    /// <see cref="LocalizationManager.Runtime.LocalizationManager.OnLanguageChanged"/> triggers,
    /// prompting any UI text refresh logic that is subscribed to OnPanelShown.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Localization Manager Bridge")]
    [DisallowMultipleComponent]
    public class LocalizationManagerBridge : MonoBehaviour
    {
        private UiManager _ui;
        private LocalizationManager.Runtime.LocalizationManager _loc;

        private void Awake()
        {
            _ui  = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _loc = GetComponent<LocalizationManager.Runtime.LocalizationManager>()
                   ?? FindFirstObjectByType<LocalizationManager.Runtime.LocalizationManager>();

            if (_ui  == null) Debug.LogWarning("[UiManager/LocalizationManagerBridge] UiManager not found.");
            if (_loc == null) Debug.LogWarning("[UiManager/LocalizationManagerBridge] LocalizationManager not found.");
        }

        private void OnEnable()
        {
            if (_loc != null) _loc.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            if (_loc != null) _loc.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(string languageCode)
        {
            if (_ui == null) return;
            // Notify each visible panel so its text components can refresh.
            foreach (var id in _ui.VisiblePanels)
                _ui.ShowPanel(id);   // triggers OnPanelShown to prompt text refresh
        }
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_LM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Localization Manager Bridge")]
    public class LocalizationManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/LocalizationManagerBridge] Bridge disabled — add UIMANAGER_LM to Scripting Define Symbols.");
    }
}
#endif
