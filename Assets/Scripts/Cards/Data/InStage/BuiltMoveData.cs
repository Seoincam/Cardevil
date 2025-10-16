using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public class BuiltMoveData
    {
        public int Length { get; }
        public SelectState<Direction> SelectState { get; }
        
        public BuiltMoveData(int length, List<Direction?> selectables)
        {
            Length = length;
            SelectState = new(selectables);
        }
    }
}