using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;

public class SlotMachine : UI_Popup
{
    [Header("UI 설정")]
    public GameObject probabilityPanel;
    [Tooltip("각 박스 이미지 사이의 간격(픽셀)")]
    public float spacing = 10f; // 박스 사이의 간격을 조절할 수 있는 변수 추가

    [Header("비율을 계산할 수치 데이터")]
    public List<float> values = new List<float> { 75, 50, 125, 250 };

    [Header("크기를 조절할 UI 이미지들")]
    public List<Image> boxImages;

    public List<Slot> slots;

    private bool isSetting = false;

    private float dropTiming = 0.4f;
    void Start()
    {
        UpdateLayout();
        Init();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SettingSlots());
        }
    }



    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(ItemButtons));

        GetButton((int)ItemButtons.Item_1).gameObject.AddUIEvent(OnItem1Clicked);
        GetButton((int)ItemButtons.Item_2).gameObject.AddUIEvent(OnItem2Clicked);
        GetButton((int)ItemButtons.Item_3).gameObject.AddUIEvent(OnItem3Clicked);
    }


    #region Slot에 관련
    
    IEnumerator SettingSlots()
    {
        if (isSetting) { yield break; }
        isSetting = true;
        foreach (var slot in slots)
        {
            slot.SettingSlot();
            yield return new WaitForSecondsRealtime(dropTiming);
        }
        isSetting = false;
        yield return null;
    }
    #endregion

    #region Probability
    /// <summary>
    /// 확률에 따른 Box 위치 조정
    /// </summary>
    public void UpdateLayout()
    {
        // 유효성 검사: 이미지나 값이 없으면 실행하지 않음
        if (boxImages == null || boxImages.Count == 0 || values.Count == 0) return;

        RectTransform containerRect = probabilityPanel.GetComponent<RectTransform>();
        float containerWidth = containerRect.rect.width;

        float totalValue = values.Sum();
        if (totalValue <= 0)
        {
            Debug.LogError("값의 총합이 0보다 커야 합니다.");
            return;
        }

        // *** 변경된 부분 시작 ***

        // 1. 전체 간격의 총합을 계산합니다.
        // 박스가 4개면 간격은 3개가 됩니다. (박스 개수 - 1)
        int gapCount = boxImages.Count > 1 ? boxImages.Count - 1 : 0;
        float totalSpacing = spacing * gapCount;

        // 2. 전체 컨테이너 너비에서 간격의 총합을 빼서 박스들이 실제 사용할 수 있는 너비를 계산합니다.
        float widthForBoxes = containerWidth - totalSpacing;

        // *** 변경된 부분 끝 ***

        float currentX = 0f;

        for (int i = 0; i < boxImages.Count; i++)
        {
            if (i >= values.Count) break;

            RectTransform boxRect = boxImages[i].GetComponent<RectTransform>();

            float proportion = values[i] / totalValue;
            // *** 변경된 부분: containerWidth 대신 widthForBoxes를 기준으로 너비를 계산합니다. ***
            float boxWidth = widthForBoxes * proportion;

            boxRect.anchorMin = new Vector2(0, 0.5f);
            boxRect.anchorMax = new Vector2(0, 0.5f);
            boxRect.pivot = new Vector2(0, 0.5f);

            boxRect.anchoredPosition = new Vector2(currentX, 0);
            boxRect.sizeDelta = new Vector2(boxWidth, containerRect.rect.height);

            // *** 변경된 부분: 다음 위치로 이동할 때, 박스 너비와 간격을 함께 더해줍니다. ***
            currentX += boxWidth + spacing;
        }
    }

    #endregion

    #region Tool

    private void GetItemRandom()
    {

    }

    enum ItemButtons
    {
        Item_1,
        Item_2,
        Item_3
    }
    private void OnItem1Clicked(PointerEventData eventData)
    {
        int index = 0;
        slots[index].item.IsClicked();
    }

    private void OnItem2Clicked(PointerEventData eventData)
    {
        int index = 0;
        slots[index].item.IsClicked();
    }

    private void OnItem3Clicked(PointerEventData eventData)
    {
        int index = 0;
        slots[index].item.IsClicked();
    }
    #endregion
}