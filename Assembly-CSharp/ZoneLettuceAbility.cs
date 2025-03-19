public class ZoneLettuceAbility : Zone
{
	public override string ToString()
	{
		return $"{base.ToString()} (Lettuce Ability)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (m_ServerTag != zoneTag)
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.LETTUCE_ABILITY)
		{
			return false;
		}
		if (entity == null)
		{
			return false;
		}
		return true;
	}

	public override void OnSpellPowerEntityMousedOver(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		if (TargetReticleManager.Get().IsActive())
		{
			return;
		}
		foreach (Card card in m_cards)
		{
			if (card.CanPlaySpellPowerHint(spellSchool))
			{
				Spell burstSpell = card.GetActorSpell(SpellType.SPELL_POWER_HINT_BURST);
				if (burstSpell != null)
				{
					burstSpell.Reactivate();
				}
			}
		}
	}
}
