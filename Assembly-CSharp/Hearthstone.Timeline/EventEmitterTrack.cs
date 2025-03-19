using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackClipType(typeof(EventEmitterAsset))]
[TrackBindingType(typeof(GameObject))]
public class EventEmitterTrack : FireAndForgetTrack
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip clip in GetClips())
		{
			EventEmitterAsset asset = clip.asset as EventEmitterAsset;
			if (asset != null)
			{
				asset.Track = this;
			}
		}
		return base.CreateTrackMixer(graph, go, inputCount);
	}
}
