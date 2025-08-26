using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Test;
using Cardevil.Events;

namespace Cardevil.Systems
{
    public class TurnManager
    {   
        private DebugScreen debug;

        [Header("Interfaces")]
        private IUserInput userInput;
        private IPlayerMove playerMove;
        private IPlayerAction playerAction;
        private IEnemy enemy;

        public void Init()
        {
            debug = GameObject.Find("DebugCanvas").GetComponent<DebugScreen>();
            if (debug == null)
                Debug.LogError("DebugScreen을 찾지 못했습니다.");

            // 시작
            GameLoopAsync().Forget();
            SetGameState(GameManager.GameState.Action);
        }


        private async UniTaskVoid GameLoopAsync()
        {
            // TODO: 적에 대한 설명

            while (true)
            {
                // 턴이 시작될때 Enemy의 Turn 값 초기화
                Managers.Game.Enemy.TurnClear();

                await userInput.DrawCard();
                if (userInput.IsNoCard)
                {
                    // TODO: 게임 오버    
                }

                userInput.ActivateInteraction();
                await userInput.HandleUserInput();
                userInput.InactivateInteraction();

                await playerMove.Move();
                await playerAction.Attack();

                if (enemy.IsDead)
                { 
                    // TODO: 게임 클리어
                }

                if (enemy.CheckAttack())
                {
                    await enemy.Attack();
                    if (playerAction.IsDead)
                    {
                        // TODO: 게임 오버
                    }
                }
            }
        }




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
            using (var args = GameStateChangeArgs.Get())
            {
                args.Init(gameState);
                Managers.Event.GameStateChangeEvent?.Invoke(args);
            }
        }
    }
}
