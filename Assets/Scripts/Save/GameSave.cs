using Cardevil.Cards.Data.Save;
using Cardevil.Ingame;
using System;

namespace Cardevil.Save
{
    [Serializable]
    public class GameSave
    {
        public string FileName;
        public string Name;
        public long RawSaveTime;
        public PlayerStatus PlayerStatus;
        public CardLibrarySaveData CardLibraryData;
        
        public GameSave(string fileName, string name)
        {
            FileName = fileName;
            Name = name;
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            CardLibraryData = new CardLibrarySaveData();
        }
        public GameSave()
        {
            Name = "DefaultSave";
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            CardLibraryData = new CardLibrarySaveData();
        }
        
        public DateTime SaveTime
        {
            get => DateTime.FromBinary(RawSaveTime);
            set => RawSaveTime = value.ToBinary();
        }
    }
}