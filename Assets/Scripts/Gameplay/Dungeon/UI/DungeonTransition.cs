using Cardevil.Core;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.UI;
using Cardevil.UI.Playables;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Cardevil.Gameplay.Dungeon.UI
{
    public class DungeonTransition : MonoBehaviour
    {
        [SerializeField] private Image environmentImage;
        [SerializeField] private RectTransform transitionPanel;
        [SerializeField] private BlackPanel blackPanel;
        [SerializeField] private Image blackPanelImg;
        [SerializeField] private TextMeshProUGUI blackPanelText;
        [SerializeField] private CanvasGroup canvasGroup;
        [Header("PlayableDirector Settings")]
        [field:SerializeField] public PlayableDirector PlayableDirector { get; private set; }
        [field:SerializeField] public SkippableTimelinePlayableDirector SkippableTimelinePlayableDirector{ get; private set; }
        [field:Space]
        [field:Tooltip("월드씬에서 스테이지 씬으로 넘어갈 때 재생되는 타임라인 에셋입니다.")]
        [field:SerializeField] public PlayableAsset TransitionToStagePlayableAsset { get; private set; }
        [field:Tooltip("지도를 손에 드는 애니메이션이 재생되는 타임라인 에셋입니다.")]
        [field:SerializeField] public PlayableAsset HandUpPlayableAsset { get; private set; }
        [field:Obsolete("사용안함.", true)]
        [field:SerializeField] public PlayableAsset DefaultHideTransitionPlayableAsset { get; private set; }
        
        
        private RectTransform _initialEnvironmentImagePosition;
        private RectTransform _initialPanelPosition;
        private RectTransform _initialChapterUIRectTransform;
        [Header("UI Reference")]
        [SerializeField] private RectTransform chapterUIRectTransform;

        private void Awake()
        {
            // _initialEnvironmentImagePosition = Instantiate(environmentImage, transform).rectTransform;
            // _initialPanelPosition = Instantiate(transitionPanel, transform);
            
            // _initialPanelPosition.gameObject.SetActive(false);
            // _initialEnvironmentImagePosition.gameObject.SetActive(false);
            // _initialChapterUIRectTransform 
            
            
            _initialChapterUIRectTransform = Instantiate(chapterUIRectTransform, chapterUIRectTransform.parent);
            _initialChapterUIRectTransform.gameObject.SetActive(false);
            
            ExecEventBus<NodeEnteredEventArgs>.RegisterStatic(-1000, OnNodeEntered);
            ExecEventBus<StageLoopEndEventArgs>.RegisterStatic(-1000, OnStageLoopEnd);
        }

        private UniTask OnStageLoopEnd(StageLoopEndEventArgs eventArgs, CancellationToken cancellationToken)
        {
            chapterUIRectTransform.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        private UniTask OnNodeEntered(NodeEnteredEventArgs eventArgs, CancellationToken cancellationToken)
        {
            blackPanelText.text = $"지하 {eventArgs.Node.Floor}층";
            
            return UniTask.CompletedTask;
        }

        public async UniTask ShowEnterTransition(CancellationToken cancellationToken = default)
        {
            blackPanelText.gameObject.SetActive(true);
            PlayableDirector.playableAsset = TransitionToStagePlayableAsset;
            PlayableDirector.Play();
            var timeline = PlayableDirector.playableAsset as TimelineAsset;

            var durationTask = UniTask.WaitForSeconds((float)PlayableDirector.duration, cancellationToken: cancellationToken);
            var stopTask = UniTask.WaitUntil(() => PlayableDirector.state != PlayState.Playing, cancellationToken: cancellationToken);
            
            PlayerSkipCheckTask(cancellationToken).Forget();
            
            await UniTask.WhenAny(durationTask, stopTask);
        }
        
        public async UniTask ShowHandUpAnimation(CancellationToken cancellationToken = default)
        {
            canvasGroup.alpha = 1;
            PlayableDirector.playableAsset = HandUpPlayableAsset;
            PlayableDirector.Play(HandUpPlayableAsset);
            var durationTask = UniTask.WaitForSeconds((float)PlayableDirector.duration, cancellationToken: cancellationToken);
            var stopTask = UniTask.WaitUntil(() => PlayableDirector.state != PlayState.Playing, cancellationToken: cancellationToken);
            
            // PlayerSkipCheckTask(cancellationToken).Forget();
            
            await UniTask.WhenAny(durationTask, stopTask);
        }
        
        public async UniTask PlayMapHideAndBlackoutAsync(CancellationToken cancellationToken = default)
        {
            blackPanel.gameObject.SetActive(true);
            blackPanelText.gameObject.SetActive(false);
            blackPanelImg.color = new Color(blackPanelImg.color.r, blackPanelImg.color.g, blackPanelImg.color.b, 1f);
            blackPanel.CanvasGroup.alpha = 0f;
            blackPanel.CanvasGroup.interactable = false;
            blackPanel.CanvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;

            var handDownSmallTask = chapterUIRectTransform.DOAnchorPosY(-100, 0.5f).SetEase(Ease.Linear).ToUniTask(cancellationToken: cancellationToken);
            var blackPanelFadeinTask = blackPanel.CanvasGroup.DOFade(1, 0.5f).SetEase(Ease.Linear).ToUniTask(cancellationToken: cancellationToken);
            await UniTask.WhenAll(handDownSmallTask, blackPanelFadeinTask);
            ResetChapterUIVisual();
            canvasGroup.blocksRaycasts = false;
        }

        public void ResetChapterUIVisual()
        {
            chapterUIRectTransform.anchoredPosition = _initialChapterUIRectTransform.anchoredPosition;
            chapterUIRectTransform.localScale = _initialChapterUIRectTransform.localScale;
            chapterUIRectTransform.rotation = _initialChapterUIRectTransform.rotation;
        }

        public void PrepareChapterUIForHandUp()
        {
            chapterUIRectTransform.anchoredPosition = new Vector2(
                _initialChapterUIRectTransform.anchoredPosition.x,
                -100f);
            chapterUIRectTransform.localScale = _initialChapterUIRectTransform.localScale;
            chapterUIRectTransform.rotation = _initialChapterUIRectTransform.rotation;
        }
        

        /// <summary>
        /// 플레이어 입력을 감지하여 타임라인을 스킵하는 비동기 작업.
        /// TODO : 입력 감지를 다른곳에서 일임하도록 리팩토링할 수 있을듯. 일단은 간단하게 여기서 처리.
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async UniTask PlayerSkipCheckTask(CancellationToken cancellationToken = default)
        {
            // 입력 버퍼가 남아 있는거 방지
            await UniTask.DelayFrame(2, cancellationToken: cancellationToken); 
            
            while (PlayableDirector.state == PlayState.Playing)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
                {
                    SkippableTimelinePlayableDirector.SkipToNextMarker();
                }
                await UniTask.Yield(cancellationToken);
            }
        }
    }
}
