using Cardevil.Events.ExecEvents;
using Cardevil.NewCard.InStage;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Cardevil.Core.Turn
{
    public interface ITurnEnemy
    {
        bool IsDead { get; }

        UniTask<int> AttackAsync(TileVector playerPosition);

        UniTask TakeDamageAsync(float damage);

        void OnTurnEnd();
    }

    public interface ITurnPlayer
    {
        bool IsDead { get; }
        TileVector Position { get; }

        UniTask OnMove(PlayerMoveArgs args, CancellationToken cancellationToken);
        UniTask TakeDamageAsync(float damage);
    }
    
    public class NewTurnManager
    {
        private StageCardCorePresenter _cardCore;
        private ITurnEnemy _enemy;
        private ITurnPlayer _player;

        private void Initialize()
        {
            ExecEventBus<PlayerMoveArgs>.RegisterStatic(0, _player.OnMove);
        }
        
        private async UniTask CoreLoopAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _cardCore.WaitUntilPlayerInputAsync();
                var finalScore= await _cardCore.UseAsync();

                if (finalScore > 0f)
                {
                    await _enemy.TakeDamageAsync(finalScore);
                }

                if (_enemy.IsDead)
                {
                    // 승리 or 적 교체
                    break;
                }

                float enemyDamage = await _enemy.AttackAsync(_player.Position);

                if (enemyDamage > 0f)
                {
                    await _player.TakeDamageAsync(enemyDamage);
                }

                if (_player.IsDead)
                {
                    // 게임 오버
                    break;
                }
                
                _enemy.OnTurnEnd();
            }
        }
    }
}