using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class SpellTable : MonoBehaviour
{
	public List<SpellTableEntry> m_Table = new List<SpellTableEntry>();

	private Dictionary<SpellType, SpellTableEntry> m_Entries;

	public bool TryGetEntry(SpellType type, out SpellTableEntry entry)
	{
		if (m_Entries == null)
		{
			IntialzieSpellTable();
		}
		return m_Entries.TryGetValue(type, out entry);
	}

	private void IntialzieSpellTable()
	{
		m_Entries = new Dictionary<SpellType, SpellTableEntry>();
		foreach (SpellTableEntry e in m_Table)
		{
			if (m_Entries.ContainsKey(e.m_Type))
			{
				Error.AddDevWarning("Spell Table", "Spell Table: {0} Entry List contains more than one entry for spell type {1}, please remove the duplicate.", base.name, e.m_Type);
			}
			else
			{
				m_Entries.Add(e.m_Type, e);
			}
		}
	}

	private Spell GetSpell(SpellType spellType, bool isLocal = false)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			if (isLocal && entry.m_Spell != null)
			{
				return entry.m_Spell;
			}
			if (string.IsNullOrEmpty(entry.m_SpellPrefabName))
			{
				Error.AddDevWarning("Spell Table", "The Spell Prefab Name for {0} is empty.", entry.m_Type);
				return null;
			}
			string prefabName = entry.m_SpellPrefabName;
			Spell spell = SpellManager.Get().GetSpell(prefabName);
			spell.SetSpellType(spellType);
			if (isLocal)
			{
				entry.m_Spell = spell;
				TransformUtil.AttachAndPreserveLocalTransform(spell.gameObject.transform, base.gameObject.transform);
			}
			return spell;
		}
		return null;
	}

	public Spell GetSpellInstance(SpellType spellType)
	{
		return GetSpell(spellType);
	}

	public Spell GetLocalSpell(SpellType spellType)
	{
		return GetSpell(spellType, isLocal: true);
	}

	public void ReleaseAllSpells()
	{
		foreach (SpellTableEntry entry in m_Table)
		{
			if (entry.m_Spell != null)
			{
				Object.DestroyImmediate(entry.m_Spell.gameObject);
				Object.DestroyImmediate(entry.m_Spell);
				entry.m_Spell = null;
			}
		}
	}

	public void Show()
	{
		foreach (SpellTableEntry entry in m_Table)
		{
			if (!(entry.m_Spell == null) && entry.m_Type != 0)
			{
				entry.m_Spell.Show();
			}
		}
	}

	public void Hide()
	{
		foreach (SpellTableEntry entry in m_Table)
		{
			if (!(entry.m_Spell == null))
			{
				entry.m_Spell.Hide();
			}
		}
	}
}
