namespace Cardevil.UI
{
    /// <summary>
    /// 원형 UI 요소 인터페이스.
    /// 원 좌표계를 사용하는 것에 붙여도 상관 없는듯
    /// </summary>
    public interface ICircularUI
    {
        public void Initialize(float startAngle, float endAngle, bool clockwise);
        // public float AbsAngle { get; }
        public float Value { get; }
    }

}