using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Dungeon.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Gameplay.Dungeon.Node
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
        [Tooltip("노드 스프라이트 스케일")]
        [SerializeField] protected float spriteScale = 1f;
        [Header("옵션 스프라이트")]
        [Tooltip("완료 상태 오버레이 스프라이트 (옵션)")]
        [SerializeField] protected Sprite completedOverlaySprite;
        [Tooltip("오버레이 스케일")]
        [SerializeField] protected float overlayScale = 1f;
        
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
        /// 이 노드가 다음 노드로 이동하기 전에 클리어가 필요한지 여부
        /// </summary>
        public virtual bool RequiresClearToProgress => true;

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
        
        public virtual Sprite GetNodeBackgroundSprite(NodeState state)
        {
            return state switch
            {
                NodeState.Hidden => null,
                _ => lockedSprite
            };
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
                NodeState.Passed => null,
                NodeState.Hidden => null,
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
            

            
            if (nodeUI.BackgroundImage)
            {
                Sprite bgSprite = GetNodeBackgroundSprite(state);
                SetImageSpriteAndColor(nodeUI.BackgroundImage, bgSprite, nodeColor);
            }
            
            if (nodeUI.NodeImage)
            {
                Sprite stateSprite = GetNodeSpriteForState(state);
                SetImageSpriteAndColor(nodeUI.NodeImage, stateSprite, nodeColor);
            }

            // // 텍스트 설정
            // if (nodeUI.NodeText)
            // {
            //     if (state == NodeState.Locked || state == NodeState.Completed)
            //     {
            //         nodeUI.NodeText.text = "";
            //     }
            //     else
            //     {
            //         nodeUI.NodeText.text = string.IsNullOrEmpty(displayName) ? name : displayName;
            //         nodeUI.NodeText.color = textColor;
            //     }
            // }
            
            // 오버레이 설정
            if (nodeUI.OverlayImage)
            {
                if (state == NodeState.Completed && completedOverlaySprite)
                {
                    nodeUI.OverlayImage.gameObject.SetActive(true);
                    nodeUI.OverlayImage.sprite = completedOverlaySprite;
                    nodeUI.NodeImage.SetNativeSize();
                    nodeUI.OverlayImage.transform.localScale = new Vector3(overlayScale, overlayScale, 1f);
                }
                else
                {
                    nodeUI.OverlayImage.gameObject.SetActive(false);
                }
            }
        }
        
        protected void SetImageSpriteAndColor(Image image, Sprite sprite, Color color)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = color;
                image.SetNativeSize();
                image.transform.localScale = new Vector3(spriteScale, spriteScale, 1f);
            }
            else
            {
                image.sprite = null;
                image.color = Color.clear;
            }
        }
    }
}