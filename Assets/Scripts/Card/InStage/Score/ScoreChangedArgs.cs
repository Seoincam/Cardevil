using Cardevil.Events.ExecEvents;

namespace Cardevil.Card.InStage.Score
{
    /// <summary>
    /// 점수 계산 과정 중 점수가 바뀔 때마다 호출되는 이벤트의 인자.
    /// </summary>
    public class ScoreChangedArgs : ExecEventArgs<ScoreChangedArgs>
    {
        public float PreviousScore { get; private set; }
        public float CurrentScore { get; private set; }

        public static ScoreChangedArgs Get(float previousScore, float currentScore)
        {
            var args = Get();
            args.PreviousScore = previousScore;
            args.CurrentScore = currentScore;

            return args;
        }

        public override void Clear()
        {
            base.Clear();
            PreviousScore = 0;
            CurrentScore = 0;
        }
    }
}