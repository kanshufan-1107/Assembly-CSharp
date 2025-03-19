using UnityEngine;

public class ZoneHeroPower : Zone
{
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
		return true;
	}

	public override Transform GetZoneTransformForCard(Card card)
	{
		return base.transform;
	}
}
