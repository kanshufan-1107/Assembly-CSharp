using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.InGameMessage.UI;

internal static class MessageDataModelFactory
{
	private static List<DefLoader.DisposableCardDef> s_disposableCardDefMercBounties;

	public static List<IDataModel> CreateDataModel(MessageUIData data)
	{
		switch (data.LayoutType)
		{
		case MessageLayoutType.TEXT:
			return CreateTextContentDataModel(data);
		case MessageLayoutType.DEBUG:
			return CreateDebugContentDataModel(data);
		case MessageLayoutType.SHOP:
			return CreateShopContentDataModel(data);
		case MessageLayoutType.LAUNCH:
			return CreateLaunchContentDataModel(data);
		case MessageLayoutType.CHANGE:
			return CreateChangeContentDataModel(data);
		case MessageLayoutType.MERCENARY:
			return CreateMercenaryContentDataModel(data);
		case MessageLayoutType.MERCENARY_BOUNTY:
			return CreateMercenaryBountyContentDataModel(data);
		case MessageLayoutType.EMPTY:
			return CreateEmptyMailboxContentDataModel(data);
		default:
			Log.InGameMessage.PrintWarning($"Unsupported IGM Layout type {data.LayoutType}. Cannot create data model");
			return null;
		}
	}

	private static List<IDataModel> CreateDebugContentDataModel(MessageUIData data)
	{
		MessageDebugContentDataModel dataModel = new MessageDebugContentDataModel();
		dataModel.TestString = (data.MessageData as TestDebugMessageUIData)?.TestString;
		return new List<IDataModel> { dataModel };
	}

