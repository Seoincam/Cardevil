using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cardevil.UI.Components; // 추가: 제공해주신 툴팁 네임스페이스
using Cardevil.UI;

namespace Cardevil.Gameplay.Enemy
{
    // 이미지 컴포넌트와 툴팁 컴포넌트가 반드시 함께 있도록 강제합니다.
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ShowTooltipOnHover))]
    public class IconUIElement : MonoBehaviour
    {
        private Image _iconImage;
        private Tween _shakeTween;

        // 툴팁 컴포넌트 참조
        private ShowTooltipOnHover _tooltipComponent;

        private void Awake()
        {
            _iconImage = GetComponent<Image>();
            _tooltipComponent = GetComponent<ShowTooltipOnHover>();
        }

        // 스프라이트 변경 로직 (기존과 동일)
        public void SetSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                _iconImage.sprite = sprite;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
            }
        }

        /// <summary>
        /// 컨트롤러에서 호출하여 툴팁의 내용만 교체합니다.
        /// </summary>
        public void UpdateTooltipData(string title, string description)
        {
            // 1. 프리팹(Inspector)에 세팅되어 있는 기존 툴팁 데이터를 가져옵니다.
            // (만약 비어있다면 새로 생성합니다.)
            var currentData = _tooltipComponent.TooltipData ?? new TooltipData();

            // 2. 다른 내부 데이터는 건드리지 않고, 제목과 내용만 업데이트합니다.
            currentData.Title = title;
            currentData.Description = description;

            // 3. 변경된 데이터를 다시 적용합니다.
            _tooltipComponent.SetTooltipData(currentData);
        }

        // 긴박함 애니메이션 (기존과 동일)
        public void PlayWarningAnimation()
        {
            StopAnimation();
            _shakeTween = transform.DOShakeRotation(1f, new Vector3(0, 0, 15f), 10, 90f, false)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        // 애니메이션 정지 (기존과 동일)
        public void StopAnimation()
        {
            if (_shakeTween != null && _shakeTween.IsActive())
            {
                _shakeTween.Kill();
                _shakeTween = null;
            }
            transform.localRotation = Quaternion.identity;
        }
    }
}