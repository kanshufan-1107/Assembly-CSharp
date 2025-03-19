using Assets;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Send an event based on quest info.")]
[ActionCategory("Pegasus")]
public class QuestEventAction : FsmStateAction
{
	public FsmOwnerDefault m_QuestObject;

	public Achieve.Type m_AchievementType;

	public EventTimingType m_ActivatedBySpecialEventType;

	public bool m_IsLegendary;

	public FsmEvent m_SendEventOnMatch;

	public FsmEvent m_SendEventOnNotMatch;

	public override void Reset()
	{
		m_QuestObject = null;
		m_AchievementType = Achieve.Type.INVALID;
		m_ActivatedBySpecialEventType = EventTimingType.UNKNOWN;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_QuestObject != null)
		{
			Achievement achieve = null;
			GameObject go = base.Fsm.GetOwnerDefaultTarget(m_QuestObject);
			if (go != null)
			{
				QuestTile tile = go.GetComponentInParent<QuestTile>();
				if (tile != null)
				{
					achieve = tile.GetQuest();
				}
				if (achieve == null)
				{
					FriendlyChallengeDialog frame = go.GetComponentInParent<FriendlyChallengeDialog>();
					if (frame != null)
					{
						achieve = frame.GetQuest();
					}
				}
				if (achieve != null)
				{
					if (CheckAchieve(achieve))
					{
						if (m_SendEventOnMatch != null)
						{
							base.Fsm.Event(m_SendEventOnMatch);
						}
					}
					else if (m_SendEventOnNotMatch != null)
					{
						base.Fsm.Event(m_SendEventOnNotMatch);
					}
				}
				else
				{
					Debug.LogError("No Achievement found! (QuestTile or FriendlyChallengeDialog required in Parent chain of FsmOwnerDefault)");
				}
			}
		}
		Finish();
	}

	private bool CheckAchieve(Achievement achieve)
	{
		if (m_AchievementType != 0 && achieve.AchieveType != m_AchievementType)
		{
			return false;
		}
		if (m_IsLegendary && !achieve.IsLegendary)
		{
			return false;
		}
		if (m_ActivatedBySpecialEventType != EventTimingType.UNKNOWN)
		{
			AchieveRegionDataDbfRecord regionData = achieve.GetCurrentRegionData();
			if (regionData == null)
			{
				return false;
			}
			if (m_ActivatedBySpecialEventType != regionData.ActivateEvent)
			{
				return false;
			}
		}
		return true;
	}
}
