using Cardevil.Cards.Data.Save;
using Cardevil.Ingame;
using Cardevil.SceneManagement;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Save
{ 
    /// <summary>
    /// 게임의 세이브와 로드를 관리하는 매니저 클래스
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    [Serializable]
    public class SaveLoadManager
    {
        [SerializeField, Tooltip("예외 씬")] 
        private List<Scenes> exceptionScenes = new();
        
        [SerializeField] private GameSave _currentSave;
        [SerializeField] private Optional<SaveSlot> _currentSlot;
        
        private List<ISaveLoad> _saveLoadObjects = new List<ISaveLoad>();
        private IDataService _dataService;
        
        public GameSave CurrentSave => _currentSave;
        public Optional<SaveSlot> CurrentSlot => _currentSlot;
        
        public string DefaultSaveName = "AutoSave";
        private const string SlotFilePrefix = "SaveSlot_";

        
        public void NewSaveData(SaveSlot slot, string name)
        {
            _currentSlot = new Optional<SaveSlot>(slot);
            _currentSave = new GameSave(GetSlotFileName(slot), name);
            _dataService.Save(_currentSave);
        }

        public void DeleteSaveData(SaveSlot slot)
        {
            if (!TryGetSaveData(slot, out var saveData))
            {
                LogEx.LogWarning($"{slot}에 세이브 데이터가 존재하지 않음.");
                return;
            }

            _dataService.Delete(saveData.FileName);
        }

        public bool TryGetSaveData(SaveSlot slot, out GameSave saveData)
        {
            saveData = null;
            
            string fileName = GetSlotFileName(slot);
            if (!_dataService.SaveExists(fileName))
                return false;

            saveData = _dataService.Load(fileName);
            return saveData != null;
        }
        
        private string GetSlotFileName(SaveSlot slot)
        {
            return $"{SlotFilePrefix}{(int)slot}";
        }
        
        
        public void Init()
        {
            _dataService = new FileDataService(new JsonSerializer());
            
            SceneLoader.SceneLoaded -= OnSceneLoaded;
            SceneLoader.SceneLoaded += OnSceneLoaded;
        }

        public bool Register(ISaveLoad saveLoadObj)
        {
            if (_saveLoadObjects.Contains(saveLoadObj))
                return false;
            _saveLoadObjects.Add(saveLoadObj);
            return true;
        }
        public bool Unregister(ISaveLoad saveLoadObj)
        {
            return _saveLoadObjects.Remove(saveLoadObj);
        }
        
        
        private void OnSceneLoaded(Scenes scene, LoadSceneMode mode)
        {
            if (exceptionScenes.Contains(scene))
                return;
            if (CurrentSave != null)
            {
                ApplySaveToGame();
            }
        }
        
        public void NewGame() => NewGame(DefaultSaveName);
        public void NewGame(string saveName)
        {
            _currentSave = new GameSave
            {
                Name = saveName,
                SaveTime = DateTime.Now,
                PlayerStatus = new PlayerStatus(),
                CardLibraryData = new CardLibrarySaveData()
            };
        }
        
        public void SaveGame()
        {

            if (_dataService == null)
            {
                _dataService = new FileDataService(new JsonSerializer());
            }
            
            if (_currentSave == null)
            {
                throw new InvalidOperationException("No current save to save.");
            }
            _currentSave.SaveTime = DateTime.Now;
            foreach (var saveLoadObj in _saveLoadObjects)
            {
                saveLoadObj.Save(_currentSave);
            }
            _dataService.Save(_currentSave, true);
        }

        public void LoadGame(SaveSlot slot)
        {
            _currentSave = _dataService.Load(GetSlotFileName(slot));
            if (_currentSave == null)
            {
                throw new InvalidOperationException($"Failed to load save 'slot_{(int)slot}'.");
            }
            ApplySaveToGame();
        }

        
        /// <summary>
        /// 현재 세이브 데이터를 게임에 적용합니다.
        /// 각 오브젝트의 reference는 서로 공유되지 않습니다.
        /// </summary>
        public void ApplySaveToGame()
        {
            if (_currentSave == null)
            {
                throw new InvalidOperationException("No current save to apply.");
            }
            foreach (var saveLoadObj in _saveLoadObjects)
            {
                saveLoadObj.Load(_currentSave);
            }
        }
        public void ReloadGame()
        {
            // LoadGame(_currentSave.Name);
        }
        
        public void DeleteSave(string name)
        {
            if (!_dataService.Delete(name))
            {
                throw new InvalidOperationException($"Failed to delete save '{name}'. It may not exist.");
            }
        }


        
    }
}


