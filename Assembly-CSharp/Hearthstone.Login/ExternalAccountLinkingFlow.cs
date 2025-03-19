using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Core;
using Hearthstone.Login.UI;
using MiniJSON;
using UnityEngine.Networking;

namespace Hearthstone.Login;

public class ExternalAccountLinkingFlow
{
	private struct ChallengePostResponseData
	{
		public string userCode;

		public string deviceCode;

		public string verificationUri;

		public string verificationUriWithCode;

		public string pollingUri;

		public int pollingIntervalSeconds;
	}

	private static AccountLinkingUIController m_currentUIController;

	private static int m_numberOfLinkingTries;

	private static ExternalAccountLinkingState.Status m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.UNKNOWN_ERROR;

	public static void StartLinkingAccount(string challengeUrl, Action onProvideToken, ulong externalAccountID, ILogger Logger)
	{
		if (m_numberOfLinkingTries > 10)
		{
			m_currentUIController?.Hide();
			FatalErrorMgr.Get().ClearAllErrors();
			Error.AddFatal(new ErrorParams
			{
				m_type = ErrorType.FATAL,
				m_reason = FatalErrorReason.ACCOUNT_SETUP_ERROR,
				m_message = GameStrings.Format("GLOBAL_ERROR_STEAM_ACCOUNT_LINKING")
			});
			TelemetryManager.Client().SendExternalAccountLinkingState(m_lastAccountLinkingStatus, externalAccountID);
		}
		else
		{
			m_numberOfLinkingTries++;
			LoginManager loginManager = ServiceManager.Get<LoginManager>();
			loginManager.OnLoginCompleted -= delegate
			{
				OnLoginCompleted(externalAccountID, loginManager);
			};
			loginManager.OnLoginCompleted += delegate
			{
				OnLoginCompleted(externalAccountID, loginManager);
			};
			Processor.QueueJobIfNotExist("Job_ProcessLinkingExternalAccount", Job_ProcessLinkingExternalAccount(challengeUrl, onProvideToken, externalAccountID, Logger));
		}
	}

	private static void OnLoginCompleted(ulong externalAccountID, LoginManager loginManager)
	{
		if (m_lastAccountLinkingStatus == ExternalAccountLinkingState.Status.SUCCESS)
		{
			TelemetryManager.Client().SendExternalAccountLinkingState(ExternalAccountLinkingState.Status.SUCCESS, externalAccountID);
		}
		loginManager.OnLoginCompleted -= delegate
		{
			OnLoginCompleted(externalAccountID, loginManager);
		};
	}

