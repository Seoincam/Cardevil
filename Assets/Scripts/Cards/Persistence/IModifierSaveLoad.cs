namespace Cardevil.Cards.Persistence
{
    /// <summary>
    /// 수정자 세이브/로드 인터페이스.
    /// 수정자 직렬화 및 역직렬화 처리.
    /// </summary>
    public interface IModifierSaveLoad
    {
        CardModifierSaveData Serialize();
        void Deserialize(CardModifierSaveData data);
    }
}