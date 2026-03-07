using Cardevil.Attributes;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    /// <summary>
    /// Presenter들을 생성하며 연결해줌.
    /// 상위 로직은 하위 로직을 직접 제어, 하위 로직은 이벤트만 발행하고 상위 로직을 모르도록 설계함.
    /// </summary>
    public class StageCardManager : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private StageCardCoreView coreView;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private HandBarView handBarView;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private ValueSelectionView valueSelectionView;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private CardScoreView cardScoreView;
        
        [Header("Presenters")]
        [SerializeField] private StageCardCorePresenter corePresenter;
        [SerializeField] private HandBarPresenter handBarPresenter;
        [SerializeField] private ValueSelectionPresenter valueSelectionPresenter;
        [SerializeField] private ScorePresenter scorePresenter;

        public StageCardCorePresenter Core => corePresenter;

        public void Initialize(IScoreProviderRegistry scoreProviderRegistry)
        {
            valueSelectionPresenter = new ValueSelectionPresenter(valueSelectionView);
            handBarPresenter = new HandBarPresenter(handBarView, valueSelectionPresenter);
            scorePresenter = new ScorePresenter(cardScoreView); 
            corePresenter = new StageCardCorePresenter(coreView, handBarPresenter, scorePresenter, scoreProviderRegistry);
            
            handBarPresenter.HandRankChanged += scorePresenter.OnHandRankChanged;
        }
    }
}