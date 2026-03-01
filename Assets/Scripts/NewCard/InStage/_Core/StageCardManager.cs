using Cardevil.NewCard.InStage.Score;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    /// <summary>
    /// Presenter들을 생성하며 연결해줌.
    /// 상위 로직은 하위 로직을 직접 제어, 하위 로직은 이벤트만 발행하고 상위 로직을 모르도록 설계함.
    /// </summary>
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

        public StageCardCorePresenter Core => corePresenter;

        public void Initialize()
        {
            valueSelectionPresenter = new ValueSelectionPresenter(valueSelectionView);
            handBarPresenter = new HandBarPresenter(handBarView, valueSelectionPresenter);
            scorePresenter = new ScorePresenter(cardScoreView); 
            corePresenter = new StageCardCorePresenter(coreView, handBarPresenter, scorePresenter);
            
            handBarPresenter.HandRankChanged += scorePresenter.OnHandRankChanged;
        }
    }
}