using Cardevil.Events.ExecEvents;
using Cardevil.NewCard.InStage.Calculator;
using Cardevil.NewCard.InStage.Score;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCorePresenter : IDisposable
    {
        [SerializeField] private StageCardCoreModel model = new();
        
        private UniTaskCompletionSource _playerInputWaiter;
        
        private StepSequencer _sequencer = new()
        {
            // TODO: 실제 유물 받아오기
            EachCardScoreOperatorProviders = new List<IEachCardScoreOperatorProvider>(),
            TotalScoreOperatorProviders = new List<ITotalScoreOperatorProvider>()
        };
        
        // 외부 주입
        private HandBarPresenter _handBarPresenter;
        private ScorePresenter _scorePresenter;
        private StageCardCoreView _view;
        
        public StageCardCorePresenter(StageCardCoreView view, HandBarPresenter handBarPresenter, ScorePresenter scorePresenter)
        {
            _view = view;
            view.UseClicked += OnUseClicked;
            view.DiscardClicked += OnDiscardClicked;
            
            view.SetAllButtonState(false);
            
            _handBarPresenter = handBarPresenter;
            _scorePresenter = scorePresenter;
            handBarPresenter.HandBarStateChanged += OnHandBarStateChanged;
            
            var states = model.Draw(6);
            _handBarPresenter.DrawAsync(states).Forget();
            _handBarPresenter.CanInteract = true;
        }
        
        public void Dispose()
        {
            _playerInputWaiter?.TrySetCanceled();
            _playerInputWaiter = null;
        }

        /// <summary>
        /// 플레이어가 사용하기 버튼을 누를 때까지 대기.
        /// </summary>
        public async UniTask WaitUntilPlayerInputAsync()
        {
            _playerInputWaiter = new UniTaskCompletionSource();
            await _playerInputWaiter.Task;

            _playerInputWaiter = null;
        }

        /// <summary>
        /// 플레이어의 카드를 사용함. 모든 유물과 적 로직 포함.
        /// </summary>
        /// <returns> 계산된 최종 데미지. </returns>
        public async UniTask<float> UseAsync()
        {
            // 플레이어 로직
            var playerSteps = _sequencer.BuildPlayerSteps(
                _handBarPresenter.SortedSelection,
                _handBarPresenter.HandRankData
            );
        
            foreach (var step in playerSteps)
            {
                switch (step)
                {
                    case ScoreStep scoreStep: 
                        await _scorePresenter.AddOperatorAsync(scoreStep.Operator);
                        continue;
                    
                    case MoveStep moveStep:
                        await ExecEventBus<PlayerMoveArgs>.InvokeMergedAndDispose(moveStep.Args);
                        await _handBarPresenter.DiscardAsync(moveStep.Card);
                        continue;
                    
                    case DiscardStep discardStep: 
                        await _handBarPresenter.DiscardAsync(discardStep.Card);
                        continue;
                }
            }

            await _scorePresenter.ApplyOperatorsAsync();
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            await _handBarPresenter.DiscardSelectionAsync();
            
            // 적 로직
            var enemySteps = _sequencer.BuildEnemySteps();

            foreach (var step in enemySteps)
            {
                
            }

            return await _scorePresenter.ApplyOperatorsAsync();
            
            
            // 임시 뽑기
            
            // await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            //
            // var states = model.Draw(count);
            // await _handBarPresenter.DrawAsync(states);
            //
            // _handBarPresenter.CanInteract = true;
        }
        
        private void OnHandBarStateChanged(in HandBarPresenter.SelectionState state)
        {
            _view.SetUseButtonState(state.CanUseSelection);
            _view.SetDiscardButtonState(state.CanDiscard);
        }

        private void OnUseClicked()
        {
            Debug.Assert(_handBarPresenter.CanUseSelection, "카드를 사용할 수 없지만 사용 버튼이 눌렸음.");
            
            _view.SetAllButtonState(false);
            _playerInputWaiter?.TrySetResult();
        }

        private void OnDiscardClicked()
        {
            Debug.Assert(_handBarPresenter.CanDiscard, "카드를 버릴 수 없지만 버리기 버튼이 눌렸음.");
            
            _view.SetAllButtonState(false);

            async UniTaskVoid DiscardAsync()
            {
                _handBarPresenter.CanInteract = false;
                
                var discardedStates = await _handBarPresenter.DiscardSelectionAsync();
                model.Discard(discardedStates);

                var states = model.Draw(discardedStates.Count);
                await _handBarPresenter.DrawAsync(states);
                
                _handBarPresenter.CanInteract = true;
            }
            DiscardAsync().Forget();
        }
    }
}