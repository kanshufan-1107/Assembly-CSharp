using System;

[Serializable]
public class SpellTableEntry
{
	[CustomEditField(SortPopupByName = true)]
	public SpellType m_Type;

	[CustomEditField(Hide = true)]
	public Spell m_Spell;

	[CustomEditField(T = EditType.SPELL)]
	public string m_SpellPrefabName = "";
}
