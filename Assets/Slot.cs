using UnityEngine;
using DG.Tweening;
using Cardevil.Item;
using Cardevil.Item.gold;
using UnityEngine.UI;
using Database;
using Database.Generated;
using System.Linq;

public class Slot : MonoBehaviour
{
    public Item item; // 슬롯에 등장할 아이템
    public Image itemIconImage;
    public Text itemNameText;

    public GameObject slotItem;

    [Header("Animation Settings")]
    public float startYPosition = 500f;  // 떨어질 Y위치
    public float dropDuration = 0.8f;    // 떨어지기 시작하는 시간
    public float floatStrength = 10f;    // 얼마나 세게 떨어지는지
    public float floatDuration = 1.5f;   // 떨어지는 시간
    public void SettingSlot(int[] probList)
    {
        // 슬롯 아이템이 활성화 상태인지 확인
        slotItem.SetActive(true);

        RectTransform rt = slotItem.GetComponent<RectTransform>();

        // 1. 이전 애니메이션을 모두 중지하고 시작 위치로 리셋
        rt.DOKill();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

        // 2. 낙하 및 바운스 애니메이션 실행
        rt.DOAnchorPosY(0, dropDuration)
          .SetEase(Ease.OutBounce);


        // 아이템을 설정
        Item itemtmp = SettingItem(probList);
        item = itemtmp;
        itemNameText.text = itemtmp.itemName;

        SetData(itemtmp);
    }


    public Item SettingItem(int[] probList)
    {
        item = Managers.Item.GetRandomItem(probList);
        return item;
    }

    #region tool

    public void SetData(Item findItem)
    {
        DatabaseManager database = Managers.Game._database;


        Debug.Log("이미지 로딩중입니다");
        // 이미지 로딩
        if (database.TryGetSprite(findItem.macinRewardData.URL, out Sprite loadedSprite))
        {
            // 성공! 찾은 스프라이트를 Image 컴포넌트에 적용
            itemIconImage.sprite = loadedSprite;
            itemIconImage.color = Color.white; // 이미지가 보이도록 색상 조절
            Debug.Log("이미지 로딩 성공");
        }
        else
        {
            // 실패. URL이 없거나, 로딩에 실패한 경우
            // 여기에 '이미지 없음' 기본 아이콘을 설정하는 등의 예외 처리를 할 수 있습니다.
            itemIconImage.sprite = null; // 또는 defaultIconSprite;
            itemIconImage.color = Color.clear; // 이미지가 없으면 투명하게
            Debug.LogWarning($"아이템 '{findItem.macinRewardData.URL}'의 이미지를 캐시에서 찾을 수 없습니다. URL: {findItem.macinRewardData.URL}");
        }
    }
    #endregion
}
