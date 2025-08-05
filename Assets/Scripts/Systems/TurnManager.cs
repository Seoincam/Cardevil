using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cardevil.InGame.Enemy;
namespace Cardevil.Systems
{
    public enum GameState
    {
        Pause, PlayerInput, Action
    }

    public class TurnManager : Singleton<TurnManager>, IPlayerInputHandler, IPlayerInputReceiver, IPlayerDamageReceiver, IPlayerActionHandler, IBossDamageReceiver, IBossActionHandler
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

        
        // UniTaskCompletionSource 

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;

            var cardManager = FindAnyObjectByType<CardManager>();

            // 이벤트 구독
            SubscribePlayerInput();
            cardManager.OnUseCard += EndGetInput;

            SubscribePlayerAction();
            SubscribeBossDamage();
            cardManager.OnUseCard += ReceiveInput;

            SubscribeBossAction();
            SubscribePlayerDamage();
        }

        void Start()
        {
            GameLoopAsync().Forget();
        }


        private async UniTaskVoid GameLoopAsync()
        {
            while (true)
            {
                if (PlayerInputAsync == null || PlayerActionAsync == null || BossActionAsync == null)
                {
                    await UniTask.Yield();
                    continue;
                }

                // Player Input
                await PlayerInputAsync.Invoke();
                //  -> 데미지, 이동, 효과 등 계산

                // Player Action
                await PlayerActionAsync.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                //  -> 보스 상태 체크

                // Boss Action
                await BossActionAsync.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                //  -> 플레이어 상태 체크
            }

            // TODO: cancel, break 로직 추가
        }


        #region Player input

        // IPlayerInputHandler
        public void SubscribePlayerInput()
        {
            PlayerInputAsync += HandlePlayerInputAsync;
        }

        public async UniTask HandlePlayerInputAsync()
        {
            SetGameState(GameState.PlayerInput);

            while (!isPlayerInputReceived)
                await UniTask.Yield();

            isPlayerInputReceived = false;
        }

        public void EndGetInput(CardResult _)
        {
            isPlayerInputReceived = true;
        }

        #endregion



        #region Player Action

        // IPlayerInputReceiver 
        private int playerDealtDamage;       
        public void ReceiveInput(CardResult result)
        {
            var move = result.moves.Count() != 0 ? "이동 있음" : "이동 없음";
            playerActionText.text = $@"(임시) Player Input을 받았습니다.
            콤보: {result.combo}
            데미지: {result.damage}
            이동: {move}
            ";

            playerDealtDamage = result.damage;
        }


        // IPlayerDamageReceiver
        public void SubscribeBossDamage()
        {
            OnBossDamageDealt += ReceiveBossDamage;
        }

        public void ReceiveBossDamage(int amount)
        {
            playerActionText.text = $"{amount} 데미지를 받았다!";
        }


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
            SetProgressBar(playerActionProgressBar, value: 1f);
            var duration = 3f;
            var startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(playerActionProgressBar, value: remaining / duration);

                var elapsed = Time.time - startTime;
                if (elapsed > .48f && elapsed < .5f)
                    OnPlayerDamageDealt?.Invoke(playerDealtDamage);

                await UniTask.Yield();
            }

            playerActionText.text = ". . .";
            
        }

        #endregion



        #region Boss Action
        
        // IBossDamageReceiver
        public void SubscribePlayerDamage()
        {
            OnPlayerDamageDealt += ReceivePlayerDamage;
        }

        public void UnsubscribePlayerDamage(){}

        public void ReceivePlayerDamage(int amount)
        {
            bossActionText.text = $"{amount} 데미지를 받았다!";
        }


        // IBossActionHandler
        public event Action<int> OnBossDamageDealt;

        public async UniTask HandleBossActionAsync()
        {
            // 애니메이션 등 실행 (임시로 3초 대기)
            SetProgressBar(bossActionProgressBar, value: 1f);
            var duration = 2f;
            var startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(bossActionProgressBar, value: remaining / duration);

                var elapsed = Time.time - startTime;
                if (elapsed > .48f && elapsed < .5f)
                    OnBossDamageDealt?.Invoke(1);

                await UniTask.Yield();
            }

            bossActionText.text = ". . .";
            // 임시 대기 목록
            Managers.Game.Enemy.AttackEnemyTurnStart(); // Enemy Attack 시작

        }

        public void SubscribeBossAction()
        {
            BossActionAsync += HandleBossActionAsync;
        }
        
        #endregion



        private void SetProgressBar(Slider progressBar, float value)
        {
            progressBar.value = value;
        }

        public void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
            OnGameStateChanged?.Invoke();
        }
    }
}
