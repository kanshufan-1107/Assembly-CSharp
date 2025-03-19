using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Http;

public interface IHttpRequest : IDisposable
{
	bool IsNetworkError { get; }

	bool IsHttpError { get; }

	int TimeoutSeconds { set; }

	bool DidTimeout { get; }

	string ErrorString { get; }

	int ResponseStatusCode { get; }

	Dictionary<string, string> ResponseHeaders { get; }

	string ResponseAsString { get; }

	Texture ResponseAsTexture { get; }

	AsyncOperation SendRequest();

	void SetRequestHeaders(IEnumerable<KeyValuePair<string, string>> headers);
}
