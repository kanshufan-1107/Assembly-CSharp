using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class JournalTrayDisplay : MonoBehaviour
{
	public enum JournalTab
	{
		Event,
		Quest,
		Reward,
		Achievment,
		Profile,
		ApprenticeTrack,
		TavernGuide,
		Unknown
	}

	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType = Global.RewardTrackType.GLOBAL;

	[SerializeField]
	private bool m_shouldHideTrayOnTabChanged = true;

	[Tooltip("The journal opens to this tab index the first time it is opened each session")]
	[SerializeField]
	private int m_initialTabIndexPerSession = 1;

	[Tooltip("The journal simulates its first transition as if a button has been clicked, allowing popups etc. to be triggered for that tab.")]
	[SerializeField]
	private bool m_simulateFirstTransition;

	public const string TAB_SELECTED = "CODE_JOURNAL_TAB_SELECTED";

	public const string FINISHED_EVENT = "CODE_{0}_FINISHED";

	public const string ACTIVE_TAB_CLICKED_EVENT = "CODE_ACTIVE_{0}_CLICKED";

	private static readonly Dictionary<Global.RewardTrackType, int> s_activeTabIndexByType = new Dictionary<Global.RewardTrackType, int>();

	private JournalMetaDataModel m_journalMetaDatamodel;

	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(HandleReady);
		m_widget.RegisterEventListener(HandleEvent);
	}

	public static void SetActiveTabForTrackType(Global.RewardTrackType journalTrackType, JournalTab tab)
	{
		s_activeTabIndexByType[journalTrackType] = (int)tab;
	}

	public static JournalTab GetActiveTabForTrackType(Global.RewardTrackType journalTrackType)
	{
		if (s_activeTabIndexByType.TryGetValue(journalTrackType, out var activeTab))
		{
			return (JournalTab)activeTab;
		}
		return JournalTab.Unknown;
	}

	public JournalTab GetActiveJournalTab()
	{
		if (!JournalPopup.s_isShowing || m_widget == null)
		{
			return JournalTab.Unknown;
		}
		return (JournalTab)m_journalMetaDatamodel.TabIndex;
	}

	public void ForceChangeActiveTabViaDeepLink(JournalTab tab)
	{
		if (JournalPopup.s_isShowing && !(m_widget == null))
		{
			string eventName = tab.ToString().ToUpper() + "_TAB";
			m_widget.TriggerEvent(eventName);
		}
	}

	private void HandleReady(object unused)
	{
		m_journalMetaDatamodel = m_widget.GetDataModel<JournalMetaDataModel>();
		if (m_journalMetaDatamodel == null)
		{
			Debug.LogError("JournalTrayDisplay created without valid JournalMetaDataModel");
			return;
		}
		if (!s_activeTabIndexByType.ContainsKey(m_rewardTrackType))
		{
			if (m_rewardTrackType == Global.RewardTrackType.GLOBAL && m_journalMetaDatamodel.EventActive && m_journalMetaDatamodel.EventIsNew)
			{
				s_activeTabIndexByType[m_rewardTrackType] = 0;
			}
			else if (m_rewardTrackType == Global.RewardTrackType.GLOBAL && !m_journalMetaDatamodel.IsApprenticeTrackActive && m_journalMetaDatamodel.IsTavernGuideActive)
			{
				s_activeTabIndexByType[m_rewardTrackType] = 6;
			}
			else
			{
				s_activeTabIndexByType[m_rewardTrackType] = m_initialTabIndexPerSession;
			}
		}
		m_journalMetaDatamodel.TabIndex = s_activeTabIndexByType[m_rewardTrackType];
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "CODE_JOURNAL_TAB_SELECTED")
		{
			HandleTabSelected();
		}
	}

	private void HandleTabSelected()
	{
		if (!m_journalMetaDatamodel.DoneChangingTabs && !m_simulateFirstTransition)
		{
			return;
		}
		TooltipPanelManager.Get().HideTooltipPanels();
		m_journalMetaDatamodel.DoneChangingTabs = false;
		EventDataModel e = m_widget.GetDataModel<EventDataModel>();
		if (e == null)
		{
			Debug.LogError("HandleTabSelected: no payload was sent with the CODE_JOURNAL_TAB_SELECTED event!");
			return;
		}
		if (!s_activeTabIndexByType.TryGetValue(m_rewardTrackType, out var previousIndex))
		{
			Debug.LogError(string.Format("{0}: no active tab index defined for reward track found of type {1}.", "JournalTrayDisplay", m_rewardTrackType));
			return;
		}
		if (e.Payload is IConvertible)
		{
			int selectedTabIndex = Convert.ToInt32(e.Payload);
			s_activeTabIndexByType[m_rewardTrackType] = selectedTabIndex;
			m_journalMetaDatamodel.TabIndex = selectedTabIndex;
		}
		if (previousIndex == s_activeTabIndexByType[m_rewardTrackType])
		{
			if (!m_simulateFirstTransition)
			{
				m_widget.RegisterDoneChangingStatesListener(delegate
				{
					IWidgetEventListener[] componentsInChildren = m_widget.GetComponentsInChildren<IWidgetEventListener>(includeInactive: false);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].EventReceived($"CODE_ACTIVE_{e.SourceName}_CLICKED", default(TriggerEventParameters));
					}
					m_journalMetaDatamodel.DoneChangingTabs = true;
				}, null, callImmediatelyIfSet: true, doOnce: true);
				return;
			}
			m_simulateFirstTransition = false;
		}
		if (m_shouldHideTrayOnTabChanged)
		{
			m_widget.Hide();
		}
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			m_widget.TriggerEvent($"CODE_{e.SourceName}_FINISHED", new TriggerEventParameters(e.SourceName, previousIndex, noDownwardPropagation: true, ignorePlaymaker: true));
			if (e.SourceName.Equals("REWARD_TAB"))
			{
				RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
				if (rewardTrack == null || !rewardTrack.IsValid)
				{
					Debug.LogError(string.Format("{0}: no reward track found of type {1}.", "JournalTrayDisplay", m_rewardTrackType));
				}
				else if (rewardTrack.TrackDataModel.Season > rewardTrack.TrackDataModel.SeasonLastSeen)
				{
					rewardTrack.SetRewardTrackSeasonLastSeen(rewardTrack.TrackDataModel.Season);
				}
			}
			if (m_shouldHideTrayOnTabChanged)
			{
				m_widget.Show();
			}
			m_journalMetaDatamodel.DoneChangingTabs = true;
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}
}
