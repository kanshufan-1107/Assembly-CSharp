namespace Hearthstone.Progression;

public class CatchupPackEventManager
{
	public static CatchupPackEventDbfRecord GetCurrentCatchupPackBoxDressingEvent()
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		return GameDbf.CatchupPackEvent.GetRecord(delegate(CatchupPackEventDbfRecord asset)
		{
			EventTimingType boxDressingEventTiming = asset.BoxDressingEventTiming;
			return eventTimingManager.IsEventActive(boxDressingEventTiming);
		});
	}
}
