using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Events.ExecEvents;
using System.Collections.Generic;
using System.Linq;

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

        public static CardDeckChangeArgs Get(int currentDeckCount, int newDeckCount, IReadOnlyCardsModel newModel)
        {
            var args = Get();
            args.Init(currentDeckCount, newDeckCount, newModel);
            return args;
        }

        public int OldDeckCount { get; private set; }
        public int NewDeckCount { get; private set; }
        public IReadOnlyCardsModel NewModel { get; private set; }
        // 남은 덱 카드 개수는 이벤트로 변화주지 않음.

        private void Init(int currentDeckCount, int newDeckCount, IReadOnlyCardsModel newModel)
        {
            OldDeckCount = currentDeckCount;
            NewDeckCount = newDeckCount;
            NewModel = newModel;
        }

        public override void Clear()
        {
            base.Clear();
            // 자신의 멤버 변수를 초기화하도록 수정
            OldDeckCount = 0;
            NewDeckCount = 0;
            NewModel = null;
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
        public CardData CardData { get; private set; }

        public static EachCardDiscardedArgs Get(CardData cardData)
        {
            var args = Get();
            args.Init(cardData);
            return args;
        }

        private void Init(CardData cardData) => CardData = cardData;

        public override void Clear()
        {
            base.Clear();
            CardData = null;
        }
    }

    /// <summary>
    /// 전투 중 데미지 계산에 사용되는 이벤트 인자.
    /// </summary>
    public class CardDamageEvaluationArgs : ExecEventArgs<CardDamageEvaluationArgs>
    {
        private IReadOnlyList<Card> _cards;
        public float Damage { get; private set; }
        public HandRanking HandRanking { get; private set; }

        public IReadOnlyList<CardData> AttackCards => _cards?
                .Where(c => c.Data.IsAttack)
                .Select(c => c.Data)
                .ToArray();

        public IReadOnlyList<CardData> MoveCards => _cards?
            .Where(c => c.Data.IsMove)
            .Select(c => c.Data)
            .ToArray();

        public static CardDamageEvaluationArgs Get(IReadOnlyList<Card> cards, HandRanking handRanking)
        {
            var args = Get();
            args.Initialize(cards, handRanking);
            return args;
        }

        public void AddDamage(float amount) => Damage += amount;
        public void MultiplyDamage(float multiplier) => Damage *= multiplier;

        public void SetCards(Card[] cards) => _cards = cards;
        
        public enum Order
        {
            First = int.MinValue,
            
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

        public EvaluationResult BuildResult() => new((int)Damage, AttackCards, MoveCards, HandRanking);
        public override void Clear()
        {
            base.Clear();
            Damage = 0f;
            HandRanking = HandRanking.None;
        }

        private void Initialize(IReadOnlyList<Card> cards, HandRanking handRanking)
        {
            _cards = cards;
            HandRanking = handRanking;
        }
    }
}