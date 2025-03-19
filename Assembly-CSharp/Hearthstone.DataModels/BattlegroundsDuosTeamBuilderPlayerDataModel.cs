using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsDuosTeamBuilderPlayerDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 937;

	private int m_BattlegroundsDuosRating;

	private GameAccountIdDataModel m_GameAccountId;

	private string m_DisplayName;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "battlegrounds_duos_rating",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "game_account_id",
			Type = typeof(GameAccountIdDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "display_name",
			Type = typeof(string)
		}
	};

	public int DataModelId => 937;

	public string DataModelDisplayName => "battlegrounds_duos_team_builder_player";

	public int BattlegroundsDuosRating
	{
		get
		{
			return m_BattlegroundsDuosRating;
		}
		set
		{
			if (m_BattlegroundsDuosRating != value)
			{
				m_BattlegroundsDuosRating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public GameAccountIdDataModel GameAccountId
	{
		get
		{
			return m_GameAccountId;
		}
		set
		{
			if (m_GameAccountId != value)
			{
				RemoveNestedDataModel(m_GameAccountId);
				RegisterNestedDataModel(value);
				m_GameAccountId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			if (!(m_DisplayName == value))
			{
				m_DisplayName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BattlegroundsDuosTeamBuilderPlayerDataModel()
	{
		RegisterNestedDataModel(m_GameAccountId);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_BattlegroundsDuosRating;
		return ((num + m_BattlegroundsDuosRating.GetHashCode()) * 31 + ((m_GameAccountId != null) ? m_GameAccountId.GetPropertiesHashCode() : 0)) * 31 + ((m_DisplayName != null) ? m_DisplayName.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_BattlegroundsDuosRating;
			return true;
		case 1:
			value = m_GameAccountId;
			return true;
		case 2:
			value = m_DisplayName;
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
			BattlegroundsDuosRating = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			GameAccountId = ((value != null) ? ((GameAccountIdDataModel)value) : null);
			return true;
		case 2:
			DisplayName = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
