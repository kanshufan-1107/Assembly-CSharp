using System.Collections.Generic;
using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageBuildingDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 488;

	private MercenaryBuilding.Mercenarybuildingtype m_BuildingType;

	private int m_CurrentTierId;

	private bool m_Enabled;

	private bool m_IsConstructionAnimationPlaying;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 489,
			PropertyDisplayName = "building_type",
			Type = typeof(MercenaryBuilding.Mercenarybuildingtype)
		},
		new DataModelProperty
		{
			PropertyId = 490,
			PropertyDisplayName = "current_tier_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 497,
			PropertyDisplayName = "enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 570,
			PropertyDisplayName = "is_construction_animation_playing",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 488;

	public string DataModelDisplayName => "mercenary_village_building";

	public MercenaryBuilding.Mercenarybuildingtype BuildingType
	{
		get
		{
			return m_BuildingType;
		}
		set
		{
			if (m_BuildingType != value)
			{
				m_BuildingType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentTierId
	{
		get
		{
			return m_CurrentTierId;
		}
		set
		{
			if (m_CurrentTierId != value)
			{
				m_CurrentTierId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

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

	public bool IsConstructionAnimationPlaying
	{
		get
		{
			return m_IsConstructionAnimationPlaying;
		}
		set
		{
			if (m_IsConstructionAnimationPlaying != value)
			{
				m_IsConstructionAnimationPlaying = value;
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
		_ = m_BuildingType;
		int num2 = (num + m_BuildingType.GetHashCode()) * 31;
		_ = m_CurrentTierId;
		int num3 = (num2 + m_CurrentTierId.GetHashCode()) * 31;
		_ = m_Enabled;
		int num4 = (num3 + m_Enabled.GetHashCode()) * 31;
		_ = m_IsConstructionAnimationPlaying;
		return num4 + m_IsConstructionAnimationPlaying.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 489:
			value = m_BuildingType;
			return true;
		case 490:
			value = m_CurrentTierId;
			return true;
		case 497:
			value = m_Enabled;
			return true;
		case 570:
			value = m_IsConstructionAnimationPlaying;
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
		case 489:
			BuildingType = ((value != null) ? ((MercenaryBuilding.Mercenarybuildingtype)value) : MercenaryBuilding.Mercenarybuildingtype.VILLAGE);
			return true;
		case 490:
			CurrentTierId = ((value != null) ? ((int)value) : 0);
			return true;
		case 497:
			Enabled = value != null && (bool)value;
			return true;
		case 570:
			IsConstructionAnimationPlaying = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 489:
			info = Properties[0];
			return true;
		case 490:
			info = Properties[1];
			return true;
		case 497:
			info = Properties[2];
			return true;
		case 570:
			info = Properties[3];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
