using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems.Save;
using Cardevil.Gameplay;
using Cardevil.Gameplay.Relics.Core;
using UnityEngine;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoadRoot, INewGameInitializable
    {
        [field: SerializeField, VisibleOnly] public PlayerStatus PlayerStatus { get; private set; }
        [field: SerializeField, VisibleOnly] public CardRepository CardRepository { get; private set; }
        [field: SerializeField, VisibleOnly] public ScoreProviderRegistry ScoreProviderRegistry { get; private set; }
        
        public RelicManager Relic { get; private set; }

        public void Init()
        {
            CardevilCore.SaveLoad.Register(this);
            
            PlayerStatus = new PlayerStatus();
            ScoreProviderRegistry = new ScoreProviderRegistry();
            Relic = new RelicManager(PlayerStatus, ScoreProviderRegistry);
            CardRepository = new CardRepository();
        }
        
        public void SetUpNewGame(GameSave currentSave)
        {
            PlayerStatus.SetUpNewGame(currentSave);
            CardRepository.SetUpNewGame(currentSave);
        }
        
        public void Save(GameSave currentSave)
        {
            PlayerStatus.Save(currentSave);
            CardRepository.Save(currentSave);
            Relic.Save(currentSave);
        }

        public void Load(GameSave currentSave)
        {
            PlayerStatus.Load(currentSave);
            CardRepository.Load(currentSave);
            Relic.Load(currentSave);
        }
    }
}