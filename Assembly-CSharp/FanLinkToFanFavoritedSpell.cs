using System.Collections.Generic;
using UnityEngine;

public class FanLinkToFanFavoritedSpell : MouseOverLinkSpell
{
	protected override void GetAllTargets(Entity source, List<GameObject> targets)
	{
		if (source == null || targets == null)
		{
			return;
		}
		if (source.HasTag(GAME_TAG.FAN_LINK))
		{
			source.GetCard();
			source.GetEnchantments();
			Entity linkedEntity = GameState.Get().GetEntity(source.GetTag(GAME_TAG.FAN_LINK));
			if (linkedEntity != null)
			{
				Card linkedEntityCard = linkedEntity.GetCard();
				if (linkedEntityCard != null)
				{
					targets.Add(linkedEntityCard.gameObject);
				}
			}
		}
		else
		{
			foreach (Entity enchant in source.GetAttachments())
			{
				if (!enchant.HasTag(GAME_TAG.FAN_LINK))
				{
					continue;
				}
				Entity linkedEntity2 = GameState.Get().GetEntity(enchant.GetTag(GAME_TAG.FAN_LINK));
				if (linkedEntity2 != null)
				{
					Card linkedEntityCard2 = linkedEntity2.GetCard();
					if (linkedEntityCard2 != null)
					{
						targets.Add(linkedEntityCard2.gameObject);
					}
				}
			}
		}
		targets.Add(source.GetCard().gameObject);
	}
}
