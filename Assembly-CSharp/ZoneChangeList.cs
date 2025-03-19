#define ZONE_CHANGE_DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Blizzard.T5.Core;
using Cysharp.Threading.Tasks;
using PegasusGame;

public class ZoneChangeList
{
	private int m_id;

	private int m_predictedPosition;

	private bool m_ignoreCardZoneChanges;

	private bool m_canceledChangeList;

	private bool m_ignoreCardZonePurePosChanges;

	private PowerTaskList m_taskList;

	private List<ZoneChange> m_changes = new List<ZoneChange>();

	private HashSet<Zone> m_dirtyZones = new HashSet<Zone>();

	private List<ZoneChangeList> m_generatedLocalChangeLists = new List<ZoneChangeList>();

	private bool m_complete;

	private ZoneMgr.ChangeCompleteCallback m_completeCallback;

	private object m_completeCallbackUserData;

	public int GetId()
	{
		return m_id;
	}

	public void SetId(int id)
	{
		m_id = id;
	}

	public bool IsLocal()
	{
		return m_taskList == null;
	}

	public int GetPredictedPosition()
	{
		return m_predictedPosition;
	}

	public void SetPredictedPosition(int pos)
	{
		m_predictedPosition = pos;
	}

	public void SetIgnoreCardZoneChanges(bool ignore)
	{
		m_ignoreCardZoneChanges = ignore;
	}

	public void SetIgnoreCardZonePurePosChanges(bool ignore)
	{
		m_ignoreCardZonePurePosChanges = ignore;
	}

	public bool ShouldIgnoreCardZonePurePosChanges()
	{
		return m_ignoreCardZonePurePosChanges;
	}

	public bool IsCanceledChangeList()
	{
		return m_canceledChangeList;
	}

	public void SetCanceledChangeList(bool canceledChangeList)
	{
		m_canceledChangeList = canceledChangeList;
	}

	public void SetZoneInputBlocking(bool block)
	{
		for (int i = 0; i < m_changes.Count; i++)
		{
			ZoneChange zoneChange = m_changes[i];
			Zone srcZone = zoneChange.GetSourceZone();
			if (srcZone != null)
			{
				srcZone.BlockInput(block);
			}
			Zone dstZone = zoneChange.GetDestinationZone();
			if (dstZone != null)
			{
				dstZone.BlockInput(block);
			}
		}
	}

	public bool IsComplete()
	{
		return m_complete;
	}

	public void SetCompleteCallback(ZoneMgr.ChangeCompleteCallback callback)
	{
		m_completeCallback = callback;
	}

	public void SetCompleteCallbackUserData(object userData)
	{
		m_completeCallbackUserData = userData;
	}

	public void FireCompleteCallback()
	{
		DebugPrint("ZoneChangeList.FireCompleteCallback() - m_id={0} m_taskList={1} m_changes.Count={2} m_complete={3} m_completeCallback={4}", m_id, (m_taskList == null) ? "(null)" : m_taskList.GetId().ToString(), m_changes.Count, m_complete, (m_completeCallback == null) ? "(null)" : "(not null)");
		if (m_completeCallback != null)
		{
			m_completeCallback(this, m_completeCallbackUserData);
		}
	}

	public PowerTaskList GetTaskList()
	{
		return m_taskList;
	}

	public void SetTaskList(PowerTaskList taskList)
	{
		m_taskList = taskList;
	}

	public List<ZoneChange> GetChanges()
	{
		return m_changes;
	}

	public ZoneChange GetLocalTriggerChange()
	{
		if (!IsLocal())
		{
			return null;
		}
		if (m_changes.Count <= 0)
		{
			return null;
		}
		return m_changes[0];
	}

	public Card GetLocalTriggerCard()
	{
		return GetLocalTriggerChange()?.GetEntity().GetCard();
	}

	public void AddChange(ZoneChange change)
	{
		m_changes.Add(change);
	}

	public void RemoveChange(ZoneChange change)
	{
		m_changes.Remove(change);
	}

