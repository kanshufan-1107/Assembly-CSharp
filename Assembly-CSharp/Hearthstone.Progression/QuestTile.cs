using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestTile : MonoBehaviour
{
	[SerializeField]
	private GameObject m_deepLinkButton;

	private Widget m_widget;

	private bool m_isRerollPending;

	private bool m_isRerollAnimPlaying;

	private bool m_wasRerollSuccessful;

	private int m_grantedQuestId;

	public static string QUEST_TILE_WIDGET_ASSET = "QuestTile.prefab:6a05035200522f3418a150db7763ce95";

	private static string DEEP_LINK_BUTTON_PRESSED = "DEEP_LINK_BUTTON_PRESSED";

	private static string SHOW_QUEST_INSPECT_POPUP = "SHOW_QUEST_INSPECT_POPUP";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(WidgetEventListener);
		QuestManager.Get().OnQuestRerolled += OnQuestRerolled;
		QuestManager.Get().OnQuestRerollCountChanged += OnQuestRerollCountChanged;
		m_widget.RegisterDoneChangingStatesListener(OnDoneChangingStatesOnce, null, callImmediatelyIfSet: true, doOnce: true);
		m_widget.RegisterDoneChangingStatesListener(OnDoneChangingStates);
	}

	private void OnDestroy()
	{
		if (m_widget != null)
		{
			m_widget.RemoveDoneChangingStatesListener(OnDoneChangingStatesOnce);
			m_widget.RemoveDoneChangingStatesListener(OnDoneChangingStates);
		}
		if (QuestManager.Get() != null)
		{
			QuestManager.Get().OnQuestRerolled -= OnQuestRerolled;
			QuestManager.Get().OnQuestRerollCountChanged -= OnQuestRerollCountChanged;
		}
	}

	private void OnDoneChangingStatesOnce(object unused)
	{
		if (ProgressUtils.ShowDebugIds)
		{
			m_widget.TriggerEvent("DEBUG_SHOW_ID");
		}
	}

	private void OnDoneChangingStates(object unused)
	{
		if (m_widget.GetDataModel<RewardTrackDataModel>() != null)
		{
			return;
		}
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null && questDataModel.RewardTrackType != 0)
		{
			RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(questDataModel.RewardTrackType);
			if (rewardTrack != null && rewardTrack.IsValid)
			{
				m_widget.BindDataModel(rewardTrack.TrackDataModel);
			}
		}
	}

	private void WidgetEventListener(string eventName)
	{
		if (eventName.Equals("CLICKED_REROLL"))
		{
			RerollQuest();
		}
		else if (eventName.Equals("CLICKED_ABANDON"))
		{
			AbandonQuest();
		}
		else if (eventName.Equals(DEEP_LINK_BUTTON_PRESSED))
		{
			DeepLinkFromQuest();
		}
		else if (eventName.Equals(SHOW_QUEST_INSPECT_POPUP))
		{
			InspectQuestTile();
		}
	}

	private void RerollQuest()
	{
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null && !m_isRerollPending)
		{
			if (!Network.IsLoggedIn())
			{
				ProgressUtils.ShowOfflinePopup();
			}
			else if (QuestManager.Get().RerollQuest(questDataModel.QuestId))
			{
				m_isRerollPending = true;
				m_widget.TriggerEvent("CODE_REROLLED");
				m_isRerollAnimPlaying = true;
			}
		}
	}

	private void AbandonQuest()
	{
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null)
		{
			if (!Network.IsLoggedIn())
			{
				ProgressUtils.ShowOfflinePopup();
				return;
			}
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_PROGRESSION_ABANDON_QUEST_HEADER");
			info.m_text = GameStrings.Get("GLUE_PROGRESSION_ABANDON_QUEST_BODY");
			info.m_showAlertIcon = true;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = OnConfirmAbandonQuestResponse;
			info.m_responseUserData = questDataModel;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void DeepLinkFromQuest()
	{
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null)
		{
			string deepLink = questDataModel.DeepLink;
			if (!string.IsNullOrEmpty(deepLink))
			{
				int questId = questDataModel.QuestId;
				TelemetryManager.Client().SendQuestTileClick(questId, GetQuestTileDisplayContext(questDataModel), QuestTileClickType.QTCT_DEEP_LINK, deepLink);
				DeepLinkManager.ExecuteDeepLink(deepLink.Substring("hearthstone://".Length).Split('/'), DeepLinkManager.DeepLinkSource.QUEST, questId);
			}
		}
	}

	private void InspectQuestTile()
	{
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null)
		{
			TelemetryManager.Client().SendQuestTileClick(questDataModel.QuestId, GetQuestTileDisplayContext(questDataModel), QuestTileClickType.QTCT_INSPECT, string.Empty);
		}
	}

	private void OnConfirmAbandonQuestResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			QuestDataModel questDataModel = (QuestDataModel)userData;
			if (questDataModel != null && QuestManager.Get().AbandonQuest(questDataModel.QuestId))
			{
				TelemetryManager.Client().SendQuestTileClick(questDataModel.QuestId, GetQuestTileDisplayContext(questDataModel), QuestTileClickType.QTCT_ABANDON, string.Empty);
				m_widget.TriggerEvent("CODE_REROLLED");
				m_isRerollAnimPlaying = true;
			}
		}
	}

	private void OnQuestRerolled(int rerolledQuestId, int grantedQuestId, bool success)
	{
		if (m_isRerollPending)
		{
			QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
			if (questDataModel != null && questDataModel.QuestId == rerolledQuestId)
			{
				TelemetryManager.Client().SendQuestTileClick(questDataModel.QuestId, GetQuestTileDisplayContext(questDataModel), QuestTileClickType.QTCT_REROLL, string.Empty);
				m_isRerollPending = false;
				m_wasRerollSuccessful = success;
				m_grantedQuestId = grantedQuestId;
				PlayQuestGrantedAnimIfReady();
			}
		}
	}

	private void OnQuestRerollCountChanged(int questPoolId, int rerollCount)
	{
		QuestDataModel questDataModel = m_widget.GetDataModel<QuestDataModel>();
		if (questDataModel != null && questDataModel.PoolId == questPoolId)
		{
			questDataModel.RerollCount = rerollCount;
		}
	}

	private void OnRerollAnimFinished()
	{
		m_isRerollAnimPlaying = false;
		PlayQuestGrantedAnimIfReady();
	}

	private void PlayQuestGrantedAnimIfReady()
	{
		if (m_isRerollPending || m_isRerollAnimPlaying)
		{
			return;
		}
		if (m_wasRerollSuccessful)
		{
			UpdateQuestDataModelByQuestId(m_grantedQuestId);
			m_widget.RegisterDoneChangingStatesListener(delegate
			{
				m_widget.TriggerEvent("CODE_GRANTED_BY_REROLL");
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			m_widget.Show();
		}
	}

	private void UpdateQuestDataModelByQuestId(int questId)
	{
		m_widget.GetDataModel<QuestDataModel>()?.CopyFromDataModel(QuestManager.Get().CreateQuestDataModelById(questId));
	}

	public DisplayContext GetQuestTileDisplayContext(QuestDataModel questDataModel)
	{
		if (questDataModel == null)
		{
			return DisplayContext.DC_CONTEXT_UNKNOWN;
		}
		if (questDataModel.DisplayMode == QuestManager.QuestDisplayMode.Notification)
		{
			return DisplayContext.DC_NOTIFICATION_POPUP;
		}
		JournalTrayDisplay.JournalTab activeJournalTab = JournalTrayDisplay.JournalTab.Unknown;
		Box theBox = Box.Get();
		if (theBox == null)
		{
			return DisplayContext.DC_CONTEXT_UNKNOWN;
		}
		JournalButton journalButton = theBox.GetJournalButton();
		if (JournalPopup.s_isShowing)
		{
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.BACON)
			{
				return DisplayContext.DC_BG_QUEST_LOG;
			}
			if (journalButton != null)
			{
				JournalTrayDisplay journalTrayDisplay = journalButton.GetJournalTrayDisplay();
				if (journalTrayDisplay != null)
				{
					activeJournalTab = journalTrayDisplay.GetActiveJournalTab();
				}
			}
		}
		return activeJournalTab switch
		{
			JournalTrayDisplay.JournalTab.Event => DisplayContext.DC_EVENT_PAGE, 
			JournalTrayDisplay.JournalTab.Quest => DisplayContext.DC_QUEST_LOG, 
			_ => DisplayContext.DC_CONTEXT_UNKNOWN, 
		};
	}
}
