using Timeline.Samples;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cardevil.InGame.SlotMachine.Playables
{
    public class BoolValuePlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        [NoFoldOut]
        public BoolValuePlayableBehaviour template = new BoolValuePlayableBehaviour();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<BoolValuePlayableBehaviour>.Create(graph, template);
        }
    }
}

