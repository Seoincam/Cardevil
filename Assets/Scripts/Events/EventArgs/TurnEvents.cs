using Cardevil.Cards.InStage.Model;
using Cardevil.Events.ExecEvents;
using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Events
{
    public class EnterStageArgs : ExecEventArgs<EnterStageArgs>
    {
        public TileVector PlayerPosition { get; private set; } 
        public int PlayerMaxHand { get; private set; }

        public static EnterStageArgs Get(TileVector playerPosition, int playerMaxHand)
        {
            var args = Get();
            args.PlayerPosition = playerPosition;
            args.PlayerMaxHand = playerMaxHand;

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
            /// 입장 시의 모든 이벤트가 끝나고 메인 루프로 넘어갈 때 호출됨.
            /// </summary>
            Last = int.MaxValue,
        }
    }
    
    public class PlayerAttackArgs2 : ExecEventArgs<PlayerAttackArgs2>
    {
        public EvaluationResult EvaluationResult { get; private set; }

        public static PlayerAttackArgs2 Get(EvaluationResult evaluationResult)
        {
            var args = Get();
            args.EvaluationResult = evaluationResult;
            return args;
        }

        public enum Order
        {
            PlayerAttackMotion,
            EnemyAttackedMotion
        }

        public override void Clear()
        {
            base.Clear();
            EvaluationResult = default;
        }
    }

    public class EnemyAttackArgs : ExecEventArgs<EnemyAttackArgs>
    {
        public enum Order
        {
            EnemyAttackMotion,
            PlayerAttackedMotion
        }
    }
}