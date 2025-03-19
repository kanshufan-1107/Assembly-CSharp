using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Services;
using Hearthstone.APIGateway;
using Hearthstone.Core;
using MiniJSON;

namespace Hearthstone.InGameMessage.Personalization;

public class GIGateway
{
	private bool m_hasInitializedGatewayScopes;

	private const string GI_EVENT_DISPATCHER_SCOPE = "gi.eventdispatcher";

	private const string GI_GATEWAY_ENDPOINT = "gi/liveservice/ed/eventdispatcher.Event/DispatchAll";

	private const string JSON_CONTENT_TYPE = "application/json";

	private ILogger Logger { get; }

	public GIGateway(ILogger logger)
	{
		Logger = logger;
	}

	public async void GetMessages(Action<string[]> onMessageRetreived)
	{
		if (!Network.IsLoggedIn())
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Attempted to get personalized messages while not logged in");
			onMessageRetreived(null);
		}
		if (ServiceManager.TryGet<APIGatewayService>(out var gateway))
		{
			onMessageRetreived(await GetMessagesFromGateway(gateway));
			return;
		}
		Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Could not get personalized messages, API Gateway service not available");
		onMessageRetreived(null);
	}

	private async Task<string[]> GetMessagesFromGateway(APIGatewayService gateway)
	{
		try
		{
			InitializeScopesIfNeeded(gateway);
			using StringContent content = new StringContent(ConstructJsonContentString());
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			string response = await gateway.PostRequestAsStringAsync("gi/liveservice/ed/eventdispatcher.Event/DispatchAll", content).ConfigureAwait(continueOnCapturedContext: false);
			if (string.IsNullOrEmpty(response))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Unable to get messages from Gateway. Null or empty response");
				ReportMessageRetrivalResult(success: false);
				return null;
			}
			return ExtractMessagesFromResponseJSON(response);
		}
		catch (Exception ex)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Exception retrieving messages from gateway: " + ex.Message);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
			return null;
		}
	}

	private void InitializeScopesIfNeeded(APIGatewayService gateway)
	{
		if (!m_hasInitializedGatewayScopes)
		{
			gateway.AddRequiredScopes(new List<string> { "gi.eventdispatcher" });
			m_hasInitializedGatewayScopes = true;
		}
	}

	private static string ConstructJsonContentString()
	{
		return Json.Serialize(new JsonNode
		{
			{
				"bnetAccountId",
				BattleNet.Get().GetMyAccountId().Low
			},
			{ "franchise", "hearthstone" }
		});
	}

	private string[] ExtractMessagesFromResponseJSON(string responseJson)
	{
		try
		{
			List<string> messageIds = new List<string>();
			foreach (object item in (Json.Deserialize(responseJson) as JsonNode)["events"] as JsonList)
			{
				string message = (item as JsonNode)["message"] as string;
				messageIds.Add(message);
			}
			ReportMessageRetrivalResult(success: true, messageIds);
			return messageIds.ToArray();
		}
		catch (Exception ex)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Failed to parse GI Message response: " + ex.Message);
			ReportMessageRetrivalResult(success: false);
			ExceptionReporter.Get()?.ReportCaughtException(ex);
			return null;
		}
	}

	private void ReportMessageRetrivalResult(bool success, List<string> messageIds = null)
	{
		Processor.ScheduleCallback(0f, realTime: true, delegate
		{
			messageIds = messageIds ?? new List<string>();
			int count = messageIds.Count;
			TelemetryManager.Client().SendPersonalizedMessagesResult(success, count, messageIds);
		});
	}
}
