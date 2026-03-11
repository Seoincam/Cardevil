using Cardevil.Core.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cardevil.UI.Playables
{
    [RequireComponent(typeof(PlayableDirector))]
    public class SkippableTimelinePlayableDirector : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;
        private TimelineAsset cachedTimelineAsset;
        private List<SignalEmitter> cachedMarkers = new ();
        
        private void Reset()
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        public void SkipToNextMarker(bool skipToEndIfNoMoreMarkers = true)
        {
            if (playableDirector.state != PlayState.Playing)
                return;
            if (!TryCacheTimelineAsset())
            {
                Debug.LogWarning("PlayableAsset is not a TimelineAsset. Skipping is not supported.");
                return;
            }
            
            int nextMarkerIndex = GetNextMarkerIndex();
            LogEx.Log($"Current Time: {playableDirector.time}, Next Marker Index: {nextMarkerIndex}");
            LogEx.Log("Cached Markers:");
            foreach (var marker in cachedMarkers)
            {
                LogEx.Log($"  Marker: {marker.name}, Time: {marker.time}");
            }
            if (nextMarkerIndex != -1)
            {
                LogEx.Log($"Next Marker Time: {cachedMarkers[nextMarkerIndex].time}");
                double nextMarkerTime = cachedMarkers[nextMarkerIndex].time;
                playableDirector.time = nextMarkerTime;
                // playableDirector.Evaluate();
            }
            else
            {
                if (skipToEndIfNoMoreMarkers)
                {
                    // 타임라인 끝까지 이동
                    playableDirector.time = playableDirector.playableAsset.duration;
                    playableDirector.Evaluate();
                }
            }
           
        }

        private int GetNextMarkerIndex()
        {
            // 현재는 전체탐색. 향후 최적화 필요할 수 있으려나??
            
            double currentTime = playableDirector.time;
            for (int i = 0; i < cachedMarkers.Count; i++)
            {
                if (cachedMarkers[i].time > currentTime)
                {
                    return i;
                }
            }

            return -1; // No more markers
        }
        
        private bool TryCacheTimelineAsset()
        {
            if(cachedTimelineAsset == null || cachedTimelineAsset != playableDirector.playableAsset)
            {
                cachedTimelineAsset = playableDirector.playableAsset as TimelineAsset;
                if (cachedTimelineAsset != null)
                {
                    cachedMarkers.Clear();
                    foreach (var marker in cachedTimelineAsset.markerTrack.GetMarkers())
                    {
                        
                        if (marker is SignalEmitter signalEmitter)
                        {
                            if (signalEmitter.asset != null)
                            {
                                LogEx.Log($"  Signal asset: {signalEmitter.asset.name}");
                                LogEx.Log($"  Signal {signalEmitter.name}");
                            }
                            if (signalEmitter.asset.name.StartsWith("SkipMarker"))
                            {
                                cachedMarkers.Add(signalEmitter);
                            }

                            
                        }
                    }
                }
                cachedMarkers.Sort((a, b) => a.time.CompareTo(b.time));
                return cachedTimelineAsset != null;
            }

            return true;
        }
    }
}