using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects
{
    [Serializable]
    public class ModifyStatDefinition : EffectDefinition
    {
        [SerializeField] private StatType targetStat;
        [SerializeField] private int delta;

        public override string EditorName => "스탯 수정";
        public override string EditorDescription => $"유물이 활성화되어 있는 동안 <color=#FFD700>{targetStat} 스탯</color>에 <color=#FFD700>{delta}</color>만큼 더해서 계산합니다.";

        public override EffectInstance CreateRuntimeInstance(RelicInstance context) => new Instance(this, context);
        
        public class Instance : EffectInstance, IStatModifier
        {
            private readonly ModifyStatDefinition _definition;
            private int _modifierId;
            
            public StatType TargetType => _definition.targetStat;

            public Instance(ModifyStatDefinition definition, RelicInstance context) : base(context)
            {
                _definition = definition;
            }
            
            public int Modify(int previousValue)
            {
                return previousValue + _definition.delta;
            }
            
            public override void OnActive()
            {
                _modifierId = Context.CommonContext.PlayerStatus.AddModifier(this);
            }

            public override void OnInactive()
            {
                Context.CommonContext.PlayerStatus.SafeRemoveModifier(_modifierId, this);
                _modifierId = -1;
            }
        }
    }
}