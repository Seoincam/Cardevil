using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Relics.Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Cardevil.Gameplay.Root.Stage
{
    /// <summary>
    /// 스테이지 클리어 후 보상과 관련된 로직을 총괄함.
    /// </summary>
    public class StageClearRewardPresenter : IDisposable
    {
        private const int RelicOptionCount = 3;
        
        private readonly ClearRewardTableView _tableView;
        private readonly ClearRewardRelicChestView _relicChestView;
        private readonly PlayerStatus _playerStatus;
        private readonly RelicManager _relicManager;
        
        private readonly int _coinCount;
        private readonly DungeonNodeTypes _type;
        
        private bool _relicChestOpened;
        private List<RelicDefinition> _currentRelicOptions;
        
        public UniTaskCompletionSource RewardWaiter { get; }
        
        private bool IsBossStage => _type is DungeonNodeTypes.MiniBoss or DungeonNodeTypes.FinalBoss;
        private RelicRarity TargetRarity => _type switch 
        {
            DungeonNodeTypes.MiniBoss => RelicRarity.MiddleBoss,
            DungeonNodeTypes.FinalBoss => RelicRarity.FinalBoss,
                
            _ => throw new ArgumentOutOfRangeException(nameof(_type), _type, null)
        };
        
        
        public StageClearRewardPresenter(
            ClearRewardTableView tableView, 
            ClearRewardRelicChestView relicChestView,
            PlayerStatus playerStatus,
            RelicManager relicManager,
            int coinCount, 
            DungeonNodeTypes type)
        {
            _tableView = tableView;
            _relicChestView = relicChestView;
            _playerStatus = playerStatus;
            _relicManager = relicManager;
            
            _coinCount = coinCount;
            _type = type;

            tableView.Tapped += HandleTableTapped;
            relicChestView.RelicClicked += HandleRelicClicked;
            relicChestView.RerollClicked += HandleRerollClicked;

            RewardWaiter = new UniTaskCompletionSource();
        }
        
        public void Dispose()
        {
            if (_tableView) _tableView.Tapped -= HandleTableTapped;
            if (_relicChestView) _relicChestView.RelicClicked -= HandleRelicClicked;
            if (_relicChestView) _relicChestView.RerollClicked -= HandleRerollClicked;
        }


        public void Show()
        {
            _tableView.PlayShowAnimationAsync(_coinCount, IsBossStage).Forget();
        }
        
        
        private void HandleTableTapped()
        {
            // 보스 스테이지였을 경우, 유물 획득
            if (IsBossStage && !_relicChestOpened)
            {
                _currentRelicOptions = _relicManager.GetRandomUnownedRelicsByRarity(TargetRarity, RelicOptionCount);
                
                _relicChestView.PlayShowAnimationAsync(_currentRelicOptions)
                    .ContinueWith(() => _tableView.HideChestIcon())
                    .Forget();
                
                _relicChestOpened = true;
            }
            
            // 일반 스테이지거나, 이미 유물을 획득했을 경우
            else
            {
                _playerStatus.ModifyBaseValue(PlayerStatType.Gold, _coinCount);
                _tableView.PlayHideAnimationAsync().Forget();
            }
        }

        private void HandleRelicClicked(int index)
        {
            var selected = _currentRelicOptions[index];
            _relicManager.AddRelic(selected);
            
            _relicChestView.PlayHideAnimationAsync().Forget();
        }

        private void HandleRerollClicked(int index)
        {
            var newRelicDef = _relicManager.RerollSingleRelic(TargetRarity, _currentRelicOptions, index);
            _relicChestView.RefreshRelic(index, newRelicDef);
        }
    }
}