using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateSecretViewer : TeammateViewer
{
	private ZoneSecret teammateSecretZone;

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		teammateSecretZone = ZoneMgr.Get().FindZonesOfType<ZoneSecret>(Player.Side.TEAMMATE_FRIENDLY).FirstOrDefault();
		teammateSecretZone.SetOriginalPosition(teammateSecretZone.transform.localPosition + m_teammateBoardPosition);
	}

	public override bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		return entityData.Zone == 7;
	}

	public override void AddEntityToViewer(TeammateEntityData entityData)
	{
		if (m_entityActors.ContainsKey(entityData.EntityID))
		{
			Actor entityActor = m_entityActors[entityData.EntityID];
			UpdateActorInSecretZone(entityData, entityActor);
		}
		else
		{
			LoadActor(entityData);
		}
	}

	public override void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		CreateActorInSecretZone(entityData, entityActor, cardDef);
	}

	private void CreateActorInSecretZone(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		SetTagsForTeammateEntity(dummyEntity, entityData, cardDef.EntityDef.GetTags(), TAG_ZONE.SECRET);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		dummyEntity.GetCard().SetActor(entityActor);
		dummyEntity.GetCard().SetAlwaysAllowTooptip(!dummyEntity.IsQuest());
		SetActorStatsAndDefinitions(cardDef, entityData, entityActor);
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		dummyEntity.GetCard().ActivateHandStateSpells();
		entityActor.GetCard().ShowSecretQuestBirth();
		entityActor.GetCard().SetTransitionStyle(ZoneTransitionStyle.INSTANT);
		teammateSecretZone.AddCard(entityActor.GetCard());
		entityActor.GetCard().SetZone(teammateSecretZone);
		entityActor.GetCard().ShowExhaustedChange(exhausted: true);
	}

	private void UpdateActorInSecretZone(TeammateEntityData entityData, Actor entityActor)
	{
		Entity dummyEntity = entityActor.GetEntity();
		TagMap oldTagMap = new TagMap();
		oldTagMap.SetTags(dummyEntity.GetTags().GetMap());
		SetTagsForTeammateEntity(dummyEntity, entityData, dummyEntity.GetEntityDef().GetTags(), TAG_ZONE.SECRET);
		SetActorStats(entityData, entityActor);
		TagMap newTagMap = dummyEntity.GetTags();
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		dummyEntity.GetCard().ActivateHandStateSpells();
		(from x in oldTagMap.GetMap()
			where !newTagMap.GetMap().ContainsKey(x.Key)
			select x).ToList().ForEach(delegate(KeyValuePair<int, int> x)
		{
			newTagMap.GetMap().Add(x.Key, 0);
		});
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

	protected override void RemovedActor(Actor actor)
	{
		teammateSecretZone.RemoveCard(actor.GetCard());
	}

	public override void PostAddingEntitiesToViewer()
	{
		teammateSecretZone.SetController(GameState.Get().GetFriendlySidePlayer());
		teammateSecretZone.UpdateLayout();
	}

	public override void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
	}
}
