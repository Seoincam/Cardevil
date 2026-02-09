using Cardevil.Attributes;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.NewCard.Core
{

    [Serializable]
    public sealed class CardSpec
    {
        [field: SerializeField, VisibleOnly] public uint ID { get; private set; }
        [field: SerializeField, VisibleOnly] public CardType Type { get; private set; }

        [field: SerializeReference, VisibleOnly]
        private List<ISpecElement> elements = new();

        private CardStateBuilder _builder = new();
        private CardState _cachedState;
        private bool _isDirty = true;

        public IReadOnlyList<ISpecElement> Elements => elements;

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

        public bool IsAttackCard => Type == CardType.Attack;
        public bool IsMoveCard => Type == CardType.Move;

        public CardSpec(uint id, CardType type)
        {
            ID = id;
            Type = type;
        }

        public void AddElements(params ISpecElement[] specElements) => elements.AddRange(specElements);
    }
}