using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class MaterialModifierAsset : PlayableAsset, ITimelineClipAsset
{
	public MaterialModifierBehaviour template;

	public MaterialModifierTrack Track { get; set; }

	public MaterialModifierBehaviour Behaviour { get; private set; }

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<MaterialModifierBehaviour> playable = ScriptPlayable<MaterialModifierBehaviour>.Create(graph, template);
		Behaviour = playable.GetBehaviour();
		playable.GetBehaviour().PlayableGraph = graph;
		playable.GetBehaviour().TrackAsset = Track;
		return playable;
	}
}
