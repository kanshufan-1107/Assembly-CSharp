using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementSection : MonoBehaviour
{
	public const string SECTION_CHANGED = "CODE_SECTION_CHANGED";

	public const string SHOW_ACHIEVEMENT_SECTION_HEADER = "CODE_SHOW_ACHIEVEMENT_SECTION_HEADER";

	public const string HIDE_ACHIEVEMENT_SECTION_HEADER = "CODE_HIDE_ACHIEVEMENT_SECTION_HEADER";

	public const string SHOW_ACHIEVEMENT_TILE = "CODE_SHOW_ACHIEVEMENT_TILE";

	[SerializeField]
	private List<Widget> m_columWidgets = new List<Widget>();

	private List<AchievementListDataModel> m_achievementListDataModels = new List<AchievementListDataModel>();

	private Widget m_widget;

	private AchievementSectionDataModel m_sectionDataModel;

	private int m_numColumsToWaitFor;

	private Coroutine m_showDelayRoutine;

	public event Action<AchievementSectionDataModel> OnSectionChanged = delegate
	{
	};

	private void Start()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		m_widget.RegisterReadyListener(delegate
		{
			HandleSectionChanged();
			AchievementManager achievementManager = AchievementManager.Get();
			if (achievementManager != null)
			{
				achievementManager.OnStatusChanged += OnStatusChanged;
			}
		});
		m_columWidgets.ForEach(delegate(Widget x)
		{
			AchievementListDataModel achievementListDataModel = new AchievementListDataModel();
			m_achievementListDataModels.Add(achievementListDataModel);
			x.BindDataModel(achievementListDataModel);
		});
	}

	private void OnDisable()
	{
		if (m_showDelayRoutine != null)
		{
			StopCoroutine(m_showDelayRoutine);
		}
	}

	private void OnEnable()
	{
		ShowIfReady();
	}

	private void OnDestroy()
	{
		AchievementManager achievementMan = AchievementManager.Get();
		if (achievementMan != null)
		{
			achievementMan.OnStatusChanged -= OnStatusChanged;
		}
		if (m_showDelayRoutine != null)
		{
			StopCoroutine(m_showDelayRoutine);
		}
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "CODE_SECTION_CHANGED")
		{
			HandleSectionChanged();
		}
	}

	private void HandleSectionChanged()
	{
		AchievementSectionDataModel section = m_widget.GetDataModel<AchievementSectionDataModel>();
		if (section == null)
		{
			Debug.LogWarning("Unexpected state: no bound section");
		}
		else
		{
			if (section == m_sectionDataModel)
			{
				return;
			}
			if (m_showDelayRoutine != null)
			{
				StopCoroutine(m_showDelayRoutine);
			}
			m_sectionDataModel = section;
			m_widget.TriggerEvent("CODE_HIDE_ACHIEVEMENT_SECTION_HEADER");
			UpdateAchievementLists();
			m_numColumsToWaitFor = m_achievementListDataModels.Count((AchievementListDataModel x) => x.Achievements.Count > 0);
			this.OnSectionChanged(section);
			m_columWidgets.ForEach(delegate(Widget x)
			{
				Listable componentInChildren = x.GetComponentInChildren<Listable>();
				if (componentInChildren != null)
				{
					componentInChildren.RegisterDoneChangingStatesListener(delegate
					{
						m_numColumsToWaitFor--;
						ShowIfReady();
					}, null, callImmediatelyIfSet: true, doOnce: true);
				}
			});
		}
	}

	private void UpdateAchievementLists()
	{
		DataModelList<AchievementDataModel> displayedAchievements = m_sectionDataModel.SetDisplayedAchievements();
		int i = 0;
		while (i < m_columWidgets.Count)
		{
			m_achievementListDataModels[i].Achievements.OverwriteDataModels(displayedAchievements.Where((AchievementDataModel _, int index) => index % m_columWidgets.Count == i).ToDataModelList());
			int num = i + 1;
			i = num;
		}
	}

	private void OnStatusChanged(int achievementId, AchievementManager.AchievementStatus status)
	{
		if (status == AchievementManager.AchievementStatus.REWARD_GRANTED || status == AchievementManager.AchievementStatus.REWARD_ACKED)
		{
			AchievementDataModel achievement = FindAchievement(achievementId);
			if (achievement != null)
			{
				SelectNextTier(achievement);
			}
		}
	}

	private AchievementDataModel FindAchievement(int achievementId)
	{
		return m_sectionDataModel.Achievements.Achievements.FirstOrDefault((AchievementDataModel element) => element.ID == achievementId);
	}

	private void SelectNextTier(AchievementDataModel achievement)
	{
		AchievementDataModel nextAchievement = achievement.FindNextAchievement(m_sectionDataModel.Achievements.Achievements);
		if (nextAchievement != null)
		{
			for (int i = 0; i < m_columWidgets.Count && !ReplaceAchievement(achievement, nextAchievement, m_achievementListDataModels[i]); i++)
			{
			}
		}
	}

	private bool ReplaceAchievement(AchievementDataModel oldAchievement, AchievementDataModel newAchievement, AchievementListDataModel list)
	{
		if (list == null)
		{
			return false;
		}
		int index = list.Achievements.IndexOf(oldAchievement);
		if (index < 0)
		{
			return false;
		}
		list.Achievements[index] = newAchievement;
		return true;
	}

	private void ShowIfReady()
	{
		if (m_numColumsToWaitFor <= 0 && m_achievementListDataModels.Sum((AchievementListDataModel x) => x.Achievements.Count) != 0)
		{
			m_widget.TriggerEvent("CODE_SHOW_ACHIEVEMENT_TILE");
			m_showDelayRoutine = StartCoroutine(WaitAndShowHeader());
		}
	}

	private IEnumerator WaitAndShowHeader()
	{
		yield return new WaitForSeconds(m_sectionDataModel.DisplayDelay);
		m_widget.TriggerEvent("CODE_SHOW_ACHIEVEMENT_SECTION_HEADER");
	}
}
