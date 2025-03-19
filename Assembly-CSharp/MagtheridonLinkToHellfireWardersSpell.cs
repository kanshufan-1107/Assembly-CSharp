using System.Collections.Generic;
using UnityEngine;

public class MagtheridonLinkToHellfireWardersSpell : MouseOverLinkSpell
{
	public static readonly string MagtheridonId = "BT_850";

	public static readonly string HellfireWarderId = "BT_850t";

	protected override void GetAllTargets(Entity source, List<GameObject> targets)
	{
		if (source == null || targets == null)
		{
			return;
		}
		ZoneMgr zoneMgr = ZoneMgr.Get();
		if (zoneMgr == null)
		{
			return;
		}
		bool isTriggeredByMagtheridon = false;
		bool sourceIsFriendly = source.IsControlledByFriendlySidePlayer();
		Player.Side searchForWardersSide;
		Player.Side searchForMagtheridonsSide;
		if (source.GetCardId() == MagtheridonId)
		{
			searchForWardersSide = ((!sourceIsFriendly) ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			searchForMagtheridonsSide = (sourceIsFriendly ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			isTriggeredByMagtheridon = true;
		}
		else
		{
			if (!(source.GetCardId() == HellfireWarderId))
			{
				return;
			}
			searchForWardersSide = (sourceIsFriendly ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
			searchForMagtheridonsSide = ((!sourceIsFriendly) ? Player.Side.FRIENDLY : Player.Side.OPPOSING);
		}
		ZonePlay searchThisForWarders = zoneMgr.FindZoneOfType<ZonePlay>(searchForWardersSide);
		ZonePlay zonePlay = zoneMgr.FindZoneOfType<ZonePlay>(searchForMagtheridonsSide);
		int magtheridonsAdded = 0;
		int hellfireWardersAdded = 0;
		foreach (Card card in zonePlay.GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (!(cardEntity.GetCardId() == MagtheridonId) || !cardEntity.IsDormant())
			{
				continue;
			}
			if (isTriggeredByMagtheridon)
			{
				if (card.gameObject == Source)
				{
					targets.Add(card.gameObject);
					magtheridonsAdded++;
				}
			}
			else
			{
				targets.Add(card.gameObject);
				magtheridonsAdded++;
			}
		}
		foreach (Card card2 in searchThisForWarders.GetCards())
		{
			if (card2.GetEntity().GetCardId() == HellfireWarderId)
			{
				targets.Add(card2.gameObject);
				hellfireWardersAdded++;
			}
		}
		if (magtheridonsAdded == 0 || hellfireWardersAdded == 0)
		{
			targets.Clear();
		}
	}
}
