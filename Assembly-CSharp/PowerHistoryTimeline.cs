using System.Collections.Generic;
using Blizzard.T5.Core;

public class PowerHistoryTimeline
{
	public int m_firstTaskId;

	public int m_lastTaskId;

	public int m_slushTime;

	public float m_startTime;

	public float m_endTime;

	public List<PowerHistoryTimelineEntry> m_orderedEvents = new List<PowerHistoryTimelineEntry>();

	public Map<int, int> m_orderedEventIndexLookup = new Map<int, int>();

	public void AddTimelineEntry(int taskId, int slushTime)
	{
		PowerHistoryTimelineEntry entry = new PowerHistoryTimelineEntry();
		entry.taskId = taskId;
		entry.expectedTime = slushTime;
		if (m_orderedEvents.Count == 0)
		{
			entry.expectedStartOffset = 0;
		}
		else
		{
			PowerHistoryTimelineEntry previous = m_orderedEvents[m_orderedEvents.Count - 1];
			entry.expectedStartOffset = previous.expectedStartOffset + previous.expectedTime;
		}
		m_orderedEvents.Add(entry);
		m_orderedEventIndexLookup.Add(taskId, m_orderedEvents.Count - 1);
	}
}
