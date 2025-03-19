using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;
using Hearthstone.Http;
using MiniJSON;
using UnityEngine;

namespace Content.Delivery;

public class ContentConnect
{
	public const string ENV_PRODUCTION = "production";

	public const string ENV_DEV = "dev";

	public const int CACHE_AGE = 0;

	public const string ENV_DEFAULT = "production";

	private const int MIN_REFRESH_TIME = 3600;

	private ContentConnectData m_data = new ContentConnectData();

	private Dictionary<string, string> m_headers = new Dictionary<string, string>();

	private ContentConnectSettings m_settings;

	private DateTime m_lastRequestTime = DateTime.UnixEpoch;

	public static bool ContentstackEnabled => NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>()?.ContentstackEnabled ?? true;

	public bool Ready
	{
		get
		{
			if (!string.IsNullOrEmpty(ServiceUrl) && !InProcessingQuery && !IsCachedDataValid)
			{
				return CanMakeRequest;
			}
			return false;
		}
	}

	public int ValidSeconds
	{
		get
		{
			if (m_data != null)
			{
				ulong secondsSinceLastDownload = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now) - m_data.m_lastDownloadTime;
				if (secondsSinceLastDownload < (ulong)m_data.m_age)
				{
					return m_data.m_age - (int)secondsSinceLastDownload;
				}
			}
			return -1;
		}
	}

	private string ServiceUrl
	{
		get
		{
			return m_settings.m_url;
		}
		set
		{
			m_settings.m_url = value;
			m_data.m_sha1OfServiceUrl = Crypto.SHA1.Calc(value);
		}
	}

	private string ContentType => m_settings.m_contentType;

	private bool InProcessingQuery { get; set; }

	private bool IsCachedDataValid
	{
		get
		{
			int validSecond = ValidSeconds;
			if (validSecond > 0 && !string.IsNullOrEmpty(m_data.m_response))
			{
				Log.ContentConnect.PrintDebug("{0}, still valid within {1}s", ContentType, validSecond);
				return true;
			}
			return false;
		}
	}

	private bool CanMakeRequest => (DateTime.Now - m_lastRequestTime).TotalSeconds >= 3600.0;

	public void ResetServiceURL(string url)
	{
		ServiceUrl = url;
	}

	public static string OptionStringFormat(int age, ulong downloadtime, string sha1OfServiceUrl, string hexResponse)
	{
		return $"{1:D2}:{age}|{downloadtime}|{sha1OfServiceUrl}|{hexResponse}";
	}

	public IEnumerator Query(ResponseProcessHandler responseProcessHandler, object param, string queryParameter, bool force)
	{
		while (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null)
		{
			yield return null;
		}
		if (!ContentstackEnabled)
		{
			Log.ContentConnect.PrintDebug("Skip to update because Contentstack is disabled: {0}", ContentType);
			responseProcessHandler(string.Empty, param);
			yield break;
		}
		if (!CanMakeRequest)
		{
			Log.ContentConnect.PrintDebug("Ignoring query request as we are not ready to make one");
			responseProcessHandler?.Invoke(m_data?.m_response ?? string.Empty, param);
			yield break;
		}
		InProcessingQuery = true;
		if (!force && IsCachedDataValid)
		{
			Log.ContentConnect.PrintInfo("{0}, Using cached response: {1}; (will refresh after: {2}s)", ContentType, m_data.m_response, m_data.m_age);
		}
		else
		{
			_ = queryParameter;
			string text = ((!string.IsNullOrEmpty(queryParameter)) ? (ServiceUrl + "&query=" + Uri.EscapeDataString(queryParameter)) : ServiceUrl);
			string requestUrl = text;
			Log.ContentConnect.PrintDebug("Query: {0}", requestUrl);
			IHttpRequest httpRequest = HttpRequestFactory.Get().CreateGetRequest(requestUrl);
			httpRequest.SetRequestHeaders(m_headers);
			m_lastRequestTime = DateTime.Now;
			yield return httpRequest.SendRequest();
			if (httpRequest.IsNetworkError || httpRequest.IsHttpError)
			{
				Debug.LogWarning("Failed to download url in ContentConnect: " + ServiceUrl);
				Debug.LogWarning(httpRequest.ErrorString);
				responseProcessHandler(string.Empty, param);
				InProcessingQuery = false;
				int serverErrorCode = GetErrorCodeFromJsonResponse(httpRequest.ResponseAsString);
				TelemetryManager.Client().SendContentConnectFailedToConnect(ServiceUrl, httpRequest.ResponseStatusCode, serverErrorCode);
				yield break;
			}
			if (httpRequest.ResponseHeaders.TryGetValue("X-Cache", out var cacheResult))
			{
				Log.InGameMessage.PrintDebug("Cache response (Shield, Local): " + cacheResult);
			}
			m_data.m_response = httpRequest.ResponseAsString;
			Log.ContentConnect.PrintDebug("url text is " + m_data.m_response);
			m_data.m_age = ((m_settings.m_overrideAge == 0) ? GetCacheAge(httpRequest) : m_settings.m_overrideAge);
			m_data.m_age = Math.Max(m_data.m_age, 3600);
			m_data.m_lastDownloadTime = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
			Serialize();
		}
		responseProcessHandler(m_data.m_response, param);
		InProcessingQuery = false;
	}

	protected void Init(ContentConnectSettings setting)
	{
		m_settings = setting;
		ServiceUrl = setting.m_url;
		Log.ContentConnect.Print("Url: " + ServiceUrl);
		m_headers["Accept"] = "application/json";
		if (!string.IsNullOrEmpty(m_settings.m_apiKey))
		{
			m_headers["api_key"] = m_settings.m_apiKey;
		}
		if (!string.IsNullOrEmpty(m_settings.m_accessToken))
		{
			m_headers["access_token"] = m_settings.m_accessToken;
		}
		Deserialize();
	}

	private static int GetCacheAge(IHttpRequest httpRequest)
	{
		string cacheControl = string.Empty;
		Dictionary<string, string> responseHeaders = httpRequest.ResponseHeaders;
		if (responseHeaders != null && (responseHeaders.TryGetValue("CACHE-CONTROL", out cacheControl) || responseHeaders.TryGetValue("Strict-Transport-Security", out cacheControl)))
		{
			string[] array = cacheControl.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string token = array[i].ToLowerInvariant().Trim();
				if (token.StartsWith("max-age"))
				{
					string[] subtokens = token.Split('=');
					if (subtokens.Length == 2 && int.TryParse(subtokens[1], out var cacheAge))
					{
						return cacheAge;
					}
				}
			}
		}
		return 0;
	}

	private void Deserialize()
	{
		if (Enum.TryParse<Option>(m_settings.m_saveLocation, out var option))
		{
			string str = Options.Get().GetString(option);
			if (string.IsNullOrEmpty(str))
			{
				return;
			}
			int posAge = 0;
			int posLastDownloadTime = 1;
			int posResponse = 3;
			string[] values;
			if (str[2] == ':')
			{
				string ver = str.Substring(0, 2);
				if (!int.TryParse(ver, out var dataVersion) || dataVersion != 1)
				{
					Log.ContentConnect.PrintDebug("Unknown content version: {0}", ver);
					return;
				}
				values = str.Substring(3).Split('|');
			}
			else
			{
				values = str.Split('|');
			}
			if (values.Length > 3)
			{
				if (!int.TryParse(values[posAge], out m_data.m_age))
				{
					Debug.LogWarningFormat("Failed to convert Age to int: {0}", values[posAge]);
				}
				if (values.Length > posLastDownloadTime && !ulong.TryParse(values[posLastDownloadTime], out m_data.m_lastDownloadTime))
				{
					Debug.LogWarningFormat("Failed to convert LastDownloadTime to ulong: {0}", values[posLastDownloadTime]);
				}
				m_data.m_response = TextUtils.FromHexString(values[posResponse]);
			}
		}
		else
		{
			if (!File.Exists(m_settings.m_saveLocation))
			{
				return;
			}
			string failedReason = string.Empty;
			ContentConnectData data = null;
			try
			{
				data = JsonUtility.FromJson<ContentConnectData>(File.ReadAllText(m_settings.m_saveLocation));
			}
			catch (Exception ex)
			{
				Log.ContentConnect.PrintError("Cannot read the data from '{0}': {1}", m_settings.m_saveLocation, ex.Message);
				failedReason = ex.Message;
			}
			if (data != null)
			{
				m_data = data;
				TryMigrationData();
			}
			else
			{
				try
				{
					File.Delete(m_settings.m_saveLocation);
				}
				catch (Exception ex2)
				{
					Log.ContentConnect.PrintWarning("Cannot delete the broken file '{0}': {1}", m_settings.m_saveLocation, ex2.Message);
					failedReason = failedReason + " | " + ex2.Message;
				}
			}
			if (!string.IsNullOrEmpty(failedReason))
			{
				TelemetryManager.Client().SendContentConnectJsonOpFailed(ContentConnectJsonOpFailed.JsonOp.READ, Path.GetFileName(m_settings.m_saveLocation), failedReason);
			}
		}
	}

	private void TryMigrationData()
	{
		_ = m_data.m_dataVersion;
		_ = 1;
	}

	private void Serialize()
	{
		if (string.IsNullOrEmpty(m_data.m_response))
		{
			return;
		}
		if (Enum.TryParse<Option>(m_settings.m_saveLocation, out var option))
		{
			if (m_data.m_response.Length > 4096)
			{
				Log.ContentConnect.PrintWarning("Aborting because of excessively large response:{0} > {1}", m_data.m_response.Length, 4096);
			}
			else
			{
				Options.Get().SetString(option, OptionStringFormat(m_data.m_age, m_data.m_lastDownloadTime, m_data.m_sha1OfServiceUrl, TextUtils.ToHexString(m_data.m_response)));
			}
			return;
		}
		string failedReason = string.Empty;
		try
		{
			string json = JsonUtility.ToJson(m_data, !HearthstoneApplication.IsPublic());
			File.WriteAllText(m_settings.m_saveLocation, json);
		}
		catch (Exception ex)
		{
			Log.ContentConnect.PrintError("Cannot write the data to '{0}': {1}", m_settings.m_saveLocation, ex.Message);
			failedReason = ex.Message;
		}
		if (!string.IsNullOrEmpty(failedReason))
		{
			TelemetryManager.Client().SendContentConnectJsonOpFailed(ContentConnectJsonOpFailed.JsonOp.WRITE, Path.GetFileName(m_settings.m_saveLocation), failedReason);
		}
	}

	private int GetErrorCodeFromJsonResponse(string response)
	{
		try
		{
			if (Json.Deserialize(response) is JsonNode jsonNode && jsonNode.ContainsKey("error_code"))
			{
				return (int)Convert.ChangeType(jsonNode["error_code"], typeof(int));
			}
		}
		catch (Exception ex)
		{
			Log.ContentConnect.PrintError("Failed to parse the response: {0}\n'{1}'", ex.Message, response);
		}
		return 0;
	}
}
