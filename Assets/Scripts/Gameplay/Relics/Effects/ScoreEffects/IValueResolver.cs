using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    public interface IValueResolver
    {
        float GetValue(RelicInstance context);
        string GetDescription(float baseValue);
    }

    [Serializable]
    public class ConstantResolver : IValueResolver
    {
        public float GetValue(RelicInstance context) => 1f;

        public string GetDescription(float baseValue) => $"{baseValue}";
    }

    [Serializable]
    public class PlayerStatResolver : IValueResolver
    {
        [Header("추적할 플레이어 스탯")]
        [SerializeField] private PlayerStatType targetStat;
        
        public float GetValue(RelicInstance context)
        {
            var playerStatus = context.CommonContext.PlayerStatus;
            
            int statValue = playerStatus.GetFinalValue(targetStat);
            return statValue;
        }

        public string GetDescription(float baseValue)
        {
            return $"({targetStat} x {baseValue})";
        }
    }
}