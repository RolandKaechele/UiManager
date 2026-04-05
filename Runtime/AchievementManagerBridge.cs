#if UIMANAGER_ACH
using UnityEngine;
using AchievementManager.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and AchievementManager.
    /// Enable define <c>UIMANAGER_ACH</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Shows an achievement notification panel whenever an achievement is unlocked.
    /// Assign a panel with id <see cref="notificationPanelId"/> (default: <c>"achievement_notification"</c>)
    /// in the UiManager Inspector. The panel is shown and then automatically hidden after
    /// <see cref="autoHideDelay"/> seconds (0 = no auto-hide).
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Achievement Manager Bridge")]
    [DisallowMultipleComponent]
    public class AchievementManagerBridge : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [Tooltip("Panel id to show when an achievement is unlocked.")]
        [SerializeField] private string notificationPanelId = "achievement_notification";

        [Tooltip("Seconds to keep the notification visible before auto-hiding it. Set 0 to disable auto-hide.")]
        [SerializeField] private float autoHideDelay = 3f;

        // ─── References ───────────────────────────────────────────────────────
        private UiManager _ui;
        private AchievementManager.Runtime.AchievementManager _achievements;

        private Coroutine _hideCoroutine;

        // ─── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _ui           = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _achievements = GetComponent<AchievementManager.Runtime.AchievementManager>()
                            ?? FindFirstObjectByType<AchievementManager.Runtime.AchievementManager>();

            if (_ui           == null) Debug.LogWarning("[UiManager/AchievementManagerBridge] UiManager not found.");
            if (_achievements == null) Debug.LogWarning("[UiManager/AchievementManagerBridge] AchievementManager not found.");
        }

        private void OnEnable()
        {
            if (_achievements != null)
                _achievements.OnAchievementUnlocked += OnAchievementUnlocked;
        }

        private void OnDisable()
        {
            if (_achievements != null)
                _achievements.OnAchievementUnlocked -= OnAchievementUnlocked;

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
        }

        // ─── Handlers ─────────────────────────────────────────────────────────
        private void OnAchievementUnlocked(string _id)
        {
            if (_ui == null || string.IsNullOrEmpty(notificationPanelId)) return;

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _ui.ShowPanel(notificationPanelId);

            if (autoHideDelay > 0f)
                _hideCoroutine = StartCoroutine(AutoHide());
        }

        private System.Collections.IEnumerator AutoHide()
        {
            yield return new UnityEngine.WaitForSeconds(autoHideDelay);
            _ui?.HidePanel(notificationPanelId);
            _hideCoroutine = null;
        }
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub – enable define <c>UIMANAGER_ACH</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Achievement Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class AchievementManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
