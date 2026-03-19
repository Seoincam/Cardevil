using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects
{
    [Serializable]
    public class ModifyStatDefinition : EffectDefinition
    {
        [SerializeField] private PlayerStatType targetStat;
        [SerializeField] private int delta;

        public override string EditorName => "스탯 수정";
        public override string EditorDescription => $"유물이 활성화되어 있는 동안 <color=#FFD700>{targetStat} 스탯</color>에 <color=#FFD700>{delta}</color>만큼 더해서 계산합니다.";

        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);
        
        public class Runtime : EffectRuntime, IStatModifier
        {
            private readonly ModifyStatDefinition _definition;

            public int ModifierId { get; set; } = -1;
            public PlayerStatType TargetType => _definition.targetStat;

            public Runtime(ModifyStatDefinition definition, RelicInstance context) : base(context)
            {
                _definition = definition;
            }
            
            public int Modify(int previousValue)
            {
                return previousValue + _definition.delta;
            }
            
            public override void OnActive()
            {
                ModifierId = Context.CommonContext.PlayerStatus.AddModifier(this);
            }

            public override void OnInactive()
            {
                Context.CommonContext.PlayerStatus.SafeRemoveModifier(ModifierId, this);
                ModifierId = -1;
            }
        }
    }
}