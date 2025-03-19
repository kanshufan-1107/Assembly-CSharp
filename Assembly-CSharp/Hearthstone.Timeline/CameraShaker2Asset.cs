using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class CameraShaker2Asset : PlayableAsset, ITimelineClipAsset
{
	public CameraShaker2Behaviour template;

	public CameraShaker2Behaviour Behaviour { get; private set; }

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<CameraShaker2Behaviour> playable = ScriptPlayable<CameraShaker2Behaviour>.Create(graph, template);
		Behaviour = playable.GetBehaviour();
		return playable;
	}

	public ShakerBehaviour GetBehaviour()
	{
		return Behaviour;
	}
}
