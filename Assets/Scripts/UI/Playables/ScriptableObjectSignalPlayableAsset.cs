using Timeline.Samples;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cardevil.UI.Playables
{
    public class ScriptableObjectSignalPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;
        [NoFoldOut] 
        public ScriptableObjectSignalBehaviour template = new ScriptableObjectSignalBehaviour();
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ScriptableObjectSignalBehaviour>.Create(graph, template);
        }
    }
}