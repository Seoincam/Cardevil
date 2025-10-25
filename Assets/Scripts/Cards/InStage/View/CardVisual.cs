using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.ScriptableObjects;

namespace Cardevil.Cards.InStage.View
{
    [RequireComponent(typeof(Canvas), typeof(Poolable))]
    public class CardVisual : MonoBehaviour, IEvaluateVisual, IClearable
    {
        [Header("Card")]
        [SerializeField, VisibleOnly] private Card parentCard;
        
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;

        [Header("Card Visual")]
        [SerializeField] private Transform shakeObject;
        [SerializeField] private Image frontImage;
        [SerializeField] private Image backImage;
        [SerializeField] private Image[] numberImages;

        [Header("Shadow Visual")]
        [SerializeField] private Transform shadowTransform;

        private Poolable _poolable;
        private Canvas _canvas;
        private CardDeckVisual _deckVisual;

        private IReadOnlyStageCardsModel _model;
        private VisualTransformDelta _delta;
        private VisualState _state;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _delta.shadowOriginPosition = shadowTransform.localPosition;

            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
        }

        public void Clear()
        {
            RemoveAllEvents();

            frontImage.rectTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            backImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            shakeObject.localEulerAngles = Vector3.zero;

            _canvas.overrideSorting = false;
            _state.isDiscarded = false;
            _state.isInitialized = false;
        }

        private void RemoveAllEvents()
        {
            if (parentCard)
            {
                UnsubscribeFromParent(parentCard);
                parentCard = null;
            }

            if (_model != null)
            {
                // _model.HandChanged -= UpdateIndex;
            }

            parentCard = null;
            _model = null;
        }

        public void Init(Card parentCard, CardVisualSpriteSet visualSpriteSet, IReadOnlyStageCardsModel model)
        {
            if (_state.isInitialized) return;
            
            this.parentCard = parentCard;
            _model = model;
            
            // Subscribe Events
            SubscribeToParent(parentCard);
            // _model.HandChanged += UpdateIndex;
            
            _canvas.overrideSorting = false; // @PoolableRoot로 갈 때 자동으로 overrideSorting = true가 됨.
            // UpdateVisual();
            frontImage.sprite = visualSpriteSet.FrontBackgroundImage;
            numberImages[0].sprite = visualSpriteSet.FrontNumberImage;
            numberImages[0].gameObject.SetActive(visualSpriteSet.FrontNumberImage);
            
            var deckVisuals = FindObjectsByType<CardDeckVisual>(FindObjectsSortMode.None);
            if (deckVisuals == null || deckVisuals.Length == 0) { LogEx.LogError("씬 내에 Deck Visual이 존재하지 않음!"); return; }
            _deckVisual = deckVisuals[0];

            transform.position = _deckVisual.Front.position;

            _state.isInitialized = true;
        }

        private void Update()
        {
            if (!_state.isInitialized || !parentCard || _state.isDiscarded)
                return;

            SmoothFollowPosition();
            SmoothFollowRotation();
            CurveOffsetsForSlot();
            ApplyCurveTilt();
        }
        
        #region Update

