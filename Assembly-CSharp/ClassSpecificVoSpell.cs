using System.Collections.Generic;
using UnityEngine;

public class ClassSpecificVoSpell : CardSoundSpell
{
	public ClassSpecificVoData m_ClassSpecificVoData = new ClassSpecificVoData();

	public override AudioSource DetermineBestAudioSource()
	{
		AudioSource source = SearchForClassSpecificVo();
		if ((bool)source)
		{
			return source;
		}
		return base.DetermineBestAudioSource();
	}

	private AudioSource SearchForClassSpecificVo()
	{
		foreach (SpellZoneTag zoneTag in m_ClassSpecificVoData.m_ZonesToSearch)
		{
			List<Zone> zones = SpellUtils.FindZonesFromTag(this, zoneTag, m_ClassSpecificVoData.m_SideToSearch);
			AudioSource source = SearchForClassSpecificVo(zones);
			if ((bool)source)
			{
				return source;
			}
		}
		return null;
	}

	private AudioSource SearchForClassSpecificVo(List<Zone> zones)
	{
		if (zones == null)
		{
			return null;
		}
		foreach (Zone zone in zones)
		{
			foreach (Card card in zone.GetCards())
			{
				SpellClassTag classTag = SpellUtils.ConvertClassTagToSpellEnum(card.GetEntity().GetClass());
				if (classTag == SpellClassTag.NONE)
				{
					continue;
				}
				foreach (ClassSpecificVoLine line in m_ClassSpecificVoData.m_Lines)
				{
					if (line.m_Class == classTag)
					{
						return line.m_AudioSource;
					}
				}
			}
		}
		return null;
	}
}
