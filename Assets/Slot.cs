using UnityEngine;
using DG.Tweening;
using Cardevil.Item;

public class Slot : MonoBehaviour
{
    public Item item; // 슬롯에 등장할 아이템

    public GameObject slotItem;

    [Header("Animation Settings")]
    public float startYPosition = 500f;  // 떨어질 Y위치
    public float dropDuration = 0.8f;    // 떨어지기 시작하는 시간
    public float floatStrength = 10f;    // 얼마나 세게 떨어지는지
    public float floatDuration = 1.5f;   // 떨어지는 시간
    public void SettingSlot()
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
    }

 
}
