using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class WaitForFullLoginFlowComplete : IJobDependency, IAsyncJobResult
{
	public bool IsReady()
	{
		if (ServiceManager.TryGet<LoginManager>(out var loginManager))
		{
			return loginManager.IsFullLoginFlowComplete;
		}
		return false;
	}
}
