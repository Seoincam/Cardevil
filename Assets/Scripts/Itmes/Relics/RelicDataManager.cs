using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Relics
{
    public class RelicDataManager : MonoBehaviour
    {
        public static RelicDataManager Instance { get; private set; }

        [Header("Test")]
        [SerializeField] RelicTestSO test;

        [Header("All")]
        [SerializeField] List<Relic> _allRelics = new();
        [SerializeField] List<RelicEffect> _allEffects = new();

        [Header("Player")]
        [SerializeField] List<Relic> _playerRelics = new();
        [SerializeField] List<RelicEffect> _playerEffects = new();

        // ID로 탐색시 더 빠르게 접근.
        private readonly Dictionary<(string id, int level), Relic> _relicById = new();
        private readonly Dictionary<string, RelicEffect> _effectById = new();

        private Text text;

        public IReadOnlyList<Relic> PlayerRelics => _playerRelics;

        void Awake()
        {
            Instance = this;
            text = GetComponent<Text>();
        }


        /// <summary>
        /// Data를 기반으로 실제 Relic 및 RelicEffect를 생성.
        /// </summary>
        [ContextMenu("Init")]
        public void Init()
        {
            var datas = Managers.Database.Database;

            _allRelics.Clear();
            _allEffects.Clear();
            _playerRelics.Clear();
            _playerEffects.Clear();
            _relicById.Clear();
            _effectById.Clear();

            // Build effects
            foreach (var data in datas.RelicEffectOnEvaluationDataList)
            {
                var effect = new RelicEffect(data);

                if (string.IsNullOrEmpty(effect.EffectId))
                {
                    Debug.LogWarning("[RelicDataManager] Effect with empty ID ignored.");
                    continue;
                }

                if (_effectById.ContainsKey(effect.EffectId))
                {
                    Debug.LogWarning($"[RelicDataManager] Duplicate EffectId detected: {effect.EffectId}. Overwriting previous entry.");
                }

                _allEffects.Add(effect);
                _effectById[effect.EffectId] = effect;
            }

            // Build relics
            foreach (var data in datas.RelicDataList)
            {
                var relic = new Relic(this, data);

                if (string.IsNullOrEmpty(relic.RelicId))
                {
                    Debug.LogWarning("[RelicDataManager] Relic with empty ID ignored.");
                    continue;
                }

                if (_relicById.ContainsKey((relic.RelicId, data.Level)))
                {
                    Debug.LogWarning($"[RelicDataManager] Duplicate RelicId detected: {relic.RelicId} LV.{data.Level}. Overwriting previous entry.");
                }

                _allRelics.Add(relic);
                _relicById[(relic.RelicId, data.Level)] = relic;
            }

            #region Debug 

            if (test != null && test.playerRelics != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[ 유물 ]");
                foreach (var relicId in test.playerRelics)
                {
                    var relic = GetRelicById(relicId);
                    if (relic == null)
                    {
                        sb.AppendLine($"<Missing Relic: {relicId}>");
                        continue;
                    }

                    _playerRelics.Add(relic);

                    var effects = relic.Effects;
                    if (effects != null)
                    {
                        foreach (var effect in effects)
                            _playerEffects.Add(effect);
                    }

                    sb.AppendLine($"<{relic.Name}>");
                    sb.AppendLine(relic.Description);
                    sb.AppendLine();
                }

                if (text != null) text.text = sb.ToString();
            }

            #endregion
        }


        #region Helper

        public Relic GetRelicById(string relicId, int level = 1)
        {
            if (string.IsNullOrEmpty(relicId)) return null;
            else
                relicId = relicId.Trim('"');

            return _relicById.TryGetValue((relicId, level), out var relic) ? relic : null;
        }

        public RelicEffect GetEffectById(string effectId)
        {
            if (string.IsNullOrEmpty(effectId)) return null;
            else
                effectId = effectId.Trim('"');

            return _effectById.TryGetValue(effectId, out var effect) ? effect : null;
        }

        public List<RelicEffect> GetPlayerEffect(EffectType type)
        {
            if (_playerEffects == null || _playerEffects.Count == 0) return new List<RelicEffect>(0);
            return _playerEffects.Where(e => e.EffectType == type).ToList();
        }

        #endregion
    }
}
