using System;

namespace Cardevil.Cards.Data.InStage
{
    public enum CardKind { Number, Move }

    [Serializable]
    public class InStageCardData
    {
        public int Id { get; }
        public CardKind Kind { get; }
        public BuiltNumberData Number { get; }
        public BuiltMoveData Move { get; }

        private InStageCardData(int id, CardKind kind, BuiltNumberData number, BuiltMoveData move)
        {
            Id = id;
            Kind = kind;
            Number = number;
            Move = move;
        }

        public static InStageCardData FromNumber(int id, BuiltNumberData number) 
            => new(id, CardKind.Number, number, null);

        public static InStageCardData FromMove(int id, BuiltMoveData move)
            => new(id, CardKind.Move, null, move);
    }
}