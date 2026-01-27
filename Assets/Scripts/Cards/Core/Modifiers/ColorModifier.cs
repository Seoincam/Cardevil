using Cardevil.Attributes;
using Cardevil.Cards.Persistence;
using System;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    // /// <summary>
    // /// 카드의 색상을 변경하는 Modifier.
    // /// </summary>
    // [Serializable]
    // public sealed class ColorModifier : IModifier
    // {
    //     [SerializeField, VisibleOnly] private ModifierType type = ModifierType.AttackColorSelectable;
    //     [SerializeField, VisibleOnly] private CardColor color;
    //     
    //     public ModifierType Type => type;
    //     
    //     /// <summary>
    //     /// 지정된 색상으로 설정.
    //     /// 여러 개가 있을 경우 가장 마지막에 지정한 색상으로 설정.
    //     /// </summary>
    //     /// <param name="color">적용할 카드 색상</param>
    //     public ColorModifier(CardColor color)
    //     {
    //         this.color = color;
    //     }
    //
    //     public void Apply(CardData.Builder b)
    //     {
    //         b.SetColor(color);
    //     }
    //     
    //     public CardModifierSaveData Serialize()
    //     {
    //         return new CardModifierSaveData() { type = type, payload = JsonUtility.ToJson(this) };
    //     }
    //
    //     public void Deserialize(CardModifierSaveData data)
    //     {
    //         var loaded = JsonUtility.FromJson<ColorModifier>(data.payload);
    //         color = loaded.color;
    //     }
    // }
}