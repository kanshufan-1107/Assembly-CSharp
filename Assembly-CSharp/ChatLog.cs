using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using UnityEngine;

public class ChatLog : MonoBehaviour
{
	[Serializable]
	public class Prefabs
	{
		public MobileChatLogMessageFrame myMessage;

		public MobileChatLogMessageFrame theirMessage;

		public MobileChatLogMessageFrame systemMessage;

		public MobileChatLogMessageFrame deckcodeMessage;
	}

	[Serializable]
	public class MessageInfo
	{
		public Color infoColor = Color.yellow;

		public Color errorColor = Color.red;

		public Color notificationColor = Color.cyan;
	}

	public TouchList messageFrames;

	public GameObject cameraTarget;

	public Prefabs prefabs;

	public MessageInfo messageInfo;

	public MobileChatNotification notifications;

	private const int maxMessageFrames = 500;

	private const int frameWidthOffset = 10;

	private const GameLayer messageLayer = GameLayer.BattleNetChat;

	private const CustomViewEntryPoint BattleNetChatViewEntryPoint = CustomViewEntryPoint.BattleNetChat;

	private const string OverridePassName = "BattleNetChatLog";

	private const string BgOverridePassName = "BattleNetChatBG";

	private const uint MessagesRenderingLayerMask = 1u;

	private const uint BgRenderingLayerMask = 2u;

	private BnetPlayer receiver;

	private IGraphicsManager m_graphicsManager;

	private CameraOverridePass m_messagesCameraOverridePass;

	private CameraOverridePass m_bgCameraOverridePass;

	public BnetPlayer Receiver
	{
		get
		{
			return receiver;
		}
		set
		{
			if (receiver == value)
			{
				return;
			}
			receiver = value;
			if (receiver != null)
			{
				UpdateMessages();
				if (!receiver.IsOnline())
				{
					AddReceiverOfflineMessage();
				}
				messageFrames.ScrollValue = 1f;
			}
		}
	}

	private void Awake()
	{
		CreateMessagesCamera();
		if (notifications != null)
		{
			notifications.Notified += OnNotified;
		}
		BnetWhisperMgr.Get().AddWhisperListener(OnWhisper);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_graphicsManager.OnResolutionChangedEvent += OnResizeAfterCurrentFrame;
	}

	private void Start()
	{
		messageFrames.SelectionEnabled = true;
	}

	private void OnDestroy()
	{
		m_messagesCameraOverridePass?.Unschedule();
		m_bgCameraOverridePass?.Unschedule();
		if (PegUI.Get() != null)
		{
			PegUI.Get().UnregisterFromRenderPassPriorityHitTest(this);
		}
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
		BnetWhisperMgr.Get().RemoveWhisperListener(OnWhisper);
		if (m_graphicsManager != null)
		{
			m_graphicsManager.OnResolutionChangedEvent -= OnResizeAfterCurrentFrame;
		}
		if (notifications != null)
		{
			notifications.Notified -= OnNotified;
		}
	}

	public void OnResize()
	{
		ResizeMessageFrames();
		UpdateMessagesCamera();
	}

	private void ResizeMessageFrames()
	{
		float previousScrollValue = messageFrames.ScrollValue;
		foreach (ITouchListItem renderedItem in messageFrames.RenderedItems)
		{
			MobileChatLogMessageFrame messageFrame = renderedItem as MobileChatLogMessageFrame;
			if (messageFrame != null)
			{
				messageFrame.Width = messageFrames.ClipSize.x - messageFrames.padding.x - 10f;
				messageFrame.UpdateLocalBounds();
			}
		}
		messageFrames.RecalculateItemSizeAndOffsets(ignoreCurrentPosition: true);
		messageFrames.ScrollValue = previousScrollValue;
	}

	public void OnWhisperFailed()
	{
		if (!BnetPresenceMgr.Get().GetMyPlayer().IsOnline())
		{
			AddSenderOfflineMessage();
		}
		else
		{
			AddReceiverOfflineMessage();
		}
	}

