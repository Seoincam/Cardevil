using Cardevil.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    public class DungeonUI : UI_Popup
    {
        [SerializeField, VisibleOnly] private int currentShowingDungeonId = -1;
        [SerializeField] private List<DungeonStageUI> stageUis = new List<DungeonStageUI>();
        
        public override void Init()
        {
            base.Init();
            
        }

        public void UpdateShowingDungeon(int id)
        {
            foreach (DungeonStageUI stage in stageUis)
            {
                stage.gameObject.SetActive(false);
            }
            DungeonStageUI stageUi = null;
            foreach (DungeonStageUI stage in stageUis)
            {
                if (stage.DungeonId == id)
                {
                    stageUi = stage;
                    break;
                }
            }
            if (stageUi == null)
            {
                Debug.LogError($"No DungeonStageUI found for dungeon ID {id}");
                return;
            }
            stageUi.gameObject.SetActive(true);
        }
        
        [ContextMenu("Test Inc Dungeon Id")]
        public void TestIncDungeonId()
        {
            currentShowingDungeonId++;
            if (currentShowingDungeonId >= stageUis.Count)
            {
                currentShowingDungeonId = 0;
            }
            UpdateShowingDungeon(currentShowingDungeonId);
        }
        
    }
}