        /// <summary>
        /// 카드 비주얼의 월드 위치를 부모 카드 위치로 부드럽게 보간해 따라감.
        /// 드래그 중에는 곡선 수직 오프셋을 적용하지 않음.
        /// 리롤 상태나 드로우 연출 중일 때는 부모 카드를 따라가지 않음.
        /// </summary>
        private void SmoothFollowPosition()
        {
            if (parentCard.IsReroll)
                return;
            // if (_state.isDrawing)
            //     return;

            var verticalOffset = Vector3.up * (parentCard.IsDragging ? 0 : _delta.curveYOffset);
            transform.position = Vector3.Lerp(transform.position, parentCard.transform.position + verticalOffset, t: visualSetting.FollowSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 본체 카드와의 상대 이동 벡터를 이용해 회전값을 부드럽게 보간.
        /// 드래그 중에는 이동 누적값 기반으로 더 안정적인 회전 제공.
        /// </summary>
        private void SmoothFollowRotation()
        {
            Vector3 movement = transform.position - parentCard.transform.position;
            
            // 이동 델타 보간 (드래그 중 떨림 완화)
            _delta.movementDelta = Vector3.Lerp(_delta.movementDelta, movement, 20 * Time.deltaTime);
            
            // 드래그 중에는 누적 델타, 그 외엔 즉시 델타 사용
            Vector3 source = parentCard.IsDragging ? _delta.movementDelta : movement;
            Vector3 targetRotation = source * visualSetting.RotationAmount;
            _delta.rotationDelta = Vector3.Lerp(_delta.rotationDelta, targetRotation, visualSetting.RotationSpeed * Time.deltaTime);
            
            // 과도한 Z 틸트 방지
            float z = Mathf.Clamp(_delta.rotationDelta.x, -50f, 50f);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
        }
        
        /// <summary>
        /// 현재 슬롯 인덱스(<c>_slotIndex</c>)를 기반으로
        /// 곡선 배치용 수직 오프셋과 회전 오프셋을 계산해 저장.
        /// 리롤 중엔 생략.
        /// </summary>
        private void CurveOffsetsForSlot()
        {
            if (parentCard.IsReroll) return;
            
            var c = visualSetting.Curve;

            int total = Mathf.Max(1, _model.Hand.Count - 1); // 0 나눗셈 방지
            float factor = (float)_state.handIndex / total;
            
            _delta.curveYOffset = c.positioning.Evaluate(factor) * c.positioningInfluence * total;
            _delta.curveRotationOffset = c.rotation.Evaluate(factor);
        }

        /// <summary>
        /// 곡선 회전 오프셋을 바탕으로 손패에서 카드의 시각적 틸트를 적용.
        /// 드래그 중에는 틸트를 0으로 하여 가독성을 유지.
        /// 리롤/버리기 상태에서는 틸트 적용을 생략.
        /// </summary>
        private void ApplyCurveTilt()
        {
            if (_state.isDiscarded) return;
            if (parentCard.IsReroll) return;

            var c = visualSetting.Curve;
            
            float currentZ = shakeObject.localEulerAngles.z;
            float targetZ = parentCard.IsDragging
                ? 0f
                : (_delta.curveRotationOffset * (c.rotationInfluence * _model.Hand.Count));
            
            // current -> target으로 보간
            float t = (visualSetting.TiltSpeed * 0.5f) * Time.deltaTime;
            float nextZ = Mathf.LerpAngle(currentZ, targetZ, t);

            shakeObject.localEulerAngles = new Vector3(0f, 0f, nextZ);
        }
        
        #endregion
        
        #region Subscribe

        private void SubscribeToParent(Card p)
        {
            if (!p) return;

            p.PointerDown += OnPointerDown;
            p.PointerUp += OnPointerUp;
            p.DragStarted += OnBeginDrag;
            p.DragEnded += OnEndDrag;
            p.ValueSelectionStarted += OnSelectStarted;
            p.ValueSelectionEnded += OnSelectEnded;

            p.RerollDrawn += OnRerollDraw;
            p.RerollDiscarded += OnRerollDiscard;
            p.RerollEnded += OnRerollEnd;
            p.Drawn += OnDraw;
            p.Discarded += OnDiscard;
        }

        private void UnsubscribeFromParent(Card p)
        {
            if (!p) return;

            p.PointerDown -= OnPointerDown;
            p.PointerUp -= OnPointerUp;
            p.DragStarted -= OnBeginDrag;
            p.DragEnded -= OnEndDrag;
            p.ValueSelectionStarted -= OnSelectStarted;
            p.ValueSelectionEnded -= OnSelectEnded;

            p.RerollDrawn -= OnRerollDraw;
            p.RerollDiscarded -= OnRerollDiscard;
            p.RerollEnded -= OnRerollEnd;
            p.Drawn -= OnDraw;
            p.Discarded -= OnDiscard;
        }

        #endregion

        #region Pointer Event

        private void OnPointerDown(Card _, CardPointerArgs args)
        {
            transform.DOScale(endValue: visualSetting.SelectScale, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition += -Vector3.up * visualSetting.ShadowOffset;
        }

        private void OnPointerUp(Card _, CardPointerArgs args)
        {
            transform.DOScale(endValue: 1f, duration: visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);

            shadowTransform.localPosition = _delta.shadowOriginPosition;
        }

        private void OnBeginDrag()
        {
            _canvas.overrideSorting = true;
        }

        private void OnEndDrag()
        {
            _canvas.overrideSorting = false;
        }

        #endregion

        #region Draw/Discard Event

        private void OnRerollDraw()
        {
            _deckVisual.OnInteraction();
            transform.DOMove(endValue: parentCard.transform.position, visualSetting.RerollDrawDuration)
                        .SetEase(visualSetting.RerollDrawEase);

            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipBackImageRatio)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.RerollFlipDuration * visualSetting.RerollFlipFrontImageRation)
                        .SetEase(visualSetting.FlipEase));
        }

