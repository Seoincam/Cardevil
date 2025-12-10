using UnityEngine;
using DG.Tweening;

namespace Cardevil.Cards.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CardVisualSetting", menuName = "Cards/Card Visual Setting")]
    public class CardVisualSettingSO : ScriptableObject
    {
        [Header("Fast Mode")]
        [SerializeField, Tooltip("true일 시, 카드 모션의 속도를 더 빠르게 재생.")]
        bool _isFastMode;
        [SerializeField, Tooltip("배속값. 0.5f일시 2배 속도임."), Range(.25f, .75f)]
        float _fastModeMultiflier = .5f;

        private float SpeedFactor => _isFastMode ? _fastModeMultiflier : 1f;
        [Space(25)]


        [Header("[ Common ]")]
        [SerializeField] float _selectOffset = 50f;
        [Space(25)]



        [Header("[ Reroll ]")]

        [Header("- Interval")]
        [SerializeField] float _rDrawInterval = .15f;
        [SerializeField] float _rDiscardInterval = .125f;
        [SerializeField] float _rDrawDiscardInterval = 1f;

        [Space]
        [SerializeField] float _rEndInterval = 1f;
        [SerializeField] float _rEndUpdateSlotInterval = .05f;

        [Space]
        [SerializeField] float _rCountScale = 1.4f;
        [SerializeField] float _rCountScaleDuration = .4f;
        [SerializeField] Ease _rCountEase = Ease.InOutBounce;

        [Header("- CardVisual Tween Duration")]
        [SerializeField] float _rFlipDuration = .3f;
        [SerializeField, Range(0, 1)] float _rFlipBackImageRatio = .8f;

        [Space]
        [SerializeField] float _rDrawDuration = .8f;
        [SerializeField] Ease _rDrawEase = Ease.OutBack;
        [SerializeField] float _rDiscardDuration = .7f;
        [SerializeField] Ease _rDiscardEase = Ease.InBack;
        [SerializeField] float _rEndMoveDuration = .5f;
        [SerializeField] Ease _rEndMoveEase = Ease.InBack;
        [Space(25)]



        [Header("[ HandBar ]")]
        [SerializeField] float _drawInterval = .225f;
        [SerializeField] float _discardInterval = .3f;
        [SerializeField] float _discardDrawInterval = .75f;
        [SerializeField] float _endDragTweenDuration = .2f;
        [Space(25)]



        [Header("[ Card ]")]
        [SerializeField] float _moveSpeedLimit = 4000f;
        [SerializeField] float _clickDetectThreshold = .2f;
        [Space(25)]



        [Header("[ Card Visual ]")]
        [Header("- Flip")]
        [SerializeField] Ease _flipEase = Ease.InOutSine;

        [Header("- Draw/Discard")]
        [SerializeField] float _drawFlipDuration = .4f;
        [SerializeField] float _drawDuration = .5f;
        [SerializeField] Ease _drawEase = Ease.InExpo;

        [Space]
        [SerializeField] float _discardDuration = .25f;
        [SerializeField] Ease _discardEase = Ease.InOutCirc;

        [Header("- Follow")]
        [SerializeField] float _followSpeed = 9;
        [SerializeField] float _rotationAmount = .2f;
        [SerializeField] float _rotationSpeed = 5;
        [SerializeField] float _tiltSpeed = 20f;

        [Header("- Select")]
        [SerializeField] float _selectScale = 1.25f;
        [SerializeField] float _selectScaleTweenDuration = .15f;
        [SerializeField] Ease _selectScaleEase = Ease.OutBack;

        [Header("- Shadow")]
        [SerializeField] float _shadowOffset = 20f;

        [Header("- Curve")]
        [SerializeField] private CurveParameters _curve;

        [Header("- Hover")] 
        public float hoverScale;
        public float hoverScaleTweenDuration = .15f;
        public Ease hoverEase = Ease.OutBack;
        [Space(25)] 



        [Header("[ Deck Visual ]")]
        [SerializeField] float _deckInteractionScale = 1.125f;
        [SerializeField] float _deckInteractionDuration = .09f;
        [SerializeField] Ease _deckInteractionEase = Ease.InOutBounce;
        [Space(25)] 
        [SerializeField] private float deckRemainViewToggleDur;

        [Header("[ Etc ]")]
        [SerializeField] private float _reviveInterval = .4f;



        #region Properties

        public float SelectOffset => _selectOffset;
        
        public float RerollDrawInterval => _rDrawInterval * SpeedFactor;
        public float RerollDiscardInterval => _rDiscardInterval * SpeedFactor;
        public float RerollDrawDiscardInterval => _rDrawDiscardInterval * SpeedFactor;
        
        public float EndRerollInterval => _rEndInterval * SpeedFactor;
        public float EndRerollUpdateSlotInterval => _rEndUpdateSlotInterval * SpeedFactor;
        
        public float RerollCountScale => _rCountScale;
        public float RerollCountScaleDuration => _rCountScaleDuration * SpeedFactor;
        public Ease RerollCountEase => _rCountEase;
        
        public float RerollFlipDuration => _rFlipDuration * SpeedFactor;
        public float RerollFlipBackImageRatio => _rFlipBackImageRatio;
        public float RerollFlipFrontImageRation => 1f - _rFlipBackImageRatio;
        
        public float RerollDrawDuration => _rDrawDuration * SpeedFactor;
        public Ease RerollDrawEase => _rDrawEase;
        public float RerollDiscardDuration => _rDiscardDuration * SpeedFactor;
        public Ease RerollDiscardEase => _rDiscardEase;
        public float RerollEndMoveDuration => _rEndMoveDuration * SpeedFactor;
        public Ease RerollEndMoveEase => _rEndMoveEase;
        
        public float DrawInterval => _drawInterval * SpeedFactor;
        public float DiscardInterval => _discardInterval * SpeedFactor;
        public float DiscardDrawInterval => _discardDrawInterval * SpeedFactor;
        public float EndDragTweenDuration => _endDragTweenDuration * SpeedFactor;
        
        public float MoveSpeedLimit => _moveSpeedLimit;
        public float ClickDetectThreshold => _clickDetectThreshold;
        
        public Ease FlipEase => _flipEase;
        
        public float DrawFlipDuration => _drawFlipDuration * SpeedFactor;
        public float DrawDuration => _drawDuration * SpeedFactor;
        public Ease DrawEase => _drawEase;
        
        public float DiscardDuration => _discardDuration * SpeedFactor;
        public Ease DiscardEase => _discardEase;
        
        public float FollowSpeed => _followSpeed;
        public float RotationAmount => _rotationAmount;
        public float RotationSpeed => _rotationSpeed;
        public float TiltSpeed => _tiltSpeed;
        
        public float SelectScale => _selectScale;
        public float SelectScaleTweenDuration => _selectScaleTweenDuration * SpeedFactor;
        public Ease SelectScaleEase => _selectScaleEase;
        
        public float ShadowOffset => _shadowOffset;
        public CurveParameters Curve => _curve;
        
        public float DeckInteractionScale => _deckInteractionScale;
        public float DeckInteractionDuration => _deckInteractionDuration * SpeedFactor;
        public Ease DeckInteractionEase => _deckInteractionEase;

        public float DeckRemainViewToggleDur => deckRemainViewToggleDur * SpeedFactor;

        public float ReviveInterval => _reviveInterval * SpeedFactor;
        
        #endregion
    }
}