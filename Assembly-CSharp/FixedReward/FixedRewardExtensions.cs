using Assets;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using UnityEngine;

namespace FixedReward;

public static class FixedRewardExtensions
{
	public static Achieve.RewardTiming GetRewardTiming(this FixedRewardMapDbfRecord record)
	{
		if (EnumUtils.TryGetEnum<Achieve.RewardTiming>(record.RewardTiming, out var timing))
		{
			return timing;
		}
		Debug.LogWarning($"QueueRewardVisual rewardMapID={record.ID} no enum value for reward visual timing {record.RewardTiming}, check fixed rewards map");
		return Achieve.RewardTiming.IMMEDIATE;
	}

	public static Reward GetFixedReward(this FixedRewardMapDbfRecord record)
	{
		Reward reward = new Reward();
		FixedRewardDbfRecord dbfRecReward = GameDbf.FixedReward.GetRecord(record.RewardId);
		if (dbfRecReward == null)
		{
			return reward;
		}
		reward.Type = dbfRecReward.Type;
		switch (reward.Type)
		{
		case Assets.FixedReward.Type.VIRTUAL_CARD:
		{
			NetCache.CardDefinition cardDef3 = dbfRecReward.GetCardDefinition();
			if (cardDef3 != null)
			{
				reward.FixedCardRewardData = new CardRewardData(cardDef3.Name, cardDef3.Premium, record.RewardCount)
				{
					FixedReward = record
				};
				if (GameUtils.IsClassicCard(dbfRecReward.CardId))
				{
					reward.FixedCraftableCardRewardData = cardDef3;
				}
			}
			break;
		}
		case Assets.FixedReward.Type.HERO_SKIN:
		{
			NetCache.CardDefinition cardDef = dbfRecReward.GetCardDefinition();
			if (cardDef != null)
			{
				reward.FixedCardRewardData = new CardRewardData(cardDef.Name, cardDef.Premium, record.RewardCount)
				{
					FixedReward = record
				};
			}
			break;
		}
		case Assets.FixedReward.Type.CARDBACK:
		{
			int cardBackID = dbfRecReward.CardBackId;
			reward.FixedCardBackRewardData = new CardBackRewardData(cardBackID);
			break;
		}
		case Assets.FixedReward.Type.CRAFTABLE_CARD:
		{
			NetCache.CardDefinition cardDef2 = dbfRecReward.GetCardDefinition();
			if (cardDef2 != null)
			{
				reward.FixedCraftableCardRewardData = cardDef2;
			}
			break;
		}
		case Assets.FixedReward.Type.META_ACTION_FLAGS:
		{
			int metaActionID = dbfRecReward.MetaActionId;
			ulong metaActionFlags = dbfRecReward.MetaActionFlags;
			reward.MetaActionData = new MetaAction(metaActionID);
			reward.MetaActionData.UpdateFlags(metaActionFlags, 0uL);
			break;
		}
		case Assets.FixedReward.Type.BATTLEGROUNDS_GUIDE_SKIN:
		{
			dbfRecReward.GetCardDefinition();
			CardDbfRecord cardRec2 = dbfRecReward.BattlegroundsGuideSkinRecord.SkinCardRecord;
			if (cardRec2 != null)
			{
				RewardItemDataModel rewardDataModel2 = new RewardItemDataModel
				{
					ItemType = RewardItemType.BATTLEGROUNDS_GUIDE_SKIN,
					Quantity = 1,
					ItemId = cardRec2.ID,
					Card = new CardDataModel
					{
						CardId = cardRec2.NoteMiniGuid
					}
				};
				reward.FixedRewardData = new RewardItemRewardData(rewardDataModel2, showQuestToast: true, global::Reward.Type.BATTLEGROUNDS_GUIDE_SKIN);
			}
			break;
		}
		case Assets.FixedReward.Type.BATTLEGROUNDS_HERO_SKIN:
		{
			dbfRecReward.GetCardDefinition();
			CardDbfRecord cardRec = dbfRecReward.BattlegroundsHeroSkinRecord.SkinCardRecord;
			if (cardRec != null)
			{
				RewardItemDataModel rewardDataModel = new RewardItemDataModel
				{
					ItemType = RewardItemType.BATTLEGROUNDS_HERO_SKIN,
					Quantity = 1,
					ItemId = cardRec.ID,
					Card = new CardDataModel
					{
						CardId = cardRec.NoteMiniGuid
					}
				};
				reward.FixedRewardData = new RewardItemRewardData(rewardDataModel, showQuestToast: true, global::Reward.Type.BATTLEGROUNDS_HERO_SKIN);
			}
			break;
		}
		}
		return reward;
	}

	private static NetCache.CardDefinition GetCardDefinition(this FixedRewardDbfRecord record)
	{
		string cardName = GameUtils.TranslateDbIdToCardId(record.CardId);
		if (cardName == null)
		{
			return null;
		}
		if (!EnumUtils.TryCast<TAG_PREMIUM>(record.CardPremium, out var premium))
		{
			return null;
		}
		return new NetCache.CardDefinition
		{
			Name = cardName,
			Premium = premium
		};
	}
}
