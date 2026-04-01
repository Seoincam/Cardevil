using Cardevil.Core.DataStructure.Serializables;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Dungeon.Node
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
        
        [System.Serializable]
        public struct SpriteSet
        {
            public Sprite Inactive;
            public Sprite Active;
            public Sprite Completed;
            public Sprite CompletedOverlay;
        }
        [SerializeField] private SerializableDictionary<DungeonNodeTypes, SpriteSet> nodeTypeToSpriteSet = new SerializableDictionary<DungeonNodeTypes, SpriteSet>();
        
        public IReadOnlyDictionary<DungeonNodeTypes, SpriteSet> NodeTypeToSpriteSet => nodeTypeToSpriteSet;
        
        public SpriteSet GetSpriteSet(DungeonNodeTypes type)
        {
            if (nodeTypeToSpriteSet.TryGetValue(type, out var spriteSet))
            {
                return spriteSet;
            }
            else
            {
                return NodeTypeToSpriteSet[DungeonNodeTypes.None];
            }
        }
        
        public Sprite GetSprite(DungeonNodeTypes type, NodeState state)
        {
            var spriteSet = GetSpriteSet(type);
            return state switch
            {
                NodeState.Locked => spriteSet.Inactive,
                NodeState.Available => spriteSet.Active,
                NodeState.Current => spriteSet.Active,
                NodeState.Completed => spriteSet.Completed,
                _ => spriteSet.Inactive,
            };
        }
        
    }
}