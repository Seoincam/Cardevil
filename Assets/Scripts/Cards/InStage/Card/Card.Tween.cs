using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.New;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.InStage.NCard
{
    public partial class NewCard
    {
        // 현재 트윈으로 움직이고 있는가?
        private bool _isTweening;
        
        private Vector3 _movementDelta;
        private float _curveYOffset;
        private Tween _hoverScaleTween;

        private bool HasParent => transform.parent;
        
        private void InitializeVisual(CardData cardData)
        {
            visual.ChangeVisualInstant(cardData);
            changeableMarker.gameObject.SetActive(cardData.CanOpenSelection);
        }

        /// <summary>
        /// <see cref="targetPosition"/>으로 보간해 부드럽게 따라감.
        /// </summary>
        private void SmoothFollowPosition()
        {
            if (!UseLerp) return;

            var verticalOffset = Vector3.up * (Is(State.Dragging) ? 0 : _curveYOffset);
            var newTargetPosition = targetPosition + verticalOffset;
            var t = visualSetting.FollowSpeed * Time.deltaTime;
            
            transform.position = Vector3.Lerp(transform.position, newTargetPosition, t);
        }

        /// <summary>
        /// 플립과 함께 비주얼을 변경합니다.
        /// </summary>
        public async UniTask UpdateVisualWithFlip(CardData cardData)
        {
            var count = changeFlipSetting.FlipCount;
            var backDur = changeFlipSetting.FrontToBackDuration;
            var frontDur = changeFlipSetting.BackToFrontDuration;
            var ease = changeFlipSetting.FlipEase;

            if (count <= 0)
            {
                LogEx.LogWarning($"{nameof(changeFlipSetting)}의 Count가 {count}입니다. 즉시 실행.");
                visual.ChangeVisualInstant(cardData);
                return;
            }

            while (count-- > 0)
            {
                await flip.FlipAsync(CardFace.Back, backDur, ease);
                if (count == 0)
                    visual.ChangeVisualInstant(cardData);
                await flip.FlipAsync(CardFace.Front, frontDur, ease);
            }
        }
        
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        /// <summary>
        /// 시각적 순서를 갱신함.
        /// </summary>
        public void UpdateVisualIndex(int index)
        {
            transform.SetSiblingIndex(index);
        }
        
        /// <summary>
        /// 값 선택 가능 마커의 표시 여부를 페이드 효과와 적용함. 
        /// </summary>
        public void SetChangeableImageWithFade(bool active)
        {
            changeableMarker.DOKill();
            changeableMarker.DOFade(active ? 1f : 0f, .2f).SetEase(Ease.InCirc);
        }

        /// <summary>
        /// 슬롯으로 이동함.
        /// </summary>
        public async UniTask MoveToSlotAsync()
        {
            await CreateMoveToSlotTween();
        }

        /// <summary>
        /// Flip과 함께 슬롯으로 이동함.
        /// 실행 전 뒷면임을 보장하고 실행됨.
        /// </summary>
        public async UniTask SafeMoveToSlotWithFlipAsync()
        {
            flip.StopAndFlipInstant(CardFace.Back);
            await MoveToSlotWithFlipAsync();
        }

        /// <summary>
        /// Flip과 함께 슬롯으로 이동함.
        /// </summary>
        public async UniTask MoveToSlotWithFlipAsync()
        {
            CardDeckVisual.Instance.OnInteraction();
            
            var flipTask = flip.FlipAsync(CardFace.Front, visualSetting.DrawFlipDuration, visualSetting.FlipEase);
            var moveTask = CreateMoveToSlotTween().ToUniTask();
            await UniTask.WhenAll(flipTask, moveTask);
        }

        /// <summary>
        /// Flip과 함께 덱으로 이동함.
        /// 실행 전 앞면임을 보장하고 실행됨.
        /// </summary>
        public async UniTask SafeMoveToDeckWithFlipAsync()
        {
            flip.StopAndFlipInstant(CardFace.Front);
            await MoveToSlotWithFlipAsync();
        }

        /// <summary>
        /// Flip과 함께 덱으로 이동함.
        /// </summary>
        public async UniTask MoveToDeckWithFlipAsync()
        {
            var flipTask = flip.FlipAsync(CardFace.Back, visualSetting.RerollFlipDuration, visualSetting.FlipEase);
            var moveTask = CreateMoveToDeckTween().ToUniTask();
            await UniTask.WhenAll(flipTask, moveTask);
            
            CardDeckVisual.Instance.OnInteraction();
        }
        
        // TODO: 트윈을 시간이 아닌 속도로!
        
        /// <returns>
        /// 슬롯으로 이동하는 트윈.
        /// </returns>
        /// <remarks>
        /// 선택됐을 경우 Offset만큼 올라옴.
        /// </remarks>
        private Tween CreateMoveToSlotTween()
        {
            _isTweening = true;
            return transform
                .DOLocalMove(LocalZeroPosition, visualSetting.RerollDrawDuration)
                .SetEase(visualSetting.RerollDrawEase)
                .OnComplete(() => _isTweening = false);
        }
        
        /// <returns>
        /// 덱으로 이동하는 트윈.
        /// </returns>
        private Tween CreateMoveToDeckTween()
        {
            _isTweening = true;
            return transform
                .DOMove(CardDeckVisual.Instance.Front.position, visualSetting.RerollDiscardDuration)
                .SetEase(visualSetting.RerollDiscardEase)
                .OnComplete(() => _isTweening = false);
        }
    }
}