using System.Collections;
using System.Collections.Generic;
using System.Net;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Http;
using UnityEngine;

public class CheatRequest
{
	public bool IsSuccessful;

	private static HttpStatusCode GetStatusCode(IDictionary<string, string> headers)
	{
		if (headers == null)
		{
			return HttpStatusCode.NotFound;
		}
		if (!headers.TryGetValue("STATUS", out var statusLine))
		{
			return HttpStatusCode.NotFound;
		}
		string[] statusSplits = statusLine.Split(' ');
		if (statusSplits.Length < 3)
		{
			return HttpStatusCode.NotFound;
		}
		if (!int.TryParse(statusSplits[1], out var statusCode))
		{
			return HttpStatusCode.NotFound;
		}
		return (HttpStatusCode)statusCode;
	}

	private IEnumerator SendGetRequestCoroutine(string url)
	{
		IHttpRequest request = HttpRequestFactory.Get().CreateGetRequest(url);
		yield return request.SendRequest();
		if (request.IsNetworkError || request.IsHttpError)
		{
			if (request.ErrorString.StartsWith("Failed to connect"))
			{
				LogError("Failed to initiate cheat request. Cheat server is unreachable.");
			}
			else
			{
				LogError(string.IsNullOrEmpty(request.ResponseAsString) ? request.ErrorString : request.ResponseAsString);
			}
			yield break;
		}
		HttpStatusCode statusCode = GetStatusCode(request.ResponseHeaders);
		if (statusCode != HttpStatusCode.OK)
		{
			LogError(statusCode, request.ResponseAsString);
			IsSuccessful = false;
		}
		else
		{
			IsSuccessful = true;
			UIStatus.Get().AddInfo(request.ResponseAsString);
		}
	}

	private IEnumerator SendDeleteRequestCoroutine(string url)
	{
		IHttpRequest request = HttpRequestFactory.Get().CreateDeleteRequest(url);
		yield return request.SendRequest();
		if (request.IsNetworkError)
		{
			if (request.ErrorString.StartsWith("Failed to connect"))
			{
				LogError("Failed to initiate cheat request. Cheat server is unreachable.");
			}
			else
			{
				LogError(request.ErrorString);
			}
			yield break;
		}
		string responseBody = request.ResponseAsString;
		HttpStatusCode statusCode = (HttpStatusCode)request.ResponseStatusCode;
		if (statusCode != HttpStatusCode.OK)
		{
			LogError(statusCode, responseBody);
			IsSuccessful = false;
		}
		else
		{
			IsSuccessful = true;
			UIStatus.Get().AddInfo(responseBody);
		}
	}

	public Coroutine SendGetRequest(string url)
	{
		return Processor.RunCoroutine(SendGetRequestCoroutine(url));
	}

	public Coroutine SendDeleteRequest(string url)
	{
		return Processor.RunCoroutine(SendDeleteRequestCoroutine(url));
	}

	public static void LogError(string message)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			UIStatus.Get().AddError(message);
			Debug.LogError(message);
		}
	}

	public static void LogError(HttpStatusCode statusCode, string message)
	{
		LogError($"{message} (status code: {(int)statusCode})");
	}
}
