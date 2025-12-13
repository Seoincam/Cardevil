namespace Cardevil.Core.Turn.Interfaces
{
    /// <summary>
    /// 턴 공격 대상 인터페이스.
    /// 생존 여부 조회 및 피해 처리 제공.
    /// </summary>
    public interface ITurnTarget
    {
        bool IsDead { get; }
        void TakeDamage(int amount);
    }
}