	public async UniTaskVoid ProcessChanges(CancellationToken token)
	{
		DebugPrint("ZoneChangeList.ProcessChanges() - m_id={0} m_taskList={1} m_changes.Count={2}", m_id, (m_taskList == null) ? "(null)" : m_taskList.GetId().ToString(), m_changes.Count);
		while (GameState.Get().MustWaitForChoices())
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		HashSet<Entity> loadingEntities = new HashSet<Entity>();
		Map<Player, DyingSecretGroup> dyingSecretMap = null;
		int num;
		for (int i = 0; i < m_changes.Count; num = i + 1, i = num)
		{
			ZoneChange change = m_changes[i];
			DebugPrint("ZoneChangeList.ProcessChanges() - processing index={0} change={1}", i, change);
			Entity entity = change.GetEntity();
			Card card = entity.GetCard();
			PowerTask powerTask = change.GetPowerTask();
			int srcControllerId = entity.GetControllerId();
			int srcPos = 0;
			Zone srcZone = null;
			if (card != null)
			{
				srcPos = card.GetZonePosition();
				srcZone = card.GetZone();
			}
			int dstControllerId = change.GetDestinationControllerId();
			int dstPos = change.GetDestinationPosition();
			Zone dstZone = change.GetDestinationZone();
			TAG_ZONE dstZoneTag = change.GetDestinationZoneTag();
			if (powerTask != null)
			{
				if (powerTask.IsCompleted())
				{
					continue;
				}
				if (loadingEntities.Contains(entity))
				{
					bool shouldWaitForLoading = true;
					if (entity.GetZonePosition() == 0 && entity.GetZone() == TAG_ZONE.PLAY && powerTask.GetPower() is Network.HistTagChange tagChange && tagChange.Entity == entity.GetEntityId() && tagChange.Tag == 263)
					{
						shouldWaitForLoading = false;
					}
					if (shouldWaitForLoading)
					{
						DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to load (powerTask=(not null))", card);
						await WaitForAndRemoveLoadingEntity(loadingEntities, entity, card, token);
						DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to load (powerTask=(not null))", card);
					}
				}
				while (!GameState.Get().GetPowerProcessor().CanDoTask(powerTask))
				{
					await UniTask.Yield(PlayerLoopTiming.Update, token);
				}
				while (ShouldWaitForOldHero(entity))
				{
					await UniTask.Yield(PlayerLoopTiming.Update, token);
				}
				powerTask.DoTask();
				if (entity.IsLoadingAssets())
				{
					loadingEntities.Add(entity);
				}
			}
			if (ShouldIgnoreZoneChange(entity))
			{
				continue;
			}
			bool zoneChanged = dstZoneTag != 0 && srcZone != dstZone;
			bool controllerChanged = dstControllerId != 0 && srcControllerId != dstControllerId;
			bool posChanged = zoneChanged || (dstPos != 0 && srcPos != dstPos);
			bool revealed = powerTask != null && powerTask.GetPower().Type == Network.PowerType.SHOW_ENTITY;
			if ((bool)UniversalInputManager.UsePhoneUI && IsDisplayableDyingSecret(entity, card, srcZone, dstZone))
			{
				if (dyingSecretMap == null)
				{
					dyingSecretMap = new Map<Player, DyingSecretGroup>();
				}
				Player controller = card.GetController();
				if (!dyingSecretMap.TryGetValue(controller, out var dyingSecretGroup))
				{
					dyingSecretGroup = new DyingSecretGroup();
					dyingSecretMap.Add(controller, dyingSecretGroup);
				}
				dyingSecretGroup.AddCard(card);
			}
			if (zoneChanged || controllerChanged || revealed)
			{
				bool transitionedZones = zoneChanged || controllerChanged;
				bool revealedSecret = revealed && entity.GetZone() == TAG_ZONE.SECRET;
				if (transitionedZones || !revealedSecret)
				{
					if (srcZone != null)
					{
						m_dirtyZones.Add(srcZone);
					}
					if (dstZone != null)
					{
						m_dirtyZones.Add(dstZone);
					}
					DebugPrint("ZoneChangeList.ProcessChanges() - TRANSITIONING card {0} to {1}", card, dstZone);
				}
				if (loadingEntities.Contains(entity))
				{
					DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to load (zoneChanged={1} controllerChanged={2} powerTask=(not null))", card, zoneChanged, controllerChanged);
					await WaitForAndRemoveLoadingEntity(loadingEntities, entity, card, token);
					DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to load (zoneChanged={1} controllerChanged={2} powerTask=(not null))", card, zoneChanged, controllerChanged);
				}
				if (!card.IsActorReady() || card.IsBeingDrawnByOpponent())
				{
					DebugPrint("ZoneChangeList.ProcessChanges() - START waiting for {0} to become ready (zoneChanged={1} controllerChanged={2} powerTask=(not null))", card, zoneChanged, controllerChanged);
					if (card.GetPrevZone() is ZoneDeck && card.GetZone() is ZoneHand && card.GetPrevZone().GetController() == card.GetZone().GetController() && TurnStartManager.Get().IsCardDrawHandled(card))
					{
						TurnStartManager.Get().DrawCardImmediately(card);
					}
					while (!card.IsActorReady() || card.IsBeingDrawnByOpponent())
					{
						await UniTask.Yield(PlayerLoopTiming.Update, token);
					}
					DebugPrint("ZoneChangeList.ProcessChanges() - END waiting for {0} to become ready (zoneChanged={1} controllerChanged={2} powerTask=(not null))", card, zoneChanged, controllerChanged);
				}
				Log.Zone.Print("ZoneChangeList.ProcessChanges() - id={0} local={1} {2} zone from {3} -> {4}", m_id, IsLocal(), card, srcZone, dstZone);
				if (transitionedZones)
				{
					if (srcZone is ZonePlay && srcZone.m_Side == Player.Side.OPPOSING && dstZone is ZoneHand && dstZone.m_Side == Player.Side.OPPOSING)
					{
						Log.FaceDownCard.Print("ZoneChangeList.ProcessChanges() - id={0} {1}.TransitionToZone(): {2} -> {3}", m_id, card, srcZone, dstZone);
						m_taskList.DebugDump(Log.FaceDownCard);
					}
					card.SetZonePosition(0);
					card.TransitionToZone(dstZone, change);
				}
				else if (revealed)
				{
					card.UpdateActor();
				}
				if (card.IsActorLoading())
				{
					loadingEntities.Add(entity);
				}
			}
			if (posChanged && (!ShouldIgnoreCardZonePurePosChanges() || srcZone != dstZone || entity.GetZone() != TAG_ZONE.PLAY))
			{
				if (srcZone != null && !zoneChanged && !controllerChanged)
				{
					m_dirtyZones.Add(srcZone);
				}
				if (dstZone != null)
				{
					m_dirtyZones.Add(dstZone);
				}
				if (card.m_minionWasMovedFromSrcToDst != null && !IsLocal())
				{
					GenerateLocalChangelistForMovedMinionWhileProcessingServerChangelist(card);
					continue;
				}
				Log.Zone.Print("ZoneChangeList.ProcessChanges() - id={0} local={1} {2} pos from {3} -> {4}", m_id, IsLocal(), card, srcPos, dstPos);
				card.SetZonePosition(dstPos);
			}
		}
		while (ShowNewHeroStats())
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		if (IsCanceledChangeList())
		{
			SetZoneInputBlocking(block: false);
		}
		ProcessDyingSecrets(dyingSecretMap);
		ZoneMgr.Get().ProcessGeneratedLocalChangeLists(m_generatedLocalChangeLists, token);
		UpdateDirtyZones(loadingEntities, token).Forget();
	}

