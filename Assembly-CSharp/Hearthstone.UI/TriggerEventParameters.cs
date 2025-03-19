namespace Hearthstone.UI;

public struct TriggerEventParameters
{
	public readonly string SourceName;

	public readonly object Payload;

	public readonly bool NoDownwardPropagation;

	public readonly bool IgnorePlaymaker;

	public static TriggerEventParameters Standard => new TriggerEventParameters(null, null, noDownwardPropagation: true, ignorePlaymaker: true);

	public static TriggerEventParameters StandardPropagateDownward => new TriggerEventParameters(null, null, noDownwardPropagation: false, ignorePlaymaker: true);

	public TriggerEventParameters(string sourceName = null, object payload = null, bool noDownwardPropagation = false, bool ignorePlaymaker = false)
	{
		SourceName = sourceName;
		Payload = payload;
		NoDownwardPropagation = noDownwardPropagation;
		IgnorePlaymaker = ignorePlaymaker;
	}
}
