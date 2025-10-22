using Cardevil.Attributes;
using Cardevil.Cards.Data.Modifiers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    [Serializable]
    public class CardDataPipeline
    {
        [SerializeField, VisibleOnly] private int id;
        [SerializeField, VisibleOnly] private CardKind kind;
        [SerializeReference, VisibleOnly] private List<IModifier> modifiers = new(); 
        
        private Guid _currentEnhancementId;
        private readonly List<Guid> _possibleEnhancementIds = new();

        #region getter

        public int Id => id;
        public CardKind Kind => kind;
        public IReadOnlyList<IModifier> Modifiers => modifiers;
        
        public Guid CurrentEnhancementId => _currentEnhancementId;
        public IReadOnlyList<Guid> PossibleEnhancementIds => _possibleEnhancementIds;

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

        public void SetEnhancement(Guid guid)
        {
            _currentEnhancementId = guid;
        }

        public void SetPossibleEnhancements(params Guid[] guids)
        {
            _possibleEnhancementIds.Clear();

            foreach (var enhancement in guids)
                _possibleEnhancementIds.Add(enhancement);
        }

        public void ClearPossibleEnhancements()
        {
            _possibleEnhancementIds.Clear();
        }
    }
}