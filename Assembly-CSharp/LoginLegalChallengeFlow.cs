using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using MiniJSON;
using UnityEngine.Networking;

public class LoginLegalChallengeFlow
{
	public struct LegalChallengeInitialResponseData
	{
		internal List<string> inputs;

		internal List<LegalAgreementData> legalAgreements;

		internal string promptTitle;

		internal string promptText;
	}

	public struct LegalAgreementData
	{
		internal string title;

		internal string externalURL;
	}

	private static string m_challengeURL;

	private static string m_tassadarLocale;

	private static ILogger m_logger;

	private static Action m_onProvideToken;

	private static LegalChallengeInitialResponseData m_initialLegalResponceData;

	public static void StartLegalChallenge(string challengeUrl, Action onProvideToken, ILogger logger)
	{
		m_challengeURL = challengeUrl;
		m_logger = logger;
		m_onProvideToken = onProvideToken;
		m_tassadarLocale = Localization.ConvertLocaleToDotNet(Localization.GetLocale().ToString());
		Processor.RunCoroutine(Job_ProcessLegalChallenge(challengeUrl, m_tassadarLocale));
	}

	private static IEnumerator<IAsyncJobResult> Job_ProcessLegalChallenge(string challengeUrl, string locale)
	{
		UnityWebRequest request = CreateLegalChallengeGetWebRequest(challengeUrl, locale);
		request.SendWebRequest();
		SimpleTimer requestTimer = new SimpleTimer(new TimeSpan(0, 0, 5));
		while (!request.isDone)
		{
			if (requestTimer.IsTimeout())
			{
				m_logger?.Log(LogLevel.Error, "Could not query legal challenge URL, request timeout");
				yield break;
			}
			yield return null;
		}
		LegalChallengeInitialResponseData responseData;
		if (request.result != UnityWebRequest.Result.Success)
		{
			m_logger?.Log(LogLevel.Error, "Could not query legal challenge URL: " + request.error);
		}
		else if (ReadLegalChallengeResponseData(request.downloadHandler.text, out responseData))
		{
			m_initialLegalResponceData = responseData;
			GetLegalChallengeUIController().ShowLegalChallengePopup(m_initialLegalResponceData, OnUserAccept);
		}
	}

	private static void OnUserAccept()
	{
		Processor.RunCoroutine(Job_SendUserLegalAgreementAcceptance(m_initialLegalResponceData));
	}

	private static IEnumerator Job_SendUserLegalAgreementAcceptance(LegalChallengeInitialResponseData m_responceData)
	{
		JsonList inputList = new JsonList();
		foreach (string item in m_responceData.inputs)
		{
			JsonNode itemNode = new JsonNode
			{
				{ "input_id", item },
				{ "value", "true" }
			};
			inputList.Add(itemNode);
		}
		string jsonBody = Json.Serialize(new JsonNode { { "inputs", inputList } });
		string url = m_challengeURL.Split('?')[0];
		UnityWebRequest request = CreateLegalChallengeAcceptanceWebRequest(url, jsonBody, m_tassadarLocale);
		request.SendWebRequest();
		SimpleTimer requestTimer = new SimpleTimer(new TimeSpan(0, 0, 5));
		while (!request.isDone)
		{
			if (requestTimer.IsTimeout())
			{
				m_logger?.Log(LogLevel.Error, "Could not accept legal challenge, request timeout");
				yield break;
			}
			yield return null;
		}
		bool done;
		if (request.result != UnityWebRequest.Result.Success)
		{
			m_logger?.Log(LogLevel.Error, "Could not accept legal challenge: " + request.error);
		}
		else if (ReadLegalChallengeAcceptanceResponseData(request.downloadHandler.text, out done))
		{
			if (done)
			{
				m_onProvideToken?.Invoke();
			}
			else
			{
				m_logger?.Log(LogLevel.Error, "Could not accept legal challenge. Response returned false. ");
			}
		}
	}

	private static UnityWebRequest CreateLegalChallengeGetWebRequest(string challengeUrl, string locale)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(challengeUrl, "GET");
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("accept", "application/json");
		unityWebRequest.SetRequestHeader("accept-encoding", "gzip, deflate, br");
		unityWebRequest.SetRequestHeader("Accept-Language", locale);
		return unityWebRequest;
	}

	private static UnityWebRequest CreateLegalChallengeAcceptanceWebRequest(string challengeUrl, string jsonBody, string locale)
	{
		UnityWebRequest unityWebRequest = UnityWebRequest.Put(challengeUrl, jsonBody);
		unityWebRequest.method = "POST";
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("accept", "application/json");
		unityWebRequest.SetRequestHeader("Accept-Language", locale);
		return unityWebRequest;
	}

	private static bool ReadLegalChallengeResponseData(string jsonText, out LegalChallengeInitialResponseData data)
	{
		data = default(LegalChallengeInitialResponseData);
		try
		{
			JsonNode challengeDic = (Json.Deserialize(jsonText) as JsonNode)["challenge"] as JsonNode;
			JsonList obj = challengeDic["inputs"] as JsonList;
			List<string> inputIDs = new List<string>();
			foreach (object item in obj)
			{
				JsonNode inputJson = item as JsonNode;
				if (inputJson["type"] as string == "checkbox")
				{
					inputIDs.Add(inputJson["input_id"] as string);
				}
			}
			data.inputs = inputIDs;
			JsonList obj2 = challengeDic["legal_agreements"] as JsonList;
			List<LegalAgreementData> legalAgreements = new List<LegalAgreementData>();
			foreach (object item2 in obj2)
			{
				JsonNode agreementJson = item2 as JsonNode;
				LegalAgreementData agreementData = default(LegalAgreementData);
				agreementData.title = agreementJson["title"] as string;
				agreementData.externalURL = agreementJson["external_url"] as string;
				legalAgreements.Add(agreementData);
			}
			data.legalAgreements = legalAgreements;
			data.promptTitle = challengeDic["title"] as string;
			data.promptText = challengeDic["confirmation"] as string;
		}
		catch (Exception ex)
		{
			m_logger?.Log(LogLevel.Error, "Could not read values from legal challenge URL response, " + ex.Message);
			return false;
		}
		return true;
	}

	private static bool ReadLegalChallengeAcceptanceResponseData(string jsonText, out bool done)
	{
		done = false;
		try
		{
			JsonNode acceptanceResponse = Json.Deserialize(jsonText) as JsonNode;
			done = (bool)acceptanceResponse["done"];
		}
		catch (Exception ex)
		{
			m_logger?.Log(LogLevel.Error, "Could not read values from legal challenge URL acceptance response: " + ex.Message);
			return false;
		}
		return true;
	}

	private static LegalChallengeUIController GetLegalChallengeUIController()
	{
		return AssetLoader.Get().InstantiatePrefab("LegalChallengePopup.prefab:e77842ff28dd1504bb157b9ac656ade9").GetComponent<LegalChallengeUIController>();
	}
}
