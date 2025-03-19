using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ClassCardCountDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 837;

	private string m_ClassName;

	private TAG_PREMIUM m_HighestPremium;

	private int m_CardCount;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "class_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "highest_premium",
			Type = typeof(TAG_PREMIUM)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "card_count",
			Type = typeof(int)
		}
	};

	public int DataModelId => 837;

	public string DataModelDisplayName => "class_card_count";

	public string ClassName
	{
		get
		{
			return m_ClassName;
		}
		set
		{
			if (!(m_ClassName == value))
			{
				m_ClassName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_PREMIUM HighestPremium
	{
		get
		{
			return m_HighestPremium;
		}
		set
		{
			if (m_HighestPremium != value)
			{
				m_HighestPremium = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CardCount
	{
		get
		{
			return m_CardCount;
		}
		set
		{
			if (m_CardCount != value)
			{
				m_CardCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_ClassName != null) ? m_ClassName.GetHashCode() : 0)) * 31;
		_ = m_HighestPremium;
		int num2 = (num + m_HighestPremium.GetHashCode()) * 31;
		_ = m_CardCount;
		return num2 + m_CardCount.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ClassName;
			return true;
		case 1:
			value = m_HighestPremium;
			return true;
		case 2:
			value = m_CardCount;
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
			ClassName = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			HighestPremium = ((value != null) ? ((TAG_PREMIUM)value) : TAG_PREMIUM.NORMAL);
			return true;
		case 2:
			CardCount = ((value != null) ? ((int)value) : 0);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
