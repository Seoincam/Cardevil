using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.Interactions;
using System.Threading.Tasks;

public class BlueFlushChoice : MonoBehaviour
{
    public UniTaskCompletionSource BlueFlushCmp { get; private set; }

    private bool isInitalized = false;
    private CardHandBar handBar;

    [SerializeField] Button addDiscardRemainCountButton;
    [SerializeField] Button reviveCardButton;

    public void GetSet(CardHandBar handBar)
    {
        if (!isInitalized)
        {
            addDiscardRemainCountButton.onClick.AddListener(AddDiscardRemainCount);
            reviveCardButton.onClick.AddListener(ReviveCard);
            this.handBar = handBar;
            isInitalized = true;
        }

        BlueFlushCmp = new();
        addDiscardRemainCountButton.interactable = true;
        reviveCardButton.interactable = true;
        gameObject.SetActive(true);
    }

    private void AddDiscardRemainCount()
    {
        addDiscardRemainCountButton.interactable = false;
        reviveCardButton.interactable = false;

        handBar.StageCardsCtx.IncreaseDiscardCount(3);
        BlueFlushCmp.TrySetResult();
        gameObject.SetActive(false);
    }

    private void ReviveCard()
    {
        addDiscardRemainCountButton.interactable = false;
        reviveCardButton.interactable = false;
        _ = ReviveCardAsync();    
    }

    private async Task ReviveCardAsync()
    {
        await handBar.Revive(3);
        BlueFlushCmp.TrySetResult();
        gameObject.SetActive(false);
    }
}
