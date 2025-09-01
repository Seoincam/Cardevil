using Cardevil.Dungeon.DungeonFactories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    [Serializable]
    public class DungeonManager
    {
        [SerializeField] private List<DungeonConfigurationSO> dungeonConfigurations = new List<DungeonConfigurationSO>();
        
        [SerializeReference] private List<Dungeon> dungeons = new List<Dungeon>();
        private int currentDungeonIndex = -1;
        
        public int CurrentDungeonIndex => currentDungeonIndex;
        public DungeonConfigurationSO CurrentDungeonConfiguration => dungeonConfigurations[currentDungeonIndex];
        public Dungeon CurrentDungeon => GetDungeon(currentDungeonIndex);

        
        public void Init()
        {
            currentDungeonIndex = 1;
            // TODO : DungeonConfig 로드
            
            // UI 기반으로 던전 생성
            
            
        }

        public Dungeon GetDungeon(int id)
        {
            foreach (var dungeon in dungeons)
            {
                if (dungeon.DungeonId == id)
                {
                    return dungeon;
                }
            }
            return null;
        }
    }
}