using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(Renderer))]
[TrackClipType(typeof(MaterialModifierAsset))]
public class MaterialModifierTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip clip in GetClips())
		{
			MaterialModifierAsset asset = clip.asset as MaterialModifierAsset;
			if (asset != null)
			{
				asset.Track = this;
			}
		}
		return base.CreateTrackMixer(graph, go, inputCount);
	}
}
