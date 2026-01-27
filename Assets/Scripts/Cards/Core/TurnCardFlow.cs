using Cardevil.Attributes;
using Cardevil.Cards.InStage;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    public interface ITurnCardFlow
    {
        /// <summary>
        /// 플레이어가 '사용하기'를 누를때까지 대기함.
        /// </summary>
        UniTask WaitPlayerInputCompleted(CancellationToken cancellationToken);
        
        /// <summary>
        /// 플레이어가 선택한 모든 카드를 사용함.
        /// 이때 플레이어의 이동도 이루어짐.
        /// </summary>
        UniTask UseAllCardsAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// 공격 카드들의 데미지를 계산함.
        /// </summary>
        UniTask<EvaluationResult?> EvaluateDamageAsync(CancellationToken cancellationToken);
    }
    
    /// <summary>
    /// 여러 Card Presenter의 로직을 한 곳에 모음.
    /// </summary>
    [Serializable]
    public class TurnCardFlow : ITurnCardFlow
    {
        [SerializeField, VisibleOnly] private StageCardsModel cardsModel;
        
        private StageCardsPresenter _stageCardsPresenter;
        private CardUsePresenter cardUsePresenter;

        public TurnCardFlow(StageCardsModel model, StageCardsPresenter stageCardsPresenter,
            CardUsePresenter cardUsePresenter)
        {
            cardsModel = model;
            _stageCardsPresenter = stageCardsPresenter;
            this.cardUsePresenter = cardUsePresenter;
        }
        
        public async UniTask WaitPlayerInputCompleted(CancellationToken cancellationToken)
        {
            await _stageCardsPresenter.WaitPlayerInputCompleted(cancellationToken);
        }

        public async UniTask UseAllCardsAsync(CancellationToken cancellationToken)
        {
            await cardUsePresenter.UseAllCardsAsync(cancellationToken);
        }

        public async UniTask<EvaluationResult?> EvaluateDamageAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}