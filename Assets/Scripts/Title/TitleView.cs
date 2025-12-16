using Cardevil.Core.Bootstrap;
using Cardevil.DataStructure.Serializables;
using Cardevil.Save;
using Cardevil.SceneManagement;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cardevil.Title
{
    public class TitleView : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private Button profileButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject dim;
        [SerializeField] private GameObject profilePanel;

        [Space] 
        [SerializeField] private SerializableDictionary<SaveSlot, SlotProfileView> profileSlots;

        private SaveLoadManager _saveLoad;
        

        private void Awake()
        {
            _saveLoad = CardevilCore.Instance.SaveLoad;
            
            profileButton.onClick.AddListener(OnProfileClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        
        private void MakeNewSave(SaveSlot slot)
        {
            var profile = profileSlots[slot];
            _saveLoad.MakeNewSave(slot, profile.nameInput.text);
            UpdateProfile(slot);
        }

        private void ContinueGame(SaveSlot slot)
        {
            if (!_saveLoad.IsAnySave(slot))
            {
                LogEx.LogWarning("No save found for slot " + slot);
                return;
            }
            
            _saveLoad.LoadGame(slot);
            SceneLoader.LoadSceneAsync(Scenes.World, LoadSceneMode.Single).Forget();
        }

        private void DeleteSave(SaveSlot slot)
        {
            _saveLoad.DeleteSave(slot);
            UpdateProfile(slot);
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
        
        
        /// <summary>
        /// 프로필 슬롯을 최신 데이터로 갱신.
        /// </summary>
        private void UpdateProfile(SaveSlot slot)
        {
            var profile = profileSlots[slot];
            profile.RemoveAllButtonListeners();
            
            if (!_saveLoad.TryGetSave(slot, out var saveData))
            {
                profile.SetDefault();

                var capturedSlot = slot;
                profile.enterButton.onClick.AddListener(() => MakeNewSave(capturedSlot));
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
