using Cardevil.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Cardevil.Core
{
    public class CameraManager : Singleton<CameraManager>
    {
        private int largestStackOrder = 0;
        private Camera cachedMainCamera;

        public Camera MainCamera
        {
            get
            {
                if (cachedMainCamera == null)
                {
                    cachedMainCamera = Camera.main;
                }
                return cachedMainCamera;
            }
        }
        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterCamera(Camera camera)
        {
            var stack = MainCamera.GetUniversalAdditionalCameraData().cameraStack;
            stack.Add(camera);
            stack.Sort(CompareCamera);
            largestStackOrder = stack .Count > 0 ? stack[^1].GetComponent<CameraSorter>()?.StackOrder ?? 0 : 0;
        }

        public void UnregisterCamera(Camera camera)
        {
            var stack = MainCamera.GetUniversalAdditionalCameraData().cameraStack;
            stack.Remove(camera);
            
            largestStackOrder = stack .Count > 0 ? stack[^1].GetComponent<CameraSorter>()?.StackOrder ?? 0 : 0;
        }

        public void EnableSceneCameras(Scenes scene) => EnableSceneCameras(scene.ToString());

        public void EnableSceneCameras(string sceneName)
        {
            var stack = MainCamera.GetUniversalAdditionalCameraData().cameraStack;
            foreach (var cam in stack)
            {
                if (cam.scene.name == sceneName)
                {
                    cam.enabled = true;
                }
            }
        }
        public void DisableSceneCameras(Scenes scene) => DisableSceneCameras(scene.ToString());
        public void DisableSceneCameras(string sceneName)
        {
            var stack = MainCamera.GetUniversalAdditionalCameraData().cameraStack;
            foreach (var cam in stack)
            {
                if (cam.scene.name == sceneName)
                {
                    cam.enabled = false;
                }
            }
        }
        
        

        private int CompareCamera(Camera a, Camera b)
        {
            var orderA = a.GetComponent<CameraSorter>()?.StackOrder ?? 0;
            var orderB = b.GetComponent<CameraSorter>()?.StackOrder ?? 0;
            return orderA.CompareTo(orderB);
        }
    }
}