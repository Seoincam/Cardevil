using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckRemain : MonoBehaviour
{
    [SerializeField] RemainCardVisual remainCardVisualPrefab;
    [SerializeField] Transform cardsPanel;
    [SerializeField] TextMeshProUGUI remainText;

    private RemainCardVisual[] cardVisuals;
    private bool isInitialized = false;
    private bool isOpened = false;

    private int DeckSize => Managers.Card.runtimeBaseDeck.Count;

    private void Init()
    {
        cardVisuals = new RemainCardVisual[DeckSize];

        // 카드 비주얼 소환
        for (int i = 0; i < 50; i++)
        {
            cardVisuals[i] = Instantiate(remainCardVisualPrefab, parent: cardsPanel);
            cardVisuals[i].Init(Managers.Card.runtimeBaseDeck[i]);
        }

        isInitialized = true;
    }

    public void Toggle()
    {
        isOpened = !isOpened;

        if (isOpened) Open();
        else Close();
    }

    private void Open()
    {
        if (!isInitialized)
            Init();

        // TODO: 추후 event로 바뀐 카드만 Update하게 수정!
        foreach (var cardVisual in cardVisuals)
            cardVisual.UpdateVisual();

        remainText.text = $"{Managers.Card.handBar.StageCardsCtx.DeckCount}/50";

        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
