#if UIMANAGER_EM
using UnityEngine;
using EventManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and EventManager.
    /// Enable define <c>UIMANAGER_EM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named <see cref="GameEvent"/>s:
    /// <list type="bullet">
    ///   <item><c>"ui.panel.shown"</c>  — <see cref="GameEvent.stringValue"/> = panel id</item>
    ///   <item><c>"ui.panel.hidden"</c> — <see cref="GameEvent.stringValue"/> = panel id</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Event Manager Bridge")]
    [DisallowMultipleComponent]
    public class EventManagerBridge : MonoBehaviour
    {
        [Tooltip("Event name fired when a panel is shown.")]
        [SerializeField] private string shownEventName  = "ui.panel.shown";

        [Tooltip("Event name fired when a panel is hidden.")]
        [SerializeField] private string hiddenEventName = "ui.panel.hidden";

        private EventManager.Runtime.EventManager _events;
        private UiManager _ui;

        private void Awake()
        {
            _events = GetComponent<EventManager.Runtime.EventManager>()
                      ?? FindFirstObjectByType<EventManager.Runtime.EventManager>();
            _ui     = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();

            if (_events == null) Debug.LogWarning("[UiManager/EventManagerBridge] EventManager not found.");
            if (_ui     == null) Debug.LogWarning("[UiManager/EventManagerBridge] UiManager not found.");
        }

        private void OnEnable()
        {
            if (_ui != null)
            {
                _ui.OnPanelShown  += OnPanelShown;
                _ui.OnPanelHidden += OnPanelHidden;
            }
        }

        private void OnDisable()
        {
            if (_ui != null)
            {
                _ui.OnPanelShown  -= OnPanelShown;
                _ui.OnPanelHidden -= OnPanelHidden;
            }
        }

        private void OnPanelShown(string id)  => _events?.Fire(new GameEvent(shownEventName,  id));
        private void OnPanelHidden(string id) => _events?.Fire(new GameEvent(hiddenEventName, id));
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_EM</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Event Manager Bridge")]
    public class EventManagerBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/EventManagerBridge] Bridge disabled — add UIMANAGER_EM to Scripting Define Symbols.");
    }
}
#endif
