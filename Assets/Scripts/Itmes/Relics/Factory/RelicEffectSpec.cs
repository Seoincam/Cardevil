using Cardevil.Cards.Data;

namespace Cardevil.Items.Relics.Factory
{
    public sealed class RelicEffectSpec
    {
        public string EffectId;
        public string EffectType;
        public bool IsPermanent;
        public int ExecutionCount;
        
        public HandRanking TriggerHandRanking;
        public int TriggerHp;
        public float TriggerPossibility;

        public bool IsBasedKillCount;
        public bool IsPlus;
        public float EffectValue;
    }
}