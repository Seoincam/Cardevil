using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cardevil.UI.Playables
{
    [TrackColor(0.3f, 0.6f, 0.9f)]
    [TrackClipType(typeof(BoolValuePlayableAsset))]
    [TrackBindingType(typeof(BoolValueSO))]
    public class BoolValueTrack : TrackAsset
    {
        public enum PostPlaybackState
        {
            True,
            False,
            Revert,
            LeaveAsIs
        }

        [SerializeField]
        private PostPlaybackState postPlaybackState = PostPlaybackState.LeaveAsIs;

        private BoolValueMixerBehaviour _mixerBehaviour;

        public PostPlaybackState PostPlaybackMode
        {
            get => postPlaybackState;
            set
            {
                postPlaybackState = value;
                UpdateTrackMode();
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<BoolValueMixerBehaviour>.Create(graph, inputCount);
            _mixerBehaviour = mixer.GetBehaviour();
            UpdateTrackMode();
            return mixer;
        }

        private void UpdateTrackMode()
        {
            if (_mixerBehaviour != null)
            {
                _mixerBehaviour.PostPlaybackMode = postPlaybackState;
            }
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset is BoolValuePlayableAsset boolAsset)
            {
                clip.displayName = boolAsset.template.value ? "True" : "False";
            }
            else
            {
                clip.displayName = "Bool";
            }

            base.OnCreateClip(clip);
        }
    }
}
