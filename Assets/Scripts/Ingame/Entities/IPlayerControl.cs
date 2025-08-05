using Cardevil.Ingame.Field;
using Cardevil.Utils;
using Cardevil.Utils.Directions;

namespace Cardevil.Ingame.Entities
{
    public interface IPlayerControl
    {
        void Move(Direction direction);
        void Move(Direction direction, int distance);
        void MoveTo(int i, int j);
        void MoveTo(TileVector tileVector);
        void MoveTo(Tile tile);
    }
}