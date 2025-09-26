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
    }


    /// <summary>
    /// 사용한 카드를 바탕으로 해석된 결과만을 가짐.
    /// </summary>
    [Serializable]
    public struct CardResult
    {
        public float _damage;

        public float Damage => _damage;
        public readonly List<NumberData> Numbers;
        public readonly List<MoveData> Moves;
        public readonly HandRanking Ranking => Rankings[0];

        private readonly CardResultContext Ctx;
        private readonly List<HandRanking> Rankings;

        public CardResult(CardResultContext ctx, List<HandRanking> rankings, List<Card> numberDatas, List<Card> moveDatas)
        {
            _damage = 0;
            Ctx = ctx;

            Rankings = rankings;
            Numbers = numberDatas.Select(n => n.data.Number)
                    .ToList();
            Moves = moveDatas.Select(m => m.data.Move)
                    .ToList(); ;
        }

        public void UpdateDamage(float damage)
        {
            _damage = damage;
        }
    }
}