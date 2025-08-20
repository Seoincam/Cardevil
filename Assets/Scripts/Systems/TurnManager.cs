using UnityEngine;
using UnityEngine.UI;
using Cardevil.Cards;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Test;
using Cardevil.Utils.Directions;

namespace Cardevil.Systems
{
    public class TurnManager : IPlayerInputReceiver, IPlayerDamageReceiver, IPlayerActionHandler, IBossDamageReceiver, IBossActionHandler
    {
        private DebugScreen debug;

        [Header("Game State")]
        public bool isPlayerInputReceived;

        // [Header("Events")]
        public delegate UniTask TurnStepAsync();

        public event TurnStepAsync PlayerInputAsync;
        public event TurnStepAsync PlayerActionAsync;
        public event TurnStepAsync BossActionAsync;

        // 카드 배분, 보스 설명 등 처리
        // 처리할 것 많아지면 추후 분리 예정
        public event TurnStepAsync PreGameAsync;

        public event Action OnGameStateChanged;

        [Header("interfaces")]
        public IPlayerInputHandler playerInputHandler;
        public IPlayerActionHandler playerActionHandler;

        public void Init()
        {
            playerInputHandler = new PlayerInputHandler();
            playerActionHandler = this;

            debug = GameObject.Find("DebugCanvas").GetComponent<DebugScreen>();
            if (debug == null)
                Debug.LogError("DebugScreen을 찾지 못했습니다.");

            // 이벤트 구독
            SubscribePlayerAction();
            SubscribePlayerInput();

            SubscribeBossAction();
            SubscribePlayerDamage();

            // 시작
            GameLoopAsync().Forget();
            SetGameState(GameManager.GameState.Action);
        }

        /*
            추후 각 인터페이스들을 다른 클래스에서 구현,
            TurnManager에 이벤트 구독 방식으로 실행
        */


        private async UniTaskVoid GameLoopAsync()
        {
            if (PreGameAsync != null)
            {
                await PreGameAsync.Invoke();    
            }

            while (true)
            {
                if (PlayerInputAsync == null || PlayerActionAsync == null || BossActionAsync == null)
                {
                    await UniTask.Yield();
                    continue;
                }

                // 턴이 시작될때 Enemy의 Turn 값 초기화
                Managers.Game.Enemy.TurnClear(); 


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
                    Direction.Up => "상",
                    Direction.Down => "하",
                    Direction.Left => "좌",
                    Direction.Right => "우",
                    _ => "?"
                }))
                : "없음";

            var text = $@"콤보: {result.combo}
            데미지: {result.damage}
            이동: {moveText}
            ";

            debug.SetTurnDebugTextBar(DebugScreen.TurnDebugTextNames.InputText, text);

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
            SetGameState(GameManager.GameState.Action);

            // 애니메이션 등 실행 (임시로 대기)
            await UpdateProgressBarAsync(DebugScreen.TurnDebugSliderNames.PlayerActionProgressBar, duration: 2f);
            OnPlayerDamageDealt?.Invoke(playerDealtDamage);
            // - -            
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
            if (amount == 0) return;
            var text = $"{amount} 데미지를 받았다!";
            debug.SetTurnDebugTextBar(DebugScreen.TurnDebugTextNames.DamageText, text);
            Managers.Game.Enemy.IsAttacked(amount);
        }


        // IBossActionHandler
        public async UniTask HandleBossActionAsync()
        {
            // 애니메이션 등 실행 (임시로 3초 대기)
            await UpdateProgressBarAsync(DebugScreen.TurnDebugSliderNames.BossActionProgressBar, duration: 2f);
            debug.SetTurnDebugTextBar(DebugScreen.TurnDebugTextNames.DamageText, ". . .");

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


        private async UniTask UpdateProgressBarAsync(DebugScreen.TurnDebugSliderNames slider, float duration)
        {
            debug.SetTurnDebugProgressBar(slider, 1f);
            var start = Time.time;
            while (Time.time - start < duration)
            {
                var remain = duration - (Time.time - start);
                debug.SetTurnDebugProgressBar(slider, remain / duration);
                await UniTask.Yield();
            }
            debug.SetTurnDebugProgressBar(slider, 0f);
        }

        public void SetGameState(GameManager.GameState gameState)
        {
            Managers.Game.currentState = gameState;
            OnGameStateChanged?.Invoke();
        }
    }
}
