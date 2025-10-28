using Cardevil.Attributes;
using Cardevil.Cards.Data.Modifiers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardDataPipeline
    {
        int Id { get; }
        CardKind Kind { get; }
        IReadOnlyList<IModifier> Modifiers { get; }
        
        Guid CurrentEnhancementId { get; }
        IReadOnlyList<Guid> NextEnhancementIds { get; }
    }
    
    [Serializable]
    public class CardDataPipeline : IReadOnlyCardDataPipeline
    {
        [SerializeField, VisibleOnly] private int id;
        [SerializeField, VisibleOnly] private CardKind kind;
        [SerializeReference, VisibleOnly] private List<IModifier> modifiers = new(); 
        
        private Guid _currentEnhancementId;
        private readonly List<Guid> _nextEnhancementIds = new();

        #region IReadOnlyCardDataPipeline Members (getter)

        public int Id => id;
        public CardKind Kind => kind;
        public IReadOnlyList<IModifier> Modifiers => modifiers;
        
        public Guid CurrentEnhancementId => _currentEnhancementId;
        public IReadOnlyList<Guid> NextEnhancementIds => _nextEnhancementIds;

        #endregion
        
        public CardDataPipeline(CardKind kind, int id)
        {
            this.kind = kind;
            this.id = id;
        }

        public void AddModifier(IModifier mod)
        {
            modifiers.Add(mod);
        }

        public void SetCurrentEnhancementId(Guid guid)
        {
            _currentEnhancementId = guid;
        }

        public void SetNextEnhancementIds(params Guid[] guids)
        {
            _nextEnhancementIds.Clear();

            foreach (var enhancement in guids)
                _nextEnhancementIds.Add(enhancement);
        }

        public void ClearNextEnhancementIds()
        {
            _nextEnhancementIds.Clear();
        }
    }
}