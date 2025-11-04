using Cardevil.Ingame.Field;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;

namespace Cardevil.Ingame.Entities
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