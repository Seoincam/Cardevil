using Cardevil.UI;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cardevil.UI.Components
{
    public class CircularSliderUI : MonoBehaviour, ICircularUI
    {
        [Header("References")]
        [SerializeField] private RectTransform rotateBase;
        [SerializeField] private Image maskImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform handleTransform;

        [Header("Settings")]
        [SerializeField] private float handleRadius = 1f;
        [SerializeField] private bool clockwise = false;

        [SerializeField] private float startAngle = 0;
        [SerializeField] private float endAngle = 360;

        [SerializeField, Range(0, 1)] private float value = 0.5f;

        public float StartAngle => startAngle;
        public float EndAngle => endAngle;

        /// <summary>
        /// 현재 Value에 따른 실제 각도를 반환합니다.
        /// </summary>
        public float CurrentAngle
        {
            get
            {
                float range = Mathf.Abs(endAngle - startAngle);
                float offset = range * value;

                return clockwise ? (startAngle - offset) : (startAngle + offset);
            }
        }

        public float Value { get => value; set => SetValue(value); }

        private void Reset()
        {
            maskImage = GetComponentInChildren<Image>();
            
            if (maskImage) maskImage.fillOrigin = 1; 
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

        private void SetValue(float newValue)
        {
            this.value = Mathf.Clamp01(newValue);
            
            float range = Mathf.Abs(endAngle - startAngle);
            float fillAmountAngle = range * this.value;
            
            if (maskImage)
            {
                maskImage.fillAmount = fillAmountAngle / 360f;
            } 
            
            if (handleTransform)
            {
                float finalAngle = CurrentAngle - startAngle;
                
                float rad = finalAngle * Mathf.Deg2Rad;
                
                Vector3 handlePos = new Vector3(
                    handleRadius * Mathf.Cos(rad), 
                    handleRadius * Mathf.Sin(rad), 
                    0
                );
                handleTransform.anchoredPosition = handlePos;

            }
        }

        private void UpdateBaseRotation()
        {

            if (rotateBase)
            {
                Vector3 currentRotation = rotateBase.localEulerAngles;
                currentRotation.z = startAngle;
                rotateBase.localEulerAngles = currentRotation;
            }
        }

        private void OnValidate()
        {
            Initialize(startAngle, endAngle, clockwise);
            UpdateBaseRotation();
            SetValue(value);
        }

        private void OnDrawGizmosSelected()
        {
            if (rotateBase == null) return;

            int segments = 50;
            Gizmos.color = Color.red;
            
            Vector3 center = transform.position;
            Vector3 prev = Vector3.zero;
            
            float radius = handleRadius > 0 ? handleRadius : 100f; 

            float range = Mathf.Abs(endAngle - startAngle);

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angleOffset = range * t;
                
                float angle = clockwise ? (startAngle - angleOffset) : (startAngle + angleOffset);
                float rad = angle * Mathf.Deg2Rad;

                Vector3 pos = center + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);

                if (i > 0)
                {
                    Gizmos.DrawLine(prev, pos);
                }
                prev = pos;
            }
            
            void DrawAngleLine(float angle, Color c)
            {
                Gizmos.color = c;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 to = center + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
                Gizmos.DrawLine(center, to);
            }

            DrawAngleLine(startAngle, Color.green);

            float finalEndAngle = clockwise ? (startAngle - range) : (startAngle + range);
            DrawAngleLine(finalEndAngle, Color.blue);
        }
    }
}