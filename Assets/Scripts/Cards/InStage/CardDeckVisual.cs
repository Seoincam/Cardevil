using Cardevil.Cards.ScriptableObjects;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Cardevil.Cards.InStage
{
    public class CardDeckVisual : MonoBehaviour
    {
        /*
        41-50
        31-40
        21-30
        11-20
        6-10
        5
        4
        3
        2
        1
        */

        [SerializeField] List<Transform> cards;

        [SerializeField] CardVisualSettingSO visualSetting;

        public Transform Front => cards[0];

        /// <summary>
        /// Draw, Discard 등의 상호작용이 있을 때 scale tween.
        /// </summary>
        public void OnInteraction()
        {
            transform.DOScale(visualSetting.DeckInteractionScale, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase)
                .OnComplete(() => transform.DOScale(1f, visualSetting.DeckInteractionDuration)
                .SetEase(visualSetting.DeckInteractionEase));
        }
    }
}