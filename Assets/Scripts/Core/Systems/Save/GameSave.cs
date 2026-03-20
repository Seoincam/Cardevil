using Cardevil.Gameplay;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Relics.Core;
using System;
using System.Collections.Generic;

namespace Cardevil.Core.Systems.Save
{
    [Serializable]
    public class GameSave
    {
        public string FileName;
        public string Name;
        public long RawSaveTime;
        public PlayerStatus PlayerStatus;
        public DungeonProgress DungeonProgress;
        public List<RelicSaveData> OwnedRelics;
        
        public GameSave(string fileName, string name)
        {
            FileName = fileName;
            Name = name;
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            DungeonProgress = new DungeonProgress(1,1);
        }
        public GameSave()
        {
            Name = "DefaultSave";
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
            DungeonProgress = new DungeonProgress(1,1);
            OwnedRelics = new List<RelicSaveData>();
        }
        
        public DateTime SaveTime
        {
            get => DateTime.FromBinary(RawSaveTime);
            set => RawSaveTime = value.ToBinary();
        }
    }
}