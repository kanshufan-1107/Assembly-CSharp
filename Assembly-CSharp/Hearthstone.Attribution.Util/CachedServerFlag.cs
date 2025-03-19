namespace Hearthstone.Attribution.Util;

public sealed class CachedServerFlag
{
	private readonly IOptions m_options;

	private readonly Option m_serverOptionFlag;

	private readonly Option m_localCachedOption;

	public bool Value
	{
		get
		{
			return GetAppropriateOptionValue();
		}
		set
		{
			UpdateValues(value);
		}
	}

	private bool ServerOptionsAvailable { get; set; }

	public CachedServerFlag(IOptions options, Option serverOptionFlag, Option localCachedOption)
	{
		m_options = options;
		m_serverOptionFlag = serverOptionFlag;
		m_localCachedOption = localCachedOption;
	}

	private bool GetAppropriateOptionValue()
	{
		bool cachedValue = m_options.GetBool(m_localCachedOption, defaultVal: false);
		return m_options.GetBool(m_serverOptionFlag, cachedValue);
	}

	public void OnServerOptionAvailable()
	{
		ServerOptionsAvailable = true;
		bool updatedValue = GetAppropriateOptionValue();
		m_options.SetBool(m_localCachedOption, updatedValue);
	}

	private void UpdateValues(bool value)
	{
		m_options.SetBool(m_localCachedOption, value);
		if (ServerOptionsAvailable)
		{
			m_options.SetBool(m_serverOptionFlag, value);
		}
	}
}