	private void GenerateLocalChangelistForMovedMinionWhileProcessingServerChangelist(Card card)
	{
		if (!(card == null) && card.m_minionWasMovedFromSrcToDst != null)
		{
			ZoneChangeList changelist = new ZoneChangeList();
			ZoneChange change = new ZoneChange();
			change.SetEntity(card.GetEntity());
			change.SetSourcePosition(card.GetZonePosition());
			change.SetDestinationPosition(card.GetEntity().GetRealTimeZonePosition());
			Log.Zone.Print("ZoneMgr.GenerateLocalChangelistForMovedMinionWhileProcessingServerChangelist() - AddChange() changeList: {0}, change: {1}", changelist, change);
			changelist.AddChange(change);
			m_generatedLocalChangeLists.Add(changelist);
		}
	}

	public override string ToString()
	{
		return $"id={m_id} changes={m_changes.Count} complete={m_complete} local={IsLocal()} localTrigger=[{GetLocalTriggerChange()}]";
	}

	private bool IsDisplayableDyingSecret(Entity entity, Card card, Zone srcZone, Zone dstZone)
	{
		if (!entity.IsSecret())
		{
			return false;
		}
		if (!(srcZone is ZoneSecret))
		{
			return false;
		}
		if (!(dstZone is ZoneGraveyard))
		{
			return false;
		}
		return true;
	}

