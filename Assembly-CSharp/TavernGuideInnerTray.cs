using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using UnityEngine;

public class TavernGuideInnerTray : MonoBehaviour
{
	[SerializeField]
	private Widget m_questSelectedTray;

	private const string QUEST_SET_CHANGED_EVENT = "CODE_QUEST_SET_UPDATE";

	private Widget m_widget;

	private int m_currentSelectedTavernQuestId;

	private int m_currentQuestSetId;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_SELECT_QUEST":
			HandleSelectQuest();
			break;
		case "CODE_CLAIM_QUEST_SET_REWARD":
			HandleClaimQuestSetReward();
			break;
		case "CODE_DEEPLINK":
			HandleDeeplink();
			break;
		case "CODE_ACK_QUEST":
			HandleAckQuest();
			break;
		case "CODE_QUEST_SET_UPDATE":
			HandleQuestSetChange();
			break;
		}
	}

	private void HandleSelectQuest()
	{
		if (!(m_widget.GetDataModel<EventDataModel>()?.Payload is TavernGuideQuestDataModel quest))
		{
			Debug.LogWarning("Unexpected state: no quest payload");
			return;
		}
		m_currentSelectedTavernQuestId = quest.ID;
		m_questSelectedTray.BindDataModel(quest);
	}

	private void HandleClaimQuestSetReward()
	{
		if (!Network.IsLoggedIn())
		{
			ProgressUtils.ShowOfflinePopup();
		}
		else if (m_widget.GetDataModel<EventDataModel>()?.Payload is TavernGuideQuestSetDataModel quest)
		{
			StartClaimSequence(TavernGuideManager.Get().GetRewardAchievementFromTavernGuideQuestId(quest.ID));
		}
	}

	private void StartClaimSequence(int achievementId)
	{
		if (achievementId != 0)
		{
			AchievementManager.Get().ClaimAchievementReward(achievementId);
		}
	}

	private void HandleDeeplink()
	{
		if (m_widget.GetDataModel<EventDataModel>()?.Payload is TavernGuideQuestDataModel quest)
		{
			string deepLink = quest.Quest.DeepLink;
			if (!string.IsNullOrEmpty(deepLink))
			{
				DeepLinkManager.ExecuteDeepLink(deepLink.Substring("hearthstone://".Length).Split('/').ToList()
					.ToArray(), DeepLinkManager.DeepLinkSource.TAVERN_GUIDE, quest.ID);
			}
		}
	}

	private void HandleAckQuest()
	{
		if (m_widget.GetDataModel<EventDataModel>()?.Payload is TavernGuideQuestDataModel quest && quest.Quest.Status == QuestManager.QuestStatus.NEW)
		{
			TavernGuideManager.Get().AckQuest(quest);
		}
	}

	private void HandleQuestSetChange()
	{
		TavernGuideQuestSetDataModel questSetDM = m_widget.GetDataModel<TavernGuideQuestSetDataModel>();
		if (questSetDM == null || questSetDM.ID == m_currentQuestSetId)
		{
			return;
		}
		m_currentQuestSetId = questSetDM.ID;
		m_questSelectedTray.UnbindDataModel(865);
		if (questSetDM.QuestLayoutType == TavernGuideQuestSet.TavernGuideQuestDisplayType.CIRCULAR)
		{
			if (questSetDM.Quests.Count > 1)
			{
				Debug.LogError("[TavernGuideInnerTray] Previously expected circular layout quest sets to only have one quest! Did something change?");
			}
			if (questSetDM.Quests.Count >= 1)
			{
				m_currentSelectedTavernQuestId = questSetDM.Quests[0].Quest.QuestId;
				m_questSelectedTray.BindDataModel(questSetDM.Quests[0]);
			}
		}
	}
}
