namespace Cardevil.Gameplay.Enemy
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
