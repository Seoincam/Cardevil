using Cardevil.Cards.Core;
using Cardevil.Cards.InStage;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Events
{
    /// <summary>
    /// 덱 카드 개수 변화 이벤트 인자.
    /// </summary>
    public class CardDeckChangeArgs : ExecEventArgs<CardDeckChangeArgs>
    {
        public enum Order
        {
            UIUpdate = int.MinValue,
        }

        public static CardDeckChangeArgs Get(int currentDeckCount, int newDeckCount, IReadOnlyStageCardsModel model)
        {
            var args = Get();
            args.Init(currentDeckCount, newDeckCount, model);
            return args;
        }

        public int OldDeckCount { get; private set; }
        public int NewDeckCount { get; private set; }
        public IReadOnlyStageCardsModel Model { get; private set; }
        // 남은 덱 카드 개수는 이벤트로 변화주지 않음.

        private void Init(int currentDeckCount, int newDeckCount, IReadOnlyStageCardsModel model)
        {
            OldDeckCount = currentDeckCount;
            NewDeckCount = newDeckCount;
            Model = model;
        }

        public override void Clear()
        {
            base.Clear();
            // 자신의 멤버 변수를 초기화하도록 수정
            OldDeckCount = 0;
            NewDeckCount = 0;
            Model = null;
        }
    }
    
    /// <summary>
    /// 카드 버리기 횟수 변화 이벤트 인자.
    /// </summary>
    public class CardDiscardChangeArgs : ExecEventArgs<CardDiscardChangeArgs>
    {
        public int OldDiscard { get; private set; }
        public int NewDiscard { get; private set; }
        
        /// <summary>
        /// 이벤트 진행으로 인해 수정된 카드 버리기 횟수. 최종적으로 해당 개수로 카드 버리기 횟수가 설정됨.
        /// </summary>
        public int ModifiedDiscard { get; set; }
        
        public static CardDiscardChangeArgs Get(int currentDiscard, int newDiscard)
        {
            var args = Get();
            args.Init(currentDiscard, newDiscard);
            return args;
        }

        public enum Order
        {
            First = int.MinValue,
            Last = int.MaxValue
        }

        private void Init(int currentDiscard, int newDiscard)
        {
            OldDiscard = currentDiscard;
            NewDiscard = newDiscard;
            ModifiedDiscard = newDiscard;
        }

        public override void Clear()
        {
            base.Clear();
            OldDiscard = 0;
            NewDiscard = 0;
            ModifiedDiscard = 0;
        }
    }
    
    /// <summary>
    /// 카드 '하나하나'가 버려지는 이벤트 인자.
    /// </summary>
    public class EachCardDiscardedArgs : ExecEventArgs<EachCardDiscardedArgs>
    {
        public CardData xCardData { get; private set; }

        public static EachCardDiscardedArgs Get(CardData cardData)
        {
            var args = Get();
            args.CardData = cardData;
            return args;
        }
        
        public override void Clear()
        {
            base.Clear();
            CardData = null;
        }
    }

    /// <summary>
    /// 전투 중 플레이어의 각 이동 카드 발동 시 사용되는 이벤트 인자.
    /// </summary>
    /// <remarks>
    /// 모든 이동 카드를 취합한 것이 아닌, 이동 카드 '하나하나'를 다룸.
    /// </remarks>
    public class PlayerMoveArgs : ExecEventArgs<PlayerMoveArgs>
    {
        private readonly List<Direction> _toMove = new(2);
        public IReadOnlyList<Direction> ToMove => _toMove;
        
        public TileVector PlayerPosition { get; private set; }

        public static PlayerMoveArgs Get(Direction direction)
        {
            var args = Get();
            args.Initialize(direction);
            return args;
        }
        
        public void AddDirection(Direction direction) => _toMove.Add(direction);

        public void SetPlayerPositionAfterMove(TileVector playerPositionOnField) =>
            PlayerPosition = playerPositionOnField;
        
        public enum Orders
        {
            /// <summary>
            /// 기본 방향이 등록되는 시점.
            /// </summary>
            DefaultRegister = int.MinValue,
            
            /// <summary>
            /// 일반적인 수정이 이뤄지는 시점.
            /// </summary>
            Normal,
            
            /// <summary>
            /// 실제 플레이어가 이동하는 시점.
            /// </summary>
            PlayerMove = int.MaxValue - 1,
            
            /// <summary>
            /// 모든 처리가 끝난 후 Model에 이동 위치를 업데이트하는 시점.
            /// </summary>
            Last = int.MaxValue
        }

        public override void Clear()
        {
            base.Clear();
            _toMove.Clear();
        }

        private void Initialize(Direction direction)
        {
            AddDirection(direction);
        }
    }

    /// <summary>
    /// 전투 중 데미지 계산에 사용되는 이벤트 인자.
    /// </summary>
    public class CardDamageEvaluationArgs : ExecEventArgs<CardDamageEvaluationArgs>
    {
        public IReadOnlyList<StageCard> Cards { get; private set; }
        public float Damage { get; private set; }
        public HandRanking HandRanking { get; private set; }

        public static CardDamageEvaluationArgs Get(IReadOnlyList<StageCard> cards, HandRanking handRanking)
        {
            var args = Get();
            args.Initialize(cards, handRanking);
            return args;
        }

        public void AddDamage(float amount) => Damage += amount;
        public void MultiplyDamage(float multiplier) => Damage *= multiplier;

        public void ClampDamage(float maxDamage) => Damage = Mathf.Min(Damage, maxDamage);

        public void SetCards(StageCard[] cards) => Cards = cards;
        
        public enum Orders
        {
            /// <summary>
            /// 최초의 View 클리어.
            /// </summary>
            ClearView = int.MinValue,
            
            /// <summary>
            /// 0. 족보 데미지 합연산.
            /// </summary>
            HandRankingDamage = 0,
            
            /// <summary>
            /// 1. 카드 기본 데미지 합연산.
            /// </summary>
            PlusCardDamage = 10,
            
            /// <summary>
            /// 2. 유물 합연산.
            /// </summary>
            PlusRelic = 20,
            
            /// <summary>
            /// 3. 유물로 인한 카드 자체 데미지 증폭 합연산.
            /// </summary>
            PlusCardDamageRelic = 30,
            
            /// <summary>
            /// 4. 칸별 증폭으로 인한 합연산.
            /// </summary>
            PlusFiled = 40,
            
            /// <summary>
            /// 5. 플레이어 상태 정보(소모품, 몹 기믹, 유물 등)로 인한 합연산.
            /// </summary>
            /// <remarks>
            /// 조건 자체의 변수가 값이 변할 수 있고, 현재 그 조건을 한시적으로 충족한 상태라면 상태 정보로 이동해서 발동하기
            /// (ex. 체력이 3이라면 최종데미지가 110% 증가 | 현재 체력이 3, 체력이 2~0으로 변할 수 있음.)
            /// </remarks>
            PlusPlayerStatus = 50,
            
            /// <summary>
            /// 6. 유물 곱연산.
            /// </summary>
            MultiplyRelic = 60,
            
            /// <summary>
            /// 7. 유물로 인한 카드 자체 데미지 증폭 곱연산.
            /// </summary>
            MultiplyCardDamageRelic = 70,
            
            /// <summary>
            /// 8. 칸별 증폭으로 인한 곱연산.
            /// </summary>
            MultiplyField = 80,
            
            /// <summary>
            /// 9. 플레이어 상태 정보(소모품, 몹 기믹, 유물 등)인한 곱 연산.
            /// </summary>
            /// <remarks>
            /// 조건 자체의 변수가 값이 변할 수 있고, 현재 그 조건을 한시적으로 충족한 상태라면 상태 정보로 이동해서 발동하기
            /// (ex. 체력이 3이라면 최종데미지가 110% 증가 | 현재 체력이 3, 체력이 2~0으로 변할 수 있음.)
            /// </remarks>
            MultiplyPlayerStatus = 90, 
            
            /// <summary>
            /// 모델에 결과를 등록합니다.
            /// 평가 과정의 마지막에 호출 되어야합니다.
            /// </summary>
            RegisterOnModel = int.MaxValue
        }
        
        public override void Clear()
        {
            base.Clear();
            Damage = 0f;
            HandRanking = HandRanking.None;
        }

        private void Initialize(IReadOnlyList<StageCard> cards, HandRanking handRanking)
        {
            Cards = cards;
            HandRanking = handRanking;
        }
    }
}