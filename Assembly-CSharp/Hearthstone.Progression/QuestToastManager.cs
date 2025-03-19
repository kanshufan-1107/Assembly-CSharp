using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;

namespace Hearthstone.Progression;

public class QuestToastManager : IService
{
	public static readonly AssetReference QUEST_PROGRESS_TOAST_PREFAB = new AssetReference("QuestProgressToast.prefab:a14a1594ad6e85242a7b50b26c840edd");

	private Dictionary<QuestPool.QuestPoolType, List<QuestDataModel>> m_questListMap = new Dictionary<QuestPool.QuestPoolType, List<QuestDataModel>>();

	private List<QuestProgressToast> m_activeToasts = new List<QuestProgressToast>();

	private int m_toastsToLoad;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		QuestManager.Get().OnQuestProgress += OnQuestProgress;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(QuestManager)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<QuestManager>(out var questManager))
		{
			questManager.OnQuestProgress -= OnQuestProgress;
		}
	}

	private void OnQuestProgress(QuestDataModel questDataModel)
	{
		if (m_questListMap.TryGetValue(questDataModel.PoolType, out var questList))
		{
			questList.Add(questDataModel);
			return;
		}
		questList = new List<QuestDataModel>();
		questList.Add(questDataModel);
		m_questListMap.Add(questDataModel.PoolType, questList);
	}

	private void WillReset()
	{
		m_questListMap.Clear();
	}

	public static QuestToastManager Get()
	{
		return ServiceManager.Get<QuestToastManager>();
	}

	public void ShowNextQuestProgress()
	{
		if (AreToastsActive())
		{
			return;
		}
		List<QuestDataModel> listToShow = new List<QuestDataModel>();
		int numToShow = ((PlatformSettings.Screen == ScreenCategory.Phone) ? 4 : 6);
		Dictionary<QuestPool.QuestPoolType, List<QuestDataModel>>.Enumerator questListIter = m_questListMap.GetEnumerator();
		while (listToShow.Count < numToShow && questListIter.MoveNext())
		{
			List<QuestDataModel> questList = questListIter.Current.Value;
			if (questList.Count != 0)
			{
				if (questList.Count + listToShow.Count <= numToShow)
				{
					listToShow.AddRange(questList);
					questList.Clear();
					continue;
				}
				int rangeSize = numToShow - listToShow.Count;
				rangeSize = Math.Min(rangeSize, questList.Count);
				listToShow.AddRange(questList.GetRange(0, rangeSize));
				questList.RemoveRange(0, rangeSize);
			}
		}
		ShowQuestProgress(listToShow);
	}

	private void ShowQuestProgress(List<QuestDataModel> questList)
	{
		m_toastsToLoad = questList.Count;
		foreach (QuestDataModel quest in questList)
		{
			Widget widget = WidgetInstance.Create(QUEST_PROGRESS_TOAST_PREFAB);
			widget.RegisterReadyListener(delegate
			{
				QuestProgressToast componentInChildren = widget.GetComponentInChildren<QuestProgressToast>();
				componentInChildren.Initialize(quest);
				m_activeToasts.Add(componentInChildren);
				m_toastsToLoad--;
				componentInChildren.Show();
			});
			widget.RegisterDoneChangingStatesListener(delegate
			{
				UpdateToastPositions();
			});
			widget.RegisterDeactivatedListener(delegate
			{
				m_activeToasts.Remove(widget.GetComponentInChildren<QuestProgressToast>());
				UpdateToastPositions();
			});
		}
		questList.Clear();
	}

	public bool AreToastsActive()
	{
		if (m_activeToasts.Count <= 0)
		{
			return m_toastsToLoad > 0;
		}
		return true;
	}

	private void UpdateToastPositions()
	{
		for (int i = 1; i < m_activeToasts.Count; i++)
		{
			TransformUtil.SetPoint(m_activeToasts[i].gameObject, Anchor.BOTTOM, m_activeToasts[i - 1].gameObject, Anchor.TOP, m_activeToasts[i - 1].GetOffset());
		}
		if (!AreToastsActive())
		{
			ShowNextQuestProgress();
		}
	}
}
