namespace Cardevil.Cards.Data.Save
{
    public interface ICardSpecSaveLoad
    {
        CardSpecSaveData Serialize();
        void Deserialize(CardSpecSaveData saveSpecSaveData);
    }
}