	private void OnWhisper(BnetWhisper whisper, object userData)
	{
		if (receiver != null && WhisperUtil.IsSpeakerOrReceiver(receiver, whisper))
		{
			AddWhisperMessage(whisper);
			messageFrames.ScrollValue = 1f;
		}
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		BnetPlayerChange change = changelist.FindChange(receiver);
		if (change == null)
		{
			return;
		}
		BnetPlayer oldPlayer = change.GetOldPlayer();
		BnetPlayer newPlayer = change.GetNewPlayer();
		if (oldPlayer == null || oldPlayer.IsOnline() != newPlayer.IsOnline())
		{
			if (newPlayer.IsOnline())
			{
				AddOnlineMessage();
			}
			else
			{
				AddReceiverOfflineMessage();
			}
		}
	}

	private void OnNotified(string text)
	{
		AddSystemMessage(text, messageInfo.notificationColor);
	}

	private void UpdateMessages()
	{
		List<MobileChatLogMessageFrame> list = messageFrames.RenderedItems.Select((ITouchListItem i) => i.GetComponent<MobileChatLogMessageFrame>()).ToList();
		messageFrames.Clear();
		foreach (MobileChatLogMessageFrame item in list)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		List<BnetWhisper> whispers = BnetWhisperMgr.Get().GetWhispersWithPlayer(receiver);
		if (whispers != null && whispers.Count > 0)
		{
			for (int j = Mathf.Max(whispers.Count - 500, 0); j < whispers.Count; j++)
			{
				BnetWhisper whisper = whispers[j];
				AddWhisperMessage(whisper);
			}
		}
		OnMessagesAdded();
	}

	private void AddWhisperMessage(BnetWhisper whisper)
	{
		string whisperMessage = ChatUtils.GetMessage(whisper);
		string deckName;
		ShareableMercenariesTeam shareableMercenariesTeam = ShareableMercenariesTeam.ParseDeckCode(whisperMessage, out deckName);
		if (shareableMercenariesTeam != null)
		{
			shareableMercenariesTeam.DeckName = deckName;
			MobileChatLogMessageFrame frame = CreateDeckcodeMessage(prefabs.deckcodeMessage, shareableMercenariesTeam);
			messageFrames.Add(frame);
			return;
		}
		ShareableDeck shareableDeck = ShareableDeck.ParseDeckCode(whisperMessage, out deckName);
		if (shareableDeck != null)
		{
			shareableDeck.DeckName = deckName;
			MobileChatLogMessageFrame frame2 = CreateDeckcodeMessage(prefabs.deckcodeMessage, shareableDeck);
			messageFrames.Add(frame2);
		}
		else
		{
			MobileChatLogMessageFrame prefab = (WhisperUtil.IsSpeaker(receiver, whisper) ? prefabs.theirMessage : prefabs.myMessage);
			MobileChatLogMessageFrame frame3 = CreateMessage(prefab, whisperMessage);
			messageFrames.Add(frame3);
		}
	}

	private void AddSystemMessage(string message, Color color)
	{
		MobileChatLogMessageFrame frame = CreateMessage(prefabs.systemMessage, message, color);
		messageFrames.Add(frame);
		OnMessagesAdded();
	}

	private void AddOnlineMessage()
	{
		string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_ONLINE", receiver.GetBestName());
		AddSystemMessage(message, messageInfo.infoColor);
	}

	private void AddReceiverOfflineMessage()
	{
		string message = GameStrings.Format("GLOBAL_CHAT_RECEIVER_OFFLINE", receiver.GetBestName());
		AddSystemMessage(message, messageInfo.errorColor);
	}

	private void AddSenderOfflineMessage()
	{
		string message = GameStrings.Get("GLOBAL_CHAT_SENDER_OFFLINE");
		AddSystemMessage(message, messageInfo.errorColor);
	}

	private void OnMessagesAdded()
	{
		if (messageFrames.RenderedItems.Count() > 500)
		{
			ITouchListItem touchListItem = messageFrames.RenderedItems.First();
			messageFrames.RemoveAt(0);
			UnityEngine.Object.Destroy(touchListItem.gameObject);
		}
		messageFrames.ScrollValue = 1f;
	}

