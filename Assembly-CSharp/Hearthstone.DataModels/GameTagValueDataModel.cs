using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class GameTagValueDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 573;

	private GAME_TAG m_GameTag;

	private int m_Value;

	private bool m_IsReferenceValue;

	private bool m_IsPowerKeywordTag;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "game_tag",
			Type = typeof(GAME_TAG)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "value",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_reference_value",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_power_keyword_tag",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 573;

	public string DataModelDisplayName => "game_tag_value";

	public GAME_TAG GameTag
	{
		get
		{
			return m_GameTag;
		}
		set
		{
			if (m_GameTag != value)
			{
				m_GameTag = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (m_Value != value)
			{
				m_Value = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsReferenceValue
	{
		get
		{
			return m_IsReferenceValue;
		}
		set
		{
			if (m_IsReferenceValue != value)
			{
				m_IsReferenceValue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsPowerKeywordTag
	{
		get
		{
			return m_IsPowerKeywordTag;
		}
		set
		{
			if (m_IsPowerKeywordTag != value)
			{
				m_IsPowerKeywordTag = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_GameTag;
		int num2 = (num + m_GameTag.GetHashCode()) * 31;
		_ = m_Value;
		int num3 = (num2 + m_Value.GetHashCode()) * 31;
		_ = m_IsReferenceValue;
		int num4 = (num3 + m_IsReferenceValue.GetHashCode()) * 31;
		_ = m_IsPowerKeywordTag;
		return num4 + m_IsPowerKeywordTag.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_GameTag;
			return true;
		case 1:
			value = m_Value;
			return true;
		case 2:
			value = m_IsReferenceValue;
			return true;
		case 3:
			value = m_IsPowerKeywordTag;
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
			GameTag = ((value != null) ? ((GAME_TAG)value) : GAME_TAG.TAG_NOT_SET);
			return true;
		case 1:
			Value = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			IsReferenceValue = value != null && (bool)value;
			return true;
		case 3:
			IsPowerKeywordTag = value != null && (bool)value;
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
