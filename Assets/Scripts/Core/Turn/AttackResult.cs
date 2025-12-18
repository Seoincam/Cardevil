using Cardevil.Cards.Data;
using Cardevil.Core.Turn.Interfaces;

namespace Cardevil.Core.Turn
{
    /// <summary>
    /// 턴 공격 결과 데이터.
    /// 공격 대상, 족보, 피해량 포함.
    /// </summary>
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