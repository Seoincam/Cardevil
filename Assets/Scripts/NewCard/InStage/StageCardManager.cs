using Cardevil.NewCard.InStage.ValueSelection;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public class StageCardManager : MonoBehaviour
    {
        [SerializeField] private StageCardCoreView coreView;
        [SerializeField] private HandBarView handBarView;
        [SerializeField] private ValueSelectionView valueSelectionView;
        
        [SerializeField] private StageCardCorePresenter corePresenter;
        [SerializeField] private HandBarPresenter handBarPresenter;
        [SerializeField] private ValueSelectionPresenter valueSelectionPresenter;

        private void Awake()
        {
            valueSelectionPresenter = new ValueSelectionPresenter(valueSelectionView);
            handBarPresenter = new HandBarPresenter(handBarView, valueSelectionPresenter);
            corePresenter = new StageCardCorePresenter(coreView, handBarPresenter);
        }
    }
}