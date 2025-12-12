using Cardevil.Cards.Data;
using Cardevil.Core.Turn.Interfaces;

namespace Cardevil.Core.Turn
{
    public readonly struct AttackResult
    {
        public ITurnTarget Target { get; }
        public HandRanking HandRanking { get; }
        public int Damage { get; }

        public AttackResult(ITurnTarget target, HandRanking handRanking, int damage)
        {
            Target = target;
            HandRanking = handRanking;
            Damage = damage;
        }
    }
}