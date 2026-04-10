namespace Cardevil.Gameplay.Relics.Core
{
    /// <summary>
    /// 플레이어의 스탯을 변화시키는 객체가 구현해야함.
    /// </summary>
    public interface IStatModifier
    {
        StatType TargetType { get; }
        int Modify(int previousValue);
    }
}