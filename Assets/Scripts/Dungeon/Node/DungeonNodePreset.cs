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
        [SerializeField] protected int count = 1;
        [SerializeField] protected string displayName;
        
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
        public int Count => count;
        public string DisplayName => displayName;
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
        
        /// <summary>
        /// 노드의 상태에 따라 적절한 스프라이트를 반환합니다.
        /// </summary>
        public virtual Sprite GetSpriteForState(NodeState state)
        {
            return state switch
            {
                NodeState.Locked => lockedSprite,
                NodeState.Available => activeSprite,
                NodeState.Current => activeSprite,
                NodeState.Completed => completedSprite,
                _ => lockedSprite
            };
        }
        
        /// <summary>
        /// 노드 UI를 그립니다. 상태에 따라 적절한 스프라이트와 텍스트를 설정합니다.
        /// </summary>
        /// <param name="nodeImage">노드 배경 이미지</param>
        /// <param name="nodeText">노드 텍스트</param>
        /// <param name="overlayImage">오버레이 이미지 (옵션)</param>
        /// <param name="state">현재 노드 상태</param>
        public virtual void DrawNodeUI(Image nodeImage, TextMeshProUGUI nodeText, Image overlayImage, NodeState state)
        {
            // 스프라이트 설정
            if (nodeImage != null)
            {
                Sprite sprite = GetSpriteForState(state);
                if (sprite != null)
                {
                    nodeImage.sprite = sprite;
                    nodeImage.color = nodeColor;
                }
                else
                {
                    nodeImage.sprite = null;
                    nodeImage.color = Color.clear;
                }
                
            }

            // 텍스트 설정 (있는 경우)
            if (nodeText != null)
            {
                if (state == NodeState.Locked || state == NodeState.Completed)
                {
                    nodeText.text = "";
                }
                else
                {
                    nodeText.text = string.IsNullOrEmpty(displayName) ? name : displayName;
                    nodeText.color = textColor;
                }
            }
            
            // 오버레이 설정
            if (overlayImage != null)
            {
                if (state == NodeState.Completed && completedOverlaySprite != null)
                {
                    overlayImage.gameObject.SetActive(true);
                    overlayImage.sprite = completedOverlaySprite;
                }
                else
                {
                    overlayImage.gameObject.SetActive(false);
                }
            }
        }
    }
}