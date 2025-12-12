using Cardevil.Ingame;
using Cardevil.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Save
{
    [Serializable]
    public class GameSave
    {
        public string Name;
        public long RawSaveTime;
        public PlayerStatus PlayerStatus;
        
        public GameSave(string name)
        {
            Name = name;
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
        }
        public GameSave()
        {
            Name = "DefaultSave";
            SaveTime = DateTime.Now;
            PlayerStatus = new PlayerStatus();
        }
        
        public DateTime SaveTime
        {
            get => DateTime.FromBinary(RawSaveTime);
            set => RawSaveTime = value.ToBinary();
        }
    }
    
    /// <summary>
    /// 게임의 세이브와 로드를 관리하는 매니저 클래스
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    [Serializable]
    public class SaveLoadManager
    {
        public string DefaultSaveName = "AutoSave";
        [SerializeField, Tooltip("예외 씬")] private List<Scenes> exceptionScenes = new();
        [SerializeField] private GameSave _currentSave = new GameSave();
        [SerializeField] private List<ISaveLoad> _saveLoadObjects = new List<ISaveLoad>();
        
        IDataService _dataService;
        
        public GameSave CurrentSave => _currentSave;
        
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
            if (_currentSave != null)
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
                PlayerStatus = new PlayerStatus()
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

        public void LoadGame(string name)
        {
            _currentSave = _dataService.Load(name);
            if (_currentSave == null)
            {
                throw new InvalidOperationException($"Failed to load save '{name}'.");
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
            LoadGame(_currentSave.Name);
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


