using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Cardevil.Systems
{
    public enum GameState
    {
        Pause, PlayerInput, Action
    }

    public class TurnManager : Singleton<TurnManager>, IPlayerInputHandler, IPlayerActionHandler, IBossActionHandler
    {
        [Header("Game State")]
        public GameState gameState = GameState.Pause;

        [Header("References")]
        public Button useCardButton;
        public Slider playerActionProgressBar;
        public Slider bossActionProgressBar;

        // [Header("Events")]
        public delegate Task ActionAsync();
        public event ActionAsync PlayerInputAsync;
        public event ActionAsync PlayerActionAsync;
        public event ActionAsync BossActionAsync;

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;

            // 이벤트 구독
            SubscribePlayerInput();
            SubscribePlayerAction();
            SubscribeBossAction();
        }

        async void Start()
        {
            await GameLoopAsync();
        }

        private async Task GameLoopAsync()
        {
            while (true)
            {
                if (PlayerActionAsync == null || PlayerActionAsync == null || BossActionAsync == null)
                    continue;

                await PlayerInputAsync.Invoke();
                // -> 사용한 카드에 따라 효과, 데미지 등을 계산

                await PlayerActionAsync.Invoke();
                await Task.Delay(500);
                // -> 플레이어 액션 후 보스 상태 체크

                await BossActionAsync.Invoke();
                await Task.Delay(500);
                // -> 보스 액션 후 플레이어 상태 체크
            }
            // TODO: 추후 break 로직 추가
        }


        // IPlayerInputHandler
        private TaskCompletionSource<bool> playerTcs;

        public async Task HandlePlayerInputAsync()
        {
            useCardButton.interactable = true;

            playerTcs = new();
            await playerTcs.Task;
        }

        public void SubscribePlayerInput()
        {
            PlayerInputAsync += HandlePlayerInputAsync;
        }

        // IPlayerActionHandler
        public async Task HandlePlayerActionAsync()
        {
            // 애니메이션 등 실행
            // (임시로 2.5초 대기)
            SetProgressBar(playerActionProgressBar, value: 1f);
            var duration = 2.5f;
            var startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(playerActionProgressBar, value: remaining / duration);

                await Task.Yield();
            }
        }

        public void SubscribePlayerAction()
        {
            PlayerActionAsync += HandlePlayerActionAsync;
        }

        // IBossActionHandler
        public async Task HandleBossActionAsync()
        {
            // 애니메이션 등 실행
            // (임시로 2초 대기)
            SetProgressBar(bossActionProgressBar, value: 1f);
            var duration = 2f;
            var startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(bossActionProgressBar, value: remaining / duration);

                await Task.Yield();
            }
        }

        public void SubscribeBossAction()
        {
            BossActionAsync += HandleBossActionAsync;
        }


        public void OnPlayerUseCard()
        {
            useCardButton.interactable = false;

            if (playerTcs != null
                && !playerTcs.Task.IsCompleted)
            {
                playerTcs.TrySetResult(true);
            }
        }

        private void SetProgressBar(Slider progressBar, float value)
        {
            progressBar.value = value;
        }
    }
}
