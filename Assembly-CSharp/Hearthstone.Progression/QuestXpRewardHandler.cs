using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class QuestXpRewardHandler
{
	private class QuestXpRewardContainer
	{
		public class UnfinishedPageDisplayCounts
		{
			public int m_unfinishedCount;

			public int m_unfinishedGameXpCount;
		}

		public List<RewardTrackXpChange> m_xpChanges;

		public QuestXpReward m_xpReward;

		public bool m_hasXpChanges;

		private int m_pageToShowThisContainer;

		private static List<UnfinishedPageDisplayCounts> s_unfinishedPageDisplayCounts;

		public bool m_finished { get; private set; }

		public bool m_gameXpFinished { get; private set; }

		public QuestXpRewardContainer()
		{
			m_xpChanges = new List<RewardTrackXpChange>();
		}

		public void AddXpChange(RewardTrackXpChange xpChange)
		{
			m_xpChanges.Add(xpChange);
			m_hasXpChanges = true;
		}

		public void SetQuestXpReward(QuestXpReward xpReward)
		{
			m_xpReward = xpReward;
		}

		public void Initialize()
		{
			if (m_hasXpChanges)
			{
				m_finished = false;
				m_gameXpFinished = false;
				s_unfinishedPageDisplayCounts[m_pageToShowThisContainer].m_unfinishedCount++;
				s_unfinishedPageDisplayCounts[m_pageToShowThisContainer].m_unfinishedGameXpCount++;
				m_xpReward.Initialize(m_xpChanges);
				m_xpChanges?.Clear();
			}
			else
			{
				m_finished = true;
				m_gameXpFinished = true;
			}
		}

		public void SetGameXpFinished(bool isFinished)
		{
			if (m_gameXpFinished != isFinished)
			{
				m_gameXpFinished = isFinished;
				int delta = ((!m_gameXpFinished) ? 1 : (-1));
				s_unfinishedPageDisplayCounts[m_pageToShowThisContainer].m_unfinishedGameXpCount += delta;
			}
		}

		public void SetPageToShow(int pageNum)
		{
			m_pageToShowThisContainer = pageNum;
		}

		public int GetPageToShow()
		{
			return m_pageToShowThisContainer;
		}

		public int XpChangesRemaining()
		{
			return m_xpReward.GetXpChangesRemaining();
		}

		public static void InitializePageCounts(int numPages)
		{
			s_unfinishedPageDisplayCounts = new List<UnfinishedPageDisplayCounts>();
			for (int idx = 0; idx < numPages; idx++)
			{
				s_unfinishedPageDisplayCounts.Add(new UnfinishedPageDisplayCounts());
			}
		}

		public static int GetUnfinishedGameXpCount(int pageNum)
		{
			return s_unfinishedPageDisplayCounts[pageNum].m_unfinishedGameXpCount;
		}

		public static int GameXpDisplaysToShow()
		{
			int total = 0;
			foreach (UnfinishedPageDisplayCounts count in s_unfinishedPageDisplayCounts)
			{
				total += count.m_unfinishedGameXpCount;
			}
			return total;
		}

		public static void ClearUnfinished()
		{
			if (s_unfinishedPageDisplayCounts == null)
			{
				return;
			}
			foreach (UnfinishedPageDisplayCounts s_unfinishedPageDisplayCount in s_unfinishedPageDisplayCounts)
			{
				s_unfinishedPageDisplayCount.m_unfinishedCount = 0;
				s_unfinishedPageDisplayCount.m_unfinishedGameXpCount = 0;
			}
		}
	}

	public static readonly AssetReference END_OF_GAME_MULTI_QUEST_REWARD_FLOW_PREFAB = new AssetReference("MultiQuestXPReward.prefab:69cdcf0065e3f1d418509368a994ec50");

	private static readonly Dictionary<Global.RewardTrackType, string> VC_STATES_BY_TRACK_TYPE = new Dictionary<Global.RewardTrackType, string>
	{
		{
			Global.RewardTrackType.GLOBAL,
			"GLOBAL"
		},
		{
			Global.RewardTrackType.BATTLEGROUNDS,
			"BATTLEGROUNDS"
		},
		{
			Global.RewardTrackType.EVENT,
			"EVENT"
		},
		{
			Global.RewardTrackType.APPRENTICE,
			"APPRENTICE"
		}
	};

	private Dictionary<Global.RewardTrackType, QuestXpRewardContainer> m_questXpRewards;

	private Action m_callback;

	private Action m_initCallback;

	private Queue<Global.RewardTrackType> m_xpTypeQueue;

	private bool m_isShowingGameXp;

	private int m_currentPage;

	private List<string> m_pageVCStates;

	public bool m_isReady
	{
		get
		{
			if (!(m_widget != null))
			{
				return false;
			}
			return m_widget.IsReady;
		}
	}

	public Widget m_widget { get; private set; }

	public bool IsShowingGameXp => m_isShowingGameXp;

	private int m_pageCount
	{
		get
		{
			if (m_pageVCStates == null)
			{
				return 0;
			}
			return m_pageVCStates.Count;
		}
	}

	public void InitWidget(Action callback)
	{
		m_initCallback = callback;
		m_callback = null;
		m_questXpRewards = new Dictionary<Global.RewardTrackType, QuestXpRewardContainer>();
		m_pageVCStates = new List<string>();
		m_widget = WidgetInstance.Create(END_OF_GAME_MULTI_QUEST_REWARD_FLOW_PREFAB);
		SpecialEventDataModel specialEventDataModel = SpecialEventManager.Get()?.GetEventDataModelForCurrentEvent();
		if (specialEventDataModel != null)
		{
			m_widget.BindDataModel(specialEventDataModel);
		}
		m_widget.Hide();
		m_widget.RegisterReadyListener(delegate
		{
			OverlayUI.Get().AddGameObject(m_widget.gameObject);
			QuestXpReward[] componentsInChildren = m_widget.GetComponentsInChildren<QuestXpReward>(includeInactive: true);
			foreach (QuestXpReward questXpReward in componentsInChildren)
			{
				Global.RewardTrackType rewardTrackType = questXpReward.GetTrackType();
				if (rewardTrackType != 0)
				{
					if (ProgressUtils.IsEventRewardTrackType(rewardTrackType))
					{
						RewardTrack currentEventRewardTrack = RewardTrackManager.Get().GetCurrentEventRewardTrack();
						if (currentEventRewardTrack != null)
						{
							rewardTrackType = currentEventRewardTrack.TrackDataModel.RewardTrackType;
							questXpReward.SetTrackType(rewardTrackType);
						}
					}
					QuestXpRewardContainer questXpRewardContainer = new QuestXpRewardContainer();
					questXpRewardContainer.SetQuestXpReward(questXpReward);
					m_questXpRewards.Add(rewardTrackType, questXpRewardContainer);
				}
			}
			m_initCallback?.Invoke();
		});
	}

	public void Initialize(List<RewardTrackXpChange> xpChanges, Action callback)
	{
		if (xpChanges == null || xpChanges.Count == 0)
		{
			return;
		}
		foreach (RewardTrackXpChange change in xpChanges)
		{
			Global.RewardTrackType trackType = (Global.RewardTrackType)change.RewardTrackType;
			if (m_questXpRewards.TryGetValue(trackType, out var xpRewardContainer))
			{
				xpRewardContainer.AddXpChange(change);
			}
		}
		m_widget.Show();
		HashSet<Global.RewardTrackType> tracksWithChanges = new HashSet<Global.RewardTrackType>();
		foreach (KeyValuePair<Global.RewardTrackType, QuestXpRewardContainer> questXpReward in m_questXpRewards)
		{
			if (questXpReward.Value.m_hasXpChanges)
			{
				tracksWithChanges.Add(questXpReward.Key);
			}
		}
		m_questXpRewards.TryGetValue(Global.RewardTrackType.GLOBAL, out var globalXpRewardContainer);
		m_questXpRewards.TryGetValue(Global.RewardTrackType.BATTLEGROUNDS, out var bgXpRewardContainer);
		if (globalXpRewardContainer == null || bgXpRewardContainer == null)
		{
			Debug.LogError("QuestXpRewardHandler::Initialize - Global & BG Track Rewards Containers both expected.");
			return;
		}
		if (globalXpRewardContainer.m_hasXpChanges && bgXpRewardContainer.m_hasXpChanges)
		{
			globalXpRewardContainer.SetPageToShow(m_pageVCStates.Count);
			bgXpRewardContainer.SetPageToShow(m_pageVCStates.Count);
			m_pageVCStates.Add("GLOBAL_BG_COMBO");
			tracksWithChanges.Remove(Global.RewardTrackType.GLOBAL);
			tracksWithChanges.Remove(Global.RewardTrackType.BATTLEGROUNDS);
		}
		foreach (Global.RewardTrackType rewardTrackType in tracksWithChanges)
		{
			if (m_questXpRewards.TryGetValue(rewardTrackType, out var xpRewardContainer2))
			{
				xpRewardContainer2.SetPageToShow(m_pageVCStates.Count);
				m_pageVCStates.Add(VC_STATES_BY_TRACK_TYPE[rewardTrackType]);
			}
		}
		tracksWithChanges.Clear();
		QuestXpRewardContainer.InitializePageCounts(m_pageCount);
		m_isShowingGameXp = true;
		foreach (QuestXpRewardContainer value in m_questXpRewards.Values)
		{
			value.Initialize();
		}
		m_currentPage = 0;
		m_callback = callback;
		StartShowingXpGains();
	}

	public void StartShowingXpGains()
	{
		if (m_pageVCStates == null || m_pageVCStates.Count == 0)
		{
			Debug.LogError("QuestXpRewardHandler::StartShowingXpGains - Page VC States not initialized or missing.");
			return;
		}
		m_widget.GetComponentInChildren<VisualController>()?.SetState(m_pageVCStates[m_currentPage]);
		if (m_xpTypeQueue == null)
		{
			m_xpTypeQueue = new Queue<Global.RewardTrackType>();
		}
		else
		{
			m_xpTypeQueue.Clear();
		}
		foreach (Global.RewardTrackType trackType in m_questXpRewards.Keys)
		{
			if (m_questXpRewards.TryGetValue(trackType, out var xpRewardContainer) && xpRewardContainer.GetPageToShow() == m_currentPage)
			{
				if (!m_isShowingGameXp)
				{
					m_isShowingGameXp = xpRewardContainer.m_hasXpChanges;
				}
				xpRewardContainer.m_xpReward.ShowXpGains(delegate
				{
					OnShowXpFinish(trackType);
				});
			}
		}
	}

	public void ContinueNotifications()
	{
		if (m_questXpRewards != null && m_xpTypeQueue != null)
		{
			foreach (Global.RewardTrackType type in m_questXpRewards.Keys)
			{
				if (m_questXpRewards.TryGetValue(type, out var xpRewardContainer) && xpRewardContainer.GetPageToShow() == m_currentPage)
				{
					m_xpTypeQueue.Enqueue(type);
				}
			}
		}
		if (!ContinueNextInQueue() && m_currentPage < m_pageCount)
		{
			TurnPage();
		}
	}

	private bool ContinueNextInQueue()
	{
		if (m_xpTypeQueue == null || m_xpTypeQueue.Count == 0)
		{
			return false;
		}
		Global.RewardTrackType trackType = m_xpTypeQueue.Dequeue();
		if (!m_questXpRewards.TryGetValue(trackType, out var xpRewardContainer) || !xpRewardContainer.m_hasXpChanges)
		{
			return ContinueNextInQueue();
		}
		xpRewardContainer.m_xpReward.ContinueNotifications();
		return true;
	}

	public void ClearAndHide()
	{
		if (m_questXpRewards == null || m_widget == null)
		{
			return;
		}
		foreach (Global.RewardTrackType type in m_questXpRewards.Keys)
		{
			if (m_questXpRewards.TryGetValue(type, out var xpRewardContainer) && xpRewardContainer != null && !xpRewardContainer.m_finished)
			{
				xpRewardContainer.m_xpReward?.ClearAndHide();
			}
		}
		m_widget?.Hide();
		m_questXpRewards?.Clear();
		m_pageVCStates?.Clear();
		QuestXpRewardContainer.ClearUnfinished();
	}

	public void TurnPage()
	{
		m_currentPage++;
		if (m_currentPage < m_pageCount)
		{
			StartShowingXpGains();
		}
		else
		{
			Terminate();
		}
	}

	public void Terminate()
	{
		ClearAndHide();
		if (m_widget != null)
		{
			UnityEngine.Object.Destroy(m_widget.gameObject);
		}
		m_widget = null;
		m_callback?.Invoke();
	}

	private void OnShowXpFinish(Global.RewardTrackType trackType)
	{
		if (m_isShowingGameXp)
		{
			if (!m_questXpRewards.TryGetValue(trackType, out var xpRewardContainer))
			{
				return;
			}
			xpRewardContainer.SetGameXpFinished(isFinished: true);
			if (QuestXpRewardContainer.GetUnfinishedGameXpCount(m_currentPage) <= 0)
			{
				m_isShowingGameXp = false;
				if (!RewardXpNotificationManager.Get().JustShowGameXp || (xpRewardContainer.XpChangesRemaining() == 0 && QuestXpRewardContainer.GameXpDisplaysToShow() > 0))
				{
					ContinueNotifications();
				}
			}
		}
		else if (!ContinueNextInQueue())
		{
			TurnPage();
		}
	}
}
