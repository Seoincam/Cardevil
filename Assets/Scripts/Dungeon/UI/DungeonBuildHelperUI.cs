using System;
using System.Collections.Generic;


using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Cardevil.Dungeon.UI
{
    public class DungeonBuildHelperUI : MonoBehaviour
    {
        public DungeonNodeUIDataComponent rootNode;
        public int startNodeId = 1;
        public bool showLineRenderer = true;
        
        [ContextMenu("Reset Node IDs")]
        public void ResetNodeIds()
        {
            if (rootNode == null)
            {
                Debug.LogError("Root node is not assigned.");
                return;
            }

            int currentId = startNodeId;
            HashSet<DungeonNodeUIDataComponent> visited = new HashSet<DungeonNodeUIDataComponent>();
            Queue<DungeonNodeUIDataComponent> queue = new Queue<DungeonNodeUIDataComponent>();
            queue.Enqueue(rootNode);
            visited.Add(rootNode);

            while (queue.Count > 0)
            {
                DungeonNodeUIDataComponent currentNode = queue.Dequeue();
                currentNode.nodeId = currentId++;
                currentNode.OnValidate();
                
                foreach (var nextNode in currentNode.nextNodes)
                {
                    if (!visited.Contains(nextNode))
                    {
                        visited.Add(nextNode);
                        queue.Enqueue(nextNode);
                    }
                }
                #if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(currentNode);
                #endif
            }

            Debug.Log("Node IDs have been reset.");
        }
        
        [ContextMenu("Set Node Texts to Type")]
        public void SetNodeTextsToType()
        {
            foreach (var node in this)
            {
                if (node == null) continue;
                
                if (node.GetComponentInChildren<TMPro.TextMeshProUGUI>() is { } text)
                {
                    text.text = $"{node.nodeType}";
                    #if UNITY_EDITOR
                    PrefabUtility.RecordPrefabInstancePropertyModifications(node.gameObject);
                    #endif
                }
                else
                {
                    Debug.LogWarning($"No TextMeshProUGUI found in children of node ID {node.nodeId}");
                }
            }
            Debug.Log("Node texts have been updated to their types.");
        }
        [ContextMenu("")]

        private void OnValidate()
        {
            if (showLineRenderer)
            {
                foreach (var node in this)
                {
                    if (node == null) continue;
                    
                    node.showDebugLines = true;
                    node.OnValidate();
                }
            }
        }

        public IEnumerator<DungeonNodeUIDataComponent> GetEnumerator()
        {
            if (rootNode == null)
            {
                yield break;
            }

            HashSet<DungeonNodeUIDataComponent> visited = new HashSet<DungeonNodeUIDataComponent>();
            Queue<DungeonNodeUIDataComponent> queue = new Queue<DungeonNodeUIDataComponent>();
            queue.Enqueue(rootNode);
            visited.Add(rootNode);

            while (queue.Count > 0)
            {
                DungeonNodeUIDataComponent currentNode = queue.Dequeue();
                yield return currentNode;

                foreach (var nextNode in currentNode.nextNodes)
                {
                    if (!visited.Contains(nextNode))
                    {
                        visited.Add(nextNode);
                        queue.Enqueue(nextNode);
                    }
                }
            }
        }
    }
}