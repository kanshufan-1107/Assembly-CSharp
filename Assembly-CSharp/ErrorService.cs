using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class ErrorService : IErrorService, IService
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

	public void AddFatal(FatalErrorReason reason, string messageKey, params object[] messageArgs)
	{
		Error.AddFatal(reason, messageKey, messageArgs);
	}
}
