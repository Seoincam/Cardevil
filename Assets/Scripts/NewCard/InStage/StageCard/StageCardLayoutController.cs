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

        public void Apply(CardVisualData data)
        {
            if (currentLayout)
            {
                Destroy(currentLayout);
            }

            // TODO: Shared 적용하기

            ICardLayout layout = data.Type switch
            {
                CardLayoutType.Single => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.SingleWithCorner => Instantiate(singlePrefab, transform).GetComponent<StageCardSingleLayout>(),
                CardLayoutType.Dual => Instantiate(dualPrefab, transform).GetComponent<StageCardDualLayout>(),
                CardLayoutType.Triple => Instantiate(triplePrefab, transform).GetComponent<StageCardTripleLayout>(),
                _ => throw new System.NotImplementedException()
            };

            layout.Apply(data);
            currentLayout = layout.GameObject;
        }
    }

    public class StageCardSingleLayout : MonoBehaviour, ICardLayout
    {
        [SerializeField] private SpriteRenderer innerFrame;
        [SerializeField] private SpriteRenderer mainSprite;
        [SerializeField] private SpriteRenderer cornerSprite;

        public GameObject GameObject => gameObject;

        public void Apply(CardVisualData data)
        {
            innerFrame.sprite = data.InnerFrame;
            mainSprite.sprite = data.MainSprite;

            if (cornerSprite)
                cornerSprite.sprite = data.CornerSprite;
        }
    }

    public class StageCardDualLayout : MonoBehaviour, ICardLayout
    {
        [SerializeField] private SpriteRenderer innerFrame;
        [SerializeField] private SpriteRenderer subSprite0;
        [SerializeField] private SpriteRenderer subSprite1;

        public GameObject GameObject => gameObject;

        public void Apply(CardVisualData data)
        {
            innerFrame.sprite = data.InnerFrame;
            subSprite0.sprite = data.SubSprites[0];
            subSprite1.sprite = data.SubSprites[1];
        }
    }

    public class StageCardTripleLayout : MonoBehaviour, ICardLayout
    {
        [SerializeField] private SpriteRenderer innerFrame;
        [SerializeField] private SpriteRenderer subSprite0;
        [SerializeField] private SpriteRenderer subSprite1;
        [SerializeField] private SpriteRenderer subSprite2;

        public GameObject GameObject => gameObject;

        public void Apply(CardVisualData data)
        {
            innerFrame.sprite = data.InnerFrame;
            subSprite0.sprite = data.SubSprites[0];
            subSprite1.sprite = data.SubSprites[1];
            subSprite2.sprite = data.SubSprites[2];
        }
    }
}
