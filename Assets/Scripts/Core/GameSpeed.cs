using System;
using UnityEngine;

namespace Cardevil.Core
{
    /// <summary>
    /// 게임 배속 관리 클래스.
    /// </summary>
    public static class GameSpeed
    {
        private static float _scale = 1f;

        public static float Scale
        {
            get => _scale;
            set => _scale = Mathf.Max(0.1f, value); // 0 이하 방지
        }
        
        /// <summary>
        /// 시간 값에 배속 적용.
        /// </summary>
        public static float Apply(float duration) => duration / _scale;
    
        public static void Reset() => _scale = 1f;
    }

    /// <summary>
    /// Config SO 등에서 <see cref="GameSpeed"/> 영향을 받는 Float 변수 래퍼.
    /// </summary>
    [Serializable]
    public struct ScalableFloat
    {
        [SerializeField] private float baseValue;
        
        public float Value => GameSpeed.Apply(baseValue);
        public float Raw => baseValue;
        
        public ScalableFloat(float value) => baseValue = value;
        
        public static implicit operator float (ScalableFloat d) => d.Value;
    }
}