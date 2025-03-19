using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryMythicTreasureDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 749;

	private int m_TreasureId;

	private CardDataModel m_MyticTreasure;

	private int m_TreasureScalar;

	private bool m_ReadyForUpgrade;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "treasure_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mytic_treasure",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "treasure_scalar",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "ready_for_upgrade",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 749;

	public string DataModelDisplayName => "mercenary_mythic_treasure";

	public int TreasureId
	{
		get
		{
			return m_TreasureId;
		}
		set
		{
			if (m_TreasureId != value)
			{
				m_TreasureId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel MyticTreasure
	{
		get
		{
			return m_MyticTreasure;
		}
		set
		{
			if (m_MyticTreasure != value)
			{
				RemoveNestedDataModel(m_MyticTreasure);
				RegisterNestedDataModel(value);
				m_MyticTreasure = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TreasureScalar
	{
		get
		{
			return m_TreasureScalar;
		}
		set
		{
			if (m_TreasureScalar != value)
			{
				m_TreasureScalar = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ReadyForUpgrade
	{
		get
		{
			return m_ReadyForUpgrade;
		}
		set
		{
			if (m_ReadyForUpgrade != value)
			{
				m_ReadyForUpgrade = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryMythicTreasureDataModel()
	{
		RegisterNestedDataModel(m_MyticTreasure);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_TreasureId;
		int num2 = ((num + m_TreasureId.GetHashCode()) * 31 + ((m_MyticTreasure != null) ? m_MyticTreasure.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TreasureScalar;
		int num3 = (num2 + m_TreasureScalar.GetHashCode()) * 31;
		_ = m_ReadyForUpgrade;
		return num3 + m_ReadyForUpgrade.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TreasureId;
			return true;
		case 1:
			value = m_MyticTreasure;
			return true;
		case 2:
			value = m_TreasureScalar;
			return true;
		case 3:
			value = m_ReadyForUpgrade;
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
			TreasureId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			MyticTreasure = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 2:
			TreasureScalar = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			ReadyForUpgrade = value != null && (bool)value;
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
