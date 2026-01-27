using Cardevil.Attributes;
using Cardevil.Cards.Core;
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
        [field: SerializeField, VisibleOnly] public CardStatus CardStatus { get; private set; }
        
        public void Init()
        {
            CardevilCore.Instance.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
            CardStatus = new CardStatus(CardevilCore.Instance.CardEnhancementData);
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            PlayerStatus.SetUpNewGame(currentSave);
            CardStatus.SetUpNewGame(currentSave);
        }
        
        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
            CardStatus.Save(currentSave);
        }

        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
            CardStatus.Load(currentSave);
        }
    }
}