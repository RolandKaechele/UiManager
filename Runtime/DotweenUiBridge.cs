#if UIMANAGER_DOTWEEN
using UnityEngine;
using DG.Tweening;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge that adds DOTween-driven fade-in and punch-scale animations to panels
    /// managed by <see cref="UiManager"/>.
    /// Enable define <c>UIMANAGER_DOTWEEN</c> in Player Settings › Scripting Define Symbols.
    /// Requires <b>DOTween Pro</b>.
    /// <para>
    /// Listens for <see cref="UiManager.OnPanelShown"/> and animates the panel's
    /// <see cref="CanvasGroup"/> from alpha 0 to 1. Ensure each panel prefab has a
    /// <see cref="CanvasGroup"/> on its root for fade support.
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/DOTween Bridge")]
    [DisallowMultipleComponent]
    public class DotweenUiBridge : MonoBehaviour
    {
        [Header("Fade In")]
        [Tooltip("Duration of the panel canvas-group fade-in on show.")]
        [SerializeField] private float fadeInDuration = 0.25f;

        [Tooltip("DOTween ease applied to the panel fade-in.")]
        [SerializeField] private Ease fadeInEase = Ease.OutQuad;

        [Header("Punch Scale")]
        [Tooltip("When true, newly shown panels play a punch-scale animation on their root transform.")]
        [SerializeField] private bool usePunchScale = true;

        [Tooltip("Punch vector applied to the panel root on show (world-space scale delta).")]
        [SerializeField] private Vector3 punchScale = new Vector3(0.06f, 0.06f, 0f);

        [Tooltip("Duration of the punch-scale animation.")]
        [SerializeField] private float punchDuration = 0.35f;

        [Tooltip("Vibrato count for the punch-scale animation.")]
        [SerializeField] private int punchVibrato = 6;

        [Tooltip("Elasticity for the punch-scale animation (0 = rigid, 1 = elastic).")]
        [Range(0f, 1f)]
        [SerializeField] private float punchElasticity = 0.5f;

        // -------------------------------------------------------------------------

        private UiManager _ui;

        private void Awake()
        {
            _ui = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            if (_ui == null) Debug.LogWarning("[UiManager/DotweenUiBridge] UiManager not found.");
        }

        private void OnEnable()
        {
            if (_ui != null) _ui.OnPanelShown += OnPanelShown;
        }

        private void OnDisable()
        {
            if (_ui != null) _ui.OnPanelShown -= OnPanelShown;
        }

        // -------------------------------------------------------------------------

        private void OnPanelShown(string id)
        {
            // UiManager names its panel instances "UI_<id>" — locate by that convention.
            var panelGo = GameObject.Find("UI_" + id);
            if (panelGo == null) return;

            var cg = panelGo.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                DOTween.Kill(cg);
                cg.alpha = 0f;
                cg.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
            }

            if (usePunchScale)
            {
                DOTween.Kill(panelGo.transform);
                panelGo.transform.DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity);
            }
        }
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_DOTWEEN</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/DOTween Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DotweenUiBridge : UnityEngine.MonoBehaviour { }
}
#endif
