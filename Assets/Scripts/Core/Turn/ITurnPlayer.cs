using Cardevil.Card.InStage;
using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Cardevil.Core.Turn
{
    public interface ITurnPlayer
    {
        bool IsDead { get; }
        TileVector Position { get; }

        UniTask OnMoveAsync(PlayerMoveArgs args, CancellationToken cancellationToken);
        UniTask AttackAsync(float damage);
        UniTask TakeDamageAsync(float damage);
    }
}