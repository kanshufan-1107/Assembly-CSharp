using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MiniSetDetailsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 266;

	private DataModelList<CardTileDataModel> m_CardTiles = new DataModelList<CardTileDataModel>();

	private CardDataModel m_SelectedCard;

	private PackDataModel m_Pack;

	private bool m_SelectedCardNotIncluded;

	private string m_NotIncludedText;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 267,
			PropertyDisplayName = "card_tiles",
			Type = typeof(DataModelList<CardTileDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 269,
			PropertyDisplayName = "selected_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 273,
			PropertyDisplayName = "pack",
			Type = typeof(PackDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 741,
			PropertyDisplayName = "selected_card_not_included",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 742,
			PropertyDisplayName = "not_included_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 266;

	public string DataModelDisplayName => "mini_set_details";

	public DataModelList<CardTileDataModel> CardTiles
	{
		get
		{
			return m_CardTiles;
		}
		set
		{
			if (m_CardTiles != value)
			{
				RemoveNestedDataModel(m_CardTiles);
				RegisterNestedDataModel(value);
				m_CardTiles = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel SelectedCard
	{
		get
		{
			return m_SelectedCard;
		}
		set
		{
			if (m_SelectedCard != value)
			{
				RemoveNestedDataModel(m_SelectedCard);
				RegisterNestedDataModel(value);
				m_SelectedCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PackDataModel Pack
	{
		get
		{
			return m_Pack;
		}
		set
		{
			if (m_Pack != value)
			{
				RemoveNestedDataModel(m_Pack);
				RegisterNestedDataModel(value);
				m_Pack = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool SelectedCardNotIncluded
	{
		get
		{
			return m_SelectedCardNotIncluded;
		}
		set
		{
			if (m_SelectedCardNotIncluded != value)
			{
				m_SelectedCardNotIncluded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string NotIncludedText
	{
		get
		{
			return m_NotIncludedText;
		}
		set
		{
			if (!(m_NotIncludedText == value))
			{
				m_NotIncludedText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MiniSetDetailsDataModel()
	{
		RegisterNestedDataModel(m_CardTiles);
		RegisterNestedDataModel(m_SelectedCard);
		RegisterNestedDataModel(m_Pack);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_CardTiles != null && !inspectedDataModels.Contains(m_CardTiles.GetHashCode()))
		{
			inspectedDataModels.Add(m_CardTiles.GetHashCode());
			hash = hash * 31 + m_CardTiles.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_SelectedCard != null && !inspectedDataModels.Contains(m_SelectedCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_SelectedCard.GetHashCode());
			hash = hash * 31 + m_SelectedCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Pack != null && !inspectedDataModels.Contains(m_Pack.GetHashCode()))
		{
			inspectedDataModels.Add(m_Pack.GetHashCode());
			hash = hash * 31 + m_Pack.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_SelectedCardNotIncluded;
		hash = num + m_SelectedCardNotIncluded.GetHashCode();
		return hash * 31 + ((m_NotIncludedText != null) ? m_NotIncludedText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 267:
			value = m_CardTiles;
			return true;
		case 269:
			value = m_SelectedCard;
			return true;
		case 273:
			value = m_Pack;
			return true;
		case 741:
			value = m_SelectedCardNotIncluded;
			return true;
		case 742:
			value = m_NotIncludedText;
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
		case 267:
			CardTiles = ((value != null) ? ((DataModelList<CardTileDataModel>)value) : null);
			return true;
		case 269:
			SelectedCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 273:
			Pack = ((value != null) ? ((PackDataModel)value) : null);
			return true;
		case 741:
			SelectedCardNotIncluded = value != null && (bool)value;
			return true;
		case 742:
			NotIncludedText = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 267:
			info = Properties[0];
			return true;
		case 269:
			info = Properties[1];
			return true;
		case 273:
			info = Properties[2];
			return true;
		case 741:
			info = Properties[3];
			return true;
		case 742:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
