public class CardTextHistoryData
{
	public int m_damageBonus;

	public int m_damageBonusDouble;

	public int m_healingBonus;

	public int m_healingDouble;

	public int m_attackBonus;

	public int m_armorBonus;

	public virtual void SetHistoryData(Entity entity, HistoryInfo historyInfo)
	{
		m_damageBonus = entity.GetDamageBonus();
		m_damageBonusDouble = entity.GetDamageBonusDouble();
		m_healingBonus = entity.GetHealingBonus();
		m_healingDouble = entity.GetHealingDouble();
		m_attackBonus = entity.GetAttackBonus();
		m_armorBonus = entity.GetArmorBonus();
	}
}
