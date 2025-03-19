using System;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

public static class LettuceRenownUtil
{
	private static MercenaryVillageRenownTradeDataModel s_currentRenownTradeDataModel;

	public static MercenariesRenownOfferData LastDismissedRequestedOffer { get; private set; }

	public static MercenariesRenownOfferData LastPurchaseRequestedOffer { get; private set; }

	public static MercenaryVillageRenownTradeDataModel GetCurrentRenownTradeData(bool updateConversionValues = false)
	{
		if (s_currentRenownTradeDataModel == null)
		{
			s_currentRenownTradeDataModel = new MercenaryVillageRenownTradeDataModel();
		}
		s_currentRenownTradeDataModel.CurrentRenownBalance = (int)ServiceManager.Get<CurrencyManager>().GetBalance(CurrencyType.RENOWN);
		if (updateConversionValues)
		{
			s_currentRenownTradeDataModel.CurrentConvertableRenownBalance = 0;
			s_currentRenownTradeDataModel.CoinConversionCount = 0;
			if (LettuceVillageDataUtil.TryGetRenownConversionRate(TAG_RARITY.RARE, out var conversionRate))
			{
				s_currentRenownTradeDataModel.RareCoinRenownConversionRate = conversionRate;
			}
			if (LettuceVillageDataUtil.TryGetRenownConversionRate(TAG_RARITY.EPIC, out conversionRate))
			{
				s_currentRenownTradeDataModel.EpicCoinRenownConversionRate = conversionRate;
			}
			if (LettuceVillageDataUtil.TryGetRenownConversionRate(TAG_RARITY.LEGENDARY, out conversionRate))
			{
				s_currentRenownTradeDataModel.LegendaryCoinRenownConversionRate = conversionRate;
			}
			CollectionManager.FindMercenariesResult mercenaries = CollectionManager.Get().FindMercenaries(null, true, null, null, null, ordered: false, null);
			if (mercenaries?.m_mercenaries != null)
			{
				foreach (LettuceMercenary mercenary in mercenaries.m_mercenaries)
				{
					if (mercenary.m_isFullyUpgraded && LettuceVillageDataUtil.TryGetRenownConversionRate(mercenary.m_rarity, out var mercConversionRate))
					{
						int num = (int)mercenary.m_currencyAmount;
						int renownToGain = Mathf.FloorToInt(num / mercConversionRate);
						int currencyConverted = num - num % mercConversionRate;
						s_currentRenownTradeDataModel.CurrentConvertableRenownBalance += renownToGain;
						s_currentRenownTradeDataModel.CoinConversionCount += currencyConverted;
					}
				}
			}
		}
		return s_currentRenownTradeDataModel;
	}

