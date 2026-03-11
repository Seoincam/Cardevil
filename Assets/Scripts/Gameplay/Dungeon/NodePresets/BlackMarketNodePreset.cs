using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Dungeon.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Gameplay.Dungeon.NodePresets
{
    /// <summary>
    /// 암시장 노드 프리셋.
    /// 던전에 미리 배치되지만, 확률에 따라 나타나거나 숨겨집니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BlackMarketNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Black Market", order = 7)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Black_Market_Inactive.png")]
    public class BlackMarketNodePreset : DungeonNodePreset
    {
        [Header("암시장 설정")]
        [Tooltip("암시장 출현 확률 (0.0 ~ 1.0). 1.0이면 반드시 출현")]
        [SerializeField, Range(0f, 1f)] private float _appearChance = 0.5f;
        
        [FormerlySerializedAs("_alwaysAppear")]
        [Tooltip("true이면 확률 무시하고 반드시 출현")]
        [SerializeField] private bool alwaysAppear;
        
        [FormerlySerializedAs("_autoConnectNextNodes")]
        [Tooltip("다음 노드를 자동으로 연결할 것인지(암시장이 스킵 가능한지)")]
        [SerializeField] private bool autoConnectNextNodes = false;
        
        [FormerlySerializedAs("_showWhenNotAppeared")]
        [Tooltip("비출현 상태이더라도 표시할 것인지")]
        [SerializeField] private bool showWhenNotAppeared = false;
        
        /// <summary>
        /// 암시장 출현 확률 (0.0 ~ 1.0)
        /// </summary>
        public float AppearChance => _appearChance;
        
        /// <summary>
        /// 반드시 출현하는 암시장인지 여부
        /// </summary>
        public bool AlwaysAppear => alwaysAppear;
        
        /// <summary>
        /// 다음 노드를 자동으로 연결할 것인지 여부
        /// </summary>
        public bool AutoConnectNextNodes => autoConnectNextNodes;
        
        /// <summary>
        /// 암시장이 나타나야 하는지 확률 체크
        /// </summary>
        public bool ShouldAppear()
        {
            if (alwaysAppear) return true;
            return Random.value < _appearChance;
        }
        
        
        public override bool RequiresClearToProgress => false;
        
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"암시장 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 암시장이 나타났습니다!");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"암시장 노드 탈출 (ID: {node.NodeId}): 암시장 거래를 마쳤습니다.");
        }


        /// <summary>
        /// 암시장의 UI를 그립니다.
        /// </summary>
        /// <param name="nodeUI"></param>
        /// <param name="state"></param>
        public override void DrawNodeUI(DungeonNodeUI nodeUI, NodeState state)
        {
            if (nodeUI == null) return;
            #if UNITY_EDITOR
            if(state == NodeState.Locked && !showWhenNotAppeared && Application.isPlaying)
            {
                nodeUI.Hide();
                return;
            }
            else
            {
                nodeUI.Show();
            }
            #else
            if(state == NodeState.Locked && !_showWhenNotAppeared)
            {
                nodeUI.Hide()
                return;
            }
            else
            {
                nodeUI.Show();
            }
            
#endif
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

