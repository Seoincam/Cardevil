using Cardevil.Core;
using Cardevil.Core.SceneManagement;
using Cardevil.Gameplay.Enemy;
using Cardevil.UI;
using Cardevil.UI.GlobalNavigationBar;
using Cardevil.UI.PopUp;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Globalization;
using System.Text;
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
            roomMobInfoPopup.gameObject.SetActive(false);
        }

        public async UniTask PlayEnterStageAnimationAsync(EnemySpawner enemySpawner)
        {
            // @machamy가 작성.
            // 페이드아웃 + 돌 가져오기
            CameraManager.Instance.DisableSceneCameras(Scenes.World);

            string BuildAttackPatternString(Database.Generated.BaseMobBossData mobRawInfo)
            {
                if (mobRawInfo?.AttackPattern == null || mobRawInfo.AttackPattern.Count == 0)
                {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < mobRawInfo.AttackPattern.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(mobRawInfo.AttackPattern[i]);
                }

                return builder.ToString();
            }

            string BuildGimmickString(Database.Generated.BaseMobBossData mobRawInfo)
            {
                if (mobRawInfo?.GimmickName == null || mobRawInfo.GimmickName.Count == 0)
                {
                    return "없음";
                }

                var gimmickValues = mobRawInfo.GimmickValue;
                int valueCount = gimmickValues?.Count ?? 0;
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < mobRawInfo.GimmickName.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    string gimmickName = mobRawInfo.GimmickName[i] ?? string.Empty;
                    string gimmickValue = "?";
                    if (gimmickValues != null && i < valueCount)
                    {
                        gimmickValue = gimmickValues[i].ToString(CultureInfo.InvariantCulture);
                    }

                    builder.Append(gimmickName);
                    builder.Append('(');
                    builder.Append(gimmickValue);
                    builder.Append(')');
                }

                return builder.ToString();
            }

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
            
            // int testMobCount = RandomUtil.GetRandomInt(1, 5);
            roomMobInfoPopup.ClearMobInfos();
            for (int i = 0; i < enemySpawner._baseMobBossDatas.Count; i++)
            {
                var mobRawInfo = enemySpawner._baseMobBossDatas[i];
                string rankString = BuildAttackPatternString(mobRawInfo);
                string gimmickString = BuildGimmickString(mobRawInfo);

                string mobName = mobRawInfo?.MobKorID ?? string.Empty;
                string description = $"BaseHP: {mobRawInfo?.BaseHP ?? 0} / 공격주기: {mobRawInfo?.AttackCycle ?? 0} / 공격 데미지: {mobRawInfo?.AttackDamage ?? 0} " +
                                     $"/ 기믹: {gimmickString}";
                string additionalInfo = $"뭘넣음?";
                var mobInfo = new MobInfo(mobName, description, rankString, additionalInfo);
                roomMobInfoPopup.AddMobInfo(mobInfo);
            }
            
            //
            // roomMobInfoPopup.AddMobInfo(MobInfo.Test);
            // for (int i = 1; i < testMobCount; i++)
            // {
            //     roomMobInfoPopup.AddMobInfo(new MobInfo($"적 {i}", "이것은 테스트용 적입니다.", "테스트 족보", $"{i*2}"));
            // }
            
            roomMobInfoPopup.SetCompleteSource(completeSource);
            
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
