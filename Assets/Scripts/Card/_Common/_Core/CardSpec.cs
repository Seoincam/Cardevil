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
            ApplyUpgradeNode(upgradeNode);
        }

        public CardSpec AddElements(params ISpecElement[] specElements)
        {
            elements.AddRange(specElements);
            _isDirty = true;
            SpecChanged?.Invoke(this);
            return this;
        }
        
        /// <param name="upgradeNode">적용할 강화 단계 노드.</param>
        /// <param name="isUIAction">
        /// <c>true</c>일 경우, UI 표시용으로 판단해 이벤트를 발행하지 않음.
        /// <c>false</c>일 경우, 실제 강화가 이루어진 것으로 판단해 이벤트를 발행함.
        /// </param>
        public CardSpec ApplyUpgradeNode(UpgradeNodeSO upgradeNode, bool isUIAction = false)
        {
            if (!upgradeNode || UpgradeNode == upgradeNode) return this;

            UpgradeNode = upgradeNode;
            _isDirty = true;
            
            if (UpgradeNode.UpgradeType == UpgradeApplyType.None) return this;

            switch (UpgradeNode.UpgradeType)
            {
                case UpgradeApplyType.OverrideColors:
                    elements.RemoveAll(e => e is IColorElement);
                    break;
                
                case UpgradeApplyType.OverrideNumbers:
                    elements.RemoveAll(e => e is INumberElement);
                    break;
                
                case UpgradeApplyType.OverrideDirections:
                    elements.RemoveAll(e => e is IDirectionElement);
                    break;
            }
            elements.AddRange(UpgradeNode.Elements);

            if (!isUIAction)
            {
                SpecChanged?.Invoke(this);
            }

            return this;
        }
    }
}