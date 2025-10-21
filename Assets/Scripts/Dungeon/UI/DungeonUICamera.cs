using DG.Tweening;
using DG.Tweening.Core;
using System;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    /// <summary>
    /// 던전 UI 카메라 래퍼
    /// </summary>
    public class DungeonUICamera : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] Ease _moveEase = Ease.InOutSine;
        [SerializeField] Canvas _dungeonUICanvas = null;
        
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                    if (_camera == null)
                    {
                        Debug.LogError("DungeonUICamera: No Camera component found on the GameObject.");
                    }
                }
                return _camera;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (_dungeonUICanvas == null)
                {
                    _dungeonUICanvas = GetComponentInParent<Canvas>();
                    if (_dungeonUICanvas == null)
                    {
                        Debug.LogError("DungeonUICamera: No Canvas component found in parent GameObjects.");
                    }
                }
                return _dungeonUICanvas;
            }
        }
        private void Reset()
        {
            _camera = GetComponent<Camera>();
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }
        }


        /// <summary>
        /// 카메라를 지정된 위치로 부드럽게 이동시킵니다. 
        /// </summary>
        /// <param name="position"></param>
        public TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> MoveTo(Vector2 position)
        {
            DOTween.Kill(transform);
            var saved = Canvas.renderMode;
            SetCanvasWorldSpace();
            var tween = transform
                .DOMove(new Vector3(position.x, position.y, transform.position.z), 0.5f)
                .SetEase(_moveEase);
            Canvas.renderMode = saved;
            return tween;
        }
        
        public void SetCanvasWorldSpace()
        {
            Canvas.renderMode = RenderMode.WorldSpace;
        }

        public void SetCanvasCameraSpace()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
    }
}
