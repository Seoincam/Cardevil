using Cardevil.Core.SceneManagement;
using Cardevil.Core.Utils;
using Cardevil.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Systems.Save
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

        private List<ISaveLoadRoot> _saveLoadRoots = new();
        private IDataService _dataService;
        
        public GameSave CurrentSave => _currentSave;
        public Optional<SaveSlot> CurrentSlot => _currentSlot;
        
        public string DefaultSaveName = "AutoSave";
        private const string SlotFilePrefix = "SaveSlot_";
        
        
        public void Init()
        {
            _dataService = new FileDataService(new JsonSerializer());
            
            SceneLoader.SceneLoaded -= OnSceneLoaded;
            SceneLoader.SceneLoaded += OnSceneLoaded;
        }

        public bool Register(ISaveLoadRoot saveLoadRoot)
        {
            if (_saveLoadRoots.Contains(saveLoadRoot))
                return false;
            _saveLoadRoots.Add(saveLoadRoot);
            return true;
        }
        public bool Unregister(ISaveLoadRoot saveLoadRoot)
        {
            return _saveLoadRoots.Remove(saveLoadRoot);
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
        
        
        /// <summary>
        /// 새로운 세이브 데이터 생성.
        /// 지정된 슬롯과 이름을 기반으로 세이브 생성 및 초기화 후 저장.
        /// </summary>
        /// <param name="slot">세이브 저장 대상 슬롯</param>
        /// <param name="name">프로필 이름</param>
        public void MakeNewSave(SaveSlot slot, string name)
        {
            var newSave = new GameSave(GetSlotFileName(slot), name);
            foreach (var saveRoot in _saveLoadRoots)
            {
                if (saveRoot is INewGameInitializable newGameInitializable)
                {
                    newGameInitializable.SetUpNewGame(newSave);
                }
            }
            
            _currentSlot = new Optional<SaveSlot>(slot);
            _currentSave = newSave;
            SaveGame();
        }

        /// <summary>
        /// 슬롯 기반 세이브 로드.
        /// 지정된 슬롯의 세이브 파일명 계산 후 로드 요청.
        /// </summary>
        public void LoadGame(SaveSlot slot)
        {
            var fileName = GetSlotFileName(slot);
            LoadGame(fileName);
        }
        
        /// <summary>
        /// 슬롯 기반 세이브 삭제.
        /// 지정된 슬롯의 세이브 존재 여부 확인 후 삭제 수행.
        /// </summary>
        public void DeleteSave(SaveSlot slot)
        {
            if (!TryGetSave(slot, out var saveData))
            {
                LogEx.LogWarning($"{slot}에 세이브 데이터가 존재하지 않음.");
                return;
            }

            DeleteSave(saveData.FileName);
        }
        
        /// <summary>
        /// 슬롯에 세이브 존재 여부 확인.
        /// 지정된 슬롯에 대응하는 세이브 파일 존재 검사.
        /// </summary>
        public bool IsAnySave(SaveSlot slot)
        {
            string fileName = GetSlotFileName(slot);
            return _dataService.SaveExists(fileName);
        }
        
        /// <summary>
        /// 슬롯 기반 세이브 데이터 조회.
        /// 지정된 슬롯에서 세이브 파일 로드 시도.
        /// </summary>
        public bool TryGetSave(SaveSlot slot, out GameSave saveData)
        {
            saveData = null;
            
            string fileName = GetSlotFileName(slot);
            if (!_dataService.SaveExists(fileName))
                return false;

            saveData = _dataService.Load(fileName);
            return saveData != null;
        }
        
        /// <summary>
        /// 슬롯에 대응하는 세이브 파일명 생성.
        /// 내부 파일 저장용 접두사와 슬롯 인덱스를 조합.
        /// </summary>
        /// <returns>생성된 세이브 파일명</returns>
        private string GetSlotFileName(SaveSlot slot)
        {
            return $"{SlotFilePrefix}{(int)slot}";
        }
        
        
        [Obsolete("Pls Use MakeNewSave() instead.")]
        public void NewGame() => NewGame(DefaultSaveName);
        
        [Obsolete("Pls Use MakeNewSave() instead.")]
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
            foreach (var saveLoadRoot in _saveLoadRoots)
            {
                saveLoadRoot.Save(_currentSave);
            }
            _dataService.Save(_currentSave, true);
        }
        
        // TODO: 이미 세이브가 적용됐으면 막기
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
            foreach (var saveLoadRoot in _saveLoadRoots)
            {
                saveLoadRoot.Load(_currentSave);
            }
        }
        public void ReloadGame()
        {
            LoadGame(_currentSave.FileName);
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


