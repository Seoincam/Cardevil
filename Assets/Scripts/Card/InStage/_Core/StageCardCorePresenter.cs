using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Events.ExecEvent;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    [Serializable]
    public class StageCardCorePresenter : IDisposable
    {
        [SerializeField] private StageCardCoreModel model = new();
        
        private StepElementBuilder _elementBuilder;
        private UniTaskCompletionSource _playerInputWaiter;
        
        // 외부 주입
        private HandBarPresenter _handBarPresenter;
        private ScorePresenter _scorePresenter;
        private StageCardCoreView _view;
        
        public StageCardCorePresenter(
            IScoreProviderRegistry scoreProviderRegistry,
            StageCardCoreView view, 
            HandBarPresenter handBarPresenter, 
            ScorePresenter scorePresenter
            )
        {
            _view = view;
            view.UseClicked += OnUseRequested;
            view.DiscardClicked += OnDiscardRequested;
            
            view.SetAllButtonState(false);
            
            _handBarPresenter = handBarPresenter;
            _scorePresenter = scorePresenter;
            handBarPresenter.HandBarStateChanged += OnHandBarStateChanged;

            _elementBuilder = new StepElementBuilder(scoreProviderRegistry);
        }
        
        public void Dispose()
        {
            _playerInputWaiter?.TrySetCanceled();
            _playerInputWaiter = null;
        }

        public async UniTask DrawAsync()
        {
            var toDrawCount = 6 - _handBarPresenter.HandCardCount; // TODO: MaxHand 변수와 연결

            if (toDrawCount > 0)
            {
                var toDraw = model.Draw(toDrawCount);
                await _handBarPresenter.DrawAsync(toDraw);
            }
        }

        /// <summary>
        /// 플레이어가 사용하기 버튼을 누를 때까지 대기.
        /// 이때 플레이어는 카드 선택, 값 변경, 순서변경 등을 수행할 수 있음.
        /// </summary>
        public async UniTask WaitUntilPlayerInputAsync()
        {
            using (Interaction())
            {
                _playerInputWaiter = new UniTaskCompletionSource();
                
                await _playerInputWaiter.Task;
                _playerInputWaiter = null;
            }
        }

        public async UniTask AddStepsAsync(params ScoreStepType[] types)
        {
            if (types == null) return;

            foreach (var type in types)
            {
                await AddStepAsync(type);
            }
        }
        
        public async UniTask AddStepAsync(ScoreStepType type)
        {
            var steps = _elementBuilder.BuildSteps(type);
            await InternalStepAsync(steps);
        }
        
        /// <summary>
        /// 내부 발동 순서에 따라, 조건에 맞는 골드 획득 유물 처리.
        /// </summary>
        public async UniTask StepGoldRelicAsync()
        {
            IReadOnlyList<IStepElement> steps = new List<IStepElement>();
            await InternalStepAsync(steps);
        }
        
        /// <summary>
        /// 추가된 ScoreOperator들을 모두 계산함.
        /// </summary>
        public async UniTask<float> ApplyScoreOperatorsAsync()
        {
            return await _scorePresenter.ApplyOperatorsAsync();
        }

        public async UniTask DiscardSelectionAsync() => await _handBarPresenter.DiscardSelectionAsync();

        public void Reroll(IReadOnlyList<ICardState> states)
        {
            model.Reroll(states);
            model.ShuffleDeck();
        }
        
        private void OnHandBarStateChanged(in HandBarPresenter.SelectionState state)
        {
            _view.SetUseButtonState(state.CanUseSelection);
            _view.SetDiscardButtonState(state.CanDiscard);
        }

        private void OnUseRequested()
        {
            Debug.Assert(_handBarPresenter.CanUseSelection, "카드를 사용할 수 없지만 사용 버튼이 눌렸음.");
            
            _view.SetAllButtonState(false);
            
            _elementBuilder.BuildContext(_handBarPresenter.SortedSelection, _handBarPresenter.HandRankData);
            _playerInputWaiter?.TrySetResult();
        }

        private void OnDiscardRequested()
        {
            Debug.Assert(_handBarPresenter.CanDiscard, "카드를 버릴 수 없지만 버리기 버튼이 눌렸음.");
            
            _view.SetAllButtonState(false);

            async UniTaskVoid DiscardAsync()
            {
                _handBarPresenter.SetInputEnabled(false);
                
                var discardedStates = await _handBarPresenter.DiscardSelectionAsync();
                model.Discard(discardedStates);

                var states = model.Draw(discardedStates.Count);
                await _handBarPresenter.DrawAsync(states);
                
                _handBarPresenter.SetInputEnabled(true);
            }
            DiscardAsync().Forget();
        }

        private async UniTask InternalStepAsync(IReadOnlyList<IStepElement> steps)
        {
            if (steps == null) return;
            
            foreach (var step in steps)
            {
                switch (step)
                {
                    case ScoreStepElement scoreStep: 
                        await _scorePresenter.AddOperatorAsync(scoreStep.Operator);
                        continue;
                    
                    case MoveStepElement moveStep:
                        await ExecEventBus<PlayerMoveArgs>.InvokeMergedAndDispose(moveStep.Args);
                        await _handBarPresenter.DiscardAsync(moveStep.Card);
                        continue;
                    
                    case DiscardStepElement discardStep: 
                        await _handBarPresenter.DiscardAsync(discardStep.Card);
                        continue;
                }
            }
        }

        private InteractionScope Interaction() => new(_handBarPresenter);

        private readonly struct InteractionScope : IDisposable
        {
            private readonly HandBarPresenter _handBarPresenter;
            
            public InteractionScope(HandBarPresenter handBarPresenter)
            {
                _handBarPresenter = handBarPresenter;
                handBarPresenter.SetInputEnabled(true);
            }
            
            public void Dispose()
            {
                _handBarPresenter.SetInputEnabled(false);
            }
        }
    }
}