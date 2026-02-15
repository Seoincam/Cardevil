using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [Serializable]
    public class ScriptableObjectSignalMixerBehaviour : PlayableBehaviour
    {
        private string _activeSignalName;
        private bool _hasActiveSignal;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var channel = playerData as PlayableSignalChanel;
            if (channel == null)
            {
                return;
            }

            var inputCount = playable.GetInputCount();
            var hasInput = false;
            string nextSignalName = null;

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight <= 0f)
                {
                    continue;
                }

                var inputPlayable = (ScriptPlayable<ScriptableObjectSignalBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();
                if (input == null || string.IsNullOrEmpty(input.signalName))
                {
                    continue;
                }

                hasInput = true;
                nextSignalName = input.signalName;
                break;
            }

            if (!hasInput)
            {
                if (_hasActiveSignal)
                {
                    channel.InvokeSignalEnded(_activeSignalName);
                    _hasActiveSignal = false;
                    _activeSignalName = null;
                }
                return;
            }

            if (!_hasActiveSignal || _activeSignalName != nextSignalName)
            {
                if (_hasActiveSignal)
                {
                    channel.InvokeSignalEnded(_activeSignalName);
                }

                _activeSignalName = nextSignalName;
                _hasActiveSignal = true;
                channel.InvokeSignalStarted(_activeSignalName);
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            _activeSignalName = null;
            _hasActiveSignal = false;
        }
    }
}