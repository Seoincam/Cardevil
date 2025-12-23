using Cardevil.Dungeon.Build;
using UnityEditor;
using UnityEngine;

namespace Cardevil.Dungeon.Editor
{
    public static class DungeonAssetEditorHelper
    {
        const string dungeonNodePrefabPath = "Assets/Prefabs/Dungeon/DungeonNodeUI.prefab";
        static GameObject cacheDungeonNodePrefab;
        private static GameObject DungeonNodePrefab
        {
            get
            {
                if (cacheDungeonNodePrefab == null)
                {
                    cacheDungeonNodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dungeonNodePrefabPath);
                }
                return cacheDungeonNodePrefab;
            }
        }

        [MenuItem("GameObject/Cardevil/Dungeon/DungeonNode", false, 10)]
        public static void CreateDungeonNode(MenuCommand menuCommand)
        {
            GameObject dungeonNode = PrefabUtility.InstantiatePrefab(DungeonNodePrefab) as GameObject;
            if (dungeonNode != null)
            {
                dungeonNode.name = "DungeonNode";
                GameObjectUtility.SetParentAndAlign(dungeonNode, menuCommand.context as GameObject);
                Undo.RegisterCreatedObjectUndo(dungeonNode, "Create Dungeon Node");
                Selection.activeObject = dungeonNode;
            }
            else
            {
                Debug.LogError("Failed to load DungeonNode prefab.");
            }
        }

        [MenuItem("GameObject/Cardevil/Dungeon/CrateBrachPointNode", false, 10)]
        public static void CreateCrateBranchPointNode(MenuCommand menuCommand)
        {
            GameObject branchPointNode = new GameObject("BranchPoint");
            var node = branchPointNode.AddComponent<ContainerNode>();
            node.isBranchPoint = true;
            node.isBranchChild = false;
            for (int i = 0; i < 2; i++)
            {
                var childNode = new GameObject($"BranchChild_{i + 1}");
                childNode.transform.SetParent(branchPointNode.transform);
                var childContainerNode = childNode.AddComponent<ContainerNode>();
                childContainerNode.isBranchPoint = false;
                childContainerNode.isBranchChild = true;
            }
            
            GameObjectUtility.SetParentAndAlign(branchPointNode, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(branchPointNode, "Create Branch Point Node");
            Selection.activeObject = branchPointNode;
        }
    }
}