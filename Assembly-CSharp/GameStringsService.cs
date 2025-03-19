using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class GameStringsService : IGameStringsService, IService
{
	public Type[] GetDependencies()
	{
		return null;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield break;
	}

	public void Shutdown()
	{
	}

	public string Get(string key)
	{
		return GameStrings.Get(key);
	}

	public string Format(string key, params object[] args)
	{
		return GameStrings.Format(key, args);
	}
}
