using Hearthstone.UI;
using PegasusLettuce;

namespace Hearthstone.DataModels;

public class LettuceMapCoinDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 199;

	private int m_Id;

	private AdventureMissionDataModel m_CoinData;

	private DataModelList<int> m_NeighborIds = new DataModelList<int>();

	private int m_NodeTypeId;

	private Mercenary.Role m_MercenaryRole;

	private LettuceMapNode.NodeState m_CoinState;

	private DataModelList<int> m_ParentIds = new DataModelList<int>();

	private bool m_LineGlowVisible;

	private string m_NodeVisualId;

	private string m_HoverTooltipHeader;

	private string m_HoverTooltipBody;

	private CardDataModel m_GrantedAnomalyCard;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "coin_data",
			Type = typeof(AdventureMissionDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "neighbord_ids",
			Type = typeof(DataModelList<int>)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "node_type_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "mercenary_role",
			Type = typeof(Mercenary.Role)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "coin_state",
			Type = typeof(LettuceMapNode.NodeState)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "parent_ids",
			Type = typeof(DataModelList<int>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "line_glow_visible",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "node_visual_id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "hover_tooltip_header",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "hover_tooltip_body",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "granted_anomaly_card",
			Type = typeof(CardDataModel)
		}
	};

	public int DataModelId => 199;

	public string DataModelDisplayName => "lettuce_map_coin";

	public int Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			if (m_Id != value)
			{
				m_Id = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AdventureMissionDataModel CoinData
	{
		get
		{
			return m_CoinData;
		}
		set
		{
			if (m_CoinData != value)
			{
				RemoveNestedDataModel(m_CoinData);
				RegisterNestedDataModel(value);
				m_CoinData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<int> NeighborIds
	{
		get
		{
			return m_NeighborIds;
		}
		set
		{
			if (m_NeighborIds != value)
			{
				RemoveNestedDataModel(m_NeighborIds);
				RegisterNestedDataModel(value);
				m_NeighborIds = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NodeTypeId
	{
		get
		{
			return m_NodeTypeId;
		}
		set
		{
			if (m_NodeTypeId != value)
			{
				m_NodeTypeId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Mercenary.Role MercenaryRole
	{
		get
		{
			return m_MercenaryRole;
		}
		set
		{
			if (m_MercenaryRole != value)
			{
				m_MercenaryRole = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMapNode.NodeState CoinState
	{
		get
		{
			return m_CoinState;
		}
		set
		{
			if (m_CoinState != value)
			{
				m_CoinState = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<int> ParentIds
	{
		get
		{
			return m_ParentIds;
		}
		set
		{
			if (m_ParentIds != value)
			{
				RemoveNestedDataModel(m_ParentIds);
				RegisterNestedDataModel(value);
				m_ParentIds = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool LineGlowVisible
	{
		get
		{
			return m_LineGlowVisible;
		}
		set
		{
			if (m_LineGlowVisible != value)
			{
				m_LineGlowVisible = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string NodeVisualId
	{
		get
		{
			return m_NodeVisualId;
		}
		set
		{
			if (!(m_NodeVisualId == value))
			{
				m_NodeVisualId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string HoverTooltipHeader
	{
		get
		{
			return m_HoverTooltipHeader;
		}
		set
		{
			if (!(m_HoverTooltipHeader == value))
			{
				m_HoverTooltipHeader = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string HoverTooltipBody
	{
		get
		{
			return m_HoverTooltipBody;
		}
		set
		{
			if (!(m_HoverTooltipBody == value))
			{
				m_HoverTooltipBody = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel GrantedAnomalyCard
	{
		get
		{
			return m_GrantedAnomalyCard;
		}
		set
		{
			if (m_GrantedAnomalyCard != value)
			{
				RemoveNestedDataModel(m_GrantedAnomalyCard);
				RegisterNestedDataModel(value);
				m_GrantedAnomalyCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMapCoinDataModel()
	{
		RegisterNestedDataModel(m_CoinData);
		RegisterNestedDataModel(m_NeighborIds);
		RegisterNestedDataModel(m_ParentIds);
		RegisterNestedDataModel(m_GrantedAnomalyCard);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Id;
		int num2 = (((num + m_Id.GetHashCode()) * 31 + ((m_CoinData != null) ? m_CoinData.GetPropertiesHashCode() : 0)) * 31 + ((m_NeighborIds != null) ? m_NeighborIds.GetPropertiesHashCode() : 0)) * 31;
		_ = m_NodeTypeId;
		int num3 = (num2 + m_NodeTypeId.GetHashCode()) * 31;
		_ = m_MercenaryRole;
		int num4 = (num3 + m_MercenaryRole.GetHashCode()) * 31;
		_ = m_CoinState;
		int num5 = ((num4 + m_CoinState.GetHashCode()) * 31 + ((m_ParentIds != null) ? m_ParentIds.GetPropertiesHashCode() : 0)) * 31;
		_ = m_LineGlowVisible;
		return ((((num5 + m_LineGlowVisible.GetHashCode()) * 31 + ((m_NodeVisualId != null) ? m_NodeVisualId.GetHashCode() : 0)) * 31 + ((m_HoverTooltipHeader != null) ? m_HoverTooltipHeader.GetHashCode() : 0)) * 31 + ((m_HoverTooltipBody != null) ? m_HoverTooltipBody.GetHashCode() : 0)) * 31 + ((m_GrantedAnomalyCard != null) ? m_GrantedAnomalyCard.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Id;
			return true;
		case 1:
			value = m_CoinData;
			return true;
		case 2:
			value = m_NeighborIds;
			return true;
		case 3:
			value = m_NodeTypeId;
			return true;
		case 4:
			value = m_MercenaryRole;
			return true;
		case 5:
			value = m_CoinState;
			return true;
		case 6:
			value = m_ParentIds;
			return true;
		case 7:
			value = m_LineGlowVisible;
			return true;
		case 8:
			value = m_NodeVisualId;
			return true;
		case 9:
			value = m_HoverTooltipHeader;
			return true;
		case 10:
			value = m_HoverTooltipBody;
			return true;
		case 11:
			value = m_GrantedAnomalyCard;
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
			Id = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			CoinData = ((value != null) ? ((AdventureMissionDataModel)value) : null);
			return true;
		case 2:
			NeighborIds = ((value != null) ? ((DataModelList<int>)value) : null);
			return true;
		case 3:
			NodeTypeId = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			MercenaryRole = ((value != null) ? ((Mercenary.Role)value) : Mercenary.Role.ROLE_INVALID);
			return true;
		case 5:
			CoinState = ((value != null) ? ((LettuceMapNode.NodeState)value) : ((LettuceMapNode.NodeState)0));
			return true;
		case 6:
			ParentIds = ((value != null) ? ((DataModelList<int>)value) : null);
			return true;
		case 7:
			LineGlowVisible = value != null && (bool)value;
			return true;
		case 8:
			NodeVisualId = ((value != null) ? ((string)value) : null);
			return true;
		case 9:
			HoverTooltipHeader = ((value != null) ? ((string)value) : null);
			return true;
		case 10:
			HoverTooltipBody = ((value != null) ? ((string)value) : null);
			return true;
		case 11:
			GrantedAnomalyCard = ((value != null) ? ((CardDataModel)value) : null);
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
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
