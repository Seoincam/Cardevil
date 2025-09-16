using UnityEngine;
using Cardevil.Item;
using Cardevil.Item.gold;
using Unity;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
                Debug.Log($"출력해야하는 index :{i}");
                tmpItem = GetRandomTypeItem(i);
                break;
            }
        }

        return tmpItem;
       


    }

    public Item GetRandomTypeItem(int index)
    {
        Define.RareType thisRare = (Define.RareType)index;
        List<Item> getitems = typeLists[index]; // 우선은 index 0~4까지에 따라서 설정하기
        Item item = getitems[UnityEngine.Random.Range(0, getitems.Count)]; // 목록에서 랜덤으로 아이템 뽑아오기
        item.type = thisRare;

        return item;
        
    }


    
    public void SettingItemTypeList()
    {
        // 추후 Data와 연결하여 설정할 수 있도록 유도

        normalList.AddRange(new List<Item> { new Gold(3), new Heal(1),new RandomGold(2,4),new StartReroll(2) } );
        rareList.AddRange(new List<Item> { new Gold(4), new Heal(2), new RandomGold(3,6), new StartReroll(3) });
        epicList.AddRange(new List<Item> { new Gold(7), new RandomGold(6,10), new StartReroll(5),new ExactUpgrade(1) });
        legendList.AddRange(new List<Item> { new Gold(9999999) });




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



}