	private static List<IDataModel> CreateTextContentDataModel(MessageUIData data)
	{
		TextMessageContentDataModel dataModel = new TextMessageContentDataModel();
		TextMessageContent textMessageContent = data.MessageData as TextMessageContent;
		dataModel.BodyText = textMessageContent.TextBody;
		dataModel.IconType = textMessageContent.ImageType;
		dataModel.Title = textMessageContent.Title;
		dataModel.ImageMaterial = (string.IsNullOrEmpty(textMessageContent.ImageMaterial) ? null : AssetLoader.Get().LoadAsset<Material>(new AssetReference(textMessageContent.ImageMaterial)));
		dataModel.ImageTexture = (string.IsNullOrEmpty(textMessageContent.ImageTexture) ? null : AssetLoader.Get().LoadAsset<Texture>(new AssetReference(textMessageContent.ImageTexture)));
		dataModel.Url = textMessageContent.Url;
		if (textMessageContent.DisplayItems != null && textMessageContent.DisplayItems.Count > 0)
		{
			foreach (TextMessageItemInformation curItem in textMessageContent.DisplayItems)
			{
				switch (curItem.ItemType)
				{
				case InGameMessageItemDisplayContent.ItemType.Card:
				{
					CardRewardData cardRewardData = new CardRewardData(curItem.ItemId, curItem.itemPremiumType, 1);
					dataModel.TextMessageItemDisplay.Add(RewardUtils.RewardDataToRewardItemDataModel(cardRewardData));
					break;
				}
				case InGameMessageItemDisplayContent.ItemType.Hero:
				{
					RewardItemDataModel rewardItemDataModel = GetHeroRewaredItemDataModel(curItem.ItemId, curItem.itemPremiumType);
					if (rewardItemDataModel != null)
					{
						dataModel.TextMessageItemDisplay.Add(rewardItemDataModel);
					}
					break;
				}
				case InGameMessageItemDisplayContent.ItemType.BattlegroundsCard:
				{
					RewardItemDataModel rewardItemDataModel = GetBattlegroundsCardItemDataModel(curItem.ItemId, curItem.itemPremiumType);
					if (rewardItemDataModel != null)
					{
						dataModel.TextMessageItemDisplay.Add(rewardItemDataModel);
					}
					break;
				}
				case InGameMessageItemDisplayContent.ItemType.Merc:
				{
					if (MercenaryMessageUtils.TryGetMercenaryRewardItemDataModel(curItem.ItemId, curItem.itemPremiumType, curItem.mercArtVariationId, out var mercRewardData))
					{
						dataModel.TextMessageItemDisplay.Add(mercRewardData);
					}
					break;
				}
				case InGameMessageItemDisplayContent.ItemType.Bounty:
					dataModel.TextMessageItemBountyDisplay.Add(CreateBountyDataItem(GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.Enabled), int.Parse(curItem.ItemId)));
					break;
				default:
					Log.InGameMessage.PrintError($"Received an unknown text display item ({curItem.ItemType})");
					break;
				}
			}
		}
		return new List<IDataModel> { dataModel };
	}

	private static List<IDataModel> CreateShopContentDataModel(MessageUIData data)
	{
		List<IDataModel> list = new List<IDataModel>();
		ShopMessageContent shopContent = data.MessageData as ShopMessageContent;
		ShopMessageContentDataModel dataModel = new ShopMessageContentDataModel
		{
			Title = shopContent.Title,
			BodyText = shopContent.TextBody,
			ImageTexture = shopContent.ImageTexture
		};
		list.Add(dataModel);
		list.Add(GetShopProductDataModel(shopContent.ProductID));
		return list;
	}

	private static IDataModel GetShopProductDataModel(long productID)
	{
		if (!ProductId.IsValid(productID))
		{
			Log.InGameMessage.PrintError($"Invalid product ID {productID}");
			return null;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.InGameMessage.PrintError("[GetShopProductDataModel] Product Data service not ready");
			return null;
		}
		ProductDataModel product = dataService.GetProductDataModel(productID);
		if (product == null)
		{
			Log.InGameMessage.PrintError("Unexpected null product data model");
			return null;
		}
		RewardListDataModel rewards = product.RewardList;
		if (rewards == null)
		{
			Log.InGameMessage.PrintError("Unexpected missing rewards list");
			return null;
		}
		if (IsMiniSetReward(rewards))
		{
			int rewardId = rewards.Items[0].ItemId;
			MiniSetDbfRecord rewardRecord = GameDbf.MiniSet.GetRecord(rewardId);
			try
			{
				return GetMiniSetRewardListDataModel(rewardRecord.DeckRecord);
			}
			catch (Exception ex)
			{
				Log.InGameMessage.PrintError("Error attempting to create miniset data models: {0}", ex.Message);
				return null;
			}
		}
		return product;
	}

	private static bool IsMiniSetReward(RewardListDataModel rewards)
	{
		if (rewards.Items.Count == 1)
		{
			return rewards.Items[0].ItemType == RewardItemType.MINI_SET;
		}
		return false;
	}

	private static IDataModel GetMiniSetRewardListDataModel(DeckDbfRecord deck)
	{
		DefLoader loader = DefLoader.Get();
		return new RewardListDataModel
		{
			Items = (from c in deck.Cards
				select loader.GetEntityDef(c.CardId) into ed
				where ed.GetRarity() == TAG_RARITY.LEGENDARY
				orderby ed.GetCost()
				select new RewardItemDataModel
				{
					ItemType = RewardItemType.CARD,
					Card = new CardDataModel
					{
						CardId = ed.GetCardId()
					}
				}).Append(new RewardItemDataModel
			{
				ItemType = RewardItemType.MINI_SET
			}).ToDataModelList()
		};
	}

	private static List<IDataModel> CreateLaunchContentDataModel(MessageUIData data)
	{
		List<IDataModel> list = new List<IDataModel>();
		LaunchMessageContent launchContent = data.MessageData as LaunchMessageContent;
		LaunchMessageContentDataModel dataModel = new LaunchMessageContentDataModel
		{
			Title = launchContent.Title,
			IconType = launchContent.IconType,
			ImageMaterial = (string.IsNullOrEmpty(launchContent.ImageMaterial) ? null : AssetLoader.Get().LoadAsset<Material>(new AssetReference(launchContent.ImageMaterial))),
			Texture = (string.IsNullOrEmpty(launchContent.ImageTexture) ? null : AssetLoader.Get().LoadAsset<Texture>(new AssetReference(launchContent.ImageTexture))),
			SubLayout = (string.IsNullOrEmpty(launchContent.SubLayout) ? "mode" : launchContent.SubLayout),
			Url = launchContent.Url
		};
		if (launchContent.Effect != null)
		{
			dataModel.LaunchEffect = new LaunchMessageEffectContentDataModel
			{
				EffectID = launchContent.Effect.EffectId,
				EffectSoundID = launchContent.Effect.EffectSoundId
			};
			if (ColorUtility.TryParseHtmlString(launchContent.Effect.EffectColor, out var color))
			{
				dataModel.LaunchEffect.EffectColor = color;
			}
		}
		list.Add(dataModel);
		return list;
	}

	private static List<IDataModel> CreateChangeContentDataModel(MessageUIData data)
	{
		List<IDataModel> dataModels = new List<IDataModel>();
		ChangeMessageContent changeContent = data.MessageData as ChangeMessageContent;
		ChangeMessageContentDataModel dataModel = new ChangeMessageContentDataModel
		{
			Title = changeContent.Title,
			BodyText = changeContent.BodyText,
			Url = changeContent.Url
		};
		if (changeContent.ChangeItems == null || changeContent.ChangeItems.Count == 0)
		{
			Log.InGameMessage.PrintWarning("There were no change items supplied with the change in game message.");
			return null;
		}
		foreach (ChangeMessageItemInformation curItem in changeContent.ChangeItems)
		{
			switch (curItem.ItemType)
			{
			case InGameMessageItemDisplayContent.ItemType.Hero:
			{
				RewardItemDataModel rewardItemDataModel = GetHeroRewaredItemDataModel(curItem.ItemId, curItem.itemPremiumType);
				if (rewardItemDataModel != null)
				{
					dataModel.ChangeMessageItemDisplay.Add(rewardItemDataModel);
				}
				break;
			}
			case InGameMessageItemDisplayContent.ItemType.Card:
			{
				CardRewardData cardRewardData = new CardRewardData(curItem.ItemId, curItem.itemPremiumType, 1);
				dataModel.ChangeMessageItemDisplay.Add(RewardUtils.RewardDataToRewardItemDataModel(cardRewardData));
				break;
			}
			case InGameMessageItemDisplayContent.ItemType.BattlegroundsCard:
			{
				RewardItemDataModel rewardItemDataModel = GetBattlegroundsCardItemDataModel(curItem.ItemId, curItem.itemPremiumType);
				if (rewardItemDataModel != null)
				{
					dataModel.ChangeMessageItemDisplay.Add(rewardItemDataModel);
				}
				break;
			}
			default:
				Log.InGameMessage.PrintError($"Received an unknown change message item ({curItem.ItemType})");
				break;
			}
		}
		dataModels.Add(dataModel);
		return dataModels;
	}

	private static List<IDataModel> CreateMercenaryContentDataModel(MessageUIData data)
	{
		List<IDataModel> dataModels = new List<IDataModel>();
		MercenaryMessageContent itemsContent = data.MessageData as MercenaryMessageContent;
		MercenaryMessagesContentDataModel dataModel = new MercenaryMessagesContentDataModel
		{
			Title = itemsContent.Title,
			BodyText = itemsContent.BodyText,
			Url = itemsContent.Url
		};
		if (itemsContent.Items == null || itemsContent.Items.Count == 0)
		{
			Log.InGameMessage.PrintWarning("There were no mercenary items supplied with the layout in game message.");
			return null;
		}
		foreach (MercenaryMessageItemInformation curItem in itemsContent.Items)
		{
			if (curItem.ItemType == InGameMessageItemDisplayContent.ItemType.Merc)
			{
				if (MercenaryMessageUtils.TryGetMercenaryRewardItemDataModel(curItem.ItemId, curItem.itemPremiumType, curItem.mercArtVariationId, out var mercRewardData))
				{
					dataModel.MercenaryMessageItemDisplay.Add(mercRewardData);
				}
			}
			else
			{
				Log.InGameMessage.PrintError($"Received an unknown mercenary item ({curItem.ItemType})");
			}
		}
		dataModels.Add(dataModel);
		return dataModels;
	}

	private static List<IDataModel> CreateMercenaryBountyContentDataModel(MessageUIData data)
	{
		UIMessageCallbacks callbacks = data.Callbacks;
		callbacks.OnClosed = (Action)Delegate.Combine(callbacks.OnClosed, new Action(OnMessageModalClosed));
		List<IDataModel> dataModels = new List<IDataModel>();
		MercenaryMessageBountyContent itemsContent = data.MessageData as MercenaryMessageBountyContent;
		MercenaryMessagesBountyContentDataModel dataModel = new MercenaryMessagesBountyContentDataModel
		{
			Title = itemsContent.Title,
			BodyText = itemsContent.BodyText,
			Url = itemsContent.Url
		};
		if (itemsContent.Items == null || itemsContent.Items.Count == 0)
		{
			Log.InGameMessage.PrintWarning("There were no mercenary bounty items supplied with the layout in message.");
			return null;
		}
		List<LettuceBountyDbfRecord> bountyRecords = (from r in GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.Enabled)
			orderby r.SortOrder
			select r).ToList();
		foreach (MercenaryMessageBountyItemInformation curItem in itemsContent.Items)
		{
			if (curItem.ItemType == InGameMessageItemDisplayContent.ItemType.Bounty)
			{
				dataModel.Bounties.Add(CreateBountyDataItem(bountyRecords, curItem.ItemId));
			}
			else
			{
				Log.InGameMessage.PrintError($"Received an unknown mercenary bounty item ({curItem.ItemType})");
			}
		}
		dataModels.Add(dataModel);
		return dataModels;
	}

	private static LettuceBountyDataModel CreateBountyDataItem(List<LettuceBountyDbfRecord> bountyRecords, int bountyBossID)
	{
		LettuceBountyDbfRecord bountyRecord = bountyRecords.Find((LettuceBountyDbfRecord r) => r.FinalBossCardId == bountyBossID);
		if (bountyRecord == null)
		{
			Log.InGameMessage.PrintWarning("There were no mercenary bounty record found for id:" + bountyBossID);
			return null;
		}
		Material bossCoinMaterial = null;
		DefLoader.DisposableCardDef disposableCardDefMercBounty = DefLoader.Get().GetCardDef(GameUtils.TranslateDbIdToCardId(bountyRecord.FinalBossCardId));
		if (disposableCardDefMercBounty != null)
		{
			bossCoinMaterial = disposableCardDefMercBounty.CardDef.m_MercenaryMapBossCoinPortrait;
			if (s_disposableCardDefMercBounties == null)
			{
				s_disposableCardDefMercBounties = new List<DefLoader.DisposableCardDef>();
			}
			s_disposableCardDefMercBounties.Add(disposableCardDefMercBounty);
		}
		return new LettuceBountyDataModel
		{
			BountyId = bountyRecord.ID,
			AdventureMission = new AdventureMissionDataModel
			{
				CoinPortraitMaterialCount = 1,
				CoinPortraitMaterial = bossCoinMaterial,
				MissionState = AdventureMissionState.UNLOCKED
			},
			Available = true,
			ComingSoonText = bountyRecord.ComingSoonText,
			PosterText = LettuceVillageDataUtil.GeneratePosterName(bountyRecord)
		};
	}

	private static List<IDataModel> CreateEmptyMailboxContentDataModel(MessageUIData data)
	{
		List<IDataModel> list = new List<IDataModel>();
		TextMessageContent emptyMailboxContent = data.MessageData as TextMessageContent;
		TextMessageContentDataModel dataModel = new TextMessageContentDataModel
		{
			IconType = emptyMailboxContent.ImageType,
			Title = emptyMailboxContent.Title
		};
		list.Add(dataModel);
		return list;
	}

	private static void OnMessageModalClosed()
	{
		s_disposableCardDefMercBounties?.DisposeValuesAndClear();
	}

	private static RewardItemDataModel GetHeroRewaredItemDataModel(string cardId, TAG_PREMIUM premium)
	{
		RewardItemDataModel rewardItemDataModel = new RewardItemDataModel
		{
			ItemType = RewardItemType.HERO_SKIN,
			ItemId = GameUtils.TranslateCardIdToDbId(cardId)
		};
		if (!RewardUtils.InitializeRewardItemDataModel(rewardItemDataModel, TAG_RARITY.INVALID, premium, out var failReason))
		{
			Log.InGameMessage.PrintError("Could not create hero reward item for " + cardId + ": " + failReason);
			return null;
		}
		return rewardItemDataModel;
	}

	private static RewardItemDataModel GetBattlegroundsCardItemDataModel(string cardId, TAG_PREMIUM premium)
	{
		RewardItemDataModel rewardItemDataModel = new RewardItemDataModel
		{
			ItemType = RewardItemType.BATTLEGROUNDS_CARD,
			ItemId = GameUtils.TranslateCardIdToDbId(cardId)
		};
		EntityDef battlegroundsCardDef = DefLoader.Get().GetEntityDef(cardId);
		if (battlegroundsCardDef != null)
		{
			CardDataModel cardDataModel = new CardDataModel
			{
				CardId = cardId,
				Premium = premium,
				Attack = battlegroundsCardDef.GetATK(),
				Health = battlegroundsCardDef.GetHealth()
			};
			rewardItemDataModel.Card = cardDataModel;
		}
		else
		{
			Log.InGameMessage.PrintError("MessageDataModelFactory.GetBattlegroundsCardItemDataModel() - No EntityDef for card ID {0}!", cardId);
		}
		return rewardItemDataModel;
	}
}
