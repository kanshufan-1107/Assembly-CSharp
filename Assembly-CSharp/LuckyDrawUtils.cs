using System;
using System.Collections.Generic;

public class LuckyDrawUtils
{
	public static LuckyDrawBoxDbfRecord GetCurrentLuckyDrawRecord()
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		if (eventTimingManager == null)
		{
			return null;
		}
		List<LuckyDrawBoxDbfRecord> records = GameDbf.LuckyDrawBox.GetRecords();
		LuckyDrawBoxDbfRecord luckyDrawBoxDbfRecord = null;
		foreach (LuckyDrawBoxDbfRecord boxRecord in records)
		{
			if (eventTimingManager.IsEventActive(boxRecord.Event))
			{
				if (luckyDrawBoxDbfRecord != null)
				{
					Error.AddDevWarning("Too Many BattleBashes", "There are at least 2 active BattleBash events active at the same time. Only 1 BattleBash can be active. Check HearthEdit 2 and ensure the start/end dates of the events are not overlapping.");
					return null;
				}
				luckyDrawBoxDbfRecord = boxRecord;
			}
		}
		return luckyDrawBoxDbfRecord;
	}

	public static int GetCurrentLuckyDrawID()
	{
		return GetCurrentLuckyDrawRecord()?.ID ?? (-1);
	}

	public static TimeSpan GetLuckyDrawTimeRemaining(int luckyDrawBoxID)
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		if (eventTimingManager == null)
		{
			return new TimeSpan(0L);
		}
		LuckyDrawBoxDbfRecord record = GameDbf.LuckyDrawBox.GetRecord(luckyDrawBoxID);
		if (record == null)
		{
			return new TimeSpan(0L);
		}
		return eventTimingManager.GetTimeLeftForEvent(record.Event);
	}

	public static void ShowErrorAndReturnToLobby()
	{
		SceneMgr.Get();
		if (InOrTransitioningToLuckyDrawScene() || InOrTransitioningToBattlegroundsLobby())
		{
			Error.AddWarningLoc("GLUE_BATTLEBASH_ERROR_HEADER", "GLUE_BATTLEBASH_ERROR_BODY");
		}
		if (InOrTransitioningToLuckyDrawScene())
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.BACON, SceneMgr.TransitionHandlerType.NEXT_SCENE);
		}
	}

	private static bool InOrTransitioningToLuckyDrawScene()
	{
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr.GetMode() == SceneMgr.Mode.LUCKY_DRAW || sceneMgr.GetNextMode() == SceneMgr.Mode.LUCKY_DRAW)
		{
			return true;
		}
		return false;
	}

	private static bool InOrTransitioningToBattlegroundsLobby()
	{
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr.GetMode() == SceneMgr.Mode.BACON || sceneMgr.GetNextMode() == SceneMgr.Mode.BACON)
		{
			return true;
		}
		return false;
	}
}