	public static void PromptPurchaseOffer(int renownOfferId, Action onCancel)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_CLAIM_OFFER_WARNING_HEADER"),
			m_text = GameStrings.Get("GLUE_LETTUCE_VILLAGE_CLAIM_OFFER_WARNING_DESCRIPTION"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				switch (response)
				{
				case AlertPopup.Response.CONFIRM:
					PurchaseOffer(renownOfferId);
					break;
				case AlertPopup.Response.CANCEL:
					onCancel();
					break;
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static void PurchaseOffer(int renownOfferId)
	{
		LastPurchaseRequestedOffer = null;
		foreach (MercenariesRenownOfferData activeRenownOffer in LettuceVillageDataUtil.ActiveRenownStates)
		{
			if (activeRenownOffer.RenownOfferId == renownOfferId)
			{
				LastPurchaseRequestedOffer = activeRenownOffer.DeepClone();
			}
		}
		if (LastPurchaseRequestedOffer == null)
		{
			Log.Lettuce.PrintError($"Cannot purchase {renownOfferId} - Unknown offer");
		}
		else
		{
			Network.Get().PurchaseRenownOffer(renownOfferId);
		}
	}

	public static void PromptDismissOffer(int renownOfferId, Action onCancel)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_DISMISS_OFFER_WARNING_HEADER"),
			m_text = GameStrings.Get("GLUE_LETTUCE_VILLAGE_DISMISS_OFFER_WARNING_DESCRIPTION"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				switch (response)
				{
				case AlertPopup.Response.CONFIRM:
					DismissOffer(renownOfferId);
					break;
				case AlertPopup.Response.CANCEL:
					onCancel();
					break;
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static void DismissOffer(int renownOfferId)
	{
		LastDismissedRequestedOffer = null;
		foreach (MercenariesRenownOfferData activeRenownOffer in LettuceVillageDataUtil.ActiveRenownStates)
		{
			if (activeRenownOffer.RenownOfferId == renownOfferId)
			{
				LastDismissedRequestedOffer = activeRenownOffer.DeepClone();
			}
		}
		if (LastDismissedRequestedOffer == null)
		{
			Log.Lettuce.PrintError($"Cannot purchase {renownOfferId} - Unknown offer");
		}
		else
		{
			Network.Get().DismissRenownOffer(renownOfferId);
		}
	}

	public static void ConvertAllExcessMercCoins()
	{
		Network.Get().ConvertExcessCoinsToRenown(new List<int>());
	}

	public static MercenariesRenownOfferData DeepClone(this MercenariesRenownOfferData cloneTarget)
	{
		return new MercenariesRenownOfferData
		{
			AddedDate = cloneTarget.AddedDate,
			CoinAmount = cloneTarget.CoinAmount,
			MercenaryId = cloneTarget.MercenaryId,
			PortraitId = cloneTarget.PortraitId,
			RenownCost = cloneTarget.RenownCost,
			RenownOfferId = cloneTarget.RenownOfferId
		};
	}

	public static RewardItemDataModel CreateRenownOfferRewardDataModel(MercenariesRenownOfferData offerData, out string description)
	{
		description = string.Empty;
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(offerData.MercenaryId, AttemptToGenerate: true);
		if (merc == null)
		{
			return null;
		}
		if (offerData.CoinAmount > 0)
		{
			description = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_COINS_REWARD", offerData.CoinAmount, merc.m_mercName);
			return new RewardItemDataModel
			{
				ItemType = RewardItemType.MERCENARY_COIN,
				Quantity = 1,
				MercenaryCoin = new LettuceMercenaryCoinDataModel
				{
					Quantity = offerData.CoinAmount,
					MercenaryId = offerData.MercenaryId
				}
			};
		}
		if (offerData.PortraitId > 0)
		{
			description = GameStrings.Format("GLUE_LETTUCE_REWARD_MERCENARY_TASK_MERCENARY_REWARD", merc.m_mercName);
			MercenaryArtVariationPremiumDbfRecord artVariationPremium = GameDbf.MercenaryArtVariationPremium.GetRecord(offerData.PortraitId);
			if (artVariationPremium == null)
			{
				return null;
			}
			return new RewardItemDataModel
			{
				ItemType = RewardItemType.MERCENARY,
				Quantity = 1,
				Mercenary = MercenaryFactory.CreateMercenaryDataModel(offerData.MercenaryId, artVariationPremium.MercenaryArtVariationId, (TAG_PREMIUM)artVariationPremium.Premium, merc),
				MercenaryCoin = new LettuceMercenaryCoinDataModel
				{
					Quantity = offerData.CoinAmount,
					MercenaryId = offerData.MercenaryId
				},
				IsMercenaryPortrait = true
			};
		}
		return null;
	}

	public static bool HasUnlockedRenownOffers()
	{
		if ((int)ServiceManager.Get<CurrencyManager>().GetBalance(CurrencyType.RENOWN) > 0)
		{
			return true;
		}
		return CollectionManager.Get().HasFullyUpgradedAnyCollectibleMercenary();
	}
}
