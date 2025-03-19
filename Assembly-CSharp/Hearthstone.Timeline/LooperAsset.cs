using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Serializable]
public class LooperAsset : PlayableAsset, ITimelineClipAsset
{
	[SerializeField]
	private LooperBehaviour m_template = new LooperBehaviour();

	public LooperBehaviour Behaviour => m_template;

	public double StartTime { get; set; }

	public int PatternIndex { get; set; }

	public ClipCaps clipCaps => ClipCaps.None;

	public override double duration => 1.0;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<LooperBehaviour> playable = ScriptPlayable<LooperBehaviour>.Create(graph, m_template);
		playable.GetBehaviour().ClipAsset = this;
		return playable;
	}
}
