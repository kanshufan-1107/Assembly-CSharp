using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageVisitorDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 336;

	private int m_MercenaryId;

	private CardDataModel m_MercenaryCard;

	private bool m_NewlyArrived;

	private bool m_IsTimedEvent;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 338,
			PropertyDisplayName = "mercenary_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 339,
			PropertyDisplayName = "mercenary_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 572,
			PropertyDisplayName = "newly_arrived",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 612,
			PropertyDisplayName = "is_timed_event",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 336;

	public string DataModelDisplayName => "mercenaryvillagevisitor";

	public int MercenaryId
	{
		get
		{
			return m_MercenaryId;
		}
		set
		{
			if (m_MercenaryId != value)
			{
				m_MercenaryId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel MercenaryCard
	{
		get
		{
			return m_MercenaryCard;
		}
		set
		{
			if (m_MercenaryCard != value)
			{
				RemoveNestedDataModel(m_MercenaryCard);
				RegisterNestedDataModel(value);
				m_MercenaryCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NewlyArrived
	{
		get
		{
			return m_NewlyArrived;
		}
		set
		{
			if (m_NewlyArrived != value)
			{
				m_NewlyArrived = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsTimedEvent
	{
		get
		{
			return m_IsTimedEvent;
		}
		set
		{
			if (m_IsTimedEvent != value)
			{
				m_IsTimedEvent = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageVisitorDataModel()
	{
		RegisterNestedDataModel(m_MercenaryCard);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_MercenaryId;
		hash = num + m_MercenaryId.GetHashCode();
		if (m_MercenaryCard != null && !inspectedDataModels.Contains(m_MercenaryCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_MercenaryCard.GetHashCode());
			hash = hash * 31 + m_MercenaryCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_NewlyArrived;
		hash = num2 + m_NewlyArrived.GetHashCode();
		int num3 = hash * 31;
		_ = m_IsTimedEvent;
		return num3 + m_IsTimedEvent.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 338:
			value = m_MercenaryId;
			return true;
		case 339:
			value = m_MercenaryCard;
			return true;
		case 572:
			value = m_NewlyArrived;
			return true;
		case 612:
			value = m_IsTimedEvent;
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
		case 338:
			MercenaryId = ((value != null) ? ((int)value) : 0);
			return true;
		case 339:
			MercenaryCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 572:
			NewlyArrived = value != null && (bool)value;
			return true;
		case 612:
			IsTimedEvent = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 338:
			info = Properties[0];
			return true;
		case 339:
			info = Properties[1];
			return true;
		case 572:
			info = Properties[2];
			return true;
		case 612:
			info = Properties[3];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
