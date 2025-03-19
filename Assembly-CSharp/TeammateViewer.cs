using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateViewer : Object
{
	protected Vector3 m_teammateBoardPosition;

	protected ZoneTeammatePlay m_zoneTeammatePlay;

	protected DuosPortal m_duosPortal;

	protected Dictionary<int, Actor> m_entityActors = new Dictionary<int, Actor>();

	protected bool DoesCardAppearOnBoard(TAG_CARDTYPE cardType)
	{
		if (cardType != TAG_CARDTYPE.MINION)
		{
			return cardType == TAG_CARDTYPE.BATTLEGROUND_SPELL;
		}
		return true;
	}

	public void SetDuosPortal(DuosPortal duosPortal)
	{
		m_duosPortal = duosPortal;
	}

	public virtual void SetupViewerSpells(Component owner)
	{
	}

	public virtual void InitZones(Vector3 teammateBoardPos)
	{
		m_teammateBoardPosition = teammateBoardPos;
		m_zoneTeammatePlay = ZoneMgr.Get().FindZonesOfType<ZoneTeammatePlay>(Player.Side.FRIENDLY).FirstOrDefault();
	}

	public virtual void UpdateZonesLayouts()
	{
		foreach (KeyValuePair<int, Actor> entityActor2 in m_entityActors)
		{
			Actor actor = entityActor2.Value;
			SetActorPosition(actor);
		}
	}

	public virtual bool IsActorInViewer(Actor actor)
	{
		return m_entityActors.ContainsValue(actor);
	}

	public virtual bool GetTeammateEntity(int entityID, out Entity teammateEntity)
	{
		teammateEntity = null;
		if (m_entityActors.TryGetValue(entityID, out var teammatesActor))
		{
			teammateEntity = teammatesActor.GetEntity();
			return true;
		}
		return false;
	}

	public void RemoveEntityActor(int entityID)
	{
		RemovedActor(m_entityActors[entityID]);
		m_entityActors.Remove(entityID);
	}

	public virtual bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		return false;
	}

	public virtual void AddEntityToViewer(TeammateEntityData entityData)
	{
	}

	public virtual void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
	}

	public virtual void PostAddingEntitiesToViewer()
	{
	}

	public virtual void UpdateViewer()
	{
	}

	public virtual void RemoveActors(TeammatesEntities teammatesEntities)
	{
		if (m_entityActors.Count == 0)
		{
			return;
		}
		List<int> removeKeys = new List<int>();
		foreach (KeyValuePair<int, Actor> entityActor2 in m_entityActors)
		{
			removeKeys.Add(entityActor2.Key);
		}
		foreach (TeammateEntityData entityPacket in teammatesEntities.Entities)
		{
			if (m_entityActors.TryGetValue(entityPacket.EntityID, out var actor) && !(actor == null) && !(actor.gameObject == null))
			{
				Entity entity = actor.GetEntity();
				int databaseID = GameUtils.TranslateCardIdToDbId(entity.GetCardId());
				if (entity.GetZone() == (TAG_ZONE)entityPacket.Zone && databaseID == entityPacket.CardDBID)
				{
					removeKeys.Remove(entityPacket.EntityID);
				}
			}
		}
		foreach (int key in removeKeys)
		{
			if (m_entityActors.TryGetValue(key, out var actor2))
			{
				RemovedActor(actor2);
				actor2.GetEntity().Destroy();
			}
			m_entityActors.Remove(key);
		}
	}

	protected virtual void RemovedActor(Actor actor)
	{
	}

	public virtual void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
	}

	protected Actor LoadActor(TeammateEntityData entityData, bool fromDiscover = false)
	{
		int cardDBID = entityData.CardDBID;
		if (cardDBID != 0)
		{
			using (DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(cardDBID))
			{
				if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
				{
					Log.Gameplay.PrintError("TeammateBoardViewer.CreateActor(): Unable to load def for card ID {0}.", cardDBID);
					return null;
				}
				TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
				foreach (Tag tag in entityData.Tags)
				{
					if (tag.Name == 12)
					{
						premium = ((tag.Value == 1) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
					}
				}
				TAG_ZONE zone = (TAG_ZONE)entityData.Zone;
				if (fromDiscover && entityData.Type == 3)
				{
					zone = TAG_ZONE.PLAY;
				}
				GameObject actorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(cardDef.EntityDef, zone, GameState.Get().GetFriendlySidePlayer(), premium), AssetLoadingOptions.IgnorePrefabPosition);
				if (actorGO == null)
				{
					Log.Gameplay.PrintError("TeammateBoardViewer.CreateActor(): Unable to load Actor for entity def {0}.", cardDef.EntityDef);
					return null;
				}
				Actor entityActor = actorGO.GetComponentInChildren<Actor>();
				entityActor.SetTeammateActor(isTeamamteActor: true);
				if (entityActor != null)
				{
					CreateActor(entityData, entityActor, cardDef);
				}
				m_entityActors[entityData.EntityID] = entityActor;
				return entityActor;
			}
		}
		return null;
	}

	protected virtual bool SetActorPosition(Actor actor, bool fromSpawn = false)
	{
		return true;
	}

	protected void CreateActorInPlay(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		SetTagsForTeammateEntity(dummyEntity, entityData, cardDef.EntityDef.GetTags(), TAG_ZONE.PLAY);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		dummyEntity.GetCard().SetActor(entityActor);
		if (entityData.Type != 3)
		{
			dummyEntity.GetCard().SetAlwaysAllowTooptip(allow: true);
		}
		if (entityData.Type == 24)
		{
			entityActor.gameObject.GetComponent<HeroBuddyWidgetCoinBased>().SetHeroBuddyIDOverride(dummyEntity.GetTag(GAME_TAG.BACON_COMPANION_ID));
		}
		dummyEntity.GetCard().SetZone(m_zoneTeammatePlay);
		bool num = SetActorPosition(entityActor, fromSpawn: true);
		SetActorStatsAndDefinitions(cardDef, entityData, entityActor);
		if (entityActor.UseCoinManaGem() && !entityActor.GetEntity().HasTag(GAME_TAG.EXHAUSTED) && entityActor.GetCard().CanShowCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		if (num)
		{
			entityActor.UpdateAllComponents();
			entityActor.GetCard().ActivateStateSpells();
		}
	}

	protected void UpdateActorInPlay(TeammateEntityData entityData, Actor entityActor, Card parentCard = null, DefLoader.DisposableFullDef cardDef = null)
	{
		if (parentCard != null)
		{
			entityActor.SetCard(parentCard);
			entityActor.SetEntity(parentCard.GetEntity());
			entityActor.SetCardDef(cardDef.DisposableCardDef);
			parentCard.SetActor(entityActor);
		}
		Entity dummyEntity = entityActor.GetEntity();
		TagMap oldTagMap = new TagMap();
		oldTagMap.SetTags(dummyEntity.GetTags().GetMap());
		SetTagsForTeammateEntity(dummyEntity, entityData, dummyEntity.GetEntityDef().GetTags(), TAG_ZONE.PLAY);
		dummyEntity.GetCard().SetZone(m_zoneTeammatePlay);
		SetActorPosition(entityActor);
		SetActorStats(entityData, entityActor);
		TagMap newTagMap = dummyEntity.GetTags();
		if (entityActor.UseCoinManaGem() && !entityActor.GetEntity().HasTag(GAME_TAG.EXHAUSTED) && entityActor.GetCard().CanShowCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		(from x in oldTagMap.GetMap()
			where !newTagMap.GetMap().ContainsKey(x.Key)
			select x).ToList().ForEach(delegate(KeyValuePair<int, int> x)
		{
			newTagMap.GetMap().Add(x.Key, 0);
		});
		if (oldTagMap.GetTag(GAME_TAG.CONTROLLER) != newTagMap.GetTag(GAME_TAG.CONTROLLER))
		{
			entityActor.GetCard().ActivateStateSpells();
		}
		int oldDamage = oldTagMap.GetTag(GAME_TAG.DAMAGE);
		int tag = newTagMap.GetTag(GAME_TAG.DAMAGE);
		int oldArmor = oldTagMap.GetTag(GAME_TAG.ARMOR);
		int newArmor = newTagMap.GetTag(GAME_TAG.ARMOR);
		int damageTaken = tag - oldDamage + (oldArmor - newArmor);
		if (damageTaken > 0)
		{
			DoTeammateDamageSplat(entityActor, damageTaken);
		}
		foreach (KeyValuePair<int, int> newTagVale in newTagMap.GetMap())
		{
			if (newTagVale.Key != 12)
			{
				TagDelta change = new TagDelta();
				change.tag = newTagVale.Key;
				change.newValue = newTagVale.Value;
				change.oldValue = oldTagMap.GetTag(newTagVale.Key);
				if (change.oldValue != change.newValue)
				{
					TagVisualConfiguration.Get().ProcessTagChange((GAME_TAG)change.tag, entityActor.GetCard(), fromShowEntity: false, change);
				}
			}
		}
	}

	protected virtual void SetViewerSpecificData(Actor actor)
	{
	}

	protected void SetActorStatsAndDefinitions(DefLoader.DisposableFullDef cardDef, TeammateEntityData entityData, Actor entityActor)
	{
		SetViewerSpecificData(entityActor);
		entityActor.SetCardDef(cardDef.DisposableCardDef);
		if (entityData.DefinitionTags.Count > 0)
		{
			bool shouldUseDynamicDef = false;
			foreach (Tag tag in entityData.DefinitionTags)
			{
				if (cardDef.EntityDef.GetTag(tag.Name) != tag.Value)
				{
					shouldUseDynamicDef = true;
					break;
				}
			}
			if (shouldUseDynamicDef)
			{
				entityActor.GetEntity().GetOrCreateDynamicDefinition();
				foreach (Tag tag2 in entityData.DefinitionTags)
				{
					entityActor.GetEntity().GetEntityDef().SetTag(tag2.Name, tag2.Value);
				}
			}
		}
		entityActor.SetPremium(entityActor.GetEntity().GetPremiumType());
		if (entityData.HasHealth)
		{
			entityActor.GetEntity().SetTag(GAME_TAG.HEALTH, entityData.Health);
			entityActor.UpdateMinionHealthText(entityActor.GetEntity().GetEntityDef().GetHealth(), entityData.Health, 0);
		}
		if (entityData.HasAttack)
		{
			entityActor.GetEntity().SetTag(GAME_TAG.ATK, entityData.Attack);
			entityActor.UpdateMinionAtkText(entityActor.GetEntity().GetEntityDef().GetATK(), entityData.Attack);
		}
		UpdateCardTextBuilder(entityActor);
	}

	protected void SetActorStats(TeammateEntityData entityData, Actor entityActor)
	{
		EntityDef entityDef = entityActor.GetEntity().GetEntityDef();
		SetViewerSpecificData(entityActor);
		entityActor.SetPremium(entityActor.GetEntity().GetPremiumType());
		if (entityData.HasHealth)
		{
			int damage = entityActor.GetEntity().GetDamage();
			entityActor.GetEntity().SetTag(GAME_TAG.HEALTH, entityData.Health);
			entityActor.UpdateMinionHealthText(entityDef.GetHealth(), entityData.Health, damage, allowJiggle: true, entityDef.HasTag(GAME_TAG.HIDE_HEALTH_NUMBER));
		}
		if (entityData.HasAttack)
		{
			entityActor.GetEntity().SetTag(GAME_TAG.ATK, entityData.Attack);
			entityActor.UpdateMinionAtkText(entityDef.GetATK(), entityData.Attack, allowJiggle: true, entityDef.HasTag(GAME_TAG.HIDE_ATTACK_NUMBER));
		}
	}

	protected void SetTagsForTeammateEntity(Entity entity, TeammateEntityData entityData, TagMap defTags, TAG_ZONE zone)
	{
		entity.ReplaceTags(defTags);
		entity.SetTag(GAME_TAG.ENTITY_ID, entityData.EntityID);
		entity.SetTag(GAME_TAG.ZONE, zone);
		entity.SetTag(GAME_TAG.CONTROLLER, entityData.Friendly ? GameState.Get().GetFriendlyPlayerId() : GameState.Get().GetOpposingPlayerId());
		entity.SetTag(GAME_TAG.SUPPRES_ALL_SOUNDS_FOR_ENTITY, tagValue: true);
		entity.SetTag(GAME_TAG.CARDTYPE, entityData.Type);
		if (entityData.HasZonePos)
		{
			entity.SetTag(GAME_TAG.ZONE_POSITION, entityData.ZonePos);
		}
		entity.SetTags(entityData.Tags);
	}

	private void DoTeammateDamageSplat(Actor actor, int damage)
	{
		Spell spell = actor.GetSpell(SpellType.DAMAGE);
		if (!(spell == null))
		{
			DamageSplatSpell obj = (DamageSplatSpell)spell;
			obj.SetDamage(damage);
			obj.ActivateState(SpellStateType.ACTION);
		}
	}

	protected void UpdateCardTextBuilder(Actor actor)
	{
		if (actor.GetEntity() != null && actor.GetEntity().HasTag(GAME_TAG.OVERRIDECARDTEXTBUILDER))
		{
			if (actor.GetEntity().GetEntityDef() != null)
			{
				actor.GetEntity().GetEntityDef().ClearCardTextBuilder();
			}
			actor.UpdatePowersText();
		}
	}
}
