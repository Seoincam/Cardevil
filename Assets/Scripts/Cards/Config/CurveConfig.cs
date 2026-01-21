using UnityEngine;

namespace Cardevil.Cards.Config
{
    /// <summary>
    /// 손패에서의 곡선 배치를 위한, 수직 오프셋 및 회전 정도 설정.
    /// </summary>
    [CreateAssetMenu(menuName = "Cards/Config/Curve")]
    public class CurveConfig : ScriptableObject
    {
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
            float factor = GetFactor(indexInHand, totalCount);
            return Position.Evaluate(factor) * PositionInfluence * totalCount;
        }

        private float GetCurveRotation(int indexInHand, int totalCount)
        {
            float factor = GetFactor(indexInHand, totalCount);
            return Rotation.Evaluate(factor) * RotationInfluence * totalCount;
        }

        private static float GetFactor(int indexInHand, int totalCount)
        {
            totalCount = Mathf.Max(1, totalCount - 1); // 0 나눗셈 방지
            return (float)indexInHand / totalCount;
        }
    }
}