using UnityEngine;

namespace Cardevil.Gameplay
{
    /// <summary>
    /// 아이소메트릭에서 스프라이트가 항상 카메라를 바라보도록 회전시키는 스크립트
    /// </summary>
    public class IsometricSprite : MonoBehaviour
    {
        [SerializeField] private bool shouldUpdate = true;
        [SerializeField] private bool lockYAxis = false; 
        [SerializeField] private Transform cameraTransform;

        private void Start()
        {
            // 성능을 위해 Start에서 캐싱 권장
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        
        public void LateUpdate()
        {
            var mainCamera = Camera.main;
            if (cameraTransform == null && mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }

            if (shouldUpdate)
            {
                Align();
            }
        }

        [ContextMenu("Align")]
        public void Align()
        {
            if (lockYAxis)
            {
                Vector3 cameraEuler = cameraTransform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0f, cameraEuler.y, 0f);
            }
            else
            {
                transform.rotation = cameraTransform.rotation;
            }
        }
    }
}