using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Cardevil.Gameplay.Enemy
{
    public class IconController : MonoBehaviour
    {
        [Header("Shared Prefab & Container")]
        [SerializeField] protected IconUIElement _iconPrefab;
        [SerializeField] protected Transform _iconContainer;

        [Header("Test_Sprites")]
        [SerializeField] Sprite TestDefaultSprite;
        [SerializeField] string defaultNameText;
        [SerializeField] string defaultContentText;

        protected List<IconUIElement> _activeIcons = new List<IconUIElement>();


        private void Start()
        {
            AddIcon(TestDefaultSprite, defaultNameText, defaultContentText); // 테스트용도
        }

        /// <summary>
        /// 아이콘을 컨테이너에 동적으로 추가합니다.
        /// </summary>
        public virtual IconUIElement AddIcon(Sprite iconSprite, string tooltipTitle, string tooltipDesc)
        {
            if (_iconPrefab == null || _iconContainer == null) return null;

            IconUIElement newIcon = Instantiate(_iconPrefab, _iconContainer);
            newIcon.SetSprite(iconSprite);
            newIcon.UpdateTooltipData(tooltipTitle, tooltipDesc);

            _activeIcons.Add(newIcon);
            return newIcon;
        }

        /// <summary>
        /// 특정 아이콘을 삭제합니다.
        /// </summary>
        public virtual void RemoveIcon(IconUIElement targetIcon)
        {
            if (targetIcon != null && _activeIcons.Contains(targetIcon))
            {
                _activeIcons.Remove(targetIcon);
                Destroy(targetIcon.gameObject);
            }
        }

        /// <summary>
        /// 동적으로 생성된 모든 아이콘을 삭제합니다.
        /// </summary>
        public virtual void ClearAllIcons()
        {
            foreach (var icon in _activeIcons)
            {
                if (icon != null) Destroy(icon.gameObject);
            }
            _activeIcons.Clear();
        }
    }
}