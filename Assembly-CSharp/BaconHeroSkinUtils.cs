using Hearthstone;

[CustomEditClass]
public static class BaconHeroSkinUtils
{
	public enum RotationType
	{
		Active,
		Resting,
		Preview
	}

	public static readonly int SKIN_ID_FOR_FAVORITED_BASE_HERO;

	public static bool CanToggleFavoriteBattlegroundsHeroSkin(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			Log.CollectionManager.PrintWarning("BaconHeroSkinUtils::CanFavoriteBattlegroundsHeroSkin - entityDef is null");
			return false;
		}
		CollectionManager collectionManager = CollectionManager.Get();
		int cardDbid = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		string cardMiniGUID = entityDef.GetCardId();
		int baseCardDbid = GameUtils.TranslateCardIdToDbId(CollectionManager.Get().GetBattlegroundsBaseHeroCardId(cardMiniGUID));
		if (!collectionManager.IsBattlegroundsHeroCard(cardMiniGUID))
		{
			Log.CollectionManager.PrintWarning("BaconHeroSkinUtils::CanFavoriteBattlegroundsHeroSkin - MiniGUID (" + cardMiniGUID + ") is not a HeroCard or a HeroSkinCard");
			return false;
		}
		NetCache.NetCacheBattlegroundsHeroSkins netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheFavoriteHeroes == null)
		{
			return false;
		}
		if (collectionManager.IsBattlegroundsHeroSkinCard(cardDbid))
		{
			if (!collectionManager.OwnsBattlegroundsHeroSkin(cardDbid))
			{
				return false;
			}
		}
		else
		{
			bool ownsAtLeastOneSkinForHero = false;
			foreach (BattlegroundsHeroSkinId battlegroundsHeroSkinId in netCacheFavoriteHeroes.OwnedBattlegroundsSkins)
			{
				if (battlegroundsHeroSkinId.IsValid())
				{
					BattlegroundsHeroSkinDbfRecord battlegroundsHeroSkinDbfRecord = GameDbf.BattlegroundsHeroSkin.GetRecord(battlegroundsHeroSkinId.ToValue());
					if (battlegroundsHeroSkinDbfRecord != null && battlegroundsHeroSkinDbfRecord.BaseCardId == baseCardDbid)
					{
						ownsAtLeastOneSkinForHero = true;
						break;
					}
				}
			}
			if (!ownsAtLeastOneSkinForHero)
			{
				return false;
			}
		}
		netCacheFavoriteHeroes.BattlegroundsFavoriteHeroSkins.TryGetValue(baseCardDbid, out var favoritedSkins);
		if (favoritedSkins == null)
		{
			return true;
		}
		bool isFavorite = IsBattlegroundsHeroSkinFavorited(entityDef);
		if (isFavorite)
		{
			if (isFavorite)
			{
				return favoritedSkins.Count > 1;
			}
			return false;
		}
		return true;
	}

	public static bool IsBattlegroundsHeroSkinFavorited(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			Log.CollectionManager.PrintWarning("BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited() - entityDef is null");
			return false;
		}
		int cardDbid = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		int baseCardID = GameUtils.TranslateCardIdToDbId(CollectionManager.Get().GetBattlegroundsBaseHeroCardId(entityDef.GetCardId()));
		int skinId = SKIN_ID_FOR_FAVORITED_BASE_HERO;
		if (!CollectionManager.Get().IsBattlegroundsHeroCard(entityDef.GetCardId()))
		{
			Log.CollectionManager.PrintWarning("BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited() - Can not favorite entity " + entityDef.GetCardId());
			return false;
		}
		if (CollectionManager.Get().IsBattlegroundsHeroSkinCard(cardDbid))
		{
			if (!CollectionManager.Get().OwnsBattlegroundsHeroSkin(cardDbid))
			{
				Log.CollectionManager.PrintWarning("BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited() - Player does not own BattlegroundsHeroSkin " + entityDef.GetCardId());
				return false;
			}
			BattlegroundsHeroSkinDbfRecord battlegroundsHeroSkinRecord = GameDbf.BattlegroundsHeroSkin.GetRecord((BattlegroundsHeroSkinDbfRecord HeroSkin) => HeroSkin.SkinCardId == cardDbid);
			if (battlegroundsHeroSkinRecord == null)
			{
				Log.CollectionManager.PrintWarning($"BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited() - Could not find BattlegroundsHeroSkinDBFRecord for ID: {cardDbid}");
				return false;
			}
			skinId = battlegroundsHeroSkinRecord.ID;
		}
		NetCache.NetCacheBattlegroundsHeroSkins netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheFavoriteHeroes == null)
		{
			return false;
		}
		netCacheFavoriteHeroes.BattlegroundsFavoriteHeroSkins.TryGetValue(baseCardID, out var favoritedSkins);
		return favoritedSkins?.Contains(skinId) ?? false;
	}

	public static bool CanFavoriteBattlegroundsGuideSkin(EntityDef entityDef)
	{
		if (!CollectionManager.Get().IsBattlegroundsGuideCardId(entityDef.GetCardId()))
		{
			return false;
		}
		int dbId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		if (CollectionManager.Get().GetBattlegroundsGuideSkinIdForCardId(dbId, out var associatedGuideSkinId))
		{
			if (!CollectionManager.Get().OwnsBattlegroundsGuideSkin(dbId))
			{
				return false;
			}
			if (CollectionManager.Get().GetFavoriteBattlegroundsGuideSkin(out var favoriteGuideSkinId))
			{
				return favoriteGuideSkinId != associatedGuideSkinId;
			}
			return true;
		}
		return CollectionManager.Get().HasFavoriteBattlegroundsGuideSkin();
	}

	public static bool IsBattlegroundsGuideSkinFavorited(EntityDef entityDef)
	{
		if (!CollectionManager.Get().IsBattlegroundsGuideCardId(entityDef.GetCardId()))
		{
			return false;
		}
		int dbId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		if (CollectionManager.Get().GetBattlegroundsGuideSkinIdForCardId(dbId, out var associatedGuideSkinId))
		{
			if (CollectionManager.Get().GetFavoriteBattlegroundsGuideSkin(out var favoriteGuideSkinId))
			{
				return favoriteGuideSkinId == associatedGuideSkinId;
			}
			return false;
		}
		if (!CollectionManager.Get().HasFavoriteBattlegroundsGuideSkin())
		{
			return CollectionManager.Get().OwnsAnyBattlegroundsGuideSkin();
		}
		return false;
	}

	public static RotationType GetBattleGroundsHeroRotationType(CardDbfRecord cardRecord, EntityDef cardDef)
	{
		if (!cardDef.HasTag(GAME_TAG.BACON_HERO_CAN_BE_DRAFTED) || !EventTimingManager.Get().IsEventActive(cardRecord.BattlegroundsActiveEvent))
		{
			return RotationType.Resting;
		}
		if (EventTimingManager.Get().IsEventActive(cardRecord.BattlegroundsEarlyAccessEvent))
		{
			return RotationType.Preview;
		}
		return RotationType.Active;
	}
}
