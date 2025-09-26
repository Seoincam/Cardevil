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

        private Text text;


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

            var sb = new StringBuilder();
            sb.AppendLine("[ 유물 ]");
            foreach (var relicId in test.playerRelics)
            {
                var relic = GetRelic(relicId);
                _playerRelics.Add(relic);
                sb.AppendLine($"<{relic.Name}>");
                sb.AppendLine(relic.Description);
                sb.AppendLine();
            }

            text.text = sb.ToString();
        }


        public Relic GetRelic(string relicId)
        {
            if (!string.IsNullOrEmpty(relicId))
                relicId = relicId.Trim('"');
            return _allRelics.FirstOrDefault(r => r.RelicId == relicId);
        }

        public RelicEffect GetEffect(string effectId)
        {
            if (!string.IsNullOrEmpty(effectId))
                effectId = effectId.Trim('"');
            return _allEffects.FirstOrDefault(e => e.EffectId == effectId);
        }
    }
}
