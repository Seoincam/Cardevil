using Cardevil.DataStructure;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Cardevil.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonNodeSettingSO", menuName = "ScriptableObject/DungeonNodeSettingSO")]
    public class DungeonNodeSettingSO : ScriptableObject
    {
        private static DungeonNodeSettingSO _default;

        public static DungeonNodeSettingSO Default
        {
            get
            {
                if (_default == null)
                {
                    LoadDefault();
                }
                return _default;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDefault()
        {
            // 리소시스 폴더에서 로드
            var assets = Resources.LoadAll<DungeonNodeSettingSO>("");
            if (assets.Length == 1)
            {
                _default = assets[0];
            }
            else if (assets.Length > 1)
            {
                foreach (var asset in assets)
                {
                    if(asset.name.Contains("Default"))
                    {
                        _default = asset;
                        break;
                    }
                }
            }
            if (_default == null)
            {
                // Addressables에서 로드 시도
                // var op = Addressables.LoadAssetAsync<DungeonNodeSettingSO>("DefaultDungeonNodeSetting");
                // op.WaitForCompletion();
                // if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                // {
                //     _default = op.Result;
                // }
                // else
                // {
                //     Debug.LogWarning("Failed to load default DungeonNodeSettingSO from Addressables.");
                // }
            }
        }
        
        public struct SpriteSet
        {
            public Sprite Inactive;
            public Sprite Active;
            public Sprite Completed;
        }
        [SerializeField] private SerializableDict<DungeonNodeTypes, SpriteSet> nodeTypeToSpriteSet = new SerializableDict<DungeonNodeTypes, SpriteSet>();
        
        public IReadOnlyDictionary<DungeonNodeTypes, SpriteSet> NodeTypeToSpriteSet => nodeTypeToSpriteSet;
        
    }
}