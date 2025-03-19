using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class EventEmitterAsset : FireAndForgetAsset
{
	public EventEmitterBehaviour template;

	public EventEmitterTrack Track { get; set; }

	public EventEmitterBehaviour EventEmitterBehaviour { get; private set; }

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<EventEmitterBehaviour> playable = ScriptPlayable<EventEmitterBehaviour>.Create(graph, template);
		playable.GetBehaviour().playableGraph = graph;
		playable.GetBehaviour().trackAsset = Track;
		EventEmitterBehaviour = playable.GetBehaviour();
		return playable;
	}
}
