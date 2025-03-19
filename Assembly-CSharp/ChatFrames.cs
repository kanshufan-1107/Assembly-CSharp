using Blizzard.GameService.SDK.Client.Integration;
using UnityEngine;

public class ChatFrames : MonoBehaviour
{
	public MobileChatLogFrame chatLogFrame;

	private bool wasShowingDialog;

	public BnetPlayer Receiver
	{
		get
		{
			return chatLogFrame.Receiver;
		}
		set
		{
			chatLogFrame.Receiver = value;
			if (chatLogFrame.Receiver == null)
			{
				ChatMgr.Get().CloseChatUI();
			}
			OnFramesMoved();
		}
	}

	private void Awake()
	{
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		chatLogFrame.CloseButtonReleased += OnCloseButtonReleased;
		ChatUtils.TrySendDeckcodeFromClipboard(chatLogFrame.OnInputComplete);
	}

	private void OnDestroy()
	{
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded);
		}
		if (FatalErrorMgr.Get() != null)
		{
			FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		}
		if (Network.Get() != null)
		{
			Network.Get().OnDisconnectedFromBattleNet -= OnDisconnectedFromBattleNet;
		}
		OnFramesMoved();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnChatReady()
	{
		if (DialogManager.Get().ShowingDialog())
		{
			chatLogFrame.Focus(focus: false);
			wasShowingDialog = true;
		}
		else
		{
			chatLogFrame.Focus(focus: true);
		}
	}

	private void Update()
	{
		bool showingDialog = DialogManager.Get().ShowingDialog();
		if (showingDialog != wasShowingDialog)
		{
			if (showingDialog && chatLogFrame.HasFocus)
			{
				OnPopupOpened();
			}
			else if (!showingDialog && !chatLogFrame.HasFocus && (ChatMgr.Get().FriendListFrame == null || !ChatMgr.Get().FriendListFrame.ShowingAddFriendFrame))
			{
				OnPopupClosed();
			}
			wasShowingDialog = showingDialog;
		}
	}

	public void Back()
	{
		if (!DialogManager.Get().ShowingDialog())
		{
			if (ChatMgr.Get().FriendListFrame.ShowingAddFriendFrame)
			{
				ChatMgr.Get().FriendListFrame.CloseAddFriendFrame();
			}
			else if (Receiver != null)
			{
				Receiver = null;
			}
			else
			{
				ChatMgr.Get().CloseChatUI();
			}
		}
	}

	public void SetHeightInScene(float value)
	{
		Vector3 pos = base.transform.position;
		base.transform.position = new Vector3(pos.x, value, pos.z);
	}

	private void OnFramesMoved()
	{
		if (ChatMgr.Get() != null)
		{
			ChatMgr.Get().OnChatFramesMoved();
		}
	}

	private void OnCloseButtonReleased()
	{
		ChatMgr.Get().CloseChatUI();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ChatMgr.Get().ShowFriendsList();
		}
	}

	private void OnPopupOpened()
	{
		if (chatLogFrame.HasFocus)
		{
			chatLogFrame.Focus(focus: false);
		}
	}

	private void OnPopupClosed()
	{
		if (Receiver != null)
		{
			chatLogFrame.Focus(focus: true);
		}
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		ChatMgr.Get().CleanUp();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		ChatMgr.Get().CleanUp();
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.FATAL_ERROR)
		{
			ChatMgr.Get().CleanUp();
		}
	}
}
