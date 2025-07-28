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
        [SerializeField] private float followSpeed = 30;

        [Header("Scale Settings")]
        [SerializeField] private float selectScale = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            transform.localPosition = new Vector3(-1000, -150);
            shadowOriginPosition = shadowTransform.localPosition;
        }

        public void Init(Card parentCard, CardData cardData)
        {
            this.parentCard = parentCard;

            transform.name = cardData.Type == CardType.Move
                ? cardData.Direction.ToString()
                : $"{cardData.Color} {cardData.Value}";

            // 이벤트 구독
            parentCard.PointerDownEvent += PointerDown;
            parentCard.PointerUpEvent += PointerUp;
            parentCard.BeginDragEvent += BeginDrag;
            parentCard.EndDragEvent += EndDrag;

            // 텍스트 설정 (임시)
            if (cardData.Type == CardType.Move)
            {
                text.text = cardData.Direction.ToString();
                text.fontSize = 35;
            }
            else
            {
                text.text = cardData.Value == 10 ? "*" : cardData.Value.ToString();
                switch (cardData.Color)
                {
                    case CardColor.Green: text.color = new Color(.25f, .7f, .25f); break;
                    case CardColor.Blue: text.color = Color.blue; break;
                    case CardColor.Red: text.color = Color.red; break;
                    default: break;
                }
            }

            isInitalized = true;
        }

        void Update()
        {
            if (!isInitalized || parentCard == null)
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

        private void PointerUp(Card _)
        {
            transform.DOScale(endValue: 1f, duration: scaleTransition)
                .SetEase(scaleEase);

            shadowTransform.localPosition = shadowOriginPosition;
        }

        private void BeginDrag(Card _)
        {
            canvas.overrideSorting = true;
        }

        private void EndDrag(Card _)
        {
            canvas.overrideSorting = false;
        }

        public void SetCardOnTrashCan(bool onTrashCan)
        {
            cardImage.color = onTrashCan
                ? new Color(.85f, .45f, .45f)
                : Color.white;
        }
    }
}