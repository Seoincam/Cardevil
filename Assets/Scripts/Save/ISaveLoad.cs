namespace Cardevil.Save
{
    public interface ISaveLoad
    {
        public void Save(GameSave currentSave);
        public void Load(GameSave currentSave);
    }
}