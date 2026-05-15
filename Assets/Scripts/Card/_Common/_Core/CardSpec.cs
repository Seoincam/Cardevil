using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class CardSpec : IDeepClonable<CardSpec>
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

        public CardSpec(int id, CardType type, UpgradeNodeSO upgradeNode = null)
        {
            ID = id;
            Type = type;
            
            if (upgradeNode)
                ApplyUpgradeNodeAndNotify(upgradeNode);
        }
        
        public CardSpec DeepClone()
        {
            var clonedElements = elements.Select(e => e.DeepClone()).ToList();
            var clone = new CardSpec(ID, Type, clonedElements)
            {
                UpgradeNode = UpgradeNode, 
                _isDirty = true, 
                _cachedState = null
            };

            return clone;
        }

        public CardSpec AddElements(params ISpecElement[] specElements)
        {
            elements.AddRange(specElements);
            _isDirty = true;
            SpecChanged?.Invoke(this);
            return this;
        }

        public CardSpec ApplyUpgradeNodeAndNotify(UpgradeNodeSO upgradeNode)
        {
            ApplyUpgradeNode(upgradeNode);
            
            SpecChanged?.Invoke(this);
            return this;
        }

        public CardSpec ApplyUpgradeNode(UpgradeNodeSO upgradeNode)
        {
            if (!upgradeNode || UpgradeNode == upgradeNode)
            {
                LogEx.LogError("UpgradeNode 적용 실패.");
                return this;
            }

            UpgradeNode = upgradeNode;
            _isDirty = true;

            if (UpgradeNode.UpgradeType == UpgradeApplyType.None) return this;
            
            switch (UpgradeNode.UpgradeType)
            {
                case UpgradeApplyType.OverrideColors:
                    // 기본색을 남겨두기 위해서 BaseColorElement는 지우지 않음.
                    elements.RemoveAll(e => e is SelectableColorElement);
                    break;
                
                case UpgradeApplyType.OverrideNumbers:
                    elements.RemoveAll(e => e is INumberElement);
                    break;
                
                case UpgradeApplyType.OverrideDirections:
                    elements.RemoveAll(e => e is IDirectionElement);
                    break;
            }
            elements.AddRange(UpgradeNode.Elements);

            return this;
        }

        /// <summary>
        /// 공격(숫자) 카드의 기본색을 변경합니다.
        /// </summary>
        /// <remarks>
        /// 기본색: 카드의 색이 정해지지 않았을 때 기본적으로 표시될 스프라이트들의 기본색.
        /// </remarks>
        public CardSpec ChangeBaseColor(CardColor targetBaseColor)
        {
            if (Type != CardType.Attack)
            {
                LogEx.LogError("공격(숫자) 카드가 아닌 카드 스펙의 기본 색을 변경할 수 없습니다.");
                return this;
            }
            
            var baseColorElement = elements.FirstOrDefault(e => e is BaseColorElement);
            if (baseColorElement == null)
            {
                LogEx.LogError("BaseColorElement가 존재하지 않습니다.");
                return this;
            }
            
            int baseColorElementIndex = elements.IndexOf(baseColorElement);
            elements[baseColorElementIndex] = new BaseColorElement(targetBaseColor);
            
            return this;
        }
    }
}