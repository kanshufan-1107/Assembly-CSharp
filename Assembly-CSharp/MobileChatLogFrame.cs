using System;
using System.Linq;
using Hearthstone.UI;
using UnityEngine;

public class MobileChatLogFrame : MonoBehaviour
{
	[Serializable]
	public class MessageInfo
	{
		public Transform messagesTopLeft;

		public Transform messagesBottomRight;
	}

	[Serializable]
	public class InputInfo
	{
		public Transform inputTopLeft;

		public Transform inputBottomRight;
	}

	[Serializable]
	public class Followers
	{
		public UIBFollowObject playerInfoFollower;

		public UIBFollowObject closeButtonFollower;

		public UIBFollowObject bubbleFollower;

		public void UpdateFollowPosition()
		{
			playerInfoFollower.UpdateFollowPosition();
			closeButtonFollower.UpdateFollowPosition();
			bubbleFollower.UpdateFollowPosition();
		}
	}

	public Spawner playerIconRef;

	public TouchList messageFrames;

	public InputInfo inputInfo;

	public TextField inputTextField;

	public MessageInfo messageInfo;

	public NineSliceElement window;

	public UberText nameText;

	public UIBButton closeButton;

	public MobileChatNotification notifications;

	public AsyncReference m_rankedMedalWidgetReference;

	public ChatLog chatLog;

	public Followers followers;

	private PlayerIcon playerIcon;

	private BnetPlayer receiver;

	private SelectableMedal m_selectableMedal;

	private Widget m_selectableMedalWidget;

	private const int ADJUST_CLOSE_BTN_COLLIDER_DELAY = 2;

	private const int CLOSE_BTN_COLLIDER_Y = 4;

	private const int CLOSE_BTN_COLLIDER_X_OFFSET = 5;

	private const int CLOSE_BTN_COLLIDER_X_FACTOR = 2;

	public bool HasFocus => inputTextField.Active;

	public BnetPlayer Receiver
	{
		get
		{
			return receiver;
		}
		set
		{
			if (receiver != value)
			{
				receiver = value;
				if (receiver != null)
				{
					playerIcon.SetPlayer(receiver);
					UpdateReceiver();
					chatLog.Receiver = receiver;
				}
			}
		}
	}

	public bool IsWaitingOnMedal
	{
		get
		{
			MedalInfoTranslator mit = RankMgr.Get().GetRankedMedalFromRankPresenceField(receiver.GetBestGameAccount());
			if (mit != null && mit.IsDisplayable())
			{
				if (!(m_selectableMedalWidget == null) && m_selectableMedalWidget.IsReady)
				{
					return m_selectableMedalWidget.IsChangingStates;
				}
				return true;
			}
			return false;
		}
	}

	public event Action InputCanceled;

	public event Action CloseButtonReleased;

	private void Awake()
	{
		playerIcon = playerIconRef.Spawn<PlayerIcon>();
		UpdateBackgroundCollider();
		inputTextField.maxCharacters = 512;
		inputTextField.Changed += OnInputChanged;
		inputTextField.Submitted += OnInputComplete;
		inputTextField.Canceled += OnInputCanceled;
		closeButton.AddEventListener(UIEventType.RELEASE, OnCloseButtonReleased);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
	}

