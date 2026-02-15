using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cardevil.InGame.SlotMachine.Playables
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(ScriptableObjectSignalPlayableAsset))]
    [TrackBindingType(typeof(PlayableSignalChanel))]
    public class ScriptableObjectSignalTrack : TrackAsset
    {
        private ScriptableObjectSignalMixerBehaviour _signalMixer;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<ScriptableObjectSignalMixerBehaviour>.Create(graph, inputCount);
            _signalMixer = mixer.GetBehaviour();
            return mixer;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset is ScriptableObjectSignalPlayableAsset signalAsset)
            {
                var displayName = signalAsset.template != null ? signalAsset.template.signalName : null;
                clip.displayName = string.IsNullOrEmpty(displayName) ? "Signal" : displayName;
            }
            else
            {
                clip.displayName = "Signal";
            }
            base.OnCreateClip(clip);
        }
    }
}