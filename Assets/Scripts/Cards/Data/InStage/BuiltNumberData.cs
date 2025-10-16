using Cardevil.Cards.Data;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public class BuiltNumberData
    {
        public CardColor Color { get; }
        public float DamageMultiplier { get; }
        public SelectState<int> SelectState { get; }
        
        public BuiltNumberData(CardColor color, List<int?> selectables, float damageMultiplier)
        {
            Color = color;
            SelectState = new(selectables);
            DamageMultiplier = damageMultiplier;
        }
    }
}