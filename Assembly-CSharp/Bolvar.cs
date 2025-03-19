using Blizzard.T5.Game.Spells;

public class Bolvar : SuperSpell
{
	public SpellValueRange[] m_atkPrefabs;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		Card sourceCard = GetSourceCard();
		Entity sourceEntity = sourceCard.GetEntity();
		ISpell prefab = DetermineRangePrefab(sourceEntity.GetATK());
		m_effectsPendingFinish++;
		ISpell spell = CloneSpell(prefab, null);
		spell.SetSource(sourceCard.gameObject);
		spell.Activate();
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private ISpell DetermineRangePrefab(int atk)
	{
		return SpellUtils.GetAppropriateElementAccordingToRanges(m_atkPrefabs, (SpellValueRange x) => x.Range, atk)?.SpellPrefab;
	}
}
