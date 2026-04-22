using Cardevil.Core;
using Cardevil.Core.SceneManagement;
using Cardevil.UI;
using Cardevil.UI.GlobalNavigationBar;
using Cardevil.UI.PopUp;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace Cardevil.Gameplay.Root
{
    /// <summary>
    /// 전투 스테이지 UI를 다루는 클래스.
    /// </summary>
    public class StageView : MonoBehaviour
    {
        [SerializeField] private float dimFadeDuration;
        [SerializeField] private CanvasGroup dimCanvasGroup;
        [SerializeField] private RoomMobInfoPopup roomMobInfoPopup;

        private void Awake()
        {
            dimCanvasGroup.blocksRaycasts = false;
            roomMobInfoPopup.Init();
            roomMobInfoPopup.gameObject.SetActive(false);
        }

        public async UniTask PlayEnterStageAnimationAsync()
        {
            // @machamy가 작성.
            
            // 페이드아웃 + 돌 가져오기
            CameraManager.Instance.DisableSceneCameras(Scenes.World);

            var blackFade = OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.8f)
                .ToUniTask(TweenCancelBehaviour.Complete);

            var rock = StageCameraCanvas.Instance.AnimShowRock(0.8f);
            
            await UniTask.WhenAll(blackFade, rock);
            
            // TODO : 가운데에 적 정보 보이기
            // 지금은 버튼 만들어서 누르면 넘어가도록 해둠.
            
            var completeSource = new UniTaskCompletionSource();

            #region Legacy Code for Confirm Button (To be removed)

            // StageCameraCanvas.Instance.OnCompleteShowRock += () => completeSource.TrySetResult();
            // var obj = new GameObject("ConfirmButton");
            // var rect = obj.AddComponent<RectTransform>();
            // var button = obj.AddComponent<UnityEngine.UI.Button>();
            // var Canvas = GameObject.Find("Card Canvas");
            // var Image = obj.AddComponent<UnityEngine.UI.Image>();
            //
            // Image.color = Color.white;
            // rect.sizeDelta = new Vector2(200, 100);
            // button.image = Image;
            // var textObj = new GameObject("Text");
            // var textRect = textObj.AddComponent<RectTransform>();
            // var text = textObj.AddComponent<TextMeshProUGUI>();
            // text.text = "Confirm";
            // text.color = Color.yellow;
            // textRect.parent = rect;
            // textRect.anchoredPosition = Vector2.zero;

            // button.transform.SetParent(StageCameraCanvas.Instance.transform, false);
            // obj.transform.SetParent(Canvas.transform, false);
            // rect.anchoredPosition = Vector2.zero;
            // button.onClick.AddListener(() => completeSource.TrySetResult());
            // button.onClick.AddListener(() => Destroy(obj, 0.1f));
            #endregion

            roomMobInfoPopup.SetMobInfo(MobInfo.Test);// TODO : 실제로는 방에 맞는 몹 정보로 설정해야 함.
            
            roomMobInfoPopup.SetCompleteSource(completeSource);
            roomMobInfoPopup.gameObject.SetActive(true);
            await roomMobInfoPopup.ShowAsync();
            

            
            
            await completeSource.Task;
            


            await UniTask.Delay(200);
            
            // GNB 보이기
            GlobalNavigationBar gnb = GlobalNavigationBar.Instance;
            gnb.gameObject.SetActive(true);
            await gnb.ShowAsync(0.4f);
        }

        public async UniTask PlayShowDimAsync()
        {
            dimCanvasGroup.blocksRaycasts = true;
            await dimCanvasGroup.DOFade(1f, dimFadeDuration);
        }

        public async UniTask PlayHideDimAsync()
        {
            await dimCanvasGroup.DOFade(0f, dimFadeDuration);
            dimCanvasGroup.blocksRaycasts = false;
        }
    }
}