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
        
        private List<Dungeon> dungeons = new List<Dungeon>();
        private int currentDungeonIndex = -1;
        
        public void Init()
        {
            currentDungeonIndex = 1;
            // TODO : DungeonConfig 로드
            
            
            dungeons.Add(new DungeonFactoryChapter1().Create(0,null));// 더미 던전
            dungeons.Add(new DungeonFactoryChapter1().Create(1,null));
            
        }
        
    }
}