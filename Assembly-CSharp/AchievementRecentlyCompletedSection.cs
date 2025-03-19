using System;
using System.Collections;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

public class AchievementRecentlyCompletedSection : MonoBehaviour
{
	public const string SHOW_ACHIEVEMENT_TILE = "CODE_SHOW_ACHIEVEMENT_TILE";

	public const string HIDE_ACHIEVEMENT_TILE = "CODE_HIDE_ACHIEVEMENT_TILE";

	public const string SHOW_COMPLETION_DATE = "SHOW_COMPLETION_DATE";

	public const string START_HIDE_SEQUENCE = "START_HIDE_SEQUENCE";

	public const string HIDE_ANIMATION_COMPLETED = "CODE_HIDE_ANIMATION_COMPLETED";

	public const string CLAIM_ANIMATION_STARTED = "CODE_CLAIM_ANIMATION_STARTED";

	public const string CLAIM_ANIMATION_COMPLETED = "CODE_CLAIM_ANIMATION_COMPLETED";

	public const string SHOW_WIDGET = "SHOW";

	[SerializeField]
	private UberText m_HeaderText;

	[SerializeField]
	private int m_maxAchievementsToShow;

	private Widget m_widget;

	private Listable m_listable;

	private readonly AchievementListDataModel m_achievementListDataModel = new AchievementListDataModel();

	private int m_numAnimationsPlaying;

	private Coroutine m_updatePositionCoroutine;

