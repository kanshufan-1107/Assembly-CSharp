using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using UnityEngine;

[CustomEditClass]
public class GameMenu : ButtonListMenu, GameMenuInterface
{
	private enum CONCEDE_WARNING
	{
		NONE,
		NO_QUEST_PROGRESS,
		LEAVE_TEAMMATE_PENALTY
	}

	[CustomEditField(Sections = "Template Items")]
	public Vector3 m_ratingsObjectMinPadding = new Vector3(0f, 0f, -0.06f);

	public Transform m_menuBone;

	public Material m_redButtonMaterial;

	public string m_anchorForKoreanRatings;

	private static GameMenu s_instance;

	private GameMenuBase m_gameMenuBase;

	private UIBButton m_concedeButton;

	private UIBButton m_endGameButton;

	private UIBButton m_leaveButton;

	private UIBButton m_restartButton;

	private UIBButton m_quitButton;

	private UIBButton m_switchAccountButton;

	private UIBButton m_loginButton;

	private UIBButton m_optionsButton;

	private UIBButton m_downloadManagerButton;

	private UIBButton m_signUpButton;

	private UIBButton m_skipNprButton;

	private UIBButton m_skipTutorial;

	private Notification m_loginButtonPopup;

	private bool m_hasSeenLoginTooltip;

	private BnetRegion m_AccountRegion;

	private GameObject m_ratingsObject;

	private Transform m_ratingsAnchor;

	private RegionSwitchMenuController m_regionSwitchMenuController = new RegionSwitchMenuController();

	private readonly Vector3 BUTTON_SCALE = 15f * Vector3.one;

	private readonly Vector3 BUTTON_SCALE_PHONE = 25f * Vector3.one;

	private int m_minTurnsForProgressAfterConcede;

	private int m_minHPForProgressAfterConcede;

	private bool m_teammateConceded;

	private int m_bgDuosLeaverPenalty;

