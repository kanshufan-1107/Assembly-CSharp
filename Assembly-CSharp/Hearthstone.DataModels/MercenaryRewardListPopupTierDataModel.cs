using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryRewardListPopupTierDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 512;

	private string m_Rating;

	private bool m_Earned;

	private bool m_IsNextTier;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "rating",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "earned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_next_tier",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 512;

	public string DataModelDisplayName => "mercenary_reward_list_popup_tier";

	public string Rating
	{
		get
		{
			return m_Rating;
		}
		set
		{
			if (!(m_Rating == value))
			{
				m_Rating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Earned
	{
		get
		{
			return m_Earned;
		}
		set
		{
			if (m_Earned != value)
			{
				m_Earned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNextTier
	{
		get
		{
			return m_IsNextTier;
		}
		set
		{
			if (m_IsNextTier != value)
			{
				m_IsNextTier = value;
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
		int num = (17 * 31 + ((m_Rating != null) ? m_Rating.GetHashCode() : 0)) * 31;
		_ = m_Earned;
		int num2 = (num + m_Earned.GetHashCode()) * 31;
		_ = m_IsNextTier;
		return num2 + m_IsNextTier.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Rating;
			return true;
		case 1:
			value = m_Earned;
			return true;
		case 2:
			value = m_IsNextTier;
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
			Rating = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Earned = value != null && (bool)value;
			return true;
		case 2:
			IsNextTier = value != null && (bool)value;
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
