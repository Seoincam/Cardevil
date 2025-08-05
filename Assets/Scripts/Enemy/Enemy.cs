using UnityEngine;
using Cardevil.Ingame.Field;


namespace InGame.Cardevil.Enemy
{
    public class Enemy : MonoBehaviour
    {
        private Field field;
        private int damage; // Enemy의 공격력
        private float HP; // Enemy의 체력
        private int attackTurnOrder;

        private void Start()
        {
            field = Managers.Game.field;
           
        }



        #region Attack 관련

        void AttackEnemyTurn()
        {
            if()
        }

        void AttackNoticeSing_Point(int pointNumber_x,int poinNumber_y)
        {
            field[pointNumber_x][poinNumber_y].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
        }

        void AttackNoticeSign_Vertical(int pointNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            
            for(int x=0;x<3;x++) // 가로 0,1,2 에대해
            {
                field[pointNumber][x].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
        }

        void AttackNoticeSign_Horizontal(int pointNumber) // 세로 공격 왼쪽부터 pointNumber 0,1,2
        {
            for (int x = 0; x < 3; x++) // 가로 0,1,2 에대해
            {
                field[x][pointNumber].HighLightAttackTile(); // 해당 타일을 하이라이트하기.
            }
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
