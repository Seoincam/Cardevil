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
        private Canvas canvas;

        private Vector3 movementDelta;
        private Vector3 rotationDelta;
        private float curveYOffset;
        private float curveRotationOffset;

        private bool isInitalized;
        private bool isDiscarded;
        private bool isDrawing;

        private Vector2 shadowOriginPosition;


        void Awake()
        {
            canvas = GetComponent<Canvas>();
            shadowOriginPosition = shadowTransform.localPosition;

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

            canvas.overrideSorting = false;
            isDiscarded = false;
            isInitalized = false;
        }

        public void Init(Card parentCard)
        {
            this.parentCard = parentCard;
            SubscribeToParent(parentCard);

            UpdateVisual();
            // Clear()에서 overrideSorting을 false로 설정해도
            // 실제로 다시 pool에서 꺼낼 때 true가 됨
            canvas.overrideSorting = false;
            transform.position = GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().Front.position;

            isInitalized = true;
        }

        void Update()
        {
            if (!isInitalized || parentCard == null || isDiscarded)
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
            if (isDrawing)
                return;

            var verticalOffset = Vector3.up * (parentCard.IsDragging ? 0 : curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: visualSetting.FollowSpeed * Time.deltaTime);
        }

        private void FollowRotation()
        {
            Vector3 movement = transform.position - parentCard.transform.position;
            movementDelta = Vector3.Lerp(movementDelta, movement, 20 * Time.deltaTime);
            Vector3 movementRotation = (parentCard.IsDragging ? movementDelta : movement) * visualSetting.RotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, visualSetting.RotationSpeed * Time.deltaTime);

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.Clamp(rotationDelta.x, -50f, 50f));
        }

        private void CurvePosition()
        {
            if (parentCard.IsReroll) return;
            if (!Managers.Card.StageCardsCtx.TryGetIndex(parentCard, out var idx)) return;

            var c = visualSetting.Curve;

            var factor = (float)idx / (Managers.Card.MaxHandCount - 1);
            curveYOffset = c.positioning.Evaluate(factor) * c.positioningInfluence * (Managers.Card.MaxHandCount - 1);
            curveRotationOffset = c.rotation.Evaluate(factor);
        }

        private void TiltCard()
        {
            if (isDiscarded) return;
            if (parentCard.IsReroll) return;

            var c = visualSetting.Curve;
            float tiltZ = parentCard.IsDragging ? 0 : (curveRotationOffset * (c.rotationInfluence * (Managers.Card.StageCardsCtx.HandCount - 1)));
            float lerpZ = Mathf.LerpAngle(tiltZ, shakeObject.localEulerAngles.z, visualSetting.TiltSpeed / 2 * Time.deltaTime);
            shakeObject.localEulerAngles = new Vector3(0, 0, lerpZ);
        }

        #endregion


        public void UpdateIndex(int index)
        {
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

            shadowTransform.localPosition = shadowOriginPosition;
        }

        private void OnBeginDrag()
        {
            canvas.overrideSorting = true;
        }

        private void OnEndDrag()
        {
            canvas.overrideSorting = false;
        }

        #endregion

        #region Draw/Discard Event

        private void OnRerollDraw()
        {
            canvas.overrideSorting = true;
            
            GameObject.Find("Deck Button").GetComponent<CardDeckVisual>().OnInteraction();
            transform.DOMove(endValue: parentCard.transform.position, visualSetting.RerollDrawDuration)
                        .SetEase(visualSetting.RerollDrawEase);

            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipBackImageRatio)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipFrontImageRation)
                        .SetEase(visualSetting.FlipEase));

            isDrawing = false;
        }

        private void OnRerollDiscard(Transform discardPoint)
        {
            isDiscarded = true;

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
            canvas.overrideSorting = false;
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

            tween.OnComplete(() => isDrawing = false);
        }

        private void OnDiscard()
        {
            isDiscarded = true;
            canvas.overrideSorting = true;

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
            canvas.overrideSorting = true;
        }

        private void OnSelectEnded(Card _)
        {
            canvas.overrideSorting = false;
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