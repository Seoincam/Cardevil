using Cardevil.Cards.Evaluation;
using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    /// <summary>
    /// 선택 카드에 따른 족보 화면 갱신, 카드의 사용을 담당하는 프리젠터.
    /// </summary>
    public class CardUsePresenter : IDisposable
    {
        private readonly StageCardsModel _model;
        private NewEvaluationPresenter _evaluationPresenter;
        
        public CardUsePresenter(StageCardsModel model)
        {
            Debug.Assert(model != null);
            
            _model = model;
            _model.Changed += OnModelChanged;
        }
        
        public void Dispose()
        {
            _model.Changed -= OnModelChanged;
        }

        // 모든 카드를 사용함. 이때 이동카드는 사용, 족보 미포함 카드는 버려짐.
        public async UniTask UseAllCardsAsync(CancellationToken cancellationToken)
        {
            var toUse = _model.Selection.ToArray();
            var handRanking = HandRankingEvaluator.EvaluateHandRankingAndGetContainingCards(
                toUse,
                out var cardsInHandRanking);

            foreach (var card in toUse)
            {
                if (card.Data.IsMove)
                {
                    var movingArgs = PlayerMoveArgs.Get(card.Data.FinalDirection);
                    await ExecEventBus<PlayerMoveArgs>.InvokeMergedAndDispose(movingArgs, cancellationToken);
                    
                    // TODO:
                    // 제거되는 이펙트와 함께 제거되어야함.
                }
                else if (card.Data.IsAttack)
                {
                    if (!cardsInHandRanking.Contains(card))
                    {
                        // TODO:
                        // 제거되는 이펙트와 함께 제거되어야함.
                    }
                    else
                    {
                        _evaluationPresenter.RegisterCard(card).Forget();
                        // await TODO: 손패에서 제거되기
                    }
                }
                
                _model.Discard(card);
                //await 
            }
        }

        private void OnModelChanged(StageCardsModel.EventType eventType)
        {
            switch (eventType)
            {
                case StageCardsModel.EventType.Selection: OnSelectionChanged(); break;
            }
        }

        // 선택패가 바뀔 때마다 족보를 재계산하고, View를 업데이트함.
        private void OnSelectionChanged()
        {
            var handRanking = HandRankingEvaluator.EvaluateHandRanking(_model.Selection);
            // TODO: View 업데이트
        }
    }
}