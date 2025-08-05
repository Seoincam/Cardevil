using UnityEngine;
using Cardevil.Ingame.Field;


namespace InGame.Cardevil.Enemy
{
    public class Enemy : MonoBehaviour
    {
        private Field field;
        private int damage;

        private void Start()
        {
            field = Managers.Game.field;
           
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
                field[pointNumber][x].GetEntity(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면

                //데미지 주기

            }
        }


        void AttackHorizontal(int pointNumber) // 가로 공격 왼쪽부터 pointNumber 0,1,2
        {
            // 가로는 0,1,2 모두
            for (int x = 0; x < 3; x++)
            {
                field[x][pointNumber].GetEntity(); // 찾아보는 타일에 있는 Entity 받아오기

                //Entity중 Player가 있다면

                //데미지 주기

            }
        }

    }
}
