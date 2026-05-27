using TMPro;
using UnityEngine;

namespace Cardevil.Card.InWorld.UI
{
    public class CardDescriptionTooltip : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Settings")]
        [SerializeField, Range(0.1f, 3f)] private float scale = 1.0f;
        
        private void OnValidate()
        {
            if (rootObject != null)
            {
                rootObject.transform.localScale = Vector3.one * scale;
            }
        }

        public void Show(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
            {
                Hide();
                return;
            }

            if (rootObject != null) rootObject.SetActive(true);
            
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = content;
        }

        public void Hide()
        {
            if (rootObject != null) rootObject.SetActive(false);
        }
    }
}
