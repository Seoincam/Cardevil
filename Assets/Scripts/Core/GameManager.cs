using Cardevil.Attributes;
using Cardevil.Card.InStage.Score.Step;
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
        [field: SerializeField, VisibleOnly] public ScoreProviderRegistry ScoreProviderRegistry { get; private set; }

        public void Init()
        {
            CardevilCore.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
            ScoreProviderRegistry = new ScoreProviderRegistry();
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