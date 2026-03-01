using System;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public enum MovementType
    {
        None,
        LerpToLocal,
        LerpToWorld,
        MoveToPointer
    }
    public interface ICardMovement
    {
        MovementType Type { get; }
        void UpdatePosition(Transform cardTransform, float deltaTime);
    }

    public class LerpToLocalMovement : ICardMovement
    {
        private readonly Func<Vector3> _getTaget;
        private readonly float _speed;
        
        public MovementType Type => MovementType.LerpToLocal;

        public LerpToLocalMovement(Func<Vector3> getTaget, float speed)
        {
            _getTaget = getTaget;
            _speed = speed;
        }
        
        public void UpdatePosition(Transform cardTransform, float deltaTime)
        {
            cardTransform.localPosition = Vector3.Lerp(
                cardTransform.localPosition,
                _getTaget(),
                _speed * deltaTime
            );
        }
    }

    public class LerpToWorldPosition : ICardMovement
    {
        private readonly Func<Vector3> _getTaget;
        private readonly float _speed;

        public MovementType Type { get; }

        public LerpToWorldPosition(MovementType type, Func<Vector3> getTaget, float speed)
        {
            Type = type;
            _getTaget = getTaget;
            _speed = speed;
        }
        
        public void UpdatePosition(Transform cardTransform, float deltaTime)
        {
            cardTransform.position = Vector3.Lerp(
                cardTransform.position,
                _getTaget(),
                _speed * deltaTime
            );
        }
    }
}