using System;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public enum RotationType
    {
        None,
        LerpToLocalZ,
        LerpWithMovement
    }
    
    public interface ICardRotation
    {
        RotationType Type { get; }
        void UpdateRotation(Transform cardTransform, float deltaTime);
    }

    public class LerpToLocalZRotation : ICardRotation
    {
        private readonly Func<float> _getTargetZ;
        private readonly float _speed;
        
        public RotationType Type { get; }

        public LerpToLocalZRotation(RotationType type, Func<float> getTargetZ, float speed)
        {
            Type = type;
            _getTargetZ = getTargetZ;
            _speed = speed;
        }
        
        public void UpdateRotation(Transform cardTransform, float deltaTime)
        {
            cardTransform.localEulerAngles = new Vector3(
                0f, 
                0f, 
                Mathf.LerpAngle(cardTransform.localEulerAngles.z, _getTargetZ(), _speed * deltaTime));
        }
    }
}