using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdventureWing_BOT_Boom : AdventureWing
{
	[Serializable]
	public class PlateCoverData
	{
		[SerializeField]
		public PegUIElement PlateCoverHitbox;

		[SerializeField]
		public string TooltipText;
	}

	public List<UIBButton> CheatPlateOpenButtons;

	public List<PlateCoverData> m_plateCoverData;

	protected override void Awake()
	{
		base.Awake();
		foreach (PlateCoverData data in m_plateCoverData)
		{
			data.PlateCoverHitbox.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				OnPlateCoverRollover(data);
			});
			data.PlateCoverHitbox.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				OnPlateCoverRollout(data);
			});
		}
		if (base.IsDevMode)
		{
			CheatPlateOpenButtons[0].AddEventListener(UIEventType.RELEASE, delegate
			{
				m_WingEventTable.DoStatePlateOpen(0);
			});
			CheatPlateOpenButtons[1].AddEventListener(UIEventType.RELEASE, delegate
			{
				m_WingEventTable.DoStatePlateOpen(1);
			});
			CheatPlateOpenButtons[2].AddEventListener(UIEventType.RELEASE, delegate
			{
				m_WingEventTable.DoStatePlateOpen(2);
			});
			CheatPlateOpenButtons[3].AddEventListener(UIEventType.RELEASE, delegate
			{
				m_WingEventTable.DoStatePlateOpen(3);
			});
		}
	}

	private void OnPlateCoverRollover(PlateCoverData data)
	{
		data.PlateCoverHitbox.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_ADVENTURE_BOT_BOOM_TOOLTIP_HEADER"), GameStrings.Get(data.TooltipText), 5f);
	}

	private void OnPlateCoverRollout(PlateCoverData data)
	{
		data.PlateCoverHitbox.GetComponent<TooltipZone>().HideTooltip();
	}

	protected override void DoOpenPlate(float unlockDelay)
	{
		List<AdventureMissionDbfRecord> sortedAdventureMissionRecordsForThisWing = GetSortedAdventureMissionRecordsForThisWing();
		bool plateOpening = false;
		foreach (AdventureMissionDbfRecord record in sortedAdventureMissionRecordsForThisWing)
		{
			int requiredProgress = record.ReqProgress;
			int currentProgress = AdventureProgressMgr.Get().GetProgressValueForWing(record.ReqWingId);
			AdventureProgressMgr.Get().GetWingAck(record.ReqWingId, out var ackedProgress);
			if (currentProgress == requiredProgress && (ackedProgress < currentProgress || HasDependentWingJustAckedRequiredProgress(record)))
			{
				int plateOpenIndex = GameDbf.Scenario.GetRecord(record.ScenarioId).SortOrder - 1;
				m_WingEventTable.DoStatePlateOpen(plateOpenIndex, unlockDelay);
				plateOpening = true;
			}
		}
		if (!plateOpening)
		{
			FireOpenPlateEndEvent(null);
		}
	}

	protected override bool InitializePlateOpenState()
	{
		List<AdventureMissionDbfRecord> sortedAdventureMissionRecordsForThisWing = GetSortedAdventureMissionRecordsForThisWing();
		bool plateStartsOpen = false;
		foreach (AdventureMissionDbfRecord record in sortedAdventureMissionRecordsForThisWing)
		{
			int requiredProgress = record.ReqProgress;
			int currentProgress = AdventureProgressMgr.Get().GetProgressValueForWing(record.ReqWingId);
			AdventureProgressMgr.Get().GetWingAck(record.ReqWingId, out var ackedProgress);
			if (currentProgress >= requiredProgress && ackedProgress >= currentProgress && !HasDependentWingJustAckedRequiredProgress())
			{
				int plateOpenIndex = GameDbf.Scenario.GetRecord(record.ScenarioId).SortOrder - 1;
				m_WingEventTable.DoStatePlateAlreadyOpen(plateOpenIndex);
				plateStartsOpen = true;
			}
		}
		return plateStartsOpen;
	}

	private List<AdventureMissionDbfRecord> GetSortedAdventureMissionRecordsForThisWing()
	{
		List<AdventureMissionDbfRecord> records = GameDbf.AdventureMission.GetRecords((AdventureMissionDbfRecord r) => r.GrantsWingId == (int)m_WingDef.GetWingId());
		records.OrderBy((AdventureMissionDbfRecord r) => GameDbf.Scenario.GetRecord(r.ScenarioId).SortOrder);
		return records;
	}
}
