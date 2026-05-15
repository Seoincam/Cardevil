using Cardevil.Core.Systems;
using Cardevil.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopView : SpecialSceneView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Shop References")]
        [SerializeField] private Transform shopItemContainer;

        [SerializeField] private List<ShopItem> shopItems;
        [SerializeField] private Button reinforceButton;
        [SerializeField] private Button exitButton;
        public event Action ReinforceRequested;

        ShopCore shopCore;

        public override void Bind(SpecialSceneCore core)
        {
            shopCore = (ShopCore)core;
            
            exitButton.onClick.AddListener(HandleExitClicked);
            
            var shopEntries = shopCore.ShopEntries;
            for (int i = 0; i < shopItems.Count && i < shopEntries.Count; i++)
            {
                TooltipData tooltipData = TooltipResolver.Resolve(shopEntries[i].TooltipKey);
                shopItems[i].Initialize(shopEntries[i], tooltipData);
                shopItems[i].OnItemClicked += () => HandleItemClicked(i);
            }
            
            reinforceButton.onClick.AddListener(HandleReinforceClicked);
        }

        private void OnDestroy()
        {
            if (reinforceButton)
            {
                reinforceButton.onClick.RemoveListener(HandleReinforceClicked);
            }

            if (exitButton)
            {
                exitButton.onClick.RemoveListener(HandleExitClicked);
            }
        }

        private void HandleReinforceClicked()
        {
            ReinforceRequested?.Invoke();
        }

        private void HandleExitClicked()
        {
            RaiseCloseRequested();
        }

        public void SetReinforceInteractable(bool value)
        {
            if (reinforceButton)
            {
                reinforceButton.interactable = value;
            }
        }

        private void HandleItemClicked(int index)
        {
            Debug.Log($"Shop item {index} clicked!");
        }

        public override async UniTask PlayEnterAsync()
        {
            var blackFade = UI.OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.2f)
                .ToUniTask(TweenCancelBehaviour.Complete);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            await UniTask.WhenAll(blackFade, canvasGroup.DOFade(1f, 0.2f).ToUniTask(TweenCancelBehaviour.Complete));
        }

        public override async UniTask PlayExitAsync()
        {
            await canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // #region Legacy
        //
        //
        // [Header("Legacy References")]
  
        // [SerializeField] private Image panelImage;
        // [SerializeField] private TextMeshProUGUI titleText;
        // [SerializeField] private TextMeshProUGUI bodyText;
        // [SerializeField] private Button closeButton;
        // [SerializeField] private TextMeshProUGUI buttonText;
        //
        // private void Awake()
        // {
        //     if (closeButton)
        //     {
        //         closeButton.onClick.AddListener(RaiseCloseRequested);
        //     }
        // }
        //
        // public override void Bind(SpecialSceneCore core)
        // {
        //     titleText.text = core.TestTitle;
        //     bodyText.text = core.TestDescription;
        //     buttonText.text = core.TestConfirmLabel;
        //     panelImage.color = core.TestAccentColor;
        // }
        //
        // public override async UniTask PlayEnterAsync()
        // {
        //     var blackFade = UI.OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.8f)
        //         .ToUniTask(TweenCancelBehaviour.Complete);
        //     canvasGroup.interactable = true;
        //     canvasGroup.blocksRaycasts = true;
        //     await blackFade;
        // }
        // #endregion
    }
}
