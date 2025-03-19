using PegasusGame;

public class CutsceneAttackSpellController : AttackSpellController
{
	private bool m_isAttackAnimFinished;

	public void TriggerAttackSpell(Spell spell, Actor sourceActor, Actor targetActor, AttackType attackType = AttackType.REGULAR)
	{
		if (sourceActor == null)
		{
			Log.CosmeticPreview.PrintError("CutsceneAttackSpellController failed to attack as source Actor was null");
		}
		else if (targetActor == null)
		{
			Log.CosmeticPreview.PrintError("CutsceneAttackSpellController failed to attack as target Actor was null");
		}
		else
		{
			TriggerAttackSpell(spell, sourceActor.GetCard(), targetActor.GetCard(), attackType);
		}
	}

	private void TriggerAttackSpell(Spell spell, Card sourceCard, Card targetCard, AttackType attackType = AttackType.REGULAR)
	{
		if (sourceCard == null)
		{
			Log.CosmeticPreview.PrintError("CutsceneAttackSpellController failed to attack as source Card was null");
			return;
		}
		if (targetCard == null)
		{
			Log.CosmeticPreview.PrintError("CutsceneAttackSpellController failed to attack as target Card was null");
			return;
		}
		m_sourceAttackSpell = spell;
		m_attackType = attackType;
		SetSource(sourceCard);
		RemoveAllTargets();
		AddTarget(targetCard);
		spell.AddStateStartedCallback(base.OnSourceAttackStateStarted);
		spell.ActivateState(SpellStateType.BIRTH);
		sourceCard.ActivateCharacterAttackEffects();
		m_isAttackAnimFinished = false;
	}

	protected override void OnFinishedTaskList()
	{
	}

	protected override void OnHeroMoveBackFinished()
	{
		OnFinished();
	}

	protected override void OnFinished()
	{
		if (!m_isAttackAnimFinished)
		{
			TriggerFakeDamageEffect();
			FireFinishedCallbacks();
			m_isAttackAnimFinished = true;
		}
	}

	private void TriggerFakeDamageEffect()
	{
		if (m_targets == null || m_targets.Count == 0)
		{
			return;
		}
		Network.HistMetaData fakeMetaData = new Network.HistMetaData
		{
			MetaType = HistoryMeta.Type.DAMAGE,
			Data = 1
		};
		foreach (Card target in m_targets)
		{
			if (!(target == null))
			{
				target.OnMetaData(fakeMetaData);
			}
		}
	}
}
