using System.Collections.Generic;
using UnityEngine;

public class CardSpecificVoSpell : CardSoundSpell
{
	public List<CardSpecificVoData> m_CardSpecificVoDataList = new List<CardSpecificVoData>();

	public override AudioSource DetermineBestAudioSource()
	{
		CardSpecificVoData voData = GetBestVoiceData();
		if (voData == null)
		{
			return base.DetermineBestAudioSource();
		}
		return voData.m_AudioSource;
	}

	public CardSpecificVoData GetBestVoiceData()
	{
		foreach (CardSpecificVoData cardVOData in m_CardSpecificVoDataList)
		{
			if (SearchForCard(cardVOData))
			{
				return cardVOData;
			}
		}
		return null;
	}

	private bool SearchForCard(CardSpecificVoData cardVOData)
	{
		foreach (SpellZoneTag zoneTag in cardVOData.m_ZonesToSearch)
		{
			List<Zone> zones = SpellUtils.FindZonesFromTag(this, zoneTag, cardVOData.m_SideToSearch);
			if (IsCardInZones(cardVOData.m_CardId, cardVOData.m_RequireTag, cardVOData.m_TagValue, zones))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCardInZones(string cardId, GAME_TAG requireTag, int tagValue, List<Zone> zones)
	{
		if (zones == null)
		{
			return false;
		}
		foreach (Zone zone in zones)
		{
			foreach (Card card in zone.GetCards())
			{
				Entity entity = card.GetEntity();
				bool passedTag = true;
				bool passedCardID = true;
				if (requireTag != 0)
				{
					passedTag = entity.GetTag(requireTag) == tagValue;
				}
				if (!string.IsNullOrEmpty(cardId))
				{
					passedCardID = entity.GetCardId() == cardId;
				}
				if (passedTag && passedCardID)
				{
					return true;
				}
			}
		}
		return false;
	}
}
