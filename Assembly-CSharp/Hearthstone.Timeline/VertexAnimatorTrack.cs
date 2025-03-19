using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackColor(1f, 0f, 0f)]
[TrackBindingType(typeof(VertexAnimation))]
[TrackClipType(typeof(VertexAnimatorClip))]
public class VertexAnimatorTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<VertexAnimatorMixerBehaviour>.Create(graph, inputCount);
	}
}
