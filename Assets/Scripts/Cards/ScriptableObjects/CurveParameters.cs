using UnityEngine;

namespace Cardevil.Cards.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CurveParameters", menuName = "Cards/Hand Curve Parameters")]
    public class CurveParameters : ScriptableObject
    {
        public AnimationCurve positioning;
        public float positioningInfluence = .1f;
        public AnimationCurve rotation;
        public float rotationInfluence = 10f;
    }
}

