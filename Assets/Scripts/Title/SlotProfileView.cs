using Cardevil.Save;
using System;
using TMPro;
using UnityEngine.UI;

namespace Cardevil.Title
{
    [Serializable]
    public class SlotProfileView
    {
        public TextMeshProUGUI dateTimeText;
        public TMP_InputField nameInput;
        public Button deleteButton;
        public Button enterButton;
        public TextMeshProUGUI enterButtonText;

        public void SetDefault()
        {
            dateTimeText.text = "-";
            nameInput.interactable = true;
            deleteButton.gameObject.SetActive(false);
            enterButton.interactable = false;
            enterButtonText.text = "만들기";
        }

        public void SetData(GameSave save)
        {
            dateTimeText.text = save.SaveTime.ToString("yyyy년 MM월 dd일 HH시 mm분");
            nameInput.text = save.Name;
            nameInput.interactable = false;
            deleteButton.gameObject.SetActive(true);
            enterButtonText.text = "이어하기";
        }

        public void OnNameInputChanged(string name)
        {
            // TODO: 이름 유효성 검사 조건 추가
            enterButton.interactable = name.Length is > 0 and <= 10;
        }
        
        public void RemoveAllButtonListeners()
        {
            deleteButton.onClick.RemoveAllListeners();
            enterButton.onClick.RemoveAllListeners();
            nameInput.onValueChanged.RemoveAllListeners();
        }
    }
}
