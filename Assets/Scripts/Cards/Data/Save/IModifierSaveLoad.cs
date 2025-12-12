namespace Cardevil.Cards.Data.Save
{
    public interface IModifierSaveLoad
    {
        CardModifierSaveData Serialize();
        void Deserialize(CardModifierSaveData data);
    }
}