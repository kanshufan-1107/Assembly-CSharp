using Hearthstone.UI;
using PegasusShared;

namespace Hearthstone.DataModels;

public class BattlegroundsMedalDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 999;

	private int m_Rating;

	private GameType m_GameType;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "rating",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "game_type",
			Type = typeof(GameType)
		}
	};

	public int DataModelId => 999;

	public string DataModelDisplayName => "battlegrounds_medal";

	public int Rating
	{
		get
		{
			return m_Rating;
		}
		set
		{
			if (m_Rating != value)
			{
				m_Rating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public GameType GameType
	{
		get
		{
			return m_GameType;
		}
		set
		{
			if (m_GameType != value)
			{
				m_GameType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Rating;
		int num2 = (num + m_Rating.GetHashCode()) * 31;
		_ = m_GameType;
		return num2 + m_GameType.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Rating;
			return true;
		case 1:
			value = m_GameType;
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
			Rating = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			GameType = ((value != null) ? ((GameType)value) : GameType.GT_UNKNOWN);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
