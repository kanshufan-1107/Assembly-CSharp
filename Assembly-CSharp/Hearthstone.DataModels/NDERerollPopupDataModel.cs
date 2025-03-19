using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class NDERerollPopupDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 760;

	private DataModelList<CardDataModel> m_RerollCards = new DataModelList<CardDataModel>();

	private DataModelList<RandomCardDataModel> m_RandomCards = new DataModelList<RandomCardDataModel>();

	private int m_Quantity;

	private string m_HeaderText;

	private string m_BodyText;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "reroll_cards",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "random_cards",
			Type = typeof(DataModelList<RandomCardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "quantity",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "header_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "body_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 760;

	public string DataModelDisplayName => "nde_reroll_popup";

	public DataModelList<CardDataModel> RerollCards
	{
		get
		{
			return m_RerollCards;
		}
		set
		{
			if (m_RerollCards != value)
			{
				RemoveNestedDataModel(m_RerollCards);
				RegisterNestedDataModel(value);
				m_RerollCards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<RandomCardDataModel> RandomCards
	{
		get
		{
			return m_RandomCards;
		}
		set
		{
			if (m_RandomCards != value)
			{
				RemoveNestedDataModel(m_RandomCards);
				RegisterNestedDataModel(value);
				m_RandomCards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Quantity
	{
		get
		{
			return m_Quantity;
		}
		set
		{
			if (m_Quantity != value)
			{
				m_Quantity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string HeaderText
	{
		get
		{
			return m_HeaderText;
		}
		set
		{
			if (!(m_HeaderText == value))
			{
				m_HeaderText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BodyText
	{
		get
		{
			return m_BodyText;
		}
		set
		{
			if (!(m_BodyText == value))
			{
				m_BodyText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public NDERerollPopupDataModel()
	{
		RegisterNestedDataModel(m_RerollCards);
		RegisterNestedDataModel(m_RandomCards);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_RerollCards != null) ? m_RerollCards.GetPropertiesHashCode() : 0)) * 31 + ((m_RandomCards != null) ? m_RandomCards.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Quantity;
		return ((num + m_Quantity.GetHashCode()) * 31 + ((m_HeaderText != null) ? m_HeaderText.GetHashCode() : 0)) * 31 + ((m_BodyText != null) ? m_BodyText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RerollCards;
			return true;
		case 1:
			value = m_RandomCards;
			return true;
		case 2:
			value = m_Quantity;
			return true;
		case 3:
			value = m_HeaderText;
			return true;
		case 4:
			value = m_BodyText;
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
			RerollCards = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 1:
			RandomCards = ((value != null) ? ((DataModelList<RandomCardDataModel>)value) : null);
			return true;
		case 2:
			Quantity = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			HeaderText = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			BodyText = ((value != null) ? ((string)value) : null);
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
		case 4:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
