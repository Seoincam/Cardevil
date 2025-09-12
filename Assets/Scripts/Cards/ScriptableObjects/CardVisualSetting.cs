using UnityEngine;
using DG.Tweening;
using Unity.Profiling;

namespace Cardevil.Cards.CardInteractinos
{
    [CreateAssetMenu(fileName = "CardVisualSetting", menuName = "Cards/Card Visual Setting")]
    public class CardVisualSetting : ScriptableObject
    {
        [Header("[ Common ]")]
        public float SelectOffset = 50f;

        [Space(25)]
        [Header("[ HandBar ]")]

        [Header("- Reroll")]
        public float RerollDrawDiscardInterval = .75f;
        public float RerollDrawInterval = .15f;
        public float RerollDiscardInterval = .15f;

        [Header("- In Game")]
        public float DrawInterval = .2f;
        public float DiscardInterval = .3f;
        public float DiscardDrawInterval = .75f;
        public float ReviveInterval = .4f;
        public float EndDragTweenDuration = .2f;


        [Space(25)]
        [Header("[ Card ]")]
        public float MoveSpeedLimit = 4000;
        public float ClickDetectThreshold = .2f;


        [Space(25)]
        [Header("[ Card Visual ]")]
        public CardDeckVisual deck;
        public Ease FlipEase = Ease.InElastic;

        [Header("- In Game")]
        public float DrawFlipDuration = .4f;
        public float DrawDuration = .2f;
        public Ease DrawEase = Ease.OutBack;

        [Space]
        public float DiscardDuration = .4f;
        public Transform DiscardPoint;
        public Ease DiscardEase = Ease.InBack;

        [Header("- Reroll")]
        public float RerollFlipDuration = .2f;
        [Range(0,1)] public float RerollFlipBackImageRatio = .5f;
        public float RerollFlipFrontImageRation => 1 - RerollFlipBackImageRatio;

        [Space]
        public float RerollDrawDuration = .2f;
        public Ease RerollDrawEase = Ease.OutBack;
        [Space]
        public float RerollDiscardDuration = .2f;
        public Ease RerollDiscardEase = Ease.InSine;

        [Header("- Follow")]
        public float FollowSpeed = 10;
        public float RotationAmount = 20;
        public float RotationSpeed = 20;
        public float TiltSpeed = 20;

        [Header("- Select")]
        public float SelectScale = 1.25f;
        public float SelectScaleTweenDuration = .15f;
        public Ease SelectScaleEase = Ease.OutBack;

        [Header("- Shadow")]
        public float ShadowOffset = 20;

        [Header("- Curve")]
        public CurveParameters Curve;
        



        [Space(25)]
        [Header("[ Deck Visual ]")]
        public float DeckInteractionScale = 1.25f;
        public float DeckInteractionDuration = .15f;
        public Ease DeckInteractionEase = Ease.OutBack;


        public void SetDeckVisual(CardDeckVisual deck)
        {
            this.deck = deck;
        }
    }
}