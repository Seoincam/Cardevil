using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data 
{
    public class InStageCardData
    {
        public int Id { get; }
        public BuiltNumberData Number;
        public BuiltMoveData Move;
        
        public InStageCardData(int id, BuiltNumberData number)
        {
            Id = id;
            Number = number;
        }

        public InStageCardData(int id, BuiltMoveData move)
        {
            Id = id;
            Move = move;
        }
    }
        
    public class BuiltNumberData
    {
        public CardColor Color { get; }
        public List<int?> Selectables { get; }
        public float DamageMultiply { get; }
        
        public BuiltNumberData(CardColor color, List<int?> selectables, float damageMultiply)
        {
            Color = color;
            Selectables = selectables;
            DamageMultiply = damageMultiply;
        }
    }

    public class BuiltMoveData
    {
        public int Length { get; }
        public List<Direction?> Selectables { get; }
        
        public BuiltMoveData(int length, List<Direction?> selectables)
        {
            Length = length;
            Selectables = selectables;
        }
    }
}