using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;

namespace Hearthstone.APIGateway;

public class APIGatewayService : IService
{
	private HttpClient m_httpClient;

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
		m_httpClient = new HttpClient();
		OAuthManager = new OAuthManager(m_httpClient, Logger, ExceptionReporter.Get(), TelemetryManager.Client());
		yield break;
	}

	private void UpdateOauthConfig()
	{
		OAuthConfiguration oauthConfig = OAuthConfigFactory.CreateConfig((!ShouldUseInternalGateway()) ? OAuthConfigFactory.AuthEnvironment.PRODUCTION : OAuthConfigFactory.AuthEnvironment.QA, BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN);
		OAuthManager.UpdateConfiguration(oauthConfig);
	}

	public void Shutdown()
	{
		m_httpClient?.Dispose();
		m_httpClient = null;
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

	public Task<HttpResponseMessage> PostRequestAsync(string endpoint, HttpContent content = null)
	{
		return SafeSendAsync(HttpMethod.Post, endpoint, content);
	}

	public async Task<string> PostRequestAsStringAsync(string endpoint, HttpContent content = null)
	{
		using HttpResponseMessage response = await PostRequestAsync(endpoint, content).ConfigureAwait(continueOnCapturedContext: false);
		if (response == null)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to get post response to grab string");
			return null;
		}
		if (!response.IsSuccessStatusCode)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, $"Error on api gateway post {response.StatusCode}: {response.ReasonPhrase}");
			return null;
		}
		return await (response.Content?.ReadAsStringAsync());
	}

	public async Task<byte[]> PostRequestAsBytesAsync(string endpoint, HttpContent content = null)
	{
		using HttpResponseMessage response = await PostRequestAsync(endpoint, content).ConfigureAwait(continueOnCapturedContext: false);
		if (response == null)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to get post request to grab string");
			return null;
		}
		if (!response.IsSuccessStatusCode)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, $"Error getting api gateway post {response.StatusCode}: {response.ReasonPhrase}");
			return null;
		}
		return await (response.Content?.ReadAsByteArrayAsync());
	}

	public Task<HttpResponseMessage> GetRequestAsync(string endpoint)
	{
		return SafeSendAsync(HttpMethod.Get, endpoint);
	}

	public async Task<string> GetRequestAsStringAsync(string endpoint)
	{
		using HttpResponseMessage response = await GetRequestAsync(endpoint).ConfigureAwait(continueOnCapturedContext: false);
		if (response == null)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to get get request to grab string");
			return null;
		}
		if (!response.IsSuccessStatusCode)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, $"Error getting api gateway get {response.StatusCode}: {response.ReasonPhrase}");
			return null;
		}
		return await (response.Content?.ReadAsStringAsync());
	}

	public async Task<byte[]> GetRequestAsBytesAsync(string endpoint)
	{
		using HttpResponseMessage response = await GetRequestAsync(endpoint).ConfigureAwait(continueOnCapturedContext: false);
		if (response == null)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Failed to get get request to grab string");
			return null;
		}
		if (!response.IsSuccessStatusCode)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, $"Error getting api gateway get {response.StatusCode}: {response.ReasonPhrase}");
			return null;
		}
		return await (response.Content?.ReadAsByteArrayAsync());
	}

	private async Task<HttpResponseMessage> SafeSendAsync(HttpMethod method, string endpoint, HttpContent content = null)
	{
		_ = 1;
		try
		{
			string authToken = await GetAuthenticationTokenAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (string.IsNullOrEmpty(authToken))
			{
				return null;
			}
			using HttpRequestMessage request = new HttpRequestMessage();
			request.Method = method;
			request.RequestUri = new Uri(ConstructGatewayUrl(endpoint));
			request.Content = content;
			request.Headers.Add("Accept", "application/json");
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
			return await m_httpClient.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (InvalidOperationException ex)
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Warning, "APIGateway Request failed, invalid url: {0}", ex.Message);
			return null;
		}
		catch (HttpRequestException ex2)
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Warning, "APIGateway Request failed: {0}", ex2.Message);
			if (ex2.InnerException != null)
			{
				Logger.Log(Blizzard.T5.Core.LogLevel.Warning, ex2.InnerException.Message);
			}
			return null;
		}
		catch (TaskCanceledException ex3)
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Warning, "APIGateway Request timed out. {0}", ex3.Message);
			if (ex3.InnerException != null)
			{
				Logger.Log(Blizzard.T5.Core.LogLevel.Warning, ex3.InnerException.Message);
			}
			return null;
		}
		catch (Exception ex4)
		{
			Logger.Log(Blizzard.T5.Core.LogLevel.Warning, "Unexpected failure with APIGateway request " + ex4.Message);
			if (ex4.InnerException != null)
			{
				Logger.Log(Blizzard.T5.Core.LogLevel.Warning, ex4.InnerException.Message);
			}
			ExceptionReporter.Get()?.ReportCaughtException(ex4);
			return null;
		}
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
