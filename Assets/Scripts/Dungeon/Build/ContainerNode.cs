using System;
using UnityEngine;

namespace Cardevil.Dungeon.Build
{
    public class ContainerNode : MonoBehaviour
    {
        [Tooltip("해당 노드가 더미 브랜치 노드인지")]public bool isBranchPoint = false;
        [Tooltip("해당 노드가 분기 시작 컨테이너인지")]public bool isBranchChild = false;
        
        
        private void OnValidate()
        {
            var nl = name.ToLower();
            if (nl.Contains("branchpoint") || nl.Contains("branch_point") || nl.Contains("branch point"))
            {
                isBranchPoint = true;
            }
            else
            {
                isBranchPoint = false;
            }
            if (nl.Contains("branchchild") || nl.Contains("branch_child") || nl.Contains("branch child"))
            {
                isBranchChild = true;
            }
            else
            {
                isBranchChild = false;
            }
        }
    }
}