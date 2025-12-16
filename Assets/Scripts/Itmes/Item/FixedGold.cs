using Cardevil.Core.Bootstrap;
using UnityEngine;
using Cardevil.Item;

namespace Cardevil.Item.gold
{
    public class FixedGold : Item
    {
        private int goldRangeMin;
        private int goldRangeMax;
        public int getGold;


        public FixedGold(int goldAmount)
        {
            getGold = goldAmount;
        }
        
        public FixedGold()
        {

        }

        /// <summary>
        /// 선택되었을때 발동하는 함수
        /// </summary>
        override public void OnClicked()
        {
            // 골드 획득하는 UI 띄우기
            Debug.Log(this.itemName);
            Managers.UI.ClosePopUpUI();
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
            // Bootstrapper.Instance.Game.PlayerStatus.gold += income;
            Debug.Log($"income :{income} 획득");
        }
        
        public override Item DeepClone()
        {
            return MemberwiseClone() as Item;
        }
    }

}