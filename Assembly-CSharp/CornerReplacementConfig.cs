using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CornerReplacementConfigData", menuName = "ScriptableObjects/CornerReplacementConfig")]
[CustomEditClass]
public class CornerReplacementConfig : ScriptableObject
{
	public const string CornerReplacementConfigReference = "Player_CornerReplacement.asset:a61dcd0d9c8367e45b20ac2f69f6cefd";

	private static CornerReplacementConfig m_instance;

	public List<CornerReplacementSpellTableEntry> m_Table = new List<CornerReplacementSpellTableEntry>();

	public static CornerReplacementConfig Get()
	{
		if (m_instance == null)
		{
			m_instance = AssetLoader.Get().LoadAsset<CornerReplacementConfig>("Player_CornerReplacement.asset:a61dcd0d9c8367e45b20ac2f69f6cefd");
			if (m_instance == null)
			{
				Error.AddDevWarning("Corner Replacement Config", "Corner Replacement Config: {0} failed to create game object.", "Player_CornerReplacement.asset:a61dcd0d9c8367e45b20ac2f69f6cefd");
				return null;
			}
		}
		return m_instance;
	}

	private bool TryGetEntry(CornerReplacementSpellType type, out CornerReplacementSpellTableEntry entry)
	{
		foreach (CornerReplacementSpellTableEntry tableEntry in m_Table)
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

	public Spell GetSpell(CornerReplacementSpellType spellType, CornerReplacementPosition corner)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			string prefabName = "";
			switch (corner)
			{
			case CornerReplacementPosition.TOP_LEFT:
				prefabName = entry.m_TopLeftSpellPrefabName;
				break;
			case CornerReplacementPosition.TOP_RIGHT:
				prefabName = entry.m_TopRightSpellPrefabName;
				break;
			case CornerReplacementPosition.BOTTOM_LEFT:
				prefabName = entry.m_BottomLeftSpellPrefabName;
				break;
			case CornerReplacementPosition.BOTTOM_RIGHT:
				prefabName = entry.m_BottomRightSpellPrefabName;
				break;
			}
			if (string.IsNullOrEmpty(prefabName))
			{
				Error.AddDevWarning("Corner Replacement Config", "The Spell Prefab Name for {0} is empty for corner {1}.", entry.m_Type, corner);
				return null;
			}
			return SpellManager.Get().GetSpell(prefabName);
		}
		return null;
	}

	public string GetActor(CornerReplacementSpellType spellType, ActorNames.ACTOR_ASSET actorName, TAG_PREMIUM premium)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			string prefabName = "";
			switch (actorName)
			{
			case ActorNames.ACTOR_ASSET.PLAY_WEAPON:
				prefabName = entry.m_WeaponActorReplacement;
				break;
			case ActorNames.ACTOR_ASSET.PLAY_HERO_POWER:
				prefabName = ((premium != 0) ? entry.m_HeroPowerGoldenPlayActorReplacement : entry.m_HeroPowerPlayActorReplacement);
				break;
			case ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER:
				prefabName = ((premium != 0) ? entry.m_HeroPowerGoldenHandActorReplacement : entry.m_HeroPowerHandActorReplacement);
				break;
			case ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER_OPPONENT:
				prefabName = ((premium != 0) ? entry.m_HeroPowerOpponentGoldenHandActorReplacement : entry.m_HeroPowerOpponentHandActorReplacement);
				break;
			}
			if (!string.IsNullOrEmpty(prefabName))
			{
				return prefabName;
			}
		}
		return null;
	}

	public Texture GetTableTopTexture(CornerReplacementSpellType spellType)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			return entry.m_TableTopTexture;
		}
		return null;
	}

	public Texture GetFrameTexture(CornerReplacementSpellType spellType)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			return entry.m_FrameTexture;
		}
		return null;
	}

	public Texture GetPlayAreaTexture(CornerReplacementSpellType spellType)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			return entry.m_PlayAreaTexture;
		}
		return null;
	}

	public Texture GetPlayAreaMaskTexture(CornerReplacementSpellType spellType)
	{
		if (TryGetEntry(spellType, out var entry))
		{
			return entry.m_PlayAreaMaskTexture;
		}
		return null;
	}
}
