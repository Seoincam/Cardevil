using Cardevil.Core.Turn.Interfaces;

namespace Cardevil.Core.Turn
{
    public readonly struct AttackResult
    {
        public ITurnTarget Target { get; }
        public int Damage { get; }

        public AttackResult(ITurnTarget target, int damage)
        {
            Target = target;
            Damage = damage;
        }
    }
}