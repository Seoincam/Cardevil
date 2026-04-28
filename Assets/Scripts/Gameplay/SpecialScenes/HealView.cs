using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class HealView : SpecialSceneView
    {
        public event Action GoldChoiceSelected;
        public event Action HealChoiceSelected;

        [SerializeField] private CanvasGroup canvasGroup;
        [Header("Animation Settings")]
        [SerializeField] private float enterFadeDuration = 0.8f;
        [SerializeField] private PlayableDirector director;
        
        [Header("References")]
        [SerializeField] private HealChoice goldChoice;
        [SerializeField] private HealChoice healChoice;
        // [SerializeField] private HealChoice defaultSelectedChoice;

        [Header("Legacy References")]
        [SerializeField] private Image panelImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        private bool _isResolvingChoice;

        private void Awake()
        {
            if (closeButton)
            {
                closeButton.onClick.AddListener(RaiseCloseRequested);
            }
            
            if (goldChoice)
            {
                goldChoice.Apply += HandleGoldChoiceApplied;
                goldChoice.ApplyAnimationCompleted += HandleChoiceAnimationCompleted;
            }

            if (healChoice)
            {
                healChoice.Apply += HandleHealChoiceApplied;
                healChoice.ApplyAnimationCompleted += HandleChoiceAnimationCompleted;
            }
        }

        private void OnDestroy()
        {
            if (closeButton)
            {
                closeButton.onClick.RemoveListener(RaiseCloseRequested);
            }

            if (goldChoice)
            {
                goldChoice.Apply -= HandleGoldChoiceApplied;
                goldChoice.ApplyAnimationCompleted -= HandleChoiceAnimationCompleted;
            }

            if (healChoice)
            {
                healChoice.Apply -= HandleHealChoiceApplied;
                healChoice.ApplyAnimationCompleted -= HandleChoiceAnimationCompleted;
            }
        }

        public override void Bind(SpecialSceneCore core)
        {
            _isResolvingChoice = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            if (closeButton)
            {
                closeButton.interactable = true;
            }

            titleText.text = core.TestTitle;
            bodyText.text = core.TestDescription;
            buttonText.text = core.TestConfirmLabel;
            panelImage.color = core.TestAccentColor;

            if (core is not HealCore healCore)
            {
                Debug.LogError($"Expected {nameof(HealCore)} but received {core?.GetType().Name ?? "null"}.");
                return;
            }

            goldChoice.SetGoldChoice(healCore.GoldAmount);
            healChoice.SetHealChoice(healCore.MinHealAmount, healCore.MaxHealAmount, healCore.SelectedHealIndex);
        }

        public override async UniTask PlayEnterAsync()
        {
            var blackFade = UI.OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, enterFadeDuration)
                .ToUniTask(TweenCancelBehaviour.Complete);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            await blackFade;
            director?.Play();
            canvasGroup.interactable = true;
            // FocusDefaultChoice();
        }

        public override async UniTask PlayExitAsync()
        {
            await canvasGroup.DOFade(0f, 0.4f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        private void HandleGoldChoiceApplied()
        {
            if (_isResolvingChoice)
            {
                return;
            }

            BeginChoiceResolution();
            GoldChoiceSelected?.Invoke();
        }
        
        private void HandleHealChoiceApplied()
        {
            if (_isResolvingChoice)
            {
                return;
            }

            BeginChoiceResolution();
            HealChoiceSelected?.Invoke();
        }

        private void HandleChoiceAnimationCompleted()
        {
            RaiseCloseRequested();
        }

        private void BeginChoiceResolution()
        {
            _isResolvingChoice = true;
            canvasGroup.interactable = false;
        }

        // private void FocusDefaultChoice()
        // {
        //     var targetChoice = defaultSelectedChoice;
        //     if (!targetChoice && choices != null && choices.Length > 0)
        //     {
        //         targetChoice = choices[0];
        //     }
        //
        //     if (!targetChoice || EventSystem.current == null)
        //     {
        //         return;
        //     }
        //
        //     EventSystem.current.SetSelectedGameObject(null);
        //     targetChoice.Select();
        // }
    }
}
