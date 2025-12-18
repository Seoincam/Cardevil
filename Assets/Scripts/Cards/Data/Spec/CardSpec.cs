using Cardevil.Attributes;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Cards.Data.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.Data.Spec
{
    public interface IReadOnlyCardSpec
    {
        int Id { get; }
        CardKind Kind { get; }
        IReadOnlyList<IModifier> Modifiers { get; }
        
        Guid CurrentEnhancementId { get; }
        IReadOnlyList<Guid> NextEnhancementIds { get; }
    }
    
    /// <summary>
    /// 카드 데이터 스펙.
    /// 수정자 및 강화 흐름을 기반으로 카드 데이터 구성.
    /// </summary>
    [Serializable]
    public class CardSpec : IReadOnlyCardSpec, ICardSpecSaveLoad
    {
        [SerializeField, VisibleOnly] private int id;
        [SerializeField, VisibleOnly] private CardKind kind;
        [SerializeReference, VisibleOnly] private List<IModifier> modifiers = new(); 
        
        private Guid _currentEnhancementId;
        private readonly List<Guid> _nextEnhancementIds = new();

        #region IReadOnlyCardSpec Members (getter)

        public int Id => id;
        public CardKind Kind => kind;
        public IReadOnlyList<IModifier> Modifiers => modifiers;
        
        public Guid CurrentEnhancementId => _currentEnhancementId;
        public IReadOnlyList<Guid> NextEnhancementIds => _nextEnhancementIds;

        #endregion
        
        public CardSpec(CardKind kind, int id)
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

        public CardSpecSaveData Serialize()
        {
            // TODO: GUID 체크
            return new CardSpecSaveData()
            {
                id = id,
                kind = kind,
                
                // TODO: 필요하다면 Linq 제거하기
                modifiers = modifiers.Select(m => m.Serialize()).ToList(),
                
                currentEnhancementId = _currentEnhancementId,
                nextEnhancementIds = _nextEnhancementIds
            };
        }

        public void Deserialize(CardSpecSaveData saveSpecSaveData)
        {
            id = saveSpecSaveData.id;
            kind = saveSpecSaveData.kind;
            
            modifiers.Clear();
            foreach (var modifier in saveSpecSaveData.modifiers)
            {
                var mod = ModifierFactory.Create(modifier);
                modifiers.Add(mod);
            }
            
            _currentEnhancementId = saveSpecSaveData.currentEnhancementId;
            _nextEnhancementIds.Clear();
            
            if (saveSpecSaveData.nextEnhancementIds != null)
                _nextEnhancementIds.AddRange(saveSpecSaveData.nextEnhancementIds);
        }

        public static CardSpec FromSaveData(CardSpecSaveData saveSpecSaveData)
        {
            var cardSpec = new CardSpec(CardKind.Attack, -1);
            cardSpec.Deserialize(saveSpecSaveData);
            return cardSpec;
        }
    }
}