	private Coroutine m_updateDisplayOrderCoroutine;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_HeaderText.Text = GameStrings.Get("GLUE_PROGRESSION_ACHIEVEMENTS_RECENTLY_COMPLETED");
		m_widget.BindDataModel(m_achievementListDataModel);
		m_widget.RegisterEventListener(HandleEvent);
		m_widget.RegisterReadyListener(delegate
		{
			m_listable = GetComponentInChildren<Listable>();
		});
		ServiceManager.Get<GameDownloadManager>()?.RegisterModuleInstallationStateChangeListener(OnModuleInstallationStateChanged, invokeImmediately: false);
		AchievementManager.Get().OnStatusChanged += OnAchievementStatusChanged;
	}

	public void OnDestroy()
	{
		AchievementManager achievementManager = AchievementManager.Get();
		if (achievementManager != null)
		{
			achievementManager.OnStatusChanged -= OnAchievementStatusChanged;
		}
		ServiceManager.Get<GameDownloadManager>()?.RegisterModuleInstallationStateChangeListener(OnModuleInstallationStateChanged, invokeImmediately: false);
		KillCoroutines();
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_HIDE_ANIMATION_COMPLETED":
			m_numAnimationsPlaying--;
			UpdateAchievementDisplayOrder();
			break;
		case "CODE_CLAIM_ANIMATION_STARTED":
			m_numAnimationsPlaying++;
			break;
		case "CODE_CLAIM_ANIMATION_COMPLETED":
			HandleClaimAnimationComplete();
			break;
		case "SHOW":
			AchievementManager.Get().ClearClaimedDates();
			UpdateAchievementDisplayOrder();
			break;
		}
	}

	public void OnDisable()
	{
		KillCoroutines();
		m_numAnimationsPlaying = 0;
	}

	private void KillCoroutines()
	{
		if (m_updatePositionCoroutine != null)
		{
			StopCoroutine(m_updatePositionCoroutine);
		}
		if (m_updateDisplayOrderCoroutine != null)
		{
			StopCoroutine(m_updateDisplayOrderCoroutine);
		}
	}

	private void UpdateAchievementDisplayOrder()
	{
		if (m_updateDisplayOrderCoroutine == null)
		{
			m_updateDisplayOrderCoroutine = StartCoroutine(UpdateAchievementDisplayOrderRoutine());
		}
	}

	private IEnumerator UpdateAchievementDisplayOrderRoutine()
	{
		while (m_numAnimationsPlaying > 0)
		{
			yield return null;
		}
		DataModelList<AchievementDataModel> achievements = AchievementManager.Get().GetRecentlyCompletedAchievements().GetCurrentSortedAchievements()
			.SortByStatusThenClaimedDate()
			.Take(m_maxAchievementsToShow)
			.ToDataModelList();
		foreach (AchievementDataModel item in achievements)
		{
			ProgressUtils.UpdateAchievementIsLocked(item);
		}
		AchievementManager.Get().LoadRewards(achievements);
		m_achievementListDataModel.Achievements.OverwriteDataModels(achievements);
		m_updateDisplayOrderCoroutine = null;
	}

	private void UpdateSingleAchievement(AchievementDataModel oldAchievement, AchievementDataModel newAchievement)
	{
		int index = m_achievementListDataModel.Achievements.IndexOf(oldAchievement);
		AchievementManager.Get().LoadReward(newAchievement);
		m_achievementListDataModel.Achievements[index] = newAchievement;
	}

	private IEnumerator UpdateListPositions()
	{
		while (m_numAnimationsPlaying > 0)
		{
			m_listable.UpdatePositions();
			yield return null;
		}
		while (m_listable.IsChangingStates)
		{
			m_listable.UpdatePositions();
			yield return null;
		}
		int frameCount = 2;
		while (frameCount > 0)
		{
			m_listable.UpdatePositions();
			yield return null;
			int num = frameCount - 1;
			frameCount = num;
		}
		m_updatePositionCoroutine = null;
	}

	private void OnAchievementStatusChanged(int achievementId, AchievementManager.AchievementStatus status)
	{
		if (status == AchievementManager.AchievementStatus.COMPLETED)
		{
			UpdateAchievementDisplayOrder();
			return;
		}
		AchievementDbfRecord achievement = GameDbf.Achievement.GetRecord(achievementId);
		if (achievement != null && achievement.RewardListRecord != null && achievement.RewardListRecord.ChooseOne && ProgressUtils.IsAchievementClaimed(status))
		{
			UpdateAchievementDisplayOrder();
		}
	}

	private bool IsAchievementLastClaimableInList(AchievementDataModel achievement)
	{
		int nextIndex = m_achievementListDataModel.Achievements.IndexOf(achievement) + 1;
		if (nextIndex < m_achievementListDataModel.Achievements.Count && m_achievementListDataModel.Achievements[nextIndex].Status == AchievementManager.AchievementStatus.COMPLETED)
		{
			return false;
		}
		return true;
	}

	private void HandleClaimAnimationComplete()
	{
		m_numAnimationsPlaying--;
		m_widget.TriggerEvent("SHOW_COMPLETION_DATE");
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel.Payload is IConvertible)
		{
			int achievementId = Convert.ToInt32(eventDataModel.Payload);
			AchievementDataModel achievement = AchievementManager.Get().GetAchievementDataModel(achievementId);
			if (achievement == null)
			{
				UpdateAchievementDisplayOrder();
				return;
			}
			AchievementDataModel nextAchievement = achievement.FindNextAchievement(AchievementManager.Get().GetRecentlyCompletedAchievements());
			if (nextAchievement != null && nextAchievement.Status == AchievementManager.AchievementStatus.COMPLETED)
			{
				UpdateSingleAchievement(achievement, nextAchievement);
			}
			else if (!IsAchievementLastClaimableInList(achievement))
			{
				m_numAnimationsPlaying++;
				if (m_updatePositionCoroutine == null)
				{
					m_updatePositionCoroutine = StartCoroutine(UpdateListPositions());
				}
				m_widget.TriggerEvent("START_HIDE_SEQUENCE", new TriggerEventParameters(null, achievementId));
			}
		}
		else
		{
			UpdateAchievementDisplayOrder();
		}
	}

	private void OnModuleInstallationStateChanged(DownloadTags.Content moduleTag, ModuleState state)
	{
		m_achievementListDataModel.Achievements.ForEach(ProgressUtils.UpdateAchievementIsLocked);
	}
}