	private MobileChatLogMessageFrame CreateMessage(MobileChatLogMessageFrame prefab, string message)
	{
		MobileChatLogMessageFrame mobileChatLogMessageFrame = UnityEngine.Object.Instantiate(prefab);
		mobileChatLogMessageFrame.Width = messageFrames.ClipSize.x - messageFrames.padding.x - 10f;
		mobileChatLogMessageFrame.Message = message;
		LayerUtils.SetLayer(mobileChatLogMessageFrame, GameLayer.BattleNetChat);
		return mobileChatLogMessageFrame;
	}

	private MobileChatLogMessageFrame CreateMessage(MobileChatLogMessageFrame prefab, string message, Color color)
	{
		MobileChatLogMessageFrame mobileChatLogMessageFrame = CreateMessage(prefab, message);
		mobileChatLogMessageFrame.Color = color;
		return mobileChatLogMessageFrame;
	}

	private MobileChatLogDeckcodeMessageFrame CreateDeckcodeMessage(MobileChatLogMessageFrame prefab, ShareableDeck shareableDeck)
	{
		MobileChatLogDeckcodeMessageFrame obj = (MobileChatLogDeckcodeMessageFrame)UnityEngine.Object.Instantiate(prefab);
		obj.Width = messageFrames.ClipSize.x - messageFrames.padding.x - 10f;
		obj.DeckcodeString = shareableDeck.Serialize(includeComments: false);
		obj.BindClassData(shareableDeck);
		LayerUtils.SetLayer(obj, GameLayer.BattleNetChat);
		return obj;
	}

	private void CreateMessagesCamera()
	{
		m_messagesCameraOverridePass = new CameraOverridePass("BattleNetChatLog", GameLayer.BattleNetChat.LayerBit());
		m_bgCameraOverridePass = new CameraOverridePass("BattleNetChatBG", GameLayer.BattleNetChat.LayerBit());
		UpdateMessagesCamera();
		m_bgCameraOverridePass.Schedule(CustomViewEntryPoint.BattleNetChat);
		m_messagesCameraOverridePass.Schedule(CustomViewEntryPoint.BattleNetChat);
		m_messagesCameraOverridePass.OverrideRenderLayerMask(1u);
		m_bgCameraOverridePass.OverrideRenderLayerMask(2u);
		if (PegUI.Get() != null)
		{
			PegUI.Get().RegisterForRenderPassPriorityHitTest(this);
		}
	}

	private Bounds GetBoundsFromGameObject(GameObject go)
	{
		Renderer goRenderer = go.GetComponent<Renderer>();
		if (goRenderer != null)
		{
			return goRenderer.bounds;
		}
		Collider goCollider = go.GetComponent<Collider>();
		if (goCollider != null)
		{
			return goCollider.bounds;
		}
		return default(Bounds);
	}

	private void UpdateMessagesCamera()
	{
		Camera bnetCamera = BaseUI.Get().GetBnetCamera();
		Bounds bounds = GetBoundsFromGameObject(cameraTarget);
		Vector3 min = bnetCamera.WorldToScreenPoint(bounds.min);
		Vector3 max = bnetCamera.WorldToScreenPoint(bounds.max);
		m_messagesCameraOverridePass.OverrideScissor(new Rect(min.x, min.y, max.x - min.x, max.y - min.y));
	}

	private void OnResizeAfterCurrentFrame(int width, int height)
	{
		StartCoroutine(UpdateMessagesCameraAfterCurrentFrame());
	}

	private IEnumerator UpdateMessagesCameraAfterCurrentFrame()
	{
		yield return null;
		UpdateMessagesCamera();
	}

	[Conditional("CHATLOG_DEBUG")]
	private void AssignMessageFrameNames()
	{
		int index = 0;
		foreach (ITouchListItem renderedItem in messageFrames.RenderedItems)
		{
			MobileChatLogMessageFrame frame = renderedItem.GetComponent<MobileChatLogMessageFrame>();
			frame.name = $"MessageFrame {index} ({frame.Message})";
			index++;
		}
	}
}