	private void ProcessDyingSecrets(Map<Player, DyingSecretGroup> dyingSecretMap)
	{
		if (dyingSecretMap == null)
		{
			return;
		}
		Map<Player, DeadSecretGroup> deadSecretMap = null;
		foreach (KeyValuePair<Player, DyingSecretGroup> pair in dyingSecretMap)
		{
			Player controller = pair.Key;
			DyingSecretGroup value = pair.Value;
			Card mainCard = value.GetMainCard();
			List<Card> cards = value.GetCards();
			List<Actor> oldActors = value.GetActors();
			for (int i = 0; i < cards.Count; i++)
			{
				Card card = cards[i];
				Actor oldActor = oldActors[i];
				if (card.WasSecretTriggered())
				{
					oldActor.Destroy();
					continue;
				}
				if (card == mainCard && card.CanShowSecretDeath())
				{
					card.ShowSecretDeath(oldActor);
				}
				else
				{
					oldActor.Destroy();
				}
				if (deadSecretMap == null)
				{
					deadSecretMap = new Map<Player, DeadSecretGroup>();
				}
				if (!deadSecretMap.TryGetValue(controller, out var deadGroup))
				{
					deadGroup = new DeadSecretGroup();
					deadGroup.SetMainCard(mainCard);
					deadSecretMap.Add(controller, deadGroup);
				}
				deadGroup.AddCard(card);
			}
		}
		BigCard.Get().ShowSecretDeaths(deadSecretMap);
	}

	private async UniTask WaitForAndRemoveLoadingEntity(HashSet<Entity> loadingEntities, Entity entity, Card card, CancellationToken token)
	{
		while (IsEntityLoading(entity, card))
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		loadingEntities.Remove(entity);
	}

	private bool IsEntityLoading(Entity entity, Card card)
	{
		if (entity.IsLoadingAssets())
		{
			return true;
		}
		if (card != null && card.IsActorLoading())
		{
			return true;
		}
		return false;
	}

