using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceAbilityTierDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 248;

	private int m_Tier;

	private string m_AbilityName;

	private string m_CoinCraftCost;

	private CardDataModel m_AbilityTierCard;

	private int m_ParentAbilityId;

	private bool m_ValidTier;

	private int m_TierId;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "tier",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "ability_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "coin_craft_cost",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "ability_tier_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "parent_ability_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "valid_tier",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "tier_id",
			Type = typeof(int)
		}
	};

	public int DataModelId => 248;

	public string DataModelDisplayName => "lettuce_ability_tier";

	public int Tier
	{
		get
		{
			return m_Tier;
		}
		set
		{
			if (m_Tier != value)
			{
				m_Tier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AbilityName
	{
		get
		{
			return m_AbilityName;
		}
		set
		{
			if (!(m_AbilityName == value))
			{
				m_AbilityName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string CoinCraftCost
	{
		get
		{
			return m_CoinCraftCost;
		}
		set
		{
			if (!(m_CoinCraftCost == value))
			{
				m_CoinCraftCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel AbilityTierCard
	{
		get
		{
			return m_AbilityTierCard;
		}
		set
		{
			if (m_AbilityTierCard != value)
			{
				RemoveNestedDataModel(m_AbilityTierCard);
				RegisterNestedDataModel(value);
				m_AbilityTierCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ParentAbilityId
	{
		get
		{
			return m_ParentAbilityId;
		}
		set
		{
			if (m_ParentAbilityId != value)
			{
				m_ParentAbilityId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ValidTier
	{
		get
		{
			return m_ValidTier;
		}
		set
		{
			if (m_ValidTier != value)
			{
				m_ValidTier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TierId
	{
		get
		{
			return m_TierId;
		}
		set
		{
			if (m_TierId != value)
			{
				m_TierId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceAbilityTierDataModel()
	{
		RegisterNestedDataModel(m_AbilityTierCard);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Tier;
		hash = num + m_Tier.GetHashCode();
		hash = hash * 31 + ((m_AbilityName != null) ? m_AbilityName.GetHashCode() : 0);
		hash = hash * 31 + ((m_CoinCraftCost != null) ? m_CoinCraftCost.GetHashCode() : 0);
		if (m_AbilityTierCard != null && !inspectedDataModels.Contains(m_AbilityTierCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_AbilityTierCard.GetHashCode());
			hash = hash * 31 + m_AbilityTierCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_ParentAbilityId;
		hash = num2 + m_ParentAbilityId.GetHashCode();
		int num3 = hash * 31;
		_ = m_ValidTier;
		hash = num3 + m_ValidTier.GetHashCode();
		int num4 = hash * 31;
		_ = m_TierId;
		return num4 + m_TierId.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Tier;
			return true;
		case 1:
			value = m_AbilityName;
			return true;
		case 2:
			value = m_CoinCraftCost;
			return true;
		case 3:
			value = m_AbilityTierCard;
			return true;
		case 6:
			value = m_ParentAbilityId;
			return true;
		case 8:
			value = m_ValidTier;
			return true;
		case 9:
			value = m_TierId;
			return true;
		default:
			value = null;
			return false;
		}
	}

	public bool SetPropertyValue(int id, object value)
	{
		switch (id)
		{
		case 0:
			Tier = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			AbilityName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			CoinCraftCost = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			AbilityTierCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 6:
			ParentAbilityId = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			ValidTier = value != null && (bool)value;
			return true;
		case 9:
			TierId = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 0:
			info = Properties[0];
			return true;
		case 1:
			info = Properties[1];
			return true;
		case 2:
			info = Properties[2];
			return true;
		case 3:
			info = Properties[3];
			return true;
		case 6:
			info = Properties[4];
			return true;
		case 8:
			info = Properties[5];
			return true;
		case 9:
			info = Properties[6];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
