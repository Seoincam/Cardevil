using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Save;
using Cardevil.Utils;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoad
    {
        [Header("State")] 
        [field: SerializeField, VisibleOnly] public PlayerStatus PlayerStatus { get; private set; }// 플레이어 상태 
        [field: SerializeField, VisibleOnly] public CardLibrary CardLibrary { get; private set; }
        
        public void Init()
        {

            // 세이브에 등록
            // 각 요소를 직접 등록할 수 있지만,
            // 혹시 모르는 Reference 문제를 방지하기 위해 this로 등록 후 Save, Load에서 처리
            // SaveLoadManager saveLoadManager = Managers.SaveLoad;
            // saveLoadManager.Register(this);
            
        }

        public void Clear()
        {

        }

        //플레이어 죽을 때 실행시킬 함수
        public void PlayerDied()
        {
            if (PlayerStatus.canRevive)
            {
                //부활하기
                PlayerStatus.CurrentHp = 1;

            }
        }

        public void GameStart()
        {
            // Debug.Log("[GameManager] Database가 초기화 되었습니다. GameStart 실행.");
            LogEx.Log("Database가 초기화 되었습니다. GameStart 실행.");
            PlayerStatus = new PlayerStatus();
            PlayerStatus.BroadcastInitialStatus();
        }

        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
            CardLibrary.Save(currentSave);
        }

        // TODO: 로드할 거 없을 때 처리하기
        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
            CardLibrary.Load(currentSave);
        }
    }
}