using Cardevil.Utils.Directions;
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
            parentCard.OnSelectStartEvent += OnSelectStarted;
            parentCard.OnSelectEndEvent += OnSelectEnded;

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
        }

        private void FollowWithLerp()
        {
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position, t: followSpeed * Time.deltaTime);
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
        }

        public void UpdateVisual()
        {
            // 이름 설정 (임시)
            if (parentCard.data is DirectionCardData dirCard)
            {
                transform.name = dirCard.Value.ToString();
                var textString = dirCard.Value != Direction.None ? dirCard.Value.ToString() : "All";
                if (dirCard.Value != Direction.None && dirCard.canSelect)
                    textString += "*";
                text.text = textString;
                text.fontSize = 35;
            }

            else if (parentCard.data is NumberCardData numCard)
            {
                transform.name = $"{numCard.Color} {numCard.Value}";
                var textString = numCard.Value == 0 ? "*" : numCard.Value.ToString();
                if (numCard.Value != 0 && numCard.canSelect)
                    textString += "*";
                text.text = textString;
                switch (numCard.Color)
                {
                    case CardColor.Green: text.color = new Color(.25f, .7f, .25f); break;
                    case CardColor.Blue: text.color = Color.blue; break;
                    case CardColor.Red: text.color = Color.red; break;
                    default: break;
                }
            }

            else
                Debug.LogError("cardData가 어떤 타입도 아닙니다.");
        }
    }
}