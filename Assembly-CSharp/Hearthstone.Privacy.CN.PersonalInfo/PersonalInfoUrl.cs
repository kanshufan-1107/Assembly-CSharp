using System;
using System.Collections.Generic;
using System.Text;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using Hearthstone.Http;
using Hearthstone.Privacy.CN.PersonalInfo.Internal;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Hearthstone.Privacy.CN.PersonalInfo;

public class PersonalInfoUrl
{
	public delegate void OnUrlOpenedFinished(bool success);

	private struct DeviceInfoResponse
	{
		public string landingUrl;

		public string error;

		public ulong expireTime;
	}

	private const int TimeoutSeconds = 5;

	private const string AppId = "wtcg";

	private DateTime m_expireTime = DateTime.UnixEpoch;

	private string m_lastUrl = string.Empty;

	private readonly IHttpRequestFactory m_httpRequestFactory;

	private readonly string m_dataSharingEndpoint;

	public PersonalInfoUrl(IHttpRequestFactory httpRequestFactory, string endpoint)
	{
		m_httpRequestFactory = httpRequestFactory;
		m_dataSharingEndpoint = endpoint;
	}

	public void OpenPersonalInfoUrl(OnUrlOpenedFinished callback)
	{
		if (DateTime.Now < m_expireTime)
		{
			Application.OpenURL(m_lastUrl);
			callback?.Invoke(success: true);
		}
		else
		{
			Processor.QueueJobIfNotExist("OpenPersonalInfoUrl", Job_OpenUrl())?.AddJobFinishedEventListener(delegate(JobDefinition _, bool success)
			{
				callback?.Invoke(success);
			});
		}
	}

	private IEnumerator<IAsyncJobResult> Job_OpenUrl()
	{
		ulong accountId = BnetUtils.TryGetBnetAccountId().GetValueOrDefault();
		if (accountId == 0L)
		{
			yield return new JobFailedResult("No account Id found to request personal info");
		}
		string deviceInfo = BuildDeviceInfoJson(accountId);
		Debug.Log("PersonalInfo Request url:" + m_dataSharingEndpoint + " data: " + deviceInfo);
		UnityWebRequest request = UnityWebRequest.Put(m_dataSharingEndpoint, deviceInfo);
		request.method = "POST";
		request.SetRequestHeader("Content-Type", "application/json");
		request.timeout = 5;
		yield return request.SendWebRequest().ToJobDependency();
		if (request.result == UnityWebRequest.Result.ConnectionError)
		{
			yield return new JobFailedResult("Failed to post request due to network error: " + request.error);
		}
		if (request.result == UnityWebRequest.Result.ProtocolError)
		{
			yield return new JobFailedResult($"Failed to post request due to http error code {request.responseCode}");
		}
		string utf8Response = "";
		if (request.downloadHandler.data != null || request.downloadHandler.data.Length != 0)
		{
			byte[] rawResponse = request.downloadHandler.data;
			utf8Response = Encoding.UTF8.GetString(rawResponse);
		}
		else
		{
			yield return new JobFailedResult("empty or missing post request data");
		}
		string response = utf8Response;
		if (string.IsNullOrEmpty(response))
		{
			yield return new JobFailedResult("empty or missing response from post request");
		}
		DeviceInfoResponse? deviceInfoResponse = ParseJson(response);
		if (!deviceInfoResponse.HasValue)
		{
			yield return new JobFailedResult("Failed to parse response from post request");
		}
		if (!string.IsNullOrEmpty(deviceInfoResponse.Value.error))
		{
			yield return new JobFailedResult(deviceInfoResponse.Value.error);
		}
		m_lastUrl = deviceInfoResponse.Value.landingUrl;
		if (string.IsNullOrEmpty(m_lastUrl))
		{
			yield return new JobFailedResult("Expected a landing url but got none");
		}
		m_expireTime = DateTime.Now.AddMilliseconds(deviceInfoResponse.Value.expireTime);
		Application.OpenURL(m_lastUrl);
	}

	private static DeviceInfoResponse? ParseJson(string response)
	{
		try
		{
			JsonNode json = Json.Deserialize(response) as JsonNode;
			DeviceInfoResponse value = default(DeviceInfoResponse);
			value.landingUrl = (json.TryGetValueAs<string>("landingUriWithGuid", out var url) ? url : string.Empty);
			value.error = (json.TryGetValueAs<string>("error", out var error) ? error : string.Empty);
			value.expireTime = (json.TryGetValueAs<string>("expireTime", out var expireTime) ? ulong.Parse(expireTime) : 0);
			return value;
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception parsing personal info json:\n " + response);
			Debug.LogException(exception);
			return null;
		}
	}

	private static string BuildDeviceInfoJson(ulong bnetId)
	{
		JsonNode deviceInfoPayload = new JsonNode
		{
			{ "accountId", bnetId },
			{ "app", "wtcg" }
		};
		JsonList deviceInfo = new JsonList();
		PersonalDeviceInfo.DeviceInfoField[] deviceInfo2 = PersonalDeviceInfo.GetDeviceInfo();
		for (int i = 0; i < deviceInfo2.Length; i++)
		{
			PersonalDeviceInfo.DeviceInfoField field = deviceInfo2[i];
			if (!string.IsNullOrEmpty(field.Label) && !string.IsNullOrEmpty(field.Value))
			{
				JsonNode infoNode = new JsonNode
				{
					{ "label", field.Label },
					{ "value", field.Value }
				};
				deviceInfo.Add(infoNode);
			}
		}
		deviceInfoPayload.Add("deviceInfo", deviceInfo);
		return Json.Serialize(deviceInfoPayload);
	}
}
