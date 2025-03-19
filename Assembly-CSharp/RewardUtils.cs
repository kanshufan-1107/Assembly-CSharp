using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Commerce;
using Hearthstone.Core;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.Streaming;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class RewardUtils
{
	private class RewardDisplayCallbackData
	{
		public List<RewardData> rewardsToDisplay;

		public Reward currentReward;

		public int rewardIndex;

		public Action doneCallback;
	}

	public class RewardItemComparer : IComparer<RewardItemDataModel>
	{
		public int Compare(RewardItemDataModel first, RewardItemDataModel second)
		{
			return CompareItemsForSort(first, second);
		}
	}

	public class RewardOwnedItemComparer : IComparer<RewardItemDataModel>
	{
		public int Compare(RewardItemDataModel first, RewardItemDataModel second)
		{
			return CompareOwnedItemsForSort(first, second);
		}
	}

	public static readonly Vector3 RewardHiddenScale = new Vector3(0.001f, 0.001f, 0.001f);

	public static readonly float RewardHideTime = 0.25f;

	public static readonly float MercRewardEndBlurTime = 0.1f;

	private static readonly AssetReference s_questRewardsTexturePage2 = new AssetReference("QuestRewards2.psd:1de88a86bd486434dab6ab887ca40254");

	private static readonly AssetReference s_arcaneOrbIcon = new AssetReference("Shop_VC2_Arcane_Orb_Icon.tif:b47e50430b8b4554688cc9e385ced3f2");

	public static List<RewardData> GetRewards(List<NetCache.ProfileNotice> notices)
	{
		List<RewardData> rewardDataList = new List<RewardData>();
		foreach (NetCache.ProfileNotice notice in notices)
		{
			RewardData rewardData = null;
			switch (notice.Type)
			{
			case NetCache.ProfileNotice.NoticeType.REWARD_BOOSTER:
			{
				NetCache.ProfileNoticeRewardBooster boosterReward = notice as NetCache.ProfileNoticeRewardBooster;
				rewardData = new BoosterPackRewardData(boosterReward.Id, boosterReward.Count);
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_CARD:
			{
				NetCache.ProfileNoticeRewardCard cardReward = notice as NetCache.ProfileNoticeRewardCard;
				rewardData = new CardRewardData(cardReward.CardID, cardReward.Premium, cardReward.Quantity);
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_BATTLEGROUNDS_GUIDE:
			{
				NetCache.ProfileNoticeRewardBattlegroundsGuideSkin battlegroundsGuideReward = notice as NetCache.ProfileNoticeRewardBattlegroundsGuideSkin;
				TryGetToastTextFromFixedRewardMap(battlegroundsGuideReward?.FixedRewardMapID ?? 0, out var toastName4, out var toastDesc4, out var shouldSkipToast4);
				if (shouldSkipToast4)
				{
					Network.Get().AckNotice(notice.NoticeID);
					break;
				}
				rewardData = new CardRewardData(battlegroundsGuideReward.CardID, TAG_PREMIUM.NORMAL, 1);
				rewardData.NameOverride = toastName4;
				rewardData.DescriptionOverride = toastDesc4;
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_BATTLEGROUNDS_FINISHER:
			{
				NetCache.ProfileNoticeRewardBattlegroundsFinisher finisherReward = notice as NetCache.ProfileNoticeRewardBattlegroundsFinisher;
				TryGetToastTextFromFixedRewardMap(finisherReward?.FixedRewardMapID ?? 0, out var toastName5, out var toastDesc5, out var shouldSkipToast5);
				if (shouldSkipToast5)
				{
					Network.Get().AckNotice(notice.NoticeID);
					break;
				}
				CollectibleBattlegroundsFinisher finisher2 = new CollectibleBattlegroundsFinisher(GameDbf.BattlegroundsFinisher.GetRecord((int)finisherReward.FinisherID));
				rewardData = new BattlegroundsFinisherRewardData(finisherReward.FinisherID, finisher2.CreateFinisherDataModel());
				rewardData.NameOverride = toastName5;
				rewardData.DescriptionOverride = toastDesc5;
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_BATTLEGROUNDS_BOARD_SKIN:
			{
				NetCache.ProfileNoticeRewardBattlegroundsBoard boardSkinReward = notice as NetCache.ProfileNoticeRewardBattlegroundsBoard;
				TryGetToastTextFromFixedRewardMap(boardSkinReward?.FixedRewardMapID ?? 0, out var toastName3, out var toastDesc3, out var shouldSkipToast3);
				if (shouldSkipToast3)
				{
					Network.Get().AckNotice(notice.NoticeID);
					break;
				}
				CollectibleBattlegroundsBoard boardSkin2 = new CollectibleBattlegroundsBoard(GameDbf.BattlegroundsBoardSkin.GetRecord((int)boardSkinReward.BoardSkinID));
				rewardData = new BattlegroundsBoardSkinRewardData(boardSkinReward.BoardSkinID, boardSkin2.CreateBoardDataModel());
				rewardData.NameOverride = toastName3;
				rewardData.DescriptionOverride = toastDesc3;
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_BATTLEGROUNDS_HERO:
			{
				NetCache.ProfileNoticeRewardBattlegroundsHeroSkin battlegroundsHeroSkinReward = notice as NetCache.ProfileNoticeRewardBattlegroundsHeroSkin;
				TryGetToastTextFromFixedRewardMap(battlegroundsHeroSkinReward?.FixedRewardMapID ?? 0, out var toastName, out var toastDesc, out var shouldSkipToast);
				if (shouldSkipToast)
				{
					Network.Get().AckNotice(notice.NoticeID);
					break;
				}
				rewardData = new CardRewardData(battlegroundsHeroSkinReward.CardID, TAG_PREMIUM.NORMAL, 1);
				rewardData.NameOverride = toastName;
				rewardData.DescriptionOverride = toastDesc;
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_BATTLEGROUNDS_EMOTE:
			{
				NetCache.ProfileNoticeRewardBattlegroundsEmote emoteReward = notice as NetCache.ProfileNoticeRewardBattlegroundsEmote;
				TryGetToastTextFromFixedRewardMap(emoteReward?.FixedRewardMapID ?? 0, out var toastName2, out var toastDesc2, out var shouldSkipToast2);
				if (shouldSkipToast2)
				{
					Network.Get().AckNotice(notice.NoticeID);
					break;
				}
				CollectibleBattlegroundsEmote emote2 = new CollectibleBattlegroundsEmote(GameDbf.BattlegroundsEmote.GetRecord((int)emoteReward.EmoteID));
				rewardData = new BattlegroundsEmoteRewardData(emoteReward.EmoteID, emote2.CreateEmoteDataModel());
				rewardData.NameOverride = toastName2;
				rewardData.DescriptionOverride = toastDesc2;
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_DUST:
				if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.HOF_COMPENSATION)
				{
					continue;
				}
				rewardData = new ArcaneDustRewardData((notice as NetCache.ProfileNoticeRewardDust).Amount);
				break;
			case NetCache.ProfileNotice.NoticeType.REWARD_MOUNT:
				rewardData = new MountRewardData((MountRewardData.MountType)(notice as NetCache.ProfileNoticeRewardMount).MountID);
				break;
			case NetCache.ProfileNotice.NoticeType.REWARD_FORGE:
				rewardData = new ForgeTicketRewardData((notice as NetCache.ProfileNoticeRewardForge).Quantity);
				break;
			case NetCache.ProfileNotice.NoticeType.REWARD_CURRENCY:
			{
				NetCache.ProfileNoticeRewardCurrency currencyReward = notice as NetCache.ProfileNoticeRewardCurrency;
				switch (currencyReward.CurrencyType)
				{
				case PegasusShared.CurrencyType.CURRENCY_TYPE_CN_ARCANE_ORBS:
					rewardData = CreateArcaneOrbRewardData(currencyReward.Amount);
					break;
				case PegasusShared.CurrencyType.CURRENCY_TYPE_GOLD:
					rewardData = new GoldRewardData(currencyReward.Amount, DateTime.FromFileTimeUtc(currencyReward.Date));
					break;
				case PegasusShared.CurrencyType.CURRENCY_TYPE_RENOWN:
					rewardData = new MercenaryRenownRewardData(currencyReward.Amount);
					break;
				case PegasusShared.CurrencyType.CURRENCY_TYPE_BG_TOKEN:
					rewardData = new BattlegroundsTokenRewardData(currencyReward.Amount, DateTime.FromFileTimeUtc(currencyReward.Date));
					break;
				}
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_CARD_BACK:
				rewardData = new CardBackRewardData((notice as NetCache.ProfileNoticeRewardCardBack).CardBackID);
				break;
			case NetCache.ProfileNotice.NoticeType.EVENT:
				rewardData = new EventRewardData((notice as NetCache.ProfileNoticeEvent).EventType);
				break;
			case NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST:
				AddRewardDataForGenericRewardChest(notice as NetCache.ProfileNoticeGenericRewardChest, ref rewardDataList);
				rewardData = null;
				break;
			case NetCache.ProfileNotice.NoticeType.MINI_SET_GRANTED:
			{
				NetCache.ProfileNoticeMiniSetGranted miniSetReward = notice as NetCache.ProfileNoticeMiniSetGranted;
				MiniSetDbfRecord record = GameDbf.MiniSet.GetRecord(miniSetReward.MiniSetID);
				if (record != null && record.HideOnClient)
				{
					Network.Get().AckNotice(notice.NoticeID);
				}
				else
				{
					rewardData = new MiniSetRewardData(miniSetReward.MiniSetID, miniSetReward.Premium);
				}
				break;
			}
			case NetCache.ProfileNotice.NoticeType.MERCENARIES_ABILITY_UNLOCK:
			{
				NetCache.ProfileNoticeMercenariesAbilityUnlock abilityUnlockNotice = notice as NetCache.ProfileNoticeMercenariesAbilityUnlock;
				rewardData = new MercenariesAbilityUnlockRewardData(abilityUnlockNotice.MercenaryId, abilityUnlockNotice.AbilityId);
				break;
			}
			case NetCache.ProfileNotice.NoticeType.MERCENARIES_BOOSTER_LICENSE:
				rewardData = BoosterPackRewardData.CreateMercenariesBoosterPackRewardData((notice as NetCache.ProfileNoticeMercenariesBoosterLicense).Count);
				break;
			case NetCache.ProfileNotice.NoticeType.MERCENARIES_MERC_LICENSE:
			{
				NetCache.ProfileNoticeMercenariesMercenaryLicense mercNotice = notice as NetCache.ProfileNoticeMercenariesMercenaryLicense;
				rewardData = CreateMercenaryOrKnockoutRewardData(mercNotice.MercenaryId, mercNotice.ArtVariationId, (TAG_PREMIUM)mercNotice.ArtVariationPremium, (int)mercNotice.CurrencyAmount);
				if (rewardData == null)
				{
					continue;
				}
				break;
			}
			case NetCache.ProfileNotice.NoticeType.MERCENARIES_RANDOM_REWARD_LICENSE:
			{
				NetCache.ProfileNoticeMercenariesRandomRewardLicense randomRewardNotice = notice as NetCache.ProfileNoticeMercenariesRandomRewardLicense;
				RewardItemDataModel mercRewardItem = CreateMercenaryRewardItemDataModel(randomRewardNotice.MercenaryId, randomRewardNotice.ArtVariationId, (TAG_PREMIUM)randomRewardNotice.ArtVariationPremium);
				GetMercenaryName(mercRewardItem.Mercenary.Card.CardId, out var mercName2, out var shortMercName2);
				if (mercName2 == null)
				{
					continue;
				}
				if (randomRewardNotice.IsConvertedMercenary)
				{
					RewardItemDataModel mercKnockoutReward = CreateMercenaryCoinsRewardData(randomRewardNotice.MercenaryId, (int)randomRewardNotice.CurrencyAmount, glowActive: true, nameActive: false).DataModel;
					rewardData = new MercenariesKnockoutRewardData(mercRewardItem, mercKnockoutReward);
					rewardData.NameOverride = GetMercenaryRarityText(mercRewardItem.Mercenary.MercenaryRarity);
					rewardData.DescriptionOverride = GetMercenaryKnockoutCoinsText((TAG_PREMIUM)randomRewardNotice.ArtVariationPremium, mercName2, shortMercName2);
				}
				else if (randomRewardNotice.CurrencyAmount > 0)
				{
					rewardData = CreateMercenaryCoinsRewardData(randomRewardNotice.MercenaryId, (int)randomRewardNotice.CurrencyAmount, glowActive: true, nameActive: false);
					rewardData.NameOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_COINS_TITLE", shortMercName2);
					rewardData.DescriptionOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_COINS_DESC", shortMercName2);
				}
				else
				{
					rewardData = new RewardItemRewardData(mercRewardItem, showQuestToast: true, Reward.Type.MERCENARY_RANDOM_MERCENARY);
					rewardData.NameOverride = GetMercenaryRarityText(mercRewardItem.Mercenary.MercenaryRarity);
					rewardData.DescriptionOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_DESC", shortMercName2);
				}
				break;
			}
			case NetCache.ProfileNotice.NoticeType.MERCENARIES_CURRENCY_LICENSE:
			{
				NetCache.ProfileNoticeMercenariesCurrencyLicense currencyNotice = notice as NetCache.ProfileNoticeMercenariesCurrencyLicense;
				rewardData = CreateMercenaryCoinsRewardData(currencyNotice.MercenaryId, (int)currencyNotice.CurrencyAmount, glowActive: true, nameActive: false);
				GetMercenaryName(LettuceMercenary.GetDefaultArtVariationRecord(currencyNotice.MercenaryId).CardRecord.NoteMiniGuid, out var mercName, out var shortMercName);
				if (mercName == null)
				{
					continue;
				}
				rewardData.NameOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_COINS_TITLE", shortMercName);
				rewardData.DescriptionOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_COINS_DESC", shortMercName);
				break;
			}
			case NetCache.ProfileNotice.NoticeType.REWARD_LUCKY_DRAW:
			{
				NetCache.ProfileNoticeLuckyDrawReward luckyDrawNotice = notice as NetCache.ProfileNoticeLuckyDrawReward;
				LuckyDrawRewardsDbfRecord rewardRecord = GameDbf.LuckyDrawRewards.GetRecord(luckyDrawNotice.LuckyDrawRewardId);
				if (rewardRecord == null)
				{
					Debug.LogErrorFormat("REWARD_LUCKY_DRAW invalid LuckyDrawRewardId: {0}", luckyDrawNotice.LuckyDrawRewardId);
					continue;
				}
				rewardData = null;
				int rewardNumber = 0;
				foreach (RewardItemDbfRecord reward in GameUtils.GetRewardItemsForRewardList(rewardRecord.RewardListRecord.ID))
				{
					RewardData luckyDrawRewardData = null;
					switch (reward.RewardType)
					{
					case RewardItem.RewardType.BATTLEGROUNDS_BOARD_SKIN:
					{
						BattlegroundsBoardSkinDbfRecord boardSkinRecord = GameDbf.BattlegroundsBoardSkin.GetRecord(reward.BattlegroundsBoardSkinId);
						CollectibleBattlegroundsBoard boardSkin = new CollectibleBattlegroundsBoard(boardSkinRecord);
						luckyDrawRewardData = new BattlegroundsBoardSkinRewardData(reward.BattlegroundsBoardSkinId, boardSkin.CreateBoardDataModel());
						luckyDrawRewardData.DescriptionOverride = GameStrings.Format("GLUE_BATTLEBASH_REWARD_NOTICE_BODY_BOARD", boardSkinRecord.CollectionName.GetString());
						break;
					}
					case RewardItem.RewardType.BATTLEGROUNDS_EMOTE:
					{
						BattlegroundsEmoteDbfRecord emoteRecord = GameDbf.BattlegroundsEmote.GetRecord(reward.BattlegroundsEmoteId);
						CollectibleBattlegroundsEmote emote = new CollectibleBattlegroundsEmote(emoteRecord);
						luckyDrawRewardData = new BattlegroundsEmoteRewardData(reward.BattlegroundsEmoteId, emote.CreateEmoteDataModel());
						string formatString = ((reward.Quantity > 1) ? "GLUE_BATTLEBASH_REWARD_NOTICE_BODY_EMOTEBUNDLE" : "GLUE_BATTLEBASH_REWARD_NOTICE_BODY_EMOTE");
						luckyDrawRewardData.DescriptionOverride = GameStrings.Format(formatString, emoteRecord.CollectionShortName.GetString());
						break;
					}
					case RewardItem.RewardType.BATTLEGROUNDS_FINISHER:
					{
						BattlegroundsFinisherDbfRecord finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(reward.BattlegroundsFinisherId);
						CollectibleBattlegroundsFinisher finisher = new CollectibleBattlegroundsFinisher(finisherRecord);
						luckyDrawRewardData = new BattlegroundsFinisherRewardData(reward.BattlegroundsFinisherId, finisher.CreateFinisherDataModel());
						luckyDrawRewardData.DescriptionOverride = GameStrings.Format("GLUE_BATTLEBASH_REWARD_NOTICE_BODY_STRIKE", finisherRecord.CollectionName.GetString());
						break;
					}
					case RewardItem.RewardType.BATTLEGROUNDS_GUIDE_SKIN:
						luckyDrawRewardData = new CardRewardData(GameUtils.TranslateDbIdToCardId(reward.BattlegroundsGuideSkinRecord.SkinCardId), TAG_PREMIUM.NORMAL, 1, showQuestToast: true);
						luckyDrawRewardData.DescriptionOverride = GameStrings.Format("GLUE_BATTLEBASH_REWARD_NOTICE_BODY_BARTENDER", reward.BattlegroundsGuideSkinRecord.SkinCardRecord.Name.GetString());
						break;
					case RewardItem.RewardType.BATTLEGROUNDS_HERO_SKIN:
						luckyDrawRewardData = new CardRewardData(GameUtils.TranslateDbIdToCardId(reward.BattlegroundsHeroSkinRecord.SkinCardId), TAG_PREMIUM.NORMAL, 1, showQuestToast: true);
						luckyDrawRewardData.DescriptionOverride = GameStrings.Format("GLUE_BATTLEBASH_REWARD_NOTICE_BODY_SKIN", reward.BattlegroundsHeroSkinRecord.SkinCardRecord.Name.GetString());
						break;
					default:
						Debug.LogErrorFormat("REWARD_LUCKY_DRAW invalid reward type: {0}", reward.RewardType.ToString());
						break;
					}
					if (luckyDrawRewardData != null)
					{
						string nameOverrideKey = ((luckyDrawNotice.LuckyDrawOrigin == ProfileNoticeLuckyDrawReward.OriginType.ORIGIN_AUTO_GRANT_FROM_EXPIRED_BOX) ? "GLUE_BATTLEBASH_REWARD_NOTICE_TITLE_EXPIRED" : "GLUE_BATTLEBASH_REWARD_NOTICE_TITLE");
						luckyDrawRewardData.NameOverride = GameStrings.Get(nameOverrideKey);
						luckyDrawRewardData.SetOrigin(NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_LUCKY_DRAW, rewardNumber++);
						luckyDrawRewardData.AddNoticeID(notice.NoticeID);
						AddRewardDataToList(luckyDrawRewardData, rewardDataList);
					}
				}
				break;
			}
			default:
				continue;
			}
			if (rewardData != null)
			{
				SetNoticeAndAddRewardDataToList(notice, ref rewardData, ref rewardDataList);
			}
		}
		return rewardDataList;
	}

	private static void SetNoticeAndAddRewardDataToList(NetCache.ProfileNotice notice, ref RewardData rewardData, ref List<RewardData> rewardDataList)
	{
		rewardData.SetOrigin(notice.Origin, notice.OriginData);
		rewardData.AddNoticeID(notice.NoticeID);
		AddRewardDataToList(rewardData, rewardDataList);
	}

	private static void AddRewardDataForGenericRewardChest(NetCache.ProfileNoticeGenericRewardChest notice, ref List<RewardData> rewardDataList)
	{
		RewardChest rewardChest = notice.RewardChest;
		if (rewardChest != null)
		{
			AddRewardDataForGenericRewardChestBag(notice, rewardChest.Bag1, 1, ref rewardDataList);
			AddRewardDataForGenericRewardChestBag(notice, rewardChest.Bag2, 2, ref rewardDataList);
			AddRewardDataForGenericRewardChestBag(notice, rewardChest.Bag3, 3, ref rewardDataList);
			AddRewardDataForGenericRewardChestBag(notice, rewardChest.Bag4, 4, ref rewardDataList);
			AddRewardDataForGenericRewardChestBag(notice, rewardChest.Bag5, 5, ref rewardDataList);
		}
	}

	private static void AddRewardDataForGenericRewardChestBag(NetCache.ProfileNoticeGenericRewardChest notice, PegasusShared.RewardBag rewardBag, int bagNum, ref List<RewardData> rewardDataList)
	{
		if (rewardBag == null)
		{
			return;
		}
		string rewardBannerName = string.Empty;
		string rewardBannerDescription = string.Empty;
		if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE)
		{
			AchieveDbfRecord achieve = GameDbf.Achieve.GetRecord((int)notice.OriginData);
			if (achieve != null)
			{
				rewardBannerName = achieve.Name;
				rewardBannerDescription = achieve.Description;
			}
		}
		if ((string.IsNullOrEmpty(rewardBannerName) || string.IsNullOrEmpty(rewardBannerDescription)) && GameDbf.RewardChest.HasRecord(notice.RewardChestAssetId))
		{
			RewardChestDbfRecord record = GameDbf.RewardChest.GetRecord(notice.RewardChestAssetId);
			if (record.Name != null && string.IsNullOrEmpty(rewardBannerName))
			{
				rewardBannerName = record.Name.GetString();
			}
			if (record.Description != null && string.IsNullOrEmpty(rewardBannerDescription))
			{
				rewardBannerDescription = record.Description.GetString();
			}
		}
		RewardData rewardData = Network.ConvertRewardBag(rewardBag);
		if (rewardData != null)
		{
			rewardData.RewardChestAssetId = notice.RewardChestAssetId;
			rewardData.RewardChestBagNum = bagNum;
			rewardData.NameOverride = rewardBannerName;
			rewardData.DescriptionOverride = rewardBannerDescription;
			SetNoticeAndAddRewardDataToList(notice, ref rewardData, ref rewardDataList);
		}
	}

	public static void GetViewableRewards(List<RewardData> rewardDataList, HashSet<Assets.Achieve.RewardTiming> rewardTimings, out List<RewardData> rewardsToShow, out List<RewardData> genericRewardChestsToShow, ref List<RewardData> purchasedCardRewardsToShow, ref List<Achievement> completedQuests)
	{
		bool canViewAchieves = GameUtils.IsAnyTutorialComplete();
		rewardsToShow = new List<RewardData>();
		genericRewardChestsToShow = new List<RewardData>();
		if (completedQuests == null)
		{
			completedQuests = new List<Achievement>();
		}
		foreach (RewardData rewardData in rewardDataList)
		{
			Log.Achievements.Print("RewardUtils.GetViewableRewards() - processing reward {0}", rewardData);
			if (NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT == rewardData.Origin)
			{
				if (!canViewAchieves)
				{
					continue;
				}
				Achievement completedQuest = AchieveManager.Get().GetAchievement((int)rewardData.OriginData);
				if (completedQuest == null)
				{
					continue;
				}
				List<long> rewardNotices = rewardData.GetNoticeIDs();
				Achievement existingCompleteAchieve = completedQuests.Find((Achievement obj) => completedQuest.ID == obj.ID);
				if (existingCompleteAchieve != null)
				{
					foreach (long noticeID in rewardNotices)
					{
						existingCompleteAchieve.AddRewardNoticeID(noticeID);
					}
					continue;
				}
				foreach (long noticeID2 in rewardNotices)
				{
					completedQuest.AddRewardNoticeID(noticeID2);
				}
				if (rewardTimings.Contains(completedQuest.RewardTiming))
				{
					completedQuests.Add(completedQuest);
				}
				continue;
			}
			if (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE)
			{
				Achievement matchingAchievement = AchieveManager.Get().GetAchievement((int)rewardData.OriginData);
				if (matchingAchievement == null || rewardTimings.Contains(matchingAchievement.RewardTiming))
				{
					genericRewardChestsToShow.Add(rewardData);
				}
				continue;
			}
			if (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST)
			{
				if (rewardData.Origin != NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_DUELS)
				{
					genericRewardChestsToShow.Add(rewardData);
				}
				continue;
			}
			bool loadReward = false;
			IGameDownloadManager DownloadManager = GameDownloadManagerProvider.Get();
			switch (rewardData.RewardType)
			{
			case Reward.Type.CARD:
			{
				CardRewardData cardReward = rewardData as CardRewardData;
				if ((GameUtils.GetCardHasTag(cardReward.CardID, GAME_TAG.BACON_SKIN) || GameUtils.GetCardHasTag(cardReward.CardID, GAME_TAG.BACON_BOB_SKIN)) && !DownloadManager.IsModuleReadyToPlay(DownloadTags.Content.Bgs))
				{
					loadReward = false;
				}
				else if (cardReward.CardID.Equals("HERO_08") && cardReward.Premium == TAG_PREMIUM.NORMAL)
				{
					loadReward = false;
					rewardData.AcknowledgeNotices();
					CollectionManager.Get().AddCardReward(cardReward, markAsNew: false);
				}
				else if (NetCache.ProfileNotice.NoticeOrigin.FROM_PURCHASE == rewardData.Origin || NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE == rewardData.Origin)
				{
					loadReward = false;
					if (StoreManager.Get() != null && StoreManager.Get().WillStoreDisplayNotice(rewardData.Origin, NetCache.ProfileNotice.NoticeType.REWARD_CARD, rewardData.OriginData))
					{
						rewardData.AcknowledgeNotices();
					}
					else if (purchasedCardRewardsToShow != null)
					{
						purchasedCardRewardsToShow.Add(rewardData);
					}
				}
				else
				{
					loadReward = true;
				}
				break;
			}
			case Reward.Type.MINI_SET:
				loadReward = false;
				if (purchasedCardRewardsToShow != null)
				{
					purchasedCardRewardsToShow.Add(rewardData);
				}
				break;
			case Reward.Type.ARCANE_DUST:
			case Reward.Type.BOOSTER_PACK:
			case Reward.Type.GOLD:
			case Reward.Type.MERCENARY_COIN:
			case Reward.Type.MERCENARY_ABILITY_UNLOCK:
			case Reward.Type.MERCENARY_BOOSTER:
			case Reward.Type.MERCENARY_MERCENARY:
			case Reward.Type.MERCENARY_RANDOM_MERCENARY:
			case Reward.Type.MERCENARY_KNOCKOUT:
			case Reward.Type.BATTLEGROUNDS_FINISHER:
			case Reward.Type.BATTLEGROUNDS_BOARD_SKIN:
			case Reward.Type.BATTLEGROUNDS_EMOTE:
			case Reward.Type.MERCENARY_RENOWN:
			case Reward.Type.BATTLEGROUNDS_TOKEN:
				loadReward = true;
				break;
			case Reward.Type.FORGE_TICKET:
			{
				bool autoAckNotices = false;
				if (NetCache.ProfileNotice.NoticeOrigin.BLIZZCON == rewardData.Origin && 2013 == rewardData.OriginData)
				{
					autoAckNotices = true;
				}
				if (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.OUT_OF_BAND_LICENSE)
				{
					Log.Achievements.Print($"RewardUtils.GetViewableRewards(): auto-acking notices for out of band license reward {rewardData}");
					autoAckNotices = true;
				}
				if (autoAckNotices)
				{
					rewardData.AcknowledgeNotices();
				}
				loadReward = false;
				break;
			}
			case Reward.Type.CARD_BACK:
				loadReward = NetCache.ProfileNotice.NoticeOrigin.SEASON != rewardData.Origin;
				break;
			}
			if (IsRewardTypeBattlegrounds(rewardData.RewardType) && !DownloadManager.IsModuleReadyToPlay(DownloadTags.Content.Bgs))
			{
				loadReward = false;
			}
			if (IsRewardTypeMercenaries(rewardData.RewardType) && !DownloadManager.IsModuleReadyToPlay(DownloadTags.Content.Merc))
			{
				loadReward = false;
			}
			if (loadReward)
			{
				rewardsToShow.Add(rewardData);
			}
		}
	}

	private static bool IsRewardTypeBattlegrounds(Reward.Type rewardType)
	{
		if (rewardType != Reward.Type.BATTLEGROUNDS_BOARD_SKIN && rewardType != Reward.Type.BATTLEGROUNDS_EMOTE && rewardType != Reward.Type.BATTLEGROUNDS_FINISHER && rewardType != Reward.Type.BATTLEGROUNDS_GUIDE_SKIN && rewardType != Reward.Type.BATTLEGROUNDS_HERO_SKIN)
		{
			return rewardType == Reward.Type.BATTLEGROUNDS_TOKEN;
		}
		return true;
	}

	private static bool IsRewardTypeMercenaries(Reward.Type rewardType)
	{
		if (rewardType != Reward.Type.MERCENARY_ABILITY_UNLOCK && rewardType != Reward.Type.MERCENARY_BOOSTER && rewardType != Reward.Type.MERCENARY_COIN && rewardType != Reward.Type.MERCENARY_EQUIPMENT && rewardType != Reward.Type.MERCENARY_EXP && rewardType != Reward.Type.MERCENARY_KNOCKOUT && rewardType != Reward.Type.MERCENARY_MERCENARY && rewardType != Reward.Type.MERCENARY_RANDOM_MERCENARY)
		{
			return rewardType == Reward.Type.MERCENARY_RENOWN;
		}
		return true;
	}

	public static void SortRewards(ref List<Reward> rewards)
	{
		if (rewards == null)
		{
			return;
		}
		rewards.Sort(delegate(Reward r1, Reward r2)
		{
			if (r1.RewardType == r2.RewardType)
			{
				if (r1.RewardType != Reward.Type.CARD)
				{
					return 0;
				}
				CardRewardData cardRewardData = r1.Data as CardRewardData;
				CardRewardData cardRewardData2 = r2.Data as CardRewardData;
				EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRewardData.CardID);
				EntityDef entityDef2 = DefLoader.Get().GetEntityDef(cardRewardData2.CardID);
				bool flag = entityDef.IsHeroSkin();
				bool flag2 = entityDef2.IsHeroSkin();
				if (flag == flag2)
				{
					return 0;
				}
				if (!flag)
				{
					return 1;
				}
				return -1;
			}
			if (Reward.Type.CARD_BACK == r1.RewardType)
			{
				return -1;
			}
			if (Reward.Type.CARD_BACK == r2.RewardType)
			{
				return 1;
			}
			if (Reward.Type.CARD == r1.RewardType)
			{
				return -1;
			}
			if (Reward.Type.CARD == r2.RewardType)
			{
				return 1;
			}
			if (Reward.Type.BOOSTER_PACK == r1.RewardType)
			{
				return -1;
			}
			if (Reward.Type.BOOSTER_PACK == r2.RewardType)
			{
				return 1;
			}
			if (Reward.Type.MOUNT == r1.RewardType)
			{
				return -1;
			}
			if (Reward.Type.MOUNT == r2.RewardType)
			{
				return 1;
			}
			if (Reward.Type.MERCENARY_EXP == r1.RewardType)
			{
				return -1;
			}
			if (Reward.Type.MERCENARY_EXP == r2.RewardType)
			{
				return 1;
			}
			if (Reward.Type.MERCENARY_ABILITY_UNLOCK == r1.RewardType)
			{
				return -1;
			}
			return (Reward.Type.MERCENARY_ABILITY_UNLOCK == r2.RewardType) ? 1 : 0;
		});
	}

	public static void AddRewardDataToList(RewardData newRewardData, List<RewardData> existingRewardDataList)
	{
		CardRewardData existingCardRewardData = GetDuplicateCardDataReward(newRewardData, existingRewardDataList);
		if (existingCardRewardData == null)
		{
			existingRewardDataList.Add(newRewardData);
			return;
		}
		CardRewardData newCardRewardData = newRewardData as CardRewardData;
		existingCardRewardData.Merge(newCardRewardData);
	}

	public static bool GetNextHeroLevelRewardText(TAG_CLASS heroClass, int heroLevel, int totalLevel, out string nextRewardTitle, out string nextRewardDescription)
	{
		int nextHeroLevelWithReward;
		RewardData heroLevelRewardData = FixedRewardsMgr.Get().GetNextHeroLevelReward(heroClass, heroLevel, out nextHeroLevelWithReward);
		int nextTotalLevelWithReward;
		RewardData totalLevelRewardData = FixedRewardsMgr.Get().GetNextTotalLevelReward(totalLevel, out nextTotalLevelWithReward);
		nextRewardTitle = string.Empty;
		nextRewardDescription = string.Empty;
		bool hasUpcomingHeroLevelReward = nextHeroLevelWithReward > 0;
		bool hasUpcomingTotalLevelReward = nextTotalLevelWithReward > 0;
		if (!hasUpcomingHeroLevelReward && !hasUpcomingTotalLevelReward)
		{
			return false;
		}
		int nearestLevelWithReward = 0;
		int distanceToNextHeroLevelReward = nextHeroLevelWithReward - heroLevel;
		int distanceToNextTotalLevelReward = nextTotalLevelWithReward - totalLevel;
		if (hasUpcomingHeroLevelReward && (!hasUpcomingTotalLevelReward || distanceToNextHeroLevelReward <= distanceToNextTotalLevelReward))
		{
			nearestLevelWithReward = nextHeroLevelWithReward;
			nextRewardDescription = GetRewardText(heroLevelRewardData);
		}
		if (hasUpcomingHeroLevelReward && hasUpcomingTotalLevelReward && distanceToNextHeroLevelReward == distanceToNextTotalLevelReward)
		{
			nextRewardDescription += "\n";
		}
		if (hasUpcomingTotalLevelReward && (!hasUpcomingHeroLevelReward || distanceToNextTotalLevelReward <= distanceToNextHeroLevelReward))
		{
			nearestLevelWithReward = heroLevel + distanceToNextTotalLevelReward;
			nextRewardDescription += GetRewardText(totalLevelRewardData);
		}
		if (nearestLevelWithReward > 0)
		{
			nextRewardTitle = GameStrings.Format("GLOBAL_HERO_LEVEL_NEXT_REWARD_TITLE", nearestLevelWithReward);
		}
		return nextRewardTitle != string.Empty;
	}

	public static string GetRewardText(RewardData rewardData)
	{
		if (rewardData == null)
		{
			return string.Empty;
		}
		switch (rewardData.RewardType)
		{
		case Reward.Type.CARD:
		{
			CardRewardData cardReward = rewardData as CardRewardData;
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardReward.CardID);
			if (cardReward.Premium == TAG_PREMIUM.GOLDEN)
			{
				return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_GOLDEN_CARD", GameStrings.Get("GLOBAL_COLLECTION_GOLDEN"), entityDef.GetName());
			}
			return entityDef.GetName();
		}
		case Reward.Type.BOOSTER_PACK:
		{
			BoosterPackRewardData boosterReward = rewardData as BoosterPackRewardData;
			string packName = GameDbf.Booster.GetRecord(boosterReward.Id).Name;
			return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_BOOSTER", packName);
		}
		case Reward.Type.ARCANE_DUST:
		{
			ArcaneDustRewardData dustReward = rewardData as ArcaneDustRewardData;
			return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_ARCANE_DUST", dustReward.Amount);
		}
		case Reward.Type.GOLD:
		{
			GoldRewardData goldReward = rewardData as GoldRewardData;
			return GameStrings.Format("GLOBAL_HERO_LEVEL_REWARD_GOLD", goldReward.Amount);
		}
		default:
			return "UNKNOWN";
		}
	}

	public static bool ShowReward(UserAttentionBlocker blocker, Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, AnimationUtil.DelOnShownWithPunch callback, object callbackData)
	{
		return ShowReward_Internal(blocker, reward, updateCacheValues, rewardPunchScale, rewardScale, string.Empty, null, callback, callbackData);
	}

	public static bool ShowReward(UserAttentionBlocker blocker, Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, string callbackName = "", object callbackData = null, GameObject callbackGO = null)
	{
		return ShowReward_Internal(blocker, reward, updateCacheValues, rewardPunchScale, rewardScale, callbackName, callbackGO, null, callbackData);
	}

	public static void SetupRewardIcon(RewardData rewardData, Renderer rewardRenderer, UberText rewardAmountLabel, out float amountToScaleReward, bool doubleGold = false)
	{
		UnityEngine.Vector2 newOffset = UnityEngine.Vector2.zero;
		amountToScaleReward = 1f;
		rewardAmountLabel.gameObject.SetActive(value: false);
		Material rewardMaterial = ((rewardRenderer != null) ? rewardRenderer.GetMaterial() : null);
		AssetHandleCallback<Texture> onTextureLoaded = delegate(AssetReference assetRef, AssetHandle<Texture> texture, object loadTextureCbData)
		{
			if (rewardRenderer != null)
			{
				ServiceManager.Get<DisposablesCleaner>()?.Attach(rewardRenderer, texture);
				if (rewardMaterial != null)
				{
					rewardMaterial.mainTexture = texture;
				}
			}
			else
			{
				texture?.Dispose();
			}
		};
		switch (rewardData.RewardType)
		{
		case Reward.Type.ARCANE_DUST:
		{
			AssetLoader.Get().LoadAsset(s_questRewardsTexturePage2, onTextureLoaded);
			newOffset = new UnityEngine.Vector2(0.25f, 0f);
			ArcaneDustRewardData dustRewardData = rewardData as ArcaneDustRewardData;
			rewardAmountLabel.Text = dustRewardData.Amount.ToString();
			rewardAmountLabel.gameObject.SetActive(value: true);
			break;
		}
		case Reward.Type.BOOSTER_PACK:
		{
			BoosterPackRewardData boosterReward = rewardData as BoosterPackRewardData;
			BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(boosterReward.Id);
			if (!string.IsNullOrEmpty(boosterRecord.QuestIconPath))
			{
				AssetLoader.Get().LoadAsset(boosterRecord.QuestIconPath, onTextureLoaded);
				newOffset = new UnityEngine.Vector2((float)boosterRecord.QuestIconOffsetX, (float)boosterRecord.QuestIconOffsetY);
				break;
			}
			Log.Achievements.PrintWarning("Booster Record ID = {0} does not have proper reward icon data", boosterReward.Id);
			newOffset = new UnityEngine.Vector2(0f, 0.75f);
			if (boosterReward.Id == 11 && boosterReward.Count > 1)
			{
				newOffset = new UnityEngine.Vector2(0f, 0.5f);
			}
			break;
		}
		case Reward.Type.CARD:
		{
			CardRewardData cardReward = rewardData as CardRewardData;
			newOffset = ((!(cardReward.CardID == "HERO_03a")) ? ((!(cardReward.CardID == "HERO_06a")) ? new UnityEngine.Vector2(0.5f, 0f) : new UnityEngine.Vector2(0.75f, 0.25f)) : new UnityEngine.Vector2(0.75f, 0.5f));
			break;
		}
		case Reward.Type.FORGE_TICKET:
			newOffset = new UnityEngine.Vector2(0.75f, 0.75f);
			amountToScaleReward = 1.46881f;
			break;
		case Reward.Type.GOLD:
		{
			newOffset = new UnityEngine.Vector2(0.25f, 0.75f);
			long rewardGoldAmount = ((GoldRewardData)rewardData).Amount;
			if (doubleGold)
			{
				rewardGoldAmount *= 2;
			}
			rewardAmountLabel.Text = rewardGoldAmount.ToString();
			rewardAmountLabel.gameObject.SetActive(value: true);
			break;
		}
		case Reward.Type.ARCANE_ORBS:
			AssetLoader.Get().LoadAsset(s_arcaneOrbIcon, onTextureLoaded);
			rewardAmountLabel.Text = ((SimpleRewardData)rewardData).Amount.ToString();
			rewardAmountLabel.gameObject.SetActive(value: true);
			rewardMaterial.mainTextureScale = new UnityEngine.Vector2(4f, 4f);
			break;
		}
		rewardMaterial.mainTextureOffset = newOffset;
	}

	public static void LoadAndDisplayRewards(List<RewardData> rewards, Action doneCallback = null)
	{
		LoadAndDisplayRewards_LoadNextReward(new RewardDisplayCallbackData
		{
			rewardsToDisplay = rewards,
			rewardIndex = 0,
			doneCallback = doneCallback
		});
	}

	private static void LoadAndDisplayRewards_LoadNextReward(RewardDisplayCallbackData callbackData)
	{
		RewardData rewardData = callbackData.rewardsToDisplay[callbackData.rewardIndex];
		callbackData.rewardIndex++;
		rewardData.LoadRewardObject(LoadAndDisplayRewards_OnRewardObjectLoaded, callbackData);
	}

	private static void LoadAndDisplayRewards_OnRewardObjectLoaded(Reward reward, object callbackData)
	{
		(callbackData as RewardDisplayCallbackData).currentReward = reward;
		PopupDisplayManager.Get().RewardPopups.DisplayRewardObject(reward, LoadAndDisplayRewards_OnRewardShown, callbackData);
		reward.ScreenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
	}

	private static void LoadAndDisplayRewards_OnRewardShown(object callbackData)
	{
		Reward reward = (callbackData as RewardDisplayCallbackData).currentReward;
		if (!(reward == null))
		{
			reward.RegisterClickListener(LoadAndDisplayRewards_OnRewardDismissed, callbackData);
			reward.EnableClickCatcher(enabled: true);
		}
	}

	private static void LoadAndDisplayRewards_OnRewardDismissed(Reward reward, object callbackData)
	{
		reward.RemoveClickListener(LoadAndDisplayRewards_OnRewardDismissed);
		RewardDisplayCallbackData data = callbackData as RewardDisplayCallbackData;
		if (data.rewardIndex >= data.rewardsToDisplay.Count)
		{
			reward.RegisterHideListener(LoadAndDisplayRewards_OnAllRewardsShown, data);
		}
		else
		{
			LoadAndDisplayRewards_LoadNextReward(data);
		}
		reward.Hide(animate: true);
	}

	private static void LoadAndDisplayRewards_OnAllRewardsShown(object callbackData)
	{
		if (!(callbackData is RewardDisplayCallbackData data))
		{
			Log.RewardBox.PrintError("RewardUtils.LoadAndDisplayRewards_OnAllRewardsShown(): callbackData was null or now RewardDisplayCallbackData");
			return;
		}
		if (data.currentReward != null)
		{
			data.currentReward.ScreenEffectsHandle.StopEffect();
		}
		data.currentReward?.RemoveHideListener(LoadAndDisplayRewards_OnAllRewardsShown, data);
		data.doneCallback?.Invoke();
	}

	public static void ShowQuestChestReward(string title, string desc, List<RewardData> rewards, Transform rewardBone, Action doneCallback, bool fromNotice = false, int noticeID = -1, string prefab = "RewardChest_Lock.prefab:06ffa33e82036694e8cacb96aa7b48e8")
	{
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			ChestRewardDisplay componentInChildren = go.GetComponentInChildren<ChestRewardDisplay>();
			componentInChildren.RegisterDoneCallback(doneCallback);
			GameUtils.SetParent(componentInChildren.m_parent.transform, rewardBone);
			if (!componentInChildren.ShowRewards_Quest(rewards, rewardBone, title, desc, fromNotice, noticeID))
			{
				UnityEngine.Object.Destroy(go);
			}
		};
		AssetLoader.Get().InstantiatePrefab(prefab, onAssetLoad);
	}

	public static void ShowMercenariesChestReward(List<RewardData> rewards, List<RewardData> bonusRewards, Transform rewardBone, Action doneCallback, bool autoOpenChest, bool fromNotice = false, int noticeID = -1)
	{
		if (rewards == null || rewards.Count == 0)
		{
			Debug.LogErrorFormat("ShowMercenariesChestReward: No rewards provided.");
			doneCallback?.Invoke();
			return;
		}
		List<RewardData> mainChestRewards = new List<RewardData>();
		List<RewardData> equipmentChestRewards = new List<RewardData>();
		List<RewardData> bonusChestRewards = new List<RewardData>();
		foreach (RewardData reward in rewards)
		{
			if (reward.RewardType == Reward.Type.MERCENARY_EQUIPMENT)
			{
				equipmentChestRewards.Add(reward);
			}
			else
			{
				mainChestRewards.Add(reward);
			}
		}
		if (bonusRewards != null)
		{
			foreach (RewardData reward2 in bonusRewards)
			{
				if (reward2.RewardType == Reward.Type.MERCENARY_EQUIPMENT)
				{
					equipmentChestRewards.Add(reward2);
				}
				else
				{
					bonusChestRewards.Add(reward2);
				}
			}
		}
		Action onMainChestComplete = delegate
		{
			if (equipmentChestRewards.Count == 0)
			{
				doneCallback?.Invoke();
			}
			else
			{
				ShowMercenariesEquipmentReward(equipmentChestRewards, doneCallback, fromNotice, noticeID);
			}
		};
		if (mainChestRewards.Count == 0 && bonusChestRewards.Count == 0 && equipmentChestRewards.Count > 0)
		{
			ShowMercenariesEquipmentReward(equipmentChestRewards, doneCallback, fromNotice, noticeID);
			return;
		}
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			ChestRewardDisplay componentInChildren = go.GetComponentInChildren<ChestRewardDisplay>();
			componentInChildren.RegisterDoneCallback(onMainChestComplete);
			if (rewardBone != null)
			{
				GameUtils.SetParent(componentInChildren.m_parent.transform, rewardBone);
			}
			if (!componentInChildren.ShowRewards_Mercenaries(mainChestRewards, bonusChestRewards, autoOpenChest, fromNotice, noticeID))
			{
				UnityEngine.Object.Destroy(go);
			}
		};
		AssetLoader.Get().InstantiatePrefab("RewardChest_Mercenaries.prefab:7ba36254f98c8914e9b9931bbede3c88", onAssetLoad);
	}

	public static void ShowMercenariesEquipmentReward(List<RewardData> rewards, Action doneCallback, bool fromNotice = false, int noticeID = -1)
	{
		Action onRewardsComplete = delegate
		{
			if (fromNotice)
			{
				Network.Get().AckNotice(noticeID);
			}
			doneCallback?.Invoke();
		};
		LoadAndDisplayRewards(rewards, onRewardsComplete);
	}

	public static void ShowConsolationMercenariesReward(ProfileNoticeMercenariesRewards.RewardType rewardType, RewardListDataModel rewards, Transform rewardBone, Action doneCallback)
	{
		ShowMercenariesReward("LettuceConsolationPrize.prefab:8c837b1ecf3fe184eadfca1a3d661f6f", rewards, rewardBone, doneCallback);
	}

	public static void ShowAutoRetireMercenariesReward(ProfileNoticeMercenariesRewards.RewardType rewardType, RewardListDataModel rewards, Transform rewardBone, Action doneCallback)
	{
		ShowMercenariesReward("LettuceAutorunPrize.prefab:05f50ccdbe9c5994e9dd5b2d19860822", rewards, rewardBone, doneCallback);
	}

	private static void ShowMercenariesReward(string prefab, RewardListDataModel rewards, Transform rewardBone, Action doneCallback)
	{
		if (rewards == null || rewards.Items.Count == 0)
		{
			Debug.LogErrorFormat("ShowConsolationMercenariesReward: No rewards provided.");
			doneCallback?.Invoke();
			return;
		}
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			MercenariesConsolationReward componentInChildren = go.GetComponentInChildren<MercenariesConsolationReward>();
			if (rewardBone != null)
			{
				GameUtils.SetParent(go.transform, rewardBone);
			}
			componentInChildren.RegisterDoneCallback(doneCallback);
			componentInChildren.RegisterDoneCallback(delegate
			{
				UnityEngine.Object.Destroy(go, 1f);
			});
			componentInChildren.VisualController.BindDataModel(rewards);
		};
		AssetLoader.Get().InstantiatePrefab(prefab, onAssetLoad);
	}

	public static void ShowMercenaryFullyUpgraded(LettuceMercenaryDataModel mercenary, Transform rewardBone, Action doneCallback)
	{
		if (mercenary == null)
		{
			Debug.LogErrorFormat("ShowMercenaryFullyUpgraded: No mercenary provided.");
			doneCallback?.Invoke();
			return;
		}
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			MercenaryFullyUpgraded componentInChildren = go.GetComponentInChildren<MercenaryFullyUpgraded>();
			if (rewardBone != null)
			{
				GameUtils.SetParent(go.transform, rewardBone);
			}
			componentInChildren.RegisterDoneCallback(doneCallback);
			componentInChildren.RegisterDoneCallback(delegate
			{
				UnityEngine.Object.Destroy(go, 1f);
			});
			componentInChildren.VisualController.BindDataModel(mercenary);
			componentInChildren.VisualController.SetState("SHOW");
		};
		AssetLoader.Get().InstantiatePrefab("MercenariesMaxedOutReward.prefab:57fbf1dc798a43547b597a5d63e18271", onAssetLoad);
	}

	public static void ShowTavernBrawlRewards(int wins, List<RewardData> rewards, Transform rewardBone, Action doneCallback, bool fromNotice = false, NetCache.ProfileNoticeTavernBrawlRewards notice = null)
	{
		TavernBrawlMode num = (fromNotice ? notice.Mode : TavernBrawlManager.Get().CurrentSeasonBrawlMode);
		long noticeId = notice?.NoticeID ?? 0;
		if (num == TavernBrawlMode.TB_MODE_NORMAL)
		{
			ShowSessionTavernBrawlRewards(wins, rewards, rewardBone, doneCallback, fromNotice, noticeId);
		}
		else
		{
			ShowHeroicSessionTavernBrawlRewards(wins, rewards, rewardBone, doneCallback, fromNotice, noticeId);
		}
	}

	public static void ShowSessionTavernBrawlRewards(int wins, List<RewardData> rewards, Transform rewardBone, Action doneCallback, bool fromNotice = false, long noticeID = -1L)
	{
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			ChestRewardDisplay componentInChildren = go.GetComponentInChildren<ChestRewardDisplay>();
			componentInChildren.RegisterDoneCallback(doneCallback);
			GameUtils.SetParent(componentInChildren.m_parent.transform, rewardBone);
			if (!componentInChildren.ShowRewards_TavernBrawl(wins, rewards, rewardBone, fromNotice, noticeID))
			{
				UnityEngine.Object.Destroy(go);
			}
		};
		AssetLoader.Get().InstantiatePrefab("RewardChest_Lock.prefab:06ffa33e82036694e8cacb96aa7b48e8", onAssetLoad);
	}

	public static void ShowLeaguePromotionRewards(int leagueId, List<RewardData> rewards, Transform rewardBone, Action doneCallback, bool fromNotice = false, long noticeID = -1L)
	{
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			ChestRewardDisplay componentInChildren = go.GetComponentInChildren<ChestRewardDisplay>();
			componentInChildren.RegisterDoneCallback(doneCallback);
			GameUtils.SetParent(componentInChildren.m_parent.transform, rewardBone);
			if (!componentInChildren.ShowRewards_LeaguePromotion(leagueId, rewards, rewardBone, fromNotice, noticeID))
			{
				UnityEngine.Object.Destroy(go);
			}
		};
		AssetLoader.Get().InstantiatePrefab("RewardChest_Lock.prefab:06ffa33e82036694e8cacb96aa7b48e8", onAssetLoad);
	}

	public static void ShowHeroicSessionTavernBrawlRewards(int wins, List<RewardData> rewards, Transform rewardBone, Action doneCallback, bool fromNotice = false, long noticeID = -1L)
	{
		PrefabCallback<GameObject> onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			HeroicBrawlRewardDisplay component = go.GetComponent<HeroicBrawlRewardDisplay>();
			component.RegisterDoneCallback(doneCallback);
			TransformUtil.AttachAndPreserveLocalTransform(component.transform, rewardBone);
			component.ShowRewards(wins, rewards, fromNotice, noticeID);
		};
		AssetLoader.Get().InstantiatePrefab("HeroicBrawlReward.prefab:8f49f1fcb5ca4485d9b6b22993e1b1ab", onAssetLoad);
	}

	public static RewardChest GenerateTavernBrawlRewardChest_CHEAT(int wins, TavernBrawlMode mode)
	{
		RewardChest chest = new RewardChest();
		PegasusShared.RewardBag boosterBag = new PegasusShared.RewardBag();
		boosterBag.RewardBooster = new ProfileNoticeRewardBooster();
		boosterBag.RewardBooster.BoosterType = 1;
		int boosterPacks = 0;
		int resource = 0;
		switch (wins)
		{
		case 0:
			boosterPacks = 1;
			break;
		case 1:
			boosterPacks = 2;
			break;
		case 2:
			boosterPacks = 4;
			break;
		case 3:
			boosterPacks = 4;
			resource = 120;
			break;
		case 4:
			boosterPacks = 5;
			resource = 230;
			break;
		case 5:
			boosterPacks = 6;
			resource = 260;
			break;
		case 6:
			boosterPacks = 7;
			resource = 290;
			break;
		case 7:
			boosterPacks = 8;
			resource = 320;
			break;
		case 8:
			boosterPacks = 9;
			resource = 350;
			break;
		case 9:
			boosterPacks = 14;
			resource = 500;
			break;
		case 10:
			boosterPacks = 15;
			resource = 550;
			break;
		case 11:
			boosterPacks = 20;
			resource = 600;
			break;
		case 12:
			boosterPacks = 50;
			resource = 1000;
			break;
		}
		boosterBag.RewardBooster.BoosterCount = boosterPacks;
		chest.Bag.Add(boosterBag);
		if (wins > 2)
		{
			PegasusShared.RewardBag dustBag = new PegasusShared.RewardBag();
			dustBag.RewardDust = new ProfileNoticeRewardDust();
			dustBag.RewardDust.Amount = resource + UnityEngine.Random.Range(-4, 4) * 5;
			PegasusShared.RewardBag goldBag = new PegasusShared.RewardBag();
			goldBag.RewardGold = new ProfileNoticeRewardCurrency();
			goldBag.RewardGold.Amount = resource + UnityEngine.Random.Range(-4, 4) * 5;
			chest.Bag.Add(goldBag);
			chest.Bag.Add(dustBag);
		}
		if (wins > 9)
		{
			PegasusShared.RewardBag cardBag1 = new PegasusShared.RewardBag();
			cardBag1.RewardCard = new ProfileNoticeRewardCard();
			cardBag1.RewardCard.Card = new PegasusShared.CardDef();
			cardBag1.RewardCard.Card.Premium = 1;
			cardBag1.RewardCard.Card.Asset = 834;
			chest.Bag.Add(cardBag1);
		}
		if (wins > 10)
		{
			PegasusShared.RewardBag cardBag2 = new PegasusShared.RewardBag();
			cardBag2.RewardCard = new ProfileNoticeRewardCard();
			cardBag2.RewardCard.Card = new PegasusShared.CardDef();
			cardBag2.RewardCard.Card.Premium = 1;
			cardBag2.RewardCard.Card.Asset = 374;
			chest.Bag.Add(cardBag2);
		}
		if (wins > 11 && mode == TavernBrawlMode.TB_MODE_HEROIC)
		{
			PegasusShared.RewardBag cardBag3 = new PegasusShared.RewardBag();
			cardBag3.RewardCard = new ProfileNoticeRewardCard();
			cardBag3.RewardCard.Card = new PegasusShared.CardDef();
			cardBag3.RewardCard.Card.Premium = 1;
			cardBag3.RewardCard.Card.Asset = 640;
			chest.Bag.Add(cardBag3);
		}
		return chest;
	}

	public static RewardChest GenerateMercenariesMapRewardChest_CHEAT()
	{
		RewardChest rewardChest = new RewardChest();
		PegasusShared.RewardBag coinBag = new PegasusShared.RewardBag();
		coinBag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		coinBag.RewardMercenariesCurrency.CurrencyDelta = 98L;
		coinBag.RewardMercenariesCurrency.MercenaryId = 1;
		rewardChest.Bag.Add(coinBag);
		PegasusShared.RewardBag coinBag2 = new PegasusShared.RewardBag();
		coinBag2.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		coinBag2.RewardMercenariesCurrency.CurrencyDelta = 87L;
		coinBag2.RewardMercenariesCurrency.MercenaryId = 100;
		rewardChest.Bag.Add(coinBag2);
		PegasusShared.RewardBag expBag = new PegasusShared.RewardBag();
		expBag.RewardMercenariesExperience = new ProfileNoticeRewardMercenariesExperience();
		expBag.RewardMercenariesExperience.ExpDelta = 76L;
		expBag.RewardMercenariesExperience.PreExp = 1000L;
		expBag.RewardMercenariesExperience.PostExp = 1076L;
		expBag.RewardMercenariesExperience.MercenaryId = 6;
		rewardChest.Bag.Add(expBag);
		PegasusShared.RewardBag expBag2 = new PegasusShared.RewardBag();
		expBag2.RewardMercenariesExperience = new ProfileNoticeRewardMercenariesExperience();
		expBag2.RewardMercenariesExperience.ExpDelta = 300L;
		expBag2.RewardMercenariesExperience.PreExp = 0L;
		expBag2.RewardMercenariesExperience.PostExp = 300L;
		expBag2.RewardMercenariesExperience.MercenaryId = 7;
		rewardChest.Bag.Add(expBag2);
		PegasusShared.RewardBag expBag3 = new PegasusShared.RewardBag();
		expBag3.RewardMercenariesExperience = new ProfileNoticeRewardMercenariesExperience();
		expBag3.RewardMercenariesExperience.ExpDelta = 10001L;
		expBag3.RewardMercenariesExperience.PreExp = 0L;
		expBag3.RewardMercenariesExperience.PostExp = 10001L;
		expBag3.RewardMercenariesExperience.MercenaryId = 8;
		rewardChest.Bag.Add(expBag3);
		PegasusShared.RewardBag equipmentBag = new PegasusShared.RewardBag();
		equipmentBag.RewardMercenariesEquipment = new ProfileNoticeRewardMercenariesEquipment();
		equipmentBag.RewardMercenariesEquipment.EquipmentId = 158;
		equipmentBag.RewardMercenariesEquipment.EquipmentTier = 4u;
		equipmentBag.RewardMercenariesEquipment.MercenaryId = 100;
		rewardChest.Bag.Add(equipmentBag);
		return rewardChest;
	}

	public static RewardChest GenerateMercenariesConsolationReward_CHEAT()
	{
		RewardChest rewardChest = new RewardChest();
		PegasusShared.RewardBag coinBag = new PegasusShared.RewardBag();
		coinBag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		coinBag.RewardMercenariesCurrency.CurrencyDelta = 98L;
		coinBag.RewardMercenariesCurrency.MercenaryId = 100;
		rewardChest.Bag.Add(coinBag);
		PegasusShared.RewardBag expBag = new PegasusShared.RewardBag();
		expBag.RewardMercenariesExperience = new ProfileNoticeRewardMercenariesExperience();
		expBag.RewardMercenariesExperience.ExpDelta = 13370L;
		expBag.RewardMercenariesExperience.PreExp = 0L;
		expBag.RewardMercenariesExperience.PostExp = 13370L;
		expBag.RewardMercenariesExperience.MercenaryId = 1;
		rewardChest.Bag.Add(expBag);
		PegasusShared.RewardBag expBag2 = new PegasusShared.RewardBag();
		expBag2.RewardMercenariesExperience = new ProfileNoticeRewardMercenariesExperience();
		expBag2.RewardMercenariesExperience.ExpDelta = 76L;
		expBag2.RewardMercenariesExperience.PreExp = 1000L;
		expBag2.RewardMercenariesExperience.PostExp = 1076L;
		expBag2.RewardMercenariesExperience.MercenaryId = 6;
		rewardChest.Bag.Add(expBag2);
		return rewardChest;
	}

	public static RewardChest GenerateMercenariesSeasonReward_CHEAT()
	{
		RewardChest rewardChest = new RewardChest();
		PegasusShared.RewardBag bag = new PegasusShared.RewardBag();
		bag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		bag.RewardMercenariesCurrency.CurrencyDelta = 98L;
		bag.RewardMercenariesCurrency.MercenaryId = 1;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		bag.RewardMercenariesCurrency.CurrencyDelta = 87L;
		bag.RewardMercenariesCurrency.MercenaryId = 100;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		bag.RewardMercenariesCurrency.CurrencyDelta = 12L;
		bag.RewardMercenariesCurrency.MercenaryId = 7;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardBooster = new ProfileNoticeRewardBooster();
		bag.RewardBooster.BoosterType = 629;
		bag.RewardBooster.BoosterCount = 3;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardRandomMercenary = new ProfileNoticeMercenariesRandomRewardLicense();
		bag.RewardRandomMercenary.MercenaryId = 2;
		bag.RewardRandomMercenary.ArtVariationId = 70;
		bag.RewardRandomMercenary.ArtVariationPremium = 2u;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		bag.RewardMercenariesCurrency.CurrencyDelta = 50L;
		bag.RewardMercenariesCurrency.MercenaryId = 102;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardMercenariesCurrency = new ProfileNoticeRewardMercenariesCurrency();
		bag.RewardMercenariesCurrency.CurrencyDelta = 42L;
		bag.RewardMercenariesCurrency.MercenaryId = 7;
		rewardChest.Bag.Add(bag);
		bag = new PegasusShared.RewardBag();
		bag.RewardRandomMercenary = new ProfileNoticeMercenariesRandomRewardLicense();
		bag.RewardRandomMercenary.MercenaryId = 2;
		bag.RewardRandomMercenary.ArtVariationId = 70;
		bag.RewardRandomMercenary.ArtVariationPremium = 1u;
		bag.RewardRandomMercenary.CurrencyAmount = 123L;
		rewardChest.Bag.Add(bag);
		return rewardChest;
	}

	public static void SetQuestTileNameLinePosition(GameObject nameLine, UberText questName, float padding)
	{
		bool num = questName.isHidden();
		if (num)
		{
			questName.Show();
		}
		TransformUtil.SetPoint(nameLine, Anchor.TOP, questName, Anchor.BOTTOM);
		nameLine.transform.localPosition = new Vector3(nameLine.transform.localPosition.x, nameLine.transform.localPosition.y, nameLine.transform.localPosition.z + padding);
		if (num)
		{
			questName.Hide();
		}
	}

	public static RewardChestContentsDbfRecord GetRewardChestContents(int rewardChestAssetId, int rewardLevel)
	{
		if (GameDbf.RewardChest.HasRecord(rewardChestAssetId))
		{
			return GameDbf.RewardChestContents.GetRecord((RewardChestContentsDbfRecord r) => r.RewardChestId == rewardChestAssetId && r.RewardLevel == rewardLevel);
		}
		return null;
	}

	public static List<RewardData> GetRewardDataFromRewardChestAsset(int rewardChestAssetId, int rewardLevel)
	{
		List<RewardData> rewardData = new List<RewardData>();
		RewardChestContentsDbfRecord contents = GetRewardChestContents(rewardChestAssetId, rewardLevel);
		if (contents != null)
		{
			int unusedSeasonId = 0;
			AddRewardDataStubForBag(contents.Bag1, unusedSeasonId, ref rewardData);
			AddRewardDataStubForBag(contents.Bag2, unusedSeasonId, ref rewardData);
			AddRewardDataStubForBag(contents.Bag3, unusedSeasonId, ref rewardData);
			AddRewardDataStubForBag(contents.Bag4, unusedSeasonId, ref rewardData);
			AddRewardDataStubForBag(contents.Bag5, unusedSeasonId, ref rewardData);
		}
		return rewardData;
	}

	public static void AddRewardDataStubForBag(int bagId, int seasonId, ref List<RewardData> rewardData)
	{
		RewardBagDbfRecord record = null;
		List<RewardBagDbfRecord> records = GameDbf.RewardBag.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			if (records[i].BagId == bagId)
			{
				record = records[i];
				break;
			}
		}
		if (record == null)
		{
			return;
		}
		switch (record.Reward)
		{
		case Assets.RewardBag.Reward.RANDOM_CARD:
			rewardData.Add(new RandomCardRewardData(GetRarityForRandomCardReward(record.RewardData), TAG_PREMIUM.NORMAL));
			break;
		case Assets.RewardBag.Reward.COM:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.COMMON, TAG_PREMIUM.NORMAL, record.Base));
			break;
		case Assets.RewardBag.Reward.GCOM:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.COMMON, TAG_PREMIUM.GOLDEN, record.Base));
			break;
		case Assets.RewardBag.Reward.RARE:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.RARE, TAG_PREMIUM.NORMAL, record.Base));
			break;
		case Assets.RewardBag.Reward.GRARE:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.RARE, TAG_PREMIUM.GOLDEN, record.Base));
			break;
		case Assets.RewardBag.Reward.EPIC:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.EPIC, TAG_PREMIUM.NORMAL, record.Base));
			break;
		case Assets.RewardBag.Reward.GEPIC:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.EPIC, TAG_PREMIUM.GOLDEN, record.Base));
			break;
		case Assets.RewardBag.Reward.LEG:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.LEGENDARY, TAG_PREMIUM.NORMAL, record.Base));
			break;
		case Assets.RewardBag.Reward.GLEG:
			rewardData.Add(new RandomCardRewardData(TAG_RARITY.LEGENDARY, TAG_PREMIUM.GOLDEN, record.Base));
			break;
		case Assets.RewardBag.Reward.RANKED_SEASON_REWARD_PACK:
		{
			int boosterDbId = RankMgr.Get().GetRankedRewardBoosterIdForSeasonId(seasonId);
			rewardData.Add(new BoosterPackRewardData(boosterDbId, record.Base));
			break;
		}
		case Assets.RewardBag.Reward.LATEST_PACK:
			rewardData.Add(new BoosterPackRewardData(record.RewardData, record.Base, record.BagId));
			break;
		case Assets.RewardBag.Reward.SPECIFIC_PACK:
			rewardData.Add(new BoosterPackRewardData(record.RewardData, record.Base));
			break;
		case Assets.RewardBag.Reward.GOLD:
			rewardData.Add(new GoldRewardData(record.Base));
			break;
		case Assets.RewardBag.Reward.DUST:
			rewardData.Add(new ArcaneDustRewardData(record.Base));
			break;
		case Assets.RewardBag.Reward.BATTLEGROUNDS_TOKEN:
			rewardData.Add(new BattlegroundsTokenRewardData(record.Base));
			break;
		case Assets.RewardBag.Reward.REWARD_CHEST_CONTENTS:
		{
			RewardChestContentsDbfRecord rewardChestContents = GameDbf.RewardChestContents.GetRecord(record.RewardData);
			if (rewardChestContents == null)
			{
				Log.All.PrintWarning("No reward chest contents of id {0} found on client for random card reward", record.RewardData);
			}
			else
			{
				ProcessRewardChestContents(rewardChestContents, seasonId, ref rewardData);
			}
			break;
		}
		case Assets.RewardBag.Reward.PACK2_DEPRECATED:
		case Assets.RewardBag.Reward.SEASONAL_CARD_BACK:
		case Assets.RewardBag.Reward.NOT_LATEST_PACK:
		case Assets.RewardBag.Reward.GOLDEN_RANDOM_CARD:
		case Assets.RewardBag.Reward.FORGE:
		case Assets.RewardBag.Reward.PACK_OFFSET_FROM_LATEST:
		case Assets.RewardBag.Reward.SPECIFIC_CARD:
		case Assets.RewardBag.Reward.SPECIFIC_CARD_2X:
		case (Assets.RewardBag.Reward)25:
		case (Assets.RewardBag.Reward)26:
		case (Assets.RewardBag.Reward)27:
		case (Assets.RewardBag.Reward)28:
		case (Assets.RewardBag.Reward)29:
		case (Assets.RewardBag.Reward)30:
		case Assets.RewardBag.Reward.RANDOM_PRERELEASE_CARD:
			break;
		}
	}

	public static void ProcessRewardChestContents(RewardChestContentsDbfRecord rewardChestContents, int seasonId, ref List<RewardData> rewardData)
	{
		foreach (int bag in new List<int> { rewardChestContents.Bag1, rewardChestContents.Bag2, rewardChestContents.Bag3, rewardChestContents.Bag4, rewardChestContents.Bag5 })
		{
			if (bag != 0)
			{
				RewardBagDbfRecord record = GameDbf.RewardBag.GetRecord((RewardBagDbfRecord r) => r.BagId == bag);
				if (record != null && !(record.Reward.ToString().ToLower() == "reward_chest_contents"))
				{
					AddRewardDataStubForBag(bag, seasonId, ref rewardData);
				}
			}
		}
	}

	public static void ShowRewardBoxes(List<RewardData> rewards, Action doneCallback, Transform bone = null, bool useLocalPosition = false, GameLayer layer = GameLayer.IgnoreFullScreenEffects, bool useDarkeningClickCatcher = false)
	{
		GameObjectCallback onAssetLoad = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (SoundManager.Get() != null)
			{
				SoundManager.Get().LoadAndPlay("card_turn_over_legendary.prefab:a8140f686bff601459e954bc23de35e0");
			}
			RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
			component.SetRewards(rewards);
			component.m_playBoxFlyoutSound = false;
			component.SetLayer(layer);
			component.UseDarkeningClickCatcher(useDarkeningClickCatcher);
			component.RegisterDoneCallback(doneCallback);
			if (bone != null)
			{
				if (useLocalPosition)
				{
					component.transform.localPosition = bone.localPosition;
				}
				else
				{
					component.transform.position = bone.position;
				}
				component.transform.localRotation = bone.localRotation;
				component.transform.localScale = bone.localScale;
			}
			component.AnimateRewards();
		};
		AssetLoader.Get().LoadGameObject(RewardBoxesDisplay.GetPrefab(rewards), onAssetLoad);
	}

	public static TAG_RARITY GetRarityForRandomCardReward(int boosterCardSetId)
	{
		BoosterCardSetDbfRecord boosterCardSet = GameDbf.BoosterCardSet.GetRecord(boosterCardSetId);
		if (boosterCardSet == null)
		{
			Log.All.PrintWarning("No BoosterCardSet of id [{0}] found)", boosterCardSetId);
			return TAG_RARITY.INVALID;
		}
		SubsetDbfRecord subset = boosterCardSet.SubsetRecord;
		if (subset == null)
		{
			Log.All.PrintWarning("No subset of id {0} found on client for random card reward on boosterCardSet {1}", boosterCardSet.SubsetId, boosterCardSet.ID);
			return TAG_RARITY.INVALID;
		}
		IEnumerable<SubsetRuleDbfRecord> source = subset.Rules.Where((SubsetRuleDbfRecord r) => r.Tag == 203);
		SubsetRuleDbfRecord rule = source.FirstOrDefault();
		if (source.Count() != 1 || rule == null || rule.RuleIsNot || rule.MinValue != rule.MaxValue)
		{
			Log.All.PrintWarning("Random card display requires exactly one rarity rule to specify a single rarity (subset id [{0}])", subset.ID);
			return TAG_RARITY.INVALID;
		}
		return (TAG_RARITY)rule.MinValue;
	}

	public static UserAttentionBlocker GetUserAttentionBlockerForReward(NetCache.ProfileNotice.NoticeOrigin origin, long originData)
	{
		if (origin != NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT && origin != NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE)
		{
			return UserAttentionBlocker.NONE;
		}
		return (UserAttentionBlocker)(GameDbf.Achieve.GetRecord((int)originData)?.AttentionBlocker ?? Assets.Achieve.AttentionBlocker.NONE);
	}

	public static bool IsMercenaryRewardPortrait(LettuceMercenaryDataModel rewardData)
	{
		MercenaryArtVariationDbfRecord artVariation = GameDbf.LettuceMercenary.GetRecord(rewardData.MercenaryId).MercenaryArtVariations.First((MercenaryArtVariationDbfRecord e) => e.CardRecord.NoteMiniGuid == rewardData.Card.CardId);
		if (rewardData.Card.Premium <= TAG_PREMIUM.NORMAL)
		{
			return !artVariation.DefaultVariation;
		}
		return true;
	}

	public static bool IsRequiredDataLoadedToShowReward(Reward reward)
	{
		Reward.Type rewardType = reward.RewardType;
		if ((uint)(rewardType - 18) <= 2u && !CollectionManager.Get().IsLettuceLoaded())
		{
			return false;
		}
		return true;
	}

	public static bool IsRequiredContextForReward(Reward reward)
	{
		Reward.Type rewardType = reward.RewardType;
		if ((uint)(rewardType - 18) <= 1u && !SceneMgr.Get().IsInLettuceMode())
		{
			return false;
		}
		return true;
	}

	public static void GetTitleAndDescriptionFromReward(Reward reward, out string title, out string description)
	{
		RewardData rewardData = reward.Data;
		title = rewardData.NameOverride;
		description = rewardData.DescriptionOverride;
		bool titleIsNull = string.IsNullOrEmpty(title);
		bool descIsNull = string.IsNullOrEmpty(description);
		if (!(titleIsNull || descIsNull))
		{
			return;
		}
		switch (rewardData.RewardType)
		{
		case Reward.Type.MINI_SET:
		{
			MiniSetRewardData miniSetReward = reward.Data as MiniSetRewardData;
			MiniSetDbfRecord record = GameDbf.MiniSet.GetRecord(miniSetReward.MiniSetID);
			int cardCount = record.DeckRecord.Cards.Count;
			if (miniSetReward.Premium == 1)
			{
				title = GameStrings.FormatLocalizedString(record.GoldenName);
			}
			if (string.IsNullOrEmpty(title))
			{
				title = GameStrings.FormatLocalizedString(record.DeckRecord.Name);
			}
			description = GameStrings.FormatLocalizedString(record.DeckRecord.Description, cardCount);
			break;
		}
		case Reward.Type.BATTLEGROUNDS_FINISHER:
		{
			BattlegroundsFinisherDataModel datamodel2 = (rewardData as BattlegroundsFinisherRewardData)?.DataModel;
			if (datamodel2 != null)
			{
				if (titleIsNull)
				{
					title = datamodel2.DetailsDisplayName;
				}
				if (descIsNull)
				{
					description = datamodel2.Description;
				}
			}
			break;
		}
		case Reward.Type.BATTLEGROUNDS_BOARD_SKIN:
		{
			BattlegroundsBoardSkinDataModel datamodel = (rewardData as BattlegroundsBoardSkinRewardData)?.DataModel;
			if (datamodel != null)
			{
				if (titleIsNull)
				{
					title = datamodel.DetailsDisplayName;
				}
				if (descIsNull)
				{
					description = datamodel.Description;
				}
			}
			break;
		}
		case Reward.Type.BATTLEGROUNDS_EMOTE:
		{
			BattlegroundsEmoteDataModel datamodel3 = (rewardData as BattlegroundsEmoteRewardData)?.DataModel;
			if (datamodel3 != null)
			{
				if (titleIsNull)
				{
					title = datamodel3.DisplayName;
				}
				if (descIsNull)
				{
					description = datamodel3.Description;
				}
			}
			break;
		}
		case Reward.Type.CARD:
		{
			CardRewardData card = rewardData as CardRewardData;
			EntityDef entityDef = DefLoader.Get().GetEntityDef(card.CardID);
			ProductClientDataDbfRecord productClientData = GameDbf.ProductClientData.GetRecord((ProductClientDataDbfRecord r) => r.PmtProductId == card.OriginData);
			if (productClientData != null)
			{
				title = GameStrings.FormatLocalizedString(productClientData.PopupTitle);
				description = GameStrings.FormatLocalizedString(productClientData.PopupBody, entityDef.GetName());
			}
			else
			{
				RewardBanner banner = reward.m_rewardBanner;
				title = banner.HeadlineText;
				description = banner.DetailsText;
			}
			break;
		}
		}
	}

	private static bool ShowReward_Internal(UserAttentionBlocker blocker, Reward reward, bool updateCacheValues, Vector3 rewardPunchScale, Vector3 rewardScale, string gameObjectCallbackName, GameObject callbackGO, AnimationUtil.DelOnShownWithPunch onShowPunchCallback, object callbackData)
	{
		if (reward == null)
		{
			return false;
		}
		if (!UserAttentionManager.CanShowAttentionGrabber(blocker, "RewardUtils.ShowReward:" + ((reward == null || reward.Data == null) ? "null" : (reward.Data.Origin.ToString() + ":" + reward.Data.OriginData + ":" + reward.Data.RewardType))))
		{
			return false;
		}
		Log.Achievements.Print("RewardUtils: Showing Reward: reward={0} reward.Data={1}", reward, reward.Data);
		AnimationUtil.ShowWithPunch(reward.gameObject, RewardHiddenScale, rewardPunchScale, rewardScale, gameObjectCallbackName, noFade: true, callbackGO, callbackData, onShowPunchCallback);
		reward.Show(updateCacheValues);
		ShowInnkeeperQuoteForReward(reward);
		return true;
	}

	private static CardRewardData GetDuplicateCardDataReward(RewardData newRewardData, List<RewardData> existingRewardData)
	{
		if (!(newRewardData is CardRewardData))
		{
			return null;
		}
		CardRewardData newCardRewardData = newRewardData as CardRewardData;
		return existingRewardData.Find(delegate(RewardData obj)
		{
			if (!(obj is CardRewardData))
			{
				return false;
			}
			CardRewardData cardRewardData = obj as CardRewardData;
			if (!cardRewardData.CardID.Equals(newCardRewardData.CardID))
			{
				return false;
			}
			if (!cardRewardData.Premium.Equals(newCardRewardData.Premium))
			{
				return false;
			}
			return cardRewardData.Origin.Equals(newCardRewardData.Origin) && cardRewardData.OriginData.Equals(newCardRewardData.OriginData);
		}) as CardRewardData;
	}

	private static void ShowInnkeeperQuoteForReward(Reward reward)
	{
		if (reward == null || Reward.Type.CARD != reward.RewardType)
		{
			return;
		}
		switch ((reward.Data as CardRewardData).InnKeeperLine)
		{
		case CardRewardData.InnKeeperTrigger.CORE_CLASS_SET_COMPLETE:
		{
			Notification firstPart = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_BASIC_DONE1_11"), "VO_INNKEEPER_BASIC_DONE1_11.prefab:9b8f8ab262305c54dbb6c847ac8b1fdb");
			if (!Options.Get().GetBool(Option.HAS_SEEN_ALL_BASIC_CLASS_CARDS_COMPLETE, defaultVal: false))
			{
				Processor.RunCoroutine(NotifyOfExpertPacksNeeded(firstPart));
			}
			break;
		}
		case CardRewardData.InnKeeperTrigger.SECOND_REWARD_EVER:
			if (!Options.Get().GetBool(Option.HAS_BEEN_NUDGED_TO_CM, defaultVal: false))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_NUDGE_CM_X"), "VO_INNKEEPER2_NUDGE_COLLECTION_10.prefab:b20c7d803cf82fb46830cba5d4bda11e");
				Options.Get().SetBool(Option.HAS_BEEN_NUDGED_TO_CM, val: true);
			}
			break;
		}
	}

	private static IEnumerator NotifyOfExpertPacksNeeded(Notification innkeeperQuote)
	{
		while (innkeeperQuote.GetAudio() == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(innkeeperQuote.GetAudio().clip.length);
		NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_BASIC_DONE2_12"), "VO_INNKEEPER_BASIC_DONE2_12.prefab:b20f6a03438c5b440a2963095330589c");
		Options.Get().SetBool(Option.HAS_SEEN_ALL_BASIC_CLASS_CARDS_COMPLETE, val: true);
	}

	public static SimpleRewardData CreateArcaneOrbRewardData(int amount)
	{
		return new SimpleRewardData(Reward.Type.ARCANE_ORBS, amount)
		{
			RewardHeadlineText = GameStrings.Get("GLOBAL_REWARD_ARCANE_ORBS_HEADLINE")
		};
	}

	public static DeckRewardData CreateDeckRewardData(int deckTemplateId, int deckId, int classId, string deckNameOverride)
	{
		return new DeckRewardData(deckTemplateId, deckId, classId, deckNameOverride);
	}

	public static bool TryGetSellableDeck(int deckId, out SellableDeckDbfRecord sellableDeckDbfRecord)
	{
		sellableDeckDbfRecord = null;
		List<SellableDeckDbfRecord> matchingSellableDeckRecords = GameDbf.SellableDeck.GetRecords((SellableDeckDbfRecord r) => r.DeckTemplateRecord != null && r.DeckTemplateRecord.DeckId == deckId);
		if (matchingSellableDeckRecords.Count == 0)
		{
			Log.Store.PrintWarning("[RewardUtils.TryGetSellableDeck] Failed to find DB record for deck reward! (ID {0})", deckId);
			return false;
		}
		if (matchingSellableDeckRecords.Count > 1)
		{
			Log.Store.PrintWarning("[RewardUtils.TryGetSellableDeck] Found multiple rewardable deck records that grant the same deck! (ID {0})", deckId);
			return false;
		}
		if (matchingSellableDeckRecords[0].DeckTemplateRecord?.DeckRecord == null)
		{
			Log.Store.PrintWarning("[RewardUtils.TryGetSellableDeck] The DB record {0} for deck reward does NOT have a deck template with a valid deck record!", matchingSellableDeckRecords[0].ID);
			return false;
		}
		sellableDeckDbfRecord = matchingSellableDeckRecords[0];
		return true;
	}

	public static RewardItemDataModel RewardDataToRewardItemDataModel(RewardData rewardData)
	{
		TAG_RARITY rarity = TAG_RARITY.INVALID;
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		RewardItemDataModel result = null;
		switch (rewardData.RewardType)
		{
		case Reward.Type.ARCANE_DUST:
		{
			ArcaneDustRewardData dustRewardData = rewardData as ArcaneDustRewardData;
			result = new RewardItemDataModel
			{
				ItemType = RewardItemType.DUST,
				Quantity = dustRewardData.Amount
			};
			break;
		}
		case Reward.Type.CARD:
		{
			CardRewardData cardRewardData = rewardData as CardRewardData;
			RewardItemDataModel rewardItemDataModel = new RewardItemDataModel
			{
				ItemType = RewardItemType.CARD,
				ItemId = GameUtils.TranslateCardIdToDbId(cardRewardData.CardID),
				Quantity = cardRewardData.Count
			};
			if (GameDbf.Card.GetRecord(rewardItemDataModel.ItemId) != null)
			{
				premium = cardRewardData.Premium;
				result = rewardItemDataModel;
			}
			break;
		}
		case Reward.Type.CARD_BACK:
		{
			CardBackRewardData cardBackRewardData = rewardData as CardBackRewardData;
			result = new RewardItemDataModel
			{
				ItemType = RewardItemType.CARD_BACK,
				ItemId = cardBackRewardData.CardBackID
			};
			break;
		}
		case Reward.Type.RANDOM_CARD:
		{
			RandomCardRewardData randomCardRewardData = rewardData as RandomCardRewardData;
			result = new RewardItemDataModel
			{
				ItemType = RewardItemType.RANDOM_CARD,
				Quantity = randomCardRewardData.Count
			};
			rarity = randomCardRewardData.Rarity;
			premium = randomCardRewardData.Premium;
			break;
		}
		case Reward.Type.ARCANE_ORBS:
		{
			SimpleRewardData simpleRewardData = rewardData as SimpleRewardData;
			result = new RewardItemDataModel
			{
				ItemType = RewardItemType.CN_ARCANE_ORBS,
				Quantity = simpleRewardData.Amount
			};
			break;
		}
		case Reward.Type.BOOSTER_PACK:
		{
			BoosterPackRewardData boosterPackRewardData = rewardData as BoosterPackRewardData;
			result = new RewardItemDataModel
			{
				ItemType = RewardItemType.BOOSTER,
				ItemId = boosterPackRewardData.Id,
				Quantity = boosterPackRewardData.Count
			};
			break;
		}
		default:
			Log.All.PrintWarning("RewardDataToRewardItemDataModel() - RewardData of type {0} is not currently supported!", rewardData.RewardType);
			break;
		}
		if (result != null)
		{
			_ = $"RewardData Error [Type = {rewardData.RewardType}]: ";
			if (!InitializeRewardItemDataModel(result, rarity, premium, out var failReason))
			{
				Log.All.PrintWarning(string.Format("RewardData Error [Type = {0}]: {1}", rewardData.RewardType, failReason ?? "Unspecified reason"));
				result = null;
			}
		}
		return result;
	}

	public static bool IsAdditionalRewardType(RewardItem.RewardType rewardType)
	{
		if (rewardType != RewardItem.RewardType.HERO_CLASS && rewardType != RewardItem.RewardType.GAME_MODE)
		{
			return rewardType == RewardItem.RewardType.LOANER_DECKS;
		}
		return true;
	}

	public static RewardListDataModel CreateRewardListDataModelFromRewardListId(int rewardListId, int chooseOneRewardItemId = 0, List<RewardItemOutput> rewardItemOutputs = null, bool includeAdditionalRewards = false)
	{
		return CreateRewardListDataModelFromRewardListRecord(GameDbf.RewardList.GetRecord(rewardListId), chooseOneRewardItemId, rewardItemOutputs, includeAdditionalRewards);
	}

	public static RewardListDataModel CreateRewardListDataModelFromRewardListRecord(RewardListDbfRecord rewardListRecord, int chooseOneRewardItemId = 0, List<RewardItemOutput> rewardItemOutputs = null, bool includeAdditionalRewards = false)
	{
		if (rewardListRecord == null)
		{
			return null;
		}
		return new RewardListDataModel
		{
			ChooseOne = rewardListRecord.ChooseOne,
			Items = (from r in GameUtils.GetRewardItemsForRewardList(rewardListRecord.ID)
				where (chooseOneRewardItemId <= 0 || r.ID == chooseOneRewardItemId) && IsAdditionalRewardType(r.RewardType) == includeAdditionalRewards && (r.RewardType != RewardItem.RewardType.DECK || r.ID != chooseOneRewardItemId)
				select r).SelectMany((RewardItemDbfRecord r) => RewardFactory.CreateRewardItemDataModel(r, rewardItemOutputs?.Find((RewardItemOutput rio) => rio.RewardItemId == r.ID)?.OutputData)).OrderBy((RewardItemDataModel item) => item, new RewardItemComparer()).ToDataModelList(),
			Description = rewardListRecord.Description
		};
	}

	public static bool InitializeRewardItemDataModelForShop(RewardItemDataModel item, Network.BundleItem netBundleItem, ProductInfo netBundle)
	{
		TAG_RARITY rarity = TAG_RARITY.INVALID;
		TAG_PREMIUM premiumType = TAG_PREMIUM.NORMAL;
		switch (item.ItemType)
		{
		case RewardItemType.RANDOM_CARD:
			premiumType = GetPremiumTypeFromNetBundleAttributes(netBundleItem);
			rarity = GetRarityForRandomCardReward(item.ItemId);
			break;
		case RewardItemType.CARD:
			premiumType = GetPremiumTypeFromNetBundleAttributes(netBundleItem);
			break;
		case RewardItemType.HERO_SKIN:
			premiumType = TAG_PREMIUM.GOLDEN;
			break;
		}
		if (!InitializeRewardItemDataModel(item, rarity, premiumType, out var failReason))
		{
			if (netBundle != null)
			{
				ProductIssues.LogError(netBundle, "License or VC grant reward invalid. " + (failReason ?? "Unspecified reason"));
			}
			return false;
		}
		return true;
	}

	private static TAG_PREMIUM GetPremiumTypeFromNetBundleAttributes(Network.BundleItem netBundleItem)
	{
		TAG_PREMIUM premiumType = TAG_PREMIUM.NORMAL;
		if (netBundleItem != null)
		{
			netBundleItem.Attributes.GetValue("premium").Match(delegate(string premium)
			{
				if (premium.Equals("1"))
				{
					premiumType = TAG_PREMIUM.GOLDEN;
				}
				else if (premium.Equals("2"))
				{
					premiumType = TAG_PREMIUM.DIAMOND;
				}
				else if (premium.Equals("3"))
				{
					premiumType = TAG_PREMIUM.SIGNATURE;
				}
			});
		}
		return premiumType;
	}

	private static CardDataModel CreateCardDataModelFromRecord(CardDbfRecord cardRec)
	{
		string rarityText = "";
		if (GameUtils.TryGetCardTagRecords(cardRec.NoteMiniGuid, out var cardTags))
		{
			CardTagDbfRecord rarityRecord = cardTags.Find((CardTagDbfRecord tagRecord) => tagRecord.TagId == 203);
			if (rarityRecord != null)
			{
				rarityText = GameStrings.GetRarityTextKey((TAG_RARITY)rarityRecord.TagValue);
			}
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardRec.ID);
		string cardText = "";
		if (entityDef != null)
		{
			cardText = entityDef.GetCardTextInHand();
		}
		return new CardDataModel
		{
			CardId = cardRec.NoteMiniGuid,
			FlavorText = cardRec.FlavorText,
			Rarity = rarityText,
			Name = cardRec.Name.GetString(),
			CardText = cardText
		};
	}

	public static bool InitializeRewardItemDataModel(RewardItemDataModel item, TAG_RARITY rarity, TAG_PREMIUM premium, out string failReason)
	{
		bool isValidItem = false;
		failReason = null;
		switch (item.ItemType)
		{
		case RewardItemType.BOOSTER:
			if (GameDbf.Booster.HasRecord(item.ItemId))
			{
				item.Booster = new PackDataModel
				{
					Type = (BoosterDbId)item.ItemId,
					Quantity = item.Quantity,
					OverrideWatermark = IsWatermarkOverridden(item.ItemId)
				};
				isValidItem = true;
			}
			else
			{
				failReason = $"Booster reward has unknown ID {item.ItemId}";
			}
			break;
		case RewardItemType.RANDOM_CARD:
			item.RandomCard = new RandomCardDataModel
			{
				Premium = premium,
				Rarity = rarity,
				Count = item.Quantity
			};
			if (rarity != 0 && Enum.IsDefined(typeof(TAG_RARITY), rarity))
			{
				isValidItem = true;
			}
			else
			{
				failReason = $"Random card reward has invalid rarity {rarity}";
			}
			break;
		case RewardItemType.ARENA_TICKET:
		case RewardItemType.BATTLEGROUNDS_BONUS:
		case RewardItemType.TAVERN_BRAWL_TICKET:
		case RewardItemType.PROGRESSION_BONUS:
		case RewardItemType.REWARD_TRACK_XP_BOOST:
		case RewardItemType.MERCENARY_COIN:
		case RewardItemType.MERCENARY:
		case RewardItemType.MERCENARY_RANDOM_MERCENARY:
		case RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC:
		case RewardItemType.MERCENARY_KNOCKOUT_RANDOM:
		case RewardItemType.LUCKY_DRAW:
			isValidItem = true;
			break;
		case RewardItemType.DUST:
		case RewardItemType.CN_RUNESTONES:
		case RewardItemType.CN_ARCANE_ORBS:
		case RewardItemType.ROW_RUNESTONES:
		case RewardItemType.BATTLEGROUNDS_TOKEN:
		{
			item.Currency = new PriceDataModel
			{
				Currency = RewardItemTypeToCurrencyType(item.ItemType),
				Amount = item.Quantity,
				DisplayText = item.Quantity.ToString()
			};
			CurrencyType currencyType = item.Currency.Currency;
			if (ShopUtils.IsCurrencyVirtual(currencyType))
			{
				if (ShopUtils.IsVirtualCurrencyEnabled() && ShopUtils.IsVirtualCurrencyTypeEnabled(currencyType))
				{
					isValidItem = true;
				}
				else
				{
					failReason = $"Reward currency {currencyType} is a virtual currency and VC is not enabled/active";
				}
			}
			else
			{
				isValidItem = true;
			}
			break;
		}
		case RewardItemType.CARD_BACK:
			if (GameDbf.CardBack.HasRecord(item.ItemId))
			{
				item.CardBack = new CardBackDataModel
				{
					CardBackId = item.ItemId
				};
				isValidItem = true;
			}
			else
			{
				failReason = $"Card Back reward has unknown ID {item.ItemId}";
			}
			break;
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
			if (CollectionManager.Get().IsBattlegroundsGuideSkinCard(item.ItemId))
			{
				CardDbfRecord heroSkinCardRec = GameDbf.Card.GetRecord(item.ItemId);
				if (heroSkinCardRec != null)
				{
					if (GameUtils.GetCardHeroRecordForCardId(heroSkinCardRec.ID) != null)
					{
						item.Card = new CardDataModel
						{
							CardId = heroSkinCardRec.NoteMiniGuid,
							Premium = premium
						};
						isValidItem = true;
					}
					else
					{
						failReason = $"Battlegrounds Guide Skin reward has Card ID {item.ItemId} with no CARD_HERO subtable. NoteMiniGuid={heroSkinCardRec.NoteMiniGuid}";
					}
				}
				else
				{
					failReason = $"Battlegrounds Guide Skin reward has unknown Card ID {item.ItemId}";
				}
			}
			else
			{
				failReason = $"Battlegrounds Guide Skin reward has Card ID {item.ItemId} with no corresponding entry in the Battlegrounds Guide Skin table";
			}
			break;
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
			if (CollectionManager.Get().IsBattlegroundsHeroSkinCard(item.ItemId))
			{
				CardDbfRecord heroSkinCardRec3 = GameDbf.Card.GetRecord(item.ItemId);
				if (heroSkinCardRec3 != null)
				{
					if (GameUtils.GetCardHeroRecordForCardId(heroSkinCardRec3.ID) != null)
					{
						item.Card = new CardDataModel
						{
							CardId = heroSkinCardRec3.NoteMiniGuid,
							Premium = premium
						};
						isValidItem = true;
					}
					else
					{
						failReason = $"Battlegrounds Hero Skin reward has Card ID {item.ItemId} with no CARD_HERO subtable. NoteMiniGuid={heroSkinCardRec3.NoteMiniGuid}";
					}
				}
				else
				{
					failReason = $"Battlegrounds Hero Skin reward has unknown Card ID {item.ItemId}";
				}
			}
			else
			{
				failReason = $"Battlegrounds Hero Skin reward has Card ID {item.ItemId} with no corresponding entry in the battlegrounds hero skin table";
			}
			break;
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			if (CollectionManager.Get().IsValidBattlegroundsBoardSkinId(BattlegroundsBoardSkinId.FromTrustedValue(item.ItemId)))
			{
				BattlegroundsBoardSkinDbfRecord boardSkinRec = GameDbf.BattlegroundsBoardSkin.GetRecord(item.ItemId);
				if (boardSkinRec != null)
				{
					bool enableCosmeticsRendering = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
					item.BGBoardSkin = new BattlegroundsBoardSkinDataModel
					{
						DisplayName = boardSkinRec.CollectionShortName,
						DetailsDisplayName = boardSkinRec.CollectionName,
						Description = boardSkinRec.Description,
						BorderType = boardSkinRec.BorderType,
						BoardDbiId = item.ItemId,
						ShopDetailsTexture = ((PlatformSettings.Screen == ScreenCategory.Phone) ? boardSkinRec.DetailsTexturePhone : boardSkinRec.DetailsTexture),
						ShopDetailsMovie = ((PlatformSettings.Screen == ScreenCategory.Phone) ? boardSkinRec.DetailsMoviePhone : boardSkinRec.DetailsMovie),
						Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)boardSkinRec.Rarity)),
						DetailsRenderConfig = boardSkinRec.DetailsRenderConfig,
						CosmeticsRenderingEnabled = enableCosmeticsRendering
					};
					isValidItem = true;
				}
				else
				{
					failReason = $"Battlegrounds Board Skin Item has unknown board skin id [{item.ItemId}]";
				}
			}
			else
			{
				failReason = $"Battlegrounds Board Skin Item has invalid card DBIid [{item.ItemId}] with no corresponding entry in the board skin table.";
			}
			break;
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			if (CollectionManager.Get().IsValidBattlegroundsFinisherId(BattlegroundsFinisherId.FromTrustedValue(item.ItemId)))
			{
				BattlegroundsFinisherDbfRecord finisherRec = GameDbf.BattlegroundsFinisher.GetRecord(item.ItemId);
				if (finisherRec != null)
				{
					bool enableCosmeticsRendering2 = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
					item.BGFinisher = new BattlegroundsFinisherDataModel
					{
						DisplayName = finisherRec.CollectionShortName,
						DetailsDisplayName = finisherRec.CollectionName,
						Description = finisherRec.Description,
						FinisherDbiId = item.ItemId,
						CapsuleType = finisherRec.CapsuleType,
						ShopDetailsMovie = finisherRec.DetailsMovie,
						ShopDetailsTexture = finisherRec.DetailsTexture,
						BodyMaterial = finisherRec.MiniBodyMaterial,
						ArtMaterial = finisherRec.MiniArtMaterial,
						Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)finisherRec.Rarity)),
						DetailsRenderConfig = finisherRec.DetailsRenderConfig,
						CosmeticsRenderingEnabled = enableCosmeticsRendering2
					};
					isValidItem = true;
				}
				else
				{
					failReason = $"Battlegrounds Finisher Item has unknown finisher id [{item.ItemId}]";
				}
			}
			else
			{
				failReason = $"Battlegrounds Finisher Item has invalid ID [{item.ItemId}] with no corresponding entry in the finisher table.";
			}
			break;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
			if (CollectionManager.Get().IsValidBattlegroundsEmoteId(BattlegroundsEmoteId.FromTrustedValue(item.ItemId)))
			{
				BattlegroundsEmoteDbfRecord emoteRec = GameDbf.BattlegroundsEmote.GetRecord(item.ItemId);
				if (emoteRec != null)
				{
					item.BGEmote = new BattlegroundsEmoteDataModel
					{
						DisplayName = emoteRec.CollectionShortName,
						Description = emoteRec.Description,
						EmoteDbiId = item.ItemId,
						Animation = emoteRec.AnimationPath,
						IsAnimating = emoteRec.IsAnimating,
						BorderType = emoteRec.BorderType,
						XOffset = (float)emoteRec.XOffset,
						ZOffset = (float)emoteRec.ZOffset,
						Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)emoteRec.Rarity))
					};
					isValidItem = true;
				}
				else
				{
					failReason = $"Battlegrounds Emote Item has unknown emote id [{item.ItemId}]";
				}
			}
			else
			{
				failReason = $"Battlegrounds Emote Item has invalid ID [{item.ItemId}] with no corresponding entry in the emote table.";
			}
			break;
		case RewardItemType.HERO_SKIN:
		{
			CardDbfRecord heroSkinCardRec2 = GameDbf.Card.GetRecord(item.ItemId);
			if (heroSkinCardRec2 != null)
			{
				if (GameUtils.GetCardHeroRecordForCardId(heroSkinCardRec2.ID) != null)
				{
					item.Card = new CardDataModel
					{
						CardId = heroSkinCardRec2.NoteMiniGuid,
						Premium = premium,
						IsShopPremiumHeroSkin = IsShopPremiumHeroSkin(heroSkinCardRec2)
					};
					isValidItem = true;
				}
				else
				{
					failReason = $"Hero Skin reward has Card ID {item.ItemId} with no CARD_HERO subtable. NoteMiniGuid={heroSkinCardRec2.NoteMiniGuid}";
				}
			}
			else
			{
				failReason = $"Hero Skin reward has unknown Card ID {item.ItemId}";
			}
			break;
		}
		case RewardItemType.CUSTOM_COIN:
		{
			CosmeticCoinDbfRecord customCoinRec = GameDbf.CosmeticCoin.GetRecord(item.ItemId);
			if (customCoinRec != null)
			{
				CardDbfRecord cardRec2 = GameDbf.Card.GetRecord(customCoinRec.CardId);
				if (cardRec2 != null)
				{
					item.Card = new CardDataModel
					{
						CardId = cardRec2.NoteMiniGuid,
						Premium = premium
					};
					isValidItem = true;
				}
				else
				{
					failReason = $"Custom Coin reward {item.ItemId} has unknown Card ID in COIN table {customCoinRec.CardId}";
				}
			}
			else
			{
				failReason = $"Custom Coin reward has unknown ID {item.ItemId}";
			}
			break;
		}
		case RewardItemType.CARD:
		{
			CardDbfRecord cardRec = GameDbf.Card.GetRecord(item.ItemId);
			if (cardRec != null)
			{
				item.Card = CreateCardDataModelFromRecord(cardRec);
				item.Card.Premium = premium;
				isValidItem = true;
			}
			else
			{
				failReason = $"Card reward has unknown ID {item.ItemId}";
			}
			break;
		}
		case RewardItemType.ADVENTURE_WING:
			if (GameDbf.Wing.HasRecord(item.ItemId))
			{
				isValidItem = true;
			}
			else
			{
				failReason = $"Adventure Wing reward has unknown ID {item.ItemId}";
			}
			break;
		case RewardItemType.MINI_SET:
			if (GameDbf.MiniSet.HasRecord(item.ItemId))
			{
				isValidItem = true;
			}
			else
			{
				failReason = $"Mini Set reward has unknown ID {item.ItemId}";
			}
			break;
		case RewardItemType.SELLABLE_DECK:
			if (GameDbf.SellableDeck.HasRecord(item.ItemId))
			{
				isValidItem = true;
			}
			else
			{
				failReason = $"Sellable Deck reward has unknown ID {item.ItemId}";
			}
			break;
		case RewardItemType.MERCENARY_BOOSTER:
			item.Booster = new PackDataModel
			{
				Type = BoosterDbId.MERCENARIES,
				Quantity = item.Quantity
			};
			isValidItem = true;
			break;
		default:
			failReason = $"Reward has unsupported type {item.ItemType}";
			break;
		}
		return isValidItem;
	}

	public static CurrencyType RewardItemTypeToCurrencyType(RewardItemType itemType)
	{
		return itemType switch
		{
			RewardItemType.DUST => CurrencyType.DUST, 
			RewardItemType.CN_ARCANE_ORBS => CurrencyType.CN_ARCANE_ORBS, 
			RewardItemType.CN_RUNESTONES => CurrencyType.CN_RUNESTONES, 
			RewardItemType.ROW_RUNESTONES => CurrencyType.ROW_RUNESTONES, 
			RewardItemType.BATTLEGROUNDS_TOKEN => CurrencyType.BG_TOKEN, 
			_ => CurrencyType.NONE, 
		};
	}

	public static int GetRewardItemTypeSortOrder(RewardItemType itemType)
	{
		return itemType switch
		{
			RewardItemType.REWARD_TRACK_XP_BOOST => 50, 
			RewardItemType.HERO_SKIN => 100, 
			RewardItemType.CARD_BACK => 200, 
			RewardItemType.CARD => 300, 
			RewardItemType.MERCENARY => 350, 
			RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC => 360, 
			RewardItemType.RANDOM_CARD => 400, 
			RewardItemType.MERCENARY_RANDOM_MERCENARY => 415, 
			RewardItemType.MERCENARY_KNOCKOUT_RANDOM => 440, 
			RewardItemType.CARD_SUBSET => 425, 
			RewardItemType.BATTLEGROUNDS_BONUS => 450, 
			RewardItemType.ARENA_TICKET => 500, 
			RewardItemType.TAVERN_BRAWL_TICKET => 550, 
			RewardItemType.ADVENTURE => 600, 
			RewardItemType.ADVENTURE_WING => 700, 
			RewardItemType.BOOSTER => 800, 
			RewardItemType.MERCENARY_BOOSTER => 850, 
			RewardItemType.CN_RUNESTONES => 900, 
			RewardItemType.ROW_RUNESTONES => 950, 
			RewardItemType.CN_ARCANE_ORBS => 1000, 
			RewardItemType.MERCENARY_COIN => 1040, 
			RewardItemType.DUST => 1100, 
			RewardItemType.PROGRESSION_BONUS => 1200, 
			RewardItemType.MINI_SET => 1300, 
			RewardItemType.SELLABLE_DECK => 1400, 
			RewardItemType.BATTLEGROUNDS_FINISHER => 1433, 
			RewardItemType.BATTLEGROUNDS_BOARD_SKIN => 1466, 
			RewardItemType.BATTLEGROUNDS_GUIDE_SKIN => 1500, 
			RewardItemType.BATTLEGROUNDS_EMOTE => 1550, 
			RewardItemType.BATTLEGROUNDS_HERO_SKIN => 1600, 
			_ => int.MaxValue, 
		};
	}

	public static int CompareItemsForSort(RewardItemDataModel xItem, RewardItemDataModel yItem)
	{
		if (xItem == null && yItem == null)
		{
			return 0;
		}
		if (xItem == null)
		{
			return 1;
		}
		if (yItem == null)
		{
			return -1;
		}
		int xItemTypeOrder = GetRewardItemTypeSortOrder(xItem.ItemType);
		int yItemTypeOrder = GetRewardItemTypeSortOrder(yItem.ItemType);
		if (xItemTypeOrder < yItemTypeOrder)
		{
			return -1;
		}
		if (xItemTypeOrder > yItemTypeOrder)
		{
			return 1;
		}
		if (xItem.Quantity > yItem.Quantity)
		{
			return -1;
		}
		if (xItem.Quantity < yItem.Quantity)
		{
			return 1;
		}
		if (xItem.RandomCard != null && yItem.RandomCard != null)
		{
			return -1 * xItem.RandomCard.Premium.CompareTo(yItem.RandomCard.Premium);
		}
		if (xItem.Card != null && yItem.Card != null)
		{
			return -1 * xItem.Card.Premium.CompareTo(yItem.Card.Premium);
		}
		if (xItem.Booster != null && yItem.Booster != null)
		{
			BoosterDbfRecord xRecord = GameDbf.Booster.GetRecord((int)xItem.Booster.Type);
			BoosterDbfRecord yRecord = GameDbf.Booster.GetRecord((int)yItem.Booster.Type);
			if (xRecord == null && yRecord == null)
			{
				return 0;
			}
			if (xRecord == null)
			{
				return 1;
			}
			if (yRecord == null)
			{
				return -1;
			}
			int sortByPackResult = GameUtils.PackSortingPredicate(xRecord, yRecord);
			if (sortByPackResult != 0)
			{
				return sortByPackResult;
			}
		}
		return 0;
	}

	public static bool IsShopPremiumHeroSkin(EntityDef entityDef)
	{
		CardDbfRecord cardDbf = GameDbf.GetIndex().GetCardRecord(entityDef.GetCardId());
		if (cardDbf == null)
		{
			return false;
		}
		return IsShopPremiumHeroSkin(cardDbf);
	}

	public static bool IsShopPremiumHeroSkin(CardDbfRecord cardRecord)
	{
		if (cardRecord == null)
		{
			return false;
		}
		if (GameUtils.TryGetCardTagRecords(cardRecord.NoteMiniGuid, out var cardTags))
		{
			foreach (CardTagDbfRecord tag in cardTags)
			{
				if (tag.TagId == 3495)
				{
					return tag.TagValue == 1;
				}
			}
		}
		return false;
	}

	public static void SetNewRewardedDeck(long collectionDeckId)
	{
		Options.Get().SetLong(Option.NEWEST_REWARDED_DECK_ID, collectionDeckId);
	}

	public static bool HasNewRewardedDeck(out long collectionDeckId)
	{
		collectionDeckId = Options.Get().GetLong(Option.NEWEST_REWARDED_DECK_ID);
		return collectionDeckId != 0;
	}

	public static void MarkNewestRewardedDeckAsSeen()
	{
		SetNewRewardedDeck(0L);
	}

	public static int CompareOwnedItemsForSort(RewardItemDataModel xItem, RewardItemDataModel yItem)
	{
		if (xItem != null && xItem.Card != null && yItem != null && yItem.Card != null)
		{
			if (xItem.Card.Owned && !yItem.Card.Owned)
			{
				return 1;
			}
			if (!xItem.Card.Owned && yItem.Card.Owned)
			{
				return -1;
			}
		}
		return CompareItemsForSort(xItem, yItem);
	}

	public static int GetSortOrderFromItems(DataModelList<RewardItemDataModel> items)
	{
		foreach (RewardItemDataModel item in items)
		{
			if (AttemptToGetItemSortOrder(item, out var sortOrder))
			{
				return sortOrder;
			}
		}
		return 0;
	}

	public static bool AttemptToGetItemSortOrder(RewardItemDataModel item, out int sortOrder)
	{
		if (item != null && item.ItemType == RewardItemType.SELLABLE_DECK && IsValidSellableDeckRecordId(item.ItemId))
		{
			sortOrder = GetSortOrderForSellableDeck(item.ItemId);
			return true;
		}
		sortOrder = 0;
		return false;
	}

	public static bool IsValidSellableDeckRecordId(int sellableDeckRecordId)
	{
		return GameDbf.SellableDeck.GetRecord(sellableDeckRecordId)?.DeckTemplateRecord != null;
	}

	public static int GetSortOrderForSellableDeck(int sellableDeckRecordId)
	{
		return GameDbf.SellableDeck.GetRecord(sellableDeckRecordId).DeckTemplateRecord.SortOrder;
	}

	public static void GetMercenaryName(string cardId, out string localizedName, out string localizedShortName)
	{
		EntityDef mercCardDef = DefLoader.Get().GetEntityDef(cardId);
		if (mercCardDef == null)
		{
			localizedName = null;
			localizedShortName = null;
			return;
		}
		string name = mercCardDef.GetName();
		string shortName = mercCardDef.GetShortName();
		if (string.IsNullOrEmpty(name))
		{
			localizedName = null;
			localizedShortName = null;
		}
		else
		{
			localizedName = GameStrings.FormatLocalizedString(name);
			localizedShortName = ((!string.IsNullOrWhiteSpace(shortName)) ? GameStrings.FormatLocalizedString(shortName) : localizedName);
		}
	}

	public static string GetMercenaryRarityText(TAG_RARITY rarity)
	{
		string text = "";
		return rarity switch
		{
			TAG_RARITY.RARE => GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TITLE_RARE"), 
			TAG_RARITY.EPIC => GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TITLE_EPIC"), 
			TAG_RARITY.LEGENDARY => GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TITLE_LEGENDARY"), 
			_ => GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TITLE_COMMON"), 
		};
	}

	public static string GetMercenaryKnockoutCoinsText(TAG_PREMIUM premium, string mercName, string mercShortName)
	{
		string text = "";
		if (premium == TAG_PREMIUM.NORMAL || !GameStrings.HasPremiumText(premium))
		{
			return GameStrings.Format("GLUE_LETTUCE_REWARD_KNOCKOUT_COINS_DESC", mercName, mercShortName);
		}
		string premiumString = GameStrings.GetPremiumText(premium);
		return GameStrings.Format("GLUE_LETTUCE_REWARD_KNOCKOUT_COINS_DESC_PREMIUM", premiumString, mercName, mercShortName);
	}

	public static RewardItemRewardData CreateMercenaryRewardData(int mercId, int artVariationId, TAG_PREMIUM premium)
	{
		return new RewardItemRewardData(CreateMercenaryRewardItemDataModel(mercId, artVariationId, premium), showQuestToast: false);
	}

	public static RewardData CreateMercenaryOrKnockoutRewardData(int mercId, int artVariationId, TAG_PREMIUM premium, int currencyAmount)
	{
		RewardItemDataModel mercRewardItem = CreateMercenaryRewardItemDataModel(mercId, artVariationId, premium);
		GetMercenaryName(mercRewardItem.Mercenary.Card.CardId, out var mercName, out var shortMercName);
		if (mercName == null)
		{
			return null;
		}
		RewardData rewardData = null;
		if (currencyAmount > 0)
		{
			RewardItemDataModel mercKnockoutReward = CreateMercenaryCoinsRewardData(mercId, currencyAmount, glowActive: true, nameActive: false).DataModel;
			rewardData = new MercenariesKnockoutRewardData(mercRewardItem, mercKnockoutReward);
			rewardData.NameOverride = GetMercenaryRarityText(mercRewardItem.Mercenary.MercenaryRarity);
			if (mercName != null && shortMercName != null)
			{
				rewardData.DescriptionOverride = GetMercenaryKnockoutCoinsText(premium, mercName, shortMercName);
			}
		}
		else
		{
			rewardData = new RewardItemRewardData(mercRewardItem, showQuestToast: true, Reward.Type.MERCENARY_MERCENARY);
			if (!string.IsNullOrEmpty(mercName))
			{
				rewardData.NameOverride = mercName;
			}
			if (!string.IsNullOrEmpty(shortMercName))
			{
				rewardData.DescriptionOverride = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_DESC", shortMercName);
			}
		}
		return rewardData;
	}

	public static RewardItemDataModel CreateMercenaryRewardItemDataModel(int mercId, int artVariationId, TAG_PREMIUM premium)
	{
		LettuceMercenaryDataModel mercenary = MercenaryFactory.CreateMercenaryDataModel(mercId, artVariationId, premium);
		LettuceMercenary lettuceMercenary = CollectionManager.Get().GetMercenary(mercId, AttemptToGenerate: false, ReportError: false);
		RewardItemDataModel rewardItemDataModel = new RewardItemDataModel();
		rewardItemDataModel.ItemType = RewardItemType.MERCENARY;
		rewardItemDataModel.Quantity = 1;
		rewardItemDataModel.Mercenary = mercenary;
		rewardItemDataModel.IsMercenaryPortrait = IsMercenaryRewardPortrait(mercenary) && lettuceMercenary != null && lettuceMercenary.m_owned;
		rewardItemDataModel.Mercenary.Owned = true;
		rewardItemDataModel.Mercenary.HideXp = true;
		rewardItemDataModel.Mercenary.HideWatermark = false;
		rewardItemDataModel.Mercenary.Label = string.Empty;
		return rewardItemDataModel;
	}

	private static string GetNameFromCardReward(RewardItemDataModel rewardItemDataModel)
	{
		if (rewardItemDataModel.Card?.Name != null)
		{
			return rewardItemDataModel.Card.Name;
		}
		if (rewardItemDataModel.Card?.CardId != null)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(rewardItemDataModel.Card.CardId);
			if (entityDef != null)
			{
				return entityDef.GetName();
			}
		}
		Log.Dbf.PrintWarning("GetNameFromCardReward could not find a card name for this RewardItemDataModel.");
		return null;
	}

	public static bool IsWatermarkOverridden(int boosterSetId)
	{
		BoosterCardSetDbfRecord boosterCardSetRecord = GameDbf.BoosterCardSet?.GetRecord(boosterSetId);
		if (boosterCardSetRecord != null && !string.IsNullOrEmpty(boosterCardSetRecord.WatermarkTextureOverride))
		{
			return true;
		}
		return false;
	}

	public static string GetName(RewardItemDataModel rewardItemDataModel)
	{
		if (rewardItemDataModel == null)
		{
			Log.Gameplay.PrintWarning("GetNameFromCardReward tried to get name from a null RewardItemDataModel.");
			return null;
		}
		switch (rewardItemDataModel.ItemType)
		{
		default:
			throw new NotImplementedException();
		case RewardItemType.BOOSTER:
		case RewardItemType.MERCENARY_BOOSTER:
			return rewardItemDataModel.Booster?.BoosterName;
		case RewardItemType.HERO_SKIN:
		case RewardItemType.CARD:
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
			return GetNameFromCardReward(rewardItemDataModel);
		case RewardItemType.MERCENARY_COIN:
			return rewardItemDataModel.MercenaryCoin?.MercenaryName;
		case RewardItemType.MERCENARY:
			return rewardItemDataModel.Mercenary?.MercenaryName;
		case RewardItemType.MERCENARY_EQUIPMENT:
		case RewardItemType.MERCENARY_EQUIPMENT_ICON:
			return rewardItemDataModel.MercenaryEquip?.AbilityName;
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			return rewardItemDataModel.BGBoardSkin?.DetailsDisplayName;
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			return rewardItemDataModel.BGFinisher?.DetailsDisplayName;
		case RewardItemType.BATTLEGROUNDS_EMOTE:
			return rewardItemDataModel.BGEmote?.DisplayName;
		}
	}

	public static RewardItemDataModel CreateKnockoutSpecificMercenaryRewardItemDataModel(int mercId)
	{
		int artVariationId = 0;
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		LettuceMercenaryDataModel mercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(mercId, artVariationId, premium);
		LettuceMercenary lettuceMercenary = CollectionManager.Get().GetMercenary(mercId, AttemptToGenerate: false, ReportError: false);
		RewardItemDataModel rewardItemDataModel = new RewardItemDataModel();
		rewardItemDataModel.ItemType = RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC;
		rewardItemDataModel.Quantity = 1;
		rewardItemDataModel.Mercenary = mercenaryDataModel;
		rewardItemDataModel.IsMercenaryPortrait = IsMercenaryRewardPortrait(mercenaryDataModel) && lettuceMercenary != null && lettuceMercenary.m_owned;
		rewardItemDataModel.Mercenary.Owned = true;
		rewardItemDataModel.Mercenary.HideXp = true;
		rewardItemDataModel.Mercenary.HideWatermark = false;
		rewardItemDataModel.Mercenary.Label = string.Empty;
		return rewardItemDataModel;
	}

	public static RewardItemRewardData CreateMercenaryCoinsRewardData(int mercId, int quantity, bool glowActive, bool nameActive)
	{
		if (mercId == 0)
		{
			return new RewardItemRewardData(new RewardItemDataModel
			{
				ItemType = RewardItemType.MERCENARY_COIN,
				Quantity = 1,
				MercenaryCoin = new LettuceMercenaryCoinDataModel
				{
					Quantity = quantity,
					GlowActive = glowActive,
					IsRandom = true,
					NameActive = nameActive
				}
			}, showQuestToast: true, Reward.Type.MERCENARY_COIN);
		}
		string mercenaryCardId = GameUtils.GetCardIdFromMercenaryId(mercId);
		if (string.IsNullOrWhiteSpace(mercenaryCardId))
		{
			Log.Store.PrintError(string.Format("{0}: Failed to get card Id for merc id: {1}, from local data.", "RewardUtils", mercId));
			return null;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(mercenaryCardId);
		if (entityDef == null)
		{
			Log.Store.PrintError("RewardUtils: Failed to get entity def for merc card id: " + mercenaryCardId + ", from local data.");
			return null;
		}
		string shortName = entityDef.GetShortName();
		string mercName = (string.IsNullOrEmpty(shortName) ? entityDef.GetName() : shortName);
		return new RewardItemRewardData(new RewardItemDataModel
		{
			ItemType = RewardItemType.MERCENARY_COIN,
			Quantity = 1,
			MercenaryCoin = new LettuceMercenaryCoinDataModel
			{
				MercenaryId = mercId,
				MercenaryName = mercName,
				Quantity = quantity,
				GlowActive = glowActive,
				NameActive = nameActive
			}
		}, showQuestToast: true, Reward.Type.MERCENARY_COIN);
	}

	private static void TryGetToastTextFromFixedRewardMap(int rewardMapId, out string toastName, out string toastDesc, out bool shouldSkipToast)
	{
		toastDesc = null;
		toastName = null;
		FixedRewardMapDbfRecord fixedRewardMapRecord = GameDbf.FixedRewardMap.GetRecord(rewardMapId);
		if (fixedRewardMapRecord != null && !fixedRewardMapRecord.UseQuestToast)
		{
			shouldSkipToast = true;
			return;
		}
		shouldSkipToast = false;
		if (fixedRewardMapRecord != null)
		{
			toastName = fixedRewardMapRecord.ToastName;
			toastDesc = fixedRewardMapRecord.ToastDescription;
		}
	}

	public static IGamemodeAvailabilityService.Gamemode GetGamemodeFromRewardItemType(RewardItemType type)
	{
		IGamemodeAvailabilityService.Gamemode gamemode = IGamemodeAvailabilityService.Gamemode.NONE;
		switch (type)
		{
		case RewardItemType.UNDEFINED:
			return gamemode;
		case RewardItemType.HERO_SKIN:
		case RewardItemType.CARD_BACK:
		case RewardItemType.RANDOM_CARD:
		case RewardItemType.CARD:
		case RewardItemType.CARD_SUBSET:
			gamemode = IGamemodeAvailabilityService.Gamemode.HEARTHSTONE;
			break;
		case RewardItemType.BATTLEGROUNDS_BONUS:
		case RewardItemType.BATTLEGROUNDS_HERO_SKIN:
		case RewardItemType.BATTLEGROUNDS_GUIDE_SKIN:
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
		case RewardItemType.BATTLEGROUNDS_FINISHER:
		case RewardItemType.BATTLEGROUNDS_EMOTE:
		case RewardItemType.BATTLEGROUNDS_EMOTE_PILE:
		case RewardItemType.BATTLEGROUNDS_CARD:
			gamemode = IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS;
			break;
		case RewardItemType.MERCENARY_COIN:
		case RewardItemType.MERCENARY:
		case RewardItemType.MERCENARY_XP:
		case RewardItemType.MERCENARY_EQUIPMENT:
		case RewardItemType.MERCENARY_EQUIPMENT_ICON:
		case RewardItemType.MERCENARY_BOOSTER:
		case RewardItemType.MERCENARY_RANDOM_MERCENARY:
		case RewardItemType.MERCENARY_KNOCKOUT_SPECIFIC:
		case RewardItemType.MERCENARY_KNOCKOUT_RANDOM:
		case RewardItemType.MERCENARY_RENOWN:
			gamemode = IGamemodeAvailabilityService.Gamemode.MERCENARIES;
			break;
		case RewardItemType.ADVENTURE_WING:
		case RewardItemType.ADVENTURE:
			gamemode = IGamemodeAvailabilityService.Gamemode.SOLO_ADVENTURE;
			break;
		case RewardItemType.TAVERN_BRAWL_TICKET:
			gamemode = IGamemodeAvailabilityService.Gamemode.TAVERN_BRAWL;
			break;
		case RewardItemType.ARENA_TICKET:
			gamemode = IGamemodeAvailabilityService.Gamemode.ARENA;
			break;
		}
		return gamemode;
	}
}
