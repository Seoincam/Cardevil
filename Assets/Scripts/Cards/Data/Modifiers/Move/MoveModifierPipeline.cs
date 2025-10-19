using Cardevil.Cards.Data.InStage;
using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 이동(Move) 카드의 Modifier들을 관리,
    /// 이를 순차적으로 적용하여 최종 이동 데이터를 생성하는 파이프라인 클래스.
    /// </summary>
    public class MoveModifierPipeline
    {
        private readonly List<IMoveModifier> _mods = new();
        private readonly List<MoveEnhancementData> _possibleEnhancements = new();
        private MoveEnhancementData? _currentEnhancement = null;
        
        public IReadOnlyList<IMoveModifier> Modifiers => _mods;
        public IReadOnlyList<MoveEnhancementData> PossibleEnhancements => _possibleEnhancements;
        public MoveEnhancementData? CurrentEnhancement => _currentEnhancement;
        
        /// <summary>
        /// 이동 Modifier를 파이프라인에 추가.
        /// </summary>
        public void AddModifier(IMoveModifier mod)
        {
            _mods.Add(mod);
        }

        public void SetCurrentEnhancement(MoveEnhancementData enhancement)
        {
            _currentEnhancement = enhancement;
        }

        public void SetPossibleEnhancements(params MoveEnhancementData[] enhancements)
        {
            _possibleEnhancements.Clear();
            foreach (var enhancement in enhancements)
                _possibleEnhancements.Add(enhancement);
        }

        /// <summary>
        /// 등록된 이동 Modifier들을 순차적으로 적용하여 최종 <see cref="BuiltMoveData"/>를 생성.
        /// </summary>
        /// <remarks>
        /// 1. Selectable 타입 Modifier를 먼저 적용하여 선택 가능한 방향 슬롯을 생성.<br/>
        /// 2. 이후 SelectableConfirm 타입 Modifier를 적용하여 슬롯 내 값을 확정.
        /// </remarks>
        /// <returns>모든 Modifier 적용이 완료된 이동 데이터</returns>
        public BuiltMoveData Build()
        {
            var ctx = new MoveBuildContext { length = 1, Selectables = new() };

            foreach (var mod in _mods)
            {
                if (mod.Type == MoveModifierType.Selectable)
                    mod.Apply(ref ctx);
            }

            foreach (var mod in _mods)
            {
                if (mod.Type == MoveModifierType.SelectableConfirm)
                    mod.Apply(ref ctx);
            }
            
            return new BuiltMoveData(ctx.length, ctx.Selectables);
        }
    }
    
    /// <summary>
    /// Move Modifier들이 수정할 수 있는 빌드 중간 상태 컨텍스트.
    /// </summary>
    public struct MoveBuildContext
    {
        public int length;
        public List<Direction?> Selectables;
    }
}