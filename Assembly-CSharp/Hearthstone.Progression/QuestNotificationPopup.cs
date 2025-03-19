using System;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestNotificationPopup : MonoBehaviour
{
	public Widget m_questList;

	public int m_maxQuestsPerRow;

	private WidgetTemplate m_widget;

	private Action m_callback;

	private QuestPool.QuestPoolType m_questPoolType;

	private bool m_IKSShown;

	private bool m_shouldShowIKS;

	private const string CODE_HIDE = "CODE_HIDE";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE")
			{
				Hide();
			}
		});
		m_widget.RegisterDoneChangingStatesListener(HandleDoneChangingStates, null, callImmediatelyIfSet: true, doOnce: true);
	}

	private void OnDestroy()
	{
		m_callback?.Invoke();
		InnKeepersSpecial.UnregisterClickCallback(Hide);
		if (m_IKSShown)
		{
			InnKeepersSpecial.Close();
		}
	}

	public void Initialize(QuestListDataModel questListDataModel, Action callback, bool showIKS)
	{
		m_callback = callback;
		m_shouldShowIKS = showIKS;
		if (questListDataModel != null)
		{
			bool rewardTrackSet = false;
			if (questListDataModel.Quests.Count > m_maxQuestsPerRow)
			{
				questListDataModel.Quests.RemoveRange(m_maxQuestsPerRow, questListDataModel.Quests.Count - m_maxQuestsPerRow);
			}
			foreach (QuestDataModel quest in questListDataModel.Quests)
			{
				if (!rewardTrackSet && quest.RewardTrackType != 0)
				{
					RewardTrackDataModel rewardTrackDataModel = RewardTrackManager.Get().GetRewardTrack(quest.RewardTrackType).TrackDataModel;
					if (rewardTrackDataModel != null)
					{
						m_widget.BindDataModel(rewardTrackDataModel);
					}
					rewardTrackSet = true;
				}
				quest.DisplayMode = QuestManager.QuestDisplayMode.Notification;
			}
			questListDataModel.Quests.Sort(QuestManager.SortChainQuestsToFront);
			m_widget.BindDataModel(questListDataModel);
		}
		SceneMgr.Get().RegisterScenePreLoadEvent(OnPreLoadNextScene);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
	}

	public void Show()
	{
		OverlayUI.Get().AddGameObject(base.gameObject.transform.parent.gameObject);
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			if (m_shouldShowIKS)
			{
				InnKeepersSpecial.RegisterClickCallback(Hide);
				m_IKSShown = InnKeepersSpecial.CheckShow();
			}
			m_widget.TriggerEvent("SHOW");
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public void Hide()
	{
		AckQuests();
		SceneMgr.Get()?.UnregisterScenePreLoadEvent(OnPreLoadNextScene);
		FatalErrorMgr.Get()?.RemoveErrorListener(OnFatalError);
		if (base.transform != null && base.transform.parent != null && base.transform.parent.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
	}

	private void HandleDoneChangingStates(object unused)
	{
		Listable listable = m_questList.GetComponentInChildren<Listable>();
		if (!(listable != null))
		{
			return;
		}
		TransformUtil.SetPoint(listable.GetOrCreateColliderFromItemBounds(isEnabled: false), Anchor.CENTER_XZ, m_widget, Anchor.CENTER_XZ);
		Widget[] componentsInChildren = listable.gameObject.GetComponentsInChildren<Widget>();
		foreach (Widget widget in componentsInChildren)
		{
			QuestDataModel dataModel = widget.GetDataModel<QuestDataModel>();
			if (dataModel != null && dataModel.Status == QuestManager.QuestStatus.NEW)
			{
				widget.TriggerEvent("CODE_GRANTED");
			}
		}
	}

	private void AckQuests()
	{
		if (m_widget == null)
		{
			return;
		}
		QuestListDataModel dataModel = m_widget.GetDataModel<QuestListDataModel>();
		if (dataModel == null)
		{
			return;
		}
		QuestManager questManager = QuestManager.Get();
		if (questManager == null)
		{
			return;
		}
		foreach (QuestDataModel questDataModel in dataModel.Quests)
		{
			if (questDataModel.Status == QuestManager.QuestStatus.NEW)
			{
				questManager.AckQuest(questDataModel.QuestId);
			}
		}
	}

	private void OnPreLoadNextScene(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		Hide();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		Hide();
	}
}
