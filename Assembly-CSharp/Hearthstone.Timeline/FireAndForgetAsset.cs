using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public abstract class FireAndForgetAsset : PlayableAsset, ITimelineClipAsset
{
	public const double DEFAULT_FIRE_AND_FORGET_DURATION = 0.25;

	public override double duration => 0.25;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return default(Playable);
	}
}
