using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Cardevil.Systems
{
    public enum GameState
    {
        Pause, PlayerInput, Action
    }

    public class TurnManager : Singleton<TurnManager>, IPlayerInputReceiver, IPlayerDamageReceiver, IPlayerActionHandler, IBossDamageReceiver, IBossActionHandler
    {
        [Header("Game State")]
        public GameState gameState = GameState.Pause;
        public bool isPlayerInputReceived;

        [Header("References")]
        public Slider playerActionProgressBar;
        public Text playerActionText;

        public Slider bossActionProgressBar;
        public Text bossActionText;

        // [Header("Events")]
        public delegate UniTask TurnStepAsync();
        public event TurnStepAsync PlayerInputAsync;
        public event TurnStepAsync PlayerActionAsync;
        public event TurnStepAsync BossActionAsync;

        public event Action OnGameStateChanged;

        [Header("interfaces")]
        public IPlayerInputHandler playerInputHandler;
        public IPlayerActionHandler playerActionHandler;


        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            playerActionHandler = this;

            // 이벤트 구독
            SubscribePlayerAction();
            SubscribePlayerInput();

            SubscribeBossAction();
            SubscribePlayerDamage();

            // 시작
            GameLoopAsync().Forget();
            SetGameState(GameState.Action);
        }

        /*
            추후 각 인터페이스들을 다른 클래스에서 구현,
            TurnManager에 이벤트 구독 방식으로 실행
        */


        private async UniTaskVoid GameLoopAsync()
        {
            while (true)
            {
                if (PlayerInputAsync == null || PlayerActionAsync == null || BossActionAsync == null)
                {
                    await UniTask.Yield();
                    continue;
                }

                // Boss Action
                await BossActionAsync.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                //  -> 플레이어 상태 체크

                // Player Input
                await PlayerInputAsync.Invoke();
                //  -> 데미지, 이동, 효과 등 계산
                
                // Player Action
                await PlayerActionAsync.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                //  -> 보스 상태 체크
            }

            // TODO: cancel, break 로직 추가
        }



        #region Player Action

        // IPlayerInputReceiver 
        private int playerDealtDamage;

        public void SubscribePlayerInput()
        {
            playerInputHandler.OnPlayerInputReceived += ReceiveInput;
        }

        public void ReceiveInput(CardResult result)
        {
            var moveText = result.moves?.Length > 0
                ? string.Join(", ", result.moves.Select(d => d switch
                {
                    CardDirection.Up => "상",
                    CardDirection.Down => "하",
                    CardDirection.Left => "좌",
                    CardDirection.Right => "우",
                    _ => "?"
                }))
                : "없음";

            playerActionText.text = $@"콤보: {result.combo}
            데미지: {result.damage}
            이동: {moveText}
            ";

            playerDealtDamage = result.damage;
        }


        // IPlayerDamageReceiver
        // 필드 상 플레이어 위치 기반으로 수정

        // IPlayerActionHandler
        public event Action<int> OnPlayerDamageDealt;

        public void SubscribePlayerAction()
        {
            PlayerActionAsync += HandlePlayerActionAsync;
        }

        public async UniTask HandlePlayerActionAsync()
        {
            SetGameState(GameState.Action);

            // 애니메이션 등 실행 (임시로 대기)
            await UpdateProgressBarAsync(playerActionProgressBar, duration: 2f);
            OnPlayerDamageDealt?.Invoke(playerDealtDamage);

            playerActionText.text = ". . .";
        }

        #endregion



        #region Boss Action

        // IBossDamageReceiver
        public void SubscribePlayerDamage()
        {
            playerActionHandler.OnPlayerDamageDealt += ReceivePlayerDamage;
        }

        public void UnsubscribePlayerDamage() { }

        public void ReceivePlayerDamage(int amount)
        {
            bossActionText.text = $"{amount} 데미지를 받았다!";
        }


        // IBossActionHandler
        public async UniTask HandleBossActionAsync()
        {
            // 애니메이션 등 실행 (임시로 3초 대기)
            await UpdateProgressBarAsync(bossActionProgressBar, duration: 2f);
            bossActionText.text = ". . .";

            // 임시 대기 목록
            Managers.Game.Enemy.AttackEnemyTurnStart(); // Enemy Attack 시작
        }

        public void SubscribeBossAction()
        {
            BossActionAsync += HandleBossActionAsync;
        }

        public void UnsubscribeBossAction()
        {
            // 보스 죽을 때 사용
            BossActionAsync -= HandleBossActionAsync;
        }

        #endregion



        private void SetProgressBar(Slider progressBar, float value)
        {
            progressBar.value = value;
        }

        private async UniTask UpdateProgressBarAsync(Slider bar, float duration)
        {
            SetProgressBar(bar, 1f);
            var start = Time.time;
            while (Time.time - start < duration)
            {
                var remain = duration - (Time.time - start);
                SetProgressBar(bar, remain / duration);
                await UniTask.Yield();
            }
            SetProgressBar(bar, 0f);
        }

        public void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
            OnGameStateChanged?.Invoke();
        }
    }
}
