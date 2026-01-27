namespace Cardevil.Cards.Persistence
{
    public interface ICardSpecSaveLoad
    {
        CardSpecSaveData Serialize();
        void Deserialize(CardSpecSaveData saveSpecSaveData);
    }
}