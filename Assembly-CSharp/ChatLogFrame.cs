using System;
using Hearthstone.UI;
using UnityEngine;

public class ChatLogFrame : MonoBehaviour
{
	public ChatLogFrameBones m_Bones;

	public ChatLogFramePrefabs m_Prefabs;

	public UberText m_NameText;

	public ChatLog m_chatLog;

	public GameObject m_medalPatch;

	public AsyncReference m_rankedMedalWidgetReference;

	private PlayerIcon m_playerIcon;

	private BnetPlayer m_receiver;

	private SelectableMedal m_selectableMedal;

	private Widget m_selectableMedalWidget;

	public BnetPlayer Receiver
	{
		get
		{
			return m_receiver;
		}
		set
		{
			if (m_receiver != value)
			{
				m_receiver = value;
				if (m_receiver != null)
				{
					m_playerIcon.SetPlayer(m_receiver);
					UpdateReceiver();
					m_chatLog.Receiver = m_receiver;
				}
			}
		}
	}

	public bool IsWaitingOnMedal
	{
		get
		{
			if (Receiver == null)
			{
				return true;
			}
			MedalInfoTranslator mit = RankMgr.Get().GetRankedMedalFromRankPresenceField(Receiver.GetBestGameAccount());
			if (mit != null && mit.IsDisplayable())
			{
				if (!(m_selectableMedalWidget == null))
				{
					return m_selectableMedalWidget.IsChangingStates;
				}
				return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		InitPlayerIcon();
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
	}

	private void Start()
	{
		m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnSelectableMedalWidgetReady);
		UpdateLayout();
	}

	private void OnDestroy()
	{
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
	}

	public void UpdateLayout()
	{
		OnResize();
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (changelist.FindChange(m_receiver) != null)
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

	private void InitPlayerIcon()
	{
		m_playerIcon = UnityEngine.Object.Instantiate(m_Prefabs.m_PlayerIcon);
		m_playerIcon.transform.parent = base.transform;
		TransformUtil.CopyWorld(m_playerIcon, m_Bones.m_PlayerIcon);
		LayerUtils.SetLayer(m_playerIcon, base.gameObject.layer);
	}

	private void OnResize()
	{
		float viewMax = m_chatLog.messageFrames.ViewWindowMaxValue;
		m_chatLog.messageFrames.transform.position = (m_Bones.m_MessagesTopLeft.position + m_Bones.m_MessagesBottomRight.position) / 2f;
		Vector3 listClipSize3D = m_Bones.m_MessagesBottomRight.localPosition - m_Bones.m_MessagesTopLeft.localPosition;
		m_chatLog.messageFrames.ClipSize = new Vector2(listClipSize3D.x, Math.Abs(listClipSize3D.y));
		m_chatLog.messageFrames.ViewWindowMaxValue = viewMax;
		m_chatLog.messageFrames.ScrollValue = Mathf.Clamp01(m_chatLog.messageFrames.ScrollValue);
		m_chatLog.OnResize();
	}

	private void UpdateReceiver()
	{
		m_playerIcon.UpdateIcon();
		m_NameText.Text = FriendUtils.GetUniqueNameWithColor(m_receiver);
		MedalInfoTranslator mit = RankMgr.Get().GetRankedMedalFromRankPresenceField(m_receiver.GetBestGameAccount());
		if (m_receiver != null && m_receiver.IsDisplayable() && m_receiver.IsOnline())
		{
			if (mit == null || !mit.IsDisplayable())
			{
				m_playerIcon.Show();
			}
			else
			{
				m_playerIcon.Hide();
			}
		}
		else if (!m_receiver.IsOnline())
		{
			m_playerIcon.Show();
		}
		UpdateSelectableMedalWidget();
	}

	private void UpdateSelectableMedalWidget()
	{
		m_selectableMedal?.UpdateWidget(m_receiver, null, delegate
		{
			m_medalPatch.SetActive(value: true);
		}, delegate
		{
			m_medalPatch.SetActive(value: false);
		});
	}
}
