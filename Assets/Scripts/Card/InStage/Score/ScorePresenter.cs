using Cardevil.Core.Events.ExecEvent;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Cardevil.Card.InStage.Score
{
    [Serializable]
    public class ScorePresenter
    {
        [SerializeField] private ScoreModel model = new();
        
        private readonly CardScoreView _view;

        public float CurrentScore => model.Score;

        public ScorePresenter(CardScoreView view)
        {
            _view = view;
            _view.ClearScore();
        }

        public async UniTask AddOperatorAsync(IScoreOperator scoreOperator)
        {
            model.AddOperator(scoreOperator);
            await _view.PlayAddOperatorAsync(scoreOperator);
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f)); // 임시 대기
        }

        /// <summary>
        /// 모든 Operator를 적용하고, 최종 점수를 반환함.
        /// </summary>
        public async UniTask<float> ApplyOperatorsAsync()
        {
            foreach (var scoreOperator in model.ScoreOperators)
            {
                model.SetScore(scoreOperator.CurrentScore);
                await _view.ApplyOperator(scoreOperator);
                
                PublishScoreChangedEvent(scoreOperator.PreviousScore, scoreOperator.CurrentScore);
            }

            var totalScore = model.Score;
            model.ClearOperators();

            return totalScore;
        } 
        
        public void OnHandRankChanged(in HandRankData handRankData)
        {
            model.SetHandRank(handRankData);
            _view.UpdateHandRank(model.HandRank, model.HandRankScore);
        }

        private static void PublishScoreChangedEvent(float previous, float current)
        {
            var args = ScoreChangedArgs.Get(previous, current);
            ExecEventBus<ScoreChangedArgs>.InvokeMergedAndDispose(args).Forget();
        }
    }
}