	private int m_bgMinTurnsForProgressAfterConede;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	protected override void Awake()
	{
		m_menuParent = m_menuBone;
		m_targetLayer = GameLayer.HighPriorityUI;
		base.Awake();
		s_instance = this;
		m_useTextSizesInSlices = m_menu.m_useTextSizesInSlices;
		m_gameMenuBase = new GameMenuBase();
		m_gameMenuBase.m_showCallback = delegate
		{
			Show();
		};
		m_gameMenuBase.m_hideCallback = Hide;
		LoadRatings();
		m_concedeButton = CreateMenuButton("ConcedeButton", "GLOBAL_CONCEDE", ConcedeButtonPressed);
		ButtonListMenu.MakeButtonRed(m_concedeButton, m_redButtonMaterial);
		m_endGameButton = CreateMenuButton("EndGameButton", "GLOBAL_END_GAME", ConcedeButtonPressed);
		ButtonListMenu.MakeButtonRed(m_endGameButton, m_redButtonMaterial);
		m_leaveButton = CreateMenuButton("LeaveButton", "GLOBAL_LEAVE_SPECTATOR_MODE", LeaveButtonPressed);
		m_restartButton = CreateMenuButton("RestartButton", "GLOBAL_RESTART", RestartButtonPressed);
		if ((bool)HearthstoneApplication.CanQuitGame)
		{
			m_quitButton = CreateMenuButton("QuitButton", "GLOBAL_QUIT", QuitButtonPressed);
		}
		if (PlatformSettings.IsMobile() || PlatformSettings.IsSteam)
		{
			m_switchAccountButton = CreateMenuButton("LogoutButton", Network.ShouldBeConnectedToAurora() ? "GLOBAL_SWITCH_ACCOUNT" : "GLOBAL_LOGIN", LogoutButtonPressed);
			m_loginButton = CreateMenuButton("LoginButton", "GLOBAL_LOGIN", LoginButtonPressed);
		}
		m_optionsButton = CreateMenuButton("OptionsButton", "GLOBAL_OPTIONS", OptionsButtonPressed);
		if (m_menu.m_templateDownloadButton != null)
		{
			m_downloadManagerButton = CreateMenuButton("DownloadManagerButton", "GLOBAL_GAME_MENU_BUTTON_DOWNLOAD_MANAGER", DownloadManagerPressed, m_menu.m_templateDownloadButton);
		}
		if (m_menu.m_templateSignUpButton != null)
		{
			m_signUpButton = CreateMenuButton("SignUpButton", "GLUE_TEMPORARY_ACCOUNT_SIGN_UP", OnSignUpPressed, m_menu.m_templateSignUpButton);
		}
		m_menu.m_headerText.Text = GameStrings.Get("GLOBAL_GAME_MENU");
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		m_minTurnsForProgressAfterConcede = guardianVars.MinTurnsForProgressAfterConcede;
		m_minHPForProgressAfterConcede = guardianVars.MinHPForProgressAfterConcede;
		m_bgDuosLeaverPenalty = (int)(-1000f * guardianVars.BGDuosLeaverRatingPenalty);
		m_bgMinTurnsForProgressAfterConede = guardianVars.BGMinTurnsForProgressAfterConcede;
		if ((bool)m_menu.m_templateSkipApprenticeButton)
		{
			m_skipNprButton = CreateMenuButton("SkipNprButton", "GLOBAL_OPTIONS_SKIP_NPR", OnSkipNprButtonReleased, m_menu.m_templateSkipApprenticeButton);
			ButtonListMenu.MakeButtonRed(m_skipNprButton, m_redButtonMaterial);
		}
		if ((bool)m_menu.m_templateButton)
		{
			m_skipTutorial = CreateMenuButton("SkipTutorialButton", "GLUE_TEMPORARY_ACCOUNT_SKIP", OnSkipTutorialButtonReleased, m_menu.m_templateButton);
			ButtonListMenu.MakeButtonRed(m_skipTutorial, m_redButtonMaterial);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_gameMenuBase.DestroyOptionsMenu();
		s_instance = null;
	}

	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		string buttonText = GameStrings.Get("GLOBAL_LEAVE_SPECTATOR_MODE");
		m_leaveButton.SetText(buttonText);
	}

	public bool GameMenuIsShown()
	{
		return IsShown();
	}

	public void GameMenuShow()
	{
		Show();
	}

	public void GameMenuHide()
	{
		Hide();
	}

	public void GameMenuShowOptionsMenu()
	{
		ShowOptionsMenu();
	}

	public GameObject GameMenuGetGameObject()
	{
		return base.gameObject;
	}

	public static GameMenu Get()
	{
		return s_instance;
	}

	public override void Show(bool playSound = true)
	{
		if (MiscellaneousMenu.Get() != null && MiscellaneousMenu.Get().IsShown())
		{
			MiscellaneousMenu.Get().Hide();
		}
		if (OptionsMenu.Get() != null && OptionsMenu.Get().IsShown())
		{
			UniversalInputManager.Get().CancelTextInput(base.gameObject, force: true);
			OptionsMenu.Get().Hide();
			return;
		}
		UpdateConcedeButtonAlternativeText();
		base.Show(playSound);
		if ((bool)UniversalInputManager.UsePhoneUI && m_ratingsObject != null)
		{
			m_ratingsObject.SetActive(m_gameMenuBase.UseKoreanRating());
		}
		ShowCursorIfNeeded();
		ShowLoginTooltipIfNeeded();
		BnetBar.Get().m_menuButton.SetSelected(enable: true);
	}

	public override void Hide()
	{
		base.Hide();
		HideLoginTooltip();
		BnetBar.Get().m_menuButton.SetSelected(enable: false);
	}

	public void ShowCursorIfNeeded()
	{
		if (PegCursor.Get() != null)
		{
			PegCursor.Get().Show();
		}
	}

