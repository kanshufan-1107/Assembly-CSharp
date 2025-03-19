using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Http;
using UnityEngine;

namespace Hearthstone.BreakingNews;

public class BreakingNews : IService
{
	public enum Status
	{
		Fetching,
		Available,
		TimedOut
	}

	public delegate void BreakingNewsRecievedDelegate(string response, bool error);

	private Status m_status;

	private string m_text = "";

	private string m_error;

	private float m_timeFetched;

	private ExternalUrlService m_urlService;

	private readonly Blizzard.T5.Core.ILogger m_logger;

	private readonly IHttpRequestFactory m_httpRequestFactory;

	private const float TIMEOUT = 15f;

	public bool ShouldShowForCurrentPlatform => PlatformSettings.IsMobile();

	public BreakingNews(Blizzard.T5.Core.ILogger logger, IHttpRequestFactory httpRequestFactory)
	{
		m_logger = logger;
		m_httpRequestFactory = httpRequestFactory;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_urlService = serviceLocator.Get<ExternalUrlService>();
		if (m_urlService == null)
		{
			m_logger.Log(LogLevel.Warning, "Breaking News error : ExternalUrlService null");
			m_error = "Breaking News error : ExternalUrlService null";
		}
		else
		{
			Fetch();
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(ExternalUrlService) };
	}

	public void Shutdown()
	{
	}

	private void FetchBreakingNews(string url, BreakingNewsRecievedDelegate callback)
	{
		Processor.RunCoroutine(FetchBreakingNewsProgress(url, callback));
	}

	private IEnumerator FetchBreakingNewsProgress(string url, BreakingNewsRecievedDelegate callback)
	{
		if (string.IsNullOrEmpty(url))
		{
			m_logger.Log(LogLevel.Warning, "Breaking News fetch url is null/empty");
			m_error = "Breaking News fetch url is null/empty";
			yield break;
		}
		using IHttpRequest webRequest = m_httpRequestFactory.CreateGetRequest(url);
		yield return webRequest.SendRequest();
		if (webRequest.IsNetworkError || webRequest.IsHttpError)
		{
			callback(webRequest.ErrorString, error: true);
		}
		else
		{
			callback(webRequest.ResponseAsString, error: false);
		}
	}

	public Status GetStatus()
	{
		if (!ShouldShowForCurrentPlatform)
		{
			return Status.Available;
		}
		if (m_status == Status.Fetching && Time.realtimeSinceStartup - m_timeFetched > 15f)
		{
			m_status = Status.TimedOut;
		}
		return m_status;
	}

	public void Fetch()
	{
		if (ShouldShowForCurrentPlatform)
		{
			m_error = null;
			m_status = Status.Fetching;
			m_text = "";
			m_timeFetched = Time.realtimeSinceStartup;
			FetchBreakingNews(m_urlService.GetBreakingNewsLink(), OnResponseReceived);
		}
	}

	private void OnResponseReceived(string response, bool error)
	{
		if (error)
		{
			OnBreakingNewsError(response);
		}
		else
		{
			OnBreakingNewsResponse(response);
		}
	}

	private void OnBreakingNewsResponse(string response)
	{
		if (response == null)
		{
			m_logger.Log(LogLevel.Warning, "Breaking News response null");
			m_error = "Breaking News response null";
			return;
		}
		m_logger.Log(LogLevel.Debug, "Breaking News response received: {0}", response);
		m_text = response;
		if (m_text.Length <= 2 || m_text.ToLowerInvariant().Contains("<html>"))
		{
			m_text = "";
		}
		m_status = Status.Available;
	}

	private void OnBreakingNewsError(string error)
	{
		m_error = error;
		m_logger.Log(LogLevel.Warning, "Breaking News error received: {0}", error);
	}

	public string GetText()
	{
		if (!ShouldShowForCurrentPlatform)
		{
			return "";
		}
		if (m_status == Status.Fetching || m_status == Status.TimedOut)
		{
			m_logger.Log(LogLevel.Warning, "Fetched breaking news when it was unavailable, status={0}", m_status);
			return "";
		}
		if (m_error != null)
		{
			m_logger.Log(LogLevel.Warning, "Breaking news has an error:{0}", m_error);
			return "";
		}
		return m_text;
	}

	public string GetError()
	{
		return m_error;
	}
}
