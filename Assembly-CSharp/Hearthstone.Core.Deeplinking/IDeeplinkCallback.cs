namespace Hearthstone.Core.Deeplinking;

public interface IDeeplinkCallback
{
	void ProcessDeeplink(string url);
}
