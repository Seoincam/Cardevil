using Cardevil.Attributes;
using Cardevil.UI;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Cardevil.UI.Components
{
    /// <summary>
    /// 원형 슬라이더
    /// 내부값은 Value(0~1)로 저장되며, 이를 각도로 변환하여 표시
    /// </summary>
    public class CircularSliderUI : MonoBehaviour, ICircularUI
    {
        [SerializeField] private Image maskImage;
        [SerializeField] private Image fillImage;
        
        [SerializeField] private float handleRadius = 1f;
        [SerializeField] private RectTransform handleTransform;

        [SerializeField] private bool clockwise = false;

        // [SerializeField] private Direction startDirection = Direction.Up;
        [SerializeField] private float startAngle = 0;
        [SerializeField] private float endAngle = 360;

        public float StartAngle => startAngle;
        public float EndAngle => endAngle;
        public float CurrentAngle => startAngle + (endAngle - startAngle) * value;

        [SerializeField, Range(0, 1)] private float value = 0.5f;

        public float Value { get => value; set => SetValue(value); }
        public float AbsAngle { get => GetAbsAngle(value); }

        private void Reset()
        {
            maskImage = GetComponentInChildren<Image>();
        }

        public void Initialize(float startAngle, float endAngle, bool clockwise)
        {

            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.clockwise = clockwise;
            if (maskImage)
            {
                maskImage.fillOrigin = 1;
                maskImage.fillClockwise = clockwise;
            }

            if (fillImage)
            {
                fillImage.fillClockwise = clockwise;
            }

            UpdateBaseRotation();
        }

        private void SetValue(float value)
        {
            this.value = value;
            float fillValue = GetAbsAngle(value);
            if (maskImage)
            {
                maskImage.fillAmount = fillValue / 360;
            }
            if (handleTransform)
            {
                float angle = CurrentAngle;
                if (clockwise)
                {
                    angle = 360 - angle;
                }
                float rad = angle * Mathf.Deg2Rad;
                Vector3 handlePos = new Vector3(handleRadius * Mathf.Cos(rad), handleRadius * Mathf.Sin(rad), 0);
                handleTransform.anchoredPosition = handlePos;
            }
            // if (fillImage)
            // {
            //     fillImage.fillAmount = fillValue / 360;
            // }
        }

        /// <summary>
        /// 0~1 사이의 값을 받아서 원형 슬라이더 위의 각도로 변환
        /// </summary>
        /// <param name="value">0~1의 값</param>
        /// <returns></returns>
        public float GetAbsAngle(float value)
        {
            float deltaAngle = endAngle - startAngle;
            return Mathf.Lerp(0, deltaAngle, value);
        }

        private void UpdateBaseRotation()
        {
            var originalImageRotation = fillImage.transform.localEulerAngles;

            var currentRotation = maskImage.transform.localEulerAngles;
            currentRotation.z = startAngle;
            maskImage.transform.localEulerAngles = currentRotation;
            fillImage.transform.localEulerAngles = originalImageRotation;
        }


        private void OnValidate()
        {
            Initialize(startAngle, endAngle, clockwise);
            SetValue(value);
        }

        private void OnDrawGizmosSelected()
        {
            int segments = 100;
            Gizmos.color = Color.red;
            Vector3 prev = transform.position;
            float radius = GetComponent<RectTransform>().rect.width / 2;
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + (endAngle - startAngle) * i / segments;
                float rad = (clockwise ? 360 - angle : angle) * Mathf.Deg2Rad;
                Vector3 to = transform.position + new Vector3(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad), 0);
                Gizmos.DrawLine(prev, to);
                prev = to;
            }

            Gizmos.DrawLine(prev, transform.position);
            
            void DrawAngleLine(float angle)
            {
                float rad = (clockwise ? 360 - angle : angle) * Mathf.Deg2Rad;
                Vector3 to = transform.position + new Vector3(radius * Mathf.Cos(rad), radius * Mathf.Sin(rad), 0);
                Gizmos.DrawLine(transform.position, to);
            }
            
            // 시작과 끝 각도 표시
            Gizmos.color = Color.green;
            DrawAngleLine(startAngle);
            Gizmos.color = Color.blue;
            DrawAngleLine(endAngle);    

        }
    }
}