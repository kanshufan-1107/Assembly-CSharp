using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateHeroViewer : TeammateViewer
{
	private bool m_teammateHeroPowerDisabled;

	private Vector3 m_friendlyHeroPos;

	private Vector3 m_friendlyHeroScale;

	private Vector3 m_opposingHeroPos;

	private Vector3 m_opposingHeroScale;

	private Vector3[] m_heroPowerPos;

	private Vector3[] m_heroPowerScale;

	private Vector3 m_questRewardPos;

	private Vector3 m_questRewardScale;

	private Vector3 m_buddyButtonPos;

	private Vector3 m_buddyButtonScale;

	private Vector3 m_trinket1Pos;

	private Vector3 m_trinket1Scale;

	private Vector3 m_trinket2Pos;

	private Vector3 m_trinket2Scale;

	private Vector3 m_trinketHeropowerPos;

	private Vector3 m_trinketHeropowerScale;

	private Vector3 m_clickableButtonPos;

	private Vector3 m_clickableButtonScale;

	private Entity m_teammateHero;

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		Zone zone = ZoneMgr.Get().FindZonesOfType<ZoneHero>(Player.Side.FRIENDLY).FirstOrDefault();
		m_friendlyHeroPos = zone.transform.position + m_teammateBoardPosition;
		m_friendlyHeroScale = zone.transform.localScale;
		zone = ZoneMgr.Get().FindZonesOfType<ZoneHero>(Player.Side.OPPOSING).FirstOrDefault();
		m_opposingHeroPos = zone.transform.position + m_teammateBoardPosition;
		m_opposingHeroScale = zone.transform.localScale;
		List<ZoneHeroPower> heroPowerZones = ZoneMgr.Get().FindZonesOfType<ZoneHeroPower>(Player.Side.FRIENDLY);
		m_heroPowerPos = new Vector3[heroPowerZones.Count];
		m_heroPowerScale = new Vector3[heroPowerZones.Count];
		foreach (ZoneHeroPower zoneHeroPower in heroPowerZones)
		{
			int index = zoneHeroPower.m_heroPowerIndex;
			m_heroPowerPos[index] = zoneHeroPower.transform.position + m_teammateBoardPosition;
			m_heroPowerScale[index] = zoneHeroPower.transform.localScale;
		}
		zone = ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundQuestReward>(Player.Side.FRIENDLY).FirstOrDefault();
		m_questRewardPos = zone.transform.position + m_teammateBoardPosition;
		m_questRewardScale = zone.transform.localScale;
		zone = ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundClickableButton>(Player.Side.FRIENDLY).FirstOrDefault();
		m_clickableButtonPos = zone.transform.position + m_teammateBoardPosition;
		m_clickableButtonScale = zone.transform.localScale;
		zone = ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundHeroBuddy>(Player.Side.FRIENDLY).FirstOrDefault();
		m_buddyButtonPos = zone.transform.position + m_teammateBoardPosition;
		m_buddyButtonScale = zone.transform.localScale;
		foreach (ZoneBattlegroundTrinket trinketZone in ZoneMgr.Get().FindZonesOfType<ZoneBattlegroundTrinket>(Player.Side.FRIENDLY))
		{
			switch (trinketZone.slot)
			{
			case 1:
				m_trinket1Pos = trinketZone.transform.position + m_teammateBoardPosition;
				m_trinket1Scale = trinketZone.transform.localScale;
				break;
			case 2:
				m_trinket2Pos = trinketZone.transform.position + m_teammateBoardPosition;
				m_trinket2Scale = trinketZone.transform.localScale;
				break;
			case 3:
				m_trinketHeropowerPos = trinketZone.transform.position + m_teammateBoardPosition;
				m_trinketHeropowerScale = trinketZone.transform.localScale;
				break;
			default:
				Log.All.PrintWarning($"Unknown trinket zone slot: {trinketZone.slot}");
				break;
			}
		}
	}

	public override bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		if (entityData.Zone == 1)
		{
			if (entityData.Type != 3 && entityData.Type != 10 && entityData.Type != 40 && entityData.Type != 24)
			{
				return entityData.Type == 44;
			}
			return true;
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
	}

	public override void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		CreateActorInPlay(entityData, entityActor, cardDef);
		if (entityActor.GetEntity().GetCardType() == TAG_CARDTYPE.HERO && entityActor.GetEntity().IsControlledByFriendlySidePlayer() && (bool)m_duosPortal)
		{
			m_duosPortal.SetTeammateHeroActor(entityActor);
		}
	}

	protected override bool SetActorPosition(Actor actor, bool fromSpawn = false)
	{
		if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.HERO)
		{
			if (actor.GetEntity().IsControlledByFriendlySidePlayer())
			{
				actor.GetCard().transform.position = m_friendlyHeroPos;
				actor.GetCard().transform.localScale = m_friendlyHeroScale;
			}
			else
			{
				actor.GetCard().transform.position = m_opposingHeroPos;
				actor.GetCard().transform.localScale = m_opposingHeroScale;
			}
		}
		else if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.HERO_POWER || (actor.GetEntity().GetCardType() == TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD && actor.GetEntity().HasTag(GAME_TAG.BACON_IS_HEROPOWER_QUESTREWARD)))
		{
			int index = actor.GetEntity().GetTag(GAME_TAG.ADDITIONAL_HERO_POWER_INDEX);
			actor.GetCard().transform.position = m_heroPowerPos[index];
			actor.GetCard().transform.localScale = m_heroPowerScale[index];
		}
		else if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD)
		{
			actor.GetCard().transform.position = m_questRewardPos;
			actor.GetCard().transform.localScale = m_questRewardScale;
		}
		else if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY)
		{
			actor.GetCard().transform.position = m_buddyButtonPos;
			actor.GetCard().transform.localScale = m_buddyButtonScale;
		}
		else if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.BATTLEGROUND_TRINKET)
		{
			switch (actor.GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6))
			{
			case 1:
				actor.GetCard().transform.position = m_trinket1Pos;
				actor.GetCard().transform.localScale = m_trinket1Scale;
				break;
			case 2:
				actor.GetCard().transform.position = m_trinket2Pos;
				actor.GetCard().transform.localScale = m_trinket2Scale;
				break;
			case 3:
				actor.GetCard().transform.position = m_trinketHeropowerPos;
				actor.GetCard().transform.localScale = m_trinketHeropowerScale;
				break;
			default:
				Log.All.PrintWarning($"Unknown trinket zone slot: {actor.GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6)}");
				break;
			}
		}
		return true;
	}

	public override void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
		m_teammateHeroPowerDisabled = teammatesEntities.HeroPowerDisabled == 1;
	}

	protected override void SetViewerSpecificData(Actor actor)
	{
		if (actor.GetEntity().IsHeroPower() && m_teammateHeroPowerDisabled)
		{
			actor.GetEntity().SetTag(GAME_TAG.EXHAUSTED, tagValue: true);
		}
		if (actor.GetEntity().GetCardType() == TAG_CARDTYPE.HERO)
		{
			if (actor.GetEntity().IsControlledByFriendlySidePlayer())
			{
				m_teammateHero = actor.GetEntity();
			}
			else
			{
				actor.GetHealthObject().Hide();
			}
		}
		if (actor.GetEntity().IsBattlegroundHeroBuddy() && actor.GetEntityDef() != null)
		{
			actor.GetEntityDef().SetTag(GAME_TAG.COST, actor.GetEntity().GetTag(GAME_TAG.MODIFY_DEFINITION_COST));
		}
	}

	public Entity GetTeammateHero()
	{
		return m_teammateHero;
	}
}
