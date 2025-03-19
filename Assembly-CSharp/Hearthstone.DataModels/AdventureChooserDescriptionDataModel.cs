using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AdventureChooserDescriptionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 202;

	private CardListDataModel m_Heroes;

	private bool m_HasNewHero;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 203,
			PropertyDisplayName = "heroes",
			Type = typeof(CardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 204,
			PropertyDisplayName = "has_new_hero",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 202;

	public string DataModelDisplayName => "adventure_chooser_description";

	public CardListDataModel Heroes
	{
		get
		{
			return m_Heroes;
		}
		set
		{
			if (m_Heroes != value)
			{
				RemoveNestedDataModel(m_Heroes);
				RegisterNestedDataModel(value);
				m_Heroes = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewHero
	{
		get
		{
			return m_HasNewHero;
		}
		set
		{
			if (m_HasNewHero != value)
			{
				m_HasNewHero = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AdventureChooserDescriptionDataModel()
	{
		RegisterNestedDataModel(m_Heroes);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Heroes != null && !inspectedDataModels.Contains(m_Heroes.GetHashCode()))
		{
			inspectedDataModels.Add(m_Heroes.GetHashCode());
			hash = hash * 31 + m_Heroes.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_HasNewHero;
		return num + m_HasNewHero.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 203:
			value = m_Heroes;
			return true;
		case 204:
			value = m_HasNewHero;
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
		case 203:
			Heroes = ((value != null) ? ((CardListDataModel)value) : null);
			return true;
		case 204:
			HasNewHero = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 203:
			info = Properties[0];
			return true;
		case 204:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
