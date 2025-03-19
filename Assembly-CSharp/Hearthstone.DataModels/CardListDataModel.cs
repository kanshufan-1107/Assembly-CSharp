using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CardListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 205;

	private DataModelList<CardDataModel> m_Cards = new DataModelList<CardDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 206,
			PropertyDisplayName = "cards",
			Type = typeof(DataModelList<CardDataModel>)
		}
	};

	public int DataModelId => 205;

	public string DataModelDisplayName => "card_list";

	public DataModelList<CardDataModel> Cards
	{
		get
		{
			return m_Cards;
		}
		set
		{
			if (m_Cards != value)
			{
				RemoveNestedDataModel(m_Cards);
				RegisterNestedDataModel(value);
				m_Cards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public CardListDataModel()
	{
		RegisterNestedDataModel(m_Cards);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Cards != null && !inspectedDataModels.Contains(m_Cards.GetHashCode()))
		{
			inspectedDataModels.Add(m_Cards.GetHashCode());
			return hash * 31 + m_Cards.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 206)
		{
			value = m_Cards;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 206)
		{
			Cards = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 206)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
