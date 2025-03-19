using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using UnityEngine.Networking;

namespace Hearthstone.APIGateway;

public class APIGatewayService : IService
{
	private enum webRequestMethod
	{
		GET,
		POST
	}

	private const int WEB_REQUEST_TIMEOUT_SECONDS = 5;

	private OAuthManager OAuthManager { get; set; }

	private ILogger Logger { get; }

	public APIGatewayService(ILogger logger)
	{
		Logger = logger;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(LoginManager) };
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		OAuthManager = new OAuthManager(Logger, ExceptionReporter.Get(), TelemetryManager.Client());
		yield break;
	}

	private void UpdateOauthConfig()
	{
		OAuthConfiguration oauthConfig = OAuthConfigFactory.CreateConfig((!ShouldUseInternalGateway()) ? OAuthConfigFactory.AuthEnvironment.PRODUCTION : OAuthConfigFactory.AuthEnvironment.QA, BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN);
		OAuthManager.UpdateConfiguration(oauthConfig);
	}

	public void Shutdown()
	{
		OAuthManager = null;
	}

	public void AddRequiredScopes(IEnumerable<string> scopes)
	{
		OAuthManager.AddRequestedScopes(scopes);
	}

	public void OnLoginComplete()
	{
		UpdateOauthConfig();
		OAuthManager.ClearAccessToken();
	}

	public async Task<string> PostRequestAsStringAsync(string endpoint, string content = null)
	{
		string response = await SafeSendAsync(webRequestMethod.POST, endpoint, content).ConfigureAwait(continueOnCapturedContext: false);
		if (string.IsNullOrEmpty(response))
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to get post response to grab string");
			return null;
		}
		return response;
	}

	private async Task<string> SafeSendAsync(webRequestMethod method, string endpoint, string content = null)
	{
		string authToken = await GetAuthenticationTokenAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (method == webRequestMethod.POST)
		{
			string response = null;
			JobDefinition job = Processor.QueueJob("APIGateway SendUnityWebRequest", SendUnityWebRequest("POST", authToken, endpoint, content, delegate(string x)
			{
				response = x;
			}));
			WaitForJob dep = job.CreateDependency();
			while (!dep.IsReady())
			{
				await Task.Yield();
			}
			return response;
		}
		Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "APIGateway Request HttpMethod not supported");
		return null;
	}

	public IEnumerator<IAsyncJobResult> SendUnityWebRequest(string method, string authToken, string endpoint, string content = null, Action<string> onComplete = null)
	{
		if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(endpoint))
		{
			yield break;
		}
		UnityWebRequest request = new UnityWebRequest(new Uri(ConstructGatewayUrl(endpoint)));
		request.method = method;
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader("Accept", "application/json");
		request.SetRequestHeader("Authorization", "Bearer " + authToken);
		request.downloadHandler = new DownloadHandlerBuffer();
		if (content != null)
		{
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(content));
		}
		SimpleTimer requestTimer = new SimpleTimer(new TimeSpan(0, 0, 5));
		request.SendWebRequest();
		while (!request.isDone)
		{
			if (requestTimer.IsTimeout())
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "APIGateway Request timed out");
				onComplete?.Invoke(null);
				yield break;
			}
			yield return null;
		}
		if (request.result != UnityWebRequest.Result.Success)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "APIGateway Request error: " + request.error);
		}
		else if (string.IsNullOrEmpty(request.downloadHandler.text))
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Could not complete oauth: response null or empty");
		}
		string response = request.downloadHandler.text;
		onComplete?.Invoke(response);
		request.disposeDownloadHandlerOnDispose = true;
		request.disposeUploadHandlerOnDispose = true;
		request.Dispose();
	}

	private string ConstructGatewayUrl(string endpoint)
	{
		return GetBaseGatewayUrl() + "/" + endpoint;
	}

	private string GetBaseGatewayUrl()
	{
		if (ShouldUseInternalGateway())
		{
			return GetInternalBaseGatewayUrl();
		}
		return GetPublicBaseGatewayUrl();
	}

	private bool ShouldUseInternalGateway()
	{
		string envOverride = Vars.Key("Debug.GatewayEnv").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(envOverride))
		{
			if (envOverride.Equals("qa", StringComparison.OrdinalIgnoreCase))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Using Gateway override QA");
				return true;
			}
			if (envOverride.StartsWith("prod", StringComparison.OrdinalIgnoreCase))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Using Gateway override Prod");
				return false;
			}
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Unknown Gateway override " + envOverride);
		}
		return HearthstoneApplication.IsInternal();
	}

	private string GetInternalBaseGatewayUrl()
	{
		return string.Empty;
	}

	private string GetPublicBaseGatewayUrl()
	{
		if (BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN)
		{
			return "https://gateway.battlenet.com.cn/";
		}
		return "https://api.blizzard.com/";
	}

	private async Task<string> GetAuthenticationTokenAsync()
	{
		if (!Network.IsLoggedIn())
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Attempted to use oauth when not logged into an account");
			return null;
		}
		if (!OAuthManager.IsAuthenticated)
		{
			string ssoToken = await GetAppWebSSOTokenAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (string.IsNullOrEmpty(ssoToken))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Could not generate app sso token");
				return null;
			}
			bool authSuccess = false;
			JobDefinition job = Processor.QueueJob("PerformAuthenticationAsync", OAuthManager.PerformAuthenticationAsync(ssoToken, delegate(bool x)
			{
				authSuccess = x;
			}));
			WaitForJob dep = job.CreateDependency();
			while (!dep.IsReady())
			{
				await Task.Yield();
			}
			if (!authSuccess)
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to authenticate for oauth access token");
				return null;
			}
		}
		return OAuthManager.AuthToken;
	}

	private Task<string> GetAppWebSSOTokenAsync()
	{
		TaskCompletionSource<string> completionSource = new TaskCompletionSource<string>();
		BattleNet.Get().GenerateAppWebCredentials(delegate(bool success, string token)
		{
			string result = (success ? token : string.Empty);
			completionSource.TrySetResult(result);
		});
		return completionSource.Task;
	}
}
