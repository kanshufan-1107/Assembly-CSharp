using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Serializable]
public class LightModifierClip : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private LightModifierBehaviour m_template = new LightModifierBehaviour();

	public LightModifierBehaviour Behaviour => m_template;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<LightModifierBehaviour>.Create(graph, m_template);
	}
}
