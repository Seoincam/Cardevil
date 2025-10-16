using System;

namespace Cardevil.Cards.InStageData 
{
    /*
     * 값이 어떤 타입인지
     * -> 단순히 두개이므로 bool로 처리하자
     * 
     * 사용이 가능한지
     * -> 일단 타입으로 분기
     * 1. Selectables가 하나인 경우 : 그냥 그 값을 Final로 제공
     * 2. 여러개인 경우 : 일단 null로 둠.
     * Final이 null인지 아닌지로 CanUse 제공하면 된다
     *
     * 잠겼는지 : 간단히 구현하면 됨
     */
    
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