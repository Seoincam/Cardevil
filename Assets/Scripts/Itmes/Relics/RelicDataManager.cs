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

            foreach (var data in datas.RelicEffectOnEvaluationDataList)
            {
                var effect = new RelicEffect(data);
                _allEffects.Add(effect);
            }

            foreach (var data in datas.RelicDataList)
            {
                var relic = new Relic(this, data);
                _allRelics.Add(relic);
            }

            #region Debug 
            var sb = new StringBuilder();
            sb.AppendLine("[ 유물 ]");
            foreach (var relicId in test.playerRelics)
            {
                var relic = GetRelicById(relicId);
                _playerRelics.Add(relic);
                foreach (var effect in relic.Effects)
                {
                    _playerEffects.Add(effect);
                }
                
                sb.AppendLine($"<{relic.Name}>");
                sb.AppendLine(relic.Description);
                sb.AppendLine();
            }

            text.text = sb.ToString();
            #endregion
        }


        public Relic GetRelicById(string relicId)
        {
            if (!string.IsNullOrEmpty(relicId))
                relicId = relicId.Trim('"');
            return _allRelics.FirstOrDefault(r => r.RelicId == relicId);
        }

        public RelicEffect GetEffectById(string effectId)
        {
            if (!string.IsNullOrEmpty(effectId))
                effectId = effectId.Trim('"');
            return _allEffects.FirstOrDefault(e => e.EffectId == effectId);
        }

        public List<RelicEffect> GetPlayerEffect(EffectType type)
        {
            return _playerEffects.Where(e => e.EffectType == type)
                .ToList();
        }
    }
}
