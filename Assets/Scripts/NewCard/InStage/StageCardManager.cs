using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public class StageCardManager : MonoBehaviour
    {
        [SerializeField] private StageCardCoreView coreView;
        [SerializeField] private HandBarView handBarView;
        
        [SerializeField] private StageCardCorePresenter corePresenter;
        [SerializeField] private HandBarPresenter handBarPresenter;

        private void Awake()
        {
            handBarPresenter = new HandBarPresenter(handBarView);
            corePresenter = new StageCardCorePresenter(coreView, handBarPresenter);
        }
    }
}