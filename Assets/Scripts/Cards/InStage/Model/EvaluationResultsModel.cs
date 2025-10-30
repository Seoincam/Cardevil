using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Core;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.InStage.Model
{
    // 플레이어 캐릭터 : 방향 및 총 데미지
    // 적 : 족보
    
    public sealed class EvaluationResultsModel : IReadOnlyEvaluationResultsModel, IClearable
    {
        private readonly List<EvaluationResult> _history = new();
        private int _cursor = -1;

        #region IReadOnlyEvaluationResultsModel

        public IReadOnlyList<EvaluationResult> History => _history;
        public EvaluationResult CurrentResult => (_cursor >= 0 && _cursor < _history.Count) ? _history[_cursor] : null;

        #endregion
        
        public void Clear()
        {
            _history.Clear();
            _cursor = -1;
        }
        
        /// <summary>
        /// 다음 결과를 받을 준비로 끝에 빈 슬롯(null)을 하나 보장.
        /// </summary>
        public void StepToNext()
        {
            if (_history.Count == 0 || _history[^1] != null)
            {
                _history.Add(null);
            }
            _cursor = _history.Count - 1;
        }
        
        /// <summary>
        /// 임시 저장된 결과(<c>_committed</c>)를
        /// 현재 단계(null 슬롯)에 반영하거나, 없으면 새로 추가.
        /// </summary>
        public void Add(EvaluationResult result)
        {
            if (result == null)
            {
                LogEx.LogError("result is null.");
                return;
            }
            
            // 리스트가 비었거나 마지막이 이미 값이면 새로 추가
            if (_history.Count == 0 || _history[^1] != null)
            {
                _history.Add(result);
                _cursor = _history.Count - 1;
                return;
            }
            
            // 마지막이 null 슬롯이면 거기에 채우기
            _history[^1] = result;
            _cursor = _history.Count - 1;
        }
    }

    [Serializable]
    public sealed class EvaluationResult
    {
        [SerializeField, VisibleOnly] private int totalDamage;
        [SerializeField, VisibleOnly] private List<Direction> moves;
        [SerializeField, VisibleOnly] private HandRanking handRanking;

        public int TotalDamage => totalDamage;
        public IReadOnlyList<Direction> Moves => moves;
        public HandRanking HandRanking => handRanking;

        #region Builder

        public static Builder CreateBuilder() => new(); 
        
        public sealed class Builder
        {
            private int _totalDamage;
            private List<Direction> _moves;
            private HandRanking _handRanking;

            public Builder SetDamage(int damage)
            {
                _totalDamage += damage;
                return this;
            }
            public Builder SetMoves(List<Direction> moves)
            {
                _moves = moves;
                return this;
            }
            public Builder SetHandRanking(HandRanking handRanking)
            {
                _handRanking = handRanking;
                return this;
            }

            public EvaluationResult Build() => new(_totalDamage, _moves, _handRanking);
        }

        public EvaluationResult(int totalDamage, List<Direction> moves, HandRanking handRanking)
        {
            this.totalDamage = totalDamage;
            this.moves = moves;
            this.handRanking = handRanking;
        }

        #endregion
    }
}