	private void Start()
	{
		if (receiver == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			string savedMessage = ChatMgr.Get().GetPendingMessage(receiver.GetAccountId());
			if (savedMessage != null)
			{
				inputTextField.Text = savedMessage;
			}
		}
		m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnSelectableMedalWidgetReady);
	}

	private void OnDestroy()
	{
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
	}

	public void Focus(bool focus)
	{
		if (focus && !inputTextField.Active)
		{
			inputTextField.Activate();
		}
		else if (!focus && inputTextField.Active)
		{
			inputTextField.Deactivate();
		}
	}

	public void SetWorldRect(float x, float z, float width, float height)
	{
		bool wasActive = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		float viewMax = messageFrames.ViewWindowMaxValue;
		window.SetEntireSize(width, height);
		Vector3 topLeft = TransformUtil.ComputeWorldPoint(TransformUtil.ComputeSetPointBounds(window), new Vector3(0f, 0f, 1f));
		Vector3 delta = new Vector3(x, topLeft.y, z) - topLeft;
		base.transform.Translate(delta, Space.World);
		messageFrames.transform.position = (messageInfo.messagesTopLeft.position + messageInfo.messagesBottomRight.position) / 2f;
		Vector3 listClipSize3D = messageInfo.messagesBottomRight.position - messageInfo.messagesTopLeft.position;
		listClipSize3D *= 4f;
		messageFrames.ClipSize = new Vector2(listClipSize3D.x, Math.Abs(listClipSize3D.z));
		messageFrames.ViewWindowMaxValue = viewMax;
		messageFrames.ScrollValue = Mathf.Clamp01(messageFrames.ScrollValue);
		chatLog.OnResize();
		UpdateBackgroundCollider();
		UpdateFollowers();
		base.gameObject.SetActive(wasActive);
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (changelist.FindChange(receiver) != null)
		{
			UpdateReceiver();
		}
	}

	private void OnSelectableMedalWidgetReady(Widget widget)
	{
		m_selectableMedalWidget = widget;
		m_selectableMedal = widget.GetComponentInChildren<SelectableMedal>();
		UpdateSelectableMedalWidget();
	}

	private void OnCloseButtonReleased(UIEvent e)
	{
		if (this.CloseButtonReleased != null)
		{
			this.CloseButtonReleased();
		}
	}

	private bool IsFullScreenKeyboard()
	{
		return ChatMgr.Get().KeyboardRect.height == (float)Screen.height;
	}

	private void OnInputChanged(string input)
	{
		ChatMgr.Get().SetPendingMessage(receiver.GetAccountId(), input);
	}

	public void OnInputComplete(string input)
	{
		if (!string.IsNullOrEmpty(input))
		{
			if (!BnetWhisperMgr.Get().SendWhisper(receiver, input))
			{
				chatLog.OnWhisperFailed();
			}
			ChatMgr.Get().SetPendingMessage(receiver.GetAccountId(), null);
			ChatMgr.Get().AddRecentWhisperPlayerToTop(receiver);
		}
	}

	private void OnInputCanceled()
	{
		if (this.InputCanceled != null)
		{
			this.InputCanceled();
		}
	}

	private void UpdateReceiver()
	{
		playerIcon.UpdateIcon();
		string color = (receiver.IsOnline() ? "5ecaf0ff" : "999999ff");
		string playerName = receiver.GetBestName();
		nameText.Text = $"<color=#{color}>{playerName}</color>";
		UpdateSelectableMedalWidget();
	}

	private void UpdateSelectableMedalWidget()
	{
		if (m_selectableMedal == null || !receiver.IsOnline())
		{
			playerIcon.Show();
			return;
		}
		playerIcon.Hide();
		m_selectableMedal.gameObject.SetActive(value: true);
		m_selectableMedal.UpdateWidget(receiver, null, null, delegate
		{
			playerIcon.Show();
			m_selectableMedal.gameObject.SetActive(value: false);
		});
	}

	private void UpdateBackgroundCollider()
	{
		BoxCollider collider = GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = base.gameObject.AddComponent<BoxCollider>();
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			collider.center = new Vector3(0f, 0f, 50f);
			collider.size = new Vector3(10000f, 10000f, 0f);
			return;
		}
		Bounds bounds = window.GetComponentsInChildren<Renderer>().Aggregate(new Bounds(base.transform.position, Vector3.zero), delegate(Bounds aggregate, Renderer renderer)
		{
			if (renderer.bounds.size.x != 0f && renderer.bounds.size.y != 0f && renderer.bounds.size.z != 0f)
			{
				aggregate.Encapsulate(renderer.bounds);
			}
			return aggregate;
		});
		Vector3 min = base.transform.InverseTransformPoint(bounds.min);
		Vector3 max = base.transform.InverseTransformPoint(bounds.max);
		collider.center = (min + max) / 2f + Vector3.forward;
		collider.size = max - min;
		collider.size = new Vector3(collider.size.x, collider.size.y, 0f);
	}

	private void UpdateFollowers()
	{
		followers.UpdateFollowPosition();
	}
}
