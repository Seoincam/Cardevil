using Cardevil.Cards.InStage;
using Cardevil.Cards.InStage.Model;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;

namespace Cardevil.Events
{
    /// <summary>
    /// 전투 스테이지에 입장할 때 사용되는 이벤트 인자.
    /// </summary>
    public class EnterStageArgs : ExecEventArgs<EnterStageArgs>
    {
        public TileVector PlayerPosition { get; private set; } 

        public static EnterStageArgs Get(TileVector playerPosition, int playerMaxHand)
        {
            var args = Get();
            args.PlayerPosition = playerPosition;

            return args;
        }

        public enum Orders
        {
            /// <summary>
            /// 적에 대한 설명을 표시함.
            /// </summary>
            ShowEnemyDescription,
            
            /// <summary>
            /// 적의 첫 공격 범위를 표시함.
            /// </summary>
            ShowEnemyInitialAttackArea,
            
            /// <summary>
            /// 카드를 뽑고 리롤하는 뷰를 표시함.
            /// 리롤권이 없을 시, 카드를 뽑고 자동으로 넘어감.
            /// </summary>
            Reroll,
            
            /// <summary>
            /// 리롤 뷰에 있던 카드들을 메인 뷰로 이동 시킴. 
            /// 입장 시의 모든 이벤트가 끝나고 코어 루프로 넘어갈 때 호출됨.
            /// </summary>
            Last = int.MaxValue,
        }
    }
    
    /// <summary>
    /// 플레이어의 공격 시전 이벤트 인자.
    /// </summary>
    public class PlayerAttackArgs : ExecEventArgs<PlayerAttackArgs>
    {
        public EvaluationResult EvaluationResult { get; private set; }

        public static PlayerAttackArgs Get(EvaluationResult evaluationResult)
        {
            var args = Get();
            args.EvaluationResult = evaluationResult;
            return args;
        }

        public enum Orders
        {
            PlayerAttack,
            EnemyAttacked
        }

        public override void Clear()
        {
            base.Clear();
            EvaluationResult = default;
        }
    }

    /// <summary>
    /// 적의 공격 시전 이벤트 인자.
    /// </summary>
    public class EnemyAttackArgs : ExecEventArgs<EnemyAttackArgs>
    {
        public int TotalDamage { get; private set; }

        public void SetDamage(int totalDamage) => TotalDamage = totalDamage;
        
        public enum Orders
        {
            EnemyAttack,
            PlayerAttacked
        }

        public override void Clear()
        {
            base.Clear();
            TotalDamage = 0;
        }
    }
}