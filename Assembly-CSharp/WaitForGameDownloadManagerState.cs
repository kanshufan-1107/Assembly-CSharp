using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Streaming;

public class WaitForGameDownloadManagerState : IJobDependency, IAsyncJobResult
{
	public bool IsReady()
	{
		if (!ServiceManager.IsAvailable<GameDownloadManager>())
		{
			return false;
		}
		GameDownloadManager gameDownloadManager = ServiceManager.Get<GameDownloadManager>();
		if (!gameDownloadManager.IsReadyToPlay)
		{
			return false;
		}
		if (!gameDownloadManager.IsReadyToReadStrings || GameDbf.ShouldForceXmlLoading())
		{
			return GameDbf.IsLoaded;
		}
		return DbfShared.GetAssetBundle() != null;
	}
}
