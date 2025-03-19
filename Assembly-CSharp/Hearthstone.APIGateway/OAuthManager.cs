using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Extensions;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.Telemetry.WTCG.Client;
using HearthstoneTelemetry;
using MiniJSON;
using UnityEngine.Networking;

namespace Hearthstone.APIGateway;

internal class OAuthManager
{
	private HttpClient m_httpClient;

	private readonly ITelemetryClient m_telemetryClient;

	private HashSet<string> m_requiredScopes = new HashSet<string>();

	private const int WEB_REQUEST_TIMEOUT_SECONDS = 5;

	public string AuthToken { get; private set; }

	public DateTime ExpirationTime { get; private set; }

	public bool IsAuthenticated
	{
		get
		{
			if (string.IsNullOrEmpty(AuthToken))
			{
				return !IsTokenExpired();
			}
			return false;
		}
	}

	private OAuthConfiguration Config { get; set; }

	private ILogger Logger { get; }

	public ExceptionReporter ExceptionReporter { get; private set; }

	public OAuthManager(HttpClient httpClient, ILogger logger, ExceptionReporter exceptionReporter, ITelemetryClient telemetryClient)
	{
		Logger = logger;
		m_httpClient = httpClient;
		m_telemetryClient = telemetryClient;
		ExceptionReporter = exceptionReporter;
	}

	public void AddRequestedScopes(IEnumerable<string> scopes)
	{
		bool requireClearAccessToken = false;
		foreach (string scope in scopes)
		{
			if (!m_requiredScopes.Contains(scope))
			{
				m_requiredScopes.Add(scope);
				requireClearAccessToken = true;
			}
		}
		if (requireClearAccessToken)
		{
			ClearAccessToken();
		}
	}

	public void ClearAccessToken()
	{
		AuthToken = null;
		ExpirationTime = DateTime.Now;
	}

	public void UpdateConfiguration(OAuthConfiguration config)
	{
		if (!config.Equals(Config))
		{
			Config = config;
			ClearAccessToken();
		}
	}

