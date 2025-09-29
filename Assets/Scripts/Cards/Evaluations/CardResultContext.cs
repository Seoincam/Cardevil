using Cardevil.Cards.Interactions;
using Cardevil.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

namespace Cardevil.Cards.Evaluations
{
    public enum HandRanking
    {
        None, High,
        OnePair, TwoPair, Triple, Straight,
        RedFlush, GreenFlush, BlueFlush, BlackFlush,
        FourCard, StraightFlush
    }


    [Serializable]
    public class CardResultContext: IClearable
    {
        private List<CardResult> _history = new();

        // 현재 가르키는 인덱스
        private int _cursor = -1;

        public IReadOnlyList<CardResult> History => _history;

        public CardResult CurrentResult
        {
            get => (_cursor >= 0 && _cursor < _history.Count) ? _history[_cursor] : null;
        }

        public CardResult PreviousResult
        {
            get => _cursor > 0 ? _history[_cursor - 1] : null;
        }


        /// <summary>
        /// 기존 current를 previous로 밀고, current 자리에 null 슬롯 생성
        /// </summary>
        public void StepToNext()
        {
            _history.Add(null);
            _cursor = _history.Count - 1;
        }

        public void Push(CardResult result)
        {
            if (_history[^1] == null) _history[^1] = result;
            else _history.Add(result);

            _cursor = _history.Count - 1;
        }

        public void Clear()
        {
            _history.Clear();
            _cursor = -1;
        }

    }


    /// <summary>
    /// 사용한 카드를 바탕으로 해석된 결과만을 가짐.
    /// </summary>
    [Serializable]
    public sealed class CardResult
    {
        public float Damage { get; private set; } = 0;
        public IReadOnlyList<NumberData> Numbers { get; }
        public IReadOnlyList<MoveData> Moves { get; }
        public HandRanking Ranking => Rankings[0];
        private readonly List<HandRanking> Rankings;

        public CardResult(List<HandRanking> rankings, List<Card> numberDatas, List<Card> moveDatas)
        {
            Rankings = rankings;
            Numbers = numberDatas.Select(n => n.data.Number)
                    .ToList();
            Moves = moveDatas.Select(m => m.data.Move)
                    .ToList(); ;
        }

        public CardResult(List<Card> numberDatas, List<Card> moveDatas)
        {
            Rankings = new() { HandRanking.None };
            Numbers = numberDatas.Select(n => n.data.Number)
                    .ToList();
            Moves = moveDatas.Select(m => m.data.Move)
                    .ToList(); ;
        } 

        public void UpdateDamage(float damage)
        {
            Damage = damage;
        }
    }
}