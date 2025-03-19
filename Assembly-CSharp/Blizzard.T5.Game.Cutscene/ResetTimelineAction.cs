using System.Collections;
using System.Collections.Generic;

namespace Blizzard.T5.Game.Cutscene;

public class ResetTimelineAction : TimelineAction
{
	private List<Actor> m_actors;

	private Actor m_friendlyHero;

	private Actor m_alternateFormHero;

	public ResetTimelineAction(List<Actor> activeActors, Actor friendlyHero, Actor alternateFormHero)
	{
		if (activeActors != null)
		{
			m_friendlyHero = friendlyHero;
			m_alternateFormHero = alternateFormHero;
			m_actors = activeActors;
		}
	}

	public override void Dispose()
	{
		if (m_actors != null)
		{
			m_actors.Clear();
			m_actors = null;
		}
		m_isReady = false;
	}

	public override void Init()
	{
		if (!m_isReady)
		{
			m_isReady = m_actors != null && m_actors.Count > 0;
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			yield break;
		}
		foreach (Actor actor in m_actors)
		{
			if (actor != null)
			{
				actor.SetVisibility(isVisible: false, isInternal: false);
			}
		}
		if (m_friendlyHero != null)
		{
			m_friendlyHero?.SetVisibility(isVisible: true, isInternal: false);
		}
		if (m_alternateFormHero != null)
		{
			m_alternateFormHero?.SetVisibility(isVisible: false, isInternal: false);
		}
	}

	public override void Reset()
	{
	}

	public override void Stop()
	{
	}
}
