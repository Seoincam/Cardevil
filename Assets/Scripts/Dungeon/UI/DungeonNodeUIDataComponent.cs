using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon.UI
{
    public class DungeonNodeUIDataComponent : MonoBehaviour
    {
        public int nodeId;
        public int nodeFloor;
        public DungeonNodePreset nodePreset;
        
        public List<DungeonNodeUIDataComponent> nextNodes = new List<DungeonNodeUIDataComponent>();
        public List<DungeonNodeUIDataComponent> prevNodes = new List<DungeonNodeUIDataComponent>();

        private void Awake()
        {
            enabled = false;
        }

        public void ApplyToNodeData(DungeonNodeData nodeData)
        {
            nodeData.NodeId = nodeId;
            nodeData.Floor = nodeFloor;
            nodeData.Preset = nodePreset;
            nodeData.NextNodeIds.Clear();
            foreach (var nextNode in nextNodes)
            {
                nodeData.NextNodeIds.Add(nextNode.nodeId);
            }
            nodeData.PreviousNodeIds.Clear();
            foreach (var prevNode in prevNodes)
            {
                nodeData.PreviousNodeIds.Add(prevNode.nodeId);
            }
        }
    }
}