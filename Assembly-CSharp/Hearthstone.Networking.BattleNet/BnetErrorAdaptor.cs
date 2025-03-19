namespace Hearthstone.Networking.BattleNet;

public class BnetErrorAdaptor : IBnetErrorReporter
{
	public void ReportFatal(FatalErrorReason reason, string messageKey, params object[] messageArgs)
	{
		Error.AddFatal(reason, messageKey, messageArgs);
	}

	public void ReportDevFatal(string message, params object[] messageArgs)
	{
		Error.AddDevFatal(message, messageArgs);
	}
}
