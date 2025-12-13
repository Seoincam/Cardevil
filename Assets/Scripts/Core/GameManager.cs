using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Core.Bootstrap;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Save;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoadRoot
    {
        [Header("State")] 
        [field: SerializeField, VisibleOnly] public PlayerStatus PlayerStatus { get; private set; }
        [field: SerializeField, VisibleOnly] public CardLibrary CardLibrary { get; private set; }
        
        public void Init()
        {
            Bootstrapper.Instance.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
            CardLibrary = new CardLibrary(Bootstrapper.Instance.CardEnhancementData);
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            PlayerStatus.SetUpNewGame(currentSave);
            CardLibrary.SetUpNewGame(currentSave);
        }
        
        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
            CardLibrary.Save(currentSave);
        }

        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
            CardLibrary.Load(currentSave);
        }
    }
}