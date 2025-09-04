using Cardevil.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    public class DungeonUI : UI_Popup
    {
        [SerializeField, VisibleOnly] private int currentShowingDungeonId = -1;
        [SerializeField] private List<DungeonChapterUI> stageUis = new List<DungeonChapterUI>();
        
        public override void Init()
        {
            base.Init();
            
        }

        public void UpdateShowingDungeon(int id)
        {
            foreach (DungeonChapterUI stage in stageUis)
            {
                stage.gameObject.SetActive(false);
            }
            DungeonChapterUI chapterUI = null;
            foreach (DungeonChapterUI stage in stageUis)
            {
                if (stage.DungeonId == id)
                {
                    chapterUI = stage;
                    break;
                }
            }
            if (chapterUI == null)
            {
                Debug.LogError($"No DungeonStageUI found for dungeon ID {id}");
                return;
            }
            chapterUI.gameObject.SetActive(true);
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