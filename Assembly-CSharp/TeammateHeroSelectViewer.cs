using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateHeroSelectViewer : TeammateViewer
{
	private Card m_chosenMulliganHero;

	private List<Actor> m_lockedLeftActors = new List<Actor>();

	private List<Card> m_mulliganHeroes = new List<Card>();

	private List<Actor> m_lockedRightActors = new List<Actor>();

	private Vector3 m_chosenHeroWaitPosition;

	private Vector3[] m_mulliganHeroPositions = new Vector3[4];

	private int m_heroCount = 4;

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		ZoneHand zoneHand = ZoneMgr.Get().FindZonesOfType<ZoneHand>(Player.Side.FRIENDLY).FirstOrDefault();
		m_chosenHeroWaitPosition = MulliganManager.Get().GetHeroWaitPosition(zoneHand) + m_teammateBoardPosition;
		CalculateHeroPositions(zoneHand, m_heroCount);
	}

	private void CalculateHeroPositions(ZoneHand zoneHand, int heroCount)
	{
		m_heroCount = heroCount;
		for (int i = 0; i < m_mulliganHeroPositions.Length; i++)
		{
			m_mulliganHeroPositions[i] = MulliganManager.Get().GetHeroSelectFinalPosition(i, m_heroCount, zoneHand) + m_teammateBoardPosition;
		}
	}

	public void UpdateHeroes(ReplaceBattlegroundMulliganHero packet)
	{
		int oldCardDBID = packet.OldHeroDatabaseId;
		int newCardDBID = packet.NewHeroDatabaseId;
		if (oldCardDBID == 0 || newCardDBID == 0)
		{
			return;
		}
		using DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(newCardDBID);
		if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
		{
			Log.Gameplay.PrintError("TeammateBoardViewer.UpdateHeroes(): Unable to load def for card ID {0}.", newCardDBID);
			return;
		}
		using DefLoader.DisposableFullDef oldCardDef = DefLoader.Get().GetFullDef(oldCardDBID);
		foreach (Card heroCard in m_mulliganHeroes)
		{
			if (!heroCard.HasSameCardDef(oldCardDef.CardDef))
			{
				continue;
			}
			Actor actor = heroCard.GetActor();
			if (!(actor == null))
			{
				actor.SetEntityDef(cardDef.EntityDef);
				Entity entity = actor.GetEntity();
				if (entity != null)
				{
					entity.SetTag(GAME_TAG.ARMOR, cardDef.EntityDef.GetArmor());
					entity.SetTag(GAME_TAG.HERO_POWER, cardDef.EntityDef.GetTag(GAME_TAG.HERO_POWER));
				}
				heroCard.GetEntity().LoadCard(cardDef.EntityDef.GetCardId());
				heroCard.SetAlwaysShowCardsInTooltip(show: true);
				heroCard.CreateCardsInTooltip();
				actor.UpdateAllComponents();
			}
		}
	}

	public override bool IsActorInViewer(Actor actor)
	{
		if (actor == null)
		{
			return false;
		}
		return m_mulliganHeroes.Contains(actor.GetCard());
	}

	public override bool GetTeammateEntity(int entityID, out Entity teammateEntity)
	{
		teammateEntity = null;
		Card teammateCard = m_mulliganHeroes.Find((Card x) => x.GetEntity() != null && x.GetEntity().GetEntityId() == entityID);
		if (teammateCard == null)
		{
			return false;
		}
		teammateEntity = teammateCard.GetEntity();
		return true;
	}

	public Card GetSelectedHero()
	{
		return m_chosenMulliganHero;
	}

	private void CreateActorForMulligan(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		SetTagsForTeammateEntity(dummyEntity, entityData, cardDef.EntityDef.GetTags(), TAG_ZONE.HAND);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		dummyEntity.GetCard().SetActor(entityActor);
		SetActorStatsAndDefinitions(cardDef, entityData, entityActor);
		entityActor.SetUnlit();
		entityActor.GetMeshRenderer().gameObject.layer = 8;
		entityActor.GetHealthObject().Hide();
		GameState.Get().GetGameEntity().ApplyMulliganActorStateChanges(entityActor);
		entityActor.transform.localScale = GameState.Get().GetGameEntity().GetAlternateMulliganActorScale();
		entityActor.UpdateAllComponents();
		entityActor.GetCard().SetAlwaysShowCardsInTooltip(show: true);
		entityActor.GetCard().CreateCardsInTooltip();
	}

	private void CreateLockedActorForMulligan(Actor entityActor)
	{
		entityActor.SetUnlit();
		LayerUtils.SetLayer(entityActor.gameObject, GameLayer.Default);
		entityActor.GetMeshRenderer().gameObject.layer = 0;
		GameState.Get().GetGameEntity().ConfigureFakeMulliganCardActor(entityActor, shown: true);
		entityActor.transform.localScale = GameState.Get().GetGameEntity().GetAlternateMulliganActorScale();
	}

	private void UpdateMulliganEntities()
	{
		if (!(MulliganManager.Get() == null))
		{
			UpdateMulliganPositions(m_mulliganHeroes, m_lockedLeftActors, m_lockedRightActors);
			UpdateChosenMulliganPosition(m_chosenMulliganHero);
		}
	}

	private void UpdateMulliganPosition(GameObject go, int index)
	{
		go.transform.position = m_mulliganHeroPositions[index];
	}

	private void UpdateChosenMulliganPosition(GameObject go, bool tween = false)
	{
		iTween.Stop(go, includechildren: true);
		if (tween)
		{
			Vector3[] drawPath = new Vector3[3]
			{
				go.transform.position,
				new Vector3(go.transform.position.x, go.transform.position.y + 3.6f, go.transform.position.z),
				m_chosenHeroWaitPosition
			};
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", MulliganManager.ANIMATION_TIME_DEAL_CARD);
			moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
			iTween.MoveTo(go, moveArgs);
		}
		else
		{
			go.transform.position = m_chosenHeroWaitPosition;
		}
	}

	private void UpdateChosenMulliganPosition(Card card, bool tween = false)
	{
		if (card != null)
		{
			UpdateChosenMulliganPosition(card.gameObject, tween);
		}
	}

	private void UpdateMulliganPositions(List<Card> mulliganCards, List<Actor> lockedLeftActor, List<Actor> lockedRightActor)
	{
		int index = 0;
		if (lockedLeftActor != null)
		{
			foreach (Actor actor in lockedLeftActor)
			{
				if (actor != null)
				{
					UpdateMulliganPosition(actor.gameObject, index);
					index++;
				}
			}
		}
		if (mulliganCards != null)
		{
			foreach (Card card in mulliganCards)
			{
				if (card != null)
				{
					card.SetCardInTooltipDisplaySide(index >= 2, index < 2);
					UpdateMulliganPosition(card.gameObject, index);
					index++;
				}
			}
		}
		if (lockedRightActor == null)
		{
			return;
		}
		foreach (Actor actor2 in lockedRightActor)
		{
			if (actor2 != null)
			{
				UpdateMulliganPosition(actor2.gameObject, index);
				index++;
			}
		}
	}

	private Actor CreateMulliganHero(TeammateEntityData entityData)
	{
		if (GameState.Get().GetGameEntity() == null)
		{
			return null;
		}
		if (entityData == null)
		{
			Actor entityActor = AssetLoader.Get().InstantiatePrefab(GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME), AssetLoadingOptions.IgnorePrefabPosition).GetComponentInChildren<Actor>();
			entityActor.SetTeammateActor(isTeamamteActor: true);
			CreateLockedActorForMulligan(entityActor);
			return entityActor;
		}
		int cardDBID = entityData.CardDBID;
		if (cardDBID != 0)
		{
			using (DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(cardDBID))
			{
				if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
				{
					Log.Gameplay.PrintError("TeammateBoardViewer.CreateMulliganHero(): Unable to load def for card ID {0}.", cardDBID);
					return null;
				}
				Actor entityActor = AssetLoader.Get().InstantiatePrefab(GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME), AssetLoadingOptions.IgnorePrefabPosition).GetComponentInChildren<Actor>();
				entityActor.SetTeammateActor(isTeamamteActor: true);
				CreateActorForMulligan(entityData, entityActor, cardDef);
				return entityActor;
			}
		}
		return null;
	}

	public void AddMulliganEntitiesToViewer(TeammatesChooseEntities chooseEntities)
	{
		DeleteEntities(includeChosen: true);
		bool useLockedHeroActor = chooseEntities.Entities.Count < 4;
		if (useLockedHeroActor)
		{
			m_lockedLeftActors.Add(CreateMulliganHero(null));
		}
		foreach (TeammateEntityData entity in chooseEntities.Entities)
		{
			Actor actor = CreateMulliganHero(entity);
			if (actor != null)
			{
				m_mulliganHeroes.Add(actor.GetCard());
			}
		}
		if (useLockedHeroActor)
		{
			m_lockedRightActors.Add(CreateMulliganHero(null));
		}
		int totalHeroes = m_lockedLeftActors.Count + m_mulliganHeroes.Count + m_lockedRightActors.Count;
		if (m_heroCount != totalHeroes && ZoneMgr.Get() != null)
		{
			ZoneHand zoneHand = ZoneMgr.Get().FindZonesOfType<ZoneHand>(Player.Side.FRIENDLY).FirstOrDefault();
			CalculateHeroPositions(zoneHand, totalHeroes);
		}
		UpdateMulliganEntities();
	}

	public void ChooseEntitySelected(TeammatesEntitiesChosen entityChosen)
	{
		m_chosenMulliganHero = m_mulliganHeroes.Find((Card x) => x.GetEntity() != null && x.GetEntity().GetEntityId() == entityChosen.EntityID);
		UpdateChosenMulliganPosition(m_chosenMulliganHero, tween: true);
		GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(m_chosenMulliganHero, highlighted: false);
		GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(m_chosenMulliganHero, highlighted: false);
		Actor chosenHeroActor = m_chosenMulliganHero.GetActor();
		chosenHeroActor.RemovePing();
		chosenHeroActor.BlockPings(block: true);
		if (TeammatePingWheelManager.Get().GetActorWithActivePingWheel() == chosenHeroActor)
		{
			TeammatePingWheelManager.Get().HidePingOptions(chosenHeroActor);
		}
		DeleteEntities(includeChosen: false);
		m_mulliganHeroes.Clear();
		m_lockedLeftActors.Clear();
		m_lockedRightActors.Clear();
	}

	public void DeleteEntities(bool includeChosen)
	{
		if (includeChosen && m_chosenMulliganHero != null)
		{
			m_chosenMulliganHero.GetEntity()?.Destroy();
			m_chosenMulliganHero = null;
		}
		foreach (Card card in m_mulliganHeroes)
		{
			if (card != null && m_chosenMulliganHero != card && card.GetEntity() != null)
			{
				card.GetEntity().Destroy();
			}
		}
		foreach (Actor actor in m_lockedLeftActors)
		{
			if (actor != null)
			{
				actor.Destroy();
			}
		}
		foreach (Actor actor2 in m_lockedRightActors)
		{
			if (actor2 != null)
			{
				actor2.Destroy();
			}
		}
		m_mulliganHeroes.Clear();
		m_lockedLeftActors.Clear();
		m_lockedRightActors.Clear();
	}

	public void UpdateHeroHighlightedState(int entityId, bool isConfirmation)
	{
		Card targetCard = null;
		foreach (Card hero in m_mulliganHeroes)
		{
			if (hero.GetEntity().GetEntityId() == entityId)
			{
				targetCard = hero;
			}
		}
		if (targetCard == null)
		{
			if (isConfirmation)
			{
				foreach (Card hero2 in m_mulliganHeroes)
				{
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(hero2, highlighted: false);
				}
				return;
			}
			{
				foreach (Card hero3 in m_mulliganHeroes)
				{
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(hero3, highlighted: false);
				}
				return;
			}
		}
		if (isConfirmation)
		{
			GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(targetCard, highlighted: true);
			GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(targetCard, highlighted: false);
		}
		else
		{
			GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(targetCard, highlighted: true);
		}
		foreach (Card hero4 in m_mulliganHeroes)
		{
			if (!(hero4 == targetCard))
			{
				if (isConfirmation)
				{
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(hero4, highlighted: false);
				}
				GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(hero4, highlighted: false);
			}
		}
	}
}
