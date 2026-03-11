using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy
{
    public interface IGimmick
    {
        void Apply(Enemy enemy);
        void Remove();
    }
    public interface IUpdatableGimmick : IGimmick
    {
        void Tick(float deltaTime);
    }
}
