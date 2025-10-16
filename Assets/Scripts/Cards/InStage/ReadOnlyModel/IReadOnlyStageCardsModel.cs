using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Presenter;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.InStage.ReadOnlyModel
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
        /// 손패(Hand)가 변경되었을 때 발생하는 이벤트.  
        /// 카드가 추가되거나 제거될 때 CardVisual 등에서 UI 갱신에 사용.
        /// </summary>
        event Action HandChanged;
        
        /// <summary>
        /// 최대 손패 수.
        /// </summary>
        int MaxHand { get; }
        
        /// <summary>
        /// 남은 버리기 횟수.
        /// </summary>
        int DiscardRemain { get; }
        
        /// <summary>
        /// 현재 덱의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyList<InStageCardData> Deck { get; }
        
        /// <summary>
        /// 버린 패의 읽기 전용 뷰.
        /// </summary>
        IReadOnlyList<InStageCardData> DiscardPile { get; }

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
        
        /// <summary>
        /// 지정한 카드를 현재 손패(<see cref="Hand"/>)에서 찾아 해당 인덱스를 반환.
        /// </summary>
        /// <param name="card">인덱스를 조회할 대상 카드.</param>
        /// <param name="index">
        /// 검색 결과로 반환될 카드의 인덱스.  
        /// 카드가 손패에 존재하지 않거나 검색에 실패하면 <c>-1</c>이 반환.
        /// </param>
        bool TryGetIndex(Card card, out int index);
    }
}