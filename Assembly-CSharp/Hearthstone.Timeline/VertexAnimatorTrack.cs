using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackClipType(typeof(VertexAnimatorClip))]
[TrackBindingType(typeof(VertexAnimation))]
[TrackColor(1f, 0f, 0f)]
public class VertexAnimatorTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<VertexAnimatorMixerBehaviour>.Create(graph, inputCount);
	}
}
