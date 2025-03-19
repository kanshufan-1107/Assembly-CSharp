using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskDetailDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 671;

	private MercenaryVillageRenownTradeDataModel m_RenownTradeData;

	private MercenaryVillageRenownOfferDataModel m_CurrentRenownOfferData;

	private bool m_ProcessingRequest;

	private bool m_HasRequestedRenownConversion;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 707,
			PropertyDisplayName = "renown_trade_data",
			Type = typeof(MercenaryVillageRenownTradeDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 718,
			PropertyDisplayName = "current_renown_offer_data",
			Type = typeof(MercenaryVillageRenownOfferDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 672,
			PropertyDisplayName = "processing_request",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 719,
			PropertyDisplayName = "has_requested_renown_conversion",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 671;

	public string DataModelDisplayName => "mercenary_village_task_detail";

	public MercenaryVillageRenownTradeDataModel RenownTradeData
	{
		get
		{
			return m_RenownTradeData;
		}
		set
		{
			if (m_RenownTradeData != value)
			{
				RemoveNestedDataModel(m_RenownTradeData);
				RegisterNestedDataModel(value);
				m_RenownTradeData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MercenaryVillageRenownOfferDataModel CurrentRenownOfferData
	{
		get
		{
			return m_CurrentRenownOfferData;
		}
		set
		{
			if (m_CurrentRenownOfferData != value)
			{
				RemoveNestedDataModel(m_CurrentRenownOfferData);
				RegisterNestedDataModel(value);
				m_CurrentRenownOfferData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ProcessingRequest
	{
		get
		{
			return m_ProcessingRequest;
		}
		set
		{
			if (m_ProcessingRequest != value)
			{
				m_ProcessingRequest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasRequestedRenownConversion
	{
		get
		{
			return m_HasRequestedRenownConversion;
		}
		set
		{
			if (m_HasRequestedRenownConversion != value)
			{
				m_HasRequestedRenownConversion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageTaskDetailDataModel()
	{
		RegisterNestedDataModel(m_RenownTradeData);
		RegisterNestedDataModel(m_CurrentRenownOfferData);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_RenownTradeData != null && !inspectedDataModels.Contains(m_RenownTradeData.GetHashCode()))
		{
			inspectedDataModels.Add(m_RenownTradeData.GetHashCode());
			hash = hash * 31 + m_RenownTradeData.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_CurrentRenownOfferData != null && !inspectedDataModels.Contains(m_CurrentRenownOfferData.GetHashCode()))
		{
			inspectedDataModels.Add(m_CurrentRenownOfferData.GetHashCode());
			hash = hash * 31 + m_CurrentRenownOfferData.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_ProcessingRequest;
		hash = num + m_ProcessingRequest.GetHashCode();
		int num2 = hash * 31;
		_ = m_HasRequestedRenownConversion;
		return num2 + m_HasRequestedRenownConversion.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 707:
			value = m_RenownTradeData;
			return true;
		case 718:
			value = m_CurrentRenownOfferData;
			return true;
		case 672:
			value = m_ProcessingRequest;
			return true;
		case 719:
			value = m_HasRequestedRenownConversion;
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
		case 707:
			RenownTradeData = ((value != null) ? ((MercenaryVillageRenownTradeDataModel)value) : null);
			return true;
		case 718:
			CurrentRenownOfferData = ((value != null) ? ((MercenaryVillageRenownOfferDataModel)value) : null);
			return true;
		case 672:
			ProcessingRequest = value != null && (bool)value;
			return true;
		case 719:
			HasRequestedRenownConversion = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 707:
			info = Properties[0];
			return true;
		case 718:
			info = Properties[1];
			return true;
		case 672:
			info = Properties[2];
			return true;
		case 719:
			info = Properties[3];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
