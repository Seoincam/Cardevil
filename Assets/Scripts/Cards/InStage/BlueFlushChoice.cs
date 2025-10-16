using Cardevil.Cards.InStage.Presenter;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Cardevil.Cards.InStage
{
    public class BlueFlushChoice : MonoBehaviour
    {
        public UniTaskCompletionSource BlueFlushCmp { get; private set; }

        private bool isInitalized = false;
        private StageCardsPresenter handBar;

        [SerializeField] Button addDiscardRemainCountButton;
        [SerializeField] Button reviveCardButton;

        public void GetSet(StageCardsPresenter handBar)
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

            // Managers.Card.StageCardsCtx.IncreaseDiscardCount(3);
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
}

