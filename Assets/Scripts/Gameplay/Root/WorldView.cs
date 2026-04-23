using Cardevil.Gameplay.Dungeon;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Cardevil.Gameplay.Root
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private float mapFadeDuration = 0.2f;

        public void HideMap(DungeonManager dungeon)
        {
            if (dungeon?.UI == null)
            {
                Debug.LogError("WorldView: Dungeon UI is not available while hiding map.");
                return;
            }

            dungeon.UI.gameObject.SetActive(false);
        }

        public void ResetChapterTransform(DungeonManager dungeon)
        {
            if (dungeon?.Transition == null)
            {
                Debug.LogError("WorldView: Dungeon transition UI is not available while resetting chapter transform.");
                return;
            }

            dungeon.Transition.ResetChapterUIVisual();
        }

        public void PrepareChapterVisualForSpecialSceneReturn(DungeonManager dungeon)
        {
            if (dungeon?.Transition == null)
            {
                Debug.LogError("WorldView: Dungeon transition UI is not available while preparing special scene return.");
                return;
            }

            dungeon.Transition.PrepareChapterUIForHandUp();
        }
        
        public void PrepareMapForReturn(DungeonManager dungeon)
        {
            if (dungeon?.UI == null)
            {
                Debug.LogError("WorldView: Dungeon UI is not available while preparing map return.");
                return;
            }

            dungeon.UI.gameObject.SetActive(true);
            dungeon.UI.CanvasGroup.alpha = 0f;
            dungeon.UI.CanvasGroup.interactable = true;
            dungeon.UI.CanvasGroup.blocksRaycasts = true;
        }

        public void PrepareMapBehindSpecialScene(DungeonManager dungeon)
        {
            if (dungeon?.UI == null)
            {
                Debug.LogError("WorldView: Dungeon UI is not available while preparing special scene return.");
                return;
            }

            dungeon.UI.gameObject.SetActive(true);
            dungeon.UI.CanvasGroup.alpha = 1f;
            dungeon.UI.CanvasGroup.interactable = false;
            dungeon.UI.CanvasGroup.blocksRaycasts = false;
        }

        public void EnableMapInteraction(DungeonManager dungeon)
        {
            if (dungeon?.UI == null)
            {
                Debug.LogError("WorldView: Dungeon UI is not available while enabling map interaction.");
                return;
            }

            dungeon.UI.CanvasGroup.interactable = true;
            dungeon.UI.CanvasGroup.blocksRaycasts = true;
        }

        public async UniTask FadeInMapAsync(DungeonManager dungeon, CancellationToken ct)
        {
            if (dungeon?.UI == null)
            {
                Debug.LogError("WorldView: Dungeon UI is not available while fading map.");
                return;
            }

            await dungeon.UI.CanvasGroup.DOFade(1f, mapFadeDuration).ToUniTask(cancellationToken: ct);
        }

        public async UniTask PlayStageEnterTransitionAsync(DungeonManager dungeon, CancellationToken ct)
        {
            var transitionUI = dungeon?.Transition;
            if (transitionUI == null)
            {
                Debug.LogError("WorldView: Dungeon transition UI is not available while entering stage.");
                return;
            }

            await transitionUI.ShowEnterTransition(ct);
        }

        public async UniTask PlaySpecialScenePreludeAsync(DungeonManager dungeon, CancellationToken ct)
        {
            var transitionUI = dungeon?.Transition;
            if (transitionUI == null)
            {
                Debug.LogError("WorldView: Dungeon transition UI is not available while entering special scene.");
                return;
            }

            await transitionUI.PlayMapHideAndBlackoutAsync(ct);
        }

        public async UniTask PlayReturnToMapTransitionAsync(DungeonManager dungeon, CancellationToken ct)
        {
            var transitionUI = dungeon?.Transition;
            if (transitionUI == null)
            {
                Debug.LogError("WorldView: Dungeon transition UI is not available while returning to map.");
                return;
            }

            await transitionUI.ShowHandUpAnimation(ct);
        }
    }
}
