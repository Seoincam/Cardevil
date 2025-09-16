using UnityEngine;
using Cardevil.Item;

namespace Cardevil.Item.gold
{
    public class Gold : Item
    {
        private int goldRangeMin;
        private int goldRangeMax;
        private int getgold;


        /// <summary>
        /// 선택되었을때 발동하는 함수
        /// </summary>
        override public void IsClicked()
        {
            // 골드 획득하는 UI 띄우기
        }


        /// <summary>
        /// 골드 획득 관련 코드
        /// </summary>
        /// <param name="income"></param>
        public void GetGold(int income)
        {

        }
    }

}