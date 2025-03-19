using System;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedIntroPopup : BasicPopup
{
	public class RankedIntroPopupInfo : PopupInfo
	{
		public Action m_onHiddenCallback;
	}

	public GameObject SeasonIntroPackRoot;

	private const string SHOW_EVENT_NAME = "CODE_DIALOGMANAGER_SHOW";

	private const string HIDE_EVENT_NAME = "CODE_DIALOGMANAGER_HIDE";

	private const string HIDE_SEASON_POPUP_EVENT_NAME = "CODE_SEASONSMANAGER_HIDE";

	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private const string SETUP_SCENE_LOGIN = "SetUp_Scene_Login";

	private const string SETUP_SCENE_PLAYSCREEN = "SetUp_Scene_PlayScreen";

	private WidgetTemplate m_widget;

	private bool m_starsInfoShowing = true;

	protected override void Awake()
	{
		m_starsInfoShowing = true;
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "Button_Framed_Clicked")
			{
				Hide();
			}
			if (eventName == "CODE_HIDE_FINISHED" && m_readyToDestroyCallback != null)
			{
				m_readyToDestroyCallback(this);
			}
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
			InitSeasonsIntroPackData();
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
		if (m_starsInfoShowing)
		{
			m_starsInfoShowing = false;
			m_widget.TriggerEvent("CODE_DIALOGMANAGER_HIDE");
			return;
		}
		if (m_popupInfo is RankedIntroPopupInfo { m_onHiddenCallback: not null } popupInfo)
		{
			popupInfo.m_onHiddenCallback();
		}
		m_widget.TriggerEvent("CODE_SEASONSMANAGER_HIDE");
		IncrementRankedIntroPopupSeenCount();
	}

	private void IncrementRankedIntroPopupSeenCount()
	{
		if (RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedal(FormatType.FT_STANDARD) != null)
		{
			long rankedIntroSeenCount = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_INTRO_SEEN_COUNT, out rankedIntroSeenCount);
			GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
			long[] array = new long[1];
			rankedIntroSeenCount = (array[0] = rankedIntroSeenCount + 1);
			gameSaveDataManager.SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_INTRO_SEEN_COUNT, array));
		}
	}

	private void InitSeasonsIntroPackData()
	{
		if (SeasonIntroPackRoot == null)
		{
			Log.All.PrintError("[RankedIntroPopup] Unable to set pack data because we are missing the pack display root");
			return;
		}
		NetCache.NetCacheMedalInfo medalInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
		if (medalInfo == null)
		{
			Log.All.PrintError("[RankedIntroPopup] Unable to set pack data because medal info is missing");
			return;
		}
		int currentSeasonId = new MedalInfoTranslator(medalInfo).GetCurrentSeasonId();
		PackDataModel packDataModel = new PackDataModel
		{
			Type = (BoosterDbId)RankMgr.Get().GetRankedRewardBoosterIdForSeasonId(currentSeasonId),
			Quantity = 1
		};
		m_widget.BindDataModel(packDataModel, SeasonIntroPackRoot);
	}
}
