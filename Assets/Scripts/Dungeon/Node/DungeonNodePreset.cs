using Cardevil.Attributes;
using Cardevil.Dungeon.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 노드의 실행 로직 및 UI 표현을 담당하는 Preset 클래스.
    /// DungeonNode 진입/퇴장 시 호출되는 메서드와 UI 렌더링 메서드를 정의합니다.
    /// SO를 Enum처럼 사용하여 각 노드 타입별 외형과 동작을 정의합니다.
    /// </summary>
    [Serializable]
    public abstract class DungeonNodePreset : ScriptableObject
    {
        [Header("노드 설정")]
        [SerializeField] protected string displayName;
        [SerializeField] protected DungeonNodeTypes nodeType;
        
        
        [Header("상태별 스프라이트")]
        [Tooltip("잠김 상태 스프라이트")]
        [SerializeField] protected Sprite lockedSprite;
        [Tooltip("활성화(진입 가능) 상태 스프라이트")]
        [SerializeField] protected Sprite activeSprite;
        [Tooltip("완료 상태 스프라이트")]
        [SerializeField] protected Sprite completedSprite;
        [Tooltip("완료 상태 오버레이 스프라이트 (옵션)")]
        [SerializeField] protected Sprite completedOverlaySprite;
        
        [Header("색상 설정")]
        [SerializeField] protected Color nodeColor = Color.white;
        [Tooltip("노드 텍스트 색상")]
        [SerializeField] protected Color textColor = Color.white;
        
        // 프로퍼티
        public string DisplayName => displayName;
        
        public DungeonNodeTypes NodeType => nodeType;
        public Sprite LockedSprite => lockedSprite;
        public Sprite ActiveSprite => activeSprite;
        public Sprite CompletedSprite => completedSprite;
        public Sprite CompletedOverlaySprite => completedOverlaySprite;
        public Color NodeColor => nodeColor;
        public Color TextColor => textColor;

        /// <summary>
        /// 노드에 진입할 때 호출됩니다.
        /// </summary>
        /// <param name="node">진입한 노드</param>
        public abstract void OnEnter(DungeonNode node);
        
        /// <summary>
        /// 노드에서 나갈 때 호출됩니다.
        /// </summary>
        /// <param name="node">나가는 노드</param>
        /// <param name="info">퇴장 정보</param>
        public virtual void OnExit(DungeonNode node, NodeExitInfo info)
        {
            
        }
        
        public virtual Sprite GetNodeBackgroundSprite()
        {
            return lockedSprite;
        }
        
        /// <summary>
        /// 노드의 상태에 따라 적절한 스프라이트를 반환합니다.
        /// </summary>
        public virtual Sprite GetNodeSpriteForState(NodeState state)
        {
            return state switch
            {
                NodeState.Locked => null,
                NodeState.Available => activeSprite,
                NodeState.Current => activeSprite,
                NodeState.Completed => completedSprite,
                _ => lockedSprite
            };
        }
        
        /// <summary>
        /// 노드 UI를 그립니다. 상태에 따라 적절한 스프라이트와 텍스트를 설정합니다.
        /// </summary>
        /// <param name="nodeUI">노드 UI 컴포넌트</param>
        /// <param name="state">현재 노드 상태</param>
        public virtual void DrawNodeUI(DungeonNodeUI nodeUI, NodeState state)
        {
            if (nodeUI == null) return;
            
            void SetImageSpriteAndColor(Image image, Sprite sprite, Color color)
            {
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.color = color;
                    image.SetNativeSize();
                }
                else
                {
                    image.sprite = null;
                    image.color = Color.clear;
                }
            }
            
            if (nodeUI.BackgroundImage != null)
            {
                Sprite bgSprite = GetNodeBackgroundSprite();
                SetImageSpriteAndColor(nodeUI.BackgroundImage, bgSprite, nodeColor);
            }
            
            if (nodeUI.NodeImage != null)
            {
                Sprite stateSprite = GetNodeSpriteForState(state);
                SetImageSpriteAndColor(nodeUI.NodeImage, stateSprite, nodeColor);
            }

            // 텍스트 설정
            if (nodeUI.NodeText != null)
            {
                if (state == NodeState.Locked || state == NodeState.Completed)
                {
                    nodeUI.NodeText.text = "";
                }
                else
                {
                    nodeUI.NodeText.text = string.IsNullOrEmpty(displayName) ? name : displayName;
                    nodeUI.NodeText.color = textColor;
                }
            }
            
            // 오버레이 설정
            if (nodeUI.OverlayImage != null)
            {
                if (state == NodeState.Completed && completedOverlaySprite != null)
                {
                    nodeUI.OverlayImage.gameObject.SetActive(true);
                    nodeUI.OverlayImage.sprite = completedOverlaySprite;
                    nodeUI.NodeImage.SetNativeSize();
                }
                else
                {
                    nodeUI.OverlayImage.gameObject.SetActive(false);
                }
            }
        }
    }
}