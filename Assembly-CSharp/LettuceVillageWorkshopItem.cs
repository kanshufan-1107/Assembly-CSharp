using System;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LettuceVillageWorkshopItem : MonoBehaviour
{
	[Serializable]
	public class TierToSpriteReference
	{
		public int TierID;

		public Sprite Icon;

		public Sprite NextTierNotAvailableIcon;
	}

	[Serializable]
	public class BuildingIconsDef
	{
		public MercenaryBuilding.Mercenarybuildingtype BuildingType;

		public TierToSpriteReference[] Icons;
	}

	[Tooltip("The sprite renderer that draws the building icon")]
	public SpriteRenderer BuildingSprite;

	[Tooltip("Used to configure which icon is shown for each building / tier level")]
	public BuildingIconsDef[] BuildingIcons;

	private Widget m_widget;

	private static readonly MercenaryVillageWorkshopItemDataModel s_prewarmModel = new MercenaryVillageWorkshopItemDataModel
	{
		Prewarm = true,
		Price = new PriceDataModel
		{
			Amount = 1f,
			Currency = CurrencyType.GOLD,
			DisplayText = "1"
		}
	};

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		m_widget.BindDataModel(s_prewarmModel);
		m_widget.RegisterEventListener(OnEvent);
	}

	private void OnEvent(string eventName)
	{
		if (string.Equals(eventName, "SET_ICON", StringComparison.Ordinal))
		{
			MercenaryVillageWorkshopItemDataModel model = m_widget.GetDataModel<MercenaryVillageWorkshopItemDataModel>();
			SetBuildingSprite(model.BuildingType, model.CurrentTierId);
		}
	}

	private void SetBuildingSprite(MercenaryBuilding.Mercenarybuildingtype type, int tierId)
	{
		TierToSpriteReference[] iconList = null;
		BuildingIconsDef[] buildingIcons = BuildingIcons;
		foreach (BuildingIconsDef bldg in buildingIcons)
		{
			if (bldg.BuildingType == type)
			{
				iconList = bldg.Icons;
				break;
			}
		}
		TierToSpriteReference icon = null;
		if (iconList != null && iconList.Length != 0)
		{
			TierToSpriteReference[] array = iconList;
			foreach (TierToSpriteReference i2 in array)
			{
				if (i2.TierID == tierId)
				{
					icon = i2;
					break;
				}
			}
		}
		if (icon != null)
		{
			if (icon.NextTierNotAvailableIcon != null && LettuceVillageDataUtil.GetNextTierRecord(LettuceVillageDataUtil.GetBuildingRecordByType(type).MercenaryBuildingTiers.Find((BuildingTierDbfRecord tier) => tier.ID == tierId)) == null)
			{
				BuildingSprite.sprite = icon.NextTierNotAvailableIcon;
			}
			else
			{
				BuildingSprite.sprite = icon.Icon;
			}
		}
	}
}
