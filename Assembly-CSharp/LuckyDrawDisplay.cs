using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawDisplay : AbsSceneDisplay
{
	[Header("Lucky Draw Display")]
	public AsyncReference m_LuckyDrawWidgetReference;

	public Transform m_OnScreenBonePC;

	public Transform m_OnScreenBoneMobile;

	private LuckyDrawWidget m_luckyDrawWidget;

	private LuckyDrawManager m_luckyDrawManager;

	private bool m_luckyDrawWidgetFinishedLoading;

	[SerializeField]
	private Transform m_luckyDrawManagerRootTransform;

	private void Awake()
	{
		InitializeLuckyDrawManager();
	}

	private void InitializeLuckyDrawManager()
	{
		m_luckyDrawManager = LuckyDrawManager.Get();
		if (m_luckyDrawManager == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawDisplay] InitailizeLuckyDrawManager() lucky draw manager is null");
		}
		else if (!m_luckyDrawManager.IsIntialized())
		{
			m_luckyDrawManager.InitializeOrUpdateData();
		}
	}

	public override void Start()
	{
		base.Start();
		SetupLuckyDrawManagerPosition();
		m_luckyDrawManager.RegisterOnEventEndsListeners(OnLuckyDrawEventEnds);
		StartCoroutine(WaitForDataAndInitializeWidget());
		m_sceneDisplayWidget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "Button_Framed_Clicked")
			{
				OnBackButtonReleased();
			}
		});
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BATTLE_BASH_BUTTON_TOOLTIP, 1L));
		StartCoroutine(InitializeSceneObjects());
	}

	private void SetupLuckyDrawManagerPosition()
	{
		Transform onScreenBone = (PlatformSettings.IsMobile() ? m_OnScreenBoneMobile : m_OnScreenBonePC);
		m_luckyDrawManagerRootTransform.localPosition = onScreenBone.localPosition;
	}

	private IEnumerator InitializeSceneObjects()
	{
		while (SceneMgr.Get().IsTransitionNowOrPending() || !m_luckyDrawWidgetFinishedLoading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		m_luckyDrawWidget.SetupEndOfLoadSceneObjects();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_BattleBash);
	}

	private IEnumerator WaitForDataAndInitializeWidget()
	{
		while (m_luckyDrawManager.IsDataDirty())
		{
			yield return new WaitForSeconds(0.1f);
		}
		m_LuckyDrawWidgetReference.RegisterReadyListener<WidgetInstance>(OnLuckyDrawWidgetReady);
	}

	private void OnLuckyDrawWidgetReady(WidgetInstance widget)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawDisplay] OnLuckyDrawWidgetReady() widget was null!");
			return;
		}
		m_luckyDrawWidget = widget.Widget.GetComponent<LuckyDrawWidget>();
		Widget luckyDrawWidget = m_luckyDrawWidget?.GetComponent<Widget>();
		if (luckyDrawWidget == null)
		{
			Error.AddDevWarning("UI Error", "[LuckyDrawDisplay] OnLuckyDrawWidgetReady() could not find Widget on m_luckyDrawWidget!");
			return;
		}
		m_luckyDrawManager.SetShowHighlight(show: false);
		TelemetryManager.Client()?.SendLuckyDrawEventMessage("LuckyDrawPageEntered");
		m_luckyDrawManager.BindAllLuckyDrawDataModelToWidget(luckyDrawWidget);
		m_luckyDrawWidget.Show();
		m_luckyDrawWidgetFinishedLoading = true;
	}

	private void OnBackButtonReleased()
	{
		ReturnToBaconScene();
	}

	private void ReturnToBaconScene()
	{
		SetNextModeAndHandleTransition(SceneMgr.Mode.BACON, SceneMgr.TransitionHandlerType.NEXT_SCENE);
	}

	private void OnLuckyDrawEventEnds()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LUCKY_DRAW)
		{
			StartCoroutine(WaitThenShowBattleBashEndedPopupThenReturnToBaconScene());
		}
	}

	private IEnumerator WaitThenShowBattleBashEndedPopupThenReturnToBaconScene()
	{
		LuckyDrawManager luckyDrawManager = LuckyDrawManager.Get();
		while (luckyDrawManager.IsDataDirty())
		{
			yield return new WaitForSeconds(0.1f);
		}
		string bodyKey = ((luckyDrawManager.GetBattlegroundsLuckyDrawDataModel().Hammers > 0) ? "GLUE_BATTLEBASH_ALERT_EVENT_END_DESCRIPTION" : "GLUE_BATTLEBASH_ALERT_EVENT_END_DESCRIPTION_NO_HAMMERS");
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_BATTLEBASH_ALERT_EVENT_END_TITLE");
		info.m_text = GameStrings.Get(bodyKey);
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
		info.m_showAlertIcon = true;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
		ReturnToBaconScene();
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_luckyDrawWidgetFinishedLoading)
		{
			failureMessage = "LuckyDrawDisplay - LuckyDrawWidget never loaded";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	protected override bool ShouldStartShown()
	{
		return true;
	}

	private void OnDestroy()
	{
		if (m_luckyDrawManager != null)
		{
			m_luckyDrawManager.RemoveOnEventEndsListenders(OnLuckyDrawEventEnds);
		}
	}
}
