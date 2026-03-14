using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class Relic
    {
        [SerializeField] private string displayName;
        [SerializeField] private string displayDescription;
        [SerializeField] private Sprite displayIcon;
        
        [SerializeReference] private List<EffectBase> effects;
        
        public IReadOnlyList<EffectBase> Effects => effects;
    }

    /// <summary>
    /// 플레이어의 스탯을 변화시키는 객체가 구현해야함.
    /// </summary>
    public interface IStatModifier
    {
        int ModifierId { get; set; }
        PlayerStatType TargetType { get; }
        int Modify(int previousValue);
    }

    /// <summary>
    /// 특정 이벤트에 반응하는 객체가 구현해야함.
    /// </summary>
    public interface IGameEventListener
    {
        
    }
}