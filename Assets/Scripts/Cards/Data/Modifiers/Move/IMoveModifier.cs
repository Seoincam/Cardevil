namespace Cardevil.Cards.Data.Modifiers.Move
{
    /// <summary>
    /// 이동 카드의 Modifier(강화 효과)를 정의하는 인터페이스.  
    /// 각 Modifier는 <see cref="MoveBuildContext"/>를 참조하여 카드의 방향 데이터를 수정함.
    /// </summary>
    public interface IMoveModifier
    {
        /// <summary>Modifier의 유형.</summary>
        MoveModifierType Type { get; }

        /// <summary>
        /// 주어진 빌드 컨텍스트를 수정하여 이동 카드의 데이터를 변경.
        /// </summary>
        /// <param name="ctx">변경 대상 <see cref="MoveBuildContext"/> (ref로 전달)</param>
        void Apply(ref MoveBuildContext ctx);
    }
    
    /// <summary>
    /// 이동 카드에 적용 가능한 Modifier(강화/효과)의 종류를 정의.
    /// </summary>
    public enum MoveModifierType
    {
        /// <summary>선택 가능한 방향 슬롯을 추가.</summary>
        Selectable,
        
        /// <summary>선택 가능한 방향 슬롯의 값을 확정.</summary>
        SelectableConfirm,
    }
}