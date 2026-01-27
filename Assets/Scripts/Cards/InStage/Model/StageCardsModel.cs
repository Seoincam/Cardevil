using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    public interface IReadOnlyStageCardsModel
    {
        int MaxHand { get; }
        
        int DiscardRemain { get; }

        event Action<StageCardsModel.EventType> Changed;
        
        IReadOnlyList<CardData> Deck { get; }
        
        IReadOnlyList<CardData> DiscardPile { get; }
        
        IReadOnlyList<StageCard> Hand { get; }
        
        IReadOnlyCollection<StageCard> Selection { get; }

        int GetIndexInHand(StageCard stageCard);
    }
    
    [Serializable]
    public class StageCardsModel : IReadOnlyStageCardsModel, IDisposable
    {
        [Header("Cards")]
        [SerializeField, VisibleOnly] private List<CardData> deck = new();
        [SerializeField, VisibleOnly] private List<CardData> discardPile = new();
        [SerializeField, VisibleOnly] private List<StageCard> hand = new();
        
        [field: Header("Interacting")]
        [field: SerializeField, VisibleOnly] 
        public InteractingInfo CurrentInteracting { get; private set; }

        private HashSet<StageCard> _selection = new();
        
        public static IReadOnlyStageCardsModel Current { get; private set; }

        private readonly CardData[] _initialDeck;
        
        /// <param name="initialDeck">셔플되지 않은 초기 덱</param>
        /// <param name="maxHand">최대 손패 수</param>
        /// <param name="initialDiscardCount">초기 버리기 횟수</param>
        public StageCardsModel(CardData[] initialDeck, int maxHand, int initialDiscardCount)
        {
            _initialDeck = initialDeck;
            MaxHand = maxHand;
            DiscardRemain = initialDiscardCount;

            Current = this;
        }

        public void Dispose()
        {
            Current = null;
        }
        
        /// <summary>
        /// 최대 손패 장수.
        /// </summary>
        public int MaxHand { get; private set; }
        
        /// <summary>
        /// 남은 버리기 횟수.
        /// </summary>
        public int DiscardRemain { get; private set; }

        public event Action<EventType> Changed;

        public IReadOnlyList<CardData> Deck => deck;
        
        public IReadOnlyList<CardData> DiscardPile => discardPile;
        
        public IReadOnlyList<StageCard> Hand => hand;
        
        public IReadOnlyCollection<StageCard> Selection => _selection;

        /// <summary>
        /// 선택된 Card들을 사용할 수 있는지 여부를 반환.
        /// </summary>
        public bool CanUseCard
        {
            get
            {
                if (_selection.Count == 0) return false;

                foreach (var card in _selection)
                {
                    if (!card.Data.CompleteSelectingValue)
                        return false;
                }
                return true;
            }
        }
        
        public bool TryGetIndexInHand(StageCard stageCard, out int index)
        {
            if (hand.Contains(stageCard))
            {
                index = hand.IndexOf(stageCard);
                return true;
            }
            index = -1;
            return false;
        }

        public IReadOnlyList<StageCard> GetSortedSelection()
        {
            return _selection.OrderBy(c => hand.IndexOf(c)).ToList();
        }
        
        /// <returns>
        /// 손패 상의 인덱스.
        /// </returns>
        public int GetIndexInHand(StageCard stageCard) => hand.IndexOf(stageCard);

        /// <summary>
        /// 덱에서 Card Data를 꺼냄.
        /// </summary>
        /// <remarks>
        /// 덱의 가장 마지막에서 꺼냄.
        /// </remarks>
        public CardData PopCardData()
        {
            if (deck.Count == 0)
            {
                LogEx.LogWarning("뽑을 카드가 없음.");
                return null;
            }

            var cardData = deck[^1];
            deck.RemoveAt(deck.Count - 1);

            Changed?.Invoke(EventType.Deck);
            return cardData;
        }
        
        /// <summary>
        /// 생성된 Card 인스턴스를 손패에 등록함.
        /// </summary>
        public void AddHand(StageCard stageCard)
        {
            hand.Add(stageCard);
            Changed?.Invoke(EventType.Hand);
        }
        
        public void InsertHand(StageCard stageCard, int index)
        {
            hand.Insert(index, stageCard);
            Changed?.Invoke(EventType.Hand);
        }

        /// <summary>
        /// Card 인스턴스를 손패에서 제거함.
        /// </summary>
        public void RemoveHand(StageCard stageCard)
        {
            hand.Remove(stageCard);
            Changed?.Invoke(EventType.Hand);
        }

        /// <summary>
        /// Card 인스턴스를 손패 및 선택패에서 제거하고, Card Data를 버린 패 더미에 추가함. 
        /// </summary>
        public void Discard(StageCard stageCard)
        {
            hand.Remove(stageCard);
            _selection.Remove(stageCard);
            discardPile.Add(stageCard);
            
            Changed?.Invoke(EventType.Hand);
            Changed?.Invoke(EventType.Selection);
        }

        /// <summary>
        /// Card 인스턴스를 선택패에 추가함.
        /// </summary>
        public void Select(StageCard stageCard)
        {
            _selection.Add(stageCard);
            Changed?.Invoke(EventType.Selection);
        }

        /// <summary>
        /// Card 인스턴스를 선택패에서 제거함.
        /// </summary>
        /// <param name="stageCard"></param>
        public void Deselect(StageCard stageCard)
        {
            _selection.Remove(stageCard);
            Changed?.Invoke(EventType.Selection);
        }

        /// <summary>
        /// 상호작용 정보를 등록합니다.
        /// </summary>
        public void UpdateInteractingInfo(StageCard stageCard)
        {
            CurrentInteracting = new InteractingInfo(stageCard, Time.time, hand.IndexOf(stageCard));
        }

        /// <summary>
        /// 등록된 상호작용 정보를 해제합니다.
        /// </summary>
        public void ClearInteractingInfo()
        {
            CurrentInteracting = InteractingInfo.Empty;
        }

        /// <summary>
        /// 손패 내 Card 인스턴스의 위치를 스왑합니다.
        /// </summary>
        public void SwapInHand(StageCard stageCardA, int indexB)
        {
            int indexA = GetIndexInHand(stageCardA);
            (hand[indexA], hand[indexB]) = (hand[indexB], hand[indexA]);
            Changed?.Invoke(EventType.Hand);
        }

        /// <summary>
        /// 손패를 숫자 우선 규칙에 따라 정렬.
        /// </summary>
        public void SortHandByNumber()
        {
            hand.Sort(CompareByNumber);
            Changed?.Invoke(EventType.Hand);
        }

        /// <summary>
        /// 손패를 아이콘(색상) 우선 규칙에 따라 정렬.
        /// </summary>
        public void SortHandByIcon()
        {
            hand.Sort(CompareByIcon);
            Changed?.Invoke(EventType.Hand);
        }

        /// <summary>
        /// 모델의 모든 카드 정보를 초기화한 후 덱을 섞습니다.
        /// </summary>
        public void ClearAndShuffle()
        {
            ClearCards();
            deck = _initialDeck.ToList();
            deck.ShuffleListInPlace();
        }
        
        /// <summary>
        /// 모델의 모든 카드 정보를 초기화합니다.
        /// </summary>
        private void ClearCards()
        {
            deck.Clear();
            discardPile.Clear();
            hand.Clear();
            _selection.Clear();
        }

        /// <summary>
        /// 카드의 상호작용 정보.
        /// </summary>
        [Serializable]
        public struct InteractingInfo
        {
            [field: SerializeField] public StageCard StageCard { get; private set; }
            [field: SerializeField] public float LastInteractionTime { get; private set; }
            [field: SerializeField] public int OriginalIndex { get; private set; }

            public InteractingInfo(StageCard stageCard, float lastInteractionTime, int originalIndex)
            {
                StageCard = stageCard;
                LastInteractionTime = lastInteractionTime;
                OriginalIndex = originalIndex;
            }
            
            public bool Exist => StageCard;
            
            public static InteractingInfo Empty => new(null, Time.time, 0);

            public static implicit operator StageCard(InteractingInfo interacting) => interacting.StageCard;
        }
        
        public enum EventType
        {
            /// <summary>
            /// 덱이 변경되었음.
            /// </summary>
            Deck,
            
            /// <summary>
            /// 손패가 변경되거나 순서가 바뀌었음.
            /// </summary>
            Hand,
            
            /// <summary>
            /// 선택패가 변경되었음.
            /// </summary>
            Selection
        }

        #region Sorting Helper

        private static int CompareByNumber(StageCard a, StageCard b)
        {
            int cmp = ValueTypeOrder(a).CompareTo(ValueTypeOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = SelectableCountOrder(a).CompareTo(SelectableCountOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = DirectionOrder(a).CompareTo(DirectionOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = NumberOrder(a).CompareTo(NumberOrder(b));
            return cmp;
        }

        private static int CompareByIcon(StageCard a, StageCard b)
        {
            int cmp = ValueTypeOrder(a).CompareTo(ValueTypeOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = SelectableCountOrder(a).CompareTo(SelectableCountOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = DirectionOrder(a).CompareTo(DirectionOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = ColorOrder(a).CompareTo(ColorOrder(b));
            if (cmp != 0) return cmp;
            
            cmp = NumberOrder(a).CompareTo(NumberOrder(b));
            return cmp;
        }
        
        /// <summary>
        /// 이동 카드 우선, 공격 카드 뒤로.
        /// </summary>
        private static int ValueTypeOrder(CardData cardData)
        {
            return cardData.IsMove ? 0 : 1;
        }

        /// <summary>
        /// 선택 가능한 값 개수 기준 정렬.
        /// </summary>
        private static int SelectableCountOrder(CardData cardData)
        {
            return cardData.SelectableCount;
        }

        /// <summary>
        /// 이동 카드: 선택한 방향 기준 정렬.
        /// </summary>
        /// <remarks>
        /// 상, 하, 좌, 우, 미선택 순.
        /// </remarks>
        private static int DirectionOrder(CardData cardData)
        {
            if (!cardData.IsMove) return 0;
            if (!cardData.CompleteSelectingValue) return int.MaxValue;

            return cardData.FinalDirection switch
            {
                Direction.Up => 0,
                Direction.Down => 1,
                Direction.Left => 2,
                Direction.Right => 3,
                _ => 4
            };
        }

        /// <summary>
        /// 공격 카드: 색상 순 정렬.
        /// </summary>
        private static int ColorOrder(CardData cardData)
        {
            if (!cardData.IsAttack) return 0;
            return (int)cardData.FinalColor;
        }

        /// <summary>
        /// 공격 카드: 선택한 숫자 기준 정렬.
        /// </summary>
        private static int NumberOrder(CardData cardData)
        {
            if (!cardData.IsAttack) return 0;
            if (!cardData.CompleteSelectingValue) return int.MaxValue;
            
            return cardData.FinalNumber;
        }
        
        #endregion
    }
}