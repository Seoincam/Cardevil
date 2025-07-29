using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cardevil.Cards;
using System;
using System.Linq;

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
        public bool playerInputReceived;

        [Header("References")]
        public Slider playerActionProgressBar;
        public Text playerActionText;

        public Slider bossActionProgressBar;
        public Text bossActionText;

        // [Header("Events")]
        public delegate IEnumerator ActionCoroutine();
        public event ActionCoroutine PlayerInputCoroutine;
        public event ActionCoroutine PlayerActionCoroutine;
        public event ActionCoroutine BossActionCoroutine;

        public event Action OnGameStateChanged;

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;

            var cardManager = FindAnyObjectByType<CardManager>();
            cardManager.OnUseCard += EndGetInput; // Player Input
            cardManager.OnUseCard += ReceiveInput; // Player Action

            // 이벤트 구독
            SubscribePlayerInput();
            SubscribePlayerAction();
            SubscribeBossAction();

            SubscribeBossDamage();
            SubscribePlayerDamage();
        }

        void Start()
        {
            StartCoroutine(GameLoop());
        }

        private IEnumerator GameLoop()
        {
            while (true)
            {
                if (PlayerInputCoroutine == null || PlayerActionCoroutine == null || BossActionCoroutine == null)
                {
                    yield return null;
                    continue;
                }

                // Player Input
                yield return StartCoroutine(PlayerInputCoroutine());
                //  -> 데미지, 이동, 효과 등 계산

                // Player Action
                yield return StartCoroutine(PlayerActionCoroutine());
                yield return new WaitForSeconds(0.5f);
                //  -> 보스 상태 체크

                // Boss Action
                yield return StartCoroutine(BossActionCoroutine());
                yield return new WaitForSeconds(0.5f);
                //  -> 플레이어 상태 체크
            }
        }


        #region Player input
        // IPlayerInputHandler
        public IEnumerator HandlePlayerInputCoroutine()
        {
            SetGameState(GameState.PlayerInput);

            while (!playerInputReceived)
                yield return null;
            
            playerInputReceived = false;
        }

        public void EndGetInput(CardResult _)
        {
            playerInputReceived = true;
        }

        public void SubscribePlayerInput()
        {
            PlayerInputCoroutine += HandlePlayerInputCoroutine;
        }
        # endregion



        #region Player Action
        // IPlayerInputReceiver        
        public void ReceiveInput(CardResult result)
        {
            playerActionText.text = $@"(임시) Player Input을 받았습니다.
            콤보: {result.Combo}
            데미지: {result.TotalDamage}
            이동: {result.directions.First()}
            ";
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
            PlayerActionCoroutine += HandlePlayerActionCoroutine;
        }

        public IEnumerator HandlePlayerActionCoroutine()
        {
            SetGameState(GameState.Action);

            // 애니메이션 등 실행 (임시로 대기)
            SetProgressBar(playerActionProgressBar, value: 1f);
            var duration = 3f;
            var startTime = Time.time;

            // 임시로 진행도 표시
            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(playerActionProgressBar, value: remaining / duration);

                yield return null;
            }

            playerActionText.text = ". . .";
            var damage = 15;
            OnPlayerDamageDealt?.Invoke(damage);
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
            Debug.Log("2");
            bossActionText.text = $"{amount} 데미지를 받았다!";
        }


        // IBossActionHandler
        public event Action<int> OnBossDamageDealt;

        public IEnumerator HandleBossActionCoroutine()
        {
            // 애니메이션 등 실행 (임시로 3초 대기)
            SetProgressBar(bossActionProgressBar, value: 1f);
            var duration = 2f;
            var startTime = Time.time;

            // 임시로 진행도 표시
            while (Time.time - startTime < duration)
            {
                var remaining = duration - (Time.time - startTime);
                SetProgressBar(bossActionProgressBar, value: remaining / duration);

                if (Time.time - startTime > .48f && Time.time - startTime < .5f)
                    OnBossDamageDealt?.Invoke(10);

                yield return null;
            }

            bossActionText.text = ". . .";
        }

        public void SubscribeBossAction()
        {
            BossActionCoroutine += HandleBossActionCoroutine;
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
