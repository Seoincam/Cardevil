using UnityEngine;
using Cardevil.Item;

namespace Cardevil.Item.gold
{
    public class Gold : Item
    {
        private int goldRangeMin;
        private int goldRangeMax;
        public int getGold;


        public Gold(int goldAmount)
        {
            getGold = goldAmount;
        }

        /// <summary>
        /// 선택되었을때 발동하는 함수
        /// </summary>
        override public void IsClicked()
        {
            // 골드 획득하는 UI 띄우기
            GetGold(getGold);
        }

        private void SettingGold()
        {
            getGold = Random.Range(goldRangeMin, goldRangeMax);
        }

        /// <summary>
        /// 골드 획득 관련 코드
        /// </summary>
        /// <param name="income"></param>
        public void GetGold(int income)
        {
            Managers.Game.PlayerStatus.gold += income;
            Debug.Log($"income :{income} 획득");
        }
    }

}