using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class EventEmitterBehaviour : FireAndForgetBehaviour
{
	[HideInInspector]
	public PlayableGraph playableGraph;

	[HideInInspector]
	public EventEmitterTrack trackAsset;

	[SerializeField]
	private EventEmitterEntryCollection m_events = new EventEmitterEntryCollection();

	protected override void OnFireAndForgetEnter(Playable playable, FrameData info)
	{
		GameObject target = trackAsset.GetTrackBinding<GameObject>(playableGraph);
		if (target == null)
		{
			return;
		}
		for (int i = 0; i < m_events.Count; i++)
		{
			EventEmitterEntry entry = m_events.Get(i);
			if (entry != null && entry.CanExecute)
			{
				entry.Invoke(target);
			}
		}
	}

	protected override void OnFireAndForgetExit(Playable playable, FrameData info)
	{
	}

	protected override void OnFireAndForgetInitialize()
	{
	}
}
