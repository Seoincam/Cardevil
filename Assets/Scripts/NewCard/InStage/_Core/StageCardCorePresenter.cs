using Cardevil.NewCard.Common.Core;
using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCorePresenter
    {
        [SerializeField] private StageCardCoreModel model = new();
        
        private HandBarPresenter _handBarPresenter;
        private StageCardCoreView _view;
        
        public StageCardCorePresenter(StageCardCoreView view, HandBarPresenter handBarPresenter)
        {
            _view = view;
            _handBarPresenter = handBarPresenter;
            
            // Tests
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
            
            handBarPresenter.CanInteract = true;
        }
    }
}