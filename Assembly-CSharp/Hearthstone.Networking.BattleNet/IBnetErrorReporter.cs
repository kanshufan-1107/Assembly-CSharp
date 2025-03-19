namespace Hearthstone.Networking.BattleNet;

public interface IBnetErrorReporter
{
	void ReportFatal(FatalErrorReason reason, string messageKey, params object[] messageArgs);

	void ReportDevFatal(string message, params object[] messageArgs);
}
