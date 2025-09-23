using UnityEngine;
using DG.Tweening;
using Cardevil.Item;
using Cardevil.Item.gold;
using UnityEngine.UI;
using Database;
using Database.Generated;

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
        SettingItem(probList);
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();

        shop target= dbManager.Database.shopList[0];
        SetData(target);
    }


    public void SettingItem(int[] probList)
    {
        item = Managers.Item.GetRandomItem(probList);
    }

    #region tool
    // Slot.cs 파일의 SetData 함수를 아래 내용으로 교체해주세요.

    public void SetData(shop itemData)
    {
        // 1. DatabaseManager 인스턴스 찾기
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();
        if (dbManager == null)
        {
            Debug.LogError("⛔️ 씬에서 DatabaseManager를 찾을 수 없습니다!");
            return;
        }

        // 2. 아이템 이름 등 텍스트 정보 설정
        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        // 3. 아이템의 URL이 비어있지 않은지 확인
        string keyURL = itemData.URL?.Trim();
        if (!string.IsNullOrEmpty(keyURL))
        {
            // 4. SpriteCache에서 URL을 키로 사용해 Sprite를 '조회'
            if (dbManager.SpriteCache.TryGetValue(keyURL, out Sprite iconSprite))
            {
                Debug.Log($"✅ 스프라이트 조회 성공! URL: {keyURL}");

                if (itemIconImage == null)
                {
                    Debug.LogError("⛔️ itemIconImage 변수가 Inspector에 할당되지 않았습니다! 슬롯 프리팹을 확인하세요.");
                    return;
                }

                itemIconImage.sprite = iconSprite;
                itemIconImage.enabled = true;
                itemIconImage.color = Color.white; // 색상을 흰색(불투명)으로 강제 설정
            }
            else
            {
                // 여기가 실행되는 경우
                Debug.LogError($"⛔️ 스프라이트 로드 실패! SpriteCache에 해당 URL이 없습니다. URL: {keyURL}");

                // 현재 캐시에 어떤 URL들이 들어있는지 모두 출력해서 비교해봅니다.
                Debug.Log($"현재 캐시된 아이템 수: {dbManager.SpriteCache.Count}");
                foreach (var key in dbManager.SpriteCache.Keys)
                {
                    Debug.Log($"-> 캐시에 저장된 URL: {key}");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 이 아이템 데이터에는 URL 정보가 없습니다.");
            if (itemIconImage != null) itemIconImage.enabled = false;
        }
    }
    #endregion
}
