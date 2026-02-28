using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.InStage.Calculator;
using Cardevil.NewCard.InStage.Score;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCorePresenter
    {
        [SerializeField] private StageCardCoreModel model = new();
        
        // 외부 주입
        private HandBarPresenter _handBarPresenter;
        private ScorePresenter _scorePresenter;
        private StageCardCoreView _view;
        
        // 생성
        private CardUsingSequencer _sequencer = new()
        {
            // TODO: 실제 유물 받아오기
            EachCardScoreOperatorProviders = new List<IEachCardScoreOperatorProvider>(),
            TotalScoreOperatorProviders = new List<ITotalScoreOperatorProvider>()
        };
        
        public StageCardCorePresenter(StageCardCoreView view, HandBarPresenter handBarPresenter, ScorePresenter scorePresenter)
        {
            _view = view;
            view.UseClicked += OnUseClicked;
            view.DiscardClicked += OnDiscardClicked;
            
            view.SetAllButtonState(false);
            
            _handBarPresenter = handBarPresenter;
            _scorePresenter = scorePresenter;
            handBarPresenter.HandBarStateChanged += OnHandBarStateChanged;
            
            // Test();
            var states = model.Draw(6);
            _handBarPresenter.DrawAsync(states).Forget();
            _handBarPresenter.CanInteract = true;
        }

        private void Test()
        {
            var state1 = new CardSpec(1, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(2),
                    SelectableNumberElement.Fixed(3),
                    SelectableNumberElement.Fixed(4),
                    SelectableNumberElement.Fixed(5),
                    SelectableNumberElement.Fixed(6),
                    SelectableNumberElement.Fixed(7),
                    SelectableNumberElement.Fixed(8),
                    SelectableNumberElement.Fixed(9),
                    SelectableNumberElement.Fixed(10)
                )
                .State;
            var state2 = new CardSpec(2, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Green),
                    SelectableColorElement.Random(),
                    new BaseNumberElement(3)
                )
                .State;
            var state3 = new CardSpec(3, CardType.Move)
                .AddElements(
                    new BaseDirectionElement(Direction.Up),
                    SelectableDirectionElement.Fixed(Direction.Down)
                )
                .State;
            var state4 = new CardSpec(6, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Black),
                    new BaseNumberElement(7),
                    SelectableNumberElement.Fixed(8),
                    SelectableNumberElement.Fixed(9)
                )
                .State;
            var state5 = new CardSpec(5, CardType.Move)
                .AddElements(
                    new BaseDirectionElement(Direction.Up)
                )
                .State;
            var state6 = new CardSpec(6, CardType.Attack)
                .AddElements(
                    new BaseNumberElement(9),
                    new BaseColorElement(CardColor.Red),
                    SelectableColorElement.Fixed(CardColor.Green),
                    SelectableColorElement.Fixed(CardColor.Blue),
                    SelectableColorElement.Fixed(CardColor.Black)
                )
                .State;
            
            _handBarPresenter.AddCard(state1);
            _handBarPresenter.AddCard(state2);
            _handBarPresenter.AddCard(state3);
            _handBarPresenter.AddCard(state4);
            _handBarPresenter.AddCard(state5);
            _handBarPresenter.AddCard(state6);
            
            _handBarPresenter.CanInteract = true;
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
            UseAsync().Forget();
            
            return;
            async UniTask UseAsync()
            {
                var count = _handBarPresenter.SortedSelection.Count;
                
                var steps = _sequencer.Build(
                    _handBarPresenter.SortedSelection,
                    _handBarPresenter.HandRankData
                );
            
                foreach (var step in steps)
                {
                    switch (step)
                    {
                        case ScoreStep scoreStep: 
                            await _scorePresenter.AddOperatorAsync(scoreStep.Operator);
                            continue;
                        
                        case DiscardStep discardStep: 
                            await _handBarPresenter.DiscardAsync(discardStep.Card);
                            continue;
                        
                        case MoveStep moveStep:
                            continue;
                    }
                }

                var finalScore = await _scorePresenter.ApplyOperators();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                await _handBarPresenter.DiscardSelectionAsync();
                
                // 임시 뽑기
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                
                var states = model.Draw(count);
                await _handBarPresenter.DrawAsync(states);
                
                _handBarPresenter.CanInteract = true;
            }
        }

        private void OnDiscardClicked()
        {
            Debug.Assert(_handBarPresenter.CanDiscard, "카드를 버릴 수 없지만 버리기 버튼이 눌렸음.");
            
            _view.SetAllButtonState(false);
            DiscardAsync().Forget();
            
            return;
            async UniTaskVoid DiscardAsync()
            {
                _handBarPresenter.CanInteract = false;
                
                var discardedStates = await _handBarPresenter.DiscardSelectionAsync();
                model.Discard(discardedStates);

                var states = model.Draw(discardedStates.Count);
                await _handBarPresenter.DrawAsync(states);
                
                _handBarPresenter.CanInteract = true;
            }
        }
    }
}