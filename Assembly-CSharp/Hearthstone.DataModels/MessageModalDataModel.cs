using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MessageModalDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 298;

	private string m_LayoutType;

	private MessagePageArrowDataModel m_PageArrowPrevious;

	private MessagePageArrowDataModel m_PageArrowNext;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "layout_type",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "page_arrow_previous",
			Type = typeof(MessagePageArrowDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "page_arrow_next",
			Type = typeof(MessagePageArrowDataModel)
		}
	};

	public int DataModelId => 298;

	public string DataModelDisplayName => "message_modal";

	public string LayoutType
	{
		get
		{
			return m_LayoutType;
		}
		set
		{
			if (!(m_LayoutType == value))
			{
				m_LayoutType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MessagePageArrowDataModel PageArrowPrevious
	{
		get
		{
			return m_PageArrowPrevious;
		}
		set
		{
			if (m_PageArrowPrevious != value)
			{
				RemoveNestedDataModel(m_PageArrowPrevious);
				RegisterNestedDataModel(value);
				m_PageArrowPrevious = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MessagePageArrowDataModel PageArrowNext
	{
		get
		{
			return m_PageArrowNext;
		}
		set
		{
			if (m_PageArrowNext != value)
			{
				RemoveNestedDataModel(m_PageArrowNext);
				RegisterNestedDataModel(value);
				m_PageArrowNext = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MessageModalDataModel()
	{
		RegisterNestedDataModel(m_PageArrowPrevious);
		RegisterNestedDataModel(m_PageArrowNext);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_LayoutType != null) ? m_LayoutType.GetHashCode() : 0);
		if (m_PageArrowPrevious != null && !inspectedDataModels.Contains(m_PageArrowPrevious.GetHashCode()))
		{
			inspectedDataModels.Add(m_PageArrowPrevious.GetHashCode());
			hash = hash * 31 + m_PageArrowPrevious.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_PageArrowNext != null && !inspectedDataModels.Contains(m_PageArrowNext.GetHashCode()))
		{
			inspectedDataModels.Add(m_PageArrowNext.GetHashCode());
			return hash * 31 + m_PageArrowNext.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_LayoutType;
			return true;
		case 1:
			value = m_PageArrowPrevious;
			return true;
		case 2:
			value = m_PageArrowNext;
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
			LayoutType = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			PageArrowPrevious = ((value != null) ? ((MessagePageArrowDataModel)value) : null);
			return true;
		case 2:
			PageArrowNext = ((value != null) ? ((MessagePageArrowDataModel)value) : null);
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
