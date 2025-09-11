using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardVisual : MonoBehaviour
    {
        [Header("Card")]
        public Card parentCard;
        private bool isDiscarded = false;

        [Header("Visual")]
        [SerializeField] CardVisualSetting visualSetting;
        [SerializeField] CardSpriteManager spriteManger;

        [Space, SerializeField] Transform shakeObject;
        [SerializeField] Image frontImage;
        [SerializeField] Image backImage;
        [SerializeField] Image[] numberImages;
        
        private Canvas canvas;
        private bool isInitalized = false;

        [Header("Shadow Visual")]
        [SerializeField] Transform shadowTransform;
        private Vector2 shadowOriginPosition;

        [Header("Curve")]
        [SerializeField] private CurveParameters curve;
        private float curveYOffset;
        private float curveRotationOffset;

        private Vector3 movementDelta;
        private Vector3 rotationDelta;

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

            parentCard.OnSpawn += OnSpawn;
            parentCard.OnDiscard += OnDiscard;
            parentCard.OnDestory += Destroy;

            transform.position = visualSetting.deck.FrontCardTransform.position;

            isInitalized = true;
        }

        void Update()
        {
            if (!isInitalized || parentCard == null)
                return;

            if (isDiscarded)
                return;

            FollowWithLerp();
            FollowRotation();
            CurvePosition();
            TiltCard();
        }

        private void FollowWithLerp()
        {
            var verticalOffset = Vector3.up * (parentCard.isDragging ? 0 : curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: visualSetting.FollowSpeed * Time.deltaTime);
        }

        private void FollowRotation()
        {
            Vector3 movement = transform.position - parentCard.transform.position;
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * visualSetting.RotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, visualSetting.RotationSpeed * Time.deltaTime);

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.Clamp(rotationDelta.x, -50f, 50f));
        }

        private void CurvePosition()
        {
            if (parentCard.IsReroll)
                return;

            curveYOffset = curve.positioning.Evaluate(parentCard.NormalizedPosition) * curve.positioningInfluence * (parentCard.BarGroup.StageCardsCtx.HandCount - 1);
            curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition);
        }

        private void TiltCard()
        {
            float tiltZ = parentCard.isDragging ? 0 : (curveRotationOffset * (curve.rotationInfluence * (parentCard.BarGroup.StageCardsCtx.HandCount - 1)));
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

        private void OnSpawn()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.spawnFlipDuration * .5f)
                        .SetEase(visualSetting.spawnFlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.spawnFlipDuration * .5f)
                        .SetEase(visualSetting.spawnFlipEase));
        }

        private void OnDiscard(float discardDuration)
        {
            isDiscarded = true;
            canvas.overrideSorting = true;

            frontImage.transform.DORotate(endValue: new Vector3(0, 60, 0), discardDuration)
                .SetEase(Ease.OutBack);
            transform.DOLocalJump(endValue: new Vector3(1050, -30, 0), jumpPower: 15f, numJumps: 1, discardDuration);
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