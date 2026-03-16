using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects
{
    [Serializable]
    public class ModifyStatEffect : EffectBase, IStatModifier
    {
        [SerializeField] private PlayerStatType targetStatType;
        [SerializeField] private int value;

        public ModifyStatEffect() { }
        public ModifyStatEffect(
            IRelicContext context, 
            PlayerStatType targetStatType, 
            int value, 
            int modifierId) 
            : base(context)
        {
            this.targetStatType = targetStatType;
            this.value = value;
            ModifierId = modifierId;
        }

        public int ModifierId { get; set; }
        public PlayerStatType TargetType => targetStatType;
        
        public int Modify(int previousValue)
        {
            return previousValue + value;
        }

        public override void OnInactive()
        {
            base.OnInactive();
            
            Context.PlayerStatus.SafeRemoveModifier(ModifierId, this);
            ModifierId = -1;
        }

        public override void OnActive()
        {
            base.OnActive();
            
            ModifierId = Context.PlayerStatus.AddModifier(this);
        }
    }
}