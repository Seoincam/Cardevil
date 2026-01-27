using Cardevil.Attributes;
using Cardevil.Cards.Persistence;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    /// <summary>
    /// 선택 가능한 방향 슬롯에 실제 방향 값을 확정하는 Modifier.  
    /// - 슬롯이 2개인 경우: 첫 번째 값이 정해져 있으면 두 번째를 반대 방향으로 자동 확정  
    /// </summary>
    [Serializable]
    public sealed class DirSelectableModifier : IModifier
    {
        [SerializeField, VisibleOnly] private ModifierType type = ModifierType.MoveDirSelectable;
        [SerializeField, VisibleOnly] private Optional<Direction> direction;
        
        public ModifierType Type => type;
        
        public DirSelectableModifier(Direction? direction = null)
        {
            this.direction = new Optional<Direction>(direction);
        }
        
        /// <inheritdoc/>
        public void Apply(CardData.Builder b)
        {
            if (direction.hasValue)
            {
                b.AddDirectionSelectable(direction.value);
                return;
            }
            
            // 두 개의 슬롯이 있고, 첫 번째가 확정된 경우: 반대 방향으로 자동 확정
            if (b.DirectionSelectables.Count == 2 && b.DirectionSelectables[0].HasValue && !b.DirectionSelectables[1].HasValue)
            {
                b.AddDirectionSelectable(b.DirectionSelectables[0]!.Value.Opposite());
                return;
            }
            
            b.AddDirectionSelectable(null);
        }

        public CardModifierSaveData Serialize()
        {
            return new CardModifierSaveData() { type = type, payload = JsonUtility.ToJson(this) };

        }

        public void Deserialize(CardModifierSaveData data)
        {
            var loaded = JsonUtility.FromJson<DirSelectableModifier>(data.payload);
            direction = loaded.direction;
        }
    }
}