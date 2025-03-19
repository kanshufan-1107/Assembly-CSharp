using UnityEngine;

public class ZoneHeroPower : Zone
{
	public int m_heroPowerIndex;

	public override string ToString()
	{
		return $"{base.ToString()} (Hero Power)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.HERO_POWER)
		{
			return false;
		}
		if (entity != null && entity.GetTag(GAME_TAG.ADDITIONAL_HERO_POWER_INDEX) == m_heroPowerIndex)
		{
			return true;
		}
		return false;
	}

	public override Transform GetZoneTransformForCard(Card card)
	{
		return base.transform;
	}
}
