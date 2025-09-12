using UnityEngine;
using DG.Tweening;

namespace Cardevil.Cards.CardInteractinos
{
    [CreateAssetMenu(fileName = "CardVisualSetting", menuName = "Cards/Card Visual Setting")]
    public class CardVisualSetting : ScriptableObject
    {
        [Header("Setting")]
        [SerializeField, Tooltip("true일 시, 카드 모션의 속도를 2배로 재생합니다.")] private bool _isFastTestMode;


        [Space(25)]
        [Header("[ Common ]")]
        [SerializeField] private float _selectOffset = 50f;
        public float SelectOffset => _selectOffset;


        [Space(25)]
        [Header("[ HandBar ]")]

        [Header("- Reroll")]
        [SerializeField] private float _rerollDrawDiscardInterval = .75f;
        [SerializeField] private float _rerollDrawInterval = .15f;
        [SerializeField] private float _rerollDiscardInterval = .15f;

        public float RerollDrawDiscardInterval => _isFastTestMode ? _rerollDrawDiscardInterval * 0.5f : _rerollDrawDiscardInterval;
        public float RerollDrawInterval => _isFastTestMode ? _rerollDrawInterval * 0.5f : _rerollDrawInterval;
        public float RerollDiscardInterval => _isFastTestMode ? _rerollDiscardInterval * 0.5f : _rerollDiscardInterval;

        [Space]
        [SerializeField] private float _rerollCountScale = 1.3f;
        [SerializeField] private float _rerollCountScaleDuration = .2f;
        [SerializeField] private Ease _rerollCountEase = Ease.OutBack;

        public float RerollCountScale => _rerollCountScale;
        public float RerollCountScaleDuration => _isFastTestMode ? _rerollCountScaleDuration * 0.5f : _rerollCountScaleDuration;
        public Ease RerollCountEase => _rerollCountEase;

        [Space]
        [SerializeField] private float _endRerollInterval = .5f;
        public float EndRerollInterval => _isFastTestMode ? _endRerollInterval * 0.5f : _endRerollInterval;

        [Header("- In Game")]
        [SerializeField] private float _drawInterval = .2f;
        [SerializeField] private float _discardInterval = .3f;
        [SerializeField] private float _discardDrawInterval = .75f;
        [SerializeField] private float _reviveInterval = .4f;
        [SerializeField] private float _endDragTweenDuration = .2f;

        public float DrawInterval => _isFastTestMode ? _drawInterval * 0.5f : _drawInterval;
        public float DiscardInterval => _isFastTestMode ? _discardInterval * 0.5f : _discardInterval;
        public float DiscardDrawInterval => _isFastTestMode ? _discardDrawInterval * 0.5f : _discardDrawInterval;
        public float ReviveInterval => _isFastTestMode ? _reviveInterval * 0.5f : _reviveInterval;
        public float EndDragTweenDuration => _isFastTestMode ? _endDragTweenDuration * 0.5f : _endDragTweenDuration;


        [Space(25)]
        [Header("[ Card ]")]
        [SerializeField] private float _moveSpeedLimit = 4000f;
        [SerializeField] private float _clickDetectThreshold = .2f;

        public float MoveSpeedLimit => _moveSpeedLimit;
        public float ClickDetectThreshold => _clickDetectThreshold;


        [Space(25)]
        [Header("[ Card Visual ]")]
        [SerializeField] private CardDeckVisual _deck;
        [SerializeField] private Ease _flipEase = Ease.InElastic;

        public CardDeckVisual Deck => _deck;
        public Ease FlipEase => _flipEase;

        [Header("- In Game")]
        [SerializeField] private float _drawFlipDuration = .4f;
        [SerializeField] private float _drawDuration = .2f;
        [SerializeField] private Ease _drawEase = Ease.OutBack;

        public float DrawFlipDuration => _isFastTestMode ? _drawFlipDuration * 0.5f : _drawFlipDuration;
        public float DrawDuration => _isFastTestMode ? _drawDuration * 0.5f : _drawDuration;
        public Ease DrawEase => _drawEase;

        [Space]
        [SerializeField] private float _discardDuration = .4f;
        [SerializeField] private Transform _discardPoint;
        [SerializeField] private Ease _discardEase = Ease.InBack;

        public float DiscardDuration => _isFastTestMode ? _discardDuration * 0.5f : _discardDuration;
        public Transform DiscardPoint => _discardPoint;
        public Ease DiscardEase => _discardEase;

        [Header("- Reroll")]
        [SerializeField] private float _rerollFlipDuration = .2f;
        [SerializeField, Range(0, 1)] private float _rerollFlipBackImageRatio = .5f;
        public float RerollFlipDuration => _isFastTestMode ? _rerollFlipDuration * 0.5f : _rerollFlipDuration;
        public float RerollFlipBackImageRatio => _rerollFlipBackImageRatio;
        public float RerollFlipFrontImageRation => 1f - _rerollFlipBackImageRatio;

        [Space]
        [SerializeField] private float _rerollDrawDuration = .2f;
        [SerializeField] private Ease _rerollDrawEase = Ease.OutBack;

        public float RerollDrawDuration => _isFastTestMode ? _rerollDrawDuration * 0.5f : _rerollDrawDuration;
        public Ease RerollDrawEase => _rerollDrawEase;

        [Space]
        [SerializeField] private float _rerollDiscardDuration = .2f;
        [SerializeField] private Ease _rerollDiscardEase = Ease.InSine;

        public float RerollDiscardDuration => _isFastTestMode ? _rerollDiscardDuration * 0.5f : _rerollDiscardDuration;
        public Ease RerollDiscardEase => _rerollDiscardEase;

        [Header("- Follow")]
        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _rotationAmount = 20f;
        [SerializeField] private float _rotationSpeed = 20f;
        [SerializeField] private float _tiltSpeed = 20f;

        public float FollowSpeed => _followSpeed;
        public float RotationAmount => _rotationAmount;
        public float RotationSpeed => _rotationSpeed;
        public float TiltSpeed => _tiltSpeed;

        [Header("- Select")]
        [SerializeField] private float _selectScale = 1.25f;
        [SerializeField] private float _selectScaleTweenDuration = .15f;
        [SerializeField] private Ease _selectScaleEase = Ease.OutBack;

        public float SelectScale => _selectScale;
        public float SelectScaleTweenDuration => _isFastTestMode ? _selectScaleTweenDuration * 0.5f : _selectScaleTweenDuration;
        public Ease SelectScaleEase => _selectScaleEase;

        [Header("- Shadow")]
        [SerializeField] private float _shadowOffset = 20f;
        public float ShadowOffset => _shadowOffset;

        [Header("- Curve")]
        [SerializeField] private CurveParameters _curve;
        public CurveParameters Curve => _curve;


        [Space(25)]
        [Header("[ Deck Visual ]")]
        [SerializeField] private float _deckInteractionScale = 1.25f;
        [SerializeField] private float _deckInteractionDuration = .15f;
        [SerializeField] private Ease _deckInteractionEase = Ease.OutBack;

        public float DeckInteractionScale => _deckInteractionScale;
        public float DeckInteractionDuration => _isFastTestMode ? _deckInteractionDuration * 0.5f : _deckInteractionDuration;
        public Ease DeckInteractionEase => _deckInteractionEase;

        public void SetDeckVisual(CardDeckVisual deck)
        {
            _deck = deck;
        }
    }
}