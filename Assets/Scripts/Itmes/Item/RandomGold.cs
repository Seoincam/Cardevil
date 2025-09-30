using UnityEngine;
using Cardevil.Item;
public class RandomGold : Item
{
    private int goldRangeMin;
    private int goldRangeMax;
    public int getGold;


    public RandomGold(int min,int max)
    {
        goldRangeMin = min;
        goldRangeMax = max;
    }

    public RandomGold()
    {

    }
    public override void OnClicked()
    {
        GetGold(SettingGold());
    }

    private int SettingGold()
    {
        return Random.Range(goldRangeMin, goldRangeMax);
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
    public override Item DeepClone()
    {
        return MemberwiseClone() as Item;
    }
}
