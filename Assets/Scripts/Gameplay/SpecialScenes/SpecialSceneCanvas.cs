using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class SpecialSceneCanvas : MonoBehaviour
    {
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public Camera WorldCamera { get; private set; }

        private void Reset()
        {
            Canvas = GetComponent<Canvas>();
            WorldCamera = FindSceneCamera();
            ApplyCanvasSettings();
        }

        private void Awake()
        {
            if (!Canvas)
            {
                Canvas = GetComponent<Canvas>();
            }

            if (!WorldCamera)
            {
                WorldCamera = FindSceneCamera();
            }

            ApplyCanvasSettings();
        }

        public static SpecialSceneCanvas FindInScene(Scene scene)
        {
            var canvases =
                Object.FindObjectsByType<SpecialSceneCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var candidate in canvases)
            {
                if (candidate.gameObject.scene == scene)
                {
                    return candidate;
                }
            }

            return null;
        }

        private Camera FindSceneCamera()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var candidate in cameras)
            {
                if (candidate.gameObject.scene == gameObject.scene)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ApplyCanvasSettings()
        {
            if (!Canvas)
            {
                return;
            }

            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = WorldCamera;
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = 200;

            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0f;
        }
    }
}
