using System;
using UnityEngine.Playables;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [Serializable]
    public class BoolValueMixerBehaviour : PlayableBehaviour
    {
        public BoolValueTrack.PostPlaybackState PostPlaybackMode { get; set; } = BoolValueTrack.PostPlaybackState.LeaveAsIs;

        private BoolValueSO _boundValue;
        private bool _initialValue;
        private bool _hasInitialValue;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var valueSo = playerData as BoolValueSO;
            if (valueSo == null)
            {
                return;
            }

            if (_boundValue != valueSo)
            {
                _boundValue = valueSo;
                _initialValue = valueSo.Value;
                _hasInitialValue = true;
            }

            var inputCount = playable.GetInputCount();
            var hasInput = false;
            var nextValue = _boundValue.Value;
            var bestWeight = 0f;

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight <= 0f)
                {
                    continue;
                }

                var inputPlayable = (ScriptPlayable<BoolValuePlayableBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();
                if (input == null)
                {
                    continue;
                }

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    nextValue = input.value;
                }

                hasInput = true;
            }

            if (!hasInput)
            {
                if (_hasInitialValue)
                {
                    _boundValue.Value = _initialValue;
                }
                return;
            }

            _boundValue.Value = nextValue;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (_boundValue != null)
            {
                switch (PostPlaybackMode)
                {
                    case BoolValueTrack.PostPlaybackState.True:
                        _boundValue.Value = true;
                        break;
                    case BoolValueTrack.PostPlaybackState.False:
                        _boundValue.Value = false;
                        break;
                    case BoolValueTrack.PostPlaybackState.Revert:
                        if (_hasInitialValue)
                        {
                            _boundValue.Value = _initialValue;
                        }
                        break;
                    case BoolValueTrack.PostPlaybackState.LeaveAsIs:
                    default:
                        break;
                }
            }

            _boundValue = null;
            _hasInitialValue = false;
        }
    }
}
