using System.Collections;

namespace Blizzard.T5.Game.Cutscene;

public class SummonMinionTimelineAction : SpellTimelineAction
{
	private readonly bool m_hideOnInit;

	public SummonMinionTimelineAction(Actor source, bool hideOnInit = true)
		: base(SpellType.SUMMON_IN, source, null)
	{
		m_hideOnInit = hideOnInit;
	}

	public override void Init()
	{
		base.Init();
		if (m_isReady)
		{
			m_actionSource.SetVisibility(!m_hideOnInit, isInternal: false);
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintWarning("SummonMinionTimelineAction aborted playing as it wasn't ready...");
			yield break;
		}
		m_actionSource.SetVisibility(isVisible: false, isInternal: false);
		m_spell.AddStateStartedCallback(OnStateStartedCallback);
		yield return base.Play();
	}

	private void OnStateStartedCallback(Spell spell, SpellStateType stateType, object userData)
	{
		if (stateType != SpellStateType.BIRTH && stateType != 0)
		{
			return;
		}
		spell.RemoveStateStartedCallback(OnStateStartedCallback);
		if (m_actionSource != null)
		{
			m_actionSource.SetVisibility(isVisible: true, isInternal: false);
			Card card = m_actionSource.GetCard();
			if (card != null)
			{
				card.ActivateCharacterPlayEffects();
			}
		}
	}

	public override void Dispose()
	{
		if (m_spell != null)
		{
			m_spell.RemoveStateStartedCallback(OnStateStartedCallback);
		}
		base.Dispose();
	}
}
