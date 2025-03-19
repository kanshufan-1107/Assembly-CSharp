using System;
using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core;
using PegasusGame;
using Unity.Profiling;
using UnityEngine;

public class ZoneMgr : MonoBehaviour
{
	public delegate void ChangeCompleteCallback(ZoneChangeList changeList, object userData);

	private class TempZone
	{
		private Zone m_zone;

		private bool m_modified;

		private List<Entity> m_prevEntities = new List<Entity>();

		private List<Entity> m_entities = new List<Entity>();

		private Map<int, int> m_replacedEntities = new Map<int, int>();

		public Zone GetZone()
		{
			return m_zone;
		}

		public void SetZone(Zone zone)
		{
			m_zone = zone;
		}

		public bool IsModified()
		{
			return m_modified;
		}

		public List<Entity> GetEntities()
		{
			return m_entities;
		}

		public Entity GetEntityAtSlot(int slot)
		{
			int currentSlot = 1;
			for (int i = 0; i < m_entities.Count; i++)
			{
				Entity currentEntity = m_entities[i];
				if (currentEntity != null)
				{
					if (currentSlot == slot)
					{
						return currentEntity;
					}
					currentSlot++;
				}
			}
			return null;
		}

		public void AddInitialEntity(Entity entity)
		{
			m_entities.Add(entity);
		}

		public bool CanAcceptEntity(Entity entity)
		{
			return Get().FindZoneForEntityAndZoneTag(entity, m_zone.m_ServerTag) == m_zone;
		}

		public void AddEntity(Entity entity)
		{
			if (CanAcceptEntity(entity) && !m_entities.Contains(entity))
			{
				m_entities.Add(entity);
				m_modified = true;
			}
		}

		public void InsertEntityAtIndex(int index, Entity entity, bool bypassCanAcceptEntityCheck = false)
		{
			if ((bypassCanAcceptEntityCheck || CanAcceptEntity(entity)) && index >= 0 && index <= m_entities.Count && (index >= m_entities.Count || m_entities[index] != entity))
			{
				m_entities.Insert(index, entity);
				m_modified = true;
			}
		}

		public void InsertEntityAtSlot(int slot, Entity entity, bool bypassCanAcceptEntityCheck = false)
		{
			int index;
			for (index = 0; index < m_entities.Count; index++)
			{
				int currentSlot = GetSlotOfEntitAtIndex(index);
				if (slot <= currentSlot)
				{
					break;
				}
			}
			InsertEntityAtIndex(index, entity, bypassCanAcceptEntityCheck);
		}

		public bool RemoveEntity(Entity entity)
		{
			if (!m_entities.Remove(entity))
			{
				return false;
			}
			m_modified = true;
			return true;
		}

		public bool RemoveEntityById(int entityId)
		{
			Entity entityToRemove = null;
			foreach (Entity entity in m_entities)
			{
				if (entity.GetEntityId() == entityId)
				{
					entityToRemove = entity;
					break;
				}
			}
			if (entityToRemove == null)
			{
				return false;
			}
			m_entities.Remove(entityToRemove);
			m_modified = true;
			return true;
		}

		public int GetLastSlot()
		{
			return m_entities.Count;
		}

		public int FindEntityPos(int entityId)
		{
			return 1 + m_entities.FindIndex((Entity currEntity) => currEntity.GetEntityId() == entityId);
		}

		public bool ContainsEntity(int entityId)
		{
			return FindEntityPos(entityId) > 0;
		}

		public int FindEntityPosWithReplacements(int entityId)
		{
			while (entityId != 0)
			{
				int index = m_entities.FindIndex((Entity currEntity) => currEntity.GetEntityId() == entityId);
				int pos = 0;
				if (index >= 0 && index < m_entities.Count)
				{
					pos = GetSlotOfEntitAtIndex(index);
				}
				if (pos > 0)
				{
					return pos;
				}
				m_replacedEntities.TryGetValue(entityId, out entityId);
			}
			return 0;
		}

		private int GetSlotOfEntitAtIndex(int index)
		{
			if (index < 0 || index >= m_entities.Count)
			{
				return -1;
			}
			Entity targetEntity = m_entities[index];
			if (targetEntity == null)
			{
				return -1;
			}
			targetEntity.GetEntityId();
			int currentSlot = 1;
			for (int i = 0; i <= index; i++)
			{
				if (i == index)
				{
					return currentSlot;
				}
				if (m_entities[i] != null)
				{
					currentSlot++;
				}
			}
			return -1;
		}

		public void Sort()
		{
			if (m_modified)
			{
				m_entities.Sort(SortComparison);
				return;
			}
			Entity[] originalEntities = m_entities.ToArray();
			m_entities.Sort(SortComparison);
			for (int i = 0; i < m_entities.Count; i++)
			{
				if (originalEntities[i] != m_entities[i])
				{
					m_modified = true;
					break;
				}
			}
		}

		public void PreprocessChanges()
		{
			m_prevEntities.Clear();
			for (int i = 0; i < m_entities.Count; i++)
			{
				m_prevEntities.Add(m_entities[i]);
			}
		}

		public void PostprocessChanges()
		{
			for (int i = 0; i < m_prevEntities.Count; i++)
			{
				if (i >= m_entities.Count)
				{
					break;
				}
				Entity prevEntity = m_prevEntities[i];
				if (m_entities.FindIndex((Entity currEntity) => currEntity == prevEntity) < 0)
				{
					Entity entity = m_entities[i];
					if (!m_prevEntities.Contains(entity))
					{
						m_replacedEntities[prevEntity.GetEntityId()] = entity.GetEntityId();
					}
				}
			}
		}

		public override string ToString()
		{
			return $"{m_zone} ({m_entities.Count} entities)";
		}

		private int SortComparison(Entity entity1, Entity entity2)
		{
			int zonePosition = entity1.GetZonePosition();
			int pos2 = entity2.GetZonePosition();
			return zonePosition - pos2;
		}
	}

	private static ProfilerMarker s_zoneMgrUpdateProcessLocalChangelistsMarker = new ProfilerMarker("ZoneMgr.Update.ProcessLocalChangeLists");

	private static ProfilerMarker s_zoneMgrUpdateProcessNetworkChangelistsMarker = new ProfilerMarker("ZoneMgr.Update.ProcessServerChangeLists");

	private Map<Type, string> m_tweenNames = new Map<Type, string>
	{
		{
			typeof(ZoneHand),
			"ZoneHandUpdateLayout"
		},
		{
			typeof(ZonePlay),
			"ZonePlayUpdateLayout"
		},
		{
			typeof(ZoneWeapon),
			"ZoneWeaponUpdateLayout"
		},
		{
			typeof(ZoneBattlegroundHeroBuddy),
			"ZoneBattlegroundHeroBuddyUpdateLayout"
		},
		{
			typeof(ZoneBattlegroundQuestReward),
			"ZoneBattlegroundQuestRewardUpdateLayout"
		},
		{
			typeof(ZoneBattlegroundAnomaly),
			"ZoneBattlegroundAnomalyUpdateLayout"
		},
		{
			typeof(ZoneBattlegroundTrinket),
			"ZoneBattlegroundTrinketUpdateLayout"
		},
		{
			typeof(ZoneBattlegroundClickableButton),
			"ZoneBattlegroundClickableButtonUpdateLayout"
		}
	};

	private static ZoneMgr s_instance;

	private List<Zone> m_zones;

	private int m_nextLocalChangeListId = 1;

	private int m_nextServerChangeListId = 1;

	private Queue<ZoneChangeList> m_pendingServerChangeLists = new Queue<ZoneChangeList>();

	private ZoneChangeList m_activeServerChangeList;

	private Map<int, Entity> m_tempEntityMap = new Map<int, Entity>();

	private Map<Zone, TempZone> m_tempZoneMap = new Map<Zone, TempZone>();

	private List<ZoneChangeList> m_activeLocalChangeLists = new List<ZoneChangeList>();

	private List<ZoneChangeList> m_pendingLocalChangeLists = new List<ZoneChangeList>();

	private QueueList<ZoneChangeList> m_localChangeListHistory = new QueueList<ZoneChangeList>();

	private bool m_doAutoCorrection;

	private float m_nextDeathBlockLayoutDelaySec;

	private LettuceZoneController m_lettuceZoneController;

	private CancellationTokenSource m_updateChangeCancelTokenSource;

	private void Awake()
	{
		s_instance = this;
		m_updateChangeCancelTokenSource = new CancellationTokenSource();
		m_zones = new List<Zone>();
		base.gameObject.GetComponentsInChildren(m_zones);
		if (GameState.Get() != null)
		{
			GameState.Get().RegisterCurrentPlayerChangedListener(OnCurrentPlayerChanged);
			GameState.Get().RegisterOptionRejectedListener(OnOptionRejected);
			m_lettuceZoneController = new LettuceZoneController(GameState.Get(), InputManager.Get());
		}
	}

	private void Start()
	{
		InputManager inputMgr = InputManager.Get();
		if (inputMgr != null)
		{
			inputMgr.StartWatchingForInput();
		}
	}

	private void Update()
	{
		UpdateLocalChangeLists(m_updateChangeCancelTokenSource.Token);
		UpdateServerChangeLists(m_updateChangeCancelTokenSource.Token);
	}