	private async UniTaskVoid UpdateDirtyZones(HashSet<Entity> loadingEntities, CancellationToken token)
	{
		DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} loadingEntities.Count={1} m_dirtyZones.Count={2}", m_id, loadingEntities.Count, m_dirtyZones.Count);
		foreach (Entity entity in loadingEntities)
		{
			Card card = entity.GetCard();
			DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} START waiting for {1} to load (card={2})", m_id, entity, card);
			while (IsEntityLoading(entity, card))
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} END waiting for {1} to load (card={2})", m_id, entity, card);
		}
		if (IsDeathBlock())
		{
			float layoutDelaySec = ZoneMgr.Get().RemoveNextDeathBlockLayoutDelaySec();
			if (layoutDelaySec >= 0f)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(layoutDelaySec), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
			foreach (Zone dirtyZone2 in m_dirtyZones)
			{
				dirtyZone2.UpdateLayout();
			}
			m_dirtyZones.Clear();
		}
		else
		{
			Zone[] dirtyZones = new Zone[m_dirtyZones.Count];
			m_dirtyZones.CopyTo(dirtyZones);
			Zone[] array = dirtyZones;
			foreach (Zone dirtyZone in array)
			{
				DebugPrint("ZoneChangeList.UpdateDirtyZones() - m_id={0} START waiting for zone {1}", m_id, dirtyZone);
				if (dirtyZone is ZoneHand)
				{
					ZoneHand_UpdateLayout((ZoneHand)dirtyZone, token).Forget();
					continue;
				}
				dirtyZone.AddUpdateLayoutCompleteCallback(OnUpdateLayoutComplete);
				dirtyZone.UpdateLayout();
			}
		}
		FinishWhenPossible(token).Forget();
	}

	private bool IsDeathBlock()
	{
		if (m_taskList == null)
		{
			return false;
		}
		return m_taskList.IsDeathBlock();
	}

	private async UniTaskVoid ZoneHand_UpdateLayout(ZoneHand zoneHand, CancellationToken token)
	{
		while (!(zoneHand.GetCards().Find(delegate(Card card)
		{
			if (TurnStartManager.Get() != null && TurnStartManager.Get().IsCardDrawHandled(card))
			{
				return false;
			}
			return !card.IsDoNotSort() && !card.IsActorReady();
		}) == null))
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		zoneHand.AddUpdateLayoutCompleteCallback(OnUpdateLayoutComplete);
		zoneHand.UpdateLayout();
	}

	private void OnUpdateLayoutComplete(Zone zone, object userData)
	{
		DebugPrint("ZoneChangeList.OnUpdateLayoutComplete() - m_id={0} END waiting for zone {1}", m_id, zone);
		m_dirtyZones.Remove(zone);
	}

	private Entity GetNewHeroPlayedFromPowerTaskList()
	{
		PowerTaskList powerTaskList = GetTaskList();
		if (powerTaskList == null)
		{
			return null;
		}
		Network.HistBlockStart blockStart = powerTaskList.GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		if (blockStart.BlockType != HistoryBlock.Type.PLAY)
		{
			return null;
		}
		Entity source = powerTaskList.GetSourceEntity();
		if (source == null)
		{
			Log.Zone.PrintWarning("ZoneChangelist.GetNewHeroPlayedFromPowerTaskList() - source is null.");
			return null;
		}
		if (!source.IsHero())
		{
			return null;
		}
		return source;
	}

	private bool ShowNewHeroStats()
	{
		Entity newHero = GetNewHeroPlayedFromPowerTaskList();
		if (newHero != null)
		{
			if (!newHero.GetCard().IsActorReady())
			{
				return true;
			}
			Actor heroActor = newHero.GetCard().GetActor();
			heroActor.EnableArmorSpellAfterTransition();
			heroActor.ShowArmorSpell();
			heroActor.GetHealthObject().Show();
			heroActor.GetAttackObject().Show();
			if (newHero.GetATK() <= 0)
			{
				heroActor.GetAttackObject().ImmediatelyScaleToZero();
			}
		}
		return false;
	}

	private bool ShouldWaitForOldHero(Entity entity)
	{
		if (!entity.IsHero())
		{
			return false;
		}
		Entity newHero = GetNewHeroPlayedFromPowerTaskList();
		if (newHero == null)
		{
			return false;
		}
		if (newHero.GetEntityId() == entity.GetEntityId())
		{
			return false;
		}
		return !newHero.GetCard().IsActorReady();
	}

	private bool ShouldIgnoreZoneChange(Entity entity)
	{
		if (entity.GetCard() == null)
		{
			return true;
		}
		if (IsOldHero(entity))
		{
			return false;
		}
		Entity creator = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.CREATOR));
		if (creator != null && creator.HasReplacementsWhenPlayed())
		{
			return false;
		}
		return m_ignoreCardZoneChanges;
	}

	private bool IsOldHero(Entity entity)
	{
		Entity newHero = GetNewHeroPlayedFromPowerTaskList();
		if (newHero == null)
		{
			return false;
		}
		if (!entity.IsHero())
		{
			return false;
		}
		return newHero.GetEntityId() != entity.GetEntityId();
	}

	private async UniTaskVoid FinishWhenPossible(CancellationToken token)
	{
		while (m_dirtyZones.Count > 0)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		while (GameState.Get().IsBusy())
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		Finish();
	}

	private void Finish()
	{
		m_complete = true;
		Log.Zone.Print("ZoneChangeList.Finish() - {0}", this);
	}

	[Conditional("ZONE_CHANGE_DEBUG")]
	private void DebugPrint(string format, params object[] args)
	{
		Log.Zone.Print(format, args);
	}
}
