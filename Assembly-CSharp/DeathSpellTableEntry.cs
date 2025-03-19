using System;

[Serializable]
public class DeathSpellTableEntry
{
	[CustomEditField(SortPopupByName = true)]
	public DeathSpellType m_Type;

	[CustomEditField(Hide = true)]
	public Spell m_Spell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_SpellPrefabName = "";
}
