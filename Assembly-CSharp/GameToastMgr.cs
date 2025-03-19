using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameToastMgr : MonoBehaviour
{
	public delegate void QuestProgressToastShownCallback(int achieveId);

	private class QuestProgressToastShownListener : EventListener<QuestProgressToastShownCallback>
	{
		public void Fire(int achieveId)
		{
			m_callback(achieveId);
		}
	}

	private const float FADE_IN_TIME = 0.25f;

	private const float FADE_OUT_TIME = 0.5f;

	private const float HOLD_TIME = 4f;

	private PlatformDependentValue<Vector3> MULTIPLE_TOAST_OFFSET = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 0f, 30f),
		Phone = new Vector3(0f, 0f, 80f)
	};

	public QuestProgressToast m_questProgressToastPrefab;

	private static GameToastMgr s_instance;

	private List<GameToast> m_toasts = new List<GameToast>();

	private List<QuestProgressToastShownListener> m_questProgressToastShownListeners = new List<QuestProgressToastShownListener>();

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static GameToastMgr Get()
	{
		return s_instance;
	}

	public bool RegisterQuestProgressToastShownListener(QuestProgressToastShownCallback callback)
	{
		if (callback == null)
		{
			return false;
		}
		QuestProgressToastShownListener listener = new QuestProgressToastShownListener();
		listener.SetCallback(callback);
		listener.SetUserData(null);
		if (m_questProgressToastShownListeners.Contains(listener))
		{
			return false;
		}
		m_questProgressToastShownListeners.Add(listener);
		return true;
	}

	public bool RemoveQuestProgressToastShownListener(QuestProgressToastShownCallback callback)
	{
		if (callback == null)
		{
			return false;
		}
		QuestProgressToastShownListener listener = new QuestProgressToastShownListener();
		listener.SetCallback(callback);
		listener.SetUserData(null);
		return m_questProgressToastShownListeners.Remove(listener);
	}

	private void FireAllQuestProgressListeners(int achieveId, int progress)
	{
		QuestProgressToastShownListener[] array = m_questProgressToastShownListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(achieveId);
		}
	}

	private bool AddToast(GameToast toast)
	{
		toast.transform.parent = OverlayUI.Get().m_QuestProgressToastBone.transform;
		toast.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		toast.transform.localScale = new Vector3(110f, 1f, 110f);
		toast.transform.localPosition = Vector3.zero;
		m_toasts.Add(toast);
		RenderUtils.SetAlpha(toast.gameObject, 0f);
		UpdateToastPositions();
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", 0.25f);
		args.Add("delay", 0.25f);
		args.Add("easetype", iTween.EaseType.easeInCubic);
		args.Add("oncomplete", "FadeOutToast");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", toast);
		iTween.FadeTo(toast.gameObject, args);
		return true;
	}

	public bool AreToastsActive()
	{
		return m_toasts.Count > 0;
	}

	private void FadeOutToast(GameToast toast)
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 0f);
		args.Add("delay", 4f);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeInCubic);
		args.Add("oncomplete", "DeactivateToast");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", toast);
		iTween.FadeTo(toast.gameObject, args);
	}

	private void DeactivateToast(GameToast toast)
	{
		toast.gameObject.SetActive(value: false);
		m_toasts.Remove(toast);
		UpdateToastPositions();
	}

	private void UpdateToastPositions()
	{
		int count = 0;
		foreach (GameToast toast in m_toasts)
		{
			if (count > 0)
			{
				TransformUtil.SetPoint(toast.gameObject, Anchor.BOTTOM, m_toasts[count - 1].gameObject, Anchor.TOP, MULTIPLE_TOAST_OFFSET);
			}
			count++;
		}
	}

	public void UpdateQuestProgressToasts()
	{
		ShowQuestProgressToasts(AchieveManager.Get().GetNewlyProgressedQuests());
	}

	public void ShowQuestProgressToasts(List<Achievement> achievements)
	{
		foreach (Achievement achievement in achievements)
		{
			AddQuestProgressToast(achievement.ID, achievement.Name, achievement.Description, achievement.Progress, achievement.MaxProgress);
			achievement.AckCurrentProgressAndRewardNotices(ackIntermediateProgress: true);
		}
	}

	public void AddQuestProgressToast(int achieveId, string questName, string questDescription, int progress, int maxProgress)
	{
		QuestProgressToast toast = Object.Instantiate(m_questProgressToastPrefab);
		toast.UpdateDisplay(questName, questDescription, progress, maxProgress);
		if (AddToast(toast))
		{
			FireAllQuestProgressListeners(achieveId, progress);
		}
	}
}
