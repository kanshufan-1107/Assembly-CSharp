using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Serializable]
[NotKeyable]
public class VertexAnimatorClip : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private VertexAnimatorBehaviour m_template = new VertexAnimatorBehaviour();

	public VertexAnimatorBehaviour Behaviour => m_template;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		return ScriptPlayable<VertexAnimatorBehaviour>.Create(graph, m_template);
	}
}