	private static IEnumerator<IAsyncJobResult> Job_ProcessLinkingExternalAccount(string challengeUrl, Action onProvideToken, ulong externalAccountID, ILogger logger)
	{
		if (m_currentUIController == null)
		{
			m_currentUIController = GetAccountLinkingUIController();
		}
		UnityWebRequest request = CreateChallengePostWebRequest(challengeUrl);
		request.SendWebRequest();
		SimpleTimer requestTimer = new SimpleTimer(new TimeSpan(0, 0, 5));
		while (!request.isDone)
		{
			if (requestTimer.IsTimeout())
			{
				m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.CHALLENGE_URL_NO_RESPONSE;
				StartLinkingAccount(challengeUrl, onProvideToken, externalAccountID, logger);
				yield break;
			}
			yield return null;
		}
		if (request.result != UnityWebRequest.Result.Success)
		{
			logger?.Log(LogLevel.Error, "Could not post to external challenge URL, restarting the process: " + request.error);
			m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.CHALLENGE_URL_POST_ERROR;
			StartLinkingAccount(challengeUrl, onProvideToken, externalAccountID, logger);
			yield break;
		}
		if (!ReadChallengePostResponseData(request.downloadHandler.text, logger, out var responseData))
		{
			m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.CHALLENGE_URL_RESPONSE_ERROR;
			StartLinkingAccount(challengeUrl, onProvideToken, externalAccountID, logger);
			yield break;
		}
		m_currentUIController.ShowAccountLinkingUI(responseData.userCode, responseData.verificationUri, responseData.verificationUriWithCode);
		string jsonBody = "{\"device_code\":\"" + responseData.deviceCode + "\"}";
		SimpleTimer pollingTimer = new SimpleTimer(new TimeSpan(0, 0, responseData.pollingIntervalSeconds));
		for (int maxPollingTries = 0; 100 > maxPollingTries; maxPollingTries++)
		{
			UnityWebRequest pollingRequest = CreatePollingWebRequest(responseData.pollingUri, jsonBody);
			pollingRequest.SendWebRequest();
			requestTimer.Reset();
			while (!pollingRequest.isDone)
			{
				if (requestTimer.IsTimeout())
				{
					m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.POLLING_NO_RESPONSE;
					yield break;
				}
				yield return null;
			}
			pollingTimer.Reset();
			if (pollingRequest.responseCode == 200)
			{
				m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.SUCCESS;
				logger?.Log(LogLevel.Debug, "Account linking success");
				m_currentUIController.OnAccountLinkingComplete();
				onProvideToken?.Invoke();
				break;
			}
			try
			{
				string status = (Json.Deserialize(pollingRequest.downloadHandler.text) as JsonNode)["cause"] as string;
				logger?.Log(LogLevel.Debug, "Account linking status: " + status);
				if (status == "slow_down")
				{
					pollingTimer.SetTimeout(new TimeSpan(0, 0, responseData.pollingIntervalSeconds++));
				}
			}
			catch
			{
				logger?.Log(LogLevel.Debug, "Could not get values from account linking polling response. This is not critical. Account linking is still pending. ");
			}
			if (pollingRequest.responseCode == 400)
			{
				logger?.Log(LogLevel.Debug, "Account linking token expired. requesting a new token");
				m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.TOKEN_EXPIRED;
				StartLinkingAccount(challengeUrl, onProvideToken, externalAccountID, logger);
				break;
			}
			if (pollingRequest.responseCode == 403)
			{
				m_lastAccountLinkingStatus = ExternalAccountLinkingState.Status.LINKING_PENDING;
			}
			while (!pollingTimer.IsTimeout())
			{
				yield return null;
			}
		}
	}

	private static UnityWebRequest CreatePollingWebRequest(string pollingUri, string jsonBody)
	{
		UnityWebRequest unityWebRequest = UnityWebRequest.Put(pollingUri, jsonBody);
		unityWebRequest.method = "POST";
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("accept", "application/json");
		unityWebRequest.SetRequestHeader("accept-encoding", "gzip, deflate, br");
		return unityWebRequest;
	}

	private static UnityWebRequest CreateChallengePostWebRequest(string challengeUrl)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(challengeUrl, "POST");
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("accept", "application/json");
		unityWebRequest.SetRequestHeader("accept-encoding", "gzip, deflate, br");
		return unityWebRequest;
	}

	private static AccountLinkingUIController GetAccountLinkingUIController()
	{
		return AssetLoader.Get().InstantiatePrefab("SteamAccountLinkingPopup.prefab:1b4ef2a2f8d7e934eae87d17ecfac9c8").GetComponent<AccountLinkingUIController>();
	}

	private static bool ReadChallengePostResponseData(string jsonText, ILogger logger, out ChallengePostResponseData data)
	{
		data = default(ChallengePostResponseData);
		try
		{
			JsonNode challengeURLResponse = Json.Deserialize(jsonText) as JsonNode;
			data.userCode = challengeURLResponse["user_code"] as string;
			data.deviceCode = challengeURLResponse["device_code"] as string;
			data.verificationUri = challengeURLResponse["verification_uri"] as string;
			data.verificationUriWithCode = challengeURLResponse["verification_uri_with_code"] as string;
			data.pollingUri = challengeURLResponse["polling_uri"] as string;
			data.pollingIntervalSeconds = int.Parse(challengeURLResponse["interval_seconds"].ToString());
			logger?.Log(LogLevel.Debug, "External challenge URL post results: user_code: " + data.userCode + ", device_code: " + data.deviceCode + ", verification_uri: " + data.verificationUri);
		}
		catch (Exception ex)
		{
			logger?.Log(LogLevel.Error, "Could not get values from post to external challenge URL, restarting the process: " + ex.Message);
			return false;
		}
		return true;
	}
}
