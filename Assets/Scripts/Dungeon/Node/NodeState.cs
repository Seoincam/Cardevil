namespace Cardevil.Dungeon
{
    /// <summary>
    /// 던전 노드의 상태를 나타내는 열거형
    /// </summary>
    public enum NodeState
    {
        /// <summary>
        /// 잠긴 상태 - 아직 접근할 수 없음
        /// </summary>
        Locked,
        /// <summary>
        /// 사용 가능 - 플레이어가 선택할 수 있음
        /// </summary>
        Available,
        /// <summary>
        /// 현재 위치 - 플레이어가 현재 있는 노드
        /// </summary>
        Current,
        /// <summary>
        /// 완료됨 - 이미 클리어한 노드
        /// </summary>
        Completed,
        /// <summary>
        /// 숨겨진 상태 - UI에 표시되지 않음
        /// </summary>
        Hidden,
        /// <summary>
        /// 지나친 상태 - 선택할 수 있었지만, 현재는 선택할 수 없음
        /// </summary>
        Passed,
    }
}
