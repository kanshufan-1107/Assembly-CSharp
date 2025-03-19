using System.Collections.Generic;
using UnityEngine;

namespace Content.Delivery;

internal class ContentStackConnect : ContentConnect
{
	private const string CONTENTSTACK_HOST = "cdn.blz-contentstack.com";

	private const string CONTENTSTACK_HOST_CN = "contentstack-cdn.blzstatic.cn";

	private const string API_KEY = "blt29e62035cda0de64";

	private Dictionary<string, string> m_deliveryTokens = new Dictionary<string, string>
	{
		{ "production", "csf73275fdecfc6d3c7f670709" },
		{ "dev", "cs4a7882c8951e57ce47123bfa" }
	};

	public void InitializeURL(string contentType, string environment, string bNetLocaleName, bool regionCN, string saveLocation, int overrideAge)
	{
		string url;
		if (m_deliveryTokens.TryGetValue(environment, out var accessToken))
		{
			url = string.Format("https://{0}/v3/content_types/{1}/entries/?environment={2}&locale={3}", regionCN ? "contentstack-cdn.blzstatic.cn" : "cdn.blz-contentstack.com", contentType, environment, bNetLocaleName.ToLower());
		}
		else
		{
			Debug.LogErrorFormat("Wrong environment name is used: {0}", environment);
			url = string.Empty;
		}
		ContentConnectSettings contentConnectSettings = default(ContentConnectSettings);
		contentConnectSettings.m_contentType = contentType;
		contentConnectSettings.m_url = url;
		contentConnectSettings.m_apiKey = "blt29e62035cda0de64";
		contentConnectSettings.m_accessToken = accessToken;
		contentConnectSettings.m_saveLocation = saveLocation;
		contentConnectSettings.m_overrideAge = overrideAge;
		ContentConnectSettings settings = contentConnectSettings;
		Init(settings);
	}
}
