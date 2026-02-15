using Cardevil.Cards.Persistence;
using Cardevil.Dungeon;
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
        public CardStatusSaveData cardStatusData;
        public DungeonProgress DungeonProgress;
        
        public GameSave(string fileName, string name)
        {
            FileName = fileName;
            Name = name;
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            cardStatusData = new CardStatusSaveData();
            DungeonProgress = new DungeonProgress(1,1);
        }
        public GameSave()
        {
            Name = "DefaultSave";
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            cardStatusData = new CardStatusSaveData();
            DungeonProgress = new DungeonProgress(1,1);
        }
        
        public DateTime SaveTime
        {
            get => DateTime.FromBinary(RawSaveTime);
            set => RawSaveTime = value.ToBinary();
        }
    }
}