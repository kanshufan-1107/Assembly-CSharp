using Hearthstone.DataModels;

public static class MercenaryFactory
{
	public static LettuceMercenaryDataModel CreateEmptyMercenaryDataModel()
	{
		return new LettuceMercenaryDataModel
		{
			HideXp = false,
			HideWatermark = true,
			HideStats = false
		};
	}

	public static LettuceMercenaryDataModel CreateMercenaryDataModel(int mercenaryId, int artVariationId, TAG_PREMIUM premium, LettuceMercenary mercenary = null, CollectionUtils.MercenaryDataPopluateExtra extraRequests = CollectionUtils.MercenaryDataPopluateExtra.None)
	{
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(mercenaryId);
		CardDbfRecord cardRecord = ((artVariationId == 0) ? LettuceMercenary.GetDefaultArtVariationRecord(mercenaryId).CardRecord : GameDbf.MercenaryArtVariation.GetRecord(artVariationId).CardRecord);
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRecord.ID);
		LettuceMercenaryDataModel dataModel = CreateEmptyMercenaryDataModel();
		dataModel.MercenaryId = mercenaryRecord.ID;
		dataModel.MercenaryName = entityDef.GetLocalizedName();
		dataModel.MercenaryShortName = entityDef.GetLocalizedShortName();
		dataModel.MercenaryRole = entityDef.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
		dataModel.MercenaryRarity = (TAG_RARITY)mercenaryRecord.Rarity;
		int level = 1;
		bool fullyUpgraded = false;
		if (mercenary != null)
		{
			level = mercenary.m_level;
			fullyUpgraded = mercenary.m_isFullyUpgraded;
			int experienceInitial = (dataModel.ExperienceFinal = (int)mercenary.m_experience);
			dataModel.ExperienceInitial = experienceInitial;
			bool fullyUpgradedInitial = (dataModel.FullyUpgradedFinal = mercenary.m_isFullyUpgraded);
			dataModel.FullyUpgradedInitial = fullyUpgradedInitial;
			dataModel.Owned = mercenary.m_owned;
			dataModel.ShowAsNew = CollectionManager.Get().DoesMercenaryNeedToBeAcknowledged(mercenary);
			dataModel.NumNewPortraits = CollectionManager.Get().GetNumNewPortraitsToAcknowledgeForMercenary(mercenary);
		}
		CollectionUtils.GetMercenaryStatsByLevel(mercenaryId, level, fullyUpgraded, out var attack, out var health);
		dataModel.MercenaryLevel = level;
		if (extraRequests.HasFlag(CollectionUtils.MercenaryDataPopluateExtra.MythicStats) && mercenary != null && mercenary.m_isFullyUpgraded)
		{
			long mythicModifier = mercenary.GetMythicModifier();
			dataModel.MythicView = true;
			dataModel.MythicLevel = mercenary.m_level + mythicModifier;
			dataModel.MythicModifier = mythicModifier;
			(int, int) mythicBonus = CollectionUtils.GetMythicStatBonus(mythicModifier);
			attack += mythicBonus.Item1;
			health += mythicBonus.Item2;
		}
		dataModel.Card = new CardDataModel
		{
			CardId = cardRecord.NoteMiniGuid,
			Premium = premium,
			Attack = attack,
			Health = health
		};
		return dataModel;
	}

	public static LettuceMercenaryDataModel CreateMercenaryDataModel(LettuceMercenary mercenary, LettuceMercenary.ArtVariation desiredArtVariation = null)
	{
		return CreatePopulatedMercenaryDataModel(mercenary, CollectionUtils.MercenaryDataPopluateExtra.None, desiredArtVariation);
	}

	public static LettuceMercenaryDataModel CreateMercenaryDataModelWithCoin(LettuceMercenary mercenary)
	{
		return CreatePopulatedMercenaryDataModel(mercenary, CollectionUtils.MercenaryDataPopluateExtra.Coin, null);
	}

	public static LettuceMercenaryDataModel CreatePopulatedMercenaryDataModel(LettuceMercenary mercenary, CollectionUtils.MercenaryDataPopluateExtra extraRequests, LettuceMercenary.ArtVariation desiredArtVariation)
	{
		LettuceMercenaryDataModel lettuceMercenaryDataModel = CreateEmptyMercenaryDataModel();
		CollectionUtils.PopulateMercenaryDataModel(lettuceMercenaryDataModel, mercenary, extraRequests, desiredArtVariation);
		return lettuceMercenaryDataModel;
	}

	private static string GetLocalizedName(this EntityDef entityDef)
	{
		string name = entityDef?.GetName();
		if (!string.IsNullOrWhiteSpace(name))
		{
			return GameStrings.FormatLocalizedString(name);
		}
		return null;
	}

	private static string GetLocalizedShortName(this EntityDef entityDef)
	{
		string shortName = entityDef?.GetShortName();
		if (!string.IsNullOrWhiteSpace(shortName))
		{
			return GameStrings.FormatLocalizedString(shortName);
		}
		return entityDef.GetLocalizedName();
	}
}
