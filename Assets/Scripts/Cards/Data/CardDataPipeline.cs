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

        public void SetEnhancement(Guid id)
        {
            _currentEnhancementId = id;
        }

        public void SetPossibleEnhancements(params Guid[] ids)
        {
            _possibleEnhancementIds.Clear();

            foreach (var enhancement in ids)
                _possibleEnhancementIds.Add(enhancement);
        }
    }
}