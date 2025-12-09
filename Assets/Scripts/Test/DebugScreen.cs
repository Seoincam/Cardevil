using Cardevil.Events;
using Cardevil.Events.ExecEvents;
using TMPro;
using UnityEngine;

namespace Cardevil.Test
{
    /// <summary>
    /// 디버그 화면을 표시하는 UI 컴포넌트
    /// 메인캔버스가 아닌 디버그용 캔버스에 표시됨
    /// </summary>
    public class DebugScreen : UI_Base
    {
        public enum TextNames
        {
            PlayerHealth,
        }


        private void Start()
        {
            Init();
        }

        public override void Init()
        {
            Bind<TextMeshProUGUI>(typeof(TextNames));
        }

        private void OnEnable()
        {
            // 이벤트 리스너 등록
            ExecEventBus<PlayerHealthChangeArgs>.RegisterDynamic(OnPlayerHealthChanged);
        }
        
        private void OnDisable()
        {
            // 이벤트 리스너 제거
            ExecEventBus<PlayerHealthChangeArgs>.UnregisterDynamic(OnPlayerHealthChanged);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                gameObject.SetActive(false);
            }
        }


        private void OnPlayerHealthChanged(ExecQueue<PlayerHealthChangeArgs> queue, PlayerHealthChangeArgs args)
        {
            // 플레이어의 체력이 변경되었을 때 디버그 화면에 표시
            var playerHealthText = Get<TextMeshProUGUI>(TextNames.PlayerHealth);
            if (playerHealthText != null)
            {
                playerHealthText.text = $"hp: {args.NewHealth}";
            }
        }
    }
}