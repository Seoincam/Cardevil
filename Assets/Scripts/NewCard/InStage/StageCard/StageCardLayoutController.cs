using Cardevil.NewCard.Common.Visual;
using UnityEngine;

namespace Cardevil.NewCard.InStage.StageCard
{
    public class StageCardLayoutController : MonoBehaviour, ICardLayout
    {
        [Header("Prefabs")]
        [SerializeField] private StageCardSingleLayout singlePrefab;
        [SerializeField] private StageCardDualLayout dualPrefab;
        [SerializeField] private StageCardTripleLayout triplePrefab;

        [Header("States")] 
        [SerializeField] private GameObject currentLayout;

        public GameObject GameObject => gameObject;

        public void Apply(ICardLayoutProvider provider)
        {
            if (currentLayout)
            {
                Destroy(currentLayout);
            }
            
            // TODO: Shared 적용하기
            
            ICardLayout layout = provider.Type switch
            {
                LayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                LayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<StageCardDualLayout>(),
                LayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<StageCardTripleLayout>(),
                _ => throw new System.NotImplementedException()
            };
            
            layout.Apply(provider);
            currentLayout = layout.GameObject;
        }
    }

    public class StageCardSingleLayout : MonoBehaviour, ICardLayout
    {
        [SerializeField] private SpriteRenderer cornerSprite;
        [SerializeField] private SpriteRenderer mainSprite;

        public GameObject GameObject => gameObject;

        public void Apply(ICardLayoutProvider provider)
        {
            /*
             * 1. 숫자 1개, 선택이 된 숫자, 오망성
             * 2. 방향 1개
             * 3. 방향 여러개
             */
            
            // if (provider.Numbers.cou)
            
            cornerSprite.sprite = CardSpriteCache.GetNumber(provider.Colors[0], provider.Numbers[0]);
        }
    }

    public class StageCardDualLayout : MonoBehaviour, ICardLayout
    {
        public GameObject GameObject => gameObject;

        public void Apply(ICardLayoutProvider provider)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StageCardTripleLayout : MonoBehaviour, ICardLayout
    {
        public GameObject GameObject => gameObject;

        public void Apply(ICardLayoutProvider provider)
        {
            throw new System.NotImplementedException();
        }
    }
}