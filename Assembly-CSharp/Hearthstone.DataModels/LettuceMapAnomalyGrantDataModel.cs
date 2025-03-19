using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMapAnomalyGrantDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 495;

	private CardDataModel m_GrantedCard;

	private DataModelList<float> m_SourceNodePosition = new DataModelList<float>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "granted_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "source_node_position",
			Type = typeof(DataModelList<float>)
		}
	};

	public int DataModelId => 495;

	public string DataModelDisplayName => "lettuce_map_anomaly_grant";

	public CardDataModel GrantedCard
	{
		get
		{
			return m_GrantedCard;
		}
		set
		{
			if (m_GrantedCard != value)
			{
				RemoveNestedDataModel(m_GrantedCard);
				RegisterNestedDataModel(value);
				m_GrantedCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<float> SourceNodePosition
	{
		get
		{
			return m_SourceNodePosition;
		}
		set
		{
			if (m_SourceNodePosition != value)
			{
				RemoveNestedDataModel(m_SourceNodePosition);
				RegisterNestedDataModel(value);
				m_SourceNodePosition = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMapAnomalyGrantDataModel()
	{
		RegisterNestedDataModel(m_GrantedCard);
		RegisterNestedDataModel(m_SourceNodePosition);
	}

	public int GetPropertiesHashCode()
	{
		return (17 * 31 + ((m_GrantedCard != null) ? m_GrantedCard.GetPropertiesHashCode() : 0)) * 31 + ((m_SourceNodePosition != null) ? m_SourceNodePosition.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_GrantedCard;
			return true;
		case 1:
			value = m_SourceNodePosition;
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
			GrantedCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 1:
			SourceNodePosition = ((value != null) ? ((DataModelList<float>)value) : null);
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
