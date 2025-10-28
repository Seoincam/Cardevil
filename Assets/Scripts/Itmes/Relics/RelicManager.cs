using Cardevil.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Relics
{
    [Serializable]
    public class RelicManager
    {
        [Header("All")]
        [SerializeField] List<Relic> _allRelics = new();
        [SerializeField] List<EvaluationRelicEffect> _allEffects = new();

        // ID로 탐색시 더 빠르게 접근.
        private readonly Dictionary<(string id, int level), Relic> _relicById = new();
        private readonly Dictionary<string, EvaluationRelicEffect> _effectById = new();

        private Text text;

        /// <summary>
        /// Data를 기반으로 실제 Relic 및 RelicEffect를 생성.
        /// </summary>
        [ContextMenu("Init")]
        public void Init()
        {
            var datas = Managers.Database.Database;
            if (datas.RelicDataList == null || datas.RelicDataList.Count == 0)
            {
                LogEx.LogError("Database 초기화 전에 접근.");
            }


            _allRelics.Clear();
            _allEffects.Clear();
            _relicById.Clear();
            _effectById.Clear();

            // Build effects
            foreach (var data in datas.RelicEffectOnEvaluationDataList)
            {
                var effect = new EvaluationRelicEffect(data);

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

                if (string.IsNullOrEmpty(relic.Id))
                {
                    Debug.LogWarning("[RelicDataManager] Relic with empty ID ignored.");
                    continue;
                }

                if (_relicById.ContainsKey((relic.Id, data.Level)))
                {
                    Debug.LogWarning($"[RelicDataManager] Duplicate RelicId detected: {relic.Id} LV.{data.Level}. Overwriting previous entry.");
                }

                _allRelics.Add(relic);
                _relicById[(relic.Id, data.Level)] = relic;
            }

            /*
            if (test != null && test.playerRelics != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[ 유물 ]");
                foreach (var relicStr in test.playerRelics)
                {
                    var relicStrs = relicStr.Split('/');
                    var relic = GetRelicById(relicStrs[0], int.Parse(relicStrs[1]));
                    if (relic == null)
                    {
                        sb.AppendLine($"<Missing Relic: {relicStr}>");
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
            */
        }


        #region Helper

        public Relic GetRelicById(string relicId, int level = 1)
        {
            if (string.IsNullOrEmpty(relicId)) return null;
            else
                relicId = relicId.Trim('"');

            return _relicById.TryGetValue((relicId, level), out var relic) ? relic : null;
        }

        public EvaluationRelicEffect GetEffectById(string effectId)
        {
            if (string.IsNullOrEmpty(effectId)) return null;
            else
                effectId = effectId.Trim('"');

            return _effectById.TryGetValue(effectId, out var effect) ? effect : null;
        }

        // public List<EvaluationRelicEffect> GetPlayerEffect(EffectType type)
        // {
        //     if (_playerEffects == null || _playerEffects.Count == 0) return new List<EvaluationRelicEffect>(0);
        //     return _playerEffects.Where(e => e.EffectType == type).ToList();
        // }

        #endregion
    }
}
