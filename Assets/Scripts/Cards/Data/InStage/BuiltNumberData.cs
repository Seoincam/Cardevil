using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    /// <summary>
    /// 숫자(Number) 카드의 최종 빌드 결과 데이터.
    /// </summary>
    /// <remarks>
    /// 카드의 색상(<see cref="Color"/>), 데미지 배율(<see cref="DamageMultiplier"/>),
    /// 그리고 선택 가능한 숫자 상태(<see cref="SelectState"/>) 정보를 포함.
    /// </remarks>
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