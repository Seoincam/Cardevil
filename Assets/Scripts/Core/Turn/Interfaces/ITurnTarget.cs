namespace Cardevil.Core.Turn.Interfaces
{
    public interface ITurnTarget
    {
        bool IsDead { get; }
        void TakeDamage(int amount);
    }
}