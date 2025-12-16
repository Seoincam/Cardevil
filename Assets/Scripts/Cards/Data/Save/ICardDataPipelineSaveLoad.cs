namespace Cardevil.Cards.Data.Save
{
    public interface ICardDataPipelineSaveLoad
    {
        CardDataPipelineSaveData Serialize();
        void Deserialize(CardDataPipelineSaveData saveData);
    }
}