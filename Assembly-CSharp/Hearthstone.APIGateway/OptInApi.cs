using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using MiniJSON;

namespace Hearthstone.APIGateway;

public class OptInApi
{
	public enum OptInType
	{
		INVALID = -1,
		DISABLE_CHAT = 35,
		DISABLE_PERSONALIZED_PRODUCTS = 37,
		DISABLE_PUSH_NOTIFICATIONS = 38,
		DISABLE_NEARBY_FRIENDS = 39,
		DISABLE_AB_TESTING = 40
	}

	private enum TimeStampUpdateType
	{
		NONE = -1,
		LOCAL = 1,
		LOCAL_AND_SERVER = 2
	}

	private bool m_hasInitializedGatewayScopes;

	private const string GET_ACCOUNT_OPT_INS_REQUEST = "OptInService/v1/GetAccountOptIns";

	private const string UPDATE_ACCOUNT_OPT_IN_REQUEST = "OptInService/v1/UpdateAccountOptIn";

	private const string JSON_CONTENT_TYPE = "application/json";

	private static string[] OPT_IN_SCOPES = new string[8] { "reference.full", "account.standard", "account.marketing.privileged", "account.standard.privileged:modify", "account.standard.privileged", "account.marketing.privileged:modify", "account.standard:modify", "account.full.internal" };

	private const int GET_ACCOUNT_OPT_INS_RETRY_ATTEMPTS = 5;

	private bool m_serverOutofSync;

	private object m_optionsLock = new object();

	private Dictionary<OptInType, bool> m_cachedOptIns;

	private Dictionary<OptInType, Option> m_optInTypeToOption = new Dictionary<OptInType, Option>
	{
		[OptInType.DISABLE_CHAT] = Option.AADC_DISABLE_CHAT,
		[OptInType.DISABLE_PERSONALIZED_PRODUCTS] = Option.AADC_DISABLE_PERSONALIZED_PRODUCTS,
		[OptInType.DISABLE_PUSH_NOTIFICATIONS] = Option.AADC_PUSH_NOTIFICATIONS,
		[OptInType.DISABLE_NEARBY_FRIENDS] = Option.AADC_DISABLE_NEARBY_FRIENDS,
		[OptInType.DISABLE_AB_TESTING] = Option.AADC_DISABLE_AB_TESTING
	};

	private APIGatewayService APIGatewayService { get; }

	private ILogger Logger { get; }

	public OptInApi(APIGatewayService apiGatewayService, ILogger logger)
	{
		APIGatewayService = apiGatewayService;
		Logger = logger;
		m_cachedOptIns = new Dictionary<OptInType, bool>();
	}

	private void InitializeScopesIfNeeded(APIGatewayService gateway)
	{
		if (!m_hasInitializedGatewayScopes)
		{
			gateway.AddRequiredScopes(OPT_IN_SCOPES);
			m_hasInitializedGatewayScopes = true;
		}
	}

