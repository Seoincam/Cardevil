using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.NewCard.InStage
{
    public class StageCardCoreView : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;

        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardRemainCountText;

        public event UnityAction UseClicked;
        public event UnityAction DiscardClicked;

        public void Awake()
        {
            useButton.onClick.AddListener(() => UseClicked?.Invoke());
            discardButton.onClick.AddListener(() => DiscardClicked?.Invoke());
        }
        
        public void SetUseButtonState(bool state)
        {
            if (useButton.interactable != state)
            {
                useButton.interactable = state;
            }
        }

        public void SetDiscardButtonState(bool state)
        {
            if (discardButton.interactable != state)
            { 
                discardButton.interactable = state;   
            }
        }
        
        public void SetAllButtonState(bool state)
        {
            SetUseButtonState(state);
            SetDiscardButtonState(state);
        }
    }
}