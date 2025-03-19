using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class ParticleEmitterAsset : FireAndForgetAsset
{
	public ParticleEmitterBehaviour template;

	public ParticleEmitterTrack Track { get; set; }

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<ParticleEmitterBehaviour> playable = ScriptPlayable<ParticleEmitterBehaviour>.Create(graph, template);
		playable.GetBehaviour().PlayableGraph = graph;
		playable.GetBehaviour().TrackAsset = Track;
		return playable;
	}
}
