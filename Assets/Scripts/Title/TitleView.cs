using Cardevil.Core.Bootstrap;
using Cardevil.DataStructure.Serializables;
using Cardevil.Save;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Title
{
    public class TitleView : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private Button profileButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject dim;
        [SerializeField] private GameObject profilePanel;

        [Space] 
        [SerializeField] private SerializableDictionary<SaveSlot, ProfileSlot> profileSlots;

        private SaveLoadManager _saveLoad;
        

        private void Awake()
        {
            _saveLoad = Bootstrapper.Instance.SaveLoad;
            
            profileButton.onClick.AddListener(OnProfileClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnProfileClicked()
        {
            foreach (SaveSlot slot in Enum.GetValues(typeof(SaveSlot)))
                UpdateProfile(slot);

            dim.SetActive(true);
            profilePanel.SetActive(true);
        }

        private void OnCloseClicked()
        {
            dim.SetActive(false);
            profilePanel.SetActive(false);
        }


        private void MakeNewGame(SaveSlot slot)
        {
            var profile = profileSlots[slot];
            _saveLoad.NewSaveData(slot, profile.nameInput.text);
            UpdateProfile(slot);
        }

        private void ContinueGame(SaveSlot slot)
        {
            
        }

        private void DeleteSave(SaveSlot slot)
        {
            _saveLoad.DeleteSaveData(slot);
            UpdateProfile(slot);
        }


        /// <summary>
        /// 프로필 슬롯을 최신 데이터로 갱신.
        /// </summary>
        private void UpdateProfile(SaveSlot slot)
        {
            var profile = profileSlots[slot];
            
            profile.deleteButton.onClick.RemoveAllListeners();
            profile.enterButton.onClick.RemoveAllListeners();
            profile.nameInput.onValueChanged.RemoveAllListeners();
            
            if (!_saveLoad.TryGetSaveData(slot, out var saveData))
            {
                profile.SetDefault();

                var capturedSlot = slot;
                profile.enterButton.onClick.AddListener(() => MakeNewGame(capturedSlot));
                
                profile.nameInput.onValueChanged.AddListener(profile.OnNameInputChanged);
                return;
            }
            
            
            profile.SetData(saveData);

            var capturedSlot2 = slot;
            profile.enterButton.onClick.AddListener(() => ContinueGame(capturedSlot2));
            
            profile.deleteButton.onClick.AddListener(() => profile.nameInput.text = string.Empty);
            profile.deleteButton.onClick.AddListener(() => DeleteSave(capturedSlot2));
        }
    }
}
