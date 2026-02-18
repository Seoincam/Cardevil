using Cardevil.NewCard.Common.Core;

namespace Cardevil.NewCard.InStage.Score
{
    public class ScorePresenter
    {
        private readonly CardScoreView _view;

        public ScorePresenter(CardScoreView view)
        {
            _view = view;
            _view.Clear();
        }
        
        public void OnHandRankChanged(HandRank handRank) => _view.UpdateHandRank(handRank);
    }
}