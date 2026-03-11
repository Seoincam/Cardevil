using Cardevil.Core.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class CardSpec
    {
        [field: SerializeField, VisibleOnly] public uint ID { get; private set; }
        [field: SerializeField, VisibleOnly] public CardType Type { get; private set; }

        [SerializeReference, VisibleOnly] private List<ISpecElement> elements = new();

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

        public CardSpec(uint id, CardType type, List<ISpecElement> elements = null)
        {
            ID = id;
            Type = type;

            if (elements != null)
            {
                this.elements.AddRange(elements);
            }
        }

        public CardSpec AddElements(params ISpecElement[] specElements)
        {
            elements.AddRange(specElements);
            _isDirty = true;
            return this;
        }
    }
}