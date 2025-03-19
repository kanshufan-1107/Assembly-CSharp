using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TwistHeroicDeckRowDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 944;

	private TwistHeroicDeckDataModel m_DeckLeft;

	private TwistHeroicDeckDataModel m_DeckRight;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "deck_left",
			Type = typeof(TwistHeroicDeckDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "deck_right",
			Type = typeof(TwistHeroicDeckDataModel)
		}
	};

	public int DataModelId => 944;

	public string DataModelDisplayName => "twist_heroic_deck_row";

	public TwistHeroicDeckDataModel DeckLeft
	{
		get
		{
			return m_DeckLeft;
		}
		set
		{
			if (m_DeckLeft != value)
			{
				RemoveNestedDataModel(m_DeckLeft);
				RegisterNestedDataModel(value);
				m_DeckLeft = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TwistHeroicDeckDataModel DeckRight
	{
		get
		{
			return m_DeckRight;
		}
		set
		{
			if (m_DeckRight != value)
			{
				RemoveNestedDataModel(m_DeckRight);
				RegisterNestedDataModel(value);
				m_DeckRight = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TwistHeroicDeckRowDataModel()
	{
		RegisterNestedDataModel(m_DeckLeft);
		RegisterNestedDataModel(m_DeckRight);
	}

	public int GetPropertiesHashCode()
	{
		return (17 * 31 + ((m_DeckLeft != null) ? m_DeckLeft.GetPropertiesHashCode() : 0)) * 31 + ((m_DeckRight != null) ? m_DeckRight.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_DeckLeft;
			return true;
		case 1:
			value = m_DeckRight;
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
			DeckLeft = ((value != null) ? ((TwistHeroicDeckDataModel)value) : null);
			return true;
		case 1:
			DeckRight = ((value != null) ? ((TwistHeroicDeckDataModel)value) : null);
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
