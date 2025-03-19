using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;

public static class PremiumHeroSkinUtil
{
	public static RewardListDataModel GenerateMultiHeroSkinRewardList(ProductDataModel product)
	{
		bool hasMultiClasses;
		HashSet<string> uniqueHeroSkinIds = GetUniqueHeroSkins(GetHeroSkinIds(product), out hasMultiClasses);
		RewardListDataModel rewardList = new RewardListDataModel();
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.Card == null || uniqueHeroSkinIds.Contains(item.Card.CardId))
			{
				if (item.ItemType == RewardItemType.HERO_SKIN)
				{
					item.HeroClassCount = GetMulticlassList(item.Card.CardId).Count;
				}
				if (IsMythicProduct(item))
				{
					product.Tags.Add("mythic");
				}
				rewardList.Items.Add(item);
			}
		}
		if (hasMultiClasses && !product.Tags.Contains("multiclass"))
		{
			product.Tags.Add("multiclass");
		}
		return rewardList;
	}

	public static bool ContainsMultipleHeros(Widget widget)
	{
		if (widget == null)
		{
			return false;
		}
		widget.GetDataModel(15, out var model);
		if (model is ProductDataModel { RewardList: not null } product)
		{
			int count = 0;
			foreach (RewardItemDataModel item in product.RewardList.Items)
			{
				if (item.ItemType == RewardItemType.HERO_SKIN)
				{
					count++;
				}
			}
			return count > 1;
		}
		return false;
	}

	public static List<TAG_CLASS> GetMulticlassList(string heroSkinId)
	{
		HashSet<TAG_CLASS> heroClasses = new HashSet<TAG_CLASS> { GetClassFromCardId(heroSkinId) };
		foreach (CounterpartCardsDbfRecord counterpartCard in GameUtils.GetCounterpartCards(heroSkinId))
		{
			string counterpartCardId = GameUtils.TranslateDbIdToCardId(counterpartCard.DeckEquivalentCardId);
			heroClasses.Add(GetClassFromCardId(counterpartCardId));
		}
		return heroClasses.ToList();
	}

	public static bool IsMythicProduct(RewardItemDataModel heroSkinItem)
	{
		CardDbfRecord heroSkinCardRec = GameDbf.Card.GetRecord(heroSkinItem.ItemId);
		if (heroSkinCardRec == null)
		{
			return false;
		}
		if (GameUtils.TryGetCardTagRecords(heroSkinCardRec.NoteMiniGuid, out var cardTags))
		{
			foreach (CardTagDbfRecord item in cardTags)
			{
				if (item.TagId == 3564)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static List<string> GetHeroSkinIds(ProductDataModel product)
	{
		List<string> heroSkinIds = new List<string>();
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.HERO_SKIN)
			{
				heroSkinIds.Add(item.Card.CardId);
			}
		}
		return heroSkinIds;
	}

	private static HashSet<string> GetUniqueHeroSkins(List<string> heroSkinIds, out bool hasMultiClasses)
	{
		HashSet<string> uniqueHeroSkinIds = new HashSet<string>();
		hasMultiClasses = false;
		foreach (string id in heroSkinIds)
		{
			List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(id);
			bool counterpartExists = false;
			foreach (CounterpartCardsDbfRecord item in counterpartCards)
			{
				string counterpartCardId = GameUtils.TranslateDbIdToCardId(item.DeckEquivalentCardId);
				if (uniqueHeroSkinIds.Contains(counterpartCardId))
				{
					counterpartExists = true;
					break;
				}
			}
			if (!uniqueHeroSkinIds.Contains(id) && !counterpartExists)
			{
				uniqueHeroSkinIds.Add(id);
			}
			else
			{
				hasMultiClasses = true;
			}
		}
		return uniqueHeroSkinIds;
	}

	private static TAG_CLASS GetClassFromCardId(string cardId)
	{
		return DefLoader.Get().GetEntityDef(cardId)?.GetClass() ?? TAG_CLASS.INVALID;
	}
}
