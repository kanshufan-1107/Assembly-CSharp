public class VisualEmoteSpell : Spell
{
	public Spell m_FriendlySpellPrefab;

	public Spell m_OpponentSpellPrefab;

	public bool m_PositionOnSpeechBubble;

	protected int m_effectsPendingFinish;

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		Spell spellToUse = null;
		Card sourceCard = GetSourceCard();
		if (sourceCard != null)
		{
			Player sourcePlayer = sourceCard.GetController();
			if (sourcePlayer != null)
			{
				if (sourcePlayer.IsFriendlySide())
				{
					spellToUse = m_FriendlySpellPrefab;
				}
				else if (!sourcePlayer.IsFriendlySide())
				{
					spellToUse = m_OpponentSpellPrefab;
				}
			}
		}
		if (spellToUse != null)
		{
			Spell spell = SpellManager.Get().GetSpell(spellToUse);
			spell.SetSource(GetSource());
			spell.AddStateFinishedCallback(OnSpellStateFinished);
			spell.AddFinishedCallback(OnSpellEffectFinished);
			m_effectsPendingFinish++;
			spell.Activate();
		}
		if (!HasStateContent(SpellStateType.BIRTH))
		{
			OnStateFinished();
		}
	}

	private void FinishIfPossible()
	{
		if (m_effectsPendingFinish == 0)
		{
			base.OnSpellFinished();
		}
	}

	public override void OnSpellFinished()
	{
		FinishIfPossible();
	}

	private void OnSpellEffectFinished(Spell spell, object userData)
	{
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private void OnSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}
}
