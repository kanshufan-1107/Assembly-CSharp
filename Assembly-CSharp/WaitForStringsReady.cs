using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Streaming;

public class WaitForStringsReady : IJobDependency, IAsyncJobResult
{
	public bool IsReady()
	{
		if (!ServiceManager.TryGet<GameDownloadManager>(out var gameDownloadManager))
		{
			return false;
		}
		return gameDownloadManager.IsReadyToReadStrings;
	}
}
