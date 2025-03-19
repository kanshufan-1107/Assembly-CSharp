using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class ShakerAsset : PlayableAsset, ITimelineClipAsset
{
	public ShakerBehaviour template;

	public ShakerBehaviour Behaviour { get; private set; }

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<ShakerBehaviour> playable = ScriptPlayable<ShakerBehaviour>.Create(graph, template);
		Behaviour = playable.GetBehaviour();
		return playable;
	}

	public ShakerBehaviour GetBehaviour()
	{
		return Behaviour;
	}
}
