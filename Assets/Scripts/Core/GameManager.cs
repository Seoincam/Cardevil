using Cardevil.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Save;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoadRoot, INewGameInitializable
    {
        [Header("State")] 
        [field: SerializeField, VisibleOnly] public PlayerStatus PlayerStatus { get; private set; }

        public void Init()
        {
            CardevilCore.Instance.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            PlayerStatus.SetUpNewGame(currentSave);
        }
        
        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
        }

        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
        }
    }
}