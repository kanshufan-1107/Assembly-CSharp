using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RelatedCardsDetailsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 915;

	private DataModelList<CardTileDataModel> m_CardTiles = new DataModelList<CardTileDataModel>();

	private CardDataModel m_SelectedCard;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "card_tiles",
			Type = typeof(DataModelList<CardTileDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "selected_card",
			Type = typeof(CardDataModel)
		}
	};

	public int DataModelId => 915;

	public string DataModelDisplayName => "related_cards_details";

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

	public DataModelProperty[] Properties => m_properties;

	public RelatedCardsDetailsDataModel()
	{
		RegisterNestedDataModel(m_CardTiles);
		RegisterNestedDataModel(m_SelectedCard);
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
			return hash * 31 + m_SelectedCard.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 1:
			value = m_CardTiles;
			return true;
		case 2:
			value = m_SelectedCard;
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
		case 1:
			CardTiles = ((value != null) ? ((DataModelList<CardTileDataModel>)value) : null);
			return true;
		case 2:
			SelectedCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 1:
			info = Properties[0];
			return true;
		case 2:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
