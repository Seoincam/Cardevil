using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class RelicManager
    {
        [SerializeReference, VisibleOnly] private List<Relic> relics;
        
        private RelicContext _context;

        public void BuildContext(PlayerStatus playerStatus, ScoreProviderRegistry scoreProviderRegistry)
        {
            _context = new RelicContext(playerStatus, scoreProviderRegistry);
        }

        public async UniTask AddAsync(Relic relic)
        {
            Debug.Assert(_context != null, "_context != null");

            foreach (var effect in relic.Effects)
            {
                switch (effect)
                {
                    case IScoreProvider scoreProvider:
                        scoreProvider.Id = CardevilCore.Game.ScoreProviderRegistry.Register(scoreProvider);
                        break;
                
                    case IStatModifier statModifier: 
                        statModifier.ModifierId = CardevilCore.Game.PlayerStatus.AddModifier(statModifier);
                        break;
                
                    case IGameEventListener eventListener: break;
                }
            }
            
            // TODO: Relic Bar에 아이콘 추가
            
            // relic.OnAcquire(_context);
        }

        private class RelicContext : IRelicContext
        {
            public PlayerStatus PlayerStatus { get; }
            public ScoreProviderRegistry ScoreProviderRegistry { get; }

            public RelicContext(PlayerStatus playerStatus, ScoreProviderRegistry scoreProviderRegistry)
            {
                PlayerStatus = playerStatus;
                ScoreProviderRegistry = scoreProviderRegistry;
            }
        } 
    }

    public interface IRelicContext
    {
        PlayerStatus PlayerStatus { get; }
        ScoreProviderRegistry ScoreProviderRegistry { get; }
    }
}