	public async void Init(Action onOptInInitializationComplete)
	{
		int initNumAttemptsCount = 0;
		bool optInInitializationSuccess;
		do
		{
			initNumAttemptsCount++;
			optInInitializationSuccess = await GetAccountOptInsByAccountId();
		}
		while (initNumAttemptsCount < 5 && !optInInitializationSuccess);
		if (optInInitializationSuccess)
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Information, "GetAccountOptInsByAccountId fetch succeeded.");
			OnServerOptInDataReceived();
		}
		else
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Error, "GetAccountOptInsByAccountId fetch failed, falling back to AADC compliant settings.");
			foreach (OptInType type in Enum.GetValues(typeof(OptInType)))
			{
				m_cachedOptIns[type] = true;
			}
			OnServerOptInDataNotAvailable();
		}
		onOptInInitializationComplete?.Invoke();
	}

	public bool GetAccountOptIn(OptInType type)
	{
		if (m_cachedOptIns != null && m_cachedOptIns.TryGetValue(type, out var value))
		{
			return value;
		}
		Logger.Log(Blizzard.T5.Core.LogLevel.Error, "No OptIn value found for type {0}.", type.ToString());
		return false;
	}

	public void SetAccountOptIn(OptInType type, bool value)
	{
		Task.Run(async delegate
		{
			m_cachedOptIns[type] = value;
			bool success = await UpdateAccountOptInByAccountId(type, value);
			if (!success)
			{
				Logger.Log(Blizzard.T5.Core.LogLevel.Error, $"Could not update OptIn server for {type}");
			}
			SetOptionsOptInFeature(type, success);
		});
	}

	public void UncachedSetOptInAsync(int type, bool value, Action<bool> callback)
	{
		UpdateAccountOptInByAccountId((OptInType)type, value).ContinueWith(delegate(Task<bool> task)
		{
			callback?.Invoke(task.IsCompletedSuccessfully && task.Result);
		});
	}

	private void OnServerOptInDataNotAvailable()
	{
		SetOptInFromOptions();
		UpdateTimeStamps(TimeStampUpdateType.LOCAL);
	}

	private void OnServerOptInDataReceived()
	{
		DateTime localSaveTimeStamp = default(DateTime);
		DateTime serverSaveTimeStamp = default(DateTime);
		lock (m_optionsLock)
		{
			localSaveTimeStamp = TimeUtils.UnixTimeStampMillisecondsToDateTimeUtc(Convert.ToInt64(Options.Get().GetULong(Option.AADC_LOCAL_SAVE_TIME_STAMP)));
			serverSaveTimeStamp = TimeUtils.UnixTimeStampMillisecondsToDateTimeUtc(Convert.ToInt64(Options.Get().GetULong(Option.AADC_SERVER_SAVE_TIME_STAMP)));
		}
		if (localSaveTimeStamp > serverSaveTimeStamp)
		{
			SetOptInFromOptions();
			UpdateServerOptInData();
		}
		else if (localSaveTimeStamp == serverSaveTimeStamp)
		{
			UpdateOptionsOptInData();
			UpdateTimeStamps(TimeStampUpdateType.LOCAL_AND_SERVER);
		}
		else
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Error, $"OnOptInServerDataReceived localSaveTimeStamp {localSaveTimeStamp} is behind serverSaveTimeStamp {serverSaveTimeStamp}. Something went wrong");
		}
	}

	private void SetOptionsOptInFeature(OptInType type, bool setServerSuccessful)
	{
		if (setServerSuccessful)
		{
			lock (m_optionsLock)
			{
				SetDeviceOptions(type);
			}
			if (m_serverOutofSync)
			{
				UpdateTimeStamps(TimeStampUpdateType.LOCAL);
			}
			else
			{
				UpdateTimeStamps(TimeStampUpdateType.LOCAL_AND_SERVER);
			}
		}
		else
		{
			m_serverOutofSync = true;
			lock (m_optionsLock)
			{
				SetDeviceOptions(type);
			}
			UpdateTimeStamps(TimeStampUpdateType.LOCAL);
		}
	}

	private void UpdateOptionsOptInData()
	{
		lock (m_optionsLock)
		{
			foreach (OptInType type in Enum.GetValues(typeof(OptInType)))
			{
				if (type != OptInType.INVALID && !SetDeviceOptions(type))
				{
					Logger.Log(Blizzard.T5.Core.LogLevel.Error, "No server OptIn value found for type {0}.", type.ToString());
					m_cachedOptIns[type] = Options.Get().GetBool(m_optInTypeToOption[type]);
				}
			}
		}
	}

	private void UpdateServerOptInData()
	{
		foreach (OptInType type in Enum.GetValues(typeof(OptInType)))
		{
			if (type != OptInType.INVALID)
			{
				SetAccountOptIn(type, m_cachedOptIns[type]);
			}
		}
	}

	private void SetOptInFromOptions()
	{
		lock (m_optionsLock)
		{
			foreach (OptInType type in Enum.GetValues(typeof(OptInType)))
			{
				if (type != OptInType.INVALID)
				{
					m_cachedOptIns[type] = Options.Get().GetBool(m_optInTypeToOption[type]);
				}
			}
		}
	}

	private bool SetDeviceOptions(OptInType type)
	{
		if (m_cachedOptIns.TryGetValue(type, out var value))
		{
			Options.Get().SetBool(m_optInTypeToOption[type], value);
			return true;
		}
		return false;
	}

	private void UpdateTimeStamps(TimeStampUpdateType timeStampUpdateType)
	{
		lock (m_optionsLock)
		{
			switch (timeStampUpdateType)
			{
			case TimeStampUpdateType.LOCAL:
				Options.Get().SetULong(Option.AADC_LOCAL_SAVE_TIME_STAMP, TimeUtils.DateTimeToUnixTimeStampMilliseconds(DateTime.UtcNow));
				break;
			case TimeStampUpdateType.LOCAL_AND_SERVER:
			{
				DateTime utcNow = DateTime.UtcNow;
				Options.Get().SetULong(Option.AADC_LOCAL_SAVE_TIME_STAMP, TimeUtils.DateTimeToUnixTimeStampMilliseconds(utcNow));
				Options.Get().SetULong(Option.AADC_SERVER_SAVE_TIME_STAMP, TimeUtils.DateTimeToUnixTimeStampMilliseconds(utcNow));
				break;
			}
			default:
				Logger.Log(Blizzard.T5.Core.LogLevel.Warning, "No timestamp update selected. Something went wrong.");
				break;
			}
		}
	}

	private async Task<bool> GetAccountOptInsByAccountId()
	{
		try
		{
			InitializeScopesIfNeeded(APIGatewayService);
			string content = ConstructGetAccountOptInsByAccountIdRequest();
			string response = await APIGatewayService.PostRequestAsStringAsync("OptInService/v1/GetAccountOptIns", content).ConfigureAwait(continueOnCapturedContext: false);
			if (string.IsNullOrEmpty(response))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Unable to get messages from Gateway. Null or empty response");
				return false;
			}
			return TryExtractOptInsFromResponseJSON(response);
		}
		catch (Exception ex)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Exception retrieving messages from gateway: " + ex.Message);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
			return false;
		}
	}

	private async Task<bool> UpdateAccountOptInByAccountId(OptInType optInType, bool value)
	{
		try
		{
			InitializeScopesIfNeeded(APIGatewayService);
			string content = ConstructUpdateAccountOptInRequest(optInType, value);
			string value2 = await APIGatewayService.PostRequestAsStringAsync("OptInService/v1/UpdateAccountOptIn", content).ConfigureAwait(continueOnCapturedContext: false);
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, $"{optInType} opt-in updated to {value}.");
			if (string.IsNullOrEmpty(value2))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Unable to get messages from Gateway. Null or empty response");
				return false;
			}
		}
		catch (Exception ex)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Exception retrieving messages from gateway: " + ex.Message);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
			return false;
		}
		return true;
	}

	private string ConstructGetAccountOptInsByAccountIdRequest()
	{
		return Json.Serialize(new JsonNode { 
		{
			"accountId",
			BattleNet.Get().GetMyAccountId().Low
		} });
	}

	private string ConstructUpdateAccountOptInRequest(OptInType optInType, bool value)
	{
		return Json.Serialize(new JsonNode
		{
			{
				"accountId",
				BattleNet.Get().GetMyAccountId().Low
			},
			{
				"typeId",
				(int)optInType
			},
			{ "value", value }
		});
	}

	private bool TryExtractOptInsFromResponseJSON(string responseJson)
	{
		try
		{
			JsonNode node = Json.Deserialize(responseJson) as JsonNode;
			string error = node["error"] as string;
			if (!string.IsNullOrEmpty(error))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Error encountered while fetching opt-ins: " + error);
				return false;
			}
			foreach (object item in node["optInSelections"] as JsonList)
			{
				JsonNode obj = item as JsonNode;
				int typeId = Convert.ToInt32(obj["typeId"]);
				bool isOptedIn = Convert.ToBoolean(obj["isOptedIn"]);
				if (Enum.IsDefined(typeof(OptInType), typeId))
				{
					m_cachedOptIns[(OptInType)typeId] = isOptedIn;
				}
			}
		}
		catch (Exception ex)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Failed to parse response: " + ex.Message);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
			return false;
		}
		return true;
	}
}