	public void ShowLoginTooltipIfNeeded()
	{
		if (!Network.ShouldBeConnectedToAurora() && !m_hasSeenLoginTooltip)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				Vector3 popupSpot = new Vector3(-82.9f, 42.1f, 17.2f);
				m_loginButtonPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popupSpot, BUTTON_SCALE_PHONE, GameStrings.Get("GLOBAL_MOBILE_LOG_IN_TOOLTIP"), convertLegacyPosition: false);
			}
			else
			{
				Vector3 popupSpot = new Vector3(-46.9f, 34.2f, 9.4f);
				m_loginButtonPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popupSpot, BUTTON_SCALE, GameStrings.Get("GLOBAL_MOBILE_LOG_IN_TOOLTIP"), convertLegacyPosition: false);
			}
			if (m_loginButtonPopup != null)
			{
				m_loginButtonPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
				m_hasSeenLoginTooltip = true;
			}
		}
	}

	public void HideLoginTooltip()
	{
		if (m_loginButtonPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_loginButtonPopup);
		}
		m_loginButtonPopup = null;
	}

	public static bool IsInGameMenu()
	{
		if (!SceneMgr.Get().IsInGame() || !SceneMgr.Get().IsSceneLoaded() || LoadingScreen.Get().IsTransitioning())
		{
			return false;
		}
		if (GameState.Get() == null)
		{
			return false;
		}
		if (GameState.Get().IsGameOver())
		{
			return false;
		}
		if (TutorialProgressScreen.Get() != null && TutorialProgressScreen.Get().gameObject.activeInHierarchy)
		{
			return false;
		}
		return true;
	}

	public static bool CanLogInOrCreateAccount()
	{
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (currentMode == SceneMgr.Mode.STARTUP || currentMode == SceneMgr.Mode.LOGIN)
		{
			return false;
		}
		return true;
	}

	public void ShowOptionsMenu()
	{
		if (m_gameMenuBase != null)
		{
			m_gameMenuBase.ShowOptionsMenu();
		}
	}

	public void SetTeammateConceded(bool conceded)
	{
		m_teammateConceded = conceded;
	}

	protected override List<UIBButton> GetButtons()
	{
		List<UIBButton> buttons = new List<UIBButton>();
		bool InGame = IsInGameMenu();
		if (InGame)
		{
			bool hasTopLevelButton = false;
			if (GameUtils.CanConcedeCurrentMission())
			{
				if (GameUtils.IsWaitingForOpponentReconnect())
				{
					buttons.Add(m_endGameButton);
				}
				else
				{
					buttons.Add(m_concedeButton);
				}
				hasTopLevelButton = true;
			}
			if (SpectatorManager.Get().IsSpectatingOrWatching)
			{
				buttons.Add(m_leaveButton);
				hasTopLevelButton = true;
			}
			if (GameUtils.CanRestartCurrentMission() && !ShouldHideRestartButton())
			{
				buttons.Add(m_restartButton);
				hasTopLevelButton = true;
			}
			if (hasTopLevelButton)
			{
				buttons.Add(null);
			}
		}
		bool isSignUpButtonVisible = !IsInGameMenu() && CanLogInOrCreateAccount() && TemporaryAccountManager.IsTemporaryAccount() && m_signUpButton != null && !InGame;
		bool isSkipApprenticeButtonVisible = GameUtils.CanSkipApprentice() && !CollectionManager.Get().IsInEditMode() && Network.IsLoggedIn() && !IsInGameMenu();
		bool useDenseLayout = isSkipApprenticeButtonVisible && isSignUpButtonVisible && (bool)UniversalInputManager.UsePhoneUI;
		if (isSignUpButtonVisible)
		{
			buttons.Add(m_signUpButton);
			if (!useDenseLayout)
			{
				buttons.Add(null);
			}
			buttons.Add(m_loginButton);
		}
		if (isSkipApprenticeButtonVisible)
		{
			buttons.Add(m_skipNprButton);
			if (!useDenseLayout)
			{
				buttons.Add(null);
			}
		}
		if (Network.IsLoggedIn() && BnetBar.Get().ShowShowSkipTutorialButton && IsInGameMenu())
		{
			buttons.Add(m_skipTutorial);
			buttons.Add(null);
		}
		bool num = DemoMgr.Get().IsExpoDemo();
		if (!num && !isSignUpButtonVisible && !InGame && CanLogInOrCreateAccount() && (PlatformSettings.IsMobile() || PlatformSettings.IsSteam))
		{
			buttons.Add(m_switchAccountButton);
		}
		if (!num)
		{
			buttons.Add(m_optionsButton);
		}
		if (m_downloadManagerButton != null && !InGame && PlatformSettings.IsMobile() && DownloadManager != null && DownloadManager.IsReadyToPlay && !(isSignUpButtonVisible && isSkipApprenticeButtonVisible))
		{
			buttons.Add(m_downloadManagerButton);
		}
		if (!num && (bool)HearthstoneApplication.CanQuitGame && !useDenseLayout)
		{
			if (buttons[buttons.Count - 1] != null)
			{
				buttons.Add(null);
			}
			buttons.Add(m_quitButton);
		}
		return buttons;
	}

	protected override void LayoutMenu()
	{
		LayoutMenuButtons();
		m_menu.m_buttonContainer.UpdateSlices();
		LayoutMenuBackground();
		if (m_ratingsObject != null && m_ratingsAnchor != null)
		{
			m_ratingsObject.transform.position = m_ratingsAnchor.position;
		}
	}

	private void QuitButtonPressed(UIEvent e)
	{
		HearthstoneApplication.Get().Exit();
	}

	private void LogoutButtonPressed(UIEvent e)
	{
		HideLoginTooltip();
		Hide();
		m_regionSwitchMenuController.ShowRegionMenuWithDefaultSettings();
	}

	private void LoginButtonPressed(UIEvent e)
	{
		Hide();
		TemporaryAccountManager.Get().ShowMergeAccountPage();
	}

	private void ConcedeButtonPressed(UIEvent e)
	{
		GameState gameState = GameState.Get();
		CONCEDE_WARNING concedeWarning;
		if (gameState == null)
		{
			Hide();
		}
		else if (GameMgr.Get().IsTraditionalTutorial())
		{
			GameUtils.CompleteTraditionalTutorial();
			gameState.Concede();
			BnetBar bnetBar = BnetBar.Get();
			if (bnetBar != null)
			{
				bnetBar.HideSkipTutorialButton();
			}
			Hide();
		}
		else if (IsValidConcede(gameState, out concedeWarning))
		{
			gameState.Concede();
			Hide();
		}
		else
		{
			DialogManager.Get().ClearAllImmediatelyDontDestroy();
			ShowConfirmConcedePopup(concedeWarning);
		}
	}

	private bool IsValidConcede(GameState gameState, out CONCEDE_WARNING warning)
	{
		warning = CONCEDE_WARNING.NONE;
		if (ProgressUtils.EarlyConcedeConfirmationDisabled)
		{
			return true;
		}
		Player player = gameState.GetFriendlySidePlayer();
		int currentTurn = gameState.GetTurn();
		int minTurns = (GameMgr.Get().IsBattlegrounds() ? m_bgMinTurnsForProgressAfterConede : m_minTurnsForProgressAfterConcede);
		if (GameMgr.Get().IsRankedBattlegroundsDuoGame())
		{
			TB_BaconShopDuos baconEntity = GameState.Get()?.GetGameEntity() as TB_BaconShopDuos;
			bool teammateConceded = m_teammateConceded || player.HasTag(GAME_TAG.PLAYER_ABANDONED_BY_TEAMMATE);
			if (player.HasTag(GAME_TAG.DUOS_QUEUED_NOT_ON_TEAM) && !teammateConceded && baconEntity.HasTag(GAME_TAG.BACON_DUOS_PUNISH_LEAVERS))
			{
				warning |= CONCEDE_WARNING.LEAVE_TEAMMATE_PENALTY;
			}
		}
		if (player != null && player.IsEarlyConcedePopupAvailable() && (player.GetHero()?.GetCurrentHealth() ?? 0) > m_minHPForProgressAfterConcede && currentTurn < minTurns)
		{
			warning |= CONCEDE_WARNING.NO_QUEST_PROGRESS;
		}
		if (warning == CONCEDE_WARNING.NONE)
		{
			return true;
		}
		return false;
	}

	private void ShowConfirmConcedePopup(CONCEDE_WARNING concedeWarning)
	{
		AlertPopup.PopupInfo info = null;
		switch (concedeWarning)
		{
		case CONCEDE_WARNING.NO_QUEST_PROGRESS:
			info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_PROGRESSION_NO_QUEST_PROGRESS_FOR_CONCEDE_HEADER"),
				m_text = GameStrings.Get("GLUE_PROGRESSION_NO_QUEST_PROGRESS_FOR_CONCEDE_BODY"),
				m_confirmText = GameStrings.Get("GLUE_PROGRESSION_NO_QUEST_PROGRESS_FOR_CONCEDE_CONFIRM"),
				m_cancelText = GameStrings.Get("GLUE_PROGRESSION_NO_QUEST_PROGRESS_FOR_CONCEDE_CANCEL"),
				m_showAlertIcon = true,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_responseCallback = OnConcededWarningAlertAnswered
			};
			break;
		case CONCEDE_WARNING.LEAVE_TEAMMATE_PENALTY:
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_headerText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_TEAMMATE_PENALTY_FOR_CONCEDE_HEADER");
			popupInfo.m_text = GameStrings.Format("GLUE_PROGRESSION_LEAVE_TEAMMATE_PENALTY_FOR_CONCEDE_BODY", m_bgDuosLeaverPenalty);
			popupInfo.m_confirmText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_TEAMMATE_PENALTY_FOR_CONCEDE_CONFIRM");
			popupInfo.m_cancelText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_TEAMMATE_PENALTY_FOR_CONCEDE_CANCEL");
			popupInfo.m_showAlertIcon = true;
			popupInfo.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			popupInfo.m_responseCallback = OnConcededWarningAlertAnswered;
			info = popupInfo;
			break;
		}
		case (CONCEDE_WARNING)3:
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_headerText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_PENALTY_AND_NO_QUEST_PROGRESS_HEADER");
			popupInfo.m_text = GameStrings.Format("GLUE_PROGRESSION_LEAVE_PENALTY_AND_NO_QUEST_PROGRESS_BODY", m_bgDuosLeaverPenalty);
			popupInfo.m_confirmText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_PENALTY_AND_NO_QUEST_PROGRESS_CONFIRM");
			popupInfo.m_cancelText = GameStrings.Get("GLUE_PROGRESSION_LEAVE_PENALTY_AND_NO_QUEST_PROGRESS_CANCEL");
			popupInfo.m_showAlertIcon = true;
			popupInfo.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			popupInfo.m_responseCallback = OnConcededWarningAlertAnswered;
			info = popupInfo;
			break;
		}
		}
		DialogManager.Get().ShowPopup(info);
	}

	private void OnConcededWarningAlertAnswered(AlertPopup.Response response, object userData)
	{
		switch (response)
		{
		case AlertPopup.Response.OK:
		case AlertPopup.Response.CONFIRM:
			if (GameState.Get() != null)
			{
				GameState.Get().Concede();
			}
			break;
		case AlertPopup.Response.CANCEL:
			OnConcededWarningAlertCanceled();
			break;
		}
		Hide();
	}

	private void OnConcededWarningAlertCanceled()
	{
		GameState gameState = GameState.Get();
		if (gameState != null)
		{
			AlertPopup alertPopup = gameState.GetWaitForOpponentReconnectPopup();
			if (alertPopup != null)
			{
				alertPopup.Show();
			}
		}
	}

	private void LeaveButtonPressed(UIEvent e)
	{
		if (SpectatorManager.Get().IsInSpectatorMode())
		{
			SpectatorManager.Get().LeaveSpectatorMode();
		}
		Hide();
	}

	private void RestartButtonPressed(UIEvent e)
	{
		if (GameState.Get() != null)
		{
			GameState.Get().Restart();
		}
		Hide();
	}

	private void OptionsButtonPressed(UIEvent e)
	{
		ShowOptionsMenu();
	}

	private void AssetDownloadButtonPressed(UIEvent e)
	{
		Hide();
		DialogManager.Get().ShowAssetDownloadPopup(new AssetDownloadDialog.Info());
	}

	private void DownloadManagerPressed(UIEvent e)
	{
		Hide();
		DownloadManagerDialog.ShowMe();
	}

	private void OnSignUpPressed(UIEvent e)
	{
		Hide();
		TemporaryAccountManager.Get().ShowHealUpPage(TemporaryAccountManager.HealUpReason.GAME_MENU);
	}

	private void LoadRatings()
	{
		m_ratingsAnchor = m_menu.transform.Find(m_anchorForKoreanRatings);
		if (!m_gameMenuBase.UseKoreanRating() || !(m_ratingsAnchor != null))
		{
			return;
		}
		AssetLoader.Get().InstantiatePrefab("Korean_Ratings_OptionsScreen.prefab:aea866fab02b24ca697ede020cd85772", delegate(AssetReference name, GameObject go, object data)
		{
			if (!(go == null))
			{
				Quaternion localRotation = go.transform.localRotation;
				go.transform.parent = m_menu.transform;
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = localRotation;
				go.transform.position = m_ratingsAnchor.position;
				m_ratingsObject = go;
				LayoutMenu();
			}
		});
	}

	private bool ShouldHideRestartButton()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return false;
		}
		return gameState.GetGameEntity()?.HasTag(GAME_TAG.HIDE_RESTART_BUTTON) ?? false;
	}

	private void UpdateConcedeButtonAlternativeText()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return;
		}
		GameEntity gameEntity = gameState.GetGameEntity();
		if (gameEntity != null)
		{
			switch (gameEntity.GetTag(GAME_TAG.CONCEDE_BUTTON_ALTERNATIVE_TEXT))
			{
			case 0:
				m_concedeButton.SetText(GameStrings.Get("GLOBAL_CONCEDE"));
				break;
			case 1:
				m_concedeButton.SetText(GameStrings.Get("GLOBAL_LEAVE"));
				break;
			case 2:
				m_concedeButton.SetText(GameStrings.Get("GLOBAL_LEAVE_TUTORIAL"));
				break;
			case 3:
				m_concedeButton.SetText(GameStrings.Get("GLOBAL_SKIP_TUTORIAL"));
				break;
			default:
				m_concedeButton.SetText(GameStrings.Get("GLOBAL_CONCEDE"));
				Log.Gameplay.PrintError($"GameMenu.UpdateConcedeButtonAlternativeText() - invalid concede button alternative text");
				break;
			}
		}
	}

	private void OnSkipNprButtonReleased(UIEvent e)
	{
		Hide();
		DialogManager.Get().ShowLeaguePromoteSelfManuallyDialog(delegate
		{
			if (!Network.IsLoggedIn())
			{
				DialogManager.Get().ShowReconnectHelperDialog(RequestSkipApprentice);
			}
			else
			{
				RequestSkipApprentice();
			}
		});
	}

	private void OnSkipTutorialButtonReleased(UIEvent e)
	{
		Hide();
		BnetBar.Get().OnSkipTutorialButtonClicked(e);
	}

	private void RequestSkipApprentice()
	{
		RankMgr.Get().RequestSkipApprentice();
	}
}
