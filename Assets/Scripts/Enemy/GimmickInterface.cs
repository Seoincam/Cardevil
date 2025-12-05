using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy
{
    public interface IGimmick
    {
        void Apply(Enemy enemy);
    }
    public interface IUpdatableGimmick : IGimmick
    {
        void Tick(float deltaTime);
    }
}
