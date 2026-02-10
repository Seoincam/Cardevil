using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public class StageCardManager : MonoBehaviour
    {
        [SerializeField] private StageCardCoreView coreView;
        [SerializeField] private HandBarView handBarView;
        
        private StageCardCorePresenter _corePresenter;
        private HandBarPresenter _handBarPresenter;

        private void Awake()
        {
            _corePresenter = new StageCardCorePresenter(coreView);
            _handBarPresenter = new HandBarPresenter(handBarView);
        }
    }
}