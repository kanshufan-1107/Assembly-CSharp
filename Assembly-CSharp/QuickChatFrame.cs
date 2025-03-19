using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Fonts;
using Blizzard.T5.Services;
using UnityEngine;

public class QuickChatFrame : MonoBehaviour
{
	public QuickChatFrameBones m_Bones;

	public QuickChatFramePrefabs m_Prefabs;

	public GameObject m_Background;

	public UberText m_ReceiverNameText;

	public UberText m_LastMessageText;

	public GameObject m_LastMessageShadow;

	public PegUIElement m_ChatLogButton;

	public Font m_InputFont;

	private DropdownControl m_recentPlayerDropdown;

	private ChatLogFrame m_chatLogFrame;

	private PegUIElement m_inputBlocker;

	private List<BnetPlayer> m_recentPlayers = new List<BnetPlayer>();

	private BnetPlayer m_receiver;

	private float m_initialLastMessageTextHeight;

	private float m_initialLastMessageShadowScaleZ;

	private Font m_localizedInputFont;

	private IFontTable m_fontTable;

	private Map<Renderer, int> m_chatLogOriginalLayers = new Map<Renderer, int>();

	private void Awake()
	{
		m_fontTable = ServiceManager.Get<IFontTable>();
		InitRecentPlayers();
		if (!InitReceiver())
		{
			Object.Destroy(base.gameObject);
			return;
		}
		BnetWhisperMgr.Get().AddWhisperListener(OnWhisper);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		InitTransform();
		InitInputBlocker();
		InitChatLogFrame();
		InitInput();
		ShowInput(fromAwake: true);
		ChatUtils.TrySendDeckcodeFromClipboard(OnInputComplete);
	}

	private void Start()
	{
		InitLastMessage();
		InitRecentPlayerDropdown();
		if (ChatMgr.Get().IsChatLogFrameShown())
		{
			ShowChatLogFrame(onStart: true);
		}
		UpdateReceiver();
		ChatMgr.Get().OnChatReceiverChanged(m_receiver);
	}

