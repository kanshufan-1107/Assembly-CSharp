using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(ParticleSystem))]
[TrackColor(0.08f, 0.4f, 0.54f)]
[TrackClipType(typeof(ParticleEmitterAsset))]
public class ParticleEmitterTrack : FireAndForgetTrack
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip clip in GetClips())
		{
			ParticleEmitterAsset asset = clip.asset as ParticleEmitterAsset;
			if (asset != null)
			{
				asset.Track = this;
			}
		}
		return base.CreateTrackMixer(graph, go, inputCount);
	}
}
