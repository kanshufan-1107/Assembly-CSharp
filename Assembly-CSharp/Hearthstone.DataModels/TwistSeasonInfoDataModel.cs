using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TwistSeasonInfoDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 816;

	private string m_RemainingSeasonTime;

	private string m_SeasonDescription;

	private string m_SeasonTitle;

	private DataModelList<ValidSetDataModel> m_TwistValidSets = new DataModelList<ValidSetDataModel>();

	private bool m_HasSeenTwistNewSeasonLabel;

	private bool m_HasSeenTwistModeGlow;

	private bool m_ShouldShowTwistLoginPopup;

	private bool m_DoesCurrentPageHaveTwistHeader;

	private bool m_IsTwistSeasonEnabled;

	private bool m_ShowFormatPickerOnSeasonEnd;

	private bool m_DoesCurrentSeasonUseHeroicDecks;

	private DataModelList<TwistHeroicDeckRowDataModel> m_TwistHeroicDeckRow = new DataModelList<TwistHeroicDeckRowDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "remaining_season_time",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "season_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "season_title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "twist_valid_sets",
			Type = typeof(DataModelList<ValidSetDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "has_seen_twist_new_season_label",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "has_seen_twist_mode_glow",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "should_show_twist_login_popup",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "does_current_page_have_twist_header",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "is_twist_season_enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "show_format_picker_on_season_end",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "does_current_season_use_heroic_decks",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "twist_heroic_deck_row",
			Type = typeof(DataModelList<TwistHeroicDeckRowDataModel>)
		}
	};

	public int DataModelId => 816;

	public string DataModelDisplayName => "twist_season_info";

	public string RemainingSeasonTime
	{
		get
		{
			return m_RemainingSeasonTime;
		}
		set
		{
			if (!(m_RemainingSeasonTime == value))
			{
				m_RemainingSeasonTime = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SeasonDescription
	{
		get
		{
			return m_SeasonDescription;
		}
		set
		{
			if (!(m_SeasonDescription == value))
			{
				m_SeasonDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SeasonTitle
	{
		get
		{
			return m_SeasonTitle;
		}
		set
		{
			if (!(m_SeasonTitle == value))
			{
				m_SeasonTitle = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ValidSetDataModel> TwistValidSets
	{
		get
		{
			return m_TwistValidSets;
		}
		set
		{
			if (m_TwistValidSets != value)
			{
				RemoveNestedDataModel(m_TwistValidSets);
				RegisterNestedDataModel(value);
				m_TwistValidSets = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasSeenTwistNewSeasonLabel
	{
		get
		{
			return m_HasSeenTwistNewSeasonLabel;
		}
		set
		{
			if (m_HasSeenTwistNewSeasonLabel != value)
			{
				m_HasSeenTwistNewSeasonLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasSeenTwistModeGlow
	{
		get
		{
			return m_HasSeenTwistModeGlow;
		}
		set
		{
			if (m_HasSeenTwistModeGlow != value)
			{
				m_HasSeenTwistModeGlow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldShowTwistLoginPopup
	{
		get
		{
			return m_ShouldShowTwistLoginPopup;
		}
		set
		{
			if (m_ShouldShowTwistLoginPopup != value)
			{
				m_ShouldShowTwistLoginPopup = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DoesCurrentPageHaveTwistHeader
	{
		get
		{
			return m_DoesCurrentPageHaveTwistHeader;
		}
		set
		{
			if (m_DoesCurrentPageHaveTwistHeader != value)
			{
				m_DoesCurrentPageHaveTwistHeader = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsTwistSeasonEnabled
	{
		get
		{
			return m_IsTwistSeasonEnabled;
		}
		set
		{
			if (m_IsTwistSeasonEnabled != value)
			{
				m_IsTwistSeasonEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowFormatPickerOnSeasonEnd
	{
		get
		{
			return m_ShowFormatPickerOnSeasonEnd;
		}
		set
		{
			if (m_ShowFormatPickerOnSeasonEnd != value)
			{
				m_ShowFormatPickerOnSeasonEnd = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DoesCurrentSeasonUseHeroicDecks
	{
		get
		{
			return m_DoesCurrentSeasonUseHeroicDecks;
		}
		set
		{
			if (m_DoesCurrentSeasonUseHeroicDecks != value)
			{
				m_DoesCurrentSeasonUseHeroicDecks = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<TwistHeroicDeckRowDataModel> TwistHeroicDeckRow
	{
		get
		{
			return m_TwistHeroicDeckRow;
		}
		set
		{
			if (m_TwistHeroicDeckRow != value)
			{
				RemoveNestedDataModel(m_TwistHeroicDeckRow);
				RegisterNestedDataModel(value);
				m_TwistHeroicDeckRow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TwistSeasonInfoDataModel()
	{
		RegisterNestedDataModel(m_TwistValidSets);
		RegisterNestedDataModel(m_TwistHeroicDeckRow);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_RemainingSeasonTime != null) ? m_RemainingSeasonTime.GetHashCode() : 0);
		hash = hash * 31 + ((m_SeasonDescription != null) ? m_SeasonDescription.GetHashCode() : 0);
		hash = hash * 31 + ((m_SeasonTitle != null) ? m_SeasonTitle.GetHashCode() : 0);
		if (m_TwistValidSets != null && !inspectedDataModels.Contains(m_TwistValidSets.GetHashCode()))
		{
			inspectedDataModels.Add(m_TwistValidSets.GetHashCode());
			hash = hash * 31 + m_TwistValidSets.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_HasSeenTwistNewSeasonLabel;
		hash = num + m_HasSeenTwistNewSeasonLabel.GetHashCode();
		int num2 = hash * 31;
		_ = m_HasSeenTwistModeGlow;
		hash = num2 + m_HasSeenTwistModeGlow.GetHashCode();
		int num3 = hash * 31;
		_ = m_ShouldShowTwistLoginPopup;
		hash = num3 + m_ShouldShowTwistLoginPopup.GetHashCode();
		int num4 = hash * 31;
		_ = m_DoesCurrentPageHaveTwistHeader;
		hash = num4 + m_DoesCurrentPageHaveTwistHeader.GetHashCode();
		int num5 = hash * 31;
		_ = m_IsTwistSeasonEnabled;
		hash = num5 + m_IsTwistSeasonEnabled.GetHashCode();
		int num6 = hash * 31;
		_ = m_ShowFormatPickerOnSeasonEnd;
		hash = num6 + m_ShowFormatPickerOnSeasonEnd.GetHashCode();
		int num7 = hash * 31;
		_ = m_DoesCurrentSeasonUseHeroicDecks;
		hash = num7 + m_DoesCurrentSeasonUseHeroicDecks.GetHashCode();
		if (m_TwistHeroicDeckRow != null && !inspectedDataModels.Contains(m_TwistHeroicDeckRow.GetHashCode()))
		{
			inspectedDataModels.Add(m_TwistHeroicDeckRow.GetHashCode());
			return hash * 31 + m_TwistHeroicDeckRow.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RemainingSeasonTime;
			return true;
		case 1:
			value = m_SeasonDescription;
			return true;
		case 2:
			value = m_SeasonTitle;
			return true;
		case 3:
			value = m_TwistValidSets;
			return true;
		case 5:
			value = m_HasSeenTwistNewSeasonLabel;
			return true;
		case 6:
			value = m_HasSeenTwistModeGlow;
			return true;
		case 7:
			value = m_ShouldShowTwistLoginPopup;
			return true;
		case 8:
			value = m_DoesCurrentPageHaveTwistHeader;
			return true;
		case 9:
			value = m_IsTwistSeasonEnabled;
			return true;
		case 10:
			value = m_ShowFormatPickerOnSeasonEnd;
			return true;
		case 11:
			value = m_DoesCurrentSeasonUseHeroicDecks;
			return true;
		case 12:
			value = m_TwistHeroicDeckRow;
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
			RemainingSeasonTime = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			SeasonDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			SeasonTitle = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			TwistValidSets = ((value != null) ? ((DataModelList<ValidSetDataModel>)value) : null);
			return true;
		case 5:
			HasSeenTwistNewSeasonLabel = value != null && (bool)value;
			return true;
		case 6:
			HasSeenTwistModeGlow = value != null && (bool)value;
			return true;
		case 7:
			ShouldShowTwistLoginPopup = value != null && (bool)value;
			return true;
		case 8:
			DoesCurrentPageHaveTwistHeader = value != null && (bool)value;
			return true;
		case 9:
			IsTwistSeasonEnabled = value != null && (bool)value;
			return true;
		case 10:
			ShowFormatPickerOnSeasonEnd = value != null && (bool)value;
			return true;
		case 11:
			DoesCurrentSeasonUseHeroicDecks = value != null && (bool)value;
			return true;
		case 12:
			TwistHeroicDeckRow = ((value != null) ? ((DataModelList<TwistHeroicDeckRowDataModel>)value) : null);
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
		case 5:
			info = Properties[4];
			return true;
		case 6:
			info = Properties[5];
			return true;
		case 7:
			info = Properties[6];
			return true;
		case 8:
			info = Properties[7];
			return true;
		case 9:
			info = Properties[8];
			return true;
		case 10:
			info = Properties[9];
			return true;
		case 11:
			info = Properties[10];
			return true;
		case 12:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
