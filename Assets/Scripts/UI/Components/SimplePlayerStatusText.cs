using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Gameplay;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Cardevil.UI.Components
{
    public class SimplePlayerStatusText : MonoBehaviour
    {
        public StatType[] StatType;
        [SerializeField] private TMP_Text statusText;
        [Tooltip("첫번째 값부터 차례대로 {0}, {1}, ... 으로 포맷팅됩니다.")]
        [field: SerializeField] public string FormatString { get; private set; } = "{0}";
        
        private List<object> _displayValues = new ();

        private void Reset()
        {
            statusText = GetComponentInChildren<TMP_Text>(); 
            StatType = new StatType[] { default };
        }

        private void OnEnable()
        {
            if (CardevilCore.PlayerStatus != null)
            {
                _displayValues.Clear();
                foreach (var stat in StatType)
                {
                    _displayValues.Add(CardevilCore.PlayerStatus.GetFinalValue(stat));
                }
                UpdateText();
            }
            ExecEventBus<PlayerStatusChangedArgs>.RegisterStatic(1000, OnPlayerStatusChanged);
        }
        
        private void OnDisable()
        {
            ExecEventBus<PlayerStatusChangedArgs>.UnregisterStatic(OnPlayerStatusChanged);
        }
        
        private UniTask OnPlayerStatusChanged(PlayerStatusChangedArgs args, CancellationToken cancellationToken)
        {
            int index = Array.IndexOf(StatType, args.StatType);
            if (index >= 0)
            {
                if (_displayValues.Count <= index)
                {
                    for (int i = _displayValues.Count; i <= index; i++)
                    {
                        _displayValues.Add(0);
                    }
                }
                _displayValues[index] = args.NewValue;
            }
            
            UpdateText();
            
            return UniTask.CompletedTask;
        }
        
        private void UpdateText()
        {
            statusText.text = string.Format(FormatString, args: _displayValues.ToArray());
        }
    }
}