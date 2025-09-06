using DG.Tweening;
using TMPro;
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
        [SerializeField] Image cardImage;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Transform shakeObject;
        private Canvas canvas;
        private bool isInitalized = false;

        [Header("Shadow Visual")]
        [SerializeField] Transform shadowTransform;
        private Vector2 shadowOriginPosition;
        private float shadowOffset = 20;

        [Header("Follow Setting")]
        [SerializeField] private float followSpeed = 10;

        [Header("Scale Settings")]
        [SerializeField] private float selectScale = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

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

            parentCard.OnSpawn += OnSpawn;
            parentCard.OnDiscard += OnDiscard;
            parentCard.OnDestory += Destroy;

            isInitalized = true;
        }

        void Update()
        {
            if (!isInitalized || parentCard == null)
                return;

            if (isDiscarded)
                return;

            FollowWithLerp();
            CurvePosition();
            TiltCard();
        }

        private void FollowWithLerp()
        {
            var verticalOffset = Vector3.up * (parentCard.isDragging ? 0 : curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: followSpeed * Time.deltaTime);
        }

        private void CurvePosition()
        {
            curveYOffset = curve.positioning.Evaluate(parentCard.NormalizedPosition) * curve.positioningInfluence * (parentCard.BarGroup.StageCardsCtx.HandCount - 1);
            curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition);
        }

        private void TiltCard()
        {
            float tiltZ = parentCard.isDragging ? 0 : (curveRotationOffset * (curve.rotationInfluence * (parentCard.BarGroup.StageCardsCtx.HandCount - 1)));
            // float lerpZ = Mathf.LerpAngle(tiltZ, )
            shakeObject.eulerAngles = new Vector3(0, 0, tiltZ);
        }

        public void UpdateIndex()
        {
            transform.SetSiblingIndex(parentCard.HandIndex);
        }

        private void PointerDown(Card _)
        {
            transform.DOScale(endValue: selectScale, duration: scaleTransition)
                .SetEase(scaleEase);

            shadowTransform.localPosition += -Vector3.up * shadowOffset;
        }

        private void OnPointerUp(Card _)
        {
            transform.DOScale(endValue: 1f, duration: scaleTransition)
                .SetEase(scaleEase);

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
            // var duration = 1f;
            // cardImage.transform.DORotate(endValue: new Vector3(0, 0, 0), duration);
        }

        private void OnDiscard(float discardDuration)
        {
            isDiscarded = true;
            canvas.overrideSorting = true;

            cardImage.transform.DORotate(endValue: new Vector3(0, 60, 0), discardDuration)
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
            // 이름 설정 (임시)
            if (parentCard.data.valueType == CardData.ValueType.Move)
            {
                var move = parentCard.data.Move;
                transform.name = move.direction.ToString();

                string textString;
                if (parentCard.data.selectType == CardData.SelectType.All)
                {
                    if (!move.isSet)
                        textString = "All";
                    else
                        textString = move.direction.ToString() + "*";
                }
                else
                {
                    textString = move.direction.ToString();
                }
                text.text = textString;
                text.fontSize = 35;
            }

            else if (parentCard.data.valueType == CardData.ValueType.Number)
            {
                var number = parentCard.data.Number;
                transform.name = $"{number.color} {number.number}";
                var textString = number.number == 0 ? "*" : number.number.ToString();
                if (number.number != 0 && parentCard.data.CanOpenSelection)
                    textString += "*";
                text.text = textString;
                switch (number.color)
                {
                    case NumberData.CardColor.Green: text.color = new Color(.25f, .7f, .25f); break;
                    case NumberData.CardColor.Blue: text.color = Color.blue; break;
                    case NumberData.CardColor.Red: text.color = Color.red; break;
                    default: break;
                }
            }

            else
                Debug.LogError("cardData가 어떤 타입도 아닙니다.");
        }
    }
}