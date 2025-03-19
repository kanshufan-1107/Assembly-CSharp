using UnityEngine;

public class BaconCollectionCardVisual : CollectionCardVisual
{
	protected override bool ShouldShowNewItemGlow(Actor actor)
	{
		string cardId = actor.GetEntityDef().GetCardId();
		CollectionUtils.ViewMode visualType = GetVisualType();
		switch (visualType)
		{
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			return CollectionManager.Get().ShouldShowNewBattlegroundsHeroSkinGlow(cardId);
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			return CollectionManager.Get().ShouldShowNewBattlegroundsGuideSkinGlow(cardId);
		default:
			Debug.LogWarning(string.Format("{0}.{1}: Unexpected visual type '{2}' for card '{3}'", "BaconCollectionCardVisual", "ShouldShowNewItemGlow", visualType, cardId));
			return false;
		}
	}

	protected override bool IsInCollection(TAG_PREMIUM premium)
	{
		Actor actor = GetActor();
		if (actor == null)
		{
			return false;
		}
		EntityDef entityDef = actor.GetEntityDef();
		if (entityDef == null)
		{
			return false;
		}
		string cardId = entityDef.GetCardId();
		CollectionUtils.ViewMode visualType = GetVisualType();
		switch (visualType)
		{
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			if (entityDef.HasTag(GAME_TAG.BACON_SKIN))
			{
				return CollectionManager.Get().OwnsBattlegroundsHeroSkin(cardId);
			}
			return true;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			if (entityDef.HasTag(GAME_TAG.BACON_BOB_SKIN))
			{
				return CollectionManager.Get().OwnsBattlegroundsGuideSkin(cardId);
			}
			return true;
		default:
			Debug.LogWarning(string.Format("{0}.{1}: Unexpected visual type '{2}' for card '{3}'", "BaconCollectionCardVisual", "IsInCollection", visualType, cardId));
			return false;
		}
	}

	public override void MarkAsSeen()
	{
		string cardId = base.CardId;
		if (!string.IsNullOrEmpty(cardId))
		{
			CollectionUtils.ViewMode visualType = GetVisualType();
			switch (visualType)
			{
			case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
				CollectionManager.Get().MarkBattlegroundsHeroSkinSeen(cardId, base.Premium);
				return;
			case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
				CollectionManager.Get().MarkBattlegroundsGuideSkinSeen(cardId, base.Premium);
				return;
			}
			Debug.LogWarning(string.Format("{0}.{1}: Unexpected visual type '{2}' for card '{3}'", "BaconCollectionCardVisual", "MarkAsSeen", visualType, cardId));
		}
	}
}
