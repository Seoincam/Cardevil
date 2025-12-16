using Cardevil.Core.Bootstrap;
using UnityEngine;
using Cardevil.Item;
using Cardevil.Item.gold;
using Unity;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Database;
using Database.Generated;

public class ItemManager
{
    public List<List<Item>> typeLists = new List<List<Item>>();
    public List<Item> normalList = new List<Item>();
    public List<Item> rareList = new List<Item>();
    public List<Item> epicList = new List<Item>();
    public List<Item> legendList = new List<Item>();



    public void Init()
    {
        SettingItemTypeList();
    }
     public Item GetRandomItem(int[] probList)
    {
        Item tmpItem = null;
        float totalAmount = probList.Sum();
        float[] probFloatList = new float[probList.Length];
        for(int i=0;i<probList.Length;i++)
        {
            probFloatList[i] = (probList[i] / totalAmount)*100;
        }
        float randomFloat = UnityEngine.Random.Range(0f, 100f);

        float currentProb = 0f;
        for (int i = 0; i < probFloatList.Length; i++)
        {
            currentProb += probFloatList[i];
            if (randomFloat < currentProb)
            {
                tmpItem = GetRandomTypeItem(i);
                break;
            }
        }

        return tmpItem;
       


    }

    public Item GetRandomTypeItem(int index)
    {
        Define.RareType thisRare = (Define.RareType)index;
  


        // 이것들을 json에서 가져오기.
        List<MachineReward> filteredList = Bootstrapper.Instance.Database.Database.MachineRewardList
                                .Where(item => item.Rank == thisRare)
                                .ToList();

       
        // 가중치 랜덤 추첨 로직
        
        // 필터링된 아이템들의 모든 확률값(가중치)의 총합을 계산
        int totalWeight = filteredList.Sum(item => item.ItemProbability);

        // 0부터 totalWeight 전까지의 랜덤 숫자를 하나 뽑rl
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // 리스트를 순회하면서 랜덤 숫자를 줄여나가다 0보다 작아지는 순간의 아이템을 선택
        MachineReward selectedItem = null;
        foreach (var item in filteredList)
        {
            randomValue -= item.ItemProbability;
            if (randomValue <= 0)
            {
                selectedItem = item;
                break; // 아이템을 찾았으니 반복 종료
            }
        }

        // 이제 selectedItem 변수에 가중치에 따라 랜덤하게 뽑힌 아이템이 들어있습니다.

        if(selectedItem==null)
        {
            Debug.Log("SelectedItem이 null입니다");
            return null;
        }

        Item itemReturn = CreateItemByItemType(selectedItem);


        return itemReturn;
    }


    
    public void SettingItemTypeList()
    {
        // 추후 Data와 연결하여 설정할 수 있도록 유도

        normalList.AddRange(new List<Item> { new FixedGold(3), new Heal(1),new RandomGold(2,4),new StartReroll(2) } );
        rareList.AddRange(new List<Item> { new FixedGold(4), new Heal(2), new RandomGold(3,6), new StartReroll(3) });
        epicList.AddRange(new List<Item> { new FixedGold(7), new RandomGold(6,10), new StartReroll(5),new ExactUpgrade(1) });
        legendList.AddRange(new List<Item> { new FixedGold(9999999) });




        // 리스트에 넣어두기

        //현재 이 순서 중요. 추후에 data에 따라 넣는 코드 추가 필요
        typeLists.Add(normalList);
        typeLists.Add(rareList);
        typeLists.Add(epicList);
        typeLists.Add(legendList);
        Debug.Log($"tpyeLists의 크기 {typeLists.Count}");

    }

    /// <summary>
    /// 암시장을 조우한뒤 반드시 호출해주세요.
    /// </summary>
    public void SettingDarkUpgradeListAdd()
    {
        normalList.Add(new DarkUprade());
        rareList.Add(new DarkUprade());
        epicList.Add(new DarkUprade());
    }

    private Item CreateItemByItemType(MachineReward machineReward)
    {
        Item item;
        Define.SlotRewardType type = machineReward.ItemName;
        switch(type)
        {
            case Define.SlotRewardType.Heal: item = new Heal();
                break;
            case Define.SlotRewardType.RandomGold:
                item = new RandomGold(int.Parse(machineReward.Comment),machineReward.Value);
                
                break;
            case Define.SlotRewardType.FixedGold:
                item = new FixedGold(machineReward.Value);
                break;
            case Define.SlotRewardType.ExactUpgrade:
                item = new ExactUpgrade(machineReward.Value);
                break;
            case Define.SlotRewardType.StartReroll:
                item = new StartReroll(machineReward.Value);
                break;
            case Define.SlotRewardType.DarkUpgrade:
                item = new DarkUprade(machineReward.Value);
                break;
            case Define.SlotRewardType.Relic:
                item = new Relics();
                break;
            default: return null;
        }

        item.itemName = machineReward.ItemName.ToString();
        item.rareType = machineReward.Rank;
        item.macinRewardData = machineReward;

        return item;
    }

}
