namespace Cardevil.Dungeon
{
    /// <summary>
    /// 던전을 만드는 팩토리.
    /// 인스펙터 방식은 번거로울 것 같아 일단 하드코딩으로 구현.
    /// </summary>
    public abstract class DungeonFactory
    {
        public abstract Dungeon Create(int dungeonId, DungeonConfigurationSO dungeonConfiguration);
    }
}