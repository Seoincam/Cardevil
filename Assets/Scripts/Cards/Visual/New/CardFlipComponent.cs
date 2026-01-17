using Cardevil.Attributes;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.New
{
    public enum CardFace
    {
        None, Front, Back
    }
    
    /// <summary>
    /// 카드 앞뒷면 플립(회전) 애니메이션 담당 컴포넌트.
    /// 앞면과 뒷면 RectTransform을 90도로 회전시켜 카드 플립 효과를 제공함. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// - 보이지 않는 면은 자동으로 SetActive(false)로 처리되어 렌더링 안 함.
    /// </para>
    /// <para>
    /// - OnEnable 시 현재 회전 상태를 감지해 Face 자동 설정.
    /// </para>
    /// <para>
    /// - 애니메이션 도중 StopFlip() 호출 시 중간 상태로 남을 수 있으므로 StopAndFlipInstant() 사용 권장함!
    /// </para>
    /// </remarks>
    public class CardFlipComponent : MonoBehaviour
    {
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] 
        public RectTransform Front { get; private set; }
        
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)]
        public RectTransform Back { get; private set; }
        
        [field: SerializeField, VisibleOnly]
        public CardFace CurrentFace { get; private set; }

        private bool IsFront => CurrentFace == CardFace.Front;

        private static readonly Vector3 FrontEulerAngles = Vector3.zero;
        private static readonly Vector3 BackEulerAngles = new(0, 90, 0);
        
        private void OnEnable()
        {
            if (ApproximatelyEqual(Front.localEulerAngles, FrontEulerAngles) &&
                ApproximatelyEqual(Back.localEulerAngles, BackEulerAngles))
            {
                CurrentFace = CardFace.Front;
            }
            else if (ApproximatelyEqual(Front.localEulerAngles, BackEulerAngles) &&
                     ApproximatelyEqual(Back.localEulerAngles, FrontEulerAngles))
            {
                CurrentFace = CardFace.Back;
            }
            else
            {
                LogEx.LogWarning("현재 Front/Back 중 해당되는 Face가 없습니다.");
                CurrentFace = CardFace.None;
            }
        }

        /// <summary>
        /// 애니메이션과 함께 <see cref="targetFace"/>로 회전합니다.
        /// </summary>
        public async UniTask FlipAsync(CardFace targetFace, float duration, Ease ease)
        {
            if (targetFace == CardFace.None)
            {
                LogEx.LogError("Cannot flip to Face.None");
                return;
            }
            if (CurrentFace == targetFace) return;

            if (duration <= 0f)
            {
                FlipInstant(targetFace);
                return;
            }
            
            var toBack = IsFront ? Front : Back;
            var toFront = IsFront ? Back : Front;

            var backTween = toBack
                .DOLocalRotate(BackEulerAngles, duration * .5f)
                .SetEase(ease);
            var frontTween = toFront
                .DOLocalRotate(FrontEulerAngles, duration * .5f)
                .SetEase(ease);

            await DOTween.Sequence()
                .Append(backTween)
                .AppendCallback(() =>
                {
                    toBack.gameObject.SetActive(false);
                    toFront.gameObject.SetActive(true);
                })
                .Append(frontTween);
            
            CurrentFace = targetFace;
        }

        /// <summary>
        /// 즉시 <see cref="targetFace"/>로 회전합니다.
        /// </summary>
        public void FlipInstant(CardFace targetFace)
        {
            if (targetFace == CardFace.None)
            {
                LogEx.LogError("Cannot flip to Face.None");
                return;
            }
            
            if (CurrentFace == targetFace) return;

            if (targetFace == CardFace.Front)
            {
                Front.gameObject.SetActive(true);
                Back.gameObject.SetActive(false);
                
                Front.localEulerAngles = FrontEulerAngles;
                Back.localEulerAngles = BackEulerAngles;
            }
            else if (targetFace == CardFace.Back)
            {
                Front.gameObject.SetActive(false);
                Back.gameObject.SetActive(true);
                
                Front.localEulerAngles = BackEulerAngles;
                Back.localEulerAngles = FrontEulerAngles;
            }
            
            CurrentFace = targetFace;
        }

        /// <summary>
        /// 기존에 진행중인 Flip 애니메이션이 있다면 멈추고, 즉시 <see cref="targetFace"/>로 회전합니다.
        /// </summary>
        public void StopAndFlipInstant(CardFace targetFace)
        {
            StopFlip();
            FlipInstant(targetFace);
        }

        /// <summary>
        /// 기존에 진행중인 Flip 애니메이션을 멈춥니다.
        /// </summary>
        public void StopFlip()
        {
            Front.DOKill();
            Back.DOKill();
        }

        [ContextMenu("Flip Front")]
        private void SetFront() => FlipInstant(CardFace.Front);
        
        [ContextMenu("Flip Back")]
        private void SetBack() => FlipInstant(CardFace.Back);

        private static bool ApproximatelyEqual(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
        } 
    }
}