using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Core.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class CardSpec
    {
        public event Action<CardSpec> SpecChanged;
        
        [field: Header("Core Data")]
        [field: SerializeField, VisibleOnly] public int ID { get; private set; }
        [field: SerializeField, VisibleOnly] public CardType Type { get; private set; }

        [SerializeReference, VisibleOnly] private List<ISpecElement> elements = new();
        
        [field: Header("Upgrade Data")]
        [field: SerializeField] public UpgradeNodeSO UpgradeNode { get; private set; }
        

        private CardStateBuilder _builder = new();
        private CardState _cachedState;
        private bool _isDirty = true;

        public IReadOnlyList<ISpecElement> Elements => elements;

        /// <summary>
        /// 현재 Spec 기준으로 생성된 카드 상태.
        /// </summary>
        public CardState State
        {
            get
            {
                if (_isDirty || _cachedState == null)
                {
                    _cachedState = _builder.Build(this);
                    _isDirty = false;
                }

                return _cachedState;
            }
        }

        public bool IsAttack => Type == CardType.Attack;
        public bool IsMove => Type == CardType.Move;

        
        public CardSpec(int id, CardType type, List<ISpecElement> elements)
        {
            ID = id;
            Type = type;

            if (elements != null)
            {
                this.elements.AddRange(elements);
            }
        }

        public CardSpec(int id,
            CardType type,
            UpgradeNodeSO upgradeNode = null)
        {
            ID = id;
            Type = type;
            UpgradeNode = upgradeNode;
        }

        public CardSpec AddElements(params ISpecElement[] specElements)
        {
            elements.AddRange(specElements);
            _isDirty = true;
            SpecChanged?.Invoke(this);
            return this;
        }
    }
}