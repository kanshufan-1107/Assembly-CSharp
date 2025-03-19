using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeathSpellConfigData", menuName = "ScriptableObjects/DeathSpellConfig")]
[CustomEditClass]
public class DeathSpellConfig : ScriptableObject
{
	public const string DeathSpellConfigReference = "Player_DeathSpell.asset:34d7b7bff4337fc448af397d6dea0c56";

	private static DeathSpellConfig m_instance;

	public List<DeathSpellTableEntry> m_Table = new List<DeathSpellTableEntry>();

	public static DeathSpellConfig Get()
	{
		if (m_instance == null)
		{
			m_instance = AssetLoader.Get().LoadAsset<DeathSpellConfig>("Player_DeathSpell.asset:34d7b7bff4337fc448af397d6dea0c56");
			if (m_instance == null)
			{
				Error.AddDevWarning("Death Spell Config", "Death Spell Config: {0} failed to create game object.", "Player_DeathSpell.asset:34d7b7bff4337fc448af397d6dea0c56");
				return null;
			}
		}
		return m_instance;
	}

	private bool TryGetEntry(DeathSpellType type, out DeathSpellTableEntry entry)
	{
		foreach (DeathSpellTableEntry tableEntry in m_Table)
		{
			if (tableEntry.m_Type == type)
			{
				entry = tableEntry;
				return true;
			}
		}
		entry = null;
		return false;
	}

	public Spell GetSpell(DeathSpellType spellType)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			string prefabName = entry.m_SpellPrefabName;
			if (string.IsNullOrEmpty(prefabName))
			{
				Error.AddDevWarning("Death Spell Config", "The Spell Prefab Name for {0} is empty.", entry.m_Type);
				return null;
			}
			Spell spell = SpellManager.Get().GetSpell(prefabName);
			spell.SetSpellType(SpellType.DEATH);
			return spell;
		}
		return null;
	}
}
