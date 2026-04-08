using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Attributes;
using Cardevil.Gameplay;
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
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private RerollView rerollView; 
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private HandBarView handBarView;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private ValueSelectionView valueSelectionView;
        [SerializeField, VisibleOnly(EditableIn.EditMode)] private CardScoreView cardScoreView;
        
        [field: Header("Presenters")]
        [field: SerializeField] public RerollPresenter Reroll { get; private set; }
        [field: SerializeField] public StageCardCorePresenter Core { get; private set; }
        [field: SerializeField] public HandBarPresenter HandBar { get; private set; }
        [field: SerializeField] public ValueSelectionPresenter ValueSelection { get; private set; }
        [field: SerializeField] public ScorePresenter Score { get; private set; }
        
        public void Initialize(
            CardRepository cardRepository,
            PlayerStatus playerStatus, 
            IScoreProviderRegistry scoreProviderRegistry)
        {
            // 내부 로직
            ValueSelection = new ValueSelectionPresenter(valueSelectionView);
            HandBar = new HandBarPresenter(handBarView, ValueSelection);
            Score = new ScorePresenter(cardScoreView); 
            
            // 외부 클래스와 상호작용
            Core = new StageCardCorePresenter(cardRepository, scoreProviderRegistry, coreView, HandBar, Score);
            Reroll = new RerollPresenter(playerStatus, rerollView, Core, HandBar);
            
            HandBar.HandRankChanged += Score.OnHandRankChanged;
        }
    }
}