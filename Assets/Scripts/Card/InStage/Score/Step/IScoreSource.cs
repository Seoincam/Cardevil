namespace Cardevil.Card.InStage.Score.Step
{
    public interface IScoreSource { }

    public interface IScoreProvider : IScoreSource
    {
        int Id { get; set; }
        ScoreStepType ScoreStepType { get; }
        IScoreOperator GetScoreOperator(IScoreContext context);
    }
    
    public enum ScoreStepType : byte
    {
        /// <summary>
        /// 선택된 카드들 중, 왼쪽에 위치한 카드부터 1장씩 사용 처리.
        /// </summary>
        EachCard,
        
        /// <summary>
        /// 내부 발동 순서에 따라, 조건에 맞는 합연산 유물 처리.
        /// </summary>
        PlusRelic,
        
        /// <summary>
        /// 칸 조건에 맞는 합연산 처리.
        /// </summary>
        PlusField,
        
        /// <summary>
        /// 플레이어 상태창에 아이콘으로 존재하는 합연산을 처리.
        /// </summary>
        PlusPlayerStatus,
        
        /// <summary>
        /// 카드의 최종 데미지 증폭률에 따라 곱연산을 처리.
        /// </summary>
        MultiplyCardFinalDamage,
        
        /// <summary>
        /// 내부 발동 순서에 따라, 조건에 맞는 곱연산 유물 처리.
        /// </summary>
        MultiplyRelic,
        
        /// <summary>
        /// 칸 조건에 맞는 곱연산 처리.
        /// </summary>
        MultiplyField,
        
        /// <summary>
        /// 플레이어 상태창에 아이콘으로 존재하는 곱연산을 처리.
        /// </summary>
        MultiplyPlayerStatus,
        
        /// <summary>
        /// 적 상태창에 아이콘으로 존재하는 기믹을 처리.
        /// </summary>
        EnemyStatus,
    }
}