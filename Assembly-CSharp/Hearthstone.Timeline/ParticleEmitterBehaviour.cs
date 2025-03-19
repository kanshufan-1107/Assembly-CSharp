using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class ParticleEmitterBehaviour : FireAndForgetBehaviour
{
	private ParticleSystem m_particleSystem;

	private PlayableDirector m_director;

	[SerializeField]
	private bool m_includeChildren;

	[SerializeField]
	private bool m_stopOnTrackExit = true;

	[SerializeField]
	private ParticleSystemStopBehavior m_stopBehavior = ParticleSystemStopBehavior.StopEmitting;

	public PlayableGraph PlayableGraph { get; set; }

	public ParticleEmitterTrack TrackAsset { get; set; }

	protected override void OnFireAndForgetInitialize()
	{
		m_particleSystem = TrackAsset.GetTrackBinding<ParticleSystem>(PlayableGraph);
	}

	protected override void OnFireAndForgetEnter(Playable playable, FrameData info)
	{
		if (!(m_particleSystem == null))
		{
			m_particleSystem.Play(m_includeChildren);
			m_director = playable.GetGraph().GetResolver() as PlayableDirector;
			LooperBehaviour.OnLoop = (LooperBehaviour.LoopHandler)Delegate.Combine(LooperBehaviour.OnLoop, new LooperBehaviour.LoopHandler(OnLoop));
		}
	}

	protected override void OnFireAndForgetExit(Playable playable, FrameData info)
	{
		if (m_stopOnTrackExit && !(m_particleSystem == null))
		{
			m_particleSystem.Stop(m_includeChildren, m_stopBehavior);
			LooperBehaviour.OnLoop = (LooperBehaviour.LoopHandler)Delegate.Remove(LooperBehaviour.OnLoop, new LooperBehaviour.LoopHandler(OnLoop));
		}
	}

	private void OnLoop(PlayableDirector director)
	{
		if (!(m_director != director))
		{
			OnFireAndForgetExit(default(Playable), default(FrameData));
		}
	}
}
