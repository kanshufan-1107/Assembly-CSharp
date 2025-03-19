using System;

[Serializable]
public class ExtraTurnSpellTableEntry
{
	[CustomEditField(SortPopupByName = true)]
	public ExtraTurnSpellType m_Type;

	[CustomEditField(Hide = true)]
	public Spell m_opponentSpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_opponentSpellPrefabName = "";

	[CustomEditField(Hide = true)]
	public Spell m_friendlySpell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_friendlySpellPrefabName = "";
}
