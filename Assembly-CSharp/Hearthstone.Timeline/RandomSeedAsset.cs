using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Obsolete("Use ControlRandomized instead.", false)]
public class RandomSeedAsset : PlayableAsset
{
	public RandomSeedBehaviour template;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<RandomSeedBehaviour>.Create(graph, template);
	}
}
