using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardSpecificVoData
{
	public string m_CardId;

	public GAME_TAG m_RequireTag;

	public int m_TagValue;

	public AudioSource m_AudioSource;

	public SpellPlayerSide m_SideToSearch = SpellPlayerSide.TARGET;

	public List<SpellZoneTag> m_ZonesToSearch = new List<SpellZoneTag>
	{
		SpellZoneTag.PLAY,
		SpellZoneTag.HERO,
		SpellZoneTag.HERO_POWER,
		SpellZoneTag.WEAPON
	};

	public string m_GameStringKey;
}
