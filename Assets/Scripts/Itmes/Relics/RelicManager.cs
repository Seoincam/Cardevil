using Cardevil.Attributes;
using Cardevil.Items.Relics.Factory;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public class RelicManager
    {
        // 모든 유물
        [SerializeField, VisibleOnly] private List<Relic> all = new();
        private Dictionary<(string id, int level), Relic> _allById = new();
        private Dictionary<RelicRarity, List<Relic>> _allByRarity = new();
        
        // 보유 상태
        [SerializeField, VisibleOnly] private List<OwnedRelic> ownedRelics = new();
        private Dictionary<string, OwnedRelic> _ownedByIdCache = new();
        private Dictionary<RelicRarity, List<OwnedRelic>> _ownedByRarityCache = new();
        private bool _dirty;
        
        public IReadOnlyList<OwnedRelic> OwnedRelics { get { RebuildCache(); return ownedRelics; } }

        public void Init()
        {
            RelicFactory.RegisterAllFactory();
            for (RelicRarity r = 0; r <= RelicRarity.FinBoss; r++)
            {
                _allByRarity[r] = new List<Relic>();
                _ownedByRarityCache[r] = new List<OwnedRelic>();
            }
            
            Managers.Database.OnInitialized += () =>
            {
                foreach (Relic relic in RelicFactory.MakeRelicInstances())
                {
                    all.Add(relic);
                    _allById[(relic.Id, relic.Level)] = relic;
                    _allByRarity[relic.Rarity].Add(relic);
                }
                
                // TODO: 세이브-로드 로직 작성하며 다시 수정하기.
                // 기본 유물 획득
                foreach (var relic in all)
                {
                    if (relic.Rarity == RelicRarity.DefaultRelic && relic.Level == 0)
                        Acquire(relic);
                }
            };
        }

        public bool TryGetData(RelicRarity rarity, int count, out List<Relic> relics)
        {
            relics = null;
            RebuildCache();

            if (!_allByRarity.TryGetValue(rarity, out var allOfRarity))
                return false;

            // 후보 구성
            List<Relic> canAcquires = new(allOfRarity.Count);
            foreach (var r in allOfRarity)
            {
                if (CanAcquirable(r))
                    canAcquires.Add(r);
            }
                
            // 개수 체크
            count = Math.Max(0, count);
            if (count > canAcquires.Count)
                return false;
            
            canAcquires.ShuffleListInPlace();
            relics = canAcquires.GetRange(0, count);
            return true;
        }
        
        public bool TryAcquire(string relicId, int level = 1)
        {
            if (!_allById.TryGetValue((relicId, level), out var relicData))
                return false;

            return TryAcquire(relicData);
        }

        public bool TryAcquire(Relic relicData)
        {
            if (!CanAcquirable(relicData))
                return false;

            var newOwned = new OwnedRelic(relicData);
            ownedRelics.Add(newOwned);
            
            // TODO: (일회용이면) 효과 실행

            MakeDirty();
            return true;
        }

        private void Acquire(Relic data)
        {
            if (data == null)
            {
                LogEx.LogWarning($"Relic Data is null.");
                return;
            }
            
            var newOwned = new OwnedRelic(data);
            ownedRelics.Add(newOwned);
            
            // TODO: (일회용이면) 효과 실행

            MakeDirty();
        }

        private bool CanAcquirable(string relicId, int level = 1)
        {
            RebuildCache();
            
            if (!_allById.TryGetValue((relicId, level), out var relicData))
            {
                LogEx.LogWarning("Invalid relic id " + relicId);
                return false;
            }
                
            return CanAcquirable(relicData);
        }

        private bool CanAcquirable(Relic data)
        {
            RebuildCache();
            
            if (data == null)
                return false;
            if (data.Level == 0)
                return false;
            if (data.Rarity == RelicRarity.DefaultRelic)
                return false;
            if (_ownedByIdCache.TryGetValue(data.Id, out _))
                return false;

            return true;
        }

        private void MakeDirty()
        {
            _dirty = true;
        }
        
        private void RebuildCache()
        {
            if (!_dirty)
                return;

            _ownedByIdCache.Clear();
            for (RelicRarity r = 0; r <= RelicRarity.FinBoss; r++)
                _ownedByRarityCache[r].Clear();
            
            foreach (var owned in ownedRelics)
            {
                _ownedByIdCache[owned.Relic.Id] = owned;
                _ownedByRarityCache[owned.Relic.Rarity].Add(owned);
            }
            
            _dirty = false;
        }
        
        /// <summary>
        /// 유물을 획득하고 PlayerStatus로 넘겨줍니다.
        /// </summary>
        public void GetRelicToPlayer(RelicEffectBase relicEffectBase)
        {
            Managers.Game.PlayerStatus.relicEffectBases.Add(relicEffectBase);
        }
        
        /// <summary>
        /// 게임이 시작할때 유물의 효과를 세팅합니다.
        /// </summary>
        public void SettingRelicsEffectBase()
        {
            List<RelicEffectBase> relicList = Managers.Game.PlayerStatus.relicEffectBases; 
            foreach(var r in relicList)
            {
                r.ActivateRelicEffect();
            }
        }
    }
}