	private void OnDestroy()
	{
		BnetWhisperMgr.Get().RemoveWhisperListener(OnWhisper);
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().CancelTextInput(base.gameObject);
		}
		ChatMgr.Get().OnQuickChatFrameClosed();
	}

	public ChatLogFrame GetChatLogFrame()
	{
		return m_chatLogFrame;
	}

	public BnetPlayer GetReceiver()
	{
		return m_receiver;
	}

	public void SetReceiver(BnetPlayer player)
	{
		if (m_receiver != player)
		{
			m_receiver = player;
			UpdateReceiver();
			m_recentPlayerDropdown.setSelection(player);
			ChatMgr.Get().OnChatReceiverChanged(player);
		}
	}

	public void UpdateLayout()
	{
		if (m_chatLogFrame != null)
		{
			m_chatLogFrame.UpdateLayout();
		}
	}

	private void InitRecentPlayers()
	{
		UpdateRecentPlayers();
	}

	private void UpdateRecentPlayers()
	{
		m_recentPlayers.Clear();
		List<BnetPlayer> recentPlayers = ChatMgr.Get().GetRecentWhisperPlayers();
		for (int i = 0; i < recentPlayers.Count; i++)
		{
			BnetPlayer receiver = recentPlayers[i];
			m_recentPlayers.Add(receiver);
		}
	}

	private bool InitReceiver()
	{
		m_receiver = null;
		if (m_recentPlayers.Count == 0)
		{
			string message = ((BnetFriendMgr.Get().GetOnlineFriendCount() != 0) ? GameStrings.Get("GLOBAL_CHAT_NO_RECENT_CONVERSATIONS") : GameStrings.Get("GLOBAL_CHAT_NO_FRIENDS_ONLINE"));
			UIStatus.Get().AddError(message);
			return false;
		}
		m_receiver = m_recentPlayers[0];
		return true;
	}

	private void OnWhisper(BnetWhisper whisper, object userData)
	{
		if (m_receiver != null && WhisperUtil.IsSpeaker(m_receiver, whisper))
		{
			UpdateReceiver();
		}
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		BnetPlayerChange change = changelist.FindChange(m_receiver);
		if (change != null)
		{
			BnetPlayer oldPlayer = change.GetOldPlayer();
			BnetPlayer newPlayer = change.GetNewPlayer();
			if (oldPlayer == null || oldPlayer.IsOnline() != newPlayer.IsOnline())
			{
				UpdateReceiver();
			}
		}
	}

	private BnetWhisper FindLastWhisperFromReceiver()
	{
		List<BnetWhisper> whispers = BnetWhisperMgr.Get().GetWhispersWithPlayer(m_receiver);
		if (whispers == null)
		{
			return null;
		}
		for (int i = whispers.Count - 1; i >= 0; i--)
		{
			BnetWhisper whisper = whispers[i];
			if (WhisperUtil.IsSpeaker(m_receiver, whisper))
			{
				return whisper;
			}
		}
		return null;
	}

	private void InitTransform()
	{
		base.transform.parent = BaseUI.Get().transform;
		DefaultChatTransform();
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		if ((UniversalInputManager.Get().UseWindowsTouch() && touchScreenService.IsTouchSupported()) || touchScreenService.IsVirtualKeyboardVisible())
		{
			TransformChatForKeyboard();
		}
	}

	private void InitLastMessage()
	{
		m_LastMessageText.Text = "*** DO NOT DELETE. THIS TEXT IS USED FOR SIZE COMPUTATIONS. ***";
		m_initialLastMessageTextHeight = m_LastMessageText.GetTextWorldSpaceBounds().size.y;
		m_LastMessageText.Text = string.Empty;
		m_initialLastMessageShadowScaleZ = m_LastMessageShadow.transform.localScale.z;
	}

	private void InitInputBlocker()
	{
		Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		float offset = m_Bones.m_InputBlocker.position.z - base.transform.position.z;
		GameObject inputBlockerGo = CameraUtils.CreateInputBlocker(camera, "QuickChatInputBlocker", this, offset);
		inputBlockerGo.layer = 26;
		m_inputBlocker = inputBlockerGo.AddComponent<PegUIElement>();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, OnInputBlockerReleased);
	}

	private void OnInputBlockerReleased(UIEvent e)
	{
		Object.Destroy(base.gameObject);
	}

	private void InitChatLogFrame()
	{
		m_ChatLogButton.AddEventListener(UIEventType.RELEASE, OnChatLogButtonReleased);
	}

	private void OnChatLogButtonReleased(UIEvent e)
	{
		if (ChatMgr.Get().IsChatLogFrameShown())
		{
			HideChatLogFrame();
		}
		else
		{
			ShowChatLogFrame();
		}
		UpdateReceiver();
		UniversalInputManager.Get().FocusTextInput(base.gameObject);
	}

	private void ShowChatLogFrame(bool onStart = false)
	{
		m_chatLogFrame = Object.Instantiate(m_Prefabs.m_ChatLogFrame);
		bool isSmall = base.transform.localScale == BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.localScale;
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		if ((((UniversalInputManager.Get().IsTouchMode() && touchScreenService.IsTouchSupported()) || touchScreenService.IsVirtualKeyboardVisible()) && isSmall) || isSmall)
		{
			DefaultChatTransform();
		}
		m_chatLogFrame.transform.parent = base.transform;
		m_chatLogFrame.transform.position = m_Bones.m_ChatLog.position;
		if ((((UniversalInputManager.Get().UseWindowsTouch() && touchScreenService.IsTouchSupported()) || touchScreenService.IsVirtualKeyboardVisible()) && isSmall) || isSmall)
		{
			TransformChatForKeyboard();
		}
		GameObject objectToHide = (onStart ? base.gameObject : m_chatLogFrame.gameObject);
		StartCoroutine(ShowChatLogFrameWhenReady(objectToHide));
	}

	private IEnumerator ShowChatLogFrameWhenReady(GameObject obj)
	{
		while (m_chatLogFrame == null || m_chatLogFrame.IsWaitingOnMedal)
		{
			if (m_chatLogFrame == null)
			{
				yield break;
			}
			yield return null;
		}
		ChatMgr.Get().OnChatLogFrameShown();
	}

	private void HideChatLogFrame()
	{
		Object.Destroy(m_chatLogFrame.gameObject);
		m_chatLogFrame = null;
		ChatMgr.Get().OnChatLogFrameHidden();
	}

	private void InitRecentPlayerDropdown()
	{
		m_recentPlayerDropdown = Object.Instantiate(m_Prefabs.m_Dropdown, base.transform);
		m_recentPlayerDropdown.transform.position = m_Bones.m_RecentPlayerDropdown.position;
		m_recentPlayerDropdown.setItemTextCallback(OnRecentPlayerDropdownText);
		m_recentPlayerDropdown.setItemChosenCallback(OnRecentPlayerDropdownItemChosen);
		UpdateRecentPlayerDropdown();
		m_recentPlayerDropdown.setSelection(m_receiver);
	}

	private void UpdateRecentPlayerDropdown()
	{
		m_recentPlayerDropdown.clearItems();
		for (int i = 0; i < m_recentPlayers.Count; i++)
		{
			m_recentPlayerDropdown.addItem(m_recentPlayers[i]);
		}
	}

	private string OnRecentPlayerDropdownText(object val)
	{
		return FriendUtils.GetUniqueName((BnetPlayer)val);
	}

	private void OnRecentPlayerDropdownItemChosen(object selection, object prevSelection)
	{
		BnetPlayer friend = (BnetPlayer)selection;
		SetReceiver(friend);
	}

	private void UpdateReceiver()
	{
		UpdateLastMessage();
		if (m_chatLogFrame != null)
		{
			m_chatLogFrame.Receiver = m_receiver;
		}
	}

	private void UpdateLastMessage()
	{
		if (m_chatLogFrame != null)
		{
			HideLastMessage();
			return;
		}
		BnetWhisper lastWhisper = FindLastWhisperFromReceiver();
		if (lastWhisper == null)
		{
			HideLastMessage();
			return;
		}
		m_LastMessageText.gameObject.SetActive(value: true);
		string rawWhisperText = ChatUtils.GetMessage(lastWhisper);
		if (ChatUtils.TryGetFormattedDeckcodeMessage(rawWhisperText, showHint: false, out var formattedDeckcodeText))
		{
			m_LastMessageText.Text = formattedDeckcodeText;
		}
		else
		{
			m_LastMessageText.Text = rawWhisperText;
		}
		TransformUtil.SetPoint(m_LastMessageText, Anchor.BOTTOM_LEFT, m_Bones.m_LastMessage, Anchor.TOP_LEFT);
		m_ReceiverNameText.gameObject.SetActive(value: true);
		if (m_receiver.IsOnline())
		{
			m_ReceiverNameText.TextColor = GameColors.PLAYER_NAME_ONLINE;
		}
		else
		{
			m_ReceiverNameText.TextColor = GameColors.PLAYER_NAME_OFFLINE;
		}
		m_ReceiverNameText.Text = FriendUtils.GetUniqueName(m_receiver);
		TransformUtil.SetPoint(m_ReceiverNameText, Anchor.BOTTOM_LEFT_XZ, m_LastMessageText, Anchor.TOP_LEFT_XZ);
		m_LastMessageShadow.SetActive(value: true);
		Bounds lastMessageBounds = m_LastMessageText.GetTextWorldSpaceBounds();
		Bounds receiverBounds = m_ReceiverNameText.GetTextWorldSpaceBounds();
		float num = Mathf.Max(lastMessageBounds.max.y, receiverBounds.max.y);
		float textMinY = Mathf.Min(lastMessageBounds.min.y, receiverBounds.min.y);
		float lastMessageShadowScaleZ = (num - textMinY) * m_initialLastMessageShadowScaleZ / m_initialLastMessageTextHeight;
		TransformUtil.SetLocalScaleZ(m_LastMessageShadow, lastMessageShadowScaleZ);
	}

	private void HideLastMessage()
	{
		m_ReceiverNameText.gameObject.SetActive(value: false);
		m_LastMessageText.gameObject.SetActive(value: false);
		m_LastMessageShadow.SetActive(value: false);
	}

	private void CyclePrevReceiver()
	{
		int receiverIndex = m_recentPlayers.FindIndex((BnetPlayer currReceiver) => m_receiver == currReceiver);
		BnetPlayer receiver = ((receiverIndex != 0) ? m_recentPlayers[receiverIndex - 1] : m_recentPlayers[m_recentPlayers.Count - 1]);
		SetReceiver(receiver);
	}

	private void CycleNextReceiver()
	{
		int receiverIndex = m_recentPlayers.FindIndex((BnetPlayer currReceiver) => m_receiver == currReceiver);
		BnetPlayer receiver = ((receiverIndex != m_recentPlayers.Count - 1) ? m_recentPlayers[receiverIndex + 1] : m_recentPlayers[0]);
		SetReceiver(receiver);
	}

	private void InitInput()
	{
		FontDefinition inputFontDef = m_fontTable.GetFontDef(m_InputFont);
		if (inputFontDef == null)
		{
			m_localizedInputFont = m_InputFont;
		}
		else
		{
			m_localizedInputFont = inputFontDef.m_Font;
		}
	}

	private void ShowInput(bool fromAwake)
	{
		Camera camera = BaseUI.Get().GetBnetCamera();
		Rect rect = CameraUtils.CreateGUIViewportRect(camera, m_Bones.m_InputTopLeft, m_Bones.m_InputBottomRight);
		if (Localization.GetLocale() == Locale.thTH)
		{
			Vector3 topLeft = camera.WorldToViewportPoint(m_Bones.m_InputTopLeft.position);
			Vector3 bottomRight = camera.WorldToViewportPoint(m_Bones.m_InputBottomRight.position);
			float margin = (topLeft.y - bottomRight.y) * 0.1f;
			topLeft = new Vector3(topLeft.x, topLeft.y - margin, topLeft.z);
			bottomRight = new Vector3(bottomRight.x, bottomRight.y + margin, bottomRight.z);
			rect = new Rect(topLeft.x, 1f - topLeft.y, bottomRight.x - topLeft.x, topLeft.y - bottomRight.y);
		}
		string savedMessage = ChatMgr.Get().GetPendingMessage(m_receiver.GetAccountId());
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = base.gameObject,
			m_rect = rect,
			m_preprocessCallback = OnInputPreprocess,
			m_completedCallback = OnInputComplete,
			m_canceledCallback = OnInputCanceled,
			m_updatedCallback = OnInputChanged,
			m_font = m_localizedInputFont,
			m_maxCharacters = 512,
			m_touchScreenKeyboardHideInput = true,
			m_showVirtualKeyboard = fromAwake,
			m_hideVirtualKeyboardOnComplete = (fromAwake ? true : false),
			m_text = savedMessage
		};
		UniversalInputManager.Get().UseTextInput(inputParams);
	}

	private bool OnInputPreprocess()
	{
		if (m_recentPlayers.Count < 2)
		{
			return false;
		}
		bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetKeyDown(KeyCode.Tab) && shiftHeld))
		{
			CyclePrevReceiver();
			return true;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab))
		{
			CycleNextReceiver();
			return true;
		}
		return false;
	}

	private void OnInputChanged(string input)
	{
		ChatMgr.Get().SetPendingMessage(m_receiver.GetAccountId(), input);
	}

	private void OnInputComplete(string input)
	{
		if (this == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(input))
		{
			BnetPresenceMgr.Get().GetMyPlayer();
			if (!BnetWhisperMgr.Get().SendWhisper(m_receiver, input))
			{
				if (ChatMgr.Get().IsChatLogFrameShown())
				{
					m_chatLogFrame.m_chatLog.OnWhisperFailed();
				}
				else if (!m_receiver.IsOnline())
				{
					string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_OFFLINE", m_receiver.GetBestName());
					UIStatus.Get().AddError(message);
				}
				ChatMgr.Get().AddRecentWhisperPlayerToTop(m_receiver);
			}
		}
		ChatMgr.Get().SetPendingMessage(m_receiver.GetAccountId(), null);
		if (ChatMgr.Get().IsChatLogFrameShown())
		{
			ShowInput(fromAwake: false);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnInputCanceled(bool userRequested, GameObject requester)
	{
		Object.Destroy(base.gameObject);
	}

	private void DefaultChatTransform()
	{
		base.transform.position = BaseUI.Get().m_Bones.m_QuickChat.position;
		base.transform.localScale = BaseUI.Get().m_Bones.m_QuickChat.localScale;
		if (m_chatLogFrame != null)
		{
			m_chatLogFrame.UpdateLayout();
		}
	}

	private void TransformChatForKeyboard()
	{
		base.transform.position = BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.position;
		base.transform.localScale = BaseUI.Get().m_Bones.m_QuickChatVirtualKeyboard.localScale;
		m_Prefabs.m_Dropdown.transform.localScale = new Vector3(50f, 50f, 50f);
		if (m_chatLogFrame != null)
		{
			m_chatLogFrame.UpdateLayout();
		}
	}
}
