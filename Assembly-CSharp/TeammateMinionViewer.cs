using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateMinionViewer : TeammateViewer
{
	private const string MINION_REFRESH_SPAWN_SPELL = "Bacon_MinionSwap_CustomSpawnIn.prefab:d28086cf426282f45b333290f3b2e49d";

	private ZonePlay m_friendlyPlayZone;

	private ZonePlay m_opposingPlayZone;

	private Vector3 m_friendlyZoneCenter;

	private Vector3 m_opposingZoneCenter;

	private int m_friendlyMinionsInPlay;

	private int m_opposingMinionsZonePosOffset;

	private int m_opposingMinionsInPlay;

	private Spell m_refreshSpawnSpell;

	private Dictionary<int, Actor> m_entityMovedToBoard = new Dictionary<int, Actor>();

	public override void SetupViewerSpells(Component owner)
	{
		m_refreshSpawnSpell = SpellUtils.LoadAndSetupSpell("Bacon_MinionSwap_CustomSpawnIn.prefab:d28086cf426282f45b333290f3b2e49d", owner);
	}

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		m_friendlyPlayZone = ZoneMgr.Get().FindZonesOfType<ZonePlay>(Player.Side.FRIENDLY).FirstOrDefault();
		m_friendlyZoneCenter = m_friendlyPlayZone.GetComponent<Collider>().bounds.center + m_teammateBoardPosition;
		m_opposingPlayZone = ZoneMgr.Get().FindZonesOfType<ZonePlay>(Player.Side.OPPOSING).FirstOrDefault();
		m_opposingZoneCenter = m_opposingPlayZone.GetComponent<Collider>().bounds.center + m_teammateBoardPosition;
	}

	public override bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		if (entityData.Zone == 1)
		{
			return DoesCardAppearOnBoard((TAG_CARDTYPE)entityData.Type);
		}
		return false;
	}

	public override void AddEntityToViewer(TeammateEntityData entityData)
	{
		if (m_entityActors.ContainsKey(entityData.EntityID))
		{
			Actor entityActor = m_entityActors[entityData.EntityID];
			UpdateActorInPlay(entityData, entityActor);
		}
		else
		{
			LoadActor(entityData);
		}
		BuildMinionEnchantmentBanner(entityData, m_entityActors[entityData.EntityID]);
	}

	public void TrackZoneTransitions(TeammatesEntities teammatesEntities, TeammateHandViewer handViewer)
	{
		m_entityMovedToBoard.Clear();
		if (!TeammateBoardViewer.Get().IsViewingTeammate())
		{
			return;
		}
		foreach (TeammateEntityData entityPacket in teammatesEntities.Entities)
		{
			if (entityPacket.Zone == 1 && entityPacket.Type == 4 && handViewer.GetTeammateEntity(entityPacket.EntityID, out var entity))
			{
				m_entityMovedToBoard[entityPacket.EntityID] = entity.GetCard().GetActor();
				handViewer.RemoveEntityActor(entityPacket.EntityID);
			}
		}
	}

	protected override void RemovedActor(Actor actor)
	{
		if (!(actor == null))
		{
			Card card = actor.GetCard();
			if (!(card == null))
			{
				card.CancelCustomSpells();
			}
		}
	}

	public override void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		if (m_entityMovedToBoard.TryGetValue(entityData.EntityID, out var handActor))
		{
			UpdateActorInPlay(entityData, entityActor, handActor.GetCard(), cardDef);
		}
		else
		{
			CreateActorInPlay(entityData, entityActor, cardDef);
		}
	}

	private void BuildMinionEnchantmentBanner(TeammateEntityData entityData, Actor actor)
	{
		Entity entity = actor.GetEntity();
		List<Entity> oldEnchants = entity.GetEnchantments();
		foreach (Enchantment enchamentData in entityData.Enchantments)
		{
			Entity enchantEntity = oldEnchants.Find((Entity x) => x.GetEntityId() == enchamentData.EntityID);
			if (enchantEntity == null)
			{
				DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(enchamentData.CardDBID);
				enchantEntity = new Entity();
				enchantEntity.InitCard();
				enchantEntity.ReplaceTags(cardDef.EntityDef.GetTags());
				enchantEntity.LoadCard(cardDef.EntityDef.GetCardId());
				enchantEntity.SetTag(GAME_TAG.ENTITY_ID, enchamentData.EntityID);
				enchantEntity.SetTags(enchamentData.Tags);
				enchantEntity.SetEnchantmentPortraitCardID(enchantEntity.GetTag(GAME_TAG.CREATOR_DBID));
				entity.AddAttachment(enchantEntity);
			}
			else
			{
				enchantEntity.SetTags(enchamentData.Tags);
			}
		}
	}

	protected override bool SetActorPosition(Actor actor, bool fromSpawn = false)
	{
		int zonePos = 0;
		Entity entity = actor.GetEntity();
		Vector3 position = new Vector3(0f, 0f, 0f);
		bool isFriendlySide = entity.GetTag(GAME_TAG.CONTROLLER) == GameState.Get().GetFriendlyPlayerId();
		bool viewingTeammate = TeammateBoardViewer.Get().IsViewingTeammate();
		ZonePlay zonePlay = (isFriendlySide ? m_friendlyPlayZone : m_opposingPlayZone);
		if (zonePlay == null)
		{
			return true;
		}
		Vector3 scale = zonePlay.transform.localScale;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			scale *= 1.15f;
		}
		if (entity.HasTag(GAME_TAG.ZONE_POSITION))
		{
			zonePos = entity.GetTag(GAME_TAG.ZONE_POSITION) - (isFriendlySide ? 1 : m_opposingMinionsZonePosOffset);
		}
		position = ((!isFriendlySide) ? GetCardPositionInPlay(zonePlay, m_opposingZoneCenter, zonePos, m_opposingMinionsInPlay) : GetCardPositionInPlay(zonePlay, m_friendlyZoneCenter, zonePos, m_friendlyMinionsInPlay));
		Card card = actor.GetCard();
		if (!viewingTeammate)
		{
			iTween.Stop(card.gameObject, includechildren: true);
			card.transform.position = position;
			card.transform.localScale = scale;
			return true;
		}
		if (m_entityMovedToBoard.TryGetValue(entity.GetEntityId(), out var handActor))
		{
			actor.SetVisibility(isVisible: false, isInternal: false);
			TweenCardInPlay(card, position, scale, new Vector3(0f, 0f, 0f), zonePlay.GetTransitionTime());
			card.ActivateActorSpells_HandToPlay(handActor);
			m_entityMovedToBoard.Remove(entity.GetEntityId());
			return false;
		}
		if (fromSpawn)
		{
			actor.SetVisibility(isVisible: false, isInternal: false);
			PlaySpawnSpell(actor.GetCard(), position);
			return false;
		}
		TweenCardInPlay(card, position, zonePlay.GetTransitionTime());
		return true;
	}

	private Vector3 GetCardPositionInPlay(ZonePlay zonePlay, Vector3 zoneCenter, int index, int cardsInZone)
	{
		if (index < 0)
		{
			return zoneCenter;
		}
		float halfOneCardWidth = 0.5f * zonePlay.GetSlotWidth(cardsInZone);
		float halfTotalWidth = (float)cardsInZone * halfOneCardWidth;
		float num = zoneCenter.x - halfTotalWidth + halfOneCardWidth;
		float slotPosition = index;
		float zoneCenterX = num + slotPosition * zonePlay.GetSlotWidth(cardsInZone);
		return new Vector3(zoneCenterX, zoneCenter.y, zoneCenter.z);
	}

	private void TweenCardInPlay(Card card, Vector3 position, float moveTime)
	{
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", position);
		moveArgs.Add("delay", card.GetTransitionDelay());
		moveArgs.Add("time", moveTime);
		moveArgs.Add("name", ZoneMgr.Get().GetTweenName<ZonePlay>());
		iTween.MoveTo(card.gameObject, moveArgs);
	}

	private void TweenCardInPlay(Card card, Vector3 position, Vector3 scale, Vector3 rotation, float moveTime)
	{
		TweenCardInPlay(card, position, moveTime);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", scale);
		scaleArgs.Add("delay", card.GetTransitionDelay());
		scaleArgs.Add("time", moveTime);
		scaleArgs.Add("name", ZoneMgr.Get().GetTweenName<ZonePlay>());
		iTween.ScaleTo(card.gameObject, scaleArgs);
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("rotation", rotation);
		rotateArgs.Add("delay", card.GetTransitionDelay());
		rotateArgs.Add("time", moveTime);
		rotateArgs.Add("name", ZoneMgr.Get().GetTweenName<ZonePlay>());
		iTween.RotateTo(card.gameObject, rotateArgs);
	}

	public override void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
		CalculateMinionsInPlay(teammatesEntities.Entities);
	}

	private void CalculateMinionsInPlay(List<TeammateEntityData> teammateEntities)
	{
		m_friendlyMinionsInPlay = 0;
		m_opposingMinionsInPlay = 0;
		m_opposingMinionsZonePosOffset = 1;
		foreach (TeammateEntityData entity in teammateEntities)
		{
			if (entity.Zone != 1 || !DoesCardAppearOnBoard((TAG_CARDTYPE)entity.Type))
			{
				continue;
			}
			if (entity.Friendly)
			{
				m_friendlyMinionsInPlay++;
				continue;
			}
			m_opposingMinionsInPlay++;
			if (entity.HasZonePos && entity.ZonePos == 0)
			{
				m_opposingMinionsZonePosOffset = 0;
			}
		}
	}

	private void PlaySpawnSpell(Card card, Vector3 position)
	{
		card.transform.position = position;
		card.OverrideCustomSpawnSpell(SpellManager.Get().GetSpell(m_refreshSpawnSpell));
		card.ActivateMinionSpawnEffects();
	}
}
