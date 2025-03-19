using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExtraTurnSpellConfigData", menuName = "ScriptableObjects/ExtraTurnSpellConfig")]
[CustomEditClass]
public class ExtraTurnSpellConfig : ScriptableObject
{
	public const string ExtraTurnSpellConfigReference = "Player_ExtraTurnSpell.asset:a5424188ac59cae478a912586323320b";

	private static ExtraTurnSpellConfig m_instance;

	public List<ExtraTurnSpellTableEntry> m_Table = new List<ExtraTurnSpellTableEntry>();

	public static ExtraTurnSpellConfig Get()
	{
		if (m_instance == null)
		{
			m_instance = AssetLoader.Get().LoadAsset<ExtraTurnSpellConfig>("Player_ExtraTurnSpell.asset:a5424188ac59cae478a912586323320b");
			if (m_instance == null)
			{
				Error.AddDevWarning("Extra Turn Spell Config", "Extra Turn Spell Config: {0} failed to create game object.", "Player_ExtraTurnSpell.asset:a5424188ac59cae478a912586323320b");
				return null;
			}
		}
		return m_instance;
	}

	private bool TryGetEntry(ExtraTurnSpellType type, out ExtraTurnSpellTableEntry entry)
	{
		foreach (ExtraTurnSpellTableEntry tableEntry in m_Table)
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

	public Spell GetSpell(bool isFriendly = true)
	{
		int entityID = (isFriendly ? GameState.Get().GetFriendlySidePlayer() : GameState.Get().GetOpposingPlayer()).GetTag(GAME_TAG.NUM_TURNS_LAST_AFFECTED_BY);
		ExtraTurnSpellType spellType = (ExtraTurnSpellType)(GameState.Get().GetEntity(entityID)?.GetTag(GAME_TAG.EXTRA_TURNS_SPELL_OVERRIDE) ?? 0);
		if (TryGetEntry(spellType, out var entry))
		{
			string prefabName = (isFriendly ? entry.m_friendlySpellPrefabName : entry.m_opponentSpellPrefabName);
			if (string.IsNullOrEmpty(prefabName))
			{
				Error.AddDevWarning("Extra Turn Spell Config", "The Spell Prefab Name for {0} is empty.", entry.m_Type);
				return null;
			}
			return SpellManager.Get().GetSpell(prefabName);
		}
		return null;
	}
}
