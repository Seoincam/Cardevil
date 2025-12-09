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
    private float startYPosition = 400f;   // 떨어질 Y위치 (시작점)
    private float endYPosition = -400f;    // 지나갈 Y위치 (끝점 - 회전 효과용)
    private float dropDuration = 0.5f;     // 최종 드롭 시간 (바운스 포함)
    private float spinSpeed = 0.15f;        // 회전 시 아이템 하나가 지나가는 속도

    // 회전 애니메이션용 트윈 저장
    private Tween spinTween;

    /// <summary>
    /// 슬롯 회전을 시작합니다 (결과가 나오기 전 대기 상태).
    /// </summary>
    public void StartSpinning(int[] probList)
    {
        slotItem.SetActive(true);
        RectTransform rt = slotItem.GetComponent<RectTransform>();

        // 기존 애니메이션 제거
        rt.DOKill();

        // 위치 초기화
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

        // 무한 반복 회전 애니메이션
        // 위에서 아래로 빠르게 이동하며, 한 사이클이 끝나면 다시 위로 올리고 이미지를 바꿉니다.
        spinTween = rt.DOAnchorPosY(endYPosition, spinSpeed)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart) // 무한 반복
            .OnStepComplete(() =>
            {
                // 한 바퀴 돌 때마다(Step 완료 시) 랜덤 아이템 이미지를 잠깐 보여줌
                // 실제로 아이템 객체를 바꾸진 않고 이미지만 교체하여 시각적 효과만 줍니다.
                Item randomTempItem = Managers.Item.GetRandomItem(probList);
                SetData(randomTempItem);
            });
    }

    /// <summary>
    /// 회전을 멈추고 최종 아이템을 보여줍니다.
    /// </summary>
    public void SettingSlot(int[] probList, Item itemtmp)
    {
        // 슬롯 아이템이 활성화 상태인지 확인
        item = itemtmp;
        slotItem.SetActive(true);

        RectTransform rt = slotItem.GetComponent<RectTransform>();

        // 1. 이전 애니메이션(회전 루프)을 모두 중지하고 시작 위치로 리셋
        if (spinTween != null) spinTween.Kill();
        rt.DOKill();

        // 회전 중 이미지가 바뀌었을 수 있으므로, 최종 확정된 아이템 데이터로 세팅
        SetData(itemtmp);

        // 확정된 아이템이 위에서 툭 떨어지는 연출 시작
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startYPosition);

        // 2. 낙하 및 바운스 애니메이션 실행
        rt.DOAnchorPosY(0, dropDuration)
          .SetEase(Ease.OutBounce);
    }




    #region tool

    public void SetData(Item findItem)
    {

        if (findItem == null) return; // 예외처리

        DatabaseManager database = Managers.Game._database;

        // 이미지 로딩
        if (Managers.Database.TryGetSprite(findItem.macinRewardData.URL, out Sprite loadedSprite))
        {
            // 성공! 찾은 스프라이트를 Image 컴포넌트에 적용
            itemIconImage.sprite = loadedSprite;
            itemIconImage.color = Color.white; // 이미지가 보이도록 색상 조절
            // Debug.Log("이미지 로딩 성공");
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