using System.Collections.Generic;

namespace Cardevil.Cards.Interactions
{
    /// <summary>
    /// 스테이지 내 카드 상태를 읽기 전용으로 조회할 수 있는 인터페이스.
    /// 
    /// <para>
    /// 덱, 버린 패, 손패, 선택된 카드 등의 상태를 외부에서 확인할 때 사용.  
    /// 이 인터페이스는 <see cref="StageCardsModel"/>의 내부 데이터에 대한  
    /// 읽기 전용 접근을 보장.
    /// </para>
    /// </summary>
    public interface IReadOnlyStageCardsModel
    {
        /// <summary>
        /// 현재 덱의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyList<CardData> Deck { get; }
        
        /// <summary>
        /// 버린 패의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyList<CardData> DiscardPile { get; }

        /// <summary>
        /// 현재 손패의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyList<Card> Hand { get; }
        
        /// <summary>
        /// 현재 선택 카드 집합의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyCollection<Card> Selection { get; }
        
        /// <summary>
        /// 손패 내 인덱스 순서로 정렬된 선택 카드 목록을 반환.
        /// 매 호출 시 새로운 리스트를 생성.
        /// </summary>
        IReadOnlyList<Card> SortedSelection { get; }

    }
}