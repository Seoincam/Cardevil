using Cardevil.Attributes;
using Cardevil.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Cardevil.Core
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class CameraSorter : MonoBehaviour
    {
        [SerializeField] private int stackOrder = 0;
        [SerializeField,Tooltip("에디터에서는 Base지만, Play들어가면 Overlay로")] private bool baseInEditMode = false;
        [SerializeField,VisibleOnly] private Camera cam;
        [SerializeField,VisibleOnly] private string cameraSceneName;
        [SerializeField] private Scenes scene;
        
        public int StackOrder => stackOrder;

        private void Reset()
        {
            cam = GetComponent<Camera>();
            cameraSceneName = gameObject.scene.name;
            if (Enum.TryParse(cameraSceneName, out Scenes parsedScene))
            {
                scene = parsedScene;
            }
            else
            {
                Debug.LogWarning($"CameraSorter: Scene name '{cameraSceneName}' does not match any value in Scenes enum.");
            }
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cameraSceneName = gameObject.scene.name;
            if (Application.isPlaying)
            {
                cam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
                CameraManager.Instance.RegisterCamera(cam);
            }
            else
            {
                cam.GetUniversalAdditionalCameraData().renderType = baseInEditMode ? CameraRenderType.Base : CameraRenderType.Overlay;
            }
        }
        
        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                #if UNITY_EDITOR
                if (CameraManager.Instance != null)
                    CameraManager.Instance.UnregisterCamera(cam);
                #else
                CameraStackManager.Instance.UnregisterCamera(cam);
                #endif
            }
        }


        private void OnValidate()
        {
            if (cam == null)
                cam = GetComponent<Camera>();
            
            if (!Application.isPlaying)
            {
                cam.GetUniversalAdditionalCameraData().renderType = baseInEditMode ? CameraRenderType.Base : CameraRenderType.Overlay;
            }
        }
    }
}