using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cardevil.Card.InStage
{
    public class RerollView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ticketCountText;
        [SerializeField] private Button rerollButton;
        [SerializeField] private Button confirmButton;

        public event UnityAction RerollClicked;
        public event UnityAction ConfirmClicked;

        private void Awake()
        {
            rerollButton.onClick.AddListener(() => RerollClicked?.Invoke());
            confirmButton.onClick.AddListener(() => ConfirmClicked?.Invoke());
        }

        public void SetTicketCount(int count)
        {
            ticketCountText.text = count.ToString();
            if (count <= 0)
            {
                SetRerollButton(false);
            }
        }
        
        public void SetRerollButton(bool interactable) => rerollButton.interactable = interactable;
        public void SetConfirmButton(bool interactable) => confirmButton.interactable = interactable;
    }
}