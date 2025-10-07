using Cardevil.Cards.Interactions;
using Cardevil.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    [Serializable]
    public class CardResultContext: IClearable
    {
        private List<CardResult> _history = new();
        private CardResult _commitedResult = null;

        // 현재 가르키는 인덱스
        private int _cursor = -1;

        public IReadOnlyList<CardResult> History => _history;
        public CardResult CommitedResult
        {
            get
            {
                if (_commitedResult == null)
                    Debug.LogError("잘못된 시점에 commited Result에 접근!");
                return _commitedResult;
            }
        }

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

        /// <summary>
        /// 결과를 History에 저장 전, 임시로 보관.
        /// </summary>
        public void Commmit(CardResult result)
        {
            _commitedResult = result;
        }

        public void Push()
        {
            var result = _commitedResult;
            if (_commitedResult == null)
                Debug.LogError("커밋된 Result가 없습니다.");

            if (_history[^1] == null) _history[^1] = result;
            else _history.Add(result);

            _cursor = _history.Count - 1;
            _commitedResult = null;
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