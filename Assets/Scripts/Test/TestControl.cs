using Cardevil.Core.Bootstrap;
using Cardevil.DataStructure.Serializables;
using Cardevil.Dungeon;
using Cardevil.Manager;
using Cardevil.Pools;
using Cardevil.Utils;
using Database;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Cardevil.Test
{
    /// <summary>
    /// Context menu 등을 통한 테스트용 MonoBehaviour
    /// </summary>
    public class TestControl : MonoBehaviour
    {
        [SerializeReference] DungeonNodePreset testDungeonNodePreset;
        public SerializableDictionary<string, int> testSerializableDictionary = new SerializableDictionary<string, int>()
        {
            {"One", 1},
            {"Two", 2},
            {"Three", 3}
        };

        public SerializableReferenceDictionary<string, IComparable> testSerializableReferenceDictionary =
            new SerializableReferenceDictionary<string, IComparable>()
            {
                { "IntValue", 42 }, { "StringValue", "Hello, World!" }, { "FloatValue", 3.14f }
            };
        
        #region Unity Events

        private void Start()
        {
            
        }

        private void Update()
        {

        }

        private void LateUpdate()
        {
            
        }

        private void FixedUpdate()
        {
            
        }

        private void OnGUI()
        {
            // PlayerGUI();
            StageGUI();
        }

        #endregion

        #region Turn/Stage
        public void StageGUI()
        {
            GUILayout.Label("Stage Menu");
            if (!DatabaseManager.Instance.IsInitialized)
            {
                GUILayout.Label("데이터베이스가 초기화되지 않았습니다.");
            }
            else
            {
                if (GUILayout.Button("Enter Stage"))
                {
                    Bootstrapper.Instance.Game.EnterStage("Test");
                }
            }
        }

        #endregion

        #region Player
        [Header("Player Test")] 
        public int setHp = 3;

        [ContextMenu("Set Player HP")]
        public void SetPlayerHp()
        {
            // 플레이어의 HP를 설정하는 테스트
            if (Bootstrapper.Instance.Game.PlayerStatus != null)
            {
                Bootstrapper.Instance.Game.PlayerStatus.CurrentHp = setHp;
                Debug.Log($"플레이어의 HP를 {setHp}로 설정했습니다.");
            }
            else
            {
                Debug.LogError("플레이어가 초기화되지 않았습니다.");
            }
        }
        
        
        public void PlayerGUI()
        {
            if (Bootstrapper.Instance.Game.PlayerStatus != null)
            {
                GUILayout.Label($"Player HP: {Bootstrapper.Instance.Game.PlayerStatus.CurrentHp}");
                if (GUILayout.Button("Increase Player HP"))
                {
                    Bootstrapper.Instance.Game.PlayerStatus.CurrentHp++;
                    Debug.Log("플레이어의 HP를 증가시켰습니다.");
                }
                if (GUILayout.Button("Decrease Player HP"))
                {
                    Bootstrapper.Instance.Game.PlayerStatus.CurrentHp--;
                    Debug.Log("플레이어의 HP를 감소시켰습니다.");
                }
            }
            else
            {
                GUILayout.Label("플레이어가 초기화되지 않았습니다.");
            }
        }

        #endregion

        #region Pool
        [Header("Pool Test")]
        public List<Cardevil.Pools.Poolable> poolables = new List<Cardevil.Pools.Poolable>();
        [FormerlySerializedAs("poolableName")] public string resourcePoolableName = "TestPoolable";
        public Poolables poolableType = Poolables.TestPoolable;
        [ContextMenu("Get Test Poolable")]
        public void GetTestPoolableFromResource()
        {
            // PoolableFactorySO를 통해 Poolable 객체를 가져오는 테스트
            var poolable = AssetUtil.Instantiate(resourcePoolableName, transform).GetComponent<Cardevil.Pools.Poolable>();
            if (poolable != null)
            {
                Debug.Log("Poolable 객체를 성공적으로 가져왔습니다.");
                poolables.Add(poolable);
            }
            else
            {
                Debug.LogError("Poolable 객체를 가져오는 데 실패했습니다.");
            }
        }
        
        [ContextMenu("Get Test Poolable From Pool")]
        public void GetTestPoolableFromPool()
        {
            // PoolManager를 통해 Poolable 객체를 가져오는 테스트
            Poolable poolable = Managers.Pool.Get(poolableType);
            if (poolable != null)
            {
                Debug.Log("Poolable 객체를 성공적으로 가져왔습니다.");
                poolables.Add(poolable);
            }
            else
            {
                Debug.LogError("Poolable 객체를 가져오는 데 실패했습니다.");
            }
        }
        
        [ContextMenu("Release Test Poolable")]
        public void ReleaseTestPoolable()
        {
            // Poolable 객체를 반환하는 테스트
            if (poolables.Count > 0)
            {
                var poolable = poolables[0];
                poolable.Release();
                poolables.RemoveAt(0);
                Debug.Log("Poolable 객체를 성공적으로 반환했습니다.");
            }
            else
            {
                Debug.LogError("반환할 Poolable 객체가 없습니다.");
            }
        }
        

        #endregion

        #region Sound

        [Header("Sound Test")]
        [SerializeField] private AudioResource testAudioResource;
        [SerializeField] private string testAudioResourcePath = "Sounds/Dev/music_jingle/Pizzicato jingles/jingles_PIZZI04";
        

        [ContextMenu("Play Test Sound by resource")]
        public void PlayTestSound()
        {
            // SoundManager를 통해 사운드를 재생하는 테스트
            if (testAudioResource != null)
            {
                // Managers.Sound.Play(testAudioResource);
                Debug.Log("테스트 사운드를 재생했습니다.");
            }
        }
        [ContextMenu("Play Test Sound by name")]
        public void PlayTestSoundByName()
        {
            // SoundManager를 통해 사운드를 재생하는 테스트
            if (!string.IsNullOrEmpty(testAudioResourcePath))
            {
                // Managers.Sound.Play(testAudioResourcePath);
                Debug.Log("테스트 사운드를 재생했습니다.");
            }
            else
            {
                Debug.LogError("테스트 오디오 리소스 이름이 비어 있습니다.");
            }
        }
        
        [ContextMenu("Play Test Sound by hardcoded resource")]
        public void PlayTestSoundByHardcodedResource()
        {
            AudioClip clip = AssetUtil.Load<AudioClip>(testAudioResourcePath);
            if (clip != null)
            {
                // Managers.Sound.Play(clip);
                Debug.Log("테스트 사운드를 재생했습니다.");
            }
            else
            {
                Debug.LogError("테스트 오디오 클립을 로드하는 데 실패했습니다.");
            }
        }
        #endregion
       
    }
}