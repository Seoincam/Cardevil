using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    /// <summary>
    /// 이동(Move) 카드의 최종 빌드 결과 데이터.
    /// </summary>
    /// <remarks>
    /// 이동 거리(<see cref="Length"/>)와 선택 가능한 방향(<see cref="SelectState"/>) 정보를 포함.
    /// </remarks>
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