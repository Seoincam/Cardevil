using Cardevil.Core.Bootstrap;
using Cardevil.Events.ExecEvents;
using Cardevil.UI.GlobalNavationBar;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
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
            var durationTask = UniTask.WaitForSeconds((float)PlayableDirector.duration, cancellationToken: cancellationToken);
            var stopTask = UniTask.WaitUntil(() => PlayableDirector.state != PlayState.Playing, cancellationToken: cancellationToken);
            await UniTask.WhenAny(durationTask, stopTask);
        }
    }
}