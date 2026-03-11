using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.UI.Playables;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Cardevil.Dungeon.UI
{
    public class DungeonTransition : MonoBehaviour
    {
        [SerializeField] private Image environmentImage;
        [SerializeField] private RectTransform transitionPanel;
        [SerializeField] private Image blackPanel;
        [SerializeField] private TextMeshProUGUI blackPanelText;
        [field:SerializeField] public PlayableDirector PlayableDirector { get; private set; }
        [field:SerializeField] public SkippableTimelinePlayableDirector SkippableTimelinePlayableDirector{ get; private set; }
        
        private RectTransform _initialEnvironmentImagePosition;
        private RectTransform _initialPanelPosition;
        // [Space(2f)]
        // [Header("Legacy Settings (Unused)")]
        // [Header("Animation Settings")]
        // [Header("Phase 1: GNB Hide & Environment Zoom")]
        // [SerializeField] private float gnbHideDuration = 0.5f;
        // [SerializeField] private float zoomDuration = 0.5f;
        // [SerializeField] private float environmentZoomScale = 1.2f;
        // [Header("Phase 2: Panel Slide In")]
        // [SerializeField] private float panelSlideDuration = 0.5f;
        //
        // [SerializeField] private Ease panelSlideEase = Ease.InOutSine;
        // [Header("Phase 3: Pause")]
        // [SerializeField] private float pauseDuration = 0.5f;
        // [Header("Phase 4: Panel & Environment Zoom")]
        // [SerializeField] private float finalZoomDuration = 0.5f;
        // [SerializeField] private float finalPanelZoomScale = 100f;
        // [SerializeField] private float finalEnvironmentZoomScale = 1.5f;
        // [SerializeField] private Ease finalZoomEase = Ease.InOutSine;
        // [SerializeField] private Ease finalPanelEase = Ease.InOutSine;

        private void Awake()
        {
            // _initialEnvironmentImagePosition = Instantiate(environmentImage, transform).rectTransform;
            // _initialPanelPosition = Instantiate(transitionPanel, transform);
            
            // _initialPanelPosition.gameObject.SetActive(false);
            // _initialEnvironmentImagePosition.gameObject.SetActive(false);
            
            ExecEventBus<NodeEnteredEventArgs>.RegisterStatic(-1000, OnNodeEntered);
        }

        private UniTask OnNodeEntered(NodeEnteredEventArgs eventArgs, CancellationToken cancellationToken)
        {
            blackPanelText.text = $"지하 {eventArgs.Node.Floor}층";
            
            return UniTask.CompletedTask;
        }

        public async UniTask ShowTransition(CancellationToken cancellationToken = default)
        {
            PlayableDirector.Play();
            var timeline = PlayableDirector.playableAsset as TimelineAsset;

            var durationTask = UniTask.WaitForSeconds((float)PlayableDirector.duration, cancellationToken: cancellationToken);
            var stopTask = UniTask.WaitUntil(() => PlayableDirector.state != PlayState.Playing, cancellationToken: cancellationToken);
            
            PlayerSkipCheckTask(cancellationToken).Forget();
            
            await UniTask.WhenAny(durationTask, stopTask);
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