using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconParty : MonoBehaviour
{
	private class BaconPartyMemberInfo
	{
		public Status status;

		public BnetGameAccountId playerGameAccountId;

		public int slotId;

		public void Assign(BaconPartyMemberInfo other)
		{
			status = other.status;
			playerGameAccountId = other.playerGameAccountId;
			slotId = other.slotId;
		}
	}

	public enum Status
	{
		Inactive,
		Waiting,
		Ready,
		Leader,
		Leave,
		NotReady,
		Spectate,
		Vacant,
		Vacate
	}

	private class AnimatedEvent
	{
		public Event type;

		public BnetGameAccountId playerGameAccountId;

		public bool isReady;
	}

	private enum Event
	{
		Add,
		Remove,
		Ready
	}

	private static readonly PlatformDependentValue<string> PLATFORM_LOBBY_STATE = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "PC",
		MiniTablet = "PC",
		Tablet = "PC",
		Phone = "PHONE"
	};

	public const string ShowKickButtonEvent = "ShowKickButton";

	public const string HideKickButtonEvent = "HideKickButton";

	private const float SpectateEffectFxTime = 0.25f;

	private const string SCROLL_SECTION_MODE_STATE_SOLO = "SOLO";

	private const string SCROLL_SECTION_MODE_STATE_DUOS = "DUOS";

	public BaconDuosPartyPlanner m_partyPlanner;

	public AsyncReference m_PartyPanelReference;

	public AsyncReference m_Member0;

	public AsyncReference m_Member1;

	public AsyncReference m_Member2;

	public AsyncReference m_Member3;

	public AsyncReference m_Member4;

	public AsyncReference m_Member5;

	public AsyncReference m_Member6;

	public AsyncReference m_Member7;

	public GameObject m_ClickBlocker;

	public GameObject m_MaskObject;

	private Widget m_partyPanel;

	private List<Widget> m_members;

	private Dictionary<int, Widget> m_memberWidgetByWidgetIndex = new Dictionary<int, Widget>();

	private List<BaconPartyMemberInfo> m_memberInfo;

	private Queue<AnimatedEvent> m_animQueue;

	private List<BnetGameAccountId> m_animatingGameAccountIds;

	private bool m_animating;

	private bool m_panelLoaded;

	private int m_membersLoaded;

	private static BaconParty s_instance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public static BaconParty Get()
	{
		return s_instance;
	}

	public void Start()
	{
		s_instance = this;
		m_animQueue = new Queue<AnimatedEvent>();
		m_animatingGameAccountIds = new List<BnetGameAccountId>();
		m_members = new List<Widget>(PartyManager.BATTLEGROUNDS_PARTY_LIMIT);
		m_memberInfo = new List<BaconPartyMemberInfo>(PartyManager.BATTLEGROUNDS_PARTY_LIMIT);
		m_panelLoaded = false;
		m_membersLoaded = 0;
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		for (int i = 0; i < PartyManager.BATTLEGROUNDS_PARTY_LIMIT; i++)
		{
			m_members.Add(null);
			m_memberInfo.Add(new BaconPartyMemberInfo());
		}
		m_PartyPanelReference.RegisterReadyListener<Widget>(OnPartyPanelReady);
		m_Member0.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 0);
		});
		m_Member1.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 1);
		});
		m_Member2.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 2);
		});
		m_Member3.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 3);
		});
		m_Member4.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 4);
		});
		m_Member5.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 5);
		});
		m_Member6.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 6);
		});
		m_Member7.RegisterReadyListener(delegate(Widget c)
		{
			OnMemberReady(c, 7);
		});
		m_MaskObject.GetComponent<Maskable>().OverrideRenderPassEntryPoint(CustomViewEntryPoint.PerspectivePreFullscreenFX);
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		PartyManager.Get().AddPartyAttributeChangedListener(OnPartyAttributeChanged);
		PartyManager.Get().AddMemberAttributeChangedListener(OnMemberAttributeChange);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPresenceUpdated);
		BnetNearbyPlayerMgr.Get().AddChangeListener(OnNearbyPlayersUpdated);
		SpectatorManager.Get().OnSpectateRejected += OnSpectateRejected;
		BaconLobbyMgr.Get().AddBattlegroundsGameModeChangedListener(OnBattlegroundsGameModeChanged);
		StartCoroutine(ReconcileWhenReady());
	}

	public void OnDestroy()
	{
		if (PartyManager.Get() != null)
		{
			PartyManager.Get().RemoveChangedListener(OnPartyChanged);
			PartyManager.Get().RemoveMemberAttributeChangedListener(OnMemberAttributeChange);
			PartyManager.Get().RemovePartyAttributeChangedListener(OnPartyAttributeChanged);
		}
		if (BnetPresenceMgr.Get() != null)
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPresenceUpdated);
		}
		if (BnetNearbyPlayerMgr.Get() != null)
		{
			BnetNearbyPlayerMgr.Get().RemoveChangeListener(OnNearbyPlayersUpdated);
		}
		if (SpectatorManager.Get() != null)
		{
			SpectatorManager.Get().OnSpectateRejected -= OnSpectateRejected;
		}
		if (BaconLobbyMgr.Get() != null)
		{
			BaconLobbyMgr.Get().RemoveBattlegroundsGameModeChangedListener(OnBattlegroundsGameModeChanged);
		}
	}

	private bool IsLoadedAndReady()
	{
		if (m_panelLoaded)
		{
			return m_membersLoaded == PartyManager.BATTLEGROUNDS_PARTY_LIMIT;
		}
		return false;
	}

	private IEnumerator ReconcileWhenReady()
	{
		while (!IsLoadedAndReady())
		{
			yield return new WaitForEndOfFrame();
		}
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			while (PartyManager.Get().GetCurrentAndPendingPartyMembers().Count == 0)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		if (PartyManager.Get().GetCurrentPartySize() == 1)
		{
			LeaveParty();
		}
		RefreshDisplay();
		UpdateDataModelData();
		FriendChallengeMgr.Get().UpdateMyAvailability();
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData data, object userData)
	{
		Log.Party.PrintDebug("BaconParty.OnPartyChanged(): Event={0}, gameAccountId={1}", inviteEvent, playerGameAccountId);
		switch (inviteEvent)
		{
		case PartyManager.PartyInviteEvent.I_SENT_INVITE:
		case PartyManager.PartyInviteEvent.FRIEND_RECEIVED_INVITE:
			AddPartyMember(playerGameAccountId);
			break;
		case PartyManager.PartyInviteEvent.I_CREATED_PARTY:
			RefreshDisplay();
			break;
		case PartyManager.PartyInviteEvent.FRIEND_ACCEPTED_INVITE:
			SetReady(playerGameAccountId);
			break;
		case PartyManager.PartyInviteEvent.I_RESCINDED_INVITE:
		case PartyManager.PartyInviteEvent.FRIEND_DECLINED_INVITE:
		case PartyManager.PartyInviteEvent.INVITE_EXPIRED:
		case PartyManager.PartyInviteEvent.FRIEND_LEFT:
			RemovePartyMember(playerGameAccountId);
			break;
		case PartyManager.PartyInviteEvent.I_ACCEPTED_INVITE:
			StartCoroutine(ReconcileWhenReady());
			break;
		case PartyManager.PartyInviteEvent.LEADER_DISSOLVED_PARTY:
			RefreshDisplay();
			break;
		}
		UpdateDataModelData();
	}

	private void OnPartyAttributeChanged(Attribute attribute, object userData)
	{
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			if (attribute.Name == "battlegrounds_private")
			{
				UpdateDataModelData();
				RefreshVisuals();
			}
			else if (attribute.Name == "battlegrounds_mode")
			{
				PartyManager.Get().UpdateMyBattlegroundsModeRating();
			}
		}
	}

	private void OnMemberAttributeChange(BnetGameAccountId playerGameAccountId, Attribute attribute, object userData)
	{
		if (!PartyManager.Get().IsPlayerInCurrentParty(playerGameAccountId))
		{
			return;
		}
		BaconPartyMemberInfo memberInfo = m_memberInfo.Find((BaconPartyMemberInfo Info) => Info != null && Info.playerGameAccountId == playerGameAccountId);
		if (attribute.Name == "battlegrounds_duos_team_slot")
		{
			if (memberInfo != null)
			{
				memberInfo.slotId = (int)attribute.Value.IntValue;
				ReorderMemberInfoForMemberDisplay(memberInfo);
			}
			RefreshVisuals();
			return;
		}
		if (attribute.Name == "battlegrounds_mode_rating")
		{
			if (m_members != null)
			{
				Widget widget = m_members.Find((Widget w) => w != null && w.GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>()?.GameAccountId?.High == playerGameAccountId.High && w.GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>()?.GameAccountId?.Low == playerGameAccountId.Low);
				if (widget != null)
				{
					widget.GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>().BattlegroundsDuosRating = (int)attribute.Value.IntValue;
				}
			}
			m_partyPlanner.UpdateMemberRating(playerGameAccountId, (int)attribute.Value.IntValue);
		}
		if (memberInfo == null)
		{
			return;
		}
		Status readyStatus = GetReadyStatusForPartyMember(playerGameAccountId);
		if (memberInfo.status != Status.Waiting || readyStatus != Status.NotReady)
		{
			bool num = readyStatus != memberInfo.status;
			memberInfo.status = readyStatus;
			if (num && PartyManager.Get().IsPartyLeader())
			{
				RefreshVisuals();
			}
		}
	}

	private void CollapseDuosTeams()
	{
		if (!PartyManager.Get().IsInBattlegroundsParty() || !PartyManager.Get().IsPartyLeader())
		{
			return;
		}
		if (BaconLobbyMgr.Get().IsInDuosMode())
		{
			for (int i = 2; i < m_memberInfo.Count - 3; i += 2)
			{
				bool num = m_memberInfo[i].slotId == 0 && m_memberInfo[i + 1].slotId == 0;
				bool nextTeamIsEmpty = m_memberInfo[i + 2].slotId == 0 && m_memberInfo[i + 3].slotId == 0;
				if (num && !nextTeamIsEmpty)
				{
					m_memberInfo[i].Assign(m_memberInfo[i + 2]);
					m_memberInfo[i].slotId = i + 1;
					m_memberInfo[i + 1].Assign(m_memberInfo[i + 3]);
					m_memberInfo[i + 1].slotId = i + 2;
					m_memberInfo[i + 2].status = Status.Inactive;
					m_memberInfo[i + 2].playerGameAccountId = null;
					m_memberInfo[i + 2].slotId = 0;
					m_memberInfo[i + 3].status = Status.Inactive;
					m_memberInfo[i + 3].playerGameAccountId = null;
					m_memberInfo[i + 3].slotId = 0;
				}
			}
			return;
		}
		for (int j = 1; j < m_memberInfo.Count - 1; j++)
		{
			bool num2 = m_memberInfo[j].playerGameAccountId == null;
			bool nextTeamIsEmpty2 = m_memberInfo[j + 1].playerGameAccountId == null;
			if (num2 && !nextTeamIsEmpty2)
			{
				m_memberInfo[j].Assign(m_memberInfo[j + 1]);
				m_memberInfo[j].slotId = j + 1;
				m_memberInfo[j + 1].status = Status.Inactive;
				m_memberInfo[j + 1].playerGameAccountId = null;
				m_memberInfo[j + 1].slotId = 0;
			}
		}
	}

	private void ReorderMemberInfoForMemberDisplay(BaconPartyMemberInfo updatedMemberInfo = null)
	{
		if (PartyManager.Get().IsPartyLeader())
		{
			return;
		}
		BaconPartyMemberInfo[] temp = new BaconPartyMemberInfo[m_memberInfo.Count];
		BattlegroundsDuosTeamBuilderPlayerDataModel[] tempDataModels = new BattlegroundsDuosTeamBuilderPlayerDataModel[m_memberInfo.Count];
		List<BaconPartyMemberInfo> unordered = new List<BaconPartyMemberInfo>();
		List<BattlegroundsDuosTeamBuilderPlayerDataModel> unorderedDataModels = new List<BattlegroundsDuosTeamBuilderPlayerDataModel>();
		for (int i = 0; i < m_memberInfo.Count; i++)
		{
			if (m_memberInfo[i].slotId == 0 || m_memberInfo[i].playerGameAccountId == null || (updatedMemberInfo != null && m_memberInfo[i].slotId == updatedMemberInfo.slotId && m_memberInfo[i].playerGameAccountId != updatedMemberInfo.playerGameAccountId))
			{
				BaconPartyMemberInfo info = new BaconPartyMemberInfo();
				info.Assign(m_memberInfo[i]);
				unordered.Add(info);
				unorderedDataModels.Add(m_members[i].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>());
				m_memberInfo[i].slotId = 0;
			}
			else
			{
				temp[m_memberInfo[i].slotId - 1] = new BaconPartyMemberInfo();
				temp[m_memberInfo[i].slotId - 1].Assign(m_memberInfo[i]);
				tempDataModels[m_memberInfo[i].slotId - 1] = m_members[i].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			}
		}
		for (int j = 0; j < m_memberInfo.Count; j++)
		{
			if (temp[j] != null)
			{
				m_memberInfo[j].Assign(temp[j]);
				m_members[j].UnbindDataModel(937);
				if (tempDataModels[j] != null)
				{
					m_members[j].BindDataModel(tempDataModels[j]);
				}
			}
		}
		int memberInfoStartIndex = 0;
		for (int k = 0; k < unordered.Count; k++)
		{
			for (int l = memberInfoStartIndex; l < m_memberInfo.Count; l++)
			{
				if (temp[l] == null)
				{
					m_memberInfo[l].Assign(unordered[k]);
					m_memberInfo[l].slotId = l + 1;
					m_members[l].UnbindDataModel(937);
					if (unorderedDataModels[k] != null)
					{
						m_members[l].BindDataModel(unorderedDataModels[k]);
					}
					memberInfoStartIndex = l + 1;
					break;
				}
			}
		}
	}

	private void UpdateDuosPartyVisuals()
	{
		if (PartyManager.Get().IsPartyLeader())
		{
			int partySize = PartyManager.Get().GetCurrentPartySize();
			if (PartyManager.Get().IsInPrivateBattlegroundsParty())
			{
				if (partySize <= PartyManager.Get().GetBattlegroundsMaxRankedPartySize())
				{
					PartyManager.Get().SetBattlegroundsPrivateParty(isPrivate: false);
				}
			}
			else if (partySize > PartyManager.Get().GetBattlegroundsMaxRankedPartySize())
			{
				PartyManager.Get().SetBattlegroundsPrivateParty(isPrivate: true);
			}
		}
		base.transform.Find("ScrollingSection").GetComponent<VisualController>().SetState(BaconLobbyMgr.Get().IsInDuosMode() ? "DUOS" : "SOLO");
		if (!BaconLobbyMgr.Get().IsInDuosMode() || !PartyManager.Get().IsPartyLeader())
		{
			m_partyPlanner.ToggleOpenState(isOpen: false);
		}
		else
		{
			m_partyPlanner.ToggleOpenState(PartyManager.Get().GetCurrentAndPendingPartyMembers().Count >= 3 || m_partyPlanner.IsOpen());
		}
	}

	private void OnBattlegroundsGameModeChanged(string gameMode, bool showPartyUi, object userData)
	{
		UpdateDuosPartyVisuals();
		RefreshVisuals();
	}

	private void OnPresenceUpdated(BnetPlayerChangelist changelist, object userData)
	{
		List<BnetPlayer> changedPlayers = new List<BnetPlayer>();
		foreach (BnetPlayerChange change in changelist.GetChanges())
		{
			BnetPlayer changedPlayer = change.GetPlayer();
			changedPlayers.Add(changedPlayer);
		}
		UpdateChangedPlayersFromPresenceUpdate(changedPlayers);
	}

	private void OnNearbyPlayersUpdated(BnetRecentOrNearbyPlayerChangelist changelist, object userData)
	{
		UpdateChangedPlayersFromPresenceUpdate(changelist.GetUpdatedStrangers());
	}

	private void UpdateChangedPlayersFromPresenceUpdate(List<BnetPlayer> changedPlayers)
	{
		if (changedPlayers == null)
		{
			return;
		}
		bool partyMemberChanged = false;
		foreach (BnetPlayer changedPlayer in changedPlayers)
		{
			if (changedPlayer == BnetPresenceMgr.Get().GetMyPlayer() && changedPlayer.IsAppearingOffline())
			{
				LeaveParty();
				UpdateDataModelData();
				return;
			}
			if (!PartyManager.Get().IsPlayerInCurrentPartyOrPending(changedPlayer.GetBestGameAccountId()))
			{
				continue;
			}
			for (int i = 0; i < m_memberInfo.Count; i++)
			{
				if (m_memberInfo[i].playerGameAccountId == changedPlayer.GetBestGameAccountId())
				{
					m_memberInfo[i].status = GetReadyStatusForPartyMember(m_memberInfo[i].playerGameAccountId);
					partyMemberChanged = true;
					break;
				}
			}
		}
		if (partyMemberChanged)
		{
			RefreshVisuals();
		}
	}

	private void OnSpectateRejected()
	{
		CleanUpSpectateFx();
	}

	private void OnPartyPanelReady(Widget controller)
	{
		m_partyPanel = controller;
		m_panelLoaded = true;
		m_partyPanel.GetComponent<VisualController>().SetState(PLATFORM_LOBBY_STATE);
		m_partyPanel.RegisterEventListener(OnWidgetEventReceived);
	}

	private void OnWidgetEventReceived(string eventName)
	{
		if (eventName == "PARTYPLANNER_toggle")
		{
			m_partyPlanner.ToggleOpenState();
		}
	}

	private void OnMemberReady(Widget widget, int index)
	{
		m_memberWidgetByWidgetIndex.Add(index, widget);
		m_members[index] = widget;
		widget.RegisterEventListener(delegate(string s)
		{
			OnPartyMemberEvent(index, s);
		});
		m_membersLoaded++;
	}

	private void OnPartyMemberEvent(int index, string eventString)
	{
		if (!m_memberWidgetByWidgetIndex.ContainsKey(index))
		{
			Debug.LogErrorFormat("OnPartyMemberEvent() - No party member widget at index {0}", index);
		}
		else if (eventString == "SPECTATE_BUTTON_PRESSED")
		{
			Widget widget = m_memberWidgetByWidgetIndex[index];
			int widgetIndexInPartyMemberList = m_members.IndexOf(widget);
			if (widgetIndexInPartyMemberList == -1)
			{
				Debug.LogErrorFormat("OnPartyMemberEvent() - Widget at index {0} not found in m_members list.", index);
			}
			else
			{
				m_ClickBlocker.SetActive(value: true);
				BnetGameAccountId gameAccountId = m_memberInfo[widgetIndexInPartyMemberList].playerGameAccountId;
				StartCoroutine(SpectatePlayerWithAnimations(gameAccountId));
			}
		}
		else if (eventString == "KICK_BUTTON_PRESSED")
		{
			Widget widget2 = m_memberWidgetByWidgetIndex[index];
			int widgetIndexInPartyMemberList2 = m_members.IndexOf(widget2);
			if (widgetIndexInPartyMemberList2 == -1)
			{
				Debug.LogErrorFormat("OnPartyMemberEvent() - Widget at index {0} not found in m_members list.", index);
			}
			else
			{
				BnetGameAccountId gameAccountId2 = m_memberInfo[widgetIndexInPartyMemberList2].playerGameAccountId;
				PartyManager.Get().KickPlayerFromParty(gameAccountId2);
			}
		}
	}

	public BaconPartyDataModel GetBaconPartyDataModel()
	{
		if (!m_partyPanel.GetDataModel(154, out var dataModel))
		{
			dataModel = new BaconPartyDataModel();
			m_partyPanel.BindDataModel(dataModel);
		}
		return dataModel as BaconPartyDataModel;
	}

	private void RefreshDisplay()
	{
		if (!PartyManager.Get().IsInBattlegroundsParty())
		{
			UpdateDuosPartyVisuals();
			m_partyPlanner.ClearPartyData();
			return;
		}
		BnetGameAccountId leaderGameAccountId = PartyManager.Get().GetLeader();
		BnetPresenceMgr.Get().GetMyPlayer().GetBestGameAccountId();
		List<BnetGameAccountId> partyMembers = PartyManager.Get().GetCurrentAndPendingPartyMembers();
		partyMembers = new List<BnetGameAccountId>(new HashSet<BnetGameAccountId>(partyMembers));
		if (!PartyManager.Get().IsPartyLeader())
		{
			partyMembers.Sort(delegate(BnetGameAccountId m1, BnetGameAccountId m2)
			{
				if (m1 == leaderGameAccountId)
				{
					return -1;
				}
				return (m2 == leaderGameAccountId) ? 1 : PartyManager.Get().GetPlayerDuosSlot(m1).CompareTo(PartyManager.Get().GetPlayerDuosSlot(m2));
			});
		}
		for (int i = 0; i < PartyManager.BATTLEGROUNDS_PARTY_LIMIT; i++)
		{
			BnetGameAccountId gameAccountIdAtIndex = null;
			if (i < partyMembers.Count)
			{
				gameAccountIdAtIndex = partyMembers[i];
			}
			if (PartyManager.Get().IsPlayerInCurrentParty(gameAccountIdAtIndex))
			{
				m_memberInfo[i].status = GetReadyStatusForPartyMember(gameAccountIdAtIndex);
				m_memberInfo[i].playerGameAccountId = gameAccountIdAtIndex;
				m_memberInfo[i].slotId = PartyManager.Get().GetPlayerDuosSlot(gameAccountIdAtIndex);
				if (i > 0)
				{
					BattlegroundsDuosTeamBuilderPlayerDataModel model = new BattlegroundsDuosTeamBuilderPlayerDataModel
					{
						GameAccountId = new GameAccountIdDataModel
						{
							High = gameAccountIdAtIndex.High,
							Low = gameAccountIdAtIndex.Low
						},
						DisplayName = PartyManager.Get().GetPartyMemberName(gameAccountIdAtIndex),
						BattlegroundsDuosRating = PartyManager.Get().GetBattlegroundsPartyMemberRating(gameAccountIdAtIndex)
					};
					m_members[i].BindDataModel(model);
					m_partyPlanner.AddMember(model);
				}
			}
			else if (gameAccountIdAtIndex != null)
			{
				m_memberInfo[i].status = Status.Waiting;
				m_memberInfo[i].playerGameAccountId = gameAccountIdAtIndex;
				m_memberInfo[i].slotId = PartyManager.Get().GetPlayerDuosSlot(gameAccountIdAtIndex);
				if (i > 0)
				{
					BattlegroundsDuosTeamBuilderPlayerDataModel model2 = new BattlegroundsDuosTeamBuilderPlayerDataModel
					{
						GameAccountId = new GameAccountIdDataModel
						{
							High = gameAccountIdAtIndex.High,
							Low = gameAccountIdAtIndex.Low
						},
						DisplayName = PartyManager.Get().GetPartyMemberName(gameAccountIdAtIndex),
						BattlegroundsDuosRating = PartyManager.Get().GetBattlegroundsPartyMemberRating(gameAccountIdAtIndex)
					};
					m_members[i].BindDataModel(model2);
					m_partyPlanner.AddMember(model2);
				}
			}
			else
			{
				m_memberInfo[i].status = ((m_memberInfo[i].status == Status.Vacant) ? Status.Vacant : Status.Inactive);
				m_memberInfo[i].playerGameAccountId = null;
				m_memberInfo[i].slotId = 0;
			}
		}
		UpdateDuosPartyVisuals();
		if (!PartyManager.Get().IsPartyLeader() && BaconLobbyMgr.Get().IsInDuosMode())
		{
			ReorderMemberInfoForMemberDisplay();
		}
		RefreshVisuals();
	}

	public void ClearPartyData()
	{
		m_partyPlanner.ClearPartyData();
		foreach (Widget member in m_members)
		{
			member.UnbindDataModel(937);
		}
	}

	private void UpdateDataModelData()
	{
		if (IsLoadedAndReady())
		{
			BaconPartyDataModel baconPartyDataModel = GetBaconPartyDataModel();
			baconPartyDataModel.Active = PartyManager.Get().IsInBattlegroundsParty();
			baconPartyDataModel.Size = PartyManager.Get().GetCurrentPartySize();
			baconPartyDataModel.PrivateGame = PartyManager.Get().IsInPrivateBattlegroundsParty();
		}
	}

	public void LeaveParty()
	{
		PartyManager.Get().LeaveParty();
		UpdateDuosPartyVisuals();
	}

	private void AddPartyMember(BnetGameAccountId playerGameAccountId, bool isReady = false)
	{
		if (PartyManager.Get().GetCurrentPartySize() <= PartyManager.BATTLEGROUNDS_PARTY_LIMIT && !m_animatingGameAccountIds.Contains(playerGameAccountId) && m_memberInfo.Find((BaconPartyMemberInfo info) => info.playerGameAccountId == playerGameAccountId) == null)
		{
			AnimatedEvent e = new AnimatedEvent();
			e.type = Event.Add;
			e.playerGameAccountId = playerGameAccountId;
			e.isReady = isReady;
			m_animQueue.Enqueue(e);
			m_animatingGameAccountIds.Add(playerGameAccountId);
			Animate();
		}
	}

	private void Animate()
	{
		if (!m_animating && m_animQueue.Count != 0)
		{
			AnimatedEvent e = m_animQueue.Dequeue();
			m_animatingGameAccountIds.Remove(e.playerGameAccountId);
			switch (e.type)
			{
			case Event.Add:
				StartCoroutine(AddPartyMemberWithAnims(e.playerGameAccountId, e.isReady));
				break;
			case Event.Remove:
				StartCoroutine(RemovePartyMemberWithAnims(e.playerGameAccountId));
				break;
			}
		}
	}

	private IEnumerator AddPartyMemberWithAnims(BnetGameAccountId playerGameAccountId, bool isReady)
	{
		m_animating = true;
		while (!IsLoadedAndReady())
		{
			yield return null;
		}
		int newMemberIndex = -1;
		for (int i = 0; i < m_memberInfo.Count; i++)
		{
			if (m_memberInfo[i].status == Status.Inactive || m_memberInfo[i].status == Status.Vacant)
			{
				newMemberIndex = i;
				break;
			}
		}
		if (newMemberIndex == -1)
		{
			Log.Party.PrintError("AddPartyMemberWithAnims - No inactive members, unable to add new member.");
			m_animating = false;
			Animate();
			yield break;
		}
		m_memberInfo[newMemberIndex].status = ((!isReady) ? Status.Waiting : GetReadyStatusForPartyMember(playerGameAccountId));
		m_memberInfo[newMemberIndex].playerGameAccountId = playerGameAccountId;
		m_memberInfo[newMemberIndex].slotId = PartyManager.Get().GetPlayerDuosSlot(playerGameAccountId);
		BattlegroundsDuosTeamBuilderPlayerDataModel model = new BattlegroundsDuosTeamBuilderPlayerDataModel
		{
			GameAccountId = new GameAccountIdDataModel
			{
				High = playerGameAccountId.High,
				Low = playerGameAccountId.Low
			},
			DisplayName = PartyManager.Get().GetPartyMemberName(playerGameAccountId)
		};
		m_members[newMemberIndex].BindDataModel(model);
		m_partyPlanner.AddMember(model);
		UpdateDuosPartyVisuals();
		if (BaconLobbyMgr.Get().IsInDuosMode() && m_memberInfo[GetDuosPairedSlot(newMemberIndex)].playerGameAccountId == null)
		{
			m_memberInfo[GetDuosPairedSlot(newMemberIndex)].status = Status.Vacant;
		}
		RefreshVisuals();
		yield return new WaitForSeconds(0.5f);
		m_animating = false;
		Animate();
	}

	private void RemovePartyMember(BnetGameAccountId playerGameAccountId)
	{
		if (PartyManager.Get().GetCurrentPartySize() == 1)
		{
			LeaveParty();
			return;
		}
		m_animQueue.Enqueue(new AnimatedEvent
		{
			type = Event.Remove,
			playerGameAccountId = playerGameAccountId
		});
		Animate();
	}

	private int GetDuosPairedSlot(int index)
	{
		if (index % 2 == 0)
		{
			return index + 1;
		}
		return index - 1;
	}

	private bool ShouldSlotStayVacant(int index)
	{
		if (!BaconLobbyMgr.Get().IsInDuosMode())
		{
			return false;
		}
		int teamSlot = GetDuosPairedSlot(index);
		if (m_memberInfo[teamSlot] != null)
		{
			return m_memberInfo[teamSlot].playerGameAccountId != null;
		}
		return false;
	}

	private IEnumerator RemovePartyMemberWithAnims(BnetGameAccountId playerGameAccountId)
	{
		m_animating = true;
		while (!IsLoadedAndReady())
		{
			yield return null;
		}
		int index = GetIndexOfPartyMemberFromGameAccountId(playerGameAccountId);
		if (index < 0 || index >= PartyManager.BATTLEGROUNDS_PARTY_LIMIT)
		{
			Log.Party.PrintError("RemovePartyMemberWithAnims() - Unable to find party member with id {0}.", playerGameAccountId);
			m_animating = false;
			Animate();
			yield break;
		}
		if (ShouldSlotStayVacant(index))
		{
			m_members[index].TriggerEvent(Status.Vacate.ToString());
		}
		else if (BaconLobbyMgr.Get().IsInDuosMode())
		{
			m_members[GetDuosPairedSlot(index)].TriggerEvent(Status.Leave.ToString());
			m_memberInfo[GetDuosPairedSlot(index)].status = Status.Inactive;
		}
		yield return new WaitForSeconds(0.5f);
		m_memberInfo[index] = new BaconPartyMemberInfo();
		m_memberInfo[index].status = (ShouldSlotStayVacant(index) ? Status.Vacant : Status.Inactive);
		m_memberInfo[index].slotId = 0;
		m_partyPlanner.RemoveMember(playerGameAccountId);
		UpdateDuosPartyVisuals();
		CollapseDuosTeams();
		RefreshVisuals();
		m_animating = false;
		Animate();
	}

	private int GetIndexOfPartyMemberFromGameAccountId(BnetGameAccountId playerGameAccountId)
	{
		for (int i = 0; i < PartyManager.BATTLEGROUNDS_PARTY_LIMIT; i++)
		{
			if (m_memberInfo[i] != null && m_memberInfo[i].playerGameAccountId == playerGameAccountId)
			{
				return i;
			}
		}
		return -1;
	}

	private void SetReady(BnetGameAccountId playerGameAccountId)
	{
		int memberIndex = -1;
		for (int i = 0; i < m_memberInfo.Count; i++)
		{
			if (m_memberInfo[i].playerGameAccountId == playerGameAccountId)
			{
				memberIndex = i;
				break;
			}
		}
		if (memberIndex == -1)
		{
			if (IsLoadedAndReady())
			{
				AddPartyMember(playerGameAccountId, isReady: true);
			}
		}
		else if (memberIndex > 0 && memberIndex < PartyManager.BATTLEGROUNDS_PARTY_LIMIT)
		{
			m_memberInfo[memberIndex].status = Status.Ready;
			m_members[memberIndex].TriggerEvent(Status.Ready.ToString());
		}
	}

	private void RefreshVisuals()
	{
		bool isDuos = BaconLobbyMgr.Get().IsInDuosMode();
		for (int i = 0; i < m_members.Count; i++)
		{
			m_members[i].TriggerEvent(m_memberInfo[i].status.ToString());
			if (m_memberInfo[i].playerGameAccountId != null)
			{
				string playerName = PartyManager.Get().GetPartyMemberName(m_memberInfo[i].playerGameAccountId);
				if (isDuos && PartyManager.Get().IsPartyLeader() && PartyManager.Get().GetPlayerDuosSlot(m_memberInfo[i].playerGameAccountId) != i + 1)
				{
					PartyManager.Get().SetSelectedBattlegroundsDuosTeamSlotId(i + 1, m_memberInfo[i].playerGameAccountId);
				}
				m_members[i].transform.Find("BaconPartyMember/Root/Name").gameObject.GetComponent<UberText>().Text = playerName;
				m_members[i].transform.Find("BaconPartyMember/Root/Divider").gameObject.GetComponent<VisualController>().SetState(((i % 2 == 0 && isDuos) || i == m_memberInfo.Count - 1) ? "ODD" : "DEFAULT");
				if (PartyManager.Get().IsPartyLeader() && BattleNet.GetMyGameAccountId() != m_memberInfo[i].playerGameAccountId)
				{
					m_members[i].TriggerEvent("ShowKickButton");
				}
				else
				{
					m_members[i].TriggerEvent("HideKickButton");
				}
			}
			else
			{
				m_members[i].TriggerEvent("HideKickButton");
			}
		}
	}

	public static Status GetReadyStatusForPartyMember(BnetGameAccountId playerGameAccountId)
	{
		BnetPlayer player = BnetUtils.GetPlayer(playerGameAccountId);
		bool hasAvailabilityForPlayer = BnetFriendMgr.Get().IsFriend(player) || BnetNearbyPlayerMgr.Get().IsNearbyPlayer(player);
		if (playerGameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId())
		{
			if (!PartyManager.Get().IsPartyLeader())
			{
				return Status.Ready;
			}
			return Status.Leader;
		}
		if (PartyManager.Get().IsPlayerPendingInCurrentParty(playerGameAccountId))
		{
			return Status.Waiting;
		}
		if (!PartyManager.Get().IsPlayerInCurrentParty(playerGameAccountId))
		{
			return Status.NotReady;
		}
		if (PartyManager.Get().CanSpectatePartyMember(playerGameAccountId))
		{
			return Status.Spectate;
		}
		if (PartyManager.Get().GetLeader() == playerGameAccountId)
		{
			if (player != null && !FriendChallengeMgr.Get().IsOpponentAvailable(player))
			{
				return Status.NotReady;
			}
			return Status.Leader;
		}
		if (player == null || !hasAvailabilityForPlayer)
		{
			return Status.Ready;
		}
		if (FriendChallengeMgr.Get().IsOpponentAvailable(player))
		{
			return Status.Ready;
		}
		if (Network.Get().IsFindingGame())
		{
			return Status.Ready;
		}
		return Status.NotReady;
	}

	private IEnumerator SpectatePlayerWithAnimations(BnetGameAccountId playerGameAccountId)
	{
		DesaturateParameters? desaturate = new DesaturateParameters(0.5f);
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, 0.25f, iTween.EaseType.linear, null, null, desaturate, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		yield return new WaitForSeconds(0.25f);
		if (!PartyManager.Get().SpectatePartyMember(playerGameAccountId))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_BACON_PARTY_SPECTATE_ERROR_HEADER");
			info.m_text = GameStrings.Get("GLUE_BACON_PARTY_SPECTATE_ERROR_BODY");
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_showAlertIcon = false;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info);
			CleanUpSpectateFx();
		}
		m_ClickBlocker.SetActive(value: false);
	}

	private void CleanUpSpectateFx()
	{
		m_screenEffectsHandle.StopEffect();
	}

	public void MovePlayerToSlot(BattlegroundsDuosTeamBuilderPlayerDataModel sourceDataModel, Widget destinationWidget)
	{
		Widget sourceWidget = GetWidgetForModel(sourceDataModel);
		if (!(sourceWidget == null))
		{
			int sourceMemberInfoIndex = GetMemberInfoIndexForDataModel(sourceDataModel);
			int destinationMemberInfoIndex = destinationWidget.transform.GetSiblingIndex();
			BattlegroundsDuosTeamBuilderPlayerDataModel destinationDataModel = destinationWidget.GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			sourceWidget.UnbindDataModel(937);
			BaconPartyMemberInfo sourceInfo = m_memberInfo[sourceMemberInfoIndex];
			if (destinationDataModel != null)
			{
				destinationWidget.UnbindDataModel(937);
				sourceWidget.BindDataModel(destinationDataModel);
				m_memberInfo[sourceMemberInfoIndex] = m_memberInfo[destinationMemberInfoIndex];
			}
			else
			{
				m_memberInfo[sourceMemberInfoIndex] = new BaconPartyMemberInfo();
				m_memberInfo[sourceMemberInfoIndex].status = (ShouldSlotStayVacant(sourceMemberInfoIndex) ? Status.Vacant : Status.Inactive);
				m_memberInfo[sourceMemberInfoIndex].slotId = 0;
				m_memberInfo[sourceMemberInfoIndex].playerGameAccountId = null;
			}
			m_memberInfo[destinationMemberInfoIndex] = sourceInfo;
			destinationWidget.BindDataModel(sourceDataModel);
			sourceWidget.transform.Find("BaconPartyMember").GetComponent<VisualController>().SetState(BaconDuosPartyTray.COMPLETION_CONFIRMATION_STATE);
			destinationWidget.transform.Find("BaconPartyMember").GetComponent<VisualController>().SetState(BaconDuosPartyTray.COMPLETION_CONFIRMATION_STATE);
			CollapseDuosTeams();
			RefreshVisuals();
		}
	}

	private int GetMemberInfoIndexForDataModel(BattlegroundsDuosTeamBuilderPlayerDataModel dataModel)
	{
		for (int i = 0; i < m_memberInfo.Count; i++)
		{
			BaconPartyMemberInfo memberInfo = m_memberInfo[i];
			if (memberInfo != null && !(memberInfo.playerGameAccountId == null) && memberInfo.playerGameAccountId.High == dataModel.GameAccountId.High && memberInfo.playerGameAccountId.Low == dataModel.GameAccountId.Low)
			{
				return i;
			}
		}
		return -1;
	}

	private Widget GetWidgetForModel(BattlegroundsDuosTeamBuilderPlayerDataModel dataModel)
	{
		for (int i = 0; i < m_members.Count; i++)
		{
			BattlegroundsDuosTeamBuilderPlayerDataModel widgetDataModel = m_members[i].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			if (widgetDataModel != null && widgetDataModel.GameAccountId.High == dataModel.GameAccountId.High && widgetDataModel.GameAccountId.Low == dataModel.GameAccountId.Low)
			{
				return m_members[i];
			}
		}
		return null;
	}

	public bool IsValidDropTarget(int index)
	{
		int totalMembers = PartyManager.Get().GetCurrentAndPendingPartyMembers().Count;
		int maxValidSlot = ((totalMembers % 2 == 1) ? (totalMembers + 1) : totalMembers);
		return index < maxValidSlot;
	}

	public void EnableTrayMask()
	{
		if (m_MaskObject != null)
		{
			m_MaskObject.GetComponent<Maskable>().enabled = true;
		}
	}
}
