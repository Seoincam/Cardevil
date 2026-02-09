using Cardevil.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class CardSpec
    {
        [field: SerializeField, VisibleOnly] public uint ID { get; private set; }
        [field: SerializeField, VisibleOnly] public CardType Type { get; private set; }

        [field: SerializeReference, VisibleOnly] private List<ISpecElement> elements = new();

        private CardStateBuilder _builder = new();
        private CardState _cachedState;
        private bool _isDirty = true;

        public IReadOnlyList<ISpecElement> Elements => elements;

        /// <summary>
        /// 현재 Spec 기준으로 생성된 카드 상태.
        /// Element 변경 시, 반환 전 재빌드함.
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

        public bool IsAttackCard => Type == CardType.Attack;
        public bool IsMoveCard => Type == CardType.Move;

        public CardSpec(uint id, CardType type) : this(id, type, null) { }

        public CardSpec(uint id, CardType type, List<ISpecElement> elements)
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