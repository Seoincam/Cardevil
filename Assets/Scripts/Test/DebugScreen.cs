using Cardevil.Events;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            CardDeckCount
        }

        public enum TurnDebugSliderNames
        {
            PlayerActionProgressBar,
            BossActionProgressBar
        }
        public enum TurnDebugTextNames
        {
            InputText,
            DamageText,
        }


        private void Start()
        {
            Init();
        }

        public override void Init()
        {
            Bind<TextMeshProUGUI>(typeof(TextNames));
            Bind<Slider>(typeof(TurnDebugSliderNames));
            Bind<Text>(typeof(TurnDebugTextNames));
        }

        private void OnEnable()
        {
            // 이벤트 리스너 등록
            Managers.Event.PlayerHealthChangeEvent.AddListener(OnPlayerHealthChanged);
            Managers.Event.RemainingCardChangeEvent.AddListener(OnRemainingCardChanged, 0);
        }
        private void OnDisable()
        {
            // 이벤트 리스너 제거
            Managers.Event.PlayerHealthChangeEvent.RemoveListener(OnPlayerHealthChanged);
            Managers.Event.RemainingCardChangeEvent.RemoveListener(OnRemainingCardChanged, 0);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                gameObject.SetActive(false);
            }
        }


        private void OnPlayerHealthChanged(PlayerHealthChangeArgs args)
        {
            // 플레이어의 체력이 변경되었을 때 디버그 화면에 표시
            var playerHealthText = Get<TextMeshProUGUI>(TextNames.PlayerHealth);
            if (playerHealthText != null)
            {
                playerHealthText.text = $"hp: {args.NewHealth}";
            }
        }

        private void OnRemainingCardChanged(RemainingCardChangeArgs args)
        {
            // 남은 카드 수가 변경됐을 때 디버그 화면에 표시
            var cardDeckCounter = Get<TextMeshProUGUI>(TextNames.CardDeckCount);
            if (cardDeckCounter != null)
                cardDeckCounter.text = args.RemainingCardCount.ToString();
        }

        // event 사용 안하고 임시로 구현
        public void SetTurnDebugProgressBar(TurnDebugSliderNames sliderNames, float value)
        {
            var bar = Get<Slider>(sliderNames);
            if (bar != null)
                bar.value = value;
        }

        public void SetTurnDebugTextBar(TurnDebugTextNames textNames, string value)
        {
            var text = Get<Text>(textNames);
            if (text != null)
                text.text = value;
        }
    }
}