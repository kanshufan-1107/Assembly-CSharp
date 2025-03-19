using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MiniSetProductPage : ProductPage
{
	public UIBScrollable m_scrollbar;

	private ShopCardList m_cardList;

	private Maskable m_maskable;

	public override void Open()
	{
		m_maskable = GetComponentInChildren<Maskable>();
		m_maskable.enabled = false;
		if (m_container != null)
		{
			m_container.OverrideMusic(MusicPlaylistType.Invalid);
		}
		m_cardList = new ShopCardList(m_widget, m_scrollbar);
		base.Open();
		base.OnOpened += InitInput;
	}

	public override void Close()
	{
		base.Close();
		m_cardList.RemoveListeners();
	}

	public void InitInput(object sender, EventArgs e)
	{
		base.OnOpened -= InitInput;
		m_cardList.InitInput();
		m_maskable.enabled = true;
	}

	protected override ProductDataModel GetFirstVariantToDisplay(ProductDataModel chosenProduct, ProductDataModel chosenVariant)
	{
		bool isChosenGolden = chosenProduct.Tags.Contains("golden");
		ProductDataModel newChosenVariant = chosenVariant;
		foreach (ProductDataModel variant in chosenProduct.Variants)
		{
			bool isGoldenVariant = variant.Tags.Contains("golden");
			variant.VariantName = (isGoldenVariant ? GameStrings.Get("GLUE_STORE_PREMIUM_VARIATION_NAME_GOLDEN") : GameStrings.Get("GLUE_STORE_PREMIUM_VARIATION_NAME_NORMAL"));
			if (isGoldenVariant == isChosenGolden)
			{
				newChosenVariant = variant;
			}
		}
		return newChosenVariant;
	}

	protected override void OnProductSet()
	{
		base.OnProductSet();
		foreach (RewardItemDataModel item in base.Product.Items)
		{
			if (item.ItemType == RewardItemType.CARD)
			{
				continue;
			}
			int rewardId = item.ItemId;
			using AssetHandle<GameObject> storePackDefPrefab = ShopUtils.LoadStorePackPrefab((BoosterDbId)GameDbf.MiniSet.GetRecord(rewardId).BoosterRecord.ID);
			if (m_container != null)
			{
				m_container.OverrideMusic(storePackDefPrefab.Asset.GetComponent<StorePackDef>().GetMiniSetPlaylist());
			}
			break;
		}
	}

	public override void SelectVariant(ProductDataModel product)
	{
		base.SelectVariant(product);
		List<CardTileDataModel> cards = new List<CardTileDataModel>();
		TAG_PREMIUM premium = (product.Tags.Contains("golden") ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
		int itemCount = 0;
		BoosterDbId boosterId = BoosterDbId.INVALID;
		foreach (RewardItemDataModel item in product.Items)
		{
			int rewardId = item.ItemId;
			if (item.ItemType == RewardItemType.CARD)
			{
				cards.Add(new CardTileDataModel
				{
					CardId = item.Card.CardId,
					Count = 1,
					Premium = item.Card.Premium
				});
				itemCount++;
				continue;
			}
			MiniSetDbfRecord record = GameDbf.MiniSet.GetRecord(rewardId);
			DeckDbfRecord deck = record.DeckRecord;
			boosterId = (BoosterDbId)record.BoosterRecord.ID;
			DefLoader loader = DefLoader.Get();
			foreach (CardTileDataModel card in from cr in deck.Cards
				group cr by cr.CardId into g
				select (loader.GetEntityDef(g.Key), g.Count()) into ed
				orderby ed.Item1.GetRarity() descending, ed.Item1.GetCost()
				select new CardTileDataModel
				{
					CardId = ed.Item1.GetCardId(),
					Count = ed.Item2,
					Premium = premium
				})
			{
				cards.Add(card);
				itemCount += card.Count;
			}
		}
		product.FlavorText = GameStrings.FormatPlurals("GLUE_STORE_MINI_SET_CARD_COUNT", GameStrings.MakePlurals(itemCount), itemCount);
		m_cardList.SetData(cards, boosterId);
	}
}
