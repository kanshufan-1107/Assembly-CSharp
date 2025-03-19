using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class ChatMgr : MonoBehaviour
{
	public delegate void PlayerChatInfoChangedCallback(PlayerChatInfo chatInfo, object userData);

	public delegate void FriendListToggled(bool open);

	private class PlayerChatInfoChangedListener : EventListener<PlayerChatInfoChangedCallback>
	{
		public void Fire(PlayerChatInfo chatInfo)
		{
			m_callback(chatInfo, m_userData);
		}
	}

	private enum KeyboardState
	{
		None,
		Below,
		Above
	}

	public ChatMgrPrefabs m_Prefabs;

	public ChatMgrBubbleInfo m_ChatBubbleInfo;

	public Float_MobileOverride m_friendsListXOffset;

	public Float_MobileOverride m_friendsListYOffset;

	public Float_MobileOverride m_friendsListWidthPadding;

	public Float_MobileOverride m_friendsListHeightPadding;

	public float m_chatLogXOffset;

	public Float_MobileOverride m_friendsListWidth;

	private static ChatMgr s_instance;

	private List<ChatBubbleFrame> m_chatBubbleFrames = new List<ChatBubbleFrame>();

	private IChatLogUI m_chatLogUI;

	private FriendListFrame m_friendListFrame;

	private PegUIElement m_closeCatcher;

	private List<BnetPlayer> m_recentWhisperPlayers = new List<BnetPlayer>();

	private Map<BnetAccountId, string> m_pendingChatMessages = new Map<BnetAccountId, string>();

	private bool m_chatLogFrameShown;

	private bool m_isChatFeatureEnabled;

	private PrivacyFeaturesPopup m_chatPrivacyPopup;

	private Map<BnetPlayer, PlayerChatInfo> m_playerChatInfos = new Map<BnetPlayer, PlayerChatInfo>();

	private List<PlayerChatInfoChangedListener> m_playerChatInfoChangedListeners = new List<PlayerChatInfoChangedListener>();

	private KeyboardState keyboardState;

	private Rect keyboardArea = new Rect(0f, 0f, 0f, 0f);

	private FatalErrorMgr m_fatalErrorMgr;

	private Map<Renderer, int> m_friendListOriginalLayers = new Map<Renderer, int>();

	public FriendListFrame FriendListFrame => m_friendListFrame;

	public Rect KeyboardRect => keyboardArea;

	public event FriendListToggled OnFriendListToggled;

	public event Action OnChatLogShown;

	public static event Action OnStarted;

	private void Awake()
	{
		s_instance = this;
		m_fatalErrorMgr = FatalErrorMgr.Get();
		BnetWhisperMgr.Get().AddWhisperListener(OnWhisper);
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		m_fatalErrorMgr.AddErrorListener(OnFatalError);
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		touchScreenService.AddOnVirtualKeyboardShowListener(OnKeyboardShow);
		touchScreenService.AddOnVirtualKeyboardHideListener(OnKeyboardHide);
		HearthstoneApplication.Get().WillReset += WillReset;
		InitCloseCatcher();
		InitChatLogUI();
	}

	private void OnDestroy()
	{
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset -= WillReset;
		}
		if (ServiceManager.TryGet<ITouchScreenService>(out var touchScreenService))
		{
			touchScreenService.RemoveOnVirtualKeyboardShowListener(OnKeyboardShow);
			touchScreenService.RemoveOnVirtualKeyboardHideListener(OnKeyboardHide);
		}
		this.OnChatLogShown = null;
		s_instance = null;
		m_fatalErrorMgr.RemoveErrorListener(OnFatalError);
	}

	private void Start()
	{
		SoundManager.Get().Load("receive_message.prefab:8e90a827cd4a0e849953158396cd1ee1");
		UpdateLayout();
		if (ServiceManager.Get<ITouchScreenService>().IsVirtualKeyboardVisible())
		{
			OnKeyboardShow();
		}
		if (ChatMgr.OnStarted != null)
		{
			ChatMgr.OnStarted();
		}
	}

	private void Update()
	{
		Rect lastKeyboardArea = keyboardArea;
		keyboardArea = TextField.KeyboardArea;
		if (keyboardArea != lastKeyboardArea)
		{
			UpdateLayout();
		}
	}

	public static ChatMgr Get()
	{
		return s_instance;
	}

	private void WillReset()
	{
		CleanUp();
	}

	private void DisplayAlertPopup(string message)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
		info.m_text = message;
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
		Log.Privacy.PrintWarning("Chat is disabled. Will not show Friends List");
	}

	private KeyboardState ComputeKeyboardState()
	{
		if (keyboardArea.height > 0f)
		{
			float y = keyboardArea.y;
			float pixelsBelowKeyboard = (float)Screen.height - keyboardArea.yMax;
			if (!(y > pixelsBelowKeyboard))
			{
				return KeyboardState.Above;
			}
			return KeyboardState.Below;
		}
		return KeyboardState.None;
	}

	private void InitCloseCatcher()
	{
		GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(BaseUI.Get().GetBnetCamera(), "CloseCatcher", this);
		m_closeCatcher = inputBlockerObject.AddComponent<PegUIElement>();
		m_closeCatcher.AddEventListener(UIEventType.RELEASE, OnCloseCatcherRelease);
		m_closeCatcher.gameObject.SetActive(value: false);
	}

	private void InitChatLogUI()
	{
		if (IsMobilePlatform())
		{
			m_chatLogUI = new MobileChatLogUI();
		}
		else
		{
			m_chatLogUI = new DesktopChatLogUI();
		}
	}

	private FriendListFrame CreateFriendsListUI()
	{
		string prefabName = (UniversalInputManager.UsePhoneUI ? "FriendListFrame_phone.prefab:91e737585d7bfd2449b46fbecb87ded7" : "FriendListFrame.prefab:cdf3b7f04b5ed45cb8ba0160d43a5bf6");
		GameObject go = AssetLoader.Get().InstantiatePrefab(prefabName);
		if (go == null)
		{
			return null;
		}
		go.transform.parent = base.transform;
		return go.GetComponent<FriendListFrame>();
	}

	public void SetChatFeatureStatus(bool isEnabled)
	{
		m_isChatFeatureEnabled = isEnabled;
	}

	public void UpdateLayout()
	{
		if (m_friendListFrame != null || m_chatLogUI.IsShowing)
		{
			UpdateLayoutForOnScreenKeyboard();
		}
		UpdateChatBubbleParentLayout();
	}

	private void UpdateLayoutForOnScreenKeyboard()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			UpdateLayoutForOnScreenKeyboardOnPhone();
			return;
		}
		keyboardState = ComputeKeyboardState();
		bool isTabletMode = IsMobilePlatform();
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			isTabletMode = false;
		}
		Camera camera = BaseUI.Get().GetBnetCamera();
		float layoutWorldHeight = camera.orthographicSize * 2f;
		float layoutWorldWidth = layoutWorldHeight * camera.aspect;
		float layoutWorldTop = camera.transform.position.z + layoutWorldHeight / 2f;
		float layoutWorldLeft = camera.transform.position.x - layoutWorldWidth / 2f;
		float keyboardWorldHeight = 0f;
		if (keyboardState != KeyboardState.None && isTabletMode)
		{
			keyboardWorldHeight = layoutWorldHeight * keyboardArea.height / (float)Screen.height;
		}
		float friendsListWidth = 0f;
		if (m_friendListFrame != null)
		{
			OrientedBounds friendButtonBounds = TransformUtil.ComputeOrientedWorldBounds(BaseUI.Get().m_BnetBar.m_friendButton.gameObject, default(Vector3), default(Vector3), null, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: true);
			if (isTabletMode)
			{
				float h = ((keyboardState == KeyboardState.Below) ? keyboardWorldHeight : (friendButtonBounds.Extents[1].z * 2f));
				m_friendListFrame.SetWorldHeight(layoutWorldHeight - h);
			}
			OrientedBounds friendListBounds = m_friendListFrame.ComputeFrameWorldBounds();
			if (friendListBounds != null)
			{
				if (!isTabletMode || keyboardState != KeyboardState.Below)
				{
					float x = layoutWorldLeft + friendListBounds.Extents[0].x + friendListBounds.CenterOffset.x + (float)m_friendsListXOffset;
					float z = friendButtonBounds.GetTrueCenterPosition().z + friendButtonBounds.Extents[1].z + friendListBounds.Extents[1].z + friendListBounds.CenterOffset.z;
					m_friendListFrame.SetWorldPosition(x, z);
				}
				else if (isTabletMode && keyboardState == KeyboardState.Below)
				{
					float x2 = layoutWorldLeft + friendListBounds.Extents[0].x + friendListBounds.CenterOffset.x + (float)m_friendsListXOffset;
					float z2 = camera.transform.position.z - layoutWorldHeight / 2f + keyboardWorldHeight + friendListBounds.Extents[1].z + friendListBounds.CenterOffset.z;
					m_friendListFrame.SetWorldPosition(x2, z2);
				}
				friendsListWidth = friendListBounds.Extents[0].magnitude * 2f;
			}
		}
		if (m_chatLogUI.IsShowing)
		{
			ChatFrames frames = m_chatLogUI.GameObject.GetComponent<ChatFrames>();
			if (frames != null)
			{
				float chatLogTop = layoutWorldTop;
				if (keyboardState == KeyboardState.Above)
				{
					chatLogTop -= keyboardWorldHeight;
				}
				float chatLogHeight = layoutWorldHeight - keyboardWorldHeight;
				if (keyboardState == KeyboardState.None && isTabletMode)
				{
					OrientedBounds friendButtonBounds2 = TransformUtil.ComputeOrientedWorldBounds(BaseUI.Get().m_BnetBar.m_friendButton.gameObject, default(Vector3), default(Vector3), null, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: true);
					chatLogHeight -= friendButtonBounds2.Extents[1].z * 2f;
				}
				float chatLogLeft = layoutWorldLeft;
				if (!UniversalInputManager.UsePhoneUI)
				{
					chatLogLeft += friendsListWidth + (float)m_friendsListXOffset + m_chatLogXOffset;
				}
				float chatLogWidth = layoutWorldWidth;
				if (!UniversalInputManager.UsePhoneUI)
				{
					chatLogWidth -= friendsListWidth + (float)m_friendsListXOffset + m_chatLogXOffset;
				}
				frames.chatLogFrame.SetWorldRect(chatLogLeft, chatLogTop, chatLogWidth, chatLogHeight);
			}
		}
		OnChatFramesMoved();
	}

	private void UpdateLayoutForOnScreenKeyboardOnPhone()
	{
		keyboardState = ComputeKeyboardState();
		bool isTouchMode = UniversalInputManager.Get().IsTouchMode();
		float margin = BnetBar.Get().HorizontalMargin;
		Camera camera = BaseUI.Get().GetBnetCamera();
		float layoutWorldHeight = camera.orthographicSize * 2f;
		float layoutWorldWidth = layoutWorldHeight * camera.aspect - margin / 2f;
		float layoutWorldTop = camera.transform.position.z + layoutWorldHeight / 2f;
		float layoutWorldLeft = camera.transform.position.x - layoutWorldWidth / 2f;
		float keyboardWorldHeight = 0f;
		float keyboardWorldWidth = 0f;
		float keyboardWorldLeftOffset = 0f;
		if (keyboardState != KeyboardState.None && isTouchMode)
		{
			keyboardWorldHeight = layoutWorldHeight * keyboardArea.height / (float)Screen.height;
			keyboardWorldWidth = layoutWorldWidth * keyboardArea.width / (float)Screen.width;
			keyboardWorldLeftOffset = layoutWorldWidth * keyboardArea.xMin / (float)Screen.width;
		}
		if (m_friendListFrame != null)
		{
			float friendsListLeft = layoutWorldLeft + (float)m_friendsListXOffset;
			float friendsListTop = layoutWorldTop + (float)m_friendsListYOffset;
			float friendsListWidth = (float)m_friendsListWidth + (float)m_friendsListWidthPadding;
			float friendsListHeight = layoutWorldHeight + (float)m_friendsListHeightPadding;
			m_friendListFrame.SetWorldRect(friendsListLeft, friendsListTop, friendsListWidth, friendsListHeight);
		}
		if (m_chatLogUI.IsShowing)
		{
			ChatFrames frames = m_chatLogUI.GameObject.GetComponent<ChatFrames>();
			if (frames != null)
			{
				float chatLogTop = layoutWorldTop;
				if (keyboardState == KeyboardState.Above)
				{
					chatLogTop -= keyboardWorldHeight;
				}
				float chatLogHeight = layoutWorldHeight - keyboardWorldHeight;
				float chatLogLeft = layoutWorldLeft + keyboardWorldLeftOffset;
				if (!UniversalInputManager.UsePhoneUI)
				{
					chatLogLeft += (float)m_friendsListWidth;
				}
				float chatLogWidth = ((keyboardWorldWidth == 0f) ? layoutWorldWidth : keyboardWorldWidth);
				if (!UniversalInputManager.UsePhoneUI)
				{
					chatLogWidth -= (float)m_friendsListWidth;
				}
				frames.chatLogFrame.SetWorldRect(chatLogLeft, chatLogTop, chatLogWidth, chatLogHeight);
			}
		}
		OnChatFramesMoved();
	}

	public bool IsChatLogFrameShown()
	{
		if (IsMobilePlatform())
		{
			return IsChatLogUIShowing();
		}
		return m_chatLogFrameShown;
	}

	public bool IsChatLogUIShowing()
	{
		return m_chatLogUI.IsShowing;
	}

	private void OnCloseCatcherRelease(UIEvent e)
	{
		if (m_chatLogUI != null && m_chatLogUI.IsShowing)
		{
			m_chatLogUI.Hide();
		}
		if (FriendListFrame != null && FriendListFrame.IsInEditMode)
		{
			FriendListFrame.ExitRemoveFriendsMode();
		}
		else if (FriendListFrame != null && FriendListFrame.IsFlyoutOpen)
		{
			FriendListFrame.CloseFlyoutMenu();
		}
		else
		{
			CloseFriendsList();
		}
	}

	public bool IsFriendListShowing()
	{
		if (!(m_friendListFrame == null))
		{
			return m_friendListFrame.gameObject.activeSelf;
		}
		return false;
	}

	public void ShowFriendsList()
	{
		if ((SetRotationManager.Get() == null || !SetRotationManager.Get().CheckForSetRotationRollover()) && (PlayerMigrationManager.Get() == null || !PlayerMigrationManager.Get().CheckForPlayerMigrationRequired()))
		{
			if (m_friendListFrame == null)
			{
				m_friendListFrame = CreateFriendsListUI();
			}
			m_friendListFrame.gameObject.SetActive(value: true);
			m_closeCatcher.gameObject.SetActive(value: true);
			UpdateLayout();
			TransformUtil.SetPosY(m_closeCatcher, m_friendListFrame.transform.position.y - 100f);
			m_friendListFrame.UpdateFriendItems();
			Get().FriendListFrame.items.RecalculateItemSizeAndOffsets(ignoreCurrentPosition: true);
			if (this.OnFriendListToggled != null)
			{
				this.OnFriendListToggled(open: true);
			}
		}
	}

	private void HideFriendsList()
	{
		if (IsFriendListShowing())
		{
			m_friendListFrame.gameObject.SetActive(value: false);
		}
		if (m_closeCatcher != null)
		{
			m_closeCatcher.gameObject.SetActive(value: false);
		}
		if (this.OnFriendListToggled != null)
		{
			this.OnFriendListToggled(open: false);
		}
	}

	public void CloseFriendsList()
	{
		DestroyFriendListFrame();
	}

	public void GoBack()
	{
		if (IsFriendListShowing())
		{
			CloseChatUI();
		}
		else if (m_chatLogUI.IsShowing)
		{
			m_chatLogUI.Hide();
			ShowFriendsList();
		}
	}

	public void CloseChatUI(bool closeFriendList = true)
	{
		if (m_chatLogUI.IsShowing)
		{
			m_chatLogUI.Hide();
		}
		if (closeFriendList)
		{
			CloseFriendsList();
		}
	}

	public void CleanUp()
	{
		DestroyFriendListFrame();
	}

	private void DestroyFriendListFrame()
	{
		HideFriendsList();
		if (!(m_friendListFrame == null))
		{
			UnityEngine.Object.Destroy(m_friendListFrame.gameObject);
			m_friendListFrame = null;
		}
	}

	public void SetPendingMessage(BnetAccountId playerID, string message)
	{
		m_pendingChatMessages[playerID] = message;
	}

	public string GetPendingMessage(BnetAccountId playerID)
	{
		string message = "";
		m_pendingChatMessages.TryGetValue(playerID, out message);
		return message;
	}

	public List<BnetPlayer> GetRecentWhisperPlayers()
	{
		return m_recentWhisperPlayers;
	}

	public void AddRecentWhisperPlayerToTop(BnetPlayer player)
	{
		int existingIndex = m_recentWhisperPlayers.FindIndex((BnetPlayer currPlayer) => currPlayer == player);
		if (existingIndex < 0)
		{
			if (m_recentWhisperPlayers.Count == 10)
			{
				m_recentWhisperPlayers.RemoveAt(m_recentWhisperPlayers.Count - 1);
			}
		}
		else
		{
			m_recentWhisperPlayers.RemoveAt(existingIndex);
		}
		m_recentWhisperPlayers.Insert(0, player);
	}

	public void AddRecentWhisperPlayerToBottom(BnetPlayer player)
	{
		if (!m_recentWhisperPlayers.Contains(player))
		{
			if (m_recentWhisperPlayers.Count == 10)
			{
				m_recentWhisperPlayers.RemoveAt(m_recentWhisperPlayers.Count - 1);
			}
			m_recentWhisperPlayers.Add(player);
		}
	}

	public void AddPlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback)
	{
		AddPlayerChatInfoChangedListener(callback, null);
	}

	public void AddPlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback, object userData)
	{
		PlayerChatInfoChangedListener listener = new PlayerChatInfoChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_playerChatInfoChangedListeners.Contains(listener))
		{
			m_playerChatInfoChangedListeners.Add(listener);
		}
	}

	public bool RemovePlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback)
	{
		return RemovePlayerChatInfoChangedListener(callback, null);
	}

	public bool RemovePlayerChatInfoChangedListener(PlayerChatInfoChangedCallback callback, object userData)
	{
		PlayerChatInfoChangedListener listener = new PlayerChatInfoChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_playerChatInfoChangedListeners.Remove(listener);
	}

	public PlayerChatInfo GetPlayerChatInfo(BnetPlayer player)
	{
		PlayerChatInfo chatInfo = null;
		m_playerChatInfos.TryGetValue(player, out chatInfo);
		return chatInfo;
	}

	public PlayerChatInfo RegisterPlayerChatInfo(BnetPlayer player)
	{
		if (!m_playerChatInfos.TryGetValue(player, out var chatInfo))
		{
			chatInfo = new PlayerChatInfo();
			chatInfo.SetPlayer(player);
			m_playerChatInfos.Add(player, chatInfo);
		}
		return chatInfo;
	}

	public void UpdateFriendItemsWhenAvailable()
	{
		if (m_friendListFrame != null)
		{
			m_friendListFrame.UpdateFriendItemsWhenAvailable();
		}
	}

	public void OnFriendListOpened()
	{
		if (ServiceManager.Get<ITouchScreenService>().IsVirtualKeyboardVisible())
		{
			OnKeyboardShow();
		}
		else
		{
			UpdateChatBubbleParentLayout();
		}
	}

	public void OnFriendListClosed()
	{
		if (ServiceManager.Get<ITouchScreenService>().IsVirtualKeyboardVisible())
		{
			OnKeyboardShow();
		}
		else
		{
			UpdateChatBubbleParentLayout();
		}
	}

	public void OnFriendListFriendSelected(BnetPlayer friend)
	{
		ShowChatForPlayer(friend);
		if (m_friendListFrame != null)
		{
			m_friendListFrame.SelectFriend(friend);
		}
	}

	public void OnChatLogFrameShown()
	{
		m_chatLogFrameShown = true;
	}

	public void OnChatLogFrameHidden()
	{
		m_chatLogFrameShown = false;
	}

	public void OnChatReceiverChanged(BnetPlayer player)
	{
		UpdatePlayerFocusTime(player);
	}

	public void OnChatFramesMoved()
	{
		UpdateChatBubbleParentLayout();
	}

	public void OnQuickChatFrameClosed()
	{
		if (m_friendListFrame != null)
		{
			m_friendListFrame.ClearHighlights();
		}
	}

	public bool HandleKeyboardInput()
	{
		if (m_fatalErrorMgr.HasError())
		{
			return false;
		}
		if (InputCollection.GetKeyUp(KeyCode.Escape) && m_chatLogUI.IsShowing)
		{
			m_chatLogUI.Hide();
			return true;
		}
		if (IsMobilePlatform() && m_chatLogUI.IsShowing && InputCollection.GetKeyUp(KeyCode.Escape))
		{
			m_chatLogUI.GoBack();
			return true;
		}
		return false;
	}

	public void HandleGUIInput()
	{
		if (!m_fatalErrorMgr.HasError() && !IsMobilePlatform())
		{
			HandleGUIInputForQuickChat();
		}
	}

	private void OnWhisper(BnetWhisper whisper, object userData)
	{
		if (!m_isChatFeatureEnabled)
		{
			Log.Privacy.PrintDebug("Receiving chat messages is not enabled by Privacy settings");
			return;
		}
		BnetPlayer theirPlayer = WhisperUtil.GetTheirPlayer(whisper);
		AddRecentWhisperPlayerToTop(theirPlayer);
		BnetRecentPlayerMgr.Get().AddRecentPlayer(theirPlayer, BnetRecentPlayerMgr.RecentReason.RECENT_CHATTED);
		PlayerChatInfo chatInfo = RegisterPlayerChatInfo(WhisperUtil.GetTheirPlayer(whisper));
		try
		{
			if (m_chatLogUI.IsShowing && WhisperUtil.IsSpeakerOrReceiver(m_chatLogUI.Receiver, whisper) && IsMobilePlatform())
			{
				chatInfo.SetLastSeenWhisper(whisper);
			}
			else
			{
				PopupNewChatBubble(whisper);
			}
		}
		finally
		{
			FireChatInfoChangedEvent(chatInfo);
		}
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
		if (removedFriends == null)
		{
			return;
		}
		foreach (BnetPlayer friend in removedFriends)
		{
			int index = m_recentWhisperPlayers.FindIndex((BnetPlayer player) => friend == player);
			if (index >= 0)
			{
				m_recentWhisperPlayers.RemoveAt(index);
			}
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		CleanUp();
	}

	private void HandleGUIInputForQuickChat()
	{
		if (m_chatLogUI == null)
		{
			return;
		}
		if (!m_chatLogUI.IsShowing)
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				ShowChatForPlayer(GetMostRecentWhisperedPlayer());
			}
		}
		else if (Input.GetKeyUp(KeyCode.Escape))
		{
			m_chatLogUI.Hide();
		}
	}

	public bool IsMobilePlatform()
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			return PlatformSettings.OS != OSCategory.PC;
		}
		return false;
	}

	private void ShowChatForPlayer(BnetPlayer player)
	{
		if (!m_isChatFeatureEnabled)
		{
			if (m_chatPrivacyPopup == null)
			{
				GameObject popupGO = AssetLoader.Get().InstantiatePrefab("PrivacyPopups.prefab:99a8f571a8a35a54e90790c904bc94f8");
				m_chatPrivacyPopup = popupGO.GetComponent<PrivacyFeaturesPopup>();
				m_chatPrivacyPopup.Set(PrivacyFeatures.CHAT, m_isChatFeatureEnabled, delegate
				{
					PrivacyGate.Get().SetFeature(PrivacyFeatures.CHAT, isEnabled: true);
				}, delegate
				{
					OnChatPopupSuccess(player, m_chatPrivacyPopup);
				}, delegate
				{
					OnChatPopupCanceled(m_chatPrivacyPopup);
				});
				CloseChatUI();
				m_chatPrivacyPopup.Show();
			}
		}
		else
		{
			OnShowChatForPlayerAllowed(player);
		}
	}

	private void OnChatPopupSuccess(BnetPlayer player, PrivacyFeaturesPopup privacyPopup)
	{
		privacyPopup.Hide();
		ShowFriendsList();
		OnShowChatForPlayerAllowed(player);
		m_chatPrivacyPopup = null;
		UnityEngine.Object.Destroy(privacyPopup.gameObject, 1f);
	}

	private void OnChatPopupCanceled(PrivacyFeaturesPopup privacyPopup)
	{
		privacyPopup.Hide();
		ShowFriendsList();
		m_chatPrivacyPopup = null;
		UnityEngine.Object.Destroy(privacyPopup.gameObject, 1f);
	}

	private void OnShowChatForPlayerAllowed(BnetPlayer player)
	{
		if (player != null)
		{
			AddRecentWhisperPlayerToTop(player);
			PlayerChatInfo chatInfo = RegisterPlayerChatInfo(player);
			List<BnetWhisper> whispers = BnetWhisperMgr.Get().GetWhispersWithPlayer(player);
			if (whispers != null)
			{
				chatInfo.SetLastSeenWhisper(whispers.LastOrDefault((BnetWhisper whisper) => WhisperUtil.IsSpeaker(player, whisper)));
				FireChatInfoChangedEvent(chatInfo);
			}
		}
		if (m_chatLogUI.IsShowing)
		{
			m_chatLogUI.Hide();
		}
		if (FriendListFrame != null && FriendListFrame.IsFlyoutOpen)
		{
			FriendListFrame.CloseFlyoutMenu();
		}
		if (!m_chatLogUI.IsShowing)
		{
			if (OptionsMenu.Get() != null && OptionsMenu.Get().IsShown())
			{
				OptionsMenu.Get().Hide();
			}
			if (MiscellaneousMenu.Get() != null && MiscellaneousMenu.Get().IsShown())
			{
				MiscellaneousMenu.Get().Hide();
			}
			if (BnetBar.Get() != null)
			{
				BnetBar.Get().HideGameMenu();
			}
			m_chatLogUI.ShowForPlayer(GetMostRecentWhisperedPlayer());
			UpdateLayout();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				CloseFriendsList();
			}
			if (this.OnChatLogShown != null)
			{
				this.OnChatLogShown();
			}
		}
	}

	private BnetPlayer GetMostRecentWhisperedPlayer()
	{
		if (m_recentWhisperPlayers.Count <= 0)
		{
			return null;
		}
		return m_recentWhisperPlayers[0];
	}

	private void UpdatePlayerFocusTime(BnetPlayer player)
	{
		PlayerChatInfo chatInfo = RegisterPlayerChatInfo(player);
		chatInfo.SetLastFocusTime(Time.realtimeSinceStartup);
		FireChatInfoChangedEvent(chatInfo);
	}

	private void FireChatInfoChangedEvent(PlayerChatInfo chatInfo)
	{
		PlayerChatInfoChangedListener[] listeners = m_playerChatInfoChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(chatInfo);
		}
	}

	private void UpdateChatBubbleParentLayout()
	{
		if (BaseUI.Get().GetChatBubbleBone() != null)
		{
			m_ChatBubbleInfo.m_Parent.transform.position = BaseUI.Get().GetChatBubbleBone().transform.position;
		}
	}

	private void UpdateChatBubbleLayout()
	{
		int count = m_chatBubbleFrames.Count;
		if (count != 0)
		{
			Component relativeObject = m_ChatBubbleInfo.m_Parent;
			for (int i = count - 1; i >= 0; i--)
			{
				ChatBubbleFrame chatBubbleFrame = m_chatBubbleFrames[i];
				Anchor relativeAnchor = (UniversalInputManager.UsePhoneUI ? Anchor.BOTTOM_LEFT_XZ : Anchor.TOP_LEFT_XZ);
				Anchor bubbleAnchor = (UniversalInputManager.UsePhoneUI ? Anchor.TOP_LEFT_XZ : Anchor.BOTTOM_LEFT_XZ);
				TransformUtil.SetPoint(chatBubbleFrame, bubbleAnchor, relativeObject, relativeAnchor, Vector3.zero);
				relativeObject = chatBubbleFrame;
			}
		}
	}

	private void PopupNewChatBubble(BnetWhisper whisper)
	{
		ChatBubbleFrame bubbleFrame = CreateChatBubble(whisper);
		m_chatBubbleFrames.Add(bubbleFrame);
		UpdateChatBubbleParentLayout();
		bubbleFrame.transform.parent = m_ChatBubbleInfo.m_Parent.transform;
		bubbleFrame.transform.localScale = bubbleFrame.m_ScaleOverride;
		SoundManager.Get().LoadAndPlay("receive_message.prefab:8e90a827cd4a0e849953158396cd1ee1");
		Hashtable scaleInArgs = iTweenManager.Get().GetTweenHashTable();
		scaleInArgs.Add("scale", bubbleFrame.m_VisualRoot.transform.localScale);
		scaleInArgs.Add("time", m_ChatBubbleInfo.m_ScaleInSec);
		scaleInArgs.Add("easetype", m_ChatBubbleInfo.m_ScaleInEaseType);
		scaleInArgs.Add("oncomplete", "OnChatBubbleScaleInComplete");
		scaleInArgs.Add("oncompleteparams", bubbleFrame);
		scaleInArgs.Add("oncompletetarget", base.gameObject);
		bubbleFrame.m_VisualRoot.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
		iTween.ScaleTo(bubbleFrame.m_VisualRoot, scaleInArgs);
		MoveChatBubbles(bubbleFrame);
	}

	private ChatBubbleFrame CreateChatBubble(BnetWhisper whisper)
	{
		ChatBubbleFrame bubbleFrame = InstantiateChatBubble(m_Prefabs.m_ChatBubbleOneLineFrame, whisper);
		if (!bubbleFrame.DoesMessageFit())
		{
			UnityEngine.Object.Destroy(bubbleFrame.gameObject);
			bubbleFrame = InstantiateChatBubble(m_Prefabs.m_ChatBubbleSmallFrame, whisper);
		}
		LayerUtils.SetLayer(bubbleFrame, GameLayer.BattleNetDialog);
		return bubbleFrame;
	}

	private ChatBubbleFrame InstantiateChatBubble(ChatBubbleFrame prefab, BnetWhisper whisper)
	{
		ChatBubbleFrame chatBubbleFrame = UnityEngine.Object.Instantiate(prefab);
		chatBubbleFrame.SetWhisper(whisper);
		chatBubbleFrame.GetComponent<PegUIElement>().AddEventListener(UIEventType.RELEASE, OnChatBubbleReleased);
		return chatBubbleFrame;
	}

	private void MoveChatBubbles(ChatBubbleFrame newBubbleFrame)
	{
		Anchor relativeAnchor = Anchor.TOP_LEFT_XZ;
		Anchor bubbleAnchor = Anchor.BOTTOM_LEFT_XZ;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			relativeAnchor = Anchor.BOTTOM_LEFT_XZ;
			bubbleAnchor = Anchor.TOP_LEFT_XZ;
		}
		TransformUtil.SetPoint(newBubbleFrame, bubbleAnchor, m_ChatBubbleInfo.m_Parent, relativeAnchor, Vector3.zero);
		int count = m_chatBubbleFrames.Count;
		if (count != 1)
		{
			Vector3[] currPositions = new Vector3[count - 1];
			Component relativeObject = newBubbleFrame;
			for (int i = count - 2; i >= 0; i--)
			{
				ChatBubbleFrame bubbleFrame = m_chatBubbleFrames[i];
				currPositions[i] = bubbleFrame.transform.position;
				TransformUtil.SetPoint(bubbleFrame, bubbleAnchor, relativeObject, relativeAnchor, Vector3.zero);
				relativeObject = bubbleFrame;
			}
			for (int i2 = count - 2; i2 >= 0; i2--)
			{
				ChatBubbleFrame bubbleFrame2 = m_chatBubbleFrames[i2];
				Hashtable moveOverArgs = iTweenManager.Get().GetTweenHashTable();
				moveOverArgs.Add("islocal", true);
				moveOverArgs.Add("position", bubbleFrame2.transform.localPosition);
				moveOverArgs.Add("time", m_ChatBubbleInfo.m_MoveOverSec);
				moveOverArgs.Add("easetype", m_ChatBubbleInfo.m_MoveOverEaseType);
				bubbleFrame2.transform.position = currPositions[i2];
				iTween.Stop(bubbleFrame2.gameObject, "move");
				iTween.MoveTo(bubbleFrame2.gameObject, moveOverArgs);
			}
		}
	}

	private void OnChatBubbleScaleInComplete(ChatBubbleFrame bubbleFrame)
	{
		Hashtable fadeOutArgs = iTweenManager.Get().GetTweenHashTable();
		fadeOutArgs.Add("amount", 0f);
		fadeOutArgs.Add("delay", m_ChatBubbleInfo.m_HoldSec);
		fadeOutArgs.Add("time", m_ChatBubbleInfo.m_FadeOutSec);
		fadeOutArgs.Add("easetype", m_ChatBubbleInfo.m_FadeOutEaseType);
		fadeOutArgs.Add("oncomplete", "OnChatBubbleFadeOutComplete");
		fadeOutArgs.Add("oncompleteparams", bubbleFrame);
		fadeOutArgs.Add("oncompletetarget", base.gameObject);
		iTween.FadeTo(bubbleFrame.gameObject, fadeOutArgs);
	}

	private void OnChatBubbleFadeOutComplete(ChatBubbleFrame bubbleFrame)
	{
		UnityEngine.Object.Destroy(bubbleFrame.gameObject);
		m_chatBubbleFrames.Remove(bubbleFrame);
	}

	private void RemoveAllChatBubbles()
	{
		foreach (ChatBubbleFrame chatBubbleFrame in m_chatBubbleFrames)
		{
			UnityEngine.Object.Destroy(chatBubbleFrame.gameObject);
		}
		m_chatBubbleFrames.Clear();
	}

	private void OnChatBubbleReleased(UIEvent e)
	{
		BnetPlayer theirPlayer = WhisperUtil.GetTheirPlayer(e.GetElement().GetComponent<ChatBubbleFrame>().GetWhisper());
		ShowChatForPlayer(theirPlayer);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			RemoveAllChatBubbles();
		}
	}

	public void OnKeyboardShow()
	{
		if (m_chatLogUI.IsShowing && BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.position != m_chatLogUI.GameObject.transform.position)
		{
			ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
			touchScreenService.RemoveOnVirtualKeyboardShowListener(OnKeyboardShow);
			touchScreenService.RemoveOnVirtualKeyboardHideListener(OnKeyboardHide);
			m_chatLogUI.Hide();
			m_chatLogUI.ShowForPlayer(GetMostRecentWhisperedPlayer());
			touchScreenService.AddOnVirtualKeyboardShowListener(OnKeyboardShow);
			touchScreenService.AddOnVirtualKeyboardHideListener(OnKeyboardHide);
		}
		if ((bool)BnetBarFriendButton.Get())
		{
			Vector2 offset = new Vector2(0f, Screen.height - 150);
			GameObject relativeObject = BnetBarFriendButton.Get().gameObject;
			TransformUtil.SetPoint(m_ChatBubbleInfo.m_Parent, Anchor.BOTTOM_LEFT_XZ, relativeObject, Anchor.BOTTOM_RIGHT_XZ, offset);
		}
		int count = m_chatBubbleFrames.Count;
		if (count != 0)
		{
			Component relativeComp = m_ChatBubbleInfo.m_Parent;
			for (int i = count - 1; i >= 0; i--)
			{
				ChatBubbleFrame chatBubbleFrame = m_chatBubbleFrames[i];
				TransformUtil.SetPoint(chatBubbleFrame, Anchor.TOP_LEFT_XZ, relativeComp, Anchor.BOTTOM_LEFT_XZ, Vector3.zero);
				relativeComp = chatBubbleFrame;
			}
		}
	}

	public void OnKeyboardHide()
	{
		UpdateLayout();
		UpdateChatBubbleLayout();
	}
}
