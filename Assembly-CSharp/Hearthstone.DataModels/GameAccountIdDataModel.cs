using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class GameAccountIdDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 939;

	private ulong m_High;

	private ulong m_Low;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "high",
			Type = typeof(ulong)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "low",
			Type = typeof(ulong)
		}
	};

	public int DataModelId => 939;

	public string DataModelDisplayName => "game_account_id";

	public ulong High
	{
		get
		{
			return m_High;
		}
		set
		{
			if (m_High != value)
			{
				m_High = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ulong Low
	{
		get
		{
			return m_Low;
		}
		set
		{
			if (m_Low != value)
			{
				m_Low = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_High;
		int num2 = (num + m_High.GetHashCode()) * 31;
		_ = m_Low;
		return num2 + m_Low.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_High;
			return true;
		case 1:
			value = m_Low;
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
			High = ((value != null) ? ((ulong)value) : 0);
			return true;
		case 1:
			Low = ((value != null) ? ((ulong)value) : 0);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
