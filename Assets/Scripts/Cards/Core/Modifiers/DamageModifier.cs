using Cardevil.Attributes;
using Cardevil.Cards.Persistence;
using System;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    /// <summary>
    /// 카드의 데미지 배율을 증가시키는 Modifier.
    /// </summary>
    [Serializable]
    public sealed class DamageModifier : IModifier
    {
        [SerializeField, VisibleOnly] private ModifierType type = ModifierType.AttackDamage;
        [SerializeField, VisibleOnly] private float damage;

        public ModifierType Type => type;

        /// <summary>
        /// 증가시킬 데미지 배율을 지정하여 초기화.
        /// </summary>
        /// <param name="damage">추가할 데미지 배율</param>
        public DamageModifier(float damage = 0.05f)
        {
            this.damage = damage;
        }

        public void Apply(CardData.Builder b)
        {
            b.AddDamageMultiplier(damage);
        }

        public CardModifierSaveData Serialize()
        {
            return new CardModifierSaveData() { type = type, payload = JsonUtility.ToJson(this) };
        }

        public void Deserialize(CardModifierSaveData data)
        {
            var loaded = JsonUtility.FromJson<DamageModifier>(data.payload);
            damage = loaded.damage;
        }
    }
}