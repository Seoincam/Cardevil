using Cardevil.Cards.InStageData;
using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Modifiers.Move
{
    public class MoveModifierPipeline
    {
        private readonly List<IMoveModifier> _mods = new();

        public void Add(IMoveModifier mod)
        {
            _mods.Add(mod);
        }

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
    /// Move Modifier들이 수정할 수 있는 빌드 중간 상태 컨텍스트
    /// </summary>
    public struct MoveBuildContext
    {
        public int length;
        public List<Direction?> Selectables;
    }
}