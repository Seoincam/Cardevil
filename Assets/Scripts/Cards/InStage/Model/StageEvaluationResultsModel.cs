using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.ReadOnlyModel;
using Cardevil.Core;
using Cardevil.Utils;
using System.Collections.Generic;

namespace Cardevil.Cards.InStage.Model
{
    // 플레이어 캐릭터 : 방향 및 총 데미지
    // 적 : 족보
    
    public sealed class StageEvaluationResultsModel : IReadOnlyStageEvaluationResultsModel, IClearable
    {
        private readonly List<EvaluationResult> _history = new();
        private int _cursor = -1;

        #region IReadOnlyStageEvaluationResultsModel

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
            if (_history[^1] == null) return;
            
            _history.Add(null);
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
            
            if (_history[^1] == null)
            {
                _history[^1] = result;
            }
            else
            {
                _history.Add(result);
            }
            
            _cursor = _history.Count - 1;
        }
    }

    public sealed class EvaluationResult
    {
        public int TotalDamage { get; private set; }
        public IReadOnlyList<BuiltMoveData> Moves { get; }
        public HandRanking HandRanking { get; }

        public void SetDamage(int damage)
        {
            TotalDamage = damage;
        }
        
        public EvaluationResult(List<BuiltMoveData> moves, HandRanking handRanking = HandRanking.None)
        {
            Moves = moves;
            HandRanking = handRanking;
        }
    }
}