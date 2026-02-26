using Cardevil.NewCard.InStage.Score;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public class StageCardManager : MonoBehaviour
    {
        [SerializeField] private StageCardCoreView coreView;
        [SerializeField] private HandBarView handBarView;
        [SerializeField] private ValueSelectionView valueSelectionView;
        [SerializeField] private CardScoreView cardScoreView;
        
        [SerializeField] private StageCardCorePresenter corePresenter;
        [SerializeField] private HandBarPresenter handBarPresenter;
        [SerializeField] private ValueSelectionPresenter valueSelectionPresenter;
        [SerializeField] private ScorePresenter scorePresenter;

        private void Start()
        {
            valueSelectionPresenter = new ValueSelectionPresenter(valueSelectionView);
            scorePresenter = new ScorePresenter(cardScoreView);
            handBarPresenter = new HandBarPresenter(handBarView, valueSelectionPresenter);
            corePresenter = new StageCardCorePresenter(coreView, handBarPresenter);
            
            handBarPresenter.HandRankChanged += scorePresenter.OnHandRankChanged;
        }
    }
}