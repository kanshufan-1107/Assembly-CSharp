using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ItemPreviewOptionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 921;

	private RewardItemDataModel m_PreviewItem;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 922,
			PropertyDisplayName = "preview_item",
			Type = typeof(RewardItemDataModel)
		}
	};

	public int DataModelId => 921;

	public string DataModelDisplayName => "item_preview_option";

	public RewardItemDataModel PreviewItem
	{
		get
		{
			return m_PreviewItem;
		}
		set
		{
			if (m_PreviewItem != value)
			{
				RemoveNestedDataModel(m_PreviewItem);
				RegisterNestedDataModel(value);
				m_PreviewItem = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ItemPreviewOptionDataModel()
	{
		RegisterNestedDataModel(m_PreviewItem);
	}

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_PreviewItem != null) ? m_PreviewItem.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 922)
		{
			value = m_PreviewItem;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 922)
		{
			PreviewItem = ((value != null) ? ((RewardItemDataModel)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 922)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
