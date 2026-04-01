using Cardevil.Core.Utils;
using Cardevil.Gameplay.Field;
using Cysharp.Threading.Tasks;

namespace Cardevil.Gameplay.Player
{
    public interface IPlayerControl
    {
        UniTask MoveWithAnim(Direction direction);
        UniTask MoveWithAnim(Direction direction, int distance);
        void MoveTo(int i, int j);
        void MoveTo(TileVector tileVector);
        void MoveTo(Tile tile);
    }
}