using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 305;

	private bool m_Enabled;

	private bool m_CollectionHasUpdates;

	private bool m_ZoneSelectionHasUpdates;

	private bool m_ArenaHasUpdates;

	private bool m_ShopHasUpdates;

	private bool m_TrainingHallHasUpdates;

	private int m_NumMailboxUpdates;

	private bool m_WorkshopHasUpdates;

	private bool m_TaskboardHasUpdates;

	private int m_NumPacksAvailable;

	private bool m_ShouldAnimatePackButtonIn;

	private bool m_ShouldAnimatePackButtonOut;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 306,
			PropertyDisplayName = "villages_enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 550,
			PropertyDisplayName = "collection_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 551,
			PropertyDisplayName = "zone_selection_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 552,
			PropertyDisplayName = "arena_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 553,
			PropertyDisplayName = "shop_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 554,
			PropertyDisplayName = "training_hall_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 555,
			PropertyDisplayName = "num_mailbox_updates",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 556,
			PropertyDisplayName = "workshop_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 557,
			PropertyDisplayName = "taskboard_has_updates",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 558,
			PropertyDisplayName = "num_packs_available",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 568,
			PropertyDisplayName = "should_animate_pack_button_in",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 569,
			PropertyDisplayName = "should_animate_pack_button_out",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 305;

	public string DataModelDisplayName => "mercenary_village";

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			if (m_Enabled != value)
			{
				m_Enabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CollectionHasUpdates
	{
		get
		{
			return m_CollectionHasUpdates;
		}
		set
		{
			if (m_CollectionHasUpdates != value)
			{
				m_CollectionHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ZoneSelectionHasUpdates
	{
		get
		{
			return m_ZoneSelectionHasUpdates;
		}
		set
		{
			if (m_ZoneSelectionHasUpdates != value)
			{
				m_ZoneSelectionHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ArenaHasUpdates
	{
		get
		{
			return m_ArenaHasUpdates;
		}
		set
		{
			if (m_ArenaHasUpdates != value)
			{
				m_ArenaHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShopHasUpdates
	{
		get
		{
			return m_ShopHasUpdates;
		}
		set
		{
			if (m_ShopHasUpdates != value)
			{
				m_ShopHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TrainingHallHasUpdates
	{
		get
		{
			return m_TrainingHallHasUpdates;
		}
		set
		{
			if (m_TrainingHallHasUpdates != value)
			{
				m_TrainingHallHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumMailboxUpdates
	{
		get
		{
			return m_NumMailboxUpdates;
		}
		set
		{
			if (m_NumMailboxUpdates != value)
			{
				m_NumMailboxUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool WorkshopHasUpdates
	{
		get
		{
			return m_WorkshopHasUpdates;
		}
		set
		{
			if (m_WorkshopHasUpdates != value)
			{
				m_WorkshopHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TaskboardHasUpdates
	{
		get
		{
			return m_TaskboardHasUpdates;
		}
		set
		{
			if (m_TaskboardHasUpdates != value)
			{
				m_TaskboardHasUpdates = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumPacksAvailable
	{
		get
		{
			return m_NumPacksAvailable;
		}
		set
		{
			if (m_NumPacksAvailable != value)
			{
				m_NumPacksAvailable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldAnimatePackButtonIn
	{
		get
		{
			return m_ShouldAnimatePackButtonIn;
		}
		set
		{
			if (m_ShouldAnimatePackButtonIn != value)
			{
				m_ShouldAnimatePackButtonIn = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldAnimatePackButtonOut
	{
		get
		{
			return m_ShouldAnimatePackButtonOut;
		}
		set
		{
			if (m_ShouldAnimatePackButtonOut != value)
			{
				m_ShouldAnimatePackButtonOut = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Enabled;
		int num2 = (num + m_Enabled.GetHashCode()) * 31;
		_ = m_CollectionHasUpdates;
		int num3 = (num2 + m_CollectionHasUpdates.GetHashCode()) * 31;
		_ = m_ZoneSelectionHasUpdates;
		int num4 = (num3 + m_ZoneSelectionHasUpdates.GetHashCode()) * 31;
		_ = m_ArenaHasUpdates;
		int num5 = (num4 + m_ArenaHasUpdates.GetHashCode()) * 31;
		_ = m_ShopHasUpdates;
		int num6 = (num5 + m_ShopHasUpdates.GetHashCode()) * 31;
		_ = m_TrainingHallHasUpdates;
		int num7 = (num6 + m_TrainingHallHasUpdates.GetHashCode()) * 31;
		_ = m_NumMailboxUpdates;
		int num8 = (num7 + m_NumMailboxUpdates.GetHashCode()) * 31;
		_ = m_WorkshopHasUpdates;
		int num9 = (num8 + m_WorkshopHasUpdates.GetHashCode()) * 31;
		_ = m_TaskboardHasUpdates;
		int num10 = (num9 + m_TaskboardHasUpdates.GetHashCode()) * 31;
		_ = m_NumPacksAvailable;
		int num11 = (num10 + m_NumPacksAvailable.GetHashCode()) * 31;
		_ = m_ShouldAnimatePackButtonIn;
		int num12 = (num11 + m_ShouldAnimatePackButtonIn.GetHashCode()) * 31;
		_ = m_ShouldAnimatePackButtonOut;
		return num12 + m_ShouldAnimatePackButtonOut.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 306:
			value = m_Enabled;
			return true;
		case 550:
			value = m_CollectionHasUpdates;
			return true;
		case 551:
			value = m_ZoneSelectionHasUpdates;
			return true;
		case 552:
			value = m_ArenaHasUpdates;
			return true;
		case 553:
			value = m_ShopHasUpdates;
			return true;
		case 554:
			value = m_TrainingHallHasUpdates;
			return true;
		case 555:
			value = m_NumMailboxUpdates;
			return true;
		case 556:
			value = m_WorkshopHasUpdates;
			return true;
		case 557:
			value = m_TaskboardHasUpdates;
			return true;
		case 558:
			value = m_NumPacksAvailable;
			return true;
		case 568:
			value = m_ShouldAnimatePackButtonIn;
			return true;
		case 569:
			value = m_ShouldAnimatePackButtonOut;
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
		case 306:
			Enabled = value != null && (bool)value;
			return true;
		case 550:
			CollectionHasUpdates = value != null && (bool)value;
			return true;
		case 551:
			ZoneSelectionHasUpdates = value != null && (bool)value;
			return true;
		case 552:
			ArenaHasUpdates = value != null && (bool)value;
			return true;
		case 553:
			ShopHasUpdates = value != null && (bool)value;
			return true;
		case 554:
			TrainingHallHasUpdates = value != null && (bool)value;
			return true;
		case 555:
			NumMailboxUpdates = ((value != null) ? ((int)value) : 0);
			return true;
		case 556:
			WorkshopHasUpdates = value != null && (bool)value;
			return true;
		case 557:
			TaskboardHasUpdates = value != null && (bool)value;
			return true;
		case 558:
			NumPacksAvailable = ((value != null) ? ((int)value) : 0);
			return true;
		case 568:
			ShouldAnimatePackButtonIn = value != null && (bool)value;
			return true;
		case 569:
			ShouldAnimatePackButtonOut = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 306:
			info = Properties[0];
			return true;
		case 550:
			info = Properties[1];
			return true;
		case 551:
			info = Properties[2];
			return true;
		case 552:
			info = Properties[3];
			return true;
		case 553:
			info = Properties[4];
			return true;
		case 554:
			info = Properties[5];
			return true;
		case 555:
			info = Properties[6];
			return true;
		case 556:
			info = Properties[7];
			return true;
		case 557:
			info = Properties[8];
			return true;
		case 558:
			info = Properties[9];
			return true;
		case 568:
			info = Properties[10];
			return true;
		case 569:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
