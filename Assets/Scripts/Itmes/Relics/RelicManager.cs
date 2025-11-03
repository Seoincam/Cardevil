using Cardevil.Attributes;
using Cardevil.DebugConsole;
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

        
        /// <summary>
        /// 보유 유물 읽기 전용 목록 반환.
        /// 게터 호출 시 캐시 재빌드 수행.
        /// </summary>
        /// <remarks><see cref="RebuildCache"/> 호출 후 내부 목록 반환.</remarks>
        /// <value>보유 유물 목록</value>
        public IReadOnlyList<OwnedRelic> OwnedRelics { get { RebuildCache(); return ownedRelics; } }
        
        /// <summary>
        /// 유물 시스템 초기화.
        /// 팩토리 등록, 희귀도별 캐시 생성, DB 초기화 이후 인스턴스 로드 및 기본 유물 획득.
        /// </summary>
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

        /// <summary>
        /// 희귀도 기준 유물 후보 조회.
        /// 요청 개수 충족 시 무작위 선정 목록 반환.
        /// </summary>
        /// <param name="rarity">조회 희귀도</param>
        /// <param name="count">요청 개수</param>
        /// <param name="relics">선정된 유물 목록 출력</param>
        /// <returns>조회 성공 여부</returns>
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
        
        /// <summary>
        /// 유물 ID와 레벨 기반 획득 시도.
        /// 획득 가능 여부 검사 후 보유 목록 추가 및 더티 플래그 설정.
        /// </summary>
        /// <param name="relicId">유물 ID</param>
        /// <param name="level">유물 레벨(기본 1)</param>
        /// <returns>획득 성공 여부</returns>
        public bool TryAcquire(string relicId, int level = 1)
        {
            if (!_allById.TryGetValue((relicId, level), out var relicData))
                return false;

            return TryAcquire(relicData);
        }

        /// <summary>
        /// 유물 데이터 기반 획득 시도.
        /// 획득 가능 여부 검사 후 보유 목록 추가 및 더티 플래그 설정.
        /// </summary>
        /// <param name="relicData">대상 유물 데이터</param>
        /// <returns>획득 성공 여부</returns>
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

        #region ConsoleCommand

        [ConsoleCommand("getRelic", "Get Relic by relicId and level", "getRelic [string: relicId] [int: level (optional, default: 1)]")]
        private static void GetRelicCommand(string[] args)
        {
            string relicId = string.Empty;
            int level = 1;

            if (args.Length == 0)
            {
                DebugConsole.Console.MessageError("Please specify a relic ID.");
                return;
            }

            if (args.Length == 1)
            {
                relicId = args[0];
            }
            else if (args.Length == 2)
            {
                relicId = args[0];
                if (!int.TryParse(args[1], out level))
                {
                    DebugConsole.Console.MessageError("Invalid relic level.");
                    return;
                }
            }

            if (!Managers.Relic.TryAcquire(relicId, level))
            {
                DebugConsole.Console.MessageError("Failed to acquire relic.");
                return;
            }
            else
            {
                DebugConsole.Console.Message($"Relic acquired by {relicId}");
            }
        }

        #endregion
    }
}
