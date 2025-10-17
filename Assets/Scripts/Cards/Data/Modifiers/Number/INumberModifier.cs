namespace Cardevil.Cards.Data.Modifiers.Number
{
    /// <summary>
    /// 숫자 카드에 적용되는 Modifier의 기본 인터페이스.  
    /// 각 Modifier는 NumberBuildContext를 수정하는 Apply 메서드를 구현해야 함.
    /// </summary>
    public interface INumberModifier
    {
        /// <summary>Modifier의 유형.</summary>
        NumberModifierType Type { get; }

        /// <summary>
        /// 주어진 빌드 컨텍스트를 수정하여 카드 수치에 변화를 적용.
        /// </summary>
        /// <param name="ctx">변경 대상 NumberBuildContext (ref로 전달됨)</param>
        void Apply(ref NumberBuildContext ctx);
    }
    
    /// <summary>
    /// 숫자 카드에 적용 가능한 Modifier(강화/효과)의 종류를 정의.
    /// </summary>
    public enum NumberModifierType
    {
        /// <summary>카드 색상 변경</summary>
        Color,
        /// <summary>선택 가능한 값 추가</summary>
        Selectable,
        /// <summary>선택 가능한 값 확정</summary>
        SelectableConfirm,
        /// <summary>데미지 배율 증가</summary>
        Damage,
    }
}