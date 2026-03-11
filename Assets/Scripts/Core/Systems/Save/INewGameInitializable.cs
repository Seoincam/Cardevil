namespace Cardevil.Save
{
    /// <summary>
    /// 신규 게임 초기화 인터페이스.
    /// </summary>
    public interface INewGameInitializable
    {
        /// <summary>
        /// 새 세이브 생성 시 초기 상태 구성.
        /// </summary>
        /// <param name="save">신규 생성된 세이브 데이터</param>
        void SetUpNewGame(GameSave save);
    }
}