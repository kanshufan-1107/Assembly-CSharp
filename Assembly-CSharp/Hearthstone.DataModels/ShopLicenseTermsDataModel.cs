using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopLicenseTermsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 1142;

	private bool m_Visible;

	private OSCategory m_Platform;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "visible",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "platform",
			Type = typeof(OSCategory)
		}
	};

	public int DataModelId => 1142;

	public string DataModelDisplayName => "shop_license_terms";

	public bool Visible
	{
		get
		{
			return m_Visible;
		}
		set
		{
			if (m_Visible != value)
			{
				m_Visible = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public OSCategory Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			if (m_Platform != value)
			{
				m_Platform = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = 17 * 31;
		_ = m_Visible;
		int num2 = (num + m_Visible.GetHashCode()) * 31;
		_ = m_Platform;
		return num2 + m_Platform.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Visible;
			return true;
		case 1:
			value = m_Platform;
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
			Visible = value != null && (bool)value;
			return true;
		case 1:
			Platform = ((value != null) ? ((OSCategory)value) : ((OSCategory)0));
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
