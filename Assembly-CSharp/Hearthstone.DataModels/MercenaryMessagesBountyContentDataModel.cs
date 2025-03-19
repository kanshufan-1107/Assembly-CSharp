using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryMessagesBountyContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 525;

	private string m_Title;

	private string m_BodyText;

	private DataModelList<LettuceBountyDataModel> m_Bounties = new DataModelList<LettuceBountyDataModel>();

	private string m_Url;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "body_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "mercenary_message_bounty_item_display",
			Type = typeof(DataModelList<LettuceBountyDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "url",
			Type = typeof(string)
		}
	};

	public int DataModelId => 525;

	public string DataModelDisplayName => "mercenary_message_bounty_content";

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
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

	public DataModelList<LettuceBountyDataModel> Bounties
	{
		get
		{
			return m_Bounties;
		}
		set
		{
			if (m_Bounties != value)
			{
				RemoveNestedDataModel(m_Bounties);
				RegisterNestedDataModel(value);
				m_Bounties = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Url
	{
		get
		{
			return m_Url;
		}
		set
		{
			if (!(m_Url == value))
			{
				m_Url = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryMessagesBountyContentDataModel()
	{
		RegisterNestedDataModel(m_Bounties);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0);
		hash = hash * 31 + ((m_BodyText != null) ? m_BodyText.GetHashCode() : 0);
		if (m_Bounties != null && !inspectedDataModels.Contains(m_Bounties.GetHashCode()))
		{
			inspectedDataModels.Add(m_Bounties.GetHashCode());
			hash = hash * 31 + m_Bounties.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		return hash * 31 + ((m_Url != null) ? m_Url.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Title;
			return true;
		case 1:
			value = m_BodyText;
			return true;
		case 2:
			value = m_Bounties;
			return true;
		case 3:
			value = m_Url;
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
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			BodyText = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Bounties = ((value != null) ? ((DataModelList<LettuceBountyDataModel>)value) : null);
			return true;
		case 3:
			Url = ((value != null) ? ((string)value) : null);
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