        private void OnRerollDiscard(Transform discardPoint)
        {
            _state.isDiscarded = true;
            RemoveAllEvents();

            var tween = transform.DOMove(endValue: discardPoint.position, visualSetting.RerollDiscardDuration)
                        .SetEase(visualSetting.RerollDiscardEase);

            var sequence = DOTween.Sequence();
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.RerollFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.RerollFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));

            tween.OnComplete(() =>
            {
                _deckVisual.OnInteraction();
                Destroy();
            });
        }

        private void OnRerollEnd()
        {
            _canvas.overrideSorting = false;
        }

        private void OnDraw()
        {
            _deckVisual.OnInteraction();

            var sequence = DOTween.Sequence();
            sequence.Append(backImage.transform.DOLocalRotate(new Vector3(0, 90, 0), visualSetting.DrawFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));
            sequence.Append(frontImage.transform.DOLocalRotate(new Vector3(0, 0, 0), visualSetting.DrawFlipDuration * .5f)
                        .SetEase(visualSetting.FlipEase));
        }

        private void OnDiscard()
        {
            _state.isDiscarded = true;
            _canvas.overrideSorting = true;
            RemoveAllEvents();

            var tween = transform.DOLocalMove(endValue: new Vector3(1050, 0, 0), visualSetting.DiscardDuration)
                        .SetEase(visualSetting.DiscardEase);
            tween.OnComplete(Destroy);
        }

        #endregion

        public void UpdateVisualIndex()
        {
            if (_model.TryGetIndex(parentCard, out int index))
            {
                _state.handIndex = index;
                // TODO: 현재 SetSiblingIndex을 업데이트 할 때 마지막 visual이 제대로 업데이트 되지 않는 문제가 발생.
                // 마지막 visual일 시 더 큰 값으로 업데이트하는 등의 수정이 필요할 듯
                transform.SetSiblingIndex(index);
            }
        }
        
        private void Destroy()
        {
            DOTween.Kill(transform);
            Managers.Resource.Destroy(gameObject);
        }

        private void OnSelectStarted(Card _)
        {
            _canvas.overrideSorting = true;
        }

        private void OnSelectEnded(Card _)
        {
            _canvas.overrideSorting = false;
            // UpdateVisual();
            // TODO: 값 선택 후 다시 visual sprite set 생성. Card가 생성 후 넘겨줌.
        }
        

        public void ExecuteEvaluationAction()
        {
            transform.DOShakePosition(.6f, 50);
        }

        #region nested

        private struct VisualTransformDelta
        {
            public Vector3 movementDelta;
            public Vector3 rotationDelta;
            public float curveYOffset;
            public float curveRotationOffset;
            
            public Vector2 shadowOriginPosition;
        }
        
        [Serializable]
        private struct VisualState
        {
            public int handIndex;
            public bool isInitialized;
            public bool isDiscarded;
        }

        #endregion
    }
}