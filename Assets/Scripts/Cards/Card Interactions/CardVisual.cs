using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.CardInteractinos
{
    [RequireComponent(typeof(Canvas))]
    
    public class CardVisual : MonoBehaviour
    {
        private Canvas canvas;
        private bool isInitalized = false;

        private Vector3 movementDelta;
        private Vector3 rotationDelta;

        [Header("Card")]
        public Card parentCard;
        private bool isDiscarded = false;
        private bool isDrawing = true;

        [Header("SO")]
        [SerializeField] CardVisualSetting visualSetting;
        [SerializeField] CardSpriteManager spriteManger;

        [Header("Card Visual")]
        [SerializeField] Transform shakeObject;
        [SerializeField] Image frontImage;
        [SerializeField] Image backImage;
        [SerializeField] Image[] numberImages;

        [Header("Shadow Visual")]
        [SerializeField] Transform shadowTransform;
        private Vector2 shadowOriginPosition;

        [Header("Curve")]
        [SerializeField] private CurveParameters curve;
        private float curveYOffset;
        private float curveRotationOffset;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            shadowOriginPosition = shadowTransform.localPosition;
        }

        public void Init(Card parentCard)
        {
            this.parentCard = parentCard;
            UpdateVisual();

            // 이벤트 구독
            parentCard.OnPointerDownEvent += PointerDown;
            parentCard.OnPointerUpEvent += OnPointerUp;
            parentCard.OnBeginDragEvent += OnBeginDrag;
            parentCard.OnEndDragEvent += OnEndDrag;
            parentCard.OnSelectValueStartEvent += OnSelectStarted;
            parentCard.OnSelectValueEndEvent += OnSelectEnded;

            parentCard.OnDraw += OnDraw;
            parentCard.OnDiscard += OnDiscard;
            parentCard.OnRerollDraw += OnRerollDraw;
            parentCard.OnRerollDiscard += OnRerollDiscard;
            parentCard.OnDestory += Destroy;

            transform.position = parentCard.HandBar.deck.Front.position;

            isInitalized = true;
        }

        void Update()
        {
            if (!isInitalized || parentCard == null)
                return;

            FollowWithLerp();
            FollowRotation();
            CurvePosition();
            TiltCard();
        }

        private void FollowWithLerp()
        {
            if (isDiscarded)
                return;
            if (parentCard.IsReroll)
                return;
            if (isDrawing)
                return;

            var verticalOffset = Vector3.up * (parentCard.isDragging ? 0 : curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: visualSetting.FollowSpeed * Time.deltaTime);
        }

        private void FollowRotation()
        {
            if (isDiscarded)
                return;

            Vector3 movement = transform.position - parentCard.transform.position;
            movementDelta = Vector3.Lerp(movementDelta, movement, 20 * Time.deltaTime);
            Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * visualSetting.RotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, visualSetting.RotationSpeed * Time.deltaTime);

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.Clamp(rotationDelta.x, -50f, 50f));
        }

        private void CurvePosition()
        {
            if (isDiscarded)
                return;
            if (parentCard.IsReroll)
                return;

            curveYOffset = curve.positioning.Evaluate(parentCard.NormalizedPosition) * curve.positioningInfluence * (parentCard.HandBar.StageCardsCtx.HandCount - 1);
            curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition);
        }

        private void TiltCard()
        {
            if (isDiscarded)
                return;

            float tiltZ = parentCard.isDragging ? 0 : (curveRotationOffset * (curve.rotationInfluence * (parentCard.HandBar.StageCardsCtx.HandCount - 1)));
            float lerpZ = Mathf.LerpAngle(tiltZ, shakeObject.localEulerAngles.z, visualSetting.TiltSpeed / 2 * Time.deltaTime);
            shakeObject.localEulerAngles = new Vector3(0, 0, lerpZ);
        }



        public void UpdateIndex()
        {
            transform.SetSiblingIndex(parentCard.HandIndex);
        }

        private void PointerDown(Card _)
        {
            transform.DOScale(endValue: visualSetting.SelectScale, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition += -Vector3.up * visualSetting.ShadowOffset;
        }

        private void OnPointerUp(Card _)
        {
            transform.DOScale(endValue: 1f, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition = shadowOriginPosition;
        }

        private void OnBeginDrag(Card _)
        {
            canvas.overrideSorting = true;
        }

        private void OnEndDrag(Card _)
        {
            canvas.overrideSorting = false;
        }


        private void OnDraw()
        {
            parentCard.HandBar.deck.OnInteraction();
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


        private void OnRerollDraw()
        {
            parentCard.HandBar.deck.OnInteraction();
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
                parentCard.HandBar.deck.OnInteraction();
                parentCard.Destroy();
            });
        }



        private void Destroy()
        {
            DOTween.Kill(transform);
            Destroy(gameObject);
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
            if (parentCard.data.valueType == CardData.ValueType.Move)
            {
                var move = parentCard.data.Move;
                transform.name = move.Direction.ToString();

                frontImage.sprite = spriteManger.GetMoveBackground(move.Direction, parentCard.data.selectType);
            }

            else if (parentCard.data.valueType == CardData.ValueType.Number)
            {
                var number = parentCard.data.Number;
                transform.name = $"{number.Color} {number.Number}";

                frontImage.sprite = spriteManger.GetNumberBackground(number.Color);

                // 일단 숫자 하나만 처리
                numberImages[0].sprite = spriteManger.GetNumber(number.Color, number.Number, parentCard.data.selectType);
                numberImages[0].gameObject.SetActive(true);
                numberImages[1].gameObject.SetActive(false);
                numberImages[2].gameObject.SetActive(false);
            }

            else
                Debug.LogError("cardData가 어떤 타입도 아닙니다.");
        }
    }
}