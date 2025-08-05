using UnityEngine;
using System;
using Cardevil.Ingame.Field;


namespace Cardevil.InGame.Enemy
{
    public class Enemy : MonoBehaviour
    {
        private Field field;
        private int damage = 1; // Enemy의 공격력
        private float HP; // Enemy의 체력
        private int attackTurnOrder;
        public int attackCycle = 1; // 일단 기본 3, 몇번 마다 공격이 시행되는지?
        private int attackLineNumber;
        private int attackPointNumber_x;
        private int attackPointNumber_y;
        private bool aWakeFirst = true;

        enum AttackStyle
        {
            UnKnown,
            AttackPoint,
            AttackVertical,
            AttackHorizontal
        }
        private AttackStyle currentAttackStyle;
        private void Start()
        {
           
            field = Managers.Game.field;
            currentAttackStyle = AttackStyle.UnKnown;
            Managers.Game.enemy = this;

        }



        #region Attack 관련

        public void AttackEnemyAwake()
        {
            if (aWakeFirst == true) // 처음에선 랜덤지정
            {
                SetAttackType();
                SetAttackCycle();
                setAttackSign();
                attackTurnOrder++;
            }

        }
        public void AttackEnemyTurnStart()
        {
            AttackEnemyAwake(); // 처음으로 Awake된 상태일때
            aWakeFirst = false;
            // if 맵에 있는 모든 몹의 HP<=인가?
            // -> 다음스테이지로

            // Enemy Turn Order 감소
            attackTurnOrder--;
            Debug.Log($"다음 공격까지 {attackTurnOrder}턴 남았습니다");
            if (attackTurnOrder == 0) // 0 이라면 공격시행
            {
                AttackGo();
                RemoveHighLight();
            }

            AttackEnemyTurnEnd(); // Enemy 공격의 마무리 단계
        }

        void AttackGo()
        {
            Debug.Log($"Enemy Attack! damage : {damage}");
            if (currentAttackStyle == AttackStyle.AttackHorizontal) // 어택 타입에 따라 행동
            {
                AttackHorizontal(attackLineNumber);
            }
            else if (currentAttackStyle == AttackStyle.AttackVertical)
            {
                AttackVerical(attackLineNumber);
            }
            else if (currentAttackStyle == AttackStyle.AttackPoint)
            {
                AttackPoint(attackPointNumber_x, attackPointNumber_y);
            }
        }
        void AttackEnemyTurnEnd()
        {
            if (attackTurnOrder == 0) // 이미 공격을 했다면
            {
                // 새로운 공격 패턴 지정, 어택사이클 지정
                SetAttackType();
                SetAttackCycle();
                setAttackSign();
            }

            // 어택의 방식이 단일 공격이라면  콤보라면, 이 지정이 아니라 특별하게 나오는 순서같은게 지정될 수 있다.

            // 플레이어 턴으로 넘기기
        }

       
        void setAttackSign()
        {
            attackLineNumber = UnityEngine.Random.Range(0, 2); // 랜덤으로 위치 지정
            if (currentAttackStyle == AttackStyle.AttackHorizontal) //가로 또는 세로 공격
            {
                AttackNoticeSign_Horizontal(attackLineNumber);
            }
            else if(currentAttackStyle == AttackStyle.AttackVertical)
            {
                AttackNoticeSign_Vertical(attackLineNumber);
            }
            else if(currentAttackStyle==AttackStyle.AttackPoint)
            {
                // 포인트 랜덤으로 지정
                attackPointNumber_y = UnityEngine.Random.Range(0, 2);
                attackPointNumber_x = attackLineNumber;
                AttackNoticeSign_Point(attackPointNumber_x, attackPointNumber_y);
            }
            else { Debug.Log("currentAttackStyle을 찾지 못한 오류"); }
            Debug.Log($"Attack 예상 sign {currentAttackStyle}");
        }
        void SetAttackType()
        {
            currentAttackStyle = GetRandomAttackStyle(); // 랜덤으로 어택스타일 받기;

        }
        void SetAttackCycle()
        {
            attackTurnOrder = attackCycle; // 몇턴뒤에 공격
        }

        AttackStyle GetRandomAttackStyle() // 랜덤으로 어택스타일 받기
        {
            Array values = Enum.GetValues(typeof(AttackStyle));
            return (AttackStyle)values.GetValue(UnityEngine.Random.Range(1, values.Length));
        }


        void RemoveHighLight()
        {
            if (currentAttackStyle == AttackStyle.AttackHorizontal) // 어택 타입에 따라 행동
            {
                RemoveHighLight_Horizontal(attackLineNumber);
            }
            else if (currentAttackStyle == AttackStyle.AttackVertical)
            {
                RemoveHighLight_Vertical(attackLineNumber);
            }
            else if (currentAttackStyle == AttackStyle.AttackPoint)
            {
                RemoveHighLight_Point(attackPointNumber_x, attackPointNumber_y);
            }
        }

        void RemoveHighLight_Horizontal(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].UnHighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
        }

        void RemoveHighLight_Vertical(int lineNumber)
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].UnHighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
        }
        void RemoveHighLight_Point(int pointNumber_x,int pointNumber_y)
        {
            field[pointNumber_x][pointNumber_y].UnHighLightAttackTile(); // 해당 타일 하이라이트 off
        }
        void AttackNoticeSign_Point(int pointNumber_x,int pointNumber_y)
        {
            field[pointNumber_x][pointNumber_y].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            currentAttackStyle = AttackStyle.AttackPoint; // 포인트 어택 형태로 저장
        }

        void AttackNoticeSign_Vertical(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            
            for(int x=0;x<3;x++) // 가로 0,1,2 에대해
            {
                field[lineNumber][x].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackVertical; // 포인트 어택 형태로 저장
        }

        void AttackNoticeSign_Horizontal(int lineNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][lineNumber].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
            currentAttackStyle = AttackStyle.AttackHorizontal; // 포인트 어택 형태로 저장
        }




        // 실질적인 공격 후 데미지 주기

        void AttackPoint(int pointNumber_x,int poinNumber_y)
        {
            field[pointNumber_x][poinNumber_y].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기


            //Entity중 Player가 있다면

            //데미지 주기

        }

        void AttackVerical(int pointNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // 가로는 0,1,2 모두
            for(int x=0;x<3;x++)
            {
                field[pointNumber][x].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면

                //데미지 주기
                
            }
        }


        void AttackHorizontal(int pointNumber) // 가로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // 가로는 0,1,2 모두
            for (int x = 0; x < 3; x++)
            {
                field[x][pointNumber].GetEntities(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면

                //데미지 주기

            }
        }
        #endregion

        public void GetDamage(float damage)
        {
            HP -= damage;
            if(HP<=0)
            {
                //보스 사망 
                Destroy(this.gameObject);
            }
        }
    }
}
