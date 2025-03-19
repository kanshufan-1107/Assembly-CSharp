using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackClipType(typeof(LooperAsset))]
[TrackColor(0.8f, 0.5f, 0.1f)]
public class LooperTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip timelineClip in GetClips())
		{
			LooperAsset looperAsset = timelineClip.asset as LooperAsset;
			if (looperAsset != null)
			{
				looperAsset.StartTime = timelineClip.start;
			}
		}
		return base.CreateTrackMixer(graph, go, inputCount);
	}
}
