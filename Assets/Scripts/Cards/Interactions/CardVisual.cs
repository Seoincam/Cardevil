using Cardevil.Attributes;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Pools;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Interactions
{
    [RequireComponent(typeof(Canvas), typeof(Poolable))]
    public class CardVisual : MonoBehaviour, IEvaluateVisual, IClearable
    {
        [Header("Card")]
        [VisibleOnly] public Card parentCard;
        
        [Header("SO")]
        [SerializeField] CardVisualSettingSO visualSetting;
        [SerializeField] CardVisualSpriteFactorySO spriteFactory;

        [Header("Card Visual")]
        [SerializeField] Transform shakeObject;
        [SerializeField] Image frontImage;
        [SerializeField] Image backImage;
        [SerializeField] Image[] numberImages;

        [Header("Shadow Visual")]
        [SerializeField] Transform shadowTransform;


        private Poolable _poolable;
        private Canvas _canvas;

        private int _slotIndex;

        private Vector3 _movementDelta;
        private Vector3 _rotationDelta;
        private float _curveYOffset;
        private float _curveRotationOffset;

        private bool _isInitalized;
        private bool _isDiscarded;
        private bool _isDrawing;

        private Vector2 _shadowOriginPosition;


        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _shadowOriginPosition = shadowTransform.localPosition;

            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
        }

        public void Clear()
        {
            if (parentCard != null)
            {
                UnsubscribeFromParent(parentCard);
                parentCard = null;
            }

            frontImage.rectTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            backImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            shakeObject.localEulerAngles = Vector3.zero;

            _canvas.overrideSorting = false;
            _isDiscarded = false;
            _isInitalized = false;
        }

        public void Init(Card parentCard)
        {
            this.parentCard = parentCard;
            SubscribeToParent(parentCard);

            UpdateVisual();
            // Clear()에서 overrideSorting을 false로 설정해도
            // 실제로 다시 pool에서 꺼낼 때 true가 됨
            _canvas.overrideSorting = false;
            transform.position = GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().Front.position;

            _isInitalized = true;
        }

        void Update()
        {
            if (!_isInitalized || parentCard == null || _isDiscarded)
                return;

            FollowWithLerp();
            FollowRotation();
            CurvePosition();
            TiltCard();
        }


        #region Update

        private void FollowWithLerp()
        {
            if (parentCard.IsReroll)
                return;
            if (_isDrawing)
                return;

            var verticalOffset = Vector3.up * (parentCard.IsDragging ? 0 : _curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: visualSetting.FollowSpeed * Time.deltaTime);
        }

        private void FollowRotation()
        {
            Vector3 movement = transform.position - parentCard.transform.position;
            _movementDelta = Vector3.Lerp(_movementDelta, movement, 20 * Time.deltaTime);
            Vector3 movementRotation = (parentCard.IsDragging ? _movementDelta : movement) * visualSetting.RotationAmount;
            _rotationDelta = Vector3.Lerp(_rotationDelta, movementRotation, visualSetting.RotationSpeed * Time.deltaTime);

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.Clamp(_rotationDelta.x, -50f, 50f));
        }

        private void CurvePosition()
        {
            if (parentCard.IsReroll) return;
            var c = visualSetting.Curve;

            var factor = (float)_slotIndex / (Managers.Card.MaxHandCount - 1);
            _curveYOffset = c.positioning.Evaluate(factor) * c.positioningInfluence * (Managers.Card.MaxHandCount - 1);
            _curveRotationOffset = c.rotation.Evaluate(factor);
        }

        private void TiltCard()
        {
            if (_isDiscarded) return;
            if (parentCard.IsReroll) return;

            var c = visualSetting.Curve;
            float tiltZ = parentCard.IsDragging ? 0 : (_curveRotationOffset * (c.rotationInfluence * (6)));
            float lerpZ = Mathf.LerpAngle(tiltZ, shakeObject.localEulerAngles.z, visualSetting.TiltSpeed / 2 * Time.deltaTime);
            shakeObject.localEulerAngles = new Vector3(0, 0, lerpZ);
        }

        #endregion


        public void UpdateIndex(int index)
        {
            _slotIndex = index;
            transform.SetSiblingIndex(index);
        }


        #region Subscribe

        private void SubscribeToParent(Card p)
        {
            if (p == null) return;

            p.PointerDown += OnPointerDown;
            p.PointerUp += OnPointerUp;
            p.DragStarted += OnBeginDrag;
            p.DragEnded += OnEndDrag;
            p.ValueSelectionStarted += OnSelectStarted;
            p.ValueSelectionEnded += OnSelectEnded;

            p.RerollDrawn += OnRerollDraw;
            p.RerollDiscarded += OnRerollDiscard;
            p.RerollEnded += OnRerollEnd;
            p.Drawn += OnDraw;
            p.Discarded += OnDiscard;
            p.Destroyed += Destroy;
        }

        private void UnsubscribeFromParent(Card p)
        {
            if (p == null) return;

            p.PointerDown -= OnPointerDown;
            p.PointerUp -= OnPointerUp;
            p.DragStarted -= OnBeginDrag;
            p.DragEnded -= OnEndDrag;
            p.ValueSelectionStarted -= OnSelectStarted;
            p.ValueSelectionEnded -= OnSelectEnded;

            p.RerollDrawn -= OnRerollDraw;
            p.RerollDiscarded -= OnRerollDiscard;
            p.RerollEnded -= OnRerollEnd;
            p.Drawn -= OnDraw;
            p.Discarded -= OnDiscard;
            p.Destroyed -= Destroy;
        }

        #endregion

        #region Pointer Event

        private void OnPointerDown(Card _, CardPointerArgs args)
        {
            transform.DOScale(endValue: visualSetting.SelectScale, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition += -Vector3.up * visualSetting.ShadowOffset;
        }

        private void OnPointerUp(Card _, CardPointerArgs args)
        {
            transform.DOScale(endValue: 1f, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition = _shadowOriginPosition;
        }

        private void OnBeginDrag()
        {
            _canvas.overrideSorting = true;
        }

        private void OnEndDrag()
        {
            _canvas.overrideSorting = false;
        }

        #endregion

        #region Draw/Discard Event

        private void OnRerollDraw()
        {
            _canvas.overrideSorting = true;
            
            GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().OnInteraction();
            transform.DOMove(endValue: parentCard.transform.position, visualSetting.RerollDrawDuration)
                        .SetEase(visualSetting.RerollDrawEase);

            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipBackImageRatio)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipFrontImageRation)
                        .SetEase(visualSetting.FlipEase));

            _isDrawing = false;
        }

        private void OnRerollDiscard(Transform discardPoint)
        {
            _isDiscarded = true;

            var tween = transform.DOMove(endValue: discardPoint.position, visualSetting.RerollDiscardDuration)
                        .SetEase(visualSetting.RerollDiscardEase);

            var sequence = DOTween.Sequence();
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.RerollFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.RerollFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));

            tween.OnComplete(() =>
            {
                GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().OnInteraction();
                parentCard.Destroy();
            });
        }

        private void OnRerollEnd()
        {
            _canvas.overrideSorting = false;
        }

        private void OnDraw()
        {
            GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().OnInteraction();
            var tween = transform.DOMove(endValue: parentCard.transform.position, visualSetting.DrawDuration)
                        .SetEase(visualSetting.RerollDrawEase);

            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.DrawFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.DrawFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));

            tween.OnComplete(() => _isDrawing = false);
        }

        private void OnDiscard()
        {
            _isDiscarded = true;
            _canvas.overrideSorting = true;

            var tween = transform.DOLocalMove(endValue: new Vector3(1050, 0, 0), visualSetting.DiscardDuration)
                        .SetEase(visualSetting.DiscardEase);
            tween.OnComplete(() => parentCard.Destroy());
        }

        #endregion

        private void Destroy()
        {
            DOTween.Kill(transform);
            Managers.Resource.Destroy(gameObject);
        }

        private void OnSelectStarted(Card _)
        {
            _canvas.overrideSorting = true;
        }

        private void OnSelectEnded(Card _)
        {
            _canvas.overrideSorting = false;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            spriteFactory.UpdataVisual(parentCard.Data, frontImage, numberImages[0]);
        }

        public void ExecuteEvaluationAction()
        {
            transform.DOShakePosition(.6f, 50);
        }
    }
}