using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MiniSetLayout : MonoBehaviour
{
	public Widget m_widget;

	private const string MINI_SET_LAYOUT_UPDATE_EVENT = "MINI_SET_LAYOUT_UPDATE";

	public static MiniSetDbfRecord GetDbfRecord(ProductDataModel product)
	{
		if (product.Items.Count == 0)
		{
			return null;
		}
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.MINI_SET)
			{
				return GameDbf.MiniSet.GetRecord(item.ItemId);
			}
		}
		return null;
	}

	private void Awake()
	{
		m_widget.RegisterEventListener(HandleWidgetEvent);
	}

	private void OnDestroy()
	{
		m_widget.RemoveEventListener(HandleWidgetEvent);
	}

	private void HandleWidgetEvent(string e)
	{
		if (e == "MINI_SET_LAYOUT_UPDATE")
		{
			UpdateMiniSetLayout();
		}
	}

	private void UpdateMiniSetLayout()
	{
		ProductDataModel product = m_widget.GetDataModel<ProductDataModel>();
		if (product == null || !product.Tags.Contains("mini_set"))
		{
			return;
		}
		DeckDbfRecord deck = GetDbfRecord(product).DeckRecord;
		TAG_PREMIUM premium = (product.Tags.Contains("golden") ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
		DefLoader loader = DefLoader.Get();
		RewardListDataModel rewardList = new RewardListDataModel
		{
			Items = new DataModelList<RewardItemDataModel>()
		};
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.CARD)
			{
				rewardList.Items.Add(new RewardItemDataModel
				{
					ItemType = RewardItemType.CARD,
					Card = new CardDataModel
					{
						CardId = item.Card.CardId,
						Premium = item.Card.Premium
					}
				});
			}
			else if (item.ItemType == RewardItemType.MINI_SET && deck != null)
			{
				rewardList.Items.AddRange(from c in deck.Cards
					select loader.GetEntityDef(c.CardId) into ed
					where ed.GetRarity() == TAG_RARITY.LEGENDARY
					orderby ed.GetCost()
					select new RewardItemDataModel
					{
						ItemType = RewardItemType.CARD,
						Card = new CardDataModel
						{
							CardId = ed.GetCardId(),
							Premium = premium
						}
					});
			}
		}
		rewardList.Items.Add(new RewardItemDataModel
		{
			ItemType = RewardItemType.MINI_SET
		});
		m_widget.BindDataModel(rewardList);
	}
}
