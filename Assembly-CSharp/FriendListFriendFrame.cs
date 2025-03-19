using System;
using System.Collections;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class FriendListFriendFrame : MonoBehaviour
{
	private const float REFRESH_FRIENDS_SECONDS = 30f;

	private const bool TabletShouldCloseFriendListOnCloseChatUI = false;

	private const string SHOW_LAYOUT_WITH_ICON_EVENT = "LAYOUT_WITH_ICON";

	private const string SHOW_LAYOUT_WITHOUT_ICON_EVENT = "LAYOUT_WITHOUT_ICON";

	private const string SHOW_HIGHLIGHT_EVENT = "SHOW_HIGHLIGHT";

	private const string HIDE_HIGHLIGHT_EVENT = "HIDE_HIGHLIGHT";

	private const string SHOW_HIGHLIGHT_CODE_EVENT = "SHOW_HIGHLIGHT_CODE";

	private const string HIDE_HIGHLIGHT_CODE_EVENT = "HIDE_HIGHLIGHT_CODE";

	public PlayerIcon m_playerIcon;

	public FriendListChatIcon m_chatIcon;

	public Widget m_challengeButtonWidget;

	public Widget m_selectableMedalWidget;

	[SerializeField]
	private AsyncReference m_friendFlyoutMenuReference;

	private bool m_isHighlightForced;

	private WidgetTemplate m_widget;

	private Clickable m_clickable;

	private FriendListFrame m_friendListFrame;

	private FriendListChallengeButton m_challengeButton;

	private Widget m_friendFlyoutMenuWidget;

	private Clickable m_challengeButtonClickable;

	private bool m_isRecentPlayerFrame;

	private BnetPlayer m_player;

	private MedalInfoTranslator m_rankedMedalInfo;

	private SelectableMedal m_selectableMedal;

	private FriendDataModel m_friendDataModel;

	private Coroutine m_friendUpdateCoroutine;

	private bool m_isUIShowingHighlight;

	public bool ForceHighlight
	{
		get
		{
			return m_isHighlightForced;
		}
		set
		{
			if (m_isHighlightForced != value)
			{
				bool shouldShowHighlight = ShouldShowHighlight;
				m_isHighlightForced = value;
				bool isHighlighted = ShouldShowHighlight;
				if (shouldShowHighlight != isHighlighted)
				{
					SendHighlightEvent(isHighlighted);
				}
			}
		}
	}

	private bool ShouldShowHighlight
	{
		get
		{
			if (!m_isHighlightForced)
			{
				return m_isUIShowingHighlight;
			}
			return true;
		}
	}

	public bool ShouldShowRankedMedal
	{
		get
		{
			if (m_rankedMedalInfo != null)
			{
				return m_rankedMedalInfo.IsDisplayable();
			}
			return false;
		}
	}

	public event Action<BnetPlayer> OnFriendChanged;

	private void Awake()
	{
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		BnetWhisperMgr.Get().AddWhisperListener(OnWhisper);
		ChatMgr.Get().AddPlayerChatInfoChangedListener(OnPlayerChatInfoChanged);
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		DialogManager.Get().OnDialogHidden += UpdateFriend;
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(OnFriendFrameWidgetReady);
		m_friendDataModel = new FriendDataModel();
		m_widget.BindDataModel(m_friendDataModel);
		m_challengeButtonWidget.RegisterReadyListener(OnChallengeButtonWidgetReady);
		m_selectableMedalWidget.RegisterReadyListener(OnSelectableMedalWidgetReady);
		m_friendFlyoutMenuReference.RegisterReadyListener(delegate(Widget widget)
		{
			m_friendFlyoutMenuWidget = widget;
		});
		m_friendUpdateCoroutine = StartCoroutine(RefreshFriend());
		ChatMgr.Get().OnChatLogShown += CloseChallengeMenu;
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void OnEnable()
	{
		StopCoroutine(m_friendUpdateCoroutine);
		m_friendUpdateCoroutine = StartCoroutine(RefreshFriend());
	}

	private void OnDisable()
	{
		CloseChallengeMenu();
	}

	private void OnDestroy()
	{
		if (BnetPresenceMgr.Get() != null)
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
		}
		if (BnetWhisperMgr.Get() != null)
		{
			BnetWhisperMgr.Get().RemoveWhisperListener(OnWhisper);
		}
		if (ChatMgr.Get() != null)
		{
			ChatMgr.Get().RemovePlayerChatInfoChangedListener(OnPlayerChatInfoChanged);
			ChatMgr.Get().OnChatLogShown -= CloseChallengeMenu;
		}
		if (PartyManager.Get() != null)
		{
			PartyManager.Get().RemoveChangedListener(OnPartyChanged);
		}
		if (DialogManager.Get() != null)
		{
			DialogManager.Get().OnDialogHidden -= UpdateFriend;
		}
	}

	private IEnumerator RefreshFriend()
	{
		while (true)
		{
			UpdateFriend();
			yield return new WaitForSeconds(30f);
		}
	}

	private void OnFriendFrameWidgetReady(object unused)
	{
		m_clickable = m_widget.FindWidgetComponent<Clickable>(Array.Empty<string>());
		m_clickable.AddEventListener(UIEventType.RELEASE, OnFriendFrameReleased);
		BoxCollider collider = m_clickable.GetComponent<BoxCollider>();
		if (collider != null)
		{
			collider.size = TransformUtil.ComputeSetPointBounds(base.gameObject).size;
		}
	}

	private void OnChallengeButtonWidgetReady(object unused)
	{
		m_challengeButton = m_challengeButtonWidget.FindWidgetComponent<FriendListChallengeButton>(Array.Empty<string>());
		m_challengeButton.SetPlayer(m_player);
		m_challengeButton.FriendFrame = this;
		m_challengeButtonClickable = m_challengeButtonWidget.FindWidgetComponent<Clickable>(Array.Empty<string>());
		m_challengeButtonClickable.AddEventListener(UIEventType.RELEASE, OnChallengeButtonRelease);
		UpdateFriend();
	}

	private void OnSelectableMedalWidgetReady(object unused)
	{
		m_selectableMedal = m_selectableMedalWidget.GetComponentInChildren<SelectableMedal>();
		UpdatePlayerIcon();
	}

	private void HandleEvent(string eventName)
	{
		if (!m_friendDataModel.IsFriend)
		{
			return;
		}
		if (!(eventName == "SHOW_HIGHLIGHT"))
		{
			if (eventName == "HIDE_HIGHLIGHT")
			{
				SetHighlighted(isHighlighted: false);
			}
		}
		else
		{
			SetHighlighted(isHighlighted: true);
		}
	}

	public void Initialize(BnetPlayer player, bool isRecentPlayerFrame = false)
	{
		m_player = player;
		m_playerIcon.SetPlayer(player);
		m_isRecentPlayerFrame = isRecentPlayerFrame;
		m_friendDataModel.IsFriend = BnetFriendMgr.Get().IsFriend(m_player.GetAccountId());
		UpdateFriend();
		if (m_widget.IsChangingStates)
		{
			m_widget.Hide();
			m_widget.RegisterDoneChangingStatesListener(OnWidgetDoneChangingStates);
		}
		if (this.OnFriendChanged != null)
		{
			this.OnFriendChanged(m_player);
		}
	}

	private void OnWidgetDoneChangingStates(object payload)
	{
		if (m_widget.gameObject.activeInHierarchy && m_widget.enabled && m_widget.IsDesiredHidden)
		{
			m_widget.Show();
		}
	}

	public Widget GetWidget()
	{
		return m_widget;
	}

	public BnetPlayer GetFriend()
	{
		return m_player;
	}

	public void InitializeMobileFriendListItem(FriendListFrame friendListFrame, MobileFriendListItem item)
	{
		m_friendListFrame = friendListFrame;
		item.OnScrollOutOfViewEvent += OnScrollOutOfView;
	}

	public void OpenChallengeMenu()
	{
		if (ChatMgr.Get().IsChatLogUIShowing() && PlatformSettings.IsTablet)
		{
			ChatMgr.Get().CloseChatUI(closeFriendList: false);
			ChatMgr.Get().UpdateLayout();
			return;
		}
		m_widget.TriggerEvent("OPEN_CHALLENGE_MENU");
		if (m_friendFlyoutMenuWidget != null)
		{
			m_friendFlyoutMenuWidget.gameObject.SetActive(value: true);
		}
		if (m_friendListFrame != null)
		{
			m_friendListFrame.CloseFlyoutMenu();
			m_friendListFrame.FlyoutOpened?.Invoke();
		}
	}

	public void CloseChallengeMenu()
	{
		if (m_challengeButtonWidget != null)
		{
			m_challengeButtonWidget.TriggerEvent("CLOSE_CHALLENGE_MENU");
		}
		if (m_friendFlyoutMenuWidget != null)
		{
			m_friendFlyoutMenuWidget.gameObject.SetActive(value: false);
		}
		if (m_friendListFrame != null)
		{
			m_friendListFrame.FlyoutClosed?.Invoke();
		}
	}

	public void DismissFlyoutAndPopups(bool showAlert)
	{
		CloseChallengeMenu();
		if (m_challengeButton != null)
		{
			m_challengeButton.DismissPopups(showAlert);
		}
	}

	public void CloseFriendsListMenu()
	{
		ChatMgr.Get().CloseFriendsList();
	}

	private void OnChallengeButtonRelease(UIEvent e)
	{
		if (m_friendDataModel.IsInEditMode)
		{
			OnDeleteFriendButtonPressed();
		}
		else
		{
			OnAvailableButtonPressed();
		}
	}

	private void OnFriendFrameReleased(UIEvent e)
	{
		if (!(m_friendListFrame != null) || !m_friendListFrame.IsInEditMode)
		{
			FriendMgr.Get().SetSelectedFriend(m_player);
			if (BnetFriendMgr.Get().IsFriend(m_player.GetAccountId()))
			{
				SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
				ChatMgr.Get().OnFriendListFriendSelected(m_player);
			}
		}
	}

	private void OnAvailableButtonPressed()
	{
		if (m_challengeButton.IsChallengeMenuOpen)
		{
			CloseChallengeMenu();
		}
		else
		{
			OpenChallengeMenu();
		}
	}

	private void OnDeleteFriendButtonPressed()
	{
		if (m_friendListFrame != null)
		{
			m_friendListFrame.ShowRemoveFriendPopup(m_player);
		}
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (changelist.HasChange(m_player))
		{
			UpdateFriend();
		}
	}

	private void OnWhisper(BnetWhisper whisper, object userData)
	{
		if (m_player != null && WhisperUtil.IsSpeakerOrReceiver(m_player, whisper))
		{
			UpdateFriend();
		}
	}

	private void OnPlayerChatInfoChanged(PlayerChatInfo chatInfo, object userData)
	{
		if (m_player == chatInfo.GetPlayer())
		{
			UpdateFriend();
		}
	}

	private void OnScrollOutOfView()
	{
		if (m_challengeButton != null)
		{
			CloseChallengeMenu();
		}
	}

	public void UpdateFriend()
	{
		if (m_player == null)
		{
			return;
		}
		BnetPlayer existingFriend = BnetFriendMgr.Get().FindFriend(m_player.GetAccountId());
		if (existingFriend != null)
		{
			m_friendDataModel.PlayerName = FriendUtils.GetFriendListName(existingFriend, addColorTags: true);
		}
		else
		{
			m_friendDataModel.PlayerName = FriendUtils.GetFriendListName(m_player, addColorTags: false);
		}
		BnetGameAccount gameAccount = m_player.GetBestGameAccount();
		m_rankedMedalInfo = ((gameAccount == null) ? null : RankMgr.Get().GetRankedMedalFromRankPresenceField(gameAccount));
		m_chatIcon.UpdateIcon();
		UpdatePresence();
		if (m_isRecentPlayerFrame)
		{
			m_friendDataModel.PlayerStatus = BnetRecentPlayerMgr.Get().GetRecentReason(m_player);
			if (BnetRecentPlayerMgr.Get().IsCurrentOpponent(m_player))
			{
				if (Options.Get().GetBool(Option.STREAMER_MODE))
				{
					m_friendDataModel.PlayerName = GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME");
				}
				else if (BnetRecentPlayerMgr.Get().IsRecentStranger(m_player) && !BnetNearbyPlayerMgr.Get().IsNearbyPlayer(m_player))
				{
					m_friendDataModel.PlayerName = m_player.GetBestName();
				}
			}
		}
		UpdatePlayerIcon();
		UpdateInteractionState();
		m_friendDataModel.CanBeSpectated = SpectatorManager.Get().CanSpectate(m_player);
	}

	private void UpdateInteractionState()
	{
		if (m_challengeButtonWidget == null)
		{
			return;
		}
		if (m_challengeButton != null)
		{
			m_challengeButton.SetPlayer(m_player);
		}
		if (m_friendListFrame != null && m_friendListFrame.IsInEditMode && m_friendListFrame.EditMode == FriendListFrame.FriendListEditMode.REMOVE_FRIENDS)
		{
			m_friendDataModel.IsInEditMode = true;
			m_challengeButtonWidget.TriggerEvent("DELETE");
			return;
		}
		m_challengeButtonWidget.TriggerEvent("AVAILABLE");
		m_friendDataModel.IsInEditMode = false;
		if (m_challengeButton != null)
		{
			m_challengeButton.UpdateFlyoutMenu();
		}
	}

	private void UpdatePlayerIcon()
	{
		if (!BnetFriendMgr.Get().IsFriend(m_player))
		{
			m_widget.TriggerEvent("LAYOUT_WITHOUT_ICON");
			return;
		}
		if (!m_player.IsOnline())
		{
			m_widget.TriggerEvent("LAYOUT_WITHOUT_ICON");
			return;
		}
		BnetProgramId programId = m_player.GetBestProgramId();
		if (programId == null || programId.IsPhoenix())
		{
			m_widget.TriggerEvent("LAYOUT_WITHOUT_ICON");
			return;
		}
		m_widget.TriggerEvent("LAYOUT_WITH_ICON");
		Action displayNoMedal = delegate
		{
			m_playerIcon.Show();
			m_playerIcon.UpdateIcon();
			m_selectableMedalWidget.gameObject.SetActive(value: false);
		};
		if (programId == BnetProgramId.HEARTHSTONE)
		{
			m_playerIcon.Hide();
			m_selectableMedalWidget.gameObject.SetActive(value: true);
			m_selectableMedal?.UpdateWidget(m_player, null, null, displayNoMedal);
		}
		else
		{
			displayNoMedal();
		}
	}

	protected void UpdatePresence()
	{
		if (m_isRecentPlayerFrame && BnetRecentPlayerMgr.Get().IsRecentStranger(m_player))
		{
			m_friendDataModel.IsOnline = true;
			m_friendDataModel.IsInHS = true;
			return;
		}
		if (!m_player.IsOnline())
		{
			if (m_friendDataModel.IsOnline)
			{
				DismissFlyoutAndPopups(showAlert: true);
			}
			m_friendDataModel.IsOnline = false;
			long lastOnlineMicrosec = m_player.GetBestLastOnlineMicrosec();
			m_friendDataModel.PlayerStatus = FriendUtils.GetLastOnlineElapsedTimeString(lastOnlineMicrosec);
			return;
		}
		m_friendDataModel.IsOnline = true;
		BnetGameAccount hsAccount = m_player.GetHearthstoneGameAccount();
		if (hsAccount == null || !hsAccount.IsOnline())
		{
			BnetProgramId programId = m_player.GetBestProgramId();
			if (programId != null)
			{
				m_friendDataModel.PlayerStatus = BnetUtils.GetNameForProgramId(programId);
			}
			else
			{
				m_friendDataModel.PlayerStatus = GameStrings.Get("GLOBAL_PROGRAMNAME_PHOENIX");
			}
			m_friendDataModel.IsInHS = false;
			return;
		}
		m_friendDataModel.IsInHS = true;
		if (m_player.IsAway())
		{
			m_friendDataModel.PlayerStatus = FriendUtils.GetAwayTimeString(m_player.GetBestAwayTimeMicrosec());
			m_friendDataModel.IsAway = true;
			return;
		}
		m_friendDataModel.IsAway = false;
		if (m_player.IsBusy())
		{
			m_friendDataModel.PlayerStatus = GameStrings.Get("GLOBAL_FRIENDLIST_BUSYSTATUS");
			m_friendDataModel.IsBusy = true;
		}
		else
		{
			m_friendDataModel.IsBusy = false;
			m_friendDataModel.PlayerStatus = PresenceMgr.Get().GetStatusText(m_player);
		}
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData data, object userData)
	{
		UpdateInteractionState();
	}

	private void SetHighlighted(bool isHighlighted)
	{
		bool wasHighlighted = ShouldShowHighlight;
		m_isUIShowingHighlight = isHighlighted;
		bool nowHighlighted = ShouldShowHighlight;
		if (nowHighlighted != wasHighlighted)
		{
			SendHighlightEvent(nowHighlighted);
		}
	}

	private void SendHighlightEvent(bool shouldHighlight)
	{
		m_widget.TriggerEvent(shouldHighlight ? "SHOW_HIGHLIGHT_CODE" : "HIDE_HIGHLIGHT_CODE");
	}
}
