using Cardevil.Cards.Data.Modifiers.Move;
using Cardevil.Cards.Data.Modifiers.Number;
using Cardevil.Utils;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    public class CardDataModifierService
    {
        // <(Type, Level), Data>       
        private readonly Dictionary<(NumberModifierType, int), NumberEnhancementData> _numberEnhancements = new();
        private readonly Dictionary<(MoveModifierType, int), MoveEnhancementData> _moveEnhancements = new();
        
        public IReadOnlyDictionary<(NumberModifierType, int), NumberEnhancementData> NumberEnhancements => _numberEnhancements;
        public IReadOnlyDictionary<(MoveModifierType, int), MoveEnhancementData> MoveEnhancements => _moveEnhancements; 

        public void Init()
        {
            MakeTestData();
        }

        private void MakeTestData()
        {
            var numberSelectable = new NumberEnhancementData(NumberModifierType.Selectable, 1, 1, 1, 1f, 0f);
            _numberEnhancements[(NumberModifierType.Selectable, 1)] = numberSelectable;

            var numberDamage = new NumberEnhancementData(NumberModifierType.Damage, 1, 1, 1, 1f, 0f);
            _numberEnhancements[(NumberModifierType.Damage, 1)] = numberDamage;
            
            var move = new MoveEnhancementData(MoveModifierType.Selectable, 1, 1, 1, 1f, 0f);
            _moveEnhancements[(MoveModifierType.Selectable, 1)] = move;
        }

        public NumberEnhancementData GetBaseSelectableEnhancement()
        {
            var type = NumberModifierType.Selectable;
            var level = 1;

            if (TryGetNumberEnhancementData(type, level, out var data))
                return (NumberEnhancementData)data;

            LogEx.LogError($"Number {type} 타입의 {level} 레벨 강화 데이터를 찾을 수 없음!");
            return new NumberEnhancementData();
        }

        public NumberEnhancementData GetBaseDamageEnhancement()
        {
            var type = NumberModifierType.Damage;
            var level = 1;
            
            if (TryGetNumberEnhancementData(type, level, out var data))
                return (NumberEnhancementData)data;

            LogEx.LogError($"Number {type} 타입의 {level} 레벨 강화 데이터를 찾을 수 없음!");
            return new NumberEnhancementData();
        }

        public MoveEnhancementData GetBaseMoveEnhancement()
        {
            var type = MoveModifierType.Selectable;
            var level = 1;
            
            if (TryGetMoveEnhancementData(type, level, out var data))
                return (MoveEnhancementData)data;
            
            LogEx.LogError($"Move {type} 타입의 {level} 레벨 강화 데이터를 찾을 수 없음!");
            return new MoveEnhancementData();
        }

        public bool TryGetNumberEnhancementData(NumberModifierType type, int level,
            out NumberEnhancementData? data)
        {
            data = null;
            
            if (_numberEnhancements.TryGetValue((type, level), out var d))
            {
                data = d;
                return true;
            }

            return false;
        }

        public bool TryGetMoveEnhancementData(MoveModifierType type, int level, out MoveEnhancementData? data)
        {
            data = null;

            if (_moveEnhancements.TryGetValue((type, level), out var d))
            {
                data = d;
                return true;
            }

            return false;
        }
    }
}