using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class Relic
    {
        [SerializeField] private string id;
        
        [Header("Display")]
        [SerializeField] private Sprite displayIcon;
        [SerializeField] private string displayName;
        [SerializeField] private string displayDescription;
        
        [Header("Effects")]
        [SerializeReference] private List<EffectBase> effects = new();

        public Relic(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }
        
        public string Id => id;
        public Sprite DisplayIcon => displayIcon;
        public string DisplayName => displayName;
        public string DisplayDescription => displayDescription;
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