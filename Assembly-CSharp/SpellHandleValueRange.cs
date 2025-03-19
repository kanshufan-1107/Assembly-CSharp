using System;

[Serializable]
public class SpellHandleValueRange
{
	public ValueRange m_range;

	[CustomEditField(T = EditType.SPELL)]
	public string m_spellPrefabName;
}
