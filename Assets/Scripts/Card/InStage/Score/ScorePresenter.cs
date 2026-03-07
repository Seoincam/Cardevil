using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.Card.InStage.Score
{
    [Serializable]
    public class ScorePresenter
    {
        [SerializeField] private ScoreModel model = new();
        
        private readonly CardScoreView _view;

        public ScorePresenter(CardScoreView view)
        {
            _view = view;
            _view.ClearScore();
            
            // Test 로직
            async UniTask TestAsync()
            {
                const int testLoopCount = 10;
                for (int i = 0; i < testLoopCount; i++)
                {
                    await AddOperatorAsync(new ScoreOperator
                    {
                        Type = ScoreOperatorType.Plus, Value = Random.Range(1f, 120f)
                    });
                }   
            
                ApplyOperatorsAsync().Forget();
            }
            // TestAsync().Forget();
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
                var previousScore = model.Score;
                var currentScore = scoreOperator.Apply(previousScore);
                
                model.SetScore(currentScore);
                await _view.ApplyOperator(scoreOperator, previousScore, currentScore);
            }

            var totalScore = model.Score;
            model.Clear();

            return totalScore;
        } 
        
        public void OnHandRankChanged(in HandRankData handRankData)
        {
            model.SetHandRank(handRankData);
            _view.UpdateHandRank(model.HandRank, model.HandRankScore);
        }
    }
}