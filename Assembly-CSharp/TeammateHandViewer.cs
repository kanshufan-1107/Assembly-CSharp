using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateHandViewer : TeammateViewer
{
	private Vector3 m_zoneCenter;

	private ZoneHand m_handZone;

	private Zone m_zoneTeammateHand;

	private float m_mousedOverHeight;

	private bool m_teammatesHandEnlarged;

	private Vector3 m_enlargedCenter;

	private float m_maxWidth;

	private float m_startingScaleX;

	private int m_friendlyCardsInHand;

	private Card m_lastMousedOver;

	private Card m_lastClickedCardMobile;

	private List<CardStandIn> m_standIns = new List<CardStandIn>();

	private Dictionary<int, Actor> m_entityMovedToHand = new Dictionary<int, Actor>();

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		m_handZone = ZoneMgr.Get().FindZonesOfType<ZoneHand>(Player.Side.FRIENDLY).FirstOrDefault();
		m_zoneCenter = m_handZone.GetComponent<Collider>().bounds.center + m_teammateBoardPosition;
		m_maxWidth = m_handZone.GetComponent<Collider>().bounds.size.x;
		m_startingScaleX = m_handZone.gameObject.transform.localScale.x;
		m_enlargedCenter = m_handZone.gameObject.transform.position - m_handZone.gameObject.transform.localPosition + m_handZone.m_enlargedHandPosition + m_teammateBoardPosition;
		m_mousedOverHeight = m_handZone.transform.Find("MouseOverCardHeight").position.z;
		m_zoneTeammateHand = ZoneMgr.Get().FindZonesOfType<ZoneTeammateHand>(Player.Side.FRIENDLY).FirstOrDefault();
	}

	public override bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		return entityData.Zone == 3;
	}

	public override void AddEntityToViewer(TeammateEntityData entityData)
	{
		if (m_entityActors.ContainsKey(entityData.EntityID))
		{
			Actor entityActor = m_entityActors[entityData.EntityID];
			UpdateActorInHand(entityData, entityActor);
		}
		else
		{
			LoadActor(entityData);
		}
	}

	public void TrackZoneTransitions(TeammatesEntities teammatesEntities, TeammateMinionViewer minionViewer, TeammateDiscoverViewer discoverViewer)
	{
		m_entityMovedToHand.Clear();
		if (!TeammateBoardViewer.Get().IsViewingTeammate())
		{
			return;
		}
		foreach (TeammateEntityData entityPacket in teammatesEntities.Entities)
		{
			if (entityPacket.Zone != 3)
			{
				continue;
			}
			if (minionViewer.GetTeammateEntity(entityPacket.EntityID, out var entity))
			{
				m_entityMovedToHand[entityPacket.EntityID] = entity.GetCard().GetActor();
				minionViewer.RemoveEntityActor(entityPacket.EntityID);
				continue;
			}
			Actor discoverChosenActor = discoverViewer.GetChosenActor();
			if (discoverChosenActor != null && discoverChosenActor.GetEntity().GetEntityId() == entityPacket.EntityID)
			{
				entity = discoverChosenActor.GetEntity();
				m_entityActors[entityPacket.EntityID] = entity.GetCard().GetActor();
				entity.SetTag(GAME_TAG.ZONE, TAG_ZONE.HAND);
				discoverViewer.ClearChosenActor();
			}
		}
	}

	private Vector3 GetZoneCenter()
	{
		if (m_teammatesHandEnlarged)
		{
			return m_enlargedCenter;
		}
		return m_zoneCenter;
	}

	private void SetHandEnlarged(bool enlarged)
	{
		m_teammatesHandEnlarged = enlarged;
		foreach (KeyValuePair<int, Actor> entityActor2 in m_entityActors)
		{
			SetActorPosition(entityActor2.Value);
		}
	}

	public bool IsHandEnlarged()
	{
		return m_teammatesHandEnlarged;
	}

	public void ClickedTeamamtesHandPhone(Card card)
	{
		if (!m_teammatesHandEnlarged)
		{
			SetHandEnlarged(enlarged: true);
		}
		else
		{
			m_lastClickedCardMobile = card;
		}
	}

	public void ReleaseTeammatesCardInHand()
	{
		m_lastClickedCardMobile = null;
	}

	public void ShrinkHand()
	{
		if ((bool)UniversalInputManager.UsePhoneUI && m_teammatesHandEnlarged)
		{
			SetHandEnlarged(enlarged: false);
		}
	}

	public override void UpdateViewer()
	{
		ZoneHand zone = m_handZone;
		if (zone == null)
		{
			return;
		}
		Card mousedOver = zone.GetMousedOverCard();
		Vector3 centerOfHand = GetZoneCenter();
		if (UniversalInputManager.Get().IsTouchMode() && m_teammatesHandEnlarged)
		{
			mousedOver = m_lastClickedCardMobile;
		}
		if (m_lastMousedOver == mousedOver)
		{
			return;
		}
		if (m_lastMousedOver != null)
		{
			Vector3 oldPosition = GetMouseOverCardPosition(m_lastMousedOver, centerOfHand);
			int zonePos = m_lastMousedOver.GetEntity().GetTag(GAME_TAG.ZONE_POSITION) - 1;
			Vector3 rotation = zone.GetCardRotation(zonePos, m_friendlyCardsInHand);
			Vector3 scale = zone.GetCardScale();
			Vector3 newerPosition = GetCardPositionInHand(zone, centerOfHand, zonePos, m_friendlyCardsInHand, rotation.y);
			iTween.Stop(m_lastMousedOver.gameObject, includechildren: true);
			m_lastMousedOver.transform.position = new Vector3(oldPosition.x, centerOfHand.y, newerPosition.z + 0.5f);
			m_lastMousedOver.transform.localScale = scale;
			m_lastMousedOver.transform.localEulerAngles = rotation;
			if ((bool)UniversalInputManager.UsePhoneUI && TeammatePingWheelManager.Get() != null && TeammatePingWheelManager.Get().GetActorWithActivePingWheel() == m_lastMousedOver.GetActor())
			{
				TeammatePingWheelManager.Get().HidePingOptions(m_lastMousedOver.GetActor());
			}
			LayerUtils.SetLayer(m_lastMousedOver.gameObject, GameLayer.Default);
			TweenCardInHand(m_lastMousedOver, newerPosition, scale, rotation, 0.5f);
			TooltipPanelManager.Get().HideKeywordHelp();
			m_lastMousedOver = null;
		}
		if (!(mousedOver == null) && IsActorInViewer(mousedOver.GetActor()))
		{
			float newXScale = zone.m_SelectCardScale;
			float newZScale = zone.m_SelectCardScale;
			Vector3 newScale = new Vector3(newXScale, zone.GetCardScale().y, newZScale);
			iTween.Stop(mousedOver.gameObject, includechildren: true);
			mousedOver.transform.position = GetMouseOverCardPosition(mousedOver, centerOfHand);
			mousedOver.transform.localScale = newScale;
			mousedOver.transform.localEulerAngles = Vector3.zero;
			if (ShouldShowTooltips(mousedOver))
			{
				TooltipPanelManager.TooltipBoneSource boneSource = (ShouldShowCardTooltipOnRight(mousedOver) ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
				TooltipPanelManager.Get().UpdateKeywordHelp(mousedOver, mousedOver.GetActor(), boneSource, null, null);
			}
			LayerUtils.SetLayer(mousedOver.gameObject, GameLayer.Tooltip);
			m_lastMousedOver = mousedOver;
		}
	}

	protected override void RemovedActor(Actor actor)
	{
		if (actor == null)
		{
			return;
		}
		Card card = actor.GetCard();
		if (!(card == null))
		{
			if (m_lastMousedOver == card)
			{
				m_lastMousedOver = null;
			}
			card.CancelCustomSpells();
			RemoveStandIn(actor.GetCard());
		}
	}

	public override void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		if (m_entityMovedToHand.TryGetValue(entityData.EntityID, out var playActor))
		{
			UpdateActorInHand(entityData, entityActor, playActor.GetCard(), cardDef);
		}
		else
		{
			CreateActorInHand(entityData, entityActor, cardDef);
		}
	}

	private void CreateActorInHand(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		SetTagsForTeammateEntity(dummyEntity, entityData, cardDef.EntityDef.GetTags(), TAG_ZONE.HAND);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		dummyEntity.GetCard().SetActor(entityActor);
		dummyEntity.GetCard().SetAlwaysAllowTooptip(allow: true);
		dummyEntity.GetCard().SetZone(m_zoneTeammateHand);
		SetActorPosition(entityActor, fromSpawn: true);
		SetActorStatsAndDefinitions(cardDef, entityData, entityActor);
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		dummyEntity.GetCard().ActivateHandStateSpells();
		LayerUtils.SetLayer(dummyEntity.GetCard().gameObject, GameLayer.Default);
	}

	private void UpdateActorInHand(TeammateEntityData entityData, Actor entityActor, Card parentCard = null, DefLoader.DisposableFullDef cardDef = null)
	{
		if (parentCard != null)
		{
			entityActor.SetCard(parentCard);
			entityActor.SetEntity(parentCard.GetEntity());
			entityActor.SetCardDef(cardDef.DisposableCardDef);
			parentCard.SetActor(entityActor);
		}
		Entity dummyEntity = entityActor.GetEntity();
		SetTagsForTeammateEntity(dummyEntity, entityData, dummyEntity.GetEntityDef().GetTags(), TAG_ZONE.HAND);
		dummyEntity.GetCard().SetZone(m_zoneTeammateHand);
		SetActorPosition(entityActor);
		SetActorStats(entityData, entityActor);
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		dummyEntity.GetCard().ActivateHandStateSpells();
		LayerUtils.SetLayer(dummyEntity.GetCard().gameObject, GameLayer.Default);
	}

	protected override bool SetActorPosition(Actor actor, bool fromSpawn = false)
	{
		Entity entity = actor.GetEntity();
		int zonePos = entity.GetTag(GAME_TAG.ZONE_POSITION) - 1;
		ZoneHand zoneHand = m_handZone;
		bool num = TeammateBoardViewer.Get().IsViewingTeammate();
		Vector3 rotation = zoneHand.GetCardRotation(zonePos, m_friendlyCardsInHand);
		Vector3 scale = zoneHand.GetCardScale();
		Vector3 position = GetCardPositionInHand(zoneHand, GetZoneCenter(), zonePos, m_friendlyCardsInHand, rotation.y);
		Card card = actor.GetCard();
		UpdateStandIn(card, position, scale, rotation);
		if (!num)
		{
			iTween.Stop(card.gameObject, includechildren: true);
			card.transform.position = position;
			card.transform.localScale = scale;
			card.transform.localEulerAngles = rotation;
			return true;
		}
		if (m_entityMovedToHand.TryGetValue(entity.GetEntityId(), out var playActor))
		{
			card.CancelCustomSpells();
			TweenCardInHand(card, position, scale, rotation, 0.45f);
			playActor.Destroy();
			m_entityMovedToHand.Remove(entity.GetEntityId());
		}
		else if (fromSpawn)
		{
			card.transform.position = position;
			card.transform.localScale = scale;
			card.transform.localEulerAngles = rotation;
			card.ActivateHandSpawnSpell();
		}
		else
		{
			TweenCardInHand(card, position, scale, rotation, 0.25f);
		}
		return true;
	}

	private Vector3 GetCardPositionInHand(ZoneHand zoneHand, Vector3 centerOfHand, int index, int cardsInZone, float yRotation)
	{
		zoneHand.GetComponent<Collider>();
		float maxWidth = m_maxWidth;
		if (m_teammatesHandEnlarged)
		{
			maxWidth *= zoneHand.m_enlargedHandScale.x / m_startingScaleX;
		}
		float cardSpacing = zoneHand.GetDefaultCardSpacing(m_teammatesHandEnlarged);
		bool num = zoneHand.IsHandScrunched(cardsInZone);
		if (cardSpacing * (float)cardsInZone > maxWidth)
		{
			cardSpacing = maxWidth / (float)cardsInZone;
		}
		float newX = centerOfHand.x - cardSpacing / 2f * (float)(cardsInZone - 1 - index * 2);
		float newY = centerOfHand.y;
		float newZ = centerOfHand.z;
		float zCurving = Mathf.Pow((float)index + 0.5f - (float)cardsInZone / 2f, 2f) / (6f * (float)cardsInZone);
		if (!num)
		{
			zCurving = 0f;
		}
		float zOffset = 0f;
		if (yRotation > 0f)
		{
			zOffset = Mathf.Sin(Mathf.Abs(yRotation * ((float)Math.PI / 180f))) * (cardSpacing / 2f);
		}
		newZ -= zCurving + zOffset;
		return new Vector3(newX, newY, newZ);
	}

	private Vector3 GetMouseOverCardPosition(Card card, Vector3 centerOfHand)
	{
		ZoneHand zone = m_handZone;
		Vector3 position = card.transform.position;
		bool tinyHand = UniversalInputManager.UsePhoneUI;
		return new Vector3(position.x, centerOfHand.y + 1f, m_mousedOverHeight + (float)zone.m_SelectCardOffsetZ + (tinyHand ? zone.m_tinyHandMouseOverZOffset : 0f));
	}

	private void TweenCardInHand(Card card, Vector3 position, Vector3 scale, Vector3 rotation, float moveTime)
	{
		float delayTime = card.GetTransitionDelay();
		iTween.EaseType easeTypeToUse = iTween.EaseType.easeOutExpo;
		string tweenLabel = ZoneMgr.Get().GetTweenName<ZoneHand>();
		Hashtable tweenScaleArgs = iTweenManager.Get().GetTweenHashTable();
		tweenScaleArgs.Add("scale", scale);
		tweenScaleArgs.Add("delay", delayTime);
		tweenScaleArgs.Add("time", moveTime);
		tweenScaleArgs.Add("easetype", easeTypeToUse);
		tweenScaleArgs.Add("name", tweenLabel);
		iTween.ScaleTo(card.gameObject, tweenScaleArgs);
		Hashtable tweenRotateArgs = iTweenManager.Get().GetTweenHashTable();
		tweenRotateArgs.Add("rotation", rotation);
		tweenRotateArgs.Add("delay", delayTime);
		tweenRotateArgs.Add("time", moveTime);
		tweenRotateArgs.Add("easetype", easeTypeToUse);
		tweenRotateArgs.Add("name", tweenLabel);
		iTween.RotateTo(card.gameObject, tweenRotateArgs);
		Hashtable tweenMoveArgs = iTweenManager.Get().GetTweenHashTable();
		tweenMoveArgs.Add("position", position);
		tweenMoveArgs.Add("delay", delayTime);
		tweenMoveArgs.Add("time", moveTime);
		tweenMoveArgs.Add("easetype", easeTypeToUse);
		tweenMoveArgs.Add("name", tweenLabel);
		iTween.MoveTo(card.gameObject, tweenMoveArgs);
	}

	public override void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
		CalculateCardsInHand(teammatesEntities.Entities);
	}

	private void CalculateCardsInHand(List<TeammateEntityData> teammateEntities)
	{
		m_friendlyCardsInHand = 0;
		foreach (TeammateEntityData teammateEntity in teammateEntities)
		{
			if (teammateEntity.Zone == 3)
			{
				m_friendlyCardsInHand++;
			}
		}
	}

	public int GetCardsInHand()
	{
		return m_friendlyCardsInHand;
	}

	private void UpdateStandIn(Card card, Vector3 position, Vector3 scale, Vector3 rotation)
	{
		GameObject standInObj = null;
		foreach (CardStandIn standin in m_standIns)
		{
			if (standin.linkedCard == card)
			{
				standInObj = standin.gameObject;
				break;
			}
		}
		if (standInObj == null)
		{
			standInObj = AssetLoader.Get().InstantiatePrefab("Card_Collider_Standin.prefab:06f88b48f6884bf4cafbd6696a28ede4", AssetLoadingOptions.IgnorePrefabPosition);
			CardStandIn standIn = standInObj.GetComponent<CardStandIn>();
			standIn.linkedCard = card;
			m_standIns.Add(standIn);
		}
		standInObj.transform.position = position;
		standInObj.transform.localScale = scale;
		standInObj.transform.localEulerAngles = rotation;
	}

	private void RemoveStandIn(Card card)
	{
		foreach (CardStandIn standIn in m_standIns)
		{
			if (!(standIn.linkedCard != card))
			{
				UnityEngine.Object.Destroy(standIn.gameObject);
			}
		}
		m_standIns.RemoveAll((CardStandIn x) => x.linkedCard == card);
	}

	private bool ShouldShowTooltips(Card card)
	{
		if (card == null)
		{
			return false;
		}
		if (card.GetEntity() == null)
		{
			return true;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_teammatesHandEnlarged)
			{
				if (!card.ShouldShowCardsInTooltip())
				{
					return true;
				}
				int zonePosition = card.GetZonePosition();
				return Mathf.Abs((float)(m_friendlyCardsInHand + 1) / 2f - (float)zonePosition) <= 0.5f;
			}
			return !card.ShouldShowCardsInTooltip();
		}
		return true;
	}

	private bool ShouldShowCardTooltipOnRight(Card card)
	{
		if (InputManager.Get().HasPlayFromMiniHandEnabled() && (bool)UniversalInputManager.UsePhoneUI && !m_teammatesHandEnlarged)
		{
			return false;
		}
		Entity entity = card.GetEntity();
		if (card.GetActor() == null || card.GetActor().GetMeshRenderer() == null)
		{
			return false;
		}
		int zonePosition = card.GetZonePosition();
		float middle = (float)(m_friendlyCardsInHand + 1) / 2f;
		bool tooltipsShouldGoRight = (float)zonePosition <= middle;
		if (entity.HasTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER) || entity.IsHero())
		{
			tooltipsShouldGoRight = !tooltipsShouldGoRight;
		}
		return tooltipsShouldGoRight;
	}
}
