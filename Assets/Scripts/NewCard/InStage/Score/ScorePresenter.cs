using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.NewCard.InStage.Score
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
            
            TestAsync().Forget();
        }

        private async UniTask TestAsync()
        {
            const int testLoopCount = 30;
            for (int i = 0; i < testLoopCount; i++)
            {
                AddOperator(ScoreOperatorType.Plus, Random.Range(1, 30));
                await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            }   
            
            ApplyElements().Forget();
        }

        public void OnHandRankChanged(in HandRankData handRankData)
        {
            model.SetHandRank(handRankData);
            _view.UpdateHandRank(model.HandRank, model.HandRankScore);
        }

        public void AddOperator(ScoreOperatorType operatorType, float value)
        {
            var scoreOperator = model.AddOperator(operatorType, value);
            _view.AddOperator(scoreOperator);
        }

        public async UniTask ApplyElements()
        {
            foreach (var scoreOperator in model.ScoreOperators)
            {
                var previousScore = model.Score;
                var currentScore = scoreOperator.Apply(previousScore);
                
                model.SetScore(currentScore);
                await _view.ApplyOperator(scoreOperator, previousScore, currentScore);
            }

            model.Clear();
        } 
    }
}