	private void OnDestroy()
	{
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterCurrentPlayerChangedListener(OnCurrentPlayerChanged);
			GameState.Get().UnregisterOptionRejectedListener(OnOptionRejected);
		}
		s_instance = null;
		m_zones = null;
		if (m_updateChangeCancelTokenSource != null)
		{
			m_updateChangeCancelTokenSource.Cancel();
			m_updateChangeCancelTokenSource.Dispose();
		}
	}

	public static ZoneMgr Get()
	{
		return s_instance;
	}

	public List<Zone> GetZones()
	{
		return m_zones;
	}

	public Zone FindZoneForTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (controllerId == 0)
		{
			return null;
		}
		if (zoneTag == TAG_ZONE.INVALID)
		{
			return null;
		}
		foreach (Zone zone in m_zones)
		{
			if (zone.CanAcceptTags(controllerId, zoneTag, cardType, entity))
			{
				return zone;
			}
		}
		return null;
	}

	public Zone FindZoneForEntity(Entity entity)
	{
		if (entity.GetZone() == TAG_ZONE.INVALID)
		{
			return null;
		}
		foreach (Zone zone in m_zones)
		{
			if (zone.CanAcceptTags(entity.GetControllerId(), entity.GetZone(), entity.GetCardType(), entity))
			{
				return zone;
			}
		}
		return null;
	}

	public Zone FindZoneForEntityAndZoneTag(Entity entity, TAG_ZONE zoneTag)
	{
		if (zoneTag == TAG_ZONE.INVALID)
		{
			return null;
		}
		foreach (Zone zone in m_zones)
		{
			if (zone.CanAcceptTags(entity.GetControllerId(), zoneTag, entity.GetCardType(), entity))
			{
				return zone;
			}
		}
		return null;
	}

	public T FindZoneOfType<T>(Player.Side side) where T : Zone
	{
		Type argType = typeof(T);
		foreach (Zone zone in m_zones)
		{
			if (!(zone.GetType() != argType) && zone.m_Side == side)
			{
				return (T)zone;
			}
		}
		return null;
	}

	public List<Zone> FindZonesForSide(Player.Side playerSide)
	{
		return FindZonesOfType<Zone>(playerSide);
	}

	public List<T> FindZonesOfType<T>() where T : Zone
	{
		return FindZonesOfType<T, T>();
	}

	public List<ReturnType> FindZonesOfType<ReturnType, ArgType>() where ReturnType : Zone where ArgType : Zone
	{
		List<ReturnType> zones = new List<ReturnType>();
		Type argType = typeof(ArgType);
		foreach (Zone zone in m_zones)
		{
			if (!(zone.GetType() != argType))
			{
				zones.Add((ReturnType)zone);
			}
		}
		return zones;
	}

	public List<T> FindZonesOfType<T>(Player.Side side) where T : Zone
	{
		return FindZonesOfType<T, T>(side);
	}

	public List<ReturnType> FindZonesOfType<ReturnType, ArgType>(Player.Side side) where ReturnType : Zone where ArgType : Zone
	{
		List<ReturnType> zones = new List<ReturnType>();
		foreach (Zone zone in m_zones)
		{
			if (zone is ArgType && zone.m_Side == side)
			{
				zones.Add((ReturnType)zone);
			}
		}
		return zones;
	}

	public List<Zone> FindZonesForTag(TAG_ZONE zoneTag)
	{
		List<Zone> zones = new List<Zone>();
		foreach (Zone zone in m_zones)
		{
			if (zone.m_ServerTag == zoneTag)
			{
				zones.Add(zone);
			}
		}
		return zones;
	}

	public Map<Type, string> GetTweenNames()
	{
		return m_tweenNames;
	}

	public string GetTweenName<T>() where T : Zone
	{
		Type t = typeof(T);
		string name = "";
		m_tweenNames.TryGetValue(t, out name);
		return name;
	}

	public void RequestNextDeathBlockLayoutDelaySec(float sec)
	{
		m_nextDeathBlockLayoutDelaySec = Mathf.Max(m_nextDeathBlockLayoutDelaySec, sec);
	}

	public float RemoveNextDeathBlockLayoutDelaySec()
	{
		float nextDeathBlockLayoutDelaySec = m_nextDeathBlockLayoutDelaySec;
		m_nextDeathBlockLayoutDelaySec = 0f;
		return nextDeathBlockLayoutDelaySec;
	}

	public int PredictZonePosition(Entity entity, Zone zone, int pos)
	{
		TempZone tempZone = BuildTempZone(zone);
		PredictZoneFromPowerProcessor(tempZone);
		RemoveDraggedMinionsFromTempZone(zone, tempZone);
		int predictedPos = FindBestMinionInsertionPosition(tempZone, pos - 1, pos);
		predictedPos = ValidatePredictedMinion(entity, tempZone, predictedPos);
		m_tempZoneMap.Clear();
		m_tempEntityMap.Clear();
		return predictedPos;
	}

	private void RemoveDraggedMinionsFromTempZone(Zone originalZone, TempZone tempZone)
	{
		foreach (Card card in originalZone.GetCards())
		{
			if (card.IsBeingDragged)
			{
				tempZone.RemoveEntityById(card.GetEntity().GetEntityId());
			}
		}
	}

	public bool HasPredictedCards()
	{
		if (HasPredictedCards<ZoneSecret>(TAG_ZONE.SECRET))
		{
			return true;
		}
		if (HasPredictedCards<ZoneWeapon>(TAG_ZONE.PLAY))
		{
			return true;
		}
		if (HasPredictedCards<ZoneHero>(TAG_ZONE.PLAY))
		{
			return true;
		}
		if (HasPredictedCards<ZoneGraveyard>(TAG_ZONE.GRAVEYARD))
		{
			return true;
		}
		return false;
	}

	public bool HasPredictedMovedMinion()
	{
		foreach (Zone item in FindZonesOfType<Zone>(Player.Side.FRIENDLY))
		{
			foreach (Card card in item.GetCards())
			{
				if (card.m_minionWasMovedFromSrcToDst != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsCardInValidZoneForPredictedPosition(Card card, Zone zone)
	{
		Entity entity = card.GetEntity();
		if (entity == null)
		{
			return true;
		}
		if (zone is ZonePlay)
		{
			if (entity.GetZone() != TAG_ZONE.GRAVEYARD)
			{
				return entity.GetZone() != TAG_ZONE.SETASIDE;
			}
			return false;
		}
		return true;
	}

	public bool HasPredictedPositions()
	{
		foreach (Zone zone in FindZonesOfType<Zone>(Player.Side.FRIENDLY))
		{
			foreach (Card card in zone.GetCards())
			{
				if (IsCardInValidZoneForPredictedPosition(card, zone) && card.GetPredictedZonePosition() != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPredictedCards<T>(TAG_ZONE predictedZone) where T : Zone
	{
		foreach (T item in FindZonesOfType<T>(Player.Side.FRIENDLY))
		{
			foreach (Card card in item.GetCards())
			{
				if (card.GetEntity().GetZone() != predictedZone)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void DebugPrintZonePos()
	{
		foreach (ZonePlay item in FindZonesOfType<ZonePlay>(Player.Side.FRIENDLY))
		{
			foreach (Card card in item.GetCards())
			{
				Entity entity = card.GetEntity();
				int cp = card.GetZonePosition();
				int ep = entity.GetZonePosition();
				int realEP = entity.GetRealTimeZonePosition();
				Debug.Log($"card : {entity.GetName()} cp: {cp} ep: {ep} rep: {realEP}");
			}
		}
	}

	public bool ShouldIgnorePosChange()
	{
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (GameMgr.Get().IsBattlegrounds() && gameEntity != null)
		{
			return gameEntity.GetRealTimeStep() == 10;
		}
		return false;
	}

	public bool IsBattlegroundShoppingPhase()
	{
		if (!(GameState.Get()?.GetGameEntity() is TB_BaconShop baconEntity))
		{
			return false;
		}
		return baconEntity.IsShopPhase();
	}

	public bool IsBattlegroundBattlePhase()
	{
		if (!(GameState.Get()?.GetGameEntity() is TB_BaconShop baconEntity))
		{
			return false;
		}
		return baconEntity.IsBattlePhase();
	}

	public void OnRealTimeZonePosChange(Entity entity)
	{
		if (!ShouldIgnorePosChange())
		{
			return;
		}
		ZoneChangeList changeList = null;
		Card card = entity.GetCard();
		if (card == null)
		{
			Debug.LogError($"[OnRealTimeZonePosChange] - entity [{entity}]'s card is null");
			return;
		}
		Zone zone = card.GetZone();
		if (zone == null || card.IsMagneticTarget() || entity.GetRealTimeZone() != zone.m_ServerTag || entity.HasQueuedControllerTagChange())
		{
			return;
		}
		int cardZonePos = card.GetZonePosition();
		int realTimeEntityPos = entity.GetRealTimeZonePosition();
		if (cardZonePos != realTimeEntityPos)
		{
			ZoneChange change = new ZoneChange();
			change.SetEntity(entity);
			change.SetSourceZone(zone);
			change.SetSourceZoneTag(zone.m_ServerTag);
			change.SetSourcePosition(cardZonePos);
			change.SetDestinationPosition(realTimeEntityPos);
			change.SetDestinationZone(zone);
			change.SetDestinationZoneTag(zone.m_ServerTag);
			if (changeList == null)
			{
				int id = GetNextLocalChangeListId();
				changeList = new ZoneChangeList();
				changeList.SetId(id);
				changeList.AddChange(change);
			}
			if (changeList != null)
			{
				ProcessLocalChangeList(changeList, m_updateChangeCancelTokenSource.Token);
			}
		}
	}

	public IEnumerable<ZoneChangeList> GetActivateLocalChangeList()
	{
		return m_activeLocalChangeLists;
	}

	public bool HasActiveLocalChange()
	{
		return m_activeLocalChangeLists.Count > 0;
	}

	public bool HasPendingLocalChange()
	{
		return m_pendingLocalChangeLists.Count > 0;
	}

	public bool HasUnresolvedLocalChange()
	{
		return m_localChangeListHistory.Count > 0;
	}

	public bool HasTriggeredActiveLocalChange(Card card)
	{
		return FindTriggeredActiveLocalChangeIndex(card) >= 0;
	}

	public ZoneChangeList AddLocalZoneChange(Card triggerCard, TAG_ZONE zoneTag)
	{
		Entity triggerEntity = triggerCard.GetEntity();
		Zone destinationZone = FindZoneForEntityAndZoneTag(triggerEntity, zoneTag);
		return AddLocalZoneChange(triggerCard, destinationZone, zoneTag, 0, null, null);
	}

	public ZoneChangeList AddLocalZoneChange(Card triggerCard, Zone destinationZone, int destinationPos)
	{
		if (destinationZone == null)
		{
			Debug.LogWarning($"ZoneMgr.AddLocalZoneChange() - illegal zone change to null zone for card {triggerCard}");
			return null;
		}
		return AddLocalZoneChange(triggerCard, destinationZone, destinationZone.m_ServerTag, destinationPos, null, null);
	}

	public ZoneChangeList AddLocalZoneChange(Card triggerCard, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos, ChangeCompleteCallback callback, object userData)
	{
		if (destinationZoneTag == TAG_ZONE.INVALID)
		{
			Debug.LogWarning($"ZoneMgr.AddLocalZoneChange() - illegal zone change to {destinationZoneTag} for card {triggerCard}");
			return null;
		}
		if ((destinationZone is ZonePlay || destinationZone is ZoneHand) && destinationPos <= 0)
		{
			Debug.LogWarning($"ZoneMgr.AddLocalZoneChange() - destinationPos {destinationPos} is too small for zone {destinationZone}, min is 1");
			return null;
		}
		ZoneChangeList changeList = CreateLocalChangeList(triggerCard, destinationZone, destinationZoneTag, destinationPos, callback, userData);
		ProcessOrEnqueueLocalChangeList(changeList, m_updateChangeCancelTokenSource.Token);
		m_localChangeListHistory.Enqueue(changeList);
		return changeList;
	}

	public ZoneChangeList AddPredictedLocalZoneChange(Card triggerCard, Zone destinationZone, int destinationPos, int predictedPos)
	{
		if (triggerCard == null)
		{
			Debug.LogWarning($"ZoneMgr.AddPredictedLocalZoneChange() - triggerCard is null");
			return null;
		}
		ZoneChangeList changeList = AddLocalZoneChange(triggerCard, destinationZone, destinationPos);
		if (changeList == null)
		{
			return null;
		}
		triggerCard.SetPredictedZonePosition(predictedPos);
		changeList.SetPredictedPosition(predictedPos);
		return changeList;
	}

	public ZoneChangeList CancelLocalZoneChange(ZoneChangeList changeList, ChangeCompleteCallback callback = null, object userData = null)
	{
		if (changeList == null)
		{
			Debug.LogWarning($"ZoneMgr.CancelLocalZoneChange() - changeList is null");
			return null;
		}
		if (!m_localChangeListHistory.Remove(changeList))
		{
			Debug.LogWarning($"ZoneMgr.CancelLocalZoneChange() - changeList {changeList.GetId()} is not in history");
			return null;
		}
		ZoneChange localTriggerChange = changeList.GetLocalTriggerChange();
		Entity triggerEntity = localTriggerChange.GetEntity();
		Card triggerCard = triggerEntity.GetCard();
		Zone destinationZone = localTriggerChange.GetSourceZone();
		int destinationPos = localTriggerChange.GetSourcePosition();
		ZoneChangeList canceledChangeList = CreateLocalChangeList(triggerCard, destinationZone, destinationZone.m_ServerTag, destinationPos, callback, userData);
		if (triggerEntity.IsHero())
		{
			AddOldHeroCanceledChange(canceledChangeList, triggerCard);
		}
		canceledChangeList.SetCanceledChangeList(canceledChangeList: true);
		canceledChangeList.SetZoneInputBlocking(block: true);
		ProcessOrEnqueueLocalChangeList(canceledChangeList, m_updateChangeCancelTokenSource.Token);
		return canceledChangeList;
	}

	private void AddOldHeroCanceledChange(ZoneChangeList canceledChangeList, Card triggerCard)
	{
		Player player = triggerCard.GetController();
		Card originalHero = player.GetHeroCard();
		ZoneChange triggerChange = new ZoneChange();
		triggerChange.SetParentList(canceledChangeList);
		triggerChange.SetEntity(originalHero.GetEntity());
		triggerChange.SetDestinationZone(player.GetHeroZone());
		triggerChange.SetDestinationZoneTag(player.GetHeroZone().m_ServerTag);
		triggerChange.SetDestinationPosition(0);
		Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - AddChange() canceledChangeList: {0},  triggerChange: {1}", canceledChangeList, triggerChange);
		canceledChangeList.AddChange(triggerChange);
	}

	public static bool IsHandledPower(Network.PowerHistory power)
	{
		switch (power.Type)
		{
		case Network.PowerType.FULL_ENTITY:
		{
			Network.HistFullEntity obj = power as Network.HistFullEntity;
			bool zoneTagsFound = false;
			{
				foreach (Network.Entity.Tag tag in obj.Entity.Tags)
				{
					if (tag.Name == 202)
					{
						if (tag.Value == 1)
						{
							return false;
						}
						if (tag.Value == 2)
						{
							return false;
						}
					}
					else if (tag.Name == 49 || tag.Name == 263 || tag.Name == 50 || tag.Name == 1702 || tag.Name == 1703 || tag.Name == 2032)
					{
						zoneTagsFound = true;
					}
				}
				return zoneTagsFound;
			}
		}
		case Network.PowerType.SHOW_ENTITY:
			return true;
		case Network.PowerType.HIDE_ENTITY:
			return true;
		case Network.PowerType.TAG_CHANGE:
		{
			Network.HistTagChange tagChange = power as Network.HistTagChange;
			if (tagChange.Tag == 49 || tagChange.Tag == 263 || tagChange.Tag == 50 || tagChange.Tag == 1702 || tagChange.Tag == 1703 || tagChange.Tag == 2032)
			{
				Entity entity = GameState.Get().GetEntity(tagChange.Entity);
				if (entity != null)
				{
					if (entity.IsPlayer())
					{
						return false;
					}
					if (entity.IsGame())
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool HasActiveServerChange()
	{
		return m_activeServerChangeList != null;
	}

	public bool HasPendingServerChange()
	{
		return m_pendingServerChangeLists.Count > 0;
	}

	public ZoneChangeList AddServerZoneChanges(PowerTaskList taskList, int taskStartIndex, int taskEndIndex, ChangeCompleteCallback callback, object userData)
	{
		int id = GetNextServerChangeListId();
		ZoneChangeList changeList = new ZoneChangeList();
		changeList.SetId(id);
		changeList.SetTaskList(taskList);
		changeList.SetCompleteCallback(callback);
		changeList.SetCompleteCallbackUserData(userData);
		changeList.SetIgnoreCardZonePurePosChanges(ShouldIgnorePosChange());
		Log.Zone.Print("ZoneMgr.AddServerZoneChanges() - taskListId={0} changeListId={1} taskStart={2} taskEnd={3}", taskList.GetId(), id, taskStartIndex, taskEndIndex);
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = taskStartIndex; i <= taskEndIndex; i++)
		{
			PowerTask task = tasks[i];
			Network.PowerHistory power = task.GetPower();
			Network.PowerType powerType = power.Type;
			ZoneChange change = null;
			switch (powerType)
			{
			case Network.PowerType.FULL_ENTITY:
			{
				Network.HistFullEntity fullEntity = (Network.HistFullEntity)power;
				change = CreateZoneChangeFromFullEntity(fullEntity);
				break;
			}
			case Network.PowerType.SHOW_ENTITY:
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
				change = CreateZoneChangeFromEntity(showEntity.Entity);
				break;
			}
			case Network.PowerType.CHANGE_ENTITY:
			{
				Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)power;
				change = CreateZoneChangeFromEntity(changeEntity.Entity);
				break;
			}
			case Network.PowerType.HIDE_ENTITY:
			{
				Network.HistHideEntity hideEntity = (Network.HistHideEntity)power;
				change = CreateZoneChangeFromHideEntity(hideEntity);
				break;
			}
			case Network.PowerType.TAG_CHANGE:
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				change = CreateZoneChangeFromTagChange(tagChange);
				break;
			}
			case Network.PowerType.META_DATA:
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				change = CreateZoneChangeFromMetaData(metaData);
				break;
			}
			case Network.PowerType.CREATE_GAME:
			case Network.PowerType.RESET_GAME:
			case Network.PowerType.SUB_SPELL_START:
			case Network.PowerType.SUB_SPELL_END:
			case Network.PowerType.VO_SPELL:
			case Network.PowerType.CACHED_TAG_FOR_DORMANT_CHANGE:
			case Network.PowerType.SHUFFLE_DECK:
			case Network.PowerType.TAG_LIST_CHANGE:
				change = CreateZoneChangeForNonZoneTask();
				break;
			default:
				Debug.LogError($"ZoneMgr.AddServerZoneChanges() - id={changeList.GetId()} received unhandled power of type {powerType}");
				return null;
			}
			if (change != null)
			{
				change.SetParentList(changeList);
				change.SetPowerTask(task);
				Log.Zone.Print("ZoneMgr.AddServerZoneChanges() - AddChange() changeList: {0},  change: {1}", changeList, change);
				changeList.AddChange(change);
			}
		}
		for (int j = 0; j < changeList.GetChanges().Count; j++)
		{
			ZoneChange change2 = changeList.GetChanges()[j];
			if (!(change2.GetPowerTask().GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.CONTROLLER_AND_ZONE_CHANGE } metaData2))
			{
				continue;
			}
			if (metaData2.Info.Count != 5)
			{
				Log.Zone.PrintError("CONTROLLER_AND_ZONE_CHANGE MetaData task found ({0}), but info array isn't of size 5!");
			}
			ZoneChange zoneChange = null;
			List<ZoneChange> controllerChanges = new List<ZoneChange>();
			int oldControllerID = metaData2.Info[1];
			int newControllerID = metaData2.Info[2];
			TAG_ZONE oldZoneID = (TAG_ZONE)metaData2.Info[3];
			TAG_ZONE newZoneID = (TAG_ZONE)metaData2.Info[4];
			for (int k = j + 1; k < changeList.GetChanges().Count; k++)
			{
				ZoneChange innerChange = changeList.GetChanges()[k];
				if (innerChange.GetEntity() == change2.GetEntity())
				{
					if (innerChange.HasDestinationControllerId() && innerChange.GetDestinationControllerId() == newControllerID && innerChange.GetDestinationZoneTag() != newZoneID)
					{
						controllerChanges.Add(innerChange);
					}
					else if (innerChange.HasDestinationControllerId() && innerChange.GetDestinationControllerId() == newControllerID && innerChange.HasDestinationZoneChange() && innerChange.GetDestinationZoneTag() == newZoneID)
					{
						controllerChanges.Add(innerChange);
						zoneChange = innerChange;
					}
					else if (!innerChange.HasDestinationControllerId() && innerChange.HasDestinationZoneChange() && innerChange.GetDestinationZoneTag() == newZoneID)
					{
						zoneChange = innerChange;
					}
					if (controllerChanges.Count > 0 && zoneChange != null)
					{
						break;
					}
				}
			}
			ZoneChange finalControllerChange = controllerChanges[controllerChanges.Count - 1];
			if (controllerChanges.Count > 0 && zoneChange != null)
			{
				Entity entity = finalControllerChange.GetEntity();
				Zone sourceZone = FindZoneForTags(oldControllerID, oldZoneID, entity.GetCardType(), entity);
				zoneChange.SetSourceZone(sourceZone);
				zoneChange.SetSourceZoneTag(oldZoneID);
				zoneChange.SetDestinationControllerId(finalControllerChange.GetDestinationControllerId());
				zoneChange.SetSourceControllerId(oldControllerID);
				foreach (ZoneChange controllerChange in controllerChanges)
				{
					if (zoneChange != controllerChange)
					{
						controllerChange.ClearDestinationControllerId();
						controllerChange.SetDestinationZone(null);
					}
				}
			}
			else
			{
				Log.Zone.PrintError("CONTROLLER_AND_ZONE_CHANGE MetaData task found ({0}), but couldn't find both controller ({1}) and zone ({2}) changes in tasklist!", change2, finalControllerChange, zoneChange);
			}
		}
		m_tempEntityMap.Clear();
		m_pendingServerChangeLists.Enqueue(changeList);
		return changeList;
	}

	private void UpdateLocalChangeLists(CancellationToken token)
	{
		using (s_zoneMgrUpdateProcessLocalChangelistsMarker.Auto())
		{
			List<ZoneChangeList> completedChangeLists = null;
			int i = 0;
			while (i < m_activeLocalChangeLists.Count)
			{
				ZoneChangeList changeList = m_activeLocalChangeLists[i];
				if (!changeList.IsComplete())
				{
					i++;
					continue;
				}
				changeList.FireCompleteCallback();
				m_activeLocalChangeLists.RemoveAt(i);
				if (completedChangeLists == null)
				{
					completedChangeLists = new List<ZoneChangeList>();
				}
				completedChangeLists.Add(changeList);
			}
			if (completedChangeLists == null)
			{
				return;
			}
			foreach (ZoneChangeList item in completedChangeLists)
			{
				ZoneChange triggerChange = item.GetLocalTriggerChange();
				Card triggerCard = triggerChange.GetEntity().GetCard();
				if (item.IsCanceledChangeList())
				{
					triggerCard.SetPredictedZonePosition(0);
					if (triggerCard.m_minionWasMovedFromSrcToDst != null && triggerCard.m_minionWasMovedFromSrcToDst.m_destinationZonePosition == triggerChange.GetDestinationPosition())
					{
						triggerCard.m_minionWasMovedFromSrcToDst = null;
					}
				}
				int pendingIndex = FindTriggeredPendingLocalChangeIndex(triggerCard);
				if (pendingIndex >= 0)
				{
					ZoneChangeList pendingChangeList = m_pendingLocalChangeLists[pendingIndex];
					m_pendingLocalChangeLists.RemoveAt(pendingIndex);
					CreateLocalChangesFromTrigger(pendingChangeList, pendingChangeList.GetLocalTriggerChange());
					ProcessLocalChangeList(pendingChangeList, token);
				}
			}
		}
	}

	private void UpdateServerChangeLists(CancellationToken token)
	{
		using (s_zoneMgrUpdateProcessNetworkChangelistsMarker.Auto())
		{
			if (m_activeServerChangeList != null && m_activeServerChangeList.IsComplete())
			{
				m_activeServerChangeList.FireCompleteCallback();
				m_activeServerChangeList = null;
				m_doAutoCorrection = true;
			}
			if (HasPendingServerChange() && !HasActiveServerChange())
			{
				m_activeServerChangeList = m_pendingServerChangeLists.Dequeue();
				PostProcessServerChangeList(m_activeServerChangeList);
				m_activeServerChangeList.ProcessChanges(token).Forget();
			}
			if (m_doAutoCorrection && AutoCorrectZonesAfterServerChange(token))
			{
				m_doAutoCorrection = false;
			}
		}
	}

	private bool HasLocalChangeExitingZone(Entity entity, Zone zone)
	{
		if (HasLocalChangeExitingZone(entity, zone, m_activeLocalChangeLists))
		{
			return true;
		}
		if (HasLocalChangeExitingZone(entity, zone, m_pendingLocalChangeLists))
		{
			return true;
		}
		return false;
	}

	private bool HasLocalChangeExitingZone(Entity entity, Zone zone, List<ZoneChangeList> changeLists)
	{
		TAG_ZONE zoneTag = zone.m_ServerTag;
		foreach (ZoneChangeList changeList in changeLists)
		{
			foreach (ZoneChange change in changeList.GetChanges())
			{
				if (entity == change.GetEntity() && zoneTag == change.GetSourceZoneTag() && zoneTag != change.GetDestinationZoneTag())
				{
					return true;
				}
			}
		}
		return false;
	}

	private void PredictZoneFromPowerProcessor(TempZone tempZone)
	{
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		tempZone.PreprocessChanges();
		powerProcessor.ForEachTaskList(delegate(int queueIndex, PowerTaskList taskList)
		{
			PredictZoneFromPowerTaskList(tempZone, taskList);
		});
		tempZone.Sort();
		tempZone.PostprocessChanges();
	}

	private void PredictZoneFromPowerTaskList(TempZone tempZone, PowerTaskList taskList)
	{
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			PredictZoneFromPower(tempZone, power);
		}
	}

	private void PredictZoneFromPower(TempZone tempZone, Network.PowerHistory power)
	{
		switch (power.Type)
		{
		case Network.PowerType.FULL_ENTITY:
			PredictZoneFromFullEntity(tempZone, (Network.HistFullEntity)power);
			break;
		case Network.PowerType.SHOW_ENTITY:
			PredictZoneFromShowEntity(tempZone, (Network.HistShowEntity)power);
			break;
		case Network.PowerType.HIDE_ENTITY:
			PredictZoneFromHideEntity(tempZone, (Network.HistHideEntity)power);
			break;
		case Network.PowerType.TAG_CHANGE:
			PredictZoneFromTagChange(tempZone, (Network.HistTagChange)power);
			break;
		}
	}

	private void PredictZoneFromFullEntity(TempZone tempZone, Network.HistFullEntity fullEntity)
	{
		Entity tempEntity = RegisterTempEntity(fullEntity.Entity);
		if (tempEntity != null)
		{
			Zone zone = tempZone.GetZone();
			bool num = tempEntity.GetZone() == zone.m_ServerTag;
			bool hasCorrectController = tempEntity.GetControllerId() == zone.GetControllerId();
			if (num && hasCorrectController)
			{
				tempZone.AddEntity(tempEntity);
			}
		}
	}

	private void PredictZoneFromShowEntity(TempZone tempZone, Network.HistShowEntity showEntity)
	{
		Entity tempEntity = RegisterTempEntity(showEntity.Entity);
		foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
		{
			PredictZoneByApplyingTag(tempZone, tempEntity, (GAME_TAG)tag.Name, tag.Value);
		}
	}

	private void PredictZoneFromHideEntity(TempZone tempZone, Network.HistHideEntity hideEntity)
	{
		Entity tempEntity = RegisterTempEntity(hideEntity.Entity);
		PredictZoneByApplyingTag(tempZone, tempEntity, GAME_TAG.ZONE, hideEntity.Zone);
	}

	private void PredictZoneFromTagChange(TempZone tempZone, Network.HistTagChange tagChange)
	{
		Entity tempEntity = RegisterTempEntity(tagChange.Entity);
		PredictZoneByApplyingTag(tempZone, tempEntity, (GAME_TAG)tagChange.Tag, tagChange.Value);
	}

	private void PredictZoneByApplyingTag(TempZone tempZone, Entity tempEntity, GAME_TAG tag, int val)
	{
		if (tempEntity == null)
		{
			return;
		}
		if (tag != GAME_TAG.ZONE && tag != GAME_TAG.CONTROLLER && tag != GAME_TAG.FAKE_ZONE && tag != GAME_TAG.FAKE_CONTROLLER)
		{
			tempEntity.SetTag(tag, val);
			return;
		}
		Zone zone = tempZone.GetZone();
		bool num = tempEntity.GetZone() == zone.m_ServerTag;
		bool hasController = tempEntity.GetControllerId() == zone.GetControllerId();
		if (num && hasController)
		{
			tempZone.RemoveEntity(tempEntity);
		}
		tempEntity.SetTag(tag, val);
		bool num2 = tempEntity.GetZone() == zone.m_ServerTag;
		hasController = tempEntity.GetControllerId() == zone.GetControllerId();
		if (num2 && hasController)
		{
			tempZone.AddEntity(tempEntity);
		}
	}

	private ZoneChange CreateZoneChange(Card triggerCard, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos)
	{
		Entity triggerEntity = triggerCard.GetEntity();
		Zone sourceZone = triggerCard.GetZone();
		TAG_ZONE sourceZoneTag = ((!(sourceZone == null)) ? sourceZone.m_ServerTag : TAG_ZONE.INVALID);
		int sourcePos = triggerCard.GetZonePosition();
		ZoneChange zoneChange = new ZoneChange();
		zoneChange.SetEntity(triggerEntity);
		zoneChange.SetSourceZone(sourceZone);
		zoneChange.SetSourceZoneTag(sourceZoneTag);
		zoneChange.SetSourcePosition(sourcePos);
		zoneChange.SetDestinationZone(destinationZone);
		zoneChange.SetDestinationZoneTag(destinationZoneTag);
		zoneChange.SetDestinationPosition(destinationPos);
		return zoneChange;
	}

	private ZoneChangeList CreateLocalChangeList(Card triggerCard, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos, ChangeCompleteCallback callback, object userData)
	{
		int id = GetNextLocalChangeListId();
		Log.Zone.Print("ZoneMgr.CreateLocalChangeList() - changeListId={0}", id);
		ZoneChangeList changeList = new ZoneChangeList();
		changeList.SetId(id);
		changeList.SetCompleteCallback(callback);
		changeList.SetCompleteCallbackUserData(userData);
		ZoneChange triggerChange = CreateZoneChange(triggerCard, destinationZone, destinationZoneTag, destinationPos);
		triggerChange.SetParentList(changeList);
		Log.Zone.Print("ZoneMgr.CreateLocalChangeList() - AddChange() changeList: {0}, triggerChange: {1}", changeList, triggerChange);
		changeList.AddChange(triggerChange);
		return changeList;
	}

	private void ProcessOrEnqueueLocalChangeList(ZoneChangeList changeList, CancellationToken token)
	{
		ZoneChange triggerChange = changeList.GetLocalTriggerChange();
		Card triggerCard = triggerChange.GetEntity().GetCard();
		if (HasTriggeredActiveLocalChange(triggerCard) && !IsBattlegroundShoppingPhase())
		{
			m_pendingLocalChangeLists.Add(changeList);
			return;
		}
		CreateLocalChangesFromTrigger(changeList, triggerChange);
		ProcessLocalChangeList(changeList, token);
	}

	private void CreateLocalChangesFromTrigger(ZoneChangeList changeList, ZoneChange triggerChange)
	{
		Log.Zone.Print($"ZoneMgr.CreateLocalChangesFromTrigger() - {changeList}");
		Entity triggerEntity = triggerChange.GetEntity();
		Zone sourceZone = triggerChange.GetSourceZone();
		int sourcePos = triggerChange.GetSourcePosition();
		Zone destinationZone = triggerChange.GetDestinationZone();
		int destinationPos = triggerChange.GetDestinationPosition();
		if (sourceZone != destinationZone)
		{
			TAG_ZONE sourceZoneTag = triggerChange.GetSourceZoneTag();
			TAG_ZONE destinationZoneTag = triggerChange.GetDestinationZoneTag();
			CreateLocalChangesFromTrigger(changeList, triggerEntity, sourceZone, sourceZoneTag, sourcePos, destinationZone, destinationZoneTag, destinationPos);
		}
		else if (sourcePos != destinationPos)
		{
			CreateLocalPosOnlyChangesFromTrigger(changeList, triggerEntity, sourceZone, sourcePos, destinationPos);
		}
	}

	private void CreateLocalChangesFromTrigger(ZoneChangeList changeList, Entity triggerEntity, Zone sourceZone, TAG_ZONE sourceZoneTag, int sourcePos, Zone destinationZone, TAG_ZONE destinationZoneTag, int destinationPos)
	{
		Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - triggerEntity={0} srcZone={1} srcPos={2} dstZone={3} dstPos={4}", triggerEntity, sourceZoneTag, sourcePos, destinationZoneTag, destinationPos);
		if (sourcePos != destinationPos)
		{
			Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - srcPos={0} destPos={1}", sourcePos, destinationPos);
		}
		if (sourceZone != null && !(sourceZone is ZoneHero))
		{
			foreach (Card card in sourceZone.GetCards())
			{
				int currentZonePos = card.GetZonePosition();
				if (currentZonePos > sourcePos)
				{
					Entity entity = card.GetEntity();
					ZoneChange change = new ZoneChange();
					change.SetParentList(changeList);
					change.SetEntity(entity);
					int destinationZonePos = currentZonePos - 1;
					change.SetSourcePosition(currentZonePos);
					change.SetDestinationPosition(destinationZonePos);
					Log.Zone.Print($"ZoneMgr.CreateLocalChangesFromTrigger() - srcZone card {card} zonePos {card.GetZonePosition()} -> {destinationZonePos}");
					Log.Zone.Print($"ZoneMgr.CreateLocalChangesFromTrigger() 3 - AddChange() changeList: {changeList}, change: {change}");
					changeList.AddChange(change);
				}
			}
		}
		if (!(destinationZone != null) || destinationZone is ZoneSecret)
		{
			return;
		}
		if (destinationZone is ZoneWeapon || destinationZone is ZoneBattlegroundQuestReward || destinationZone is ZoneBattlegroundClickableButton)
		{
			List<Card> destinationCards = destinationZone.GetCards();
			if (destinationCards.Count > 0)
			{
				Entity entity2 = destinationCards[0].GetEntity();
				ZoneChange change2 = new ZoneChange();
				change2.SetParentList(changeList);
				change2.SetEntity(entity2);
				change2.SetDestinationZone(FindZoneOfType<ZoneGraveyard>(destinationZone.m_Side));
				change2.SetDestinationZoneTag(TAG_ZONE.GRAVEYARD);
				Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() 4 - AddChange() changeList: {0}, change: {1}", changeList, change2);
				changeList.AddChange(change2);
			}
		}
		else if (destinationZone is ZonePlay || destinationZone is ZoneHand)
		{
			List<Card> destinationCards2 = destinationZone.GetCards();
			ZonePlay zonePlay = destinationZone as ZonePlay;
			for (int i = 0; i < destinationCards2.Count; i++)
			{
				Card card2 = destinationCards2[i];
				int slot = ((zonePlay != null) ? zonePlay.GetSlotOfCardAtIndex(i) : (i + 1));
				if (slot >= destinationPos)
				{
					Entity entity3 = card2.GetEntity();
					int destinationZonePos2 = slot + 1;
					ZoneChange change3 = new ZoneChange();
					change3.SetParentList(changeList);
					change3.SetEntity(entity3);
					change3.SetDestinationPosition(destinationZonePos2);
					Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() - dstZone card {0} zonePos {1} -> {2}", card2, entity3.GetZonePosition(), destinationZonePos2);
					Log.Zone.Print("ZoneMgr.CreateLocalChangesFromTrigger() 5 - AddChange() changeList: {0}, change: {1}", changeList, change3);
					changeList.AddChange(change3);
				}
			}
		}
		else if (!(destinationZone is ZoneHero))
		{
			Debug.LogError($"ZoneMgr.CreateLocalChangesFromTrigger() - don't know how to predict zone position changes for zone {destinationZone}");
		}
	}

	private void CreateLocalPosOnlyChangesFromTrigger(ZoneChangeList changeList, Entity triggerEntity, Zone sourceZone, int sourcePos, int destinationPos)
	{
		List<Card> sourceCards = sourceZone.GetCards();
		if (sourcePos < destinationPos)
		{
			for (int i = 0; i < sourceCards.Count; i++)
			{
				Card card = sourceCards[i];
				Entity entity = card.GetEntity();
				int currentZonePosition = card.GetZonePosition();
				if (currentZonePosition <= destinationPos && currentZonePosition >= sourcePos)
				{
					int destinationZonePos = currentZonePosition - 1;
					if (entity == triggerEntity)
					{
						destinationZonePos = destinationPos;
					}
					ZoneChange change = new ZoneChange();
					change.SetParentList(changeList);
					change.SetEntity(entity);
					change.SetSourcePosition(card.GetZonePosition());
					change.SetDestinationPosition(destinationZonePos);
					Log.Zone.Print("ZoneMgr.CreateLocalPosOnlyChangesFromTrigger() 1 - AddChange() changeList: {0}, change: {1}", changeList, change);
					changeList.AddChange(change);
				}
			}
			return;
		}
		for (int j = 0; j < sourceCards.Count; j++)
		{
			Card card2 = sourceCards[j];
			Entity entity2 = card2.GetEntity();
			int currentZonePosition2 = card2.GetZonePosition();
			if (currentZonePosition2 <= sourcePos && currentZonePosition2 >= destinationPos)
			{
				int destinationZonePos2 = currentZonePosition2 + 1;
				if (entity2 == triggerEntity)
				{
					destinationZonePos2 = destinationPos;
				}
				ZoneChange change2 = new ZoneChange();
				change2.SetParentList(changeList);
				change2.SetEntity(entity2);
				change2.SetSourcePosition(card2.GetZonePosition());
				change2.SetDestinationPosition(destinationZonePos2);
				Log.Zone.Print("ZoneMgr.CreateLocalPosOnlyChangesFromTrigger() 2 - AddChange() changeList: {0}, change: {1}", changeList, change2);
				changeList.AddChange(change2);
			}
		}
	}

	private void ProcessLocalChangeList(ZoneChangeList changeList, CancellationToken token)
	{
		Log.Zone.Print("ZoneMgr.ProcessLocalChangeList() - [{0}]", changeList);
		m_activeLocalChangeLists.Add(changeList);
		changeList.ProcessChanges(token).Forget();
	}

	private void OnCurrentPlayerChanged(Player player, object userData)
	{
		if (player.IsLocalUser())
		{
			m_localChangeListHistory.Clear();
		}
	}

	public void ClearLocalChangeListHistory()
	{
		m_localChangeListHistory.Clear();
	}

	private void OnOptionRejected(Network.Options.Option option, object userData)
	{
		if (option.Type != Network.Options.Option.OptionType.POWER)
		{
			return;
		}
		Entity triggerEntity = GameState.Get().GetEntity(option.Main.ID);
		ZoneChangeList rejectedChangeList = FindRejectedLocalZoneChange(triggerEntity);
		if (rejectedChangeList == null)
		{
			Log.Zone.Print("ZoneMgr.RejectLocalZoneChange() - did not find a zone change to reject for {0}", triggerEntity);
			return;
		}
		Card triggerCard = triggerEntity.GetCard();
		triggerCard.SetPredictedZonePosition(0);
		ZoneChange triggerChange = rejectedChangeList.GetLocalTriggerChange();
		if (triggerCard.m_minionWasMovedFromSrcToDst != null && triggerCard.m_minionWasMovedFromSrcToDst.m_destinationZonePosition == triggerChange.GetDestinationPosition())
		{
			triggerCard.m_minionWasMovedFromSrcToDst = null;
		}
		CancelLocalZoneChange(rejectedChangeList);
	}

	private ZoneChangeList FindRejectedLocalZoneChange(Entity triggerEntity)
	{
		List<ZoneChangeList> changeLists = m_localChangeListHistory.GetList();
		for (int i = 0; i < changeLists.Count; i++)
		{
			ZoneChangeList changeList = changeLists[i];
			List<ZoneChange> changes = changeList.GetChanges();
			for (int j = 0; j < changes.Count; j++)
			{
				ZoneChange change = changes[j];
				if (change.GetEntity() == triggerEntity && change.GetDestinationZoneTag() == TAG_ZONE.PLAY)
				{
					return changeList;
				}
			}
		}
		return null;
	}

	private ZoneChange CreateZoneChangeForNonZoneTask()
	{
		ZoneChange zoneChange = new ZoneChange();
		zoneChange.SetEntity(GameState.Get().GetGameEntity());
		return zoneChange;
	}

	private ZoneChange CreateZoneChangeFromFullEntity(Network.HistFullEntity fullEntity)
	{
		Network.Entity netEnt = fullEntity.Entity;
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		if (entity == null)
		{
			Debug.LogWarning($"ZoneMgr.CreateZoneChangeFromFullEntity() - WARNING entity {netEnt.ID} DOES NOT EXIST!");
			return null;
		}
		ZoneChange change = new ZoneChange();
		change.SetEntity(entity);
		if (entity.GetCard() == null)
		{
			return change;
		}
		bool destinationZoneFound = false;
		bool destinationZonePosFound = false;
		bool destinationControllerFound = false;
		using (List<Network.Entity.Tag>.Enumerator enumerator = netEnt.Tags.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch ((GAME_TAG)enumerator.Current.Name)
				{
				case GAME_TAG.ZONE:
				case GAME_TAG.FAKE_ZONE:
					destinationZoneFound = true;
					break;
				case GAME_TAG.ZONE_POSITION:
				case GAME_TAG.FAKE_ZONE_POSITION:
					destinationZonePosFound = true;
					break;
				case GAME_TAG.CONTROLLER:
				case GAME_TAG.FAKE_CONTROLLER:
					destinationControllerFound = true;
					break;
				}
			}
		}
		if (destinationZoneFound)
		{
			change.SetDestinationZoneTag(entity.GetZone());
		}
		if (destinationZonePosFound)
		{
			change.SetDestinationPosition(entity.GetZonePosition());
		}
		if (destinationControllerFound)
		{
			change.SetDestinationControllerId(entity.GetControllerId());
		}
		if (destinationZoneFound || destinationControllerFound)
		{
			change.SetDestinationZone(FindZoneForEntity(entity));
		}
		return change;
	}

	private ZoneChange CreateZoneChangeFromEntity(Network.Entity netEnt)
	{
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		if (entity == null)
		{
			if (!GameState.Get().EntityRemovedFromGame(netEnt.ID))
			{
				Debug.LogWarning($"ZoneMgr.CreateZoneChangeFromEntity() - WARNING entity {netEnt.ID} DOES NOT EXIST!");
			}
			return null;
		}
		ZoneChange change = new ZoneChange();
		change.SetEntity(entity);
		if (entity.GetCard() == null)
		{
			return change;
		}
		Entity tempEntity = RegisterTempEntity(netEnt.ID, entity);
		if (tempEntity == null)
		{
			return change;
		}
		bool destinationZoneFound = false;
		bool destinationZonePosFound = false;
		bool destinationControllerFound = false;
		foreach (Network.Entity.Tag tag in netEnt.Tags)
		{
			tempEntity.SetTag(tag.Name, tag.Value);
			switch ((GAME_TAG)tag.Name)
			{
			case GAME_TAG.ZONE:
			case GAME_TAG.FAKE_ZONE:
				destinationZoneFound = true;
				break;
			case GAME_TAG.ZONE_POSITION:
			case GAME_TAG.FAKE_ZONE_POSITION:
				destinationZonePosFound = true;
				break;
			case GAME_TAG.CONTROLLER:
			case GAME_TAG.FAKE_CONTROLLER:
				destinationControllerFound = true;
				break;
			}
		}
		if (destinationZoneFound)
		{
			change.SetDestinationZoneTag(tempEntity.GetZone());
		}
		if (destinationZonePosFound)
		{
			change.SetDestinationPosition(tempEntity.GetZonePosition());
		}
		if (destinationControllerFound)
		{
			change.SetDestinationControllerId(tempEntity.GetControllerId());
		}
		if (destinationZoneFound || destinationControllerFound)
		{
			change.SetDestinationZone(FindZoneForEntity(tempEntity));
		}
		return change;
	}

	private ZoneChange CreateZoneChangeFromHideEntity(Network.HistHideEntity hideEntity)
	{
		Entity entity = GameState.Get().GetEntity(hideEntity.Entity);
		if (entity == null)
		{
			if (!GameState.Get().EntityRemovedFromGame(hideEntity.Entity))
			{
				Debug.LogWarning($"ZoneMgr.CreateZoneChangeFromHideEntity() - WARNING entity {hideEntity.Entity} DOES NOT EXIST! zone={hideEntity.Zone}");
			}
			return null;
		}
		ZoneChange change = new ZoneChange();
		change.SetEntity(entity);
		if (entity.GetCard() == null)
		{
			return change;
		}
		Entity tempEntity = RegisterTempEntity(hideEntity.Entity, entity);
		if (tempEntity == null)
		{
			return change;
		}
		tempEntity.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
		TAG_ZONE zoneTag = (TAG_ZONE)hideEntity.Zone;
		change.SetDestinationZoneTag(zoneTag);
		change.SetDestinationZone(FindZoneForEntity(tempEntity));
		return change;
	}

	private ZoneChange CreateZoneChangeFromTagChange(Network.HistTagChange tagChange)
	{
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			if (!GameState.Get().EntityRemovedFromGame(tagChange.Entity))
			{
				Debug.LogError($"ZoneMgr.CreateZoneChangeFromTagChange() - Entity {tagChange.Entity} does not exist");
			}
			return null;
		}
		ZoneChange change = new ZoneChange();
		change.SetEntity(entity);
		if (entity.GetCard() == null)
		{
			return change;
		}
		Entity tempEntity = RegisterTempEntity(tagChange.Entity, entity);
		if (tempEntity == null)
		{
			return change;
		}
		tempEntity.SetTag(tagChange.Tag, tagChange.Value);
		switch ((GAME_TAG)tagChange.Tag)
		{
		case GAME_TAG.ZONE:
		case GAME_TAG.FAKE_ZONE:
			change.SetDestinationZoneTag(tempEntity.GetZone());
			change.SetDestinationZone(FindZoneForEntity(tempEntity));
			break;
		case GAME_TAG.ZONE_POSITION:
		case GAME_TAG.FAKE_ZONE_POSITION:
			change.SetDestinationPosition(tempEntity.GetZonePosition());
			break;
		case GAME_TAG.CONTROLLER:
		case GAME_TAG.FAKE_CONTROLLER:
			change.SetDestinationControllerId(tempEntity.GetControllerId());
			change.SetDestinationZone(FindZoneForEntity(tempEntity));
			break;
		}
		return change;
	}

	private ZoneChange CreateZoneChangeFromMetaData(Network.HistMetaData metaData)
	{
		if (metaData.Info.Count <= 0)
		{
			return null;
		}
		Entity entity = GameState.Get().GetEntity(metaData.Info[0]);
		if (entity == null)
		{
			Debug.LogError($"ZoneMgr.CreateZoneChangeFromMetaData() - Entity {metaData.Info[0]} does not exist");
			return null;
		}
		ZoneChange zoneChange = new ZoneChange();
		zoneChange.SetEntity(entity);
		return zoneChange;
	}

	private Entity RegisterTempEntity(int id)
	{
		Entity entity = GameState.Get().GetEntity(id);
		return RegisterTempEntity(id, entity);
	}

	private Entity RegisterTempEntity(Network.Entity netEnt)
	{
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		return RegisterTempEntity(netEnt.ID, entity);
	}

	private Entity RegisterTempEntity(Entity entity)
	{
		int entID = entity?.GetEntityId() ?? (-1);
		return RegisterTempEntity(entID, entity);
	}

	private Entity RegisterTempEntity(int id, Entity entity)
	{
		if (entity == null)
		{
			string message = $"{this}.RegisterTempEntity(): Attempting to register an invalid entity! No dbid {id} exists.";
			TelemetryManager.Client().SendLiveIssue("Gameplay_ZoneManager", message);
			Log.Zone.PrintWarning(message);
		}
		Entity tempEntity = null;
		if (!m_tempEntityMap.TryGetValue(id, out tempEntity) && entity != null)
		{
			tempEntity = entity.CloneForZoneMgr();
			m_tempEntityMap.Add(id, tempEntity);
		}
		return tempEntity;
	}

	private void PostProcessServerChangeList(ZoneChangeList serverChangeList)
	{
		if (ShouldPostProcessServerChangeList(serverChangeList) && !CheckAndIgnoreServerChangeList(serverChangeList) && !ReplaceRemoteWeaponInServerChangeList(serverChangeList) && !PreventLastStandingMinionFromLeavingPlay(serverChangeList))
		{
			MergeServerChangeList(serverChangeList);
		}
	}

	private bool ShouldPostProcessServerChangeList(ZoneChangeList changeList)
	{
		List<ZoneChange> changes = changeList.GetChanges();
		for (int i = 0; i < changes.Count; i++)
		{
			if (changes[i].HasDestinationData())
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckAndIgnoreServerChangeList(ZoneChangeList serverChangeList)
	{
		Network.HistBlockStart blockStart = serverChangeList.GetTaskList().GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		if (blockStart.BlockType != HistoryBlock.Type.PLAY && blockStart.BlockType != HistoryBlock.Type.MOVE_MINION)
		{
			return false;
		}
		ZoneChangeList localChangeList = FindLocalChangeListMatchingServerChangeList(serverChangeList);
		if (localChangeList == null)
		{
			return false;
		}
		serverChangeList.SetIgnoreCardZoneChanges(ignore: true);
		Card localTriggerCard = localChangeList.GetLocalTriggerCard();
		if (blockStart.BlockType == HistoryBlock.Type.MOVE_MINION && localTriggerCard != null && localTriggerCard.m_minionWasMovedFromSrcToDst != null)
		{
			foreach (ZoneChange change in localChangeList.GetChanges())
			{
				if (change.GetDestinationPosition() == localTriggerCard.m_minionWasMovedFromSrcToDst.m_destinationZonePosition && change.GetEntity() == localTriggerCard.GetEntity())
				{
					localTriggerCard.m_minionWasMovedFromSrcToDst = null;
					break;
				}
			}
		}
		while (m_localChangeListHistory.Count > 0)
		{
			ZoneChangeList currLocalChangeList = m_localChangeListHistory.Dequeue();
			if (localChangeList == currLocalChangeList)
			{
				localChangeList.GetLocalTriggerCard().SetPredictedZonePosition(0);
				break;
			}
		}
		return true;
	}

	private ZoneChangeList FindLocalChangeListMatchingServerChangeList(ZoneChangeList serverChangeList)
	{
		foreach (ZoneChangeList localChangeList in m_localChangeListHistory)
		{
			int localPredictedPos = localChangeList.GetPredictedPosition();
			foreach (ZoneChange change in localChangeList.GetChanges())
			{
				Entity localEntity = change.GetEntity();
				TAG_ZONE localDstZoneTag = change.GetDestinationZoneTag();
				TAG_ZONE localSrcZoneTag = change.GetSourceZoneTag();
				if (localDstZoneTag == TAG_ZONE.INVALID)
				{
					continue;
				}
				bool zoneChanged = localSrcZoneTag != localDstZoneTag;
				List<ZoneChange> serverChanges = serverChangeList.GetChanges();
				for (int i = 0; i < serverChanges.Count; i++)
				{
					ZoneChange serverChange = serverChanges[i];
					Entity serverEntity = serverChange.GetEntity();
					if (localEntity != serverEntity)
					{
						continue;
					}
					if (zoneChanged)
					{
						TAG_ZONE serverDstZoneTag = serverChange.GetDestinationZoneTag();
						if (localDstZoneTag != serverDstZoneTag)
						{
							continue;
						}
						if (localDstZoneTag == TAG_ZONE.PLAY && localEntity.HasTag(GAME_TAG.TRANSFORMED_FROM_CARD) && localEntity.GetTag(GAME_TAG.TRANSFORMED_FROM_CARD) != localEntity.GetTag(GAME_TAG.DATABASE_ID))
						{
							int lastAffectedBy = localEntity.GetTag(GAME_TAG.LAST_AFFECTED_BY);
							Entity lastAffectedByEntity = GameState.Get().GetEntity(lastAffectedBy);
							if (lastAffectedByEntity != null && GameUtils.TranslateCardIdToDbId(lastAffectedByEntity.GetCardId()) == 61187)
							{
								continue;
							}
						}
					}
					int serverDstPos = FindNextDstPosChange(serverChangeList, i, serverEntity)?.GetDestinationPosition() ?? serverEntity.GetZonePosition();
					if (localPredictedPos == serverDstPos)
					{
						return localChangeList;
					}
				}
			}
		}
		return null;
	}

	private ZoneChange FindNextDstPosChange(ZoneChangeList changeList, int index, Entity entity)
	{
		List<ZoneChange> changes = changeList.GetChanges();
		for (int i = index; i < changes.Count; i++)
		{
			ZoneChange change = changes[i];
			if (change.HasDestinationZoneChange() && i != index)
			{
				return null;
			}
			if (change.HasDestinationPosition())
			{
				if (change.GetEntity() != entity)
				{
					return null;
				}
				return change;
			}
		}
		return null;
	}

	private bool ReplaceRemoteWeaponInServerChangeList(ZoneChangeList serverChangeList)
	{
		List<ZoneChange> serverChanges = serverChangeList.GetChanges();
		List<ZoneChange> list = serverChanges.FindAll(delegate(ZoneChange change)
		{
			if (!(change.GetDestinationZone() is ZoneWeapon))
			{
				return false;
			}
			PowerTask powerTask = change.GetPowerTask();
			return (powerTask == null || !powerTask.IsCompleted()) ? true : false;
		});
		bool hasWeaponChange = false;
		foreach (ZoneChange item in list)
		{
			Zone weaponZone = item.GetDestinationZone();
			if (weaponZone.GetCardCount() == 0)
			{
				continue;
			}
			Entity weaponEntity = weaponZone.GetCardAtIndex(0).GetEntity();
			bool toBeDestroyed = false;
			foreach (ZoneChange item2 in serverChanges)
			{
				PowerTask powerTask2 = item2.GetPowerTask();
				if (powerTask2 != null && powerTask2.GetPower() is Network.HistTagChange tagChange && tagChange.Entity == weaponEntity.GetEntityId() && tagChange.Tag == 360 && tagChange.Value > 0)
				{
					toBeDestroyed = true;
					break;
				}
			}
			if (toBeDestroyed)
			{
				int weaponControllerId = weaponEntity.GetControllerId();
				Zone graveyardZone = FindZoneForTags(weaponControllerId, TAG_ZONE.GRAVEYARD, TAG_CARDTYPE.WEAPON, weaponEntity);
				ZoneChange graveyardChange = new ZoneChange();
				graveyardChange.SetEntity(weaponEntity);
				graveyardChange.SetDestinationZone(graveyardZone);
				graveyardChange.SetDestinationZoneTag(TAG_ZONE.GRAVEYARD);
				graveyardChange.SetDestinationPosition(0);
				graveyardChange.SetParentList(serverChangeList);
				Log.Zone.Print("ZoneMgr.ReplaceRemoteWeaponInServerChangeList() - AddChange() serverChangeList: {0}, graveyardChange: {1}", serverChangeList, graveyardChange);
				serverChangeList.AddChange(graveyardChange);
				hasWeaponChange = true;
			}
		}
		return hasWeaponChange;
	}

	private HashSet<int> GetLastStandingMinions(ZoneChangeList serverChangeList)
	{
		Network.HistTagChange gameOverTagChange = GameState.Get().GetRealTimeGameOverTagChange();
		if (gameOverTagChange == null)
		{
			return null;
		}
		bool friendlySideLost = gameOverTagChange.Value != 4;
		bool enemySideLost = gameOverTagChange.Value == 4;
		HashSet<int> friendlyEntitiesLeavingPlay = new HashSet<int>();
		HashSet<int> enemyEntitiesLeavingPlay = new HashSet<int>();
		foreach (ZoneChange change in serverChangeList.GetChanges())
		{
			Entity entity = change.GetEntity();
			if (!entity.IsMinion())
			{
				continue;
			}
			bool num = entity.GetZone() == TAG_ZONE.PLAY;
			bool isLeavingPlay = change.HasDestinationZoneTag() && change.GetDestinationZoneTag() != TAG_ZONE.PLAY;
			if (num && isLeavingPlay)
			{
				Player.Side side = entity.GetControllerSide();
				if (side == Player.Side.FRIENDLY && friendlySideLost)
				{
					friendlyEntitiesLeavingPlay.Add(entity.GetEntityId());
				}
				else if (side == Player.Side.OPPOSING && enemySideLost)
				{
					enemyEntitiesLeavingPlay.Add(entity.GetEntityId());
				}
			}
		}
		bool checkFriendlySide = friendlyEntitiesLeavingPlay.Count != 0;
		bool checkEnemySide = enemyEntitiesLeavingPlay.Count != 0;
		if (!checkFriendlySide && !checkEnemySide)
		{
			return null;
		}
		bool hasFutureFriendlyPlayZoneChanges = false;
		bool hasFutureEnemyPlayZoneChanges = false;
		foreach (PowerTaskList item in GameState.Get().GetPowerProcessor().GetPowerQueue())
		{
			bool done = false;
			foreach (PowerTask task in item.GetTaskList())
			{
				Network.PowerHistory power = task.GetPower();
				if (!IsHandledPower(power))
				{
					continue;
				}
				ZoneChange change2 = null;
				switch (power.Type)
				{
				case Network.PowerType.FULL_ENTITY:
				{
					Network.HistFullEntity fullEntity = (Network.HistFullEntity)power;
					change2 = CreateZoneChangeFromFullEntity(fullEntity);
					break;
				}
				case Network.PowerType.SHOW_ENTITY:
				{
					Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
					change2 = CreateZoneChangeFromEntity(showEntity.Entity);
					break;
				}
				case Network.PowerType.CHANGE_ENTITY:
				{
					Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)power;
					change2 = CreateZoneChangeFromEntity(changeEntity.Entity);
					break;
				}
				case Network.PowerType.HIDE_ENTITY:
				{
					Network.HistHideEntity hideEntity = (Network.HistHideEntity)power;
					change2 = CreateZoneChangeFromHideEntity(hideEntity);
					break;
				}
				case Network.PowerType.TAG_CHANGE:
				{
					Network.HistTagChange tagChange = (Network.HistTagChange)power;
					change2 = CreateZoneChangeFromTagChange(tagChange);
					break;
				}
				case Network.PowerType.META_DATA:
				{
					Network.HistMetaData metaData = (Network.HistMetaData)power;
					change2 = CreateZoneChangeFromMetaData(metaData);
					break;
				}
				}
				if (change2 == null)
				{
					continue;
				}
				Entity entity2 = change2.GetEntity();
				if (!entity2.IsMinion())
				{
					continue;
				}
				bool num2 = entity2.GetZone() == TAG_ZONE.PLAY && change2.HasDestinationZoneTag() && change2.GetDestinationZoneTag() != TAG_ZONE.PLAY;
				bool isEnteringPlay = change2.HasDestinationZoneTag() && change2.GetDestinationZoneTag() == TAG_ZONE.PLAY;
				if (num2 || isEnteringPlay)
				{
					switch (entity2.GetControllerSide())
					{
					case Player.Side.FRIENDLY:
						hasFutureFriendlyPlayZoneChanges = true;
						break;
					case Player.Side.OPPOSING:
						hasFutureEnemyPlayZoneChanges = true;
						break;
					}
				}
				if ((!checkFriendlySide || hasFutureFriendlyPlayZoneChanges) && (!checkEnemySide || hasFutureEnemyPlayZoneChanges))
				{
					done = true;
					break;
				}
			}
			if (done)
			{
				break;
			}
		}
		HashSet<int> lastStandingEntities = new HashSet<int>();
		if (friendlySideLost && !hasFutureFriendlyPlayZoneChanges)
		{
			lastStandingEntities.UnionWith(friendlyEntitiesLeavingPlay);
		}
		if (enemySideLost && !hasFutureEnemyPlayZoneChanges)
		{
			lastStandingEntities.UnionWith(enemyEntitiesLeavingPlay);
		}
		return lastStandingEntities;
	}

	private bool PreventLastStandingMinionFromLeavingPlay(ZoneChangeList serverChangeList)
	{
		if (!GameState.Get().GetGameEntity().HasTag(GAME_TAG.LETTUCE_KEEP_LAST_STANDING_MINION_ACTOR))
		{
			return false;
		}
		HashSet<int> lastStandingMinionEntityIDs = GetLastStandingMinions(serverChangeList);
		if (lastStandingMinionEntityIDs == null || lastStandingMinionEntityIDs.Count == 0)
		{
			return false;
		}
		List<ZoneChange> changesToRemove = new List<ZoneChange>();
		foreach (ZoneChange change in serverChangeList.GetChanges())
		{
			Entity entity = change.GetEntity();
			if (lastStandingMinionEntityIDs.Contains(entity.GetEntityId()))
			{
				if (change.HasDestinationPosition())
				{
					changesToRemove.Add(change);
				}
				else if (change.HasDestinationZone())
				{
					changesToRemove.Add(change);
				}
				else if (change.GetPowerTask()?.GetPower() is Network.HistTagChange { Tag: 44, Value: 0 })
				{
					changesToRemove.Add(change);
				}
			}
		}
		foreach (ZoneChange change2 in changesToRemove)
		{
			change2.GetPowerTask()?.SetCompleted(complete: true);
			serverChangeList.RemoveChange(change2);
		}
		return true;
	}

	private bool MergeServerChangeList(ZoneChangeList serverChangeList)
	{
		Log.Zone.Print("ZoneMgr.MergeServerChangeList() Start - serverChangeList: {0}, m_tempZoneMap.Count: {1}, m_tempEntityMap.Count: {2}", serverChangeList, m_tempZoneMap.Count, m_tempEntityMap.Count);
		foreach (Zone zone in m_zones)
		{
			if (IsZoneInLocalHistory(zone))
			{
				TempZone tempZone = BuildTempZone(zone);
				m_tempZoneMap[zone] = tempZone;
				tempZone.PreprocessChanges();
			}
		}
		List<ZoneChange> serverChanges = serverChangeList.GetChanges();
		for (int i = 0; i < serverChanges.Count; i++)
		{
			ZoneChange serverChange = serverChanges[i];
			TempApplyZoneChange(serverChange);
		}
		bool changed = false;
		foreach (TempZone tempZone2 in m_tempZoneMap.Values)
		{
			tempZone2.Sort();
			tempZone2.PostprocessChanges();
			Zone zone2 = tempZone2.GetZone();
			Log.Zone.Print("ZoneMgr.MergeServerChangeList() zone: {0}", zone2);
			foreach (Card card in zone2.GetCards())
			{
				Log.Zone.Print("\tzone card: {0}", card);
			}
			Log.Zone.Print("ZoneMgr.MergeServerChangeList() tempZone: {0}", tempZone2);
			foreach (Entity entity in tempZone2.GetEntities())
			{
				Log.Zone.Print("\ttempZone entity: {0}", entity);
			}
			for (int pos = 1; pos <= zone2.GetLastSlot(); pos++)
			{
				Card card2 = zone2.GetCardAtSlot(pos);
				if (!(card2 == null))
				{
					Entity entity2 = card2.GetEntity();
					if (card2.GetPredictedZonePosition() != 0 && !tempZone2.ContainsEntity(entity2.GetEntityId()))
					{
						int insertionPos = FindBestMinionInsertionPosition(tempZone2, pos - 1, pos + 1);
						Log.Zone.Print("ZoneMgr.MergeServerChangeList() InsertEntityAtSlot() - tempZone: {0}, insertionPos: {1}, entity: {2}", tempZone2, insertionPos, entity2);
						tempZone2.InsertEntityAtSlot(insertionPos, entity2, bypassCanAcceptEntityCheck: true);
					}
				}
			}
			if (!tempZone2.IsModified())
			{
				continue;
			}
			changed = true;
			for (int sortPos = 1; sortPos <= tempZone2.GetLastSlot(); sortPos++)
			{
				Entity tempEntity = tempZone2.GetEntityAtSlot(sortPos);
				if (tempEntity != null)
				{
					Entity entity3 = tempEntity.GetCard().GetEntity();
					ZoneChange posChange = new ZoneChange();
					posChange.SetEntity(entity3);
					posChange.SetDestinationZone(zone2);
					posChange.SetDestinationZoneTag(zone2.m_ServerTag);
					posChange.SetDestinationPosition(sortPos);
					posChange.SetParentList(serverChangeList);
					Log.Zone.Print("ZoneMgr.MergeServerChangeList() - AddChange() tempZone:{0}, serverChangeList: {1}, graveyardChange: {2}", tempZone2, serverChangeList, posChange);
					serverChangeList.AddChange(posChange);
				}
			}
		}
		m_tempZoneMap.Clear();
		m_tempEntityMap.Clear();
		return changed;
	}

	private bool IsZoneInLocalHistory(Zone zone)
	{
		foreach (ZoneChangeList item in m_localChangeListHistory)
		{
			foreach (ZoneChange change in item.GetChanges())
			{
				Zone srcZone = change.GetSourceZone();
				Zone dstZone = change.GetDestinationZone();
				if (zone == srcZone || zone == dstZone)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void TempApplyZoneChange(ZoneChange change)
	{
		Log.Zone.Print("ZoneMgr.TempApplyZoneChange() - change: {0}, changeList: {1}", change, change.GetParentList());
		Network.PowerHistory power = change.GetPowerTask().GetPower();
		Entity entity = change.GetEntity();
		Entity tempEntity = RegisterTempEntity(entity);
		if (tempEntity == null)
		{
			return;
		}
		if (!change.HasDestinationZoneChange())
		{
			GameUtils.ApplyPower(tempEntity, power);
			return;
		}
		Zone srcZone = (change.HasSourceZone() ? change.GetSourceZone() : FindZoneForEntity(tempEntity));
		TempZone srcTempZone = FindTempZoneForZone(srcZone);
		if (srcTempZone != null)
		{
			bool result = srcTempZone.RemoveEntity(tempEntity);
			Log.Zone.Print("ZoneMgr.TempApplyZoneChange() - RemoveEntity() srcTempZone: {0}, tempEntity: {1}, result: {2}", srcTempZone, tempEntity, result);
		}
		GameUtils.ApplyPower(tempEntity, power);
		Zone dstZone = change.GetDestinationZone();
		TempZone dstTempZone = FindTempZoneForZone(dstZone);
		if (dstTempZone != null)
		{
			dstTempZone.AddEntity(tempEntity);
			Log.Zone.Print("ZoneMgr.TempApplyZoneChange() - AddEntity() dstTempZone: {0}, tempEntity: {1}", dstTempZone, tempEntity);
		}
	}

	private TempZone BuildTempZone(Zone zone)
	{
		TempZone tempZone = new TempZone();
		tempZone.SetZone(zone);
		List<Card> cards = zone.GetCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];
			if (card.GetPredictedZonePosition() == 0 && (!card.IsBeingDragged || zone is ZoneHand))
			{
				Entity entity = card.GetEntity();
				Entity tempEntity = RegisterTempEntity(entity);
				if (tempEntity != null)
				{
					tempZone.AddInitialEntity(tempEntity);
				}
			}
		}
		return tempZone;
	}

	private TempZone FindTempZoneForZone(Zone zone)
	{
		if (zone == null)
		{
			return null;
		}
		TempZone tempZone = null;
		m_tempZoneMap.TryGetValue(zone, out tempZone);
		return tempZone;
	}

	private int FindBestMinionInsertionPosition(TempZone tempZone, int leftPos, int rightPos)
	{
		Zone zone = tempZone.GetZone();
		int leftEntityPos = 0;
		for (int i = leftPos; i >= 1; i--)
		{
			Card card = zone.GetCardAtSlot(i);
			if (!(card == null))
			{
				Entity entity = card.GetEntity();
				leftEntityPos = tempZone.FindEntityPosWithReplacements(entity.GetEntityId());
				if (leftEntityPos != 0)
				{
					break;
				}
			}
		}
		int bestLeftPos;
		if (leftEntityPos == 0)
		{
			bestLeftPos = 1;
		}
		else
		{
			Entity leftEntity = tempZone.GetEntityAtSlot(leftEntityPos);
			bestLeftPos = leftEntityPos + 1;
			if (leftEntity != null)
			{
				int leftEntityId = leftEntity.GetEntityId();
				for (; bestLeftPos <= tempZone.GetLastSlot(); bestLeftPos++)
				{
					Entity tempEntity = tempZone.GetEntityAtSlot(bestLeftPos);
					if (tempEntity == null || tempEntity.GetCreatorId() != leftEntityId || zone.ContainsCard(tempEntity.GetCard()))
					{
						break;
					}
				}
			}
		}
		int rightEntityPos = 0;
		for (int j = rightPos; j <= zone.GetLastSlot(); j++)
		{
			Card card2 = zone.GetCardAtSlot(j);
			if (!(card2 == null))
			{
				Entity entity2 = card2.GetEntity();
				rightEntityPos = tempZone.FindEntityPosWithReplacements(entity2.GetEntityId());
				if (rightEntityPos != 0)
				{
					break;
				}
			}
		}
		int bestRightPos;
		if (rightEntityPos <= 0)
		{
			bestRightPos = tempZone.GetLastSlot() + 1;
		}
		else
		{
			Entity rightEntity = tempZone.GetEntityAtSlot(rightEntityPos);
			bestRightPos = rightEntityPos - 1;
			if (rightEntity != null)
			{
				int rightEntityId = rightEntity.GetEntityId();
				while (bestRightPos > 0)
				{
					Entity tempEntity2 = tempZone.GetEntityAtSlot(bestRightPos);
					if (tempEntity2 == null || tempEntity2.GetCreatorId() == 0 || tempEntity2.GetCreatorId() != rightEntityId || zone.ContainsCard(tempEntity2.GetCard()))
					{
						break;
					}
					bestRightPos--;
				}
			}
			bestRightPos++;
		}
		return Mathf.CeilToInt(0.5f * (float)(bestLeftPos + bestRightPos));
	}

	private int ValidatePredictedMinion(Entity entity, TempZone tempZone, int predictedPos)
	{
		int maxSlots = 7;
		if (GameState.Get() != null)
		{
			maxSlots = GameState.Get().GetMaxFriendlySlotsPerPlayer(entity);
		}
		if (tempZone.GetZone().m_ServerTag != TAG_ZONE.PLAY)
		{
			return predictedPos;
		}
		Entity magneticTarget = tempZone.GetEntityAtSlot(predictedPos);
		if (magneticTarget != null && magneticTarget.CanBeMagnitizedBy(entity) && !magneticTarget.IsDormant())
		{
			return predictedPos;
		}
		if (tempZone.GetLastSlot() == maxSlots)
		{
			return -1;
		}
		return predictedPos;
	}

	public int GetNextLocalChangeListId()
	{
		int nextLocalChangeListId = m_nextLocalChangeListId;
		m_nextLocalChangeListId = ((m_nextLocalChangeListId == int.MaxValue) ? 1 : (m_nextLocalChangeListId + 1));
		return nextLocalChangeListId;
	}

	private int GetNextServerChangeListId()
	{
		int nextServerChangeListId = m_nextServerChangeListId;
		m_nextServerChangeListId = ((m_nextServerChangeListId == int.MaxValue) ? 1 : (m_nextServerChangeListId + 1));
		return nextServerChangeListId;
	}

	private int FindTriggeredActiveLocalChangeIndex(Card card)
	{
		for (int i = 0; i < m_activeLocalChangeLists.Count; i++)
		{
			if (m_activeLocalChangeLists[i].GetLocalTriggerCard() == card)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindTriggeredPendingLocalChangeIndex(Card card)
	{
		for (int i = 0; i < m_pendingLocalChangeLists.Count; i++)
		{
			if (m_pendingLocalChangeLists[i].GetLocalTriggerCard() == card)
			{
				return i;
			}
		}
		return -1;
	}

	private bool AutoCorrectZonesAfterServerChange(CancellationToken token)
	{
		if (HasActiveLocalChange())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasActiveLocalChange()");
			return false;
		}
		if (HasPendingLocalChange())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasPendingLocalChange()");
			return false;
		}
		if (HasActiveServerChange())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasActiveServerChange()");
			return false;
		}
		if (HasPendingServerChange())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasPendingServerChange()");
			return false;
		}
		if (HasPredictedPositions())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasPredictedPositions()");
			return false;
		}
		if (HasPredictedCards())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasPredictedCards()");
			return false;
		}
		if (HasPredictedMovedMinion())
		{
			Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange() - HasPredictedMovedMinion()");
			return false;
		}
		Log.Zone.Print("ZoneMgr.AutoCorrectZonesAfterServerChange()");
		AutoCorrectZones(token, ShouldIgnorePosChange());
		return true;
	}

	public CancellationToken GetCancellationToken()
	{
		return m_updateChangeCancelTokenSource.Token;
	}

	public void AutoCorrectZones(CancellationToken token, bool ignorePurePosChange)
	{
		ZoneChangeList changeList = null;
		List<Zone> list = FindZonesOfType<Zone>(Player.Side.FRIENDLY);
		list.Add(FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING));
		foreach (Zone zone in list)
		{
			if (GameState.Get().GetGameEntity() != null && !GameState.Get().GetGameEntity().ShouldAutoCorrectZone(zone))
			{
				continue;
			}
			foreach (Card card in zone.GetCards())
			{
				Entity entity = card.GetEntity();
				TAG_ZONE entityZoneTag = entity.GetZone();
				int entityZoneControllerId = entity.GetControllerId();
				int entityZonePos = entity.GetZonePosition();
				TAG_ZONE cardZoneTag = zone.m_ServerTag;
				int cardZoneControllerId = zone.GetControllerId();
				int cardZonePos = card.GetZonePosition();
				TAG_ZONE fakeZoneTag = entity.GetTag<TAG_ZONE>(GAME_TAG.FAKE_ZONE);
				if (fakeZoneTag != 0)
				{
					entityZoneTag = fakeZoneTag;
					entityZoneControllerId = cardZoneControllerId;
				}
				int fakeZonePos = entity.GetTag(GAME_TAG.FAKE_ZONE_POSITION);
				if (fakeZonePos > 0)
				{
					entityZonePos = fakeZonePos;
				}
				bool num = entityZoneTag == cardZoneTag;
				bool controllerCorrect = entityZoneControllerId == cardZoneControllerId;
				bool posCorrect = entityZonePos == 0 || entityZonePos == cardZonePos;
				if (!(num && controllerCorrect && posCorrect))
				{
					if (changeList == null)
					{
						int id = GetNextLocalChangeListId();
						Log.Zone.Print("ZoneMgr.AutoCorrectZones() CreateLocalChangeList - changeListId={0}", id);
						changeList = new ZoneChangeList();
						changeList.SetId(id);
						changeList.SetIgnoreCardZonePurePosChanges(ignorePurePosChange);
					}
					ZoneChange change = new ZoneChange();
					change.SetEntity(entity);
					change.SetSourcePosition(cardZonePos);
					change.SetDestinationZoneTag(entityZoneTag);
					change.SetDestinationZone(FindZoneForEntity(entity));
					change.SetDestinationControllerId(entityZoneControllerId);
					change.SetDestinationPosition(entityZonePos);
					Log.Zone.Print("ZoneMgr.AutoCorrectZones() - AddChange() changeList: {0}, change: {1}", changeList, change);
					changeList.AddChange(change);
				}
			}
		}
		if (changeList != null)
		{
			ProcessLocalChangeList(changeList, token);
		}
	}

	public void ProcessGeneratedLocalChangeLists(List<ZoneChangeList> generatedChangeLists, CancellationToken token)
	{
		foreach (ZoneChangeList changeList in generatedChangeLists)
		{
			int id = GetNextLocalChangeListId();
			changeList.SetId(id);
			ProcessLocalChangeList(changeList, token);
		}
	}

	public void OnHealingDoesDamageEntityMousedOver()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnHealingDoesDamageEntityMousedOver();
		}
	}

	public void OnHealingDoesDamageEntityMousedOut()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnHealingDoesDamageEntityMousedOut();
		}
	}

	public void OnLifestealDoesDamageEntityMousedOver()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnLifestealDoesDamageEntityMousedOver();
		}
	}

	public void OnLifestealDoesDamageEntityMousedOut()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnLifestealDoesDamageEntityMousedOut();
		}
	}

	public void OnHealingDoesDamageEntityEnteredPlay()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnHealingDoesDamageEntityEnteredPlay();
		}
	}

	public void OnLifestealDoesDamageEntityEnteredPlay()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnLifestealDoesDamageEntityEnteredPlay();
		}
	}

	public void OnSpellPowerEntityMousedOver(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnSpellPowerEntityMousedOver(spellSchool);
		}
	}

	public void OnSpellPowerEntityMousedOut(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnSpellPowerEntityMousedOut(spellSchool);
		}
	}

	public void OnDiedLastCombatMousedOver()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnDiedLastCombatMousedOver();
		}
	}

	public void OnDiedLastCombatMousedOut()
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnDiedLastCombatMousedOut();
		}
	}

	public void OnSpellPowerEntityEnteredPlay(TAG_SPELL_SCHOOL spellSchool = TAG_SPELL_SCHOOL.NONE)
	{
		foreach (Zone item in FindZonesForSide(Player.Side.FRIENDLY))
		{
			item.OnSpellPowerEntityEnteredPlay(spellSchool);
		}
	}

	public Entity GetLettuceAbilitiesSourceEntity()
	{
		return m_lettuceZoneController?.GetLettuceAbilitiesSourceEntity();
	}

	public void DisplayLettuceAbilitiesForEntity(Entity entity)
	{
		m_lettuceZoneController?.DisplayLettuceAbilitiesForEntity(entity);
	}

	public void DismissMercenariesAbilityTray()
	{
		m_lettuceZoneController?.ClearDisplayedLettuceAbilities();
	}

	public void TemporarilyDismissMercenariesAbilityTray()
	{
		m_lettuceZoneController?.ClearDisplayedLettuceAbilities(hideWeaknessSplats: false, cachePreviouslySelected: true);
	}

	public void DisplayLettuceAbilitiesForPreviouslySelectedCard()
	{
		m_lettuceZoneController?.DisplayLettuceAbilitiesForPreviouslySelectedCard();
	}

	public List<Card> GetDisplayedLettuceAbilityCards()
	{
		return m_lettuceZoneController?.GetDisplayedLettuceAbilityCards();
	}

	public bool IsMercenariesAbilityTrayVisible()
	{
		return m_lettuceZoneController?.GetAbilityTray()?.IsVisible() == true;
	}

	public LettuceZoneController GetLettuceZoneController()
	{
		return m_lettuceZoneController;
	}
}
