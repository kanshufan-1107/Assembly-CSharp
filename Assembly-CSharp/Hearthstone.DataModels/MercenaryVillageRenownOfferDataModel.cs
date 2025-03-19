using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageRenownOfferDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 716;

	private string m_OfferName;

	private int m_RenownCost;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 723,
			PropertyDisplayName = "offer_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 717,
			PropertyDisplayName = "renown_cost",
			Type = typeof(int)
		}
	};

	public int DataModelId => 716;

	public string DataModelDisplayName => "mercenary_village_renown_offer";

	public string OfferName
	{
		get
		{
			return m_OfferName;
		}
		set
		{
			if (!(m_OfferName == value))
			{
				m_OfferName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RenownCost
	{
		get
		{
			return m_RenownCost;
		}
		set
		{
			if (m_RenownCost != value)
			{
				m_RenownCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_OfferName != null) ? m_OfferName.GetHashCode() : 0)) * 31;
		_ = m_RenownCost;
		return num + m_RenownCost.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 723:
			value = m_OfferName;
			return true;
		case 717:
			value = m_RenownCost;
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
		case 723:
			OfferName = ((value != null) ? ((string)value) : null);
			return true;
		case 717:
			RenownCost = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 723:
			info = Properties[0];
			return true;
		case 717:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
