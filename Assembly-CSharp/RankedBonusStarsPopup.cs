using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedBonusStarsPopup : BasicPopup
{
	public class BonusStarsPopupInfo : PopupInfo
	{
		public Action m_onHiddenCallback;
	}

	public UberText m_descriptionText;

	public UberText m_finePrintText;

	private const string SHOW_EVENT_NAME = "CODE_DIALOGMANAGER_SHOW";

	private const string HIDE_EVENT_NAME = "CODE_DIALOGMANAGER_HIDE";

	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private const string SETUP_SCENE_LOGIN = "SetUp_Scene_Login";

	private const string SETUP_SCENE_PLAYSCREEN = "SetUp_Scene_PlayScreen";

	private WidgetTemplate m_widget;

	protected override void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "Button_Framed_Clicked")
			{
				Hide();
			}
			if (eventName == "CODE_HIDE_FINISHED")
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		});
		m_widget.RegisterReadyListener(delegate
		{
			OnWidgetReady();
		});
	}

	protected override void OnDestroy()
	{
		GameObject go = base.transform.parent.gameObject;
		if (go != null && go.GetComponent<WidgetInstance>() != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		base.OnDestroy();
	}

	public override void Show()
	{
		if (!(m_widget == null))
		{
			OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: false, (!UniversalInputManager.UsePhoneUI) ? CanvasScaleMode.HEIGHT : CanvasScaleMode.WIDTH);
			UIContext.GetRoot().ShowPopup(base.gameObject);
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN || SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
			{
				m_widget.TriggerEvent("SetUp_Scene_Login");
			}
			else if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
			{
				m_widget.TriggerEvent("SetUp_Scene_PlayScreen");
			}
			m_widget.TriggerEvent("CODE_DIALOGMANAGER_SHOW");
		}
	}

	public override void Hide()
	{
		if (m_popupInfo is BonusStarsPopupInfo { m_onHiddenCallback: not null } popupInfo)
		{
			popupInfo.m_onHiddenCallback();
		}
		m_widget.TriggerEvent("CODE_DIALOGMANAGER_HIDE");
		IncrementBonusStarsPopupSeenCount();
	}

	private void OnWidgetReady()
	{
		IDataModel dataModel = null;
		m_widget.GetDataModel(123, out dataModel);
		if (dataModel is RankedPlayDataModel rankedPlayDataModel && m_descriptionText != null)
		{
			m_descriptionText.Text = GameStrings.Format("GLUE_RANKED_BONUS_STARS_DESCRIPTION", rankedPlayDataModel.StarMultiplier);
		}
	}

	private void IncrementBonusStarsPopupSeenCount()
	{
		TranslatedMedalInfo medalInfo = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedal(FormatType.FT_STANDARD);
		if (medalInfo != null)
		{
			long bonusStarsPopUpSeenCount = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_BONUS_STARS_POPUP_SEEN_COUNT, out bonusStarsPopUpSeenCount);
			List<GameSaveDataManager.SubkeySaveRequest> saveRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
			saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_SEASON_BONUS_STARS_POPUP_SEEN, medalInfo.seasonId));
			long[] array = new long[1];
			bonusStarsPopUpSeenCount = (array[0] = bonusStarsPopUpSeenCount + 1);
			saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_BONUS_STARS_POPUP_SEEN_COUNT, array));
			GameSaveDataManager.Get().SaveSubkeys(saveRequests);
		}
	}
}
