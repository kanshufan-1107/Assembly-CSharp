using System.Collections.Generic;
using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class SpecialEventDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 743;

	private string m_Name;

	private SpecialEvent.SpecialEventType m_SpecialEventType;

	private string m_ShortDescription;

	private string m_LongDescription;

	private int m_ID;

	private DataModelList<int> m_RewardTracks = new DataModelList<int>();

	private string m_ChooseTrackPrompt;

	private string m_ShortConclusion;

	private string m_LongConclusion;

	private int m_ActiveTrackId;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "special_event_type",
			Type = typeof(SpecialEvent.SpecialEventType)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "short_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "long_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "reward_tracks",
			Type = typeof(DataModelList<int>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "choose_track_prompt",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "short_conclusion",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "long_conclusion",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "active_track_id",
			Type = typeof(int)
		}
	};

	public int DataModelId => 743;

	public string DataModelDisplayName => "special_event";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public SpecialEvent.SpecialEventType SpecialEventType
	{
		get
		{
			return m_SpecialEventType;
		}
		set
		{
			if (m_SpecialEventType != value)
			{
				m_SpecialEventType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShortDescription
	{
		get
		{
			return m_ShortDescription;
		}
		set
		{
			if (!(m_ShortDescription == value))
			{
				m_ShortDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LongDescription
	{
		get
		{
			return m_LongDescription;
		}
		set
		{
			if (!(m_LongDescription == value))
			{
				m_LongDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			if (m_ID != value)
			{
				m_ID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<int> RewardTracks
	{
		get
		{
			return m_RewardTracks;
		}
		set
		{
			if (m_RewardTracks != value)
			{
				RemoveNestedDataModel(m_RewardTracks);
				RegisterNestedDataModel(value);
				m_RewardTracks = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ChooseTrackPrompt
	{
		get
		{
			return m_ChooseTrackPrompt;
		}
		set
		{
			if (!(m_ChooseTrackPrompt == value))
			{
				m_ChooseTrackPrompt = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShortConclusion
	{
		get
		{
			return m_ShortConclusion;
		}
		set
		{
			if (!(m_ShortConclusion == value))
			{
				m_ShortConclusion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LongConclusion
	{
		get
		{
			return m_LongConclusion;
		}
		set
		{
			if (!(m_LongConclusion == value))
			{
				m_LongConclusion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ActiveTrackId
	{
		get
		{
			return m_ActiveTrackId;
		}
		set
		{
			if (m_ActiveTrackId != value)
			{
				m_ActiveTrackId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public SpecialEventDataModel()
	{
		RegisterNestedDataModel(m_RewardTracks);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_SpecialEventType;
		hash = num + m_SpecialEventType.GetHashCode();
		hash = hash * 31 + ((m_ShortDescription != null) ? m_ShortDescription.GetHashCode() : 0);
		hash = hash * 31 + ((m_LongDescription != null) ? m_LongDescription.GetHashCode() : 0);
		int num2 = hash * 31;
		_ = m_ID;
		hash = num2 + m_ID.GetHashCode();
		if (m_RewardTracks != null && !inspectedDataModels.Contains(m_RewardTracks.GetHashCode()))
		{
			inspectedDataModels.Add(m_RewardTracks.GetHashCode());
			hash = hash * 31 + m_RewardTracks.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_ChooseTrackPrompt != null) ? m_ChooseTrackPrompt.GetHashCode() : 0);
		hash = hash * 31 + ((m_ShortConclusion != null) ? m_ShortConclusion.GetHashCode() : 0);
		hash = hash * 31 + ((m_LongConclusion != null) ? m_LongConclusion.GetHashCode() : 0);
		int num3 = hash * 31;
		_ = m_ActiveTrackId;
		return num3 + m_ActiveTrackId.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_SpecialEventType;
			return true;
		case 2:
			value = m_ShortDescription;
			return true;
		case 3:
			value = m_LongDescription;
			return true;
		case 4:
			value = m_ID;
			return true;
		case 5:
			value = m_RewardTracks;
			return true;
		case 6:
			value = m_ChooseTrackPrompt;
			return true;
		case 7:
			value = m_ShortConclusion;
			return true;
		case 8:
			value = m_LongConclusion;
			return true;
		case 9:
			value = m_ActiveTrackId;
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
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			SpecialEventType = ((value != null) ? ((SpecialEvent.SpecialEventType)value) : SpecialEvent.SpecialEventType.INVALID);
			return true;
		case 2:
			ShortDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			LongDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			RewardTracks = ((value != null) ? ((DataModelList<int>)value) : null);
			return true;
		case 6:
			ChooseTrackPrompt = ((value != null) ? ((string)value) : null);
			return true;
		case 7:
			ShortConclusion = ((value != null) ? ((string)value) : null);
			return true;
		case 8:
			LongConclusion = ((value != null) ? ((string)value) : null);
			return true;
		case 9:
			ActiveTrackId = ((value != null) ? ((int)value) : 0);
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
		case 5:
			info = Properties[5];
			return true;
		case 6:
			info = Properties[6];
			return true;
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		case 9:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
