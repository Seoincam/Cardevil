using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Cardevil.Systems
{
    public class TurnManager
    {   
        private CancellationTokenSource cts;

        [Header("Interfaces")]
        private ITurnPlayerInput playerInput;
        private ITurnRerollInput rerollInput;
        private ITurnPlayerMove playerMove;
        private ITurnPlayerAction playerAction;
        private ITurnEnemy enemy;


        public void Init(ITurnRerollInput rerollInput, ITurnPlayerInput playerInput, ITurnPlayerMove playerMove, ITurnPlayerAction playerAction, ITurnEnemy enemy)
        {
            if (rerollInput == null)
            {
                Debug.LogError("TurnManger.Init에서 rerollInput이 null입니다.");
                return;
            }
            if (playerInput == null)
            {
                Debug.LogError("TurnManager.Init에서 userInput이 null입니다");
                return;
            }
            if (playerMove == null)
            {
                Debug.LogError("TurnManager.Init에서 playerMove가 null입니다");
                return;
            }
            if (playerAction == null)
            {
                Debug.LogError("TurnManager.Init에서 playerAction이 null입니다");
                return;
            }
            if (enemy == null)
            {
                Debug.LogError("TurnManager.Init에서 enemy가 null입니다");
                return;
            }

            this.rerollInput = rerollInput;
            this.playerInput = playerInput;
            this.playerMove = playerMove;
            this.playerAction = playerAction;
            this.enemy = enemy;

            Init();
        }

        private void Init()
        {
            // 시작
            cts = new();
            GameLoopAsync(cts.Token).Forget();
        }


        private async UniTask GameLoopAsync(CancellationToken cts)
        {
            await enemy.TurnAttack();
            await rerollInput.RerollCard();
            
            // TODO: 적에 대한 설명
            await playerInput.DrawCard();

            while (!cts.IsCancellationRequested)
            {
                // 턴이 시작될때 Enemy의 Turn 값 초기화
                Managers.Game.Enemy.TurnClear();

                await playerInput.DrawCard();
                if (playerInput.IsNoCard)
                {
                    // TODO: 게임 오버    
                }

                playerInput.ActivateInteraction();
                await playerInput.WaitUserInput();
                playerInput.InactivateInteraction();

                await playerMove.TurnMove();
                await playerAction.TurnAttack();

                if (enemy.IsDead)
                { 
                    // TODO: 게임 클리어
                }

                await enemy.TurnAttack();
                
                if (playerAction.IsDead)
                {
                    Managers.Game.PlayerDied();
                }
            }
        }
    }
}