	public IEnumerator<IAsyncJobResult> PerformAuthenticationAsync(string ssoToken, Action<bool> onComplete)
	{
		if (!Config.IsValid)
		{
			Logger.Log(LogLevel.Warning, "Attempted to perform OAuth authentication without a valid configuration.");
			yield break;
		}
		string requestUrl = ConstructAuthRequestUrl(ssoToken);
		UnityWebRequest request = new UnityWebRequest(requestUrl);
		request.SetRequestHeader("Accept", "application/json");
		request.method = "POST";
		request.downloadHandler = new DownloadHandlerBuffer();
		SimpleTimer requestTimer = new SimpleTimer(new TimeSpan(0, 0, 5));
		request.SendWebRequest();
		while (!request.isDone)
		{
			if (requestTimer.IsTimeout())
			{
				Logger?.Log(LogLevel.Information, "Could not complete oauth: TIMEOUT");
				ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_TIMEOUT, HttpStatusCode.RequestTimeout.ToInt());
				yield break;
			}
			yield return null;
		}
		bool parseSuccess = false;
		if (request.result == UnityWebRequest.Result.Success)
		{
			if (!string.IsNullOrEmpty(request.downloadHandler.text))
			{
				parseSuccess = SafeParseAuthResponseJson(request.downloadHandler.text);
				Logger?.Log(LogLevel.Information, $"PerformAuthenticationAsync {parseSuccess}");
				ReportResultToTelemetry((!parseSuccess) ? OAuthResult.AuthResult.FAILURE_JSON_PARSE : OAuthResult.AuthResult.SUCCESS);
			}
			else
			{
				Logger?.Log(LogLevel.Information, "Could not complete oauth: response null or empty");
				ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_STATUS_CODE, HttpStatusCode.NoContent.ToInt());
			}
		}
		else
		{
			Logger?.Log(LogLevel.Information, "Could not complete oauth: " + request.error);
			ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_STATUS_CODE, HttpStatusCode.BadRequest.ToInt());
		}
		request.disposeDownloadHandlerOnDispose = true;
		request.Dispose();
		onComplete?.Invoke(parseSuccess);
	}

	private bool SafeParseAuthResponseJson(string jsonResponse)
	{
		try
		{
			JsonNode jsonNode = Json.Deserialize(jsonResponse) as JsonNode;
			if (jsonNode.ContainsKey("access_token"))
			{
				string accesToken = jsonNode["access_token"] as string;
				long expiresInSeconds = (long)jsonNode["expires_in"];
				DateTime expireDateTime = DateTime.Now.AddSeconds(expiresInSeconds);
				AuthToken = accesToken;
				ExpirationTime = expireDateTime;
				Logger?.Log(LogLevel.Debug, $"Updated OAuth token with expiration {ExpirationTime}");
				return true;
			}
			if (jsonNode.ContainsKey("error"))
			{
				string reason = (jsonNode.ContainsKey("error_description") ? jsonNode["error_description"].ToString() : "OAuth error response");
				Logger?.Log(LogLevel.Error, "Updating OAuth token failed: " + reason);
				throw new Exception(reason);
			}
			throw new Exception("Could not parse oauth response");
		}
		catch (Exception ex)
		{
			Logger?.Log(LogLevel.Warning, "Updating OAuth token failed: " + ex.Message);
			ExceptionReporter?.ReportCaughtException(ex);
			return false;
		}
	}

	private async Task<HttpResponseMessage> SafePostAsync(string url)
	{
		Logger?.Log(LogLevel.Information, "SafePostAsync " + url);
		try
		{
			using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
			requestMessage.Headers.Add("Accept", "application/json");
			HttpResponseMessage obj = await m_httpClient.SendAsync(requestMessage).ConfigureAwait(continueOnCapturedContext: false);
			if (obj.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception("Unexpected failure requesting oauth token for URL: " + url);
			}
			return obj;
		}
		catch (InvalidOperationException ex)
		{
			Logger?.Log(LogLevel.Warning, "Could not request oauth token. Invalid url. {0}", ex.Message);
			Logger?.Log(LogLevel.Debug, url);
			ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_INVALID_OP_EXCEPTION);
			return null;
		}
		catch (HttpRequestException ex2)
		{
			Logger?.Log(LogLevel.Warning, "Oauth token request failed. {0}", ex2.Message);
			LogInnerExceptions(LogLevel.Warning, ex2);
			ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_HTTP_EXCEPTION);
			return null;
		}
		catch (TaskCanceledException ex3)
		{
			Logger?.Log(LogLevel.Warning, "Oauth token request timed out. {0}", ex3.Message);
			LogInnerExceptions(LogLevel.Warning, ex3);
			ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_TIMEOUT);
			return null;
		}
		catch (Exception ex4)
		{
			Logger?.Log(LogLevel.Error, "Unexpected failure requesting oauth token, message: " + ex4.Message);
			LogInnerExceptions(LogLevel.Warning, ex4);
			ExceptionReporter?.ReportCaughtException(ex4);
			ReportResultToTelemetry(OAuthResult.AuthResult.FAILURE_EXCEPTION);
			return null;
		}
	}

	private void LogInnerExceptions(LogLevel level, Exception e)
	{
		if (e != null && e.InnerException != null)
		{
			Logger?.Log(level, "Inner exception: " + e.InnerException.Message);
			LogInnerExceptions(level, e.InnerException);
		}
	}

	private bool IsTokenExpired()
	{
		return DateTime.Now >= ExpirationTime;
	}

	private string ConstructAuthRequestUrl(string ssoToken)
	{
		string scopeParam = string.Empty;
		if (m_requiredScopes.Count > 0)
		{
			string scopes = string.Join(" ", m_requiredScopes);
			scopes = WebUtility.UrlEncode(scopes);
			scopeParam = "&scope=" + scopes;
		}
		string safeClientID = WebUtility.UrlEncode(Config.ClientID);
		string safeSsoToken = WebUtility.UrlEncode(ssoToken);
		return Config.AuthEndpointURL + "?client_id=" + safeClientID + "&token=" + safeSsoToken + "&grant_type=client_sso" + scopeParam;
	}

	private void ReportResultToTelemetry(OAuthResult.AuthResult result, int statusCode = -1)
	{
		OAuthResult resultMessage = new OAuthResult
		{
			Result = result
		};
		if (statusCode > 0)
		{
			resultMessage.ErrorStatusCode = statusCode;
		}
		m_telemetryClient.EnqueueMessage(resultMessage);
	}
}
