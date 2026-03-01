using UnityEngine;

namespace Cardevil.Card.InStage
{
    [CreateAssetMenu(menuName = "NewCards/Config/HandBar", fileName = "Hand Bar Config")]
    public class HandBarConfig : ScriptableObject
    {
        [field: Header("Default")]
        [field: SerializeField, Range(0f, 0.5f)] 
        public float AnchorY { get; private set; } = 0.08f;
        
        [field: SerializeField, Range(0f, 0.5f)] 
        public float HandZoneMaxY { get; private set; } = 0.25f;
        
        [field: SerializeField, Range(0f, 2f)] 
        public float CardSpacing { get; private set; } = 0.75f;
        
        [field: Header("Curve")]
        [field: SerializeField]
        public AnimationCurve Position { get; private set; }

        [field: SerializeField] 
        public float PositionInfluence { get; private set; } = 10f;

        [field: SerializeField] 
        public AnimationCurve Rotation { get; private set; }

        [field: SerializeField] 
        public float RotationInfluence { get; private set; } = 3.5f;
        
        /// <returns>
        /// 손패에서의 곡선 배치에 사용될 y 오프셋과 회전을 반환.
        /// </returns>
        public (float yOffset, float rotation) GetCurve(int indexInHand, int totalCount)
        {
            var yOffset = GetCurveYOffset(indexInHand, totalCount);
            var rotation = GetCurveRotation(indexInHand, totalCount);
            return (yOffset, rotation);
        }
        
        private float GetCurveYOffset(int indexInHand, int totalCount)
        {
            float factor = GetCurveFactor(indexInHand, totalCount);
            return Position.Evaluate(factor) * PositionInfluence * totalCount;
        }

        private float GetCurveRotation(int indexInHand, int totalCount)
        {
            float factor = GetCurveFactor(indexInHand, totalCount);
            return Rotation.Evaluate(factor) * RotationInfluence * totalCount;
        }

        private static float GetCurveFactor(int indexInHand, int totalCount)
        {
            totalCount = Mathf.Max(1, totalCount - 1); // 0 나눗셈 방지
            return (float)indexInHand / totalCount;
        }
    }
}