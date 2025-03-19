using Blizzard.T5.Services;

namespace Hearthstone.Streaming;

public static class GameDownloadManagerProvider
{
	private static GameDownloadManager s_instance;

	public static IGameDownloadManager Get()
	{
		if (s_instance == null && !ServiceManager.TryGet<GameDownloadManager>(out s_instance))
		{
			return null;
		}
		return s_instance;
	}
}
