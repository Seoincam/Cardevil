using Cardevil.NewCard.Core;
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
                    new BaseNumberElement(2)
                )
                .State;
            var state2 = new CardSpec(2, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(3)
                )
                .State;
            var state3 = new CardSpec(3, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(4)
                )
                .State;
            var state4 = new CardSpec(4, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(5)
                )
                .State;
            var state5 = new CardSpec(5, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(6)
                )
                .State;
            var state6 = new CardSpec(6, CardType.Attack)
                .AddElements(
                    new BaseColorElement(CardColor.Red),
                    new BaseNumberElement(7)
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