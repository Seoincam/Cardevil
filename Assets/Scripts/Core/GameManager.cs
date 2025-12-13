using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Core.Bootstrap;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Save;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoad
    {
        [Header("State")] 
        [field: SerializeField, VisibleOnly] public PlayerStatus PlayerStatus { get; private set; }
        [field: SerializeField, VisibleOnly] public CardLibrary CardLibrary { get; private set; }
        
        public void Init()
        {
            // 세이브에 등록
            // 각 요소를 직접 등록할 수 있지만,
            // 혹시 모르는 Reference 문제를 방지하기 위해 this로 등록 후 Save, Load에서 처리
            // SaveLoadManager saveLoadManager = Managers.SaveLoad;
            // saveLoadManager.Register(this);
            Bootstrapper.Instance.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
            CardLibrary = new CardLibrary();
        }

        public void Clear()
        {

        }
        
        /// <summary>
        /// 새 게임에 진입하기 전, 필요한 요소를 초기화합니다.
        /// </summary>
        public void EnterNewGame(GameSave currentSave)
        {
            CardLibrary.Init(Bootstrapper.Instance.CardEnhancementData);
            CardLibrary.CreateBasePipelines();
            Bootstrapper.Instance.SaveLoad.SaveGame();
        }
        
        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
            CardLibrary.Load(currentSave);
        }
        

        
        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
            CardLibrary.Save(currentSave);
        }

    }
}