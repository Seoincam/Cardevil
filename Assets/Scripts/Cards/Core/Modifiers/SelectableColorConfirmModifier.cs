using Cardevil.Attributes;
using Cardevil.Cards.Persistence;
using Cardevil.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    /// <summary>
    /// 선택 가능한 색 중 null 상태의 색을 실제 색깔로 확정하는 Modifier.
    /// 값이 지정되지 않은 경우 랜덤 선택함.
    /// </summary>
    public class SelectableColorConfirmModifier : IModifier
    {
        [field: SerializeField, VisibleOnly]
        public ModifierType Type { get; private set; } = ModifierType.AttackColorSelectableConfirm;

        [SerializeField, VisibleOnly] private Optional<CardColor> color;

        /// <summary>
        /// null로 둘 경우 랜덤으로 결정됨.
        /// </summary>
        /// <param name="color">확정할 값</param>
        public SelectableColorConfirmModifier(CardColor? color = null)
        {
            this.color = new Optional<CardColor>(color);
        }
        
        public void Apply(CardData.Builder b)
        {
            if (!color.hasValue)
            {
                var availableColors = new List<CardColor>
                {
                    CardColor.Red, CardColor.Green, CardColor.Blue, CardColor.Black
                };

                foreach (var c in b.ColorSelectables)
                {
                    if (c.HasValue)
                        availableColors.Remove(c.Value);
                }
                
                int radomIndex = UnityEngine.Random.Range(0, availableColors.Count);
                color = new Optional<CardColor>(availableColors[radomIndex]);
            }
            
            b.AddColorSelectable(color.value);
        }
        
        public CardModifierSaveData Serialize()
        {
            return new CardModifierSaveData { type = Type, payload = JsonUtility.ToJson(this) };
        }

        public void Deserialize(CardModifierSaveData data)
        {
            var loaded = JsonUtility.FromJson<SelectableColorConfirmModifier>(data.payload);
            color = loaded.color;
        }
    }
}