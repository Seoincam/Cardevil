using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI.PopUp
{
    [RequireComponent(typeof(Image))]
    public class SlotButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum ButtonType { SelectOrUpgrade, Reroll }

        [Header("버튼 타입 설정")]
        public ButtonType type;

        [Header("상태별 스프라이트")]
        public Sprite defaultSprite;
        [Tooltip("리롤 버튼 전용 호버 이미지")]
        public Sprite hoverSprite;
        [Tooltip("클릭했을 때, 혹은 리롤 진행 중일 때의 이미지")]
        public Sprite pressedSprite;
        [Tooltip("작동 불가(재화 부족 등)일 때의 이미지")]
        public Sprite blockedSprite;

        private Image _image;
        private bool _isBlocked = false;
        private bool _isRolling = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
            UpdateVisual();
        }

        /// <summary>
        /// 외부(SlotMachine)에서 버튼의 블락 상태를 갱신할 때 호출
        /// </summary>
        public void SetBlocked(bool isBlocked)
        {
            _isBlocked = isBlocked;
            UpdateVisual();
        }

        /// <summary>
        /// 외부(SlotMachine)에서 리롤 상태를 갱신할 때 호출
        /// </summary>
        public void SetRolling(bool isRolling)
        {
            _isRolling = isRolling;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_image == null) return;

            // 1. 리롤 중일 경우 (호버/클릭 무시하고 유지)
            if (type == ButtonType.Reroll && _isRolling)
            {
                _image.sprite = pressedSprite;
                return;
            }

            // 2. 블락 상태일 경우
            if (type == ButtonType.SelectOrUpgrade && _isBlocked)
            {
                _image.sprite = blockedSprite;
                return;
            }

            // 3. 기본 상태
            _image.sprite = defaultSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isBlocked || _isRolling) return;

            if (type == ButtonType.Reroll && hoverSprite != null)
                _image.sprite = hoverSprite;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isBlocked || _isRolling) return;
            _image.sprite = defaultSprite;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isBlocked || _isRolling) return;

            if (pressedSprite != null)
                _image.sprite = pressedSprite;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isBlocked || _isRolling) return;

            // 손을 뗐을 때 기본으로 돌아감. 
            // 단, 클릭 로직에서 즉시 SetBlocked나 SetRolling이 호출되면 UpdateVisual이 우선됨
            _image.sprite = defaultSprite;
        }
    }
}