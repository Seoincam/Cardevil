using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.InWorld.UI.Selection;
using Cardevil.Card.InWorld.UI.Upgrade;
using Cardevil.Core.Utils;
using Cardevil.UI.Flow;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Cardevil.Card.InWorld.UI
{
    public sealed class CardUpgradeUiFlow
    {
        private readonly CardRepository _repository;
        private readonly UpgradeNodeDatabaseSO _upgradeDatabase;
        private readonly int _selectionCandidateCount;

        public CardUpgradeUiFlow(CardRepository repository, UpgradeNodeDatabaseSO upgradeDatabase, int selectionCandidateCount)
        {
            _repository = repository;
            _upgradeDatabase = upgradeDatabase;
            _selectionCandidateCount = selectionCandidateCount;
        }

        public async UniTask<UiFlowResult<int>> SelectAndUpgradeAsync(CancellationToken cancellationToken = default)
        {
            var host = CardWorldUiHost.Instantiate();
            if (!host || !host.SelectionView || !host.UpgradeView)
            {
                return UiFlowResult<int>.Canceled();
            }

            var selectionPresenter = new SelectionPresenter(_repository, host.SelectionView, _selectionCandidateCount);
            var upgradePresenter = new CardUpgradePresenter(_repository, _upgradeDatabase, host.UpgradeView);
            upgradePresenter.CardUpgraded += selectionPresenter.HandleCardUpgraded;

            try
            {
                var lastResult = UiFlowResult<int>.Canceled();
                var selected = await selectionPresenter.RequestSelectionAsync(Vector2.zero, cancellationToken);
                while (!selected.IsCanceled)
                {
                    var upgraded = await upgradePresenter.RequestUpgradeAsync(selected.Value, cancellationToken);
                    if (upgraded.IsCompleted)
                    {
                        lastResult = upgraded;
                    }

                    selected = await selectionPresenter.WaitForSelectionAsync(cancellationToken);
                }

                return lastResult;
            }
            finally
            {
                if (host)
                {
                    // Object.Destroy(host.gameObject);
                    AssetUtil.Destroy(host.gameObject);
                }
            }
        }

    }
}
