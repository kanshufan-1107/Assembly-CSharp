using Hearthstone.DataModels;

public class ShopTabData
{
	private readonly ShopTabDataModel m_shopTabDataModel;

	public string Id
	{
		get
		{
			return m_shopTabDataModel.Id;
		}
		set
		{
			m_shopTabDataModel.Id = value;
		}
	}

	public string Name
	{
		get
		{
			return m_shopTabDataModel.Name;
		}
		set
		{
			m_shopTabDataModel.Name = value;
		}
	}

	public string Icon
	{
		set
		{
			m_shopTabDataModel.Icon = value;
		}
	}

	public bool Locked
	{
		get
		{
			return m_shopTabDataModel.Locked;
		}
		set
		{
			m_shopTabDataModel.Locked = value;
		}
	}

	public IGamemodeAvailabilityService.Gamemode LockedMode
	{
		set
		{
			m_shopTabDataModel.LockedMode = value;
		}
	}

	public IGamemodeAvailabilityService.Status LockedReason
	{
		set
		{
			m_shopTabDataModel.LockedReason = value;
		}
	}

	public bool HasUndisplayedProducts
	{
		set
		{
			m_shopTabDataModel.HasUndisplayedProducts = value;
		}
	}

	public bool NotificationEnabled
	{
		get
		{
			return m_shopTabDataModel.NotificationEnabled;
		}
		set
		{
			m_shopTabDataModel.NotificationEnabled = value;
		}
	}

	public bool Enabled { get; set; }

	public int Order { get; set; }

	public ShopTabData(string id, string name)
	{
		m_shopTabDataModel = new ShopTabDataModel();
		Id = id;
		Name = name;
	}

	public ShopTabDataModel GetTabDataModel()
	{
		return m_shopTabDataModel;
	}
}
