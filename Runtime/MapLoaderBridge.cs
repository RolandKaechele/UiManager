#if UIMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace UiManager.Runtime
{
    /// <summary>
    /// Optional bridge between UiManager and MapLoaderFramework.
    /// Enable define <c>UIMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Shows a loading panel when a chapter change begins (<see cref="MapLoaderManager.OnChapterChanged"/>)
    /// and hides it when the new map finishes loading (<see cref="MapLoaderManager.OnMapLoaded"/>).
    /// </para>
    /// </summary>
    [AddComponentMenu("UiManager/Map Loader Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderBridge : MonoBehaviour
    {
        [Tooltip("Id of the loading screen panel.")]
        [SerializeField] private string loadingPanelId = "loading";

        private UiManager _ui;
        private MapLoaderManager _mlf;

        private void Awake()
        {
            _ui  = GetComponent<UiManager>() ?? FindFirstObjectByType<UiManager>();
            _mlf = GetComponent<MapLoaderManager>() ?? FindFirstObjectByType<MapLoaderManager>();

            if (_ui  == null) Debug.LogWarning("[UiManager/MapLoaderBridge] UiManager not found.");
            if (_mlf == null) Debug.LogWarning("[UiManager/MapLoaderBridge] MapLoaderManager not found.");
        }

        private void OnEnable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged += OnChapterChanged;
                _mlf.OnMapLoaded      += OnMapLoaded;
            }
        }

        private void OnDisable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged -= OnChapterChanged;
                _mlf.OnMapLoaded      -= OnMapLoaded;
            }
        }

        private void OnChapterChanged(int previous, int current) => _ui?.ShowPanel(loadingPanelId);
        private void OnMapLoaded(MapData mapData)                 => _ui?.HidePanel(loadingPanelId);
    }
}
#else
namespace UiManager.Runtime
{
    /// <summary>No-op stub — enable define <c>UIMANAGER_MLF</c> to activate.</summary>
    [UnityEngine.AddComponentMenu("UiManager/Map Loader Bridge")]
    public class MapLoaderBridge : UnityEngine.MonoBehaviour
    {
        private void Awake() =>
            UnityEngine.Debug.Log("[UiManager/MapLoaderBridge] Bridge disabled — add UIMANAGER_MLF to Scripting Define Symbols.");
    }
}
#endif
