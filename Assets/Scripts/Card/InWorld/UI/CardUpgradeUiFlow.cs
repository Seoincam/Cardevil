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
        private const string DefaultSelectionText = "강화할 카드를 선택하세요.";
        private const string DefaultUpgradeText = "아래 골드를 내고 다음 단계로 확정 강화합니다.";

        private readonly CardRepository _repository;
        private readonly UpgradeNodeDatabaseSO _upgradeDatabase;
        private readonly int _selectionCandidateCount;
        private readonly Presentation _presentation;

        public CardUpgradeUiFlow(CardRepository repository, UpgradeNodeDatabaseSO upgradeDatabase, int selectionCandidateCount)
            : this(repository, upgradeDatabase, selectionCandidateCount, Presentation.Default)
        {
        }

        public CardUpgradeUiFlow(
            CardRepository repository,
            UpgradeNodeDatabaseSO upgradeDatabase,
            int selectionCandidateCount,
            Presentation presentation)
        {
            _repository = repository;
            _upgradeDatabase = upgradeDatabase;
            _selectionCandidateCount = selectionCandidateCount;
            _presentation = presentation;
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
                host.SetMainUi(_presentation.Icon, _presentation.SelectionText);
                var selected = await selectionPresenter.RequestSelectionAsync(Vector2.zero, cancellationToken);
                while (!selected.IsCanceled)
                {
                    host.SetMainUi(_presentation.Icon, _presentation.UpgradeText);
                    var upgraded = await upgradePresenter.RequestUpgradeAsync(selected.Value, cancellationToken);
                    if (upgraded.IsCompleted)
                    {
                        lastResult = upgraded;
                    }

                    host.SetMainUi(_presentation.Icon, _presentation.SelectionText);
                    selected = await selectionPresenter.WaitForSelectionAsync(cancellationToken);
                }

                return lastResult;
            }
            finally
            {
                if (host)
                {
                    AssetUtil.Destroy(host.gameObject);
                }
            }
        }

        public readonly struct Presentation
        {
            public readonly Sprite Icon;
            public readonly string SelectionText;
            public readonly string UpgradeText;

            public Presentation(Sprite icon, string selectionText, string upgradeText)
            {
                Icon = icon;
                SelectionText = string.IsNullOrWhiteSpace(selectionText) ? DefaultSelectionText : selectionText;
                UpgradeText = string.IsNullOrWhiteSpace(upgradeText) ? DefaultUpgradeText : upgradeText;
            }

            public static Presentation Default => new(null, DefaultSelectionText, DefaultUpgradeText);
        }
    }
}
