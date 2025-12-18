namespace Cardevil.Save
{
    /// <summary>
    /// 세이브/로드 루트 인터페이스.
    /// 세이브 로드 처리 및 신규 게임 초기화 기능 통합.
    /// </summary>
    public interface ISaveLoadRoot : ISaveLoad, INewGameInitializable
    {
    }
}