using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone;
using Hearthstone.Commerce;
using Hearthstone.DataModels;
using Hearthstone.Store;
using PegasusShared;
using PegasusUtil;

public class RewardFactory
{
	public static List<RewardItemDataModel> CreateRewardItemDataModel(RewardItemDbfRecord itemRecord, RewardItemOutputData rewardItemOutputData = null)
	{
		List<RewardItemDataModel> resultList = null;
		RewardItemDataModel resultSingle = null;
		bool unsupportedType = false;
		switch (itemRecord.RewardType)
		{
		case RewardItem.RewardType.GOLD:
		case RewardItem.RewardType.TAVERN_TICKET:
		case RewardItem.RewardType.REWARD_TRACK_XP_BOOST:
			resultSingle = CreateSimpleRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.DUST:
		case RewardItem.RewardType.ARCANE_ORBS:
		case RewardItem.RewardType.RENOWN:
		case RewardItem.RewardType.BATTLEGROUNDS_TOKEN:
			resultSingle = CreateCurrencyRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BOOSTER:
			resultSingle = CreateBoosterRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.CARD:
			resultSingle = CreateCardRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.RANDOM_CARD:
			resultSingle = CreateRandomCardRewardItemDataModel(itemRecord, rewardItemOutputData);
			break;
		case RewardItem.RewardType.CARD_BACK:
			resultSingle = CreateCardBackRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.HERO_SKIN:
			resultSingle = CreateHeroSkinRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.CUSTOM_COIN:
			resultSingle = CreateCustomCoinRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.CARD_SUBSET:
			resultList = CreateCardSubsetRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.GAME_MODE:
			resultSingle = CreateGameModeRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.HERO_CLASS:
			resultSingle = CreateHeroClassRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.DECK:
			resultSingle = CreateDeckRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.LOANER_DECKS:
			resultSingle = CreateLoanerDecksRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.MERCENARY:
			resultSingle = ((rewardItemOutputData == null || !rewardItemOutputData.HasAmount || rewardItemOutputData.Amount <= 1 || rewardItemOutputData.ArtVariationId != 0) ? CreateMercenaryRewardItemDataModel(itemRecord, rewardItemOutputData) : CreateMercenaryCoinRewardItemDataModel(itemRecord, rewardItemOutputData));
			break;
		case RewardItem.RewardType.MERCENARY_XP:
			resultSingle = CreateMercenaryXPRewardItemDataModel(itemRecord, rewardItemOutputData);
			break;
		case RewardItem.RewardType.MERCENARY_EQUIPMENT:
			resultSingle = CreateMercenaryEquipRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.MERCENARY_CURRENCY:
			resultSingle = CreateMercenaryCoinRewardItemDataModel(itemRecord, rewardItemOutputData);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_HERO_SKIN:
			resultSingle = CreateBattlegroundsHeroSkinRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_GUIDE_SKIN:
			resultSingle = CreateBattlegroundsGuideSkinRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_EMOTE:
			resultSingle = CreateBattlegroundsEmoteRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_BOARD_SKIN:
			resultSingle = CreateBattlegroundsBoardSkinRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_FINISHER:
			resultSingle = CreateBattlegroundsFinisherRewardItemDataModel(itemRecord);
			break;
		case RewardItem.RewardType.BATTLEGROUNDS_SEASON_BONUS:
			resultSingle = CreateSeasonBonusRewardItemDataModel(itemRecord);
			break;
		default:
			unsupportedType = true;
			break;
		}
		if (resultList == null && resultSingle == null)
		{
			if (unsupportedType)
			{
				Log.All.PrintWarning($"RewardItem has unsupported item type [itemid {itemRecord.ID}, type {itemRecord.RewardType}]");
			}
			else
			{
				Log.All.PrintWarning($"Failed creating RewardItem data model [itemid {itemRecord.ID}, rewardtype {itemRecord.RewardType}]");
			}
			return new List<RewardItemDataModel>();
		}
		if (resultList != null)
		{
			return resultList;
		}
		return new List<RewardItemDataModel> { resultSingle };
	}

	private static RewardItemDataModel CreateBoosterRewardItemDataModel(RewardItemDbfRecord record)
	{
		int boosterId = record.Booster;
		if (boosterId == 0)
		{
			boosterId = (int)GameUtils.GetRewardableBoosterFromSelector(record.BoosterSelector);
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = boosterId,
			Booster = new PackDataModel
			{
				Type = (BoosterDbId)boosterId,
				Quantity = record.Quantity
			}
		};
	}

	private static RewardItemDataModel CreateCardRewardItemDataModel(RewardItemDbfRecord record)
	{
		CardDbfRecord cardRec = GameDbf.Card.GetRecord(record.Card);
		if (cardRec == null)
		{
			Log.All.PrintWarning($"Card Item has unknown card id [{record.Card}]");
			return null;
		}
		string rarityText = "";
		if (GameUtils.TryGetCardTagRecords(cardRec.NoteMiniGuid, out var cardTags))
		{
			CardTagDbfRecord rarityRecord = cardTags.Find((CardTagDbfRecord tagRecord) => tagRecord.TagId == 203);
			rarityText = ((rarityRecord == null) ? "" : GameStrings.GetRarityTextKey((TAG_RARITY)rarityRecord.TagValue));
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRec.ID);
		string cardText = "";
		if (entityDef != null)
		{
			cardText = entityDef.GetCardTextInHand();
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.Card,
			Card = new CardDataModel
			{
				CardId = cardRec.NoteMiniGuid,
				Premium = (TAG_PREMIUM)record.CardPremiumLevel,
				FlavorText = cardRec.FlavorText,
				Rarity = rarityText,
				Name = cardRec.Name.GetString(),
				CardText = cardText
			},
			Booster = new PackDataModel
			{
				Type = (BoosterDbId)record.Booster
			}
		};
	}

	private static List<RewardItemDataModel> CreateCardSubsetRewardItemDataModel(RewardItemDbfRecord record)
	{
		List<RewardItemDataModel> cardSubsetRewardList = new List<RewardItemDataModel>();
		foreach (string subsetCardRec in GameDbf.GetIndex().GetSubsetById(record.SubsetId))
		{
			CardDbfRecord cardRec = GameDbf.Card.GetRecord(GameUtils.TranslateCardIdToDbId(subsetCardRec));
			cardSubsetRewardList.Add(new RewardItemDataModel
			{
				AssetId = record.ID,
				ItemType = RewardItemType.CARD,
				Quantity = record.Quantity,
				ItemId = cardRec.ID,
				Card = new CardDataModel
				{
					CardId = cardRec.NoteMiniGuid,
					Premium = (TAG_PREMIUM)record.CardPremiumLevel
				}
			});
		}
		return cardSubsetRewardList;
	}

	private static RewardItemDataModel CreateGameModeRewardItemDataModel(RewardItemDbfRecord record)
	{
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			GameModeId = (int)record.GameModeSelector,
			StandaloneDescription = record.StandaloneDescription
		};
	}

	private static RewardItemDataModel CreateHeroClassRewardItemDataModel(RewardItemDbfRecord record)
	{
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			HeroClassId = record.HeroClassId
		};
	}

	private static RewardItemDataModel CreateDeckRewardItemDataModel(RewardItemDbfRecord record)
	{
		DeckPouchDataModel deck = ShopDeckPouchDisplay.CreateDeckPouchDataModelFromDeckTemplate(record.DeckRecord, null);
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			DeckTemplateId = record.DeckId,
			Deck = deck
		};
	}

	private static RewardItemDataModel CreateLoanerDecksRewardItemDataModel(RewardItemDbfRecord record)
	{
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			StandaloneDescription = record.StandaloneDescription
		};
	}

	private static RewardItemDataModel CreateMercenaryRewardItemDataModel(RewardItemDbfRecord record, RewardItemOutputData rewardItemOutputData = null)
	{
		int mercenaryId = record.Mercenary;
		int artVariationId = record.MercenaryArtVariation;
		TAG_PREMIUM mercenaryPremium = (TAG_PREMIUM)record.MercenaryArtPremium;
		TAG_RARITY mercenaryRarity = (TAG_RARITY)record.MercenaryRarity;
		if (rewardItemOutputData != null)
		{
			if (rewardItemOutputData.HasMercenaryId && rewardItemOutputData.MercenaryId > 0)
			{
				mercenaryId = rewardItemOutputData.MercenaryId;
			}
			if (rewardItemOutputData.HasArtVariationId && rewardItemOutputData.ArtVariationId > 0)
			{
				artVariationId = rewardItemOutputData.ArtVariationId;
			}
			if (rewardItemOutputData.HasPremium)
			{
				mercenaryPremium = (TAG_PREMIUM)rewardItemOutputData.Premium;
			}
		}
		if (record.MercenarySelector == RewardItem.MercenarySelector.RANDOM && mercenaryId == 0)
		{
			return new RewardItemDataModel
			{
				Quantity = 1,
				ItemType = RewardItemType.MERCENARY_RANDOM_MERCENARY,
				RandomMercenary = new LettuceRandomMercenaryDataModel
				{
					Premium = mercenaryPremium,
					Rarity = mercenaryRarity
				}
			};
		}
		if (record.MercenarySelector == RewardItem.MercenarySelector.SPECIFIC && artVariationId == 0)
		{
			LettuceMercenary mercenary = CollectionManager.Get()?.GetMercenary(mercenaryId, AttemptToGenerate: false, ReportError: false);
			LettuceMercenaryDataModel lettuceMercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(mercenaryId, artVariationId, mercenaryPremium, mercenary);
			lettuceMercenaryDataModel.Owned = true;
			return new RewardItemDataModel
			{
				Quantity = 1,
				ItemType = RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC,
				Mercenary = lettuceMercenaryDataModel,
				IsMercenaryPortrait = (RewardUtils.IsMercenaryRewardPortrait(lettuceMercenaryDataModel) && mercenary != null && mercenary.m_owned)
			};
		}
		LettuceMercenary mercenary2 = CollectionManager.Get()?.GetMercenary(mercenaryId, AttemptToGenerate: false, ReportError: false);
		LettuceMercenaryDataModel lettuceMercenaryDataModel2 = MercenaryFactory.CreateMercenaryDataModel(mercenaryId, artVariationId, mercenaryPremium, mercenary2);
		lettuceMercenaryDataModel2.Owned = true;
		return new RewardItemDataModel
		{
			Quantity = 1,
			ItemType = RewardItemType.MERCENARY,
			Mercenary = lettuceMercenaryDataModel2,
			IsMercenaryPortrait = RewardUtils.IsMercenaryRewardPortrait(lettuceMercenaryDataModel2)
		};
	}

	public static LettuceMercenaryDataModel CreateFullyUpgradedMercenaryDataModel(int MercenaryId)
	{
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(MercenaryId);
		LettuceMercenaryDataModel lettuceMercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(mercenary);
		CollectionUtils.SetMercenaryStatsByLevel(lettuceMercenaryDataModel, MercenaryId, mercenary.m_level, isFullyUpgraded: false);
		lettuceMercenaryDataModel.FullyUpgradedFinal = true;
		return lettuceMercenaryDataModel;
	}

	private static int GetMercenariesIdForRewardItem(RewardItemDbfRecord record, RewardItemOutputData rewardItemOutputData)
	{
		switch (record.MercenarySelector)
		{
		case RewardItem.MercenarySelector.CONTEXT:
			if (LettuceVillageDataUtil.CurrentTaskContext > 0)
			{
				return LettuceVillageDataUtil.CurrentTaskContext;
			}
			if (rewardItemOutputData != null)
			{
				return rewardItemOutputData.MercenaryId;
			}
			break;
		case RewardItem.MercenarySelector.RANDOM:
			if (rewardItemOutputData != null)
			{
				return rewardItemOutputData.MercenaryId;
			}
			break;
		}
		return record.Mercenary;
	}

	private static RewardItemDataModel CreateMercenaryXPRewardItemDataModel(RewardItemDbfRecord record, RewardItemOutputData rewardItemOutputData = null)
	{
		int mercenaryId = GetMercenariesIdForRewardItem(record, rewardItemOutputData);
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercenaryId);
		CardDbfRecord cardRec = merc.GetCardRecord();
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = RewardItemType.MERCENARY_XP,
			Quantity = record.Quantity,
			ItemId = mercenaryId,
			Card = new CardDataModel
			{
				CardId = cardRec.NoteMiniGuid,
				Premium = TAG_PREMIUM.NORMAL,
				FlavorText = cardRec?.FlavorText
			},
			Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc)
		};
	}

	private static RewardItemDataModel CreateMercenaryCoinRewardItemDataModel(RewardItemDbfRecord record, RewardItemOutputData rewardItemOutputData = null)
	{
		int mercenariesIdForRewardItem = GetMercenariesIdForRewardItem(record, rewardItemOutputData);
		int quantity = record.Quantity;
		if (rewardItemOutputData != null && rewardItemOutputData.HasAmount && rewardItemOutputData.Amount > 0)
		{
			quantity = rewardItemOutputData.Amount;
		}
		return RewardUtils.CreateMercenaryCoinsRewardData(mercenariesIdForRewardItem, quantity, glowActive: false, nameActive: false).DataModel;
	}

	private static RewardItemDataModel CreateMercenaryEquipRewardItemDataModel(RewardItemDbfRecord record)
	{
		int mercenaryId = record.Mercenary;
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercenaryId);
		if (merc == null)
		{
			Log.All.PrintWarning($"MercenaryID not found [{mercenaryId}]");
			return null;
		}
		LettuceAbility equip = merc.GetLettuceEquipment(record.MercenaryEquipment);
		if (equip == null)
		{
			Log.All.PrintWarning($"Equipment ID for reward not found [mercid {mercenaryId}, equipid {record.MercenaryEquipment}]");
			return null;
		}
		LettuceAbilityDataModel equipModel = new LettuceAbilityDataModel();
		CollectionUtils.PopulateDefaultAbilityDataModelWithTier(equipModel, equip, merc, equip.GetBaseTier());
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = RewardItemType.MERCENARY_EQUIPMENT,
			Quantity = record.Quantity,
			ItemId = mercenaryId,
			Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc),
			MercenaryEquip = equipModel
		};
	}

	private static RewardItemDataModel CreateCardBackRewardItemDataModel(RewardItemDbfRecord record)
	{
		if (!GameDbf.CardBack.HasRecord(record.CardBack))
		{
			Log.All.PrintWarning($"Card Back Item has unrecognized card back id [{record.CardBack}]");
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.CardBack,
			CardBack = new CardBackDataModel
			{
				CardBackId = record.CardBack
			}
		};
	}

	private static RewardItemDataModel CreateCurrencyRewardItemDataModel(RewardItemDbfRecord record)
	{
		RewardItemType itemType = record.RewardType.ToRewardItemType();
		CurrencyType currency = RewardUtils.RewardItemTypeToCurrencyType(itemType);
		if (ShopUtils.IsCurrencyVirtual(currency) && !ShopUtils.IsVirtualCurrencyEnabled())
		{
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = itemType,
			Quantity = record.Quantity,
			Currency = new PriceDataModel
			{
				Currency = currency,
				Amount = record.Quantity,
				DisplayText = record.Quantity.ToString()
			}
		};
	}

	private static RewardItemDataModel CreateCustomCoinRewardItemDataModel(RewardItemDbfRecord record)
	{
		CosmeticCoinDbfRecord customCoinRec = GameDbf.CosmeticCoin.GetRecord(record.CustomCoin);
		if (customCoinRec == null)
		{
			Log.All.PrintWarning($"Custom Coin Item has unknown id [{record.CustomCoin}]");
			return null;
		}
		CardDbfRecord cardRec = GameDbf.Card.GetRecord(customCoinRec.CardId);
		if (cardRec == null)
		{
			Log.All.PrintWarning($"Custom Coin Item has unknown card id [{customCoinRec.CardId}]");
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.CustomCoin,
			Card = new CardDataModel
			{
				CardId = cardRec.NoteMiniGuid,
				Premium = (TAG_PREMIUM)record.CardPremiumLevel
			}
		};
	}

	private static RewardItemDataModel CreateBattlegroundsEmoteRewardItemDataModel(RewardItemDbfRecord record)
	{
		BattlegroundsEmoteDbfRecord battlegroundEmoteRec = GameDbf.BattlegroundsEmote.GetRecord(record.BattlegroundsEmoteId);
		if (battlegroundEmoteRec == null)
		{
			Log.All.PrintWarning($"Battleground Emote has unknown id [{record.BattlegroundsEmoteId}]");
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.BattlegroundsEmoteId,
			BGEmote = new BattlegroundsEmoteDataModel
			{
				DisplayName = battlegroundEmoteRec.CollectionShortName,
				Description = battlegroundEmoteRec.Description,
				EmoteDbiId = battlegroundEmoteRec.ID,
				Animation = battlegroundEmoteRec.AnimationPath,
				IsAnimating = battlegroundEmoteRec.IsAnimating,
				BorderType = battlegroundEmoteRec.BorderType,
				XOffset = (float)battlegroundEmoteRec.XOffset,
				ZOffset = (float)battlegroundEmoteRec.ZOffset,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)battlegroundEmoteRec.Rarity))
			}
		};
	}

	private static RewardItemDataModel CreateBattlegroundsBoardSkinRewardItemDataModel(RewardItemDbfRecord record)
	{
		BattlegroundsBoardSkinDbfRecord battlegroundBoardSkinRec = GameDbf.BattlegroundsBoardSkin.GetRecord(record.BattlegroundsBoardSkinId);
		if (battlegroundBoardSkinRec == null)
		{
			Log.All.PrintWarning($"Battleground Board Skin has unknown id [{record.BattlegroundsBoardSkinId}]");
			return null;
		}
		bool enableCosmeticsRendering = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.BattlegroundsBoardSkinId,
			BGBoardSkin = new BattlegroundsBoardSkinDataModel
			{
				BoardDbiId = battlegroundBoardSkinRec.ID,
				DisplayName = battlegroundBoardSkinRec.CollectionShortName,
				DetailsDisplayName = battlegroundBoardSkinRec.CollectionName,
				Description = battlegroundBoardSkinRec.Description,
				ShopDetailsMovie = battlegroundBoardSkinRec.DetailsMovie,
				ShopDetailsTexture = battlegroundBoardSkinRec.DetailsTexture,
				BorderType = battlegroundBoardSkinRec.BorderType,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)battlegroundBoardSkinRec.Rarity)),
				DetailsRenderConfig = battlegroundBoardSkinRec.DetailsRenderConfig,
				CosmeticsRenderingEnabled = enableCosmeticsRendering
			}
		};
	}

	private static RewardItemDataModel CreateBattlegroundsFinisherRewardItemDataModel(RewardItemDbfRecord record)
	{
		BattlegroundsFinisherDbfRecord battlegroundFinisherRec = GameDbf.BattlegroundsFinisher.GetRecord(record.BattlegroundsFinisherId);
		if (battlegroundFinisherRec == null)
		{
			Log.All.PrintWarning($"Battleground Board Skin has unknown id [{record.BattlegroundsBoardSkinId}]");
			return null;
		}
		bool enableCosmeticsRendering = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.BattlegroundsBoardSkinId,
			BGFinisher = new BattlegroundsFinisherDataModel
			{
				FinisherDbiId = battlegroundFinisherRec.ID,
				DisplayName = battlegroundFinisherRec.CollectionShortName,
				DetailsDisplayName = battlegroundFinisherRec.CollectionName,
				Description = battlegroundFinisherRec.Description,
				ShopDetailsMovie = battlegroundFinisherRec.DetailsMovie,
				ShopDetailsTexture = battlegroundFinisherRec.DetailsTexture,
				CapsuleType = battlegroundFinisherRec.CapsuleType,
				BodyMaterial = battlegroundFinisherRec.MiniBodyMaterial,
				ArtMaterial = battlegroundFinisherRec.MiniArtMaterial,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)battlegroundFinisherRec.Rarity)),
				DetailsRenderConfig = battlegroundFinisherRec.DetailsRenderConfig,
				CosmeticsRenderingEnabled = enableCosmeticsRendering
			}
		};
	}

	private static RewardItemDataModel CreateSeasonBonusRewardItemDataModel(RewardItemDbfRecord record)
	{
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			BattlegroundsBonusType = record.BattlegroundsBonusType.ToBattlegroundsBonusType(),
			Quantity = record.Quantity
		};
	}

	private static RewardItemDataModel CreateHeroSkinRewardItemDataModel(RewardItemDbfRecord record)
	{
		CardDbfRecord heroSkinCardRec = GameDbf.Card.GetRecord(record.Card);
		if (GameUtils.GetCardHeroRecordForCardId(heroSkinCardRec.ID) == null)
		{
			Log.All.PrintWarning($"Hero Skin Item has invalid card id [{record.Card}] where card dbf record has" + " no CARD_HERO subtable. NoteMiniGuid = " + heroSkinCardRec?.NoteMiniGuid);
			return null;
		}
		TAG_PREMIUM premium = (TAG_PREMIUM)record.CardPremiumLevel;
		string rarityText = "";
		if (GameUtils.TryGetCardTagRecords(heroSkinCardRec.NoteMiniGuid, out var cardTags))
		{
			CardTagDbfRecord rarityRecord = cardTags.Find((CardTagDbfRecord tagRecord) => tagRecord.TagId == 203);
			rarityText = ((rarityRecord == null) ? "" : GameStrings.GetRarityTextKey((TAG_RARITY)rarityRecord.TagValue));
		}
		if (GameUtils.IsVanillaHero(record.Card))
		{
			premium = TAG_PREMIUM.GOLDEN;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			ItemType = record.RewardType.ToRewardItemType(),
			Quantity = record.Quantity,
			ItemId = record.Card,
			Card = new CardDataModel
			{
				CardId = heroSkinCardRec.NoteMiniGuid,
				Name = heroSkinCardRec.Name,
				FlavorText = heroSkinCardRec.FlavorText,
				Premium = premium,
				Owned = CollectionManager.Get().IsCardInCollection(heroSkinCardRec.NoteMiniGuid, premium),
				Rarity = rarityText
			}
		};
	}

	private static RewardItemDataModel CreateRandomCardRewardItemDataModel(RewardItemDbfRecord record, RewardItemOutputData rewardItemOutputData = null)
	{
		RewardItemDataModel dataModel = new RewardItemDataModel
		{
			AssetId = record.ID,
			Quantity = record.Quantity,
			Booster = new PackDataModel
			{
				Type = (BoosterDbId)record.Booster
			}
		};
		if (rewardItemOutputData != null && rewardItemOutputData.HasCardId)
		{
			int cardId = rewardItemOutputData.CardId;
			CardDbfRecord cardRec = GameDbf.Card.GetRecord(cardId);
			if (cardRec == null)
			{
				Log.All.PrintWarning($"Random Card Item has unknown output card id [{cardId}]");
				return null;
			}
			dataModel.ItemType = RewardItemType.CARD;
			dataModel.ItemId = cardId;
			dataModel.Card = new CardDataModel
			{
				CardId = cardRec.NoteMiniGuid,
				Premium = (TAG_PREMIUM)record.CardPremiumLevel
			};
		}
		else
		{
			TAG_RARITY rarity = RewardUtils.GetRarityForRandomCardReward(record.RandomCardBoosterCardSet);
			if (rarity == TAG_RARITY.INVALID)
			{
				return null;
			}
			dataModel.ItemType = record.RewardType.ToRewardItemType();
			dataModel.RandomCard = new RandomCardDataModel
			{
				Premium = (TAG_PREMIUM)record.CardPremiumLevel,
				Rarity = rarity,
				Count = record.Quantity
			};
		}
		return dataModel;
	}

	private static RewardItemDataModel CreateSimpleRewardItemDataModel(RewardItemDbfRecord record)
	{
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			Quantity = record.Quantity,
			ItemType = record.RewardType.ToRewardItemType()
		};
	}

	private static RewardItemDataModel CreateBattlegroundsHeroSkinRewardItemDataModel(RewardItemDbfRecord record)
	{
		BattlegroundsHeroSkinId skinId = BattlegroundsHeroSkinId.FromTrustedValue(record.BattlegroundsHeroSkinId);
		if (!CollectionManager.Get().GetBattlegroundsHeroSkinCardIdForSkinId(skinId, out var skinCardId))
		{
			Log.All.PrintWarning(string.Format("{0}: Reward record {1} has invalid skin id {2}.", "CreateBattlegroundsHeroSkinRewardItemDataModel", record.ID, record.BattlegroundsGuideSkinId));
			return null;
		}
		CardDbfRecord heroSkinCardRec = GameDbf.Card.GetRecord(skinCardId);
		if (heroSkinCardRec == null)
		{
			Log.All.PrintWarning(string.Format("{0}: Reward record {1} has skin id {2} that resolved to invalid skin card id {3}.", "CreateBattlegroundsHeroSkinRewardItemDataModel", record.ID, record.BattlegroundsGuideSkinId, skinCardId));
			return null;
		}
		if (record.Quantity != 1)
		{
			Log.All.PrintWarning(string.Format("{0}: Reward record {1} has invalid quantity {2}.", "CreateBattlegroundsHeroSkinRewardItemDataModel", record.ID, record.Quantity));
		}
		BattlegroundsHeroSkinDbfRecord battlegroundsHeroSkinRec = GameDbf.BattlegroundsHeroSkin.GetRecord(record.BattlegroundsHeroSkinId);
		if (battlegroundsHeroSkinRec == null)
		{
			Log.All.PrintWarning($"Battlegrounds Hero Skin has unknown id [{record.BattlegroundsHeroSkinId}]");
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			Quantity = 1,
			ItemType = RewardItemType.BATTLEGROUNDS_HERO_SKIN,
			ItemId = skinCardId,
			Card = new CardDataModel
			{
				CardId = heroSkinCardRec.NoteMiniGuid,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)battlegroundsHeroSkinRec.Rarity)),
				Name = heroSkinCardRec.Name,
				FlavorText = GameUtils.GetCardHeroRecordForCardId(heroSkinCardRec.ID)?.Description
			}
		};
	}

	private static RewardItemDataModel CreateBattlegroundsGuideSkinRewardItemDataModel(RewardItemDbfRecord record)
	{
		BattlegroundsGuideSkinId skinId = BattlegroundsGuideSkinId.FromTrustedValue(record.BattlegroundsGuideSkinId);
		if (!CollectionManager.Get().GetBattlegroundsGuideSkinCardIdForSkinId(skinId, out var skinCardId))
		{
			Log.All.PrintWarning(string.Format("{0} invalid skin id {1}.", "CreateBattlegroundsGuideSkinRewardItemDataModel", record.BattlegroundsGuideSkinId));
			return null;
		}
		CardDbfRecord guideSkinCardRec = GameDbf.Card.GetRecord(skinCardId);
		if (guideSkinCardRec == null)
		{
			Log.All.PrintWarning(string.Format("{0} invalid skin card id {1}.", "CreateBattlegroundsGuideSkinRewardItemDataModel", skinCardId));
			return null;
		}
		BattlegroundsGuideSkinDbfRecord battlegroundsGuideSkinRec = GameDbf.BattlegroundsGuideSkin.GetRecord(record.BattlegroundsGuideSkinId);
		if (battlegroundsGuideSkinRec == null)
		{
			Log.All.PrintWarning($"Battleground Bartender Skin has unknown id [{record.BattlegroundsGuideSkinId}]");
			return null;
		}
		return new RewardItemDataModel
		{
			AssetId = record.ID,
			Quantity = 1,
			ItemType = RewardItemType.BATTLEGROUNDS_GUIDE_SKIN,
			ItemId = skinCardId,
			Card = new CardDataModel
			{
				CardId = guideSkinCardRec.NoteMiniGuid,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)battlegroundsGuideSkinRec.Rarity)),
				Name = guideSkinCardRec.Name,
				FlavorText = GameUtils.GetCardHeroRecordForCardId(guideSkinCardRec.ID)?.Description
			}
		};
	}

	public static RewardItemDataModel CreateShopRewardItemDataModel(ProductInfo netBundle, Network.BundleItem netBundleItem, out bool isValidItem)
	{
		RewardItemDataModel item = null;
		switch (netBundleItem.ItemType)
		{
		case ProductType.PRODUCT_TYPE_BOOSTER:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.BOOSTER,
				ItemId = netBundleItem.ProductData,
				Quantity = netBundleItem.Quantity
			};
			break;
		case ProductType.PRODUCT_TYPE_RANDOM_CARD:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.RANDOM_CARD,
				ItemId = netBundleItem.ProductData,
				Quantity = netBundleItem.Quantity
			};
			if (GameDbf.BoosterCardSet.HasRecord(netBundleItem.ProductData))
			{
				TAG_CARD_SET cardSet = (TAG_CARD_SET)GameDbf.BoosterCardSet.GetRecord(netBundleItem.ProductData).CardSetId;
				item.Booster = new PackDataModel
				{
					BoosterName = cardSet.ToString(),
					OverrideWatermark = RewardUtils.IsWatermarkOverridden(netBundleItem.ProductData)
				};
			}
			else if (GameDbf.RewardItem.HasRecord(netBundleItem.ProductData))
			{
				BoosterDbId itemSet = (BoosterDbId)GameDbf.RewardItem.GetRecord(netBundleItem.ProductData).Booster;
				item.Booster = new PackDataModel
				{
					Type = itemSet,
					BoosterName = itemSet.ToString()
				};
			}
			break;
		case ProductType.PRODUCT_TYPE_CURRENCY:
		{
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.UNDEFINED,
				Quantity = netBundleItem.Quantity
			};
			PegasusShared.CurrencyType currencyTypeFromUtilServer = (PegasusShared.CurrencyType)netBundleItem.ProductData;
			switch (currencyTypeFromUtilServer)
			{
			case PegasusShared.CurrencyType.CURRENCY_TYPE_DUST:
				item.ItemType = RewardItemType.DUST;
				break;
			case PegasusShared.CurrencyType.CURRENCY_TYPE_CN_RUNESTONES:
				item.ItemType = RewardItemType.CN_RUNESTONES;
				break;
			case PegasusShared.CurrencyType.CURRENCY_TYPE_CN_ARCANE_ORBS:
				item.ItemType = RewardItemType.CN_ARCANE_ORBS;
				break;
			case PegasusShared.CurrencyType.CURRENCY_TYPE_ROW_RUNESTONES:
				item.ItemType = RewardItemType.ROW_RUNESTONES;
				break;
			case PegasusShared.CurrencyType.CURRENCY_TYPE_BG_TOKEN:
				item.ItemType = RewardItemType.BATTLEGROUNDS_TOKEN;
				break;
			default:
				ProductIssues.LogError(netBundle, $"Has reward with unsupported currency type {currencyTypeFromUtilServer}");
				isValidItem = false;
				return null;
			}
			break;
		}
		case ProductType.PRODUCT_TYPE_DRAFT:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.ARENA_TICKET,
				Quantity = netBundleItem.Quantity
			};
			break;
		case ProductType.PRODUCT_TYPE_CARD_BACK:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.CARD_BACK,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_HERO:
		{
			RewardItemType itemType = RewardItemType.HERO_SKIN;
			int cardDbiId = netBundleItem.ProductData;
			if (CollectionManager.Get().IsBattlegroundsHeroSkinCard(cardDbiId))
			{
				itemType = RewardItemType.BATTLEGROUNDS_HERO_SKIN;
			}
			else if (CollectionManager.Get().IsBattlegroundsGuideSkinCard(cardDbiId))
			{
				itemType = RewardItemType.BATTLEGROUNDS_GUIDE_SKIN;
			}
			item = new RewardItemDataModel
			{
				ItemType = itemType,
				ItemId = cardDbiId,
				Quantity = 1
			};
			break;
		}
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BONUS:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.BATTLEGROUNDS_BONUS,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_BOARD_SKIN:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.BATTLEGROUNDS_BOARD_SKIN,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_FINISHER:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.BATTLEGROUNDS_FINISHER,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_BATTLEGROUNDS_EMOTE:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.BATTLEGROUNDS_EMOTE,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_PROGRESSION_BONUS:
		{
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.PROGRESSION_BONUS,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			if (netBundleItem.Attributes.GetValue("season").TryGetValue(out var season))
			{
				item.Season = season;
			}
			break;
		}
		case ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.TAVERN_BRAWL_TICKET,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_NAXX:
		case ProductType.PRODUCT_TYPE_BRM:
		case ProductType.PRODUCT_TYPE_LOE:
		case ProductType.PRODUCT_TYPE_WING:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.ADVENTURE_WING,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_HIDDEN_LICENSE:
		case ProductType.PRODUCT_TYPE_FIXED_LICENSE:
			isValidItem = true;
			return null;
		case ProductType.PRODUCT_TYPE_MINI_SET:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.MINI_SET,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_SELLABLE_DECK:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.SELLABLE_DECK,
				ItemId = netBundleItem.ProductData,
				Quantity = 1
			};
			break;
		case ProductType.PRODUCT_TYPE_MERCENARIES_BOOSTER:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.MERCENARY_BOOSTER,
				ItemId = netBundleItem.ProductData,
				Quantity = netBundleItem.Quantity
			};
			break;
		case ProductType.PRODUCT_TYPE_MERCENARIES_RANDOM_REWARD:
			if (GameDbf.MercenariesRandomReward.HasRecord(netBundleItem.ProductData))
			{
				MercenariesRandomRewardDbfRecord mercRandomDbfRecord2 = GameDbf.MercenariesRandomReward.GetRecord(netBundleItem.ProductData);
				if (mercRandomDbfRecord2.RewardType == MercenariesRandomReward.RewardType.REWARD_TYPE_MERCENARY)
				{
					item = new RewardItemDataModel
					{
						ItemType = RewardItemType.MERCENARY_RANDOM_MERCENARY,
						ItemId = netBundleItem.ProductData,
						Quantity = netBundleItem.Quantity,
						RandomMercenary = new LettuceRandomMercenaryDataModel
						{
							Premium = (TAG_PREMIUM)mercRandomDbfRecord2.Premium,
							Rarity = (TAG_RARITY)mercRandomDbfRecord2.Rarity,
							RestrictRarity = mercRandomDbfRecord2.RestrictRarity
						}
					};
				}
				else if (mercRandomDbfRecord2.RewardType == MercenariesRandomReward.RewardType.REWARD_TYPE_CURRENCY)
				{
					item = new RewardItemDataModel
					{
						ItemType = RewardItemType.MERCENARY_COIN,
						Quantity = 1,
						MercenaryCoin = new LettuceMercenaryCoinDataModel
						{
							Quantity = netBundleItem.Quantity,
							GlowActive = true,
							IsRandom = true
						}
					};
				}
				break;
			}
			ProductIssues.LogError(netBundle, $"Has license with unrecognized mercenaries random reward ID {netBundleItem.ProductData}");
			isValidItem = false;
			return null;
		case ProductType.PRODUCT_TYPE_MERCENARIES_MERCENARY:
		{
			int artVariationId = 0;
			TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
			if (netBundleItem.Attributes.GetValue("merc_art_variation_id").TryGetValue(out var artVariationString) && !int.TryParse(artVariationString, out artVariationId))
			{
				ProductIssues.LogError(netBundle, "Has license with invalid mercenaries art variation ID " + artVariationString);
				isValidItem = false;
				return null;
			}
			if (netBundleItem.Attributes.GetValue("merc_art_variation_premium").TryGetValue(out var premiumVariationString))
			{
				if (!int.TryParse(premiumVariationString, out var premiumVariationId) || premiumVariationId < 0 || premiumVariationId > 2)
				{
					ProductIssues.LogError(netBundle, "Has license with invalid mercenaries art variation premium value " + premiumVariationString);
					isValidItem = false;
					return null;
				}
				premium = (TAG_PREMIUM)premiumVariationId;
			}
			item = RewardUtils.CreateMercenaryRewardItemDataModel(netBundleItem.ProductData, artVariationId, premium);
			break;
		}
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_SPECIFIC:
			item = RewardUtils.CreateKnockoutSpecificMercenaryRewardItemDataModel(netBundleItem.ProductData);
			break;
		case ProductType.PRODUCT_TYPE_MERCENARIES_KNOCKOUT_RANDOM:
			if (GameDbf.MercenariesRandomReward.HasRecord(netBundleItem.ProductData))
			{
				MercenariesRandomRewardDbfRecord mercRandomDbfRecord = GameDbf.MercenariesRandomReward.GetRecord(netBundleItem.ProductData);
				item = new RewardItemDataModel
				{
					ItemType = RewardItemType.MERCENARY_KNOCKOUT_RANDOM,
					ItemId = netBundleItem.ProductData,
					Quantity = netBundleItem.Quantity,
					RandomMercenary = new LettuceRandomMercenaryDataModel
					{
						Premium = (TAG_PREMIUM)mercRandomDbfRecord.Premium,
						Rarity = (TAG_RARITY)mercRandomDbfRecord.Rarity,
						RestrictRarity = mercRandomDbfRecord.RestrictRarity
					}
				};
				break;
			}
			ProductIssues.LogError(netBundle, $"Has license with unrecognized mercenaries random reward ID {netBundleItem.ProductData}");
			isValidItem = false;
			return null;
		case ProductType.PRODUCT_TYPE_MERCENARIES_CURRENCY:
			item = RewardUtils.CreateMercenaryCoinsRewardData(netBundleItem.ProductData, netBundleItem.Quantity, glowActive: false, nameActive: false).DataModel;
			break;
		case ProductType.PRODUCT_TYPE_DIAMOND_CARD:
		case ProductType.PRODUCT_TYPE_SPECIFIC_CARD:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.CARD,
				ItemId = netBundleItem.ProductData,
				Quantity = netBundleItem.Quantity
			};
			break;
		case ProductType.PRODUCT_TYPE_LUCKY_DRAW:
			item = new RewardItemDataModel
			{
				ItemType = RewardItemType.LUCKY_DRAW,
				ItemId = netBundleItem.ProductData,
				Quantity = netBundleItem.Quantity
			};
			break;
		default:
			ProductIssues.LogError(netBundle, $"Has license with unrecognized reward type [{netBundleItem.ItemType}");
			isValidItem = false;
			return null;
		}
		isValidItem = RewardUtils.InitializeRewardItemDataModelForShop(item, netBundleItem, netBundle);
		return item;
	}

	public static IEnumerable<RewardItemDataModel> ConsolidateGroup(IGrouping<RewardItemType, RewardItemDataModel> group)
	{
		switch (group.Key)
		{
		case RewardItemType.ARENA_TICKET:
		case RewardItemType.GOLD:
			return ConsolidateSimpleRewardItems(group);
		case RewardItemType.REWARD_TRACK_XP_BOOST:
			return ConsolidateRewardTrackXpBoostItems(group);
		case RewardItemType.DUST:
		case RewardItemType.CN_ARCANE_ORBS:
			return ConsolidateCurrencyRewardItems(group);
		case RewardItemType.BOOSTER:
			return ConsolidateBoosterRewardItems(group);
		case RewardItemType.CARD:
			return ConsolidateCardRewardItems(group);
		case RewardItemType.RANDOM_CARD:
			return ConsolidateRandomCardRewardItems(group);
		default:
			return group;
		}
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateBoosterRewardItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by element.ItemId into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = RewardItemType.BOOSTER,
				Quantity = 0,
				ItemId = @group.Key,
				Booster = new PackDataModel
				{
					Type = (BoosterDbId)@group.Key,
					Quantity = 0
				}
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity += element.Quantity;
				acc.Booster.Quantity += element.Booster.Quantity;
				return acc;
			});
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateCardRewardItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by (ItemType: element.ItemType, ItemId: element.ItemId, Premium: element.Card.Premium) into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = @group.Key.ToTuple().Item1,
				Quantity = 0,
				ItemId = @group.Key.ToTuple().Item2,
				Card = new CardDataModel
				{
					Premium = @group.Key.ToTuple().Item3
				}
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity += element.Quantity;
				acc.Card.CardId = element.Card.CardId;
				return acc;
			});
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateCurrencyRewardItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by element.ItemType into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = @group.Key,
				Quantity = 0,
				Currency = new PriceDataModel
				{
					Currency = RewardUtils.RewardItemTypeToCurrencyType(@group.Key),
					Amount = 0f
				}
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity += element.Quantity;
				acc.Currency.Amount += element.Currency.Amount;
				acc.Currency.DisplayText = acc.Quantity.ToString();
				return acc;
			});
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateRandomCardRewardItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by (ItemId: element.ItemId, Premium: element.RandomCard.Premium, Rarity: element.RandomCard.Rarity) into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = RewardItemType.RANDOM_CARD,
				Quantity = 0,
				ItemId = @group.Key.ToTuple().Item1,
				RandomCard = new RandomCardDataModel
				{
					Premium = @group.Key.ToTuple().Item2,
					Rarity = @group.Key.ToTuple().Item3,
					Count = 0
				}
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity += element.Quantity;
				acc.Card.CardId = element.Card.CardId;
				return acc;
			});
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateRewardTrackXpBoostItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by element.ItemType into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = @group.Key,
				Quantity = 0
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity = Math.Max(acc.Quantity, element.Quantity);
				return acc;
			});
	}

	private static IEnumerable<RewardItemDataModel> ConsolidateSimpleRewardItems(IEnumerable<RewardItemDataModel> rewards)
	{
		return from element in rewards
			group element by element.ItemType into @group
			select @group.Aggregate(new RewardItemDataModel
			{
				ItemType = @group.Key,
				Quantity = 0
			}, delegate(RewardItemDataModel acc, RewardItemDataModel element)
			{
				acc.AssetId = element.AssetId;
				acc.Quantity += element.Quantity;
				return acc;
			});
	}

	public static RewardListDataModel CreateMercenaryRewardItemDataModel(RewardChest chest)
	{
		RewardListDataModel result = new RewardListDataModel();
		RewardItemDataModel rewardItem = null;
		foreach (PegasusShared.RewardBag bag in chest.Bag)
		{
			if (bag.HasRewardMercenariesCurrency && bag.RewardMercenariesCurrency.HasCurrencyDelta)
			{
				string cardId = GameUtils.GetCardIdFromMercenaryId(bag.RewardMercenariesCurrency.MercenaryId);
				EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
				if (entityDef == null)
				{
					Log.Lettuce.PrintError("OnMercenaryCoinRewardFullDefLoaded - Failed to load def for card {0}", cardId);
				}
				rewardItem = new RewardItemDataModel
				{
					ItemType = RewardItemType.MERCENARY_COIN,
					MercenaryCoin = new LettuceMercenaryCoinDataModel
					{
						MercenaryId = bag.RewardMercenariesCurrency.MercenaryId,
						MercenaryName = entityDef?.GetName(),
						Quantity = (int)bag.RewardMercenariesCurrency.CurrencyDelta,
						GlowActive = true
					}
				};
			}
			else if (bag.HasRewardMercenariesExperience && bag.RewardMercenariesExperience.HasPreExp && bag.RewardMercenariesExperience.HasPostExp)
			{
				LettuceMercenary merc = CollectionManager.Get().GetMercenary(bag.RewardMercenariesExperience.MercenaryId);
				rewardItem = new RewardItemDataModel
				{
					ItemType = RewardItemType.MERCENARY,
					Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc)
				};
				rewardItem.Mercenary.ExperienceInitial = (int)bag.RewardMercenariesExperience.PreExp;
				rewardItem.Mercenary.ExperienceFinal = (int)bag.RewardMercenariesExperience.PostExp;
				rewardItem.Mercenary.Owned = true;
				GameUtils.GetMercenaryLevelFromExperience(rewardItem.Mercenary.ExperienceInitial);
				CollectionUtils.PopulateMercenaryCardDataModel(rewardItem.Mercenary, merc.GetEquippedArtVariation());
				CollectionUtils.SetMercenaryStatsByLevel(rewardItem.Mercenary, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
			}
			else if (bag.HasRewardMercenariesEquipment)
			{
				Log.Lettuce.PrintError("CreateRewardItemDataModel - Mercenaries Equipment unsupported");
			}
			else if (bag.HasRewardRenown)
			{
				rewardItem = new RewardItemDataModel
				{
					ItemType = RewardItemType.MERCENARY_RENOWN,
					Currency = new PriceDataModel
					{
						Currency = CurrencyType.RENOWN,
						Amount = bag.RewardRenown.Amount
					}
				};
			}
			else
			{
				Log.Lettuce.PrintError("Cannot handle unknown mercenary reward bag contents in RewardFactory");
			}
			if (rewardItem != null)
			{
				result.Items.Add(rewardItem);
				rewardItem = null;
			}
		}
		return result;
	}
}
