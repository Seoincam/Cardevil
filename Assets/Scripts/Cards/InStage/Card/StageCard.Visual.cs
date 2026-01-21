using Cardevil.Cards.Core;
using Cardevil.Cards.Visual;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    // StageCard.Tween.cs
    public partial class StageCard
    {
        private Vector3 _movementDelta;
        private float _curveYOffset;
        private float _curveRotation;
        
        /// <summary>
        /// Slot으로 Lerp 이동을 할 지 여부.
        /// </summary>
        private bool LerpToSlot => !Is(State.Rerolling) && !Is(State.Tweening) && !Is(State.Dragging);

        /// <summary>
        /// 드래그되고 있지 않을 때 돌아갈 원점.
        /// 선택이 된 상태라면 위로 <c>visualSetting.SelectOffset</c>만큼 더 올라감.
        /// </summary>
        private Vector3 LocalZeroPosition
        {
            get
            {
                var position = new Vector3();
                
                if (Is(State.Selected))
                    position.y += visualSetting.SelectOffset;

                if (!Is(State.Dragging))
                    position.y += _curveYOffset;

                return position;
            }
        } 
        
        private void InitializeVisual(CardData cardData)
        {
            // TODO: 스프라이트 개수별로 다른 오브젝트를 소환할지 고려하기.
            visual.ChangeVisualInstant(cardData);
            changeableMarker.gameObject.SetActive(cardData.CanOpenSelection);
        }
        
        /// <summary>
        /// 손패에서의 커브를 계산하고 적용함.
        /// </summary>
        private void CalculateCurveInHand()
        {
            if (Is(State.Rerolling) || Is(State.Discarded)) return;
            
            int indexInHand = StageCardsModel.Current.GetIndexInHand(this);
            int totalCount = StageCardsModel.Current.MaxHand;

            (_curveYOffset, _curveRotation) = curveConfig.GetCurve(indexInHand, totalCount);

            float currentZ = transform.localEulerAngles.z;
            float targetZ = Is(State.Dragging) ? 0f : _curveRotation;
            
            float t = config.CurveRotateSpeed * .5f * Time.deltaTime;
            float nextZ = Mathf.LerpAngle(currentZ, targetZ, t);
            transform.localEulerAngles = new Vector3(0f, 0f, nextZ);
        }

        /// <summary>
        /// <see cref="LocalZeroPosition"/>으로 보간해 부드럽게 이동함.
        /// </summary>
        private void LerpMoveToSlot()
        {
            if (!LerpToSlot) return;

            var t = config.LerpToSlotSpeed * Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, LocalZeroPosition, t);
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
        /// 값 선택 가능 마커의 표시 여부를 페이드 효과와 적용함. 
        /// </summary>
        public void SetChangeableImageWithFade(bool active)
        {
            changeableMarker.DOKill();
            changeableMarker.DOFade(active ? 1f : 0f, .2f).SetEase(Ease.InCirc);
        }

        /// <summary>
        /// 리롤 중 뽑힐 때 사용됨.
        /// 슬롯(<see cref="LocalZeroPosition"/>)으로 이동 후 플립함.
        /// </summary>
        /// <remarks>Flip과 이동이 모두 완료될 때까지 대기함.</remarks>
        /// <param name="ensureBack">true면 실행 전 뒷면으로 즉시 전환.</param>
        public async UniTask RerollDrawAsync(bool ensureBack = true)
        {
            var flipDuration = rerollConfig.DrawFlipDuration;
            var flipEase = config.FlipEase;
            var moveDuration = rerollConfig.DrawMoveDuration;
            var moveEase = rerollConfig.DrawMoveEase;

            if (ensureBack)
            {
                flip.StopAndFlipInstant(CardFace.Back);
            }
            CardDeckVisual.Instance.OnInteraction();

            await CreateMoveToSlotTween(moveDuration, moveEase);
            await flip.FlipAsync(CardFace.Front, flipDuration, flipEase);
        }

        /// <summary>
        /// 리롤 중 버려질 때 사용됨. 뒷면으로 플립하고 덱으로 이동함.
        /// </summary>
        /// <param name="ensureFront">true면 실행 전 앞면으로 즉시 전환.</param>
        public async UniTask RerollDiscardAsync(bool ensureFront = false)
        {
            var flipDuration = rerollConfig.DiscardFlipDuration;
            var flipEase = config.FlipEase;
            var moveDuration = rerollConfig.DiscardMoveDuration;
            var moveEase = rerollConfig.DiscardMoveEase;
            
            if (ensureFront)
            {
                flip.StopAndFlipInstant(CardFace.Front);   
            }
            await flip.FlipAsync(CardFace.Back, flipDuration, flipEase);
            await CreateMoveToDeckTween(moveDuration, moveEase);
            
            CardDeckVisual.Instance.OnInteraction();
        }

        public async UniTask MoveOnRerollEnd()
        {
            var moveDuration = config.DrawMoveDuration;
            
            await CreateMoveToSlotTween(moveDuration);
        }

        /// <summary>
        /// 일반적으로 뽑힐 때 사용됨.
        /// <see cref="LocalZeroPosition"/>으로 이동하고 앞면으로 플립함.
        /// </summary>
        /// <param name="ensureBack">true면 실행 전 뒷면으로 즉시 전환.</param>
        public async UniTask DrawAsync(bool ensureBack = true)
        {
            var flipDuration = config.FlipDuration;
            var flipEase = config.FlipEase;
            var moveDuration = config.DrawMoveDuration;
            var moveEase = config.DrawMoveEase;
            
            if (ensureBack)
            {
                flip.StopAndFlipInstant(CardFace.Back);
            }
            
            CardDeckVisual.Instance.OnInteraction();

            await CreateMoveToSlotTween(moveDuration, moveEase);
            await flip.FlipAsync(CardFace.Front, flipDuration, flipEase);
        }
        
        public async UniTask FadeOutAsync()
        {
            await CreateFadeOutTween();
        }
        
        /// <returns>
        /// 슬롯(<see cref="LocalZeroPosition"/>)으로 이동하는 트윈.
        /// </returns>
        private Tween CreateMoveToSlotTween(float duration, Ease ease = Ease.Unset)
        {
            Set(State.Tweening, true);
            return transform
                .DOLocalMove(LocalZeroPosition, duration)
                .SetEase(ease)
                .OnComplete(() => Set(State.Tweening, false));
        }
        
        /// <returns>
        /// 덱으로 이동하는 트윈
        /// </returns>
        private Tween CreateMoveToDeckTween(float duration, Ease ease = Ease.Unset)
        {
            Set(State.Tweening, true);
            return transform
                .DOMove(CardDeckVisual.Instance.Front.position, duration)
                .SetEase(ease)
                .OnComplete(() => Set(State.Tweening, false));
        }

        private Tween CreateFadeOutTween(Ease ease = Ease.Unset)
        {
            Set(State.Tweening, true);
            return visual.CanvasGroup.DOFade(0f, .5f)
                .SetEase(ease)
                .OnComplete(() => Set(State.Tweening, false));
        }
    }
}