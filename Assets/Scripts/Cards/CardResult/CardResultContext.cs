using Cardevil.Cards.CardInteractinos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards
{
    public enum HandRanking
    {
        None, High,
        OnePair, TwoPair, Triple, Straight,
        RedFlush, GreenFlush, BlueFlush, BlackFlush,
        FourCard, StraightFlush
    }


    [Serializable]
    public class CardResultContext
    {
        private readonly List<CardResult?> _history = new();

        // 현재 가르키는 인덱스
        private int cursor = -1;

        public IReadOnlyList<CardResult?> History => _history;



        public CardResult? CurrentResult
        {
            get => cursor >= 0 ? _history[cursor] : null;
        }

        public CardResult? PreviousResult
        {
            get => cursor > 0 ? _history[cursor - 1] : null;
        }


        public bool IsBlackFlushUsed { get; private set; }


        /// <summary>
        /// History에 null을 넣어 기존 current를 previous로.
        /// </summary>
        public void StepToNext()
        {
            _history.Add(null);
        }

        public void Push(CardResult result)
        {
            if (_history[^1] == null) _history[^1] = result;
            else _history.Add(result);

            cursor = _history.Count - 1;
        }

        public void SetBlackFlushUsed()
        {
            IsBlackFlushUsed = true;
        }
    }



    /// <summary>
    /// 결과 계산시 여러 효과들이 적용되는 클래스. 이를 바탕으로 CardResult를 생성.
    /// </summary>
    public class CardEvaluationContext
    {
        /// <summary>
        /// 실제로 History에 Push될 것인가?
        /// </summary>
        public bool pushToHistory;

        public CardResultContext ctx;
        public float defaultDamage;
        public float rankingDamage;
        public List<HandRanking> rankings;

        public HandRanking Ranking => rankings[0];

        public List<CardData> datas;
        public List<CardData> numbers;
        public List<CardData> moves;

        public CardEvaluationContext(CardResultContext ctx, IEnumerable<Card> cards, bool pushToHistory)
        {
            this.pushToHistory = pushToHistory;
            this.ctx = ctx;
            rankings = null;

            datas = cards.Select(c => c.data)
                    .ToList();
            numbers = datas.Where(c => c.valueType == CardData.ValueType.Number)
                    .ToList();
            moves = datas.Where(c => c.valueType == CardData.ValueType.Move)
                    .ToList();

            // 기본 데미지 계산
            foreach (var numberData in numbers)
            {
                var baseDamage = numberData.Number.NumberValue + numberData.AdditionalDamage;
                numberData.Number.Damage = baseDamage;
            }

            defaultDamage = numbers.Sum(n => n.Number.Damage);
            rankingDamage = 0;
        }

        public CardResult MakeResult()
        {
            rankings ??= new List<HandRanking>() { HandRanking.None };
            return new CardResult(ctx, defaultDamage + rankingDamage, rankings, datas, numbers, moves);
        }
    }



    /// <summary>
    /// 사용한 카드를 바탕으로 해석된 결과만을 가짐.
    /// </summary>
    [Serializable]
    public readonly struct CardResult
    {
        public readonly float Damage;

        public readonly List<NumberData> Numbers;
        public readonly List<MoveData> Moves;
        public readonly HandRanking Ranking => Rankings[0]; 

        private readonly CardResultContext Ctx;
        private readonly List<HandRanking> Rankings;
        private readonly List<CardData> CardDatas;
       

        public string Description
        {
            get
            {
                var text = "";

                // if (Rankings[0] != HandRanking.None)
                // {
                //     if (Ctx.IsBlackFlushUsed)
                //         text += "[ Black Flush: damage 200% ]\n";

                //     if (Ctx.PreviousResult?.IsRedFlush == true)
                //         text += "[ Red Flush: damage 300% ]\n";

                //     text += $"Ranking: {Rankings[0]}\nDamage: {Damage}\n";
                // }

                return text;
            }
        }



        public CardResult(CardResultContext ctx, float damage, List<HandRanking> rankings, List<CardData> cardDatas, List<CardData> numberDatas, List<CardData> moveDatas)
        {
            Ctx = ctx;

            Damage = damage;
            Rankings = rankings;
            CardDatas = cardDatas;

            Numbers = numberDatas.Select(n => n.Number)
                    .ToList();
            Moves = moveDatas.Select(m => m.Move)
                    .ToList();;

            // IsRedFlush = IsBlueFlush = IsGreenFlush = IsBlackFlush = false;
            // if (rankings.Contains(HandRanking.Flush))
            //     switch (Numbers[0].ColorValue)
            //     {
            //         case NumberData.CardColor.Red: IsRedFlush = true; break;
            //         case NumberData.CardColor.Blue: IsBlueFlush = true; break;
            //         case NumberData.CardColor.Green: IsGreenFlush = true; break;
            //         case NumberData.CardColor.Black: IsBlackFlush = true; break;
            //     }
        }
    }
}