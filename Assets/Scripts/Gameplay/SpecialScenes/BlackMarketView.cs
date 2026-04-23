using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class BlackMarketView : SpecialSceneView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image panelImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        private void Awake()
        {
            if (closeButton)
            {
                closeButton.onClick.AddListener(RaiseCloseRequested);
            }
        }

        public override void Bind(SpecialSceneCore core)
        {
            titleText.text = core.Title;
            bodyText.text = core.Description;
            buttonText.text = core.ConfirmLabel;
            panelImage.color = core.AccentColor;
        }

        public override async UniTask PlayEnterAsync()
        {
            var blackFade = UI.OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.8f)
                .ToUniTask(TweenCancelBehaviour.Complete);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            await blackFade;
        }

        public override async UniTask PlayExitAsync()
        {
            await canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
