using Hearthstone.UI;
using PegasusShared;

namespace Hearthstone.DataModels;

public class BaconStatsFrameDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 936;

	private GameType m_GameType;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "game_type",
			Type = typeof(GameType)
		}
	};

	public int DataModelId => 936;

	public string DataModelDisplayName => "baconstatsframe";

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
		_ = m_GameType;
		return num + m_GameType.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_GameType;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			GameType = ((value != null) ? ((GameType)value) : GameType.GT_UNKNOWN);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 0)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
