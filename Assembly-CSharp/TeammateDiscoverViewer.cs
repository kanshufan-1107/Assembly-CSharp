using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

public class TeammateDiscoverViewer : TeammateViewer
{
	private ChoiceCardMgr.ChoiceState m_discoverCardsState = new ChoiceCardMgr.ChoiceState();

	private NormalButton m_discoverToggleButton;

	private bool m_discoverChoicesShown;

	private Actor m_chosenActor;

	private BaconTrinketBacker m_trinketShopBacker;

	public Actor GetChosenActor()
	{
		return m_chosenActor;
	}

	public void ClearChosenActor()
	{
		m_chosenActor = null;
	}

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		InitTeammateChoiceCard();
	}

	public override void RemoveActors(TeammatesEntities teammatesEntities)
	{
	}

	public override void CreateActor(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		CreateActorForDiscover(entityData, entityActor, cardDef);
	}

	private void InitTeammateChoiceCard()
	{
		string boneName = ChoiceCardMgr.Get().GetToggleButtonBoneName();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			boneName += "_phone";
		}
		m_discoverToggleButton = ChoiceCardMgr.Get().CreateChoiceButton(boneName, TeammateToggleButton_OnPress, TeammateToggleButton_OnRelease, GameStrings.Get("GLOBAL_HIDE"));
		m_discoverToggleButton.gameObject.AddComponent<TeammateGameObject>();
		m_discoverToggleButton.transform.position = m_discoverToggleButton.transform.position + m_teammateBoardPosition;
		SetToggleButtonActive(m_discoverToggleButton, active: false);
		GameObject trinketShopBackerObject = Object.Instantiate(ChoiceCardMgr.Get().m_ChoiceData.m_MagicItemShopBackgroundPrefab);
		trinketShopBackerObject.transform.position = new Vector3(trinketShopBackerObject.transform.position.x - 100f, trinketShopBackerObject.transform.position.y, trinketShopBackerObject.transform.position.z);
		if (trinketShopBackerObject != null)
		{
			m_trinketShopBacker = trinketShopBackerObject.GetComponent<BaconTrinketBacker>();
			m_trinketShopBacker.m_isTeammate = true;
		}
		ToggleTrinketShopActive(active: false);
	}

	private void TeammateToggleButton_OnPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("UI_MouseClick_01.prefab:fa537702a0db1c3478c989967458788b");
	}

	private void TeammateToggleButton_OnRelease(UIEvent e)
	{
		ShowTeammatesDiscoverEntites(!m_discoverChoicesShown);
	}

	private void ToggleTrinketShopActive(bool active)
	{
		if (!(m_trinketShopBacker == null))
		{
			m_trinketShopBacker.Show(active);
		}
	}

	private void SetToggleButtonActive(NormalButton button, bool active)
	{
		if (!(button == null))
		{
			button.gameObject.SetActive(active);
			Spell spell = button.m_button.GetComponent<Spell>();
			if (!(spell == null) && active)
			{
				spell.ActivateState(SpellStateType.BIRTH);
			}
		}
	}

	private void ShowTeammatesDiscoverEntites(bool show)
	{
		UpdateDiscoverToggleButton(show);
		if (m_discoverCardsState != null && m_discoverCardsState.m_isMagicItemDiscover)
		{
			ToggleTrinketShopActive(show);
		}
		foreach (KeyValuePair<int, Actor> entityActor in m_entityActors)
		{
			if (entityActor.Value != null)
			{
				entityActor.Value.gameObject.SetActive(show);
				if (entityActor.Value.GetCard() != null && entityActor.Value.GetCard().GetQuestRewardActor() != null)
				{
					entityActor.Value.GetCard().GetQuestRewardActor().gameObject.SetActive(show);
				}
			}
		}
	}

	private void UpdateDiscoverToggleButton(bool show)
	{
		if (!show)
		{
			m_discoverToggleButton.SetText(GameStrings.Get("GLOBAL_SHOW"));
		}
		else
		{
			m_discoverToggleButton.SetText(GameStrings.Get("GLOBAL_HIDE"));
		}
		m_discoverChoicesShown = show;
	}

	private void CreateActorForDiscover(TeammateEntityData entityData, Actor entityActor, DefLoader.DisposableFullDef cardDef)
	{
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		SetTagsForTeammateEntity(dummyEntity, entityData, cardDef.EntityDef.GetTags(), TAG_ZONE.SETASIDE);
		dummyEntity.SetTag(GAME_TAG.ARMOR, 0);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		dummyEntity.GetCard().SetActor(entityActor);
		SetActorStatsAndDefinitions(cardDef, entityData, entityActor);
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		entityActor.GetCard().ActivateStateSpells();
		if (dummyEntity.IsQuest())
		{
			entityActor.GetCard().UseBattlegroundQuestComponent();
			entityActor.GetCard().UpdateRewardActor();
			entityActor.UpdateAllComponents();
			entityActor.GetCard().ApplyTeammateViewUpdatesForQuestDiscover(entityData);
		}
	}

	public void AddDiscoverEntitiesToViewer(TeammatesChooseEntities chooseEntities)
	{
		DeleteEntities(includeChosen: true);
		m_discoverCardsState.m_isFriendly = true;
		m_discoverCardsState.m_isMagicItemDiscover = false;
		if (chooseEntities.Entities.Count > 0)
		{
			int cardDBID = chooseEntities.Entities[0].CardDBID;
			using DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(cardDBID);
			if (cardDef != null && cardDef.EntityDef != null && cardDef.EntityDef.GetCardType() == TAG_CARDTYPE.BATTLEGROUND_TRINKET)
			{
				m_discoverCardsState.m_isMagicItemDiscover = true;
			}
		}
		foreach (TeammateEntityData entity in chooseEntities.Entities)
		{
			Actor actor = LoadActor(entity, fromDiscover: true);
			m_discoverCardsState.m_cards.Add(actor.GetCard());
		}
		ChoiceCardMgr.Get().PopulateTransformDatas(m_discoverCardsState);
		int cardCount = m_discoverCardsState.m_cards.Count;
		for (int i = 0; i < cardCount; i++)
		{
			Card card = m_discoverCardsState.m_cards[i];
			ChoiceCardMgr.TransformData transformData = m_discoverCardsState.m_cardTransforms[i];
			card.transform.position = transformData.Position + m_teammateBoardPosition;
			card.transform.rotation = Quaternion.Euler(transformData.RotationAngles);
			card.transform.localScale = transformData.LocalScale;
		}
		if (m_discoverCardsState.m_isMagicItemDiscover)
		{
			ToggleTrinketShopActive(active: true);
		}
		SetToggleButtonActive(m_discoverToggleButton, cardCount > 0);
		UpdateDiscoverToggleButton(show: true);
	}

	public void ChooseEntitySelected(TeammatesEntitiesChosen entityChosen)
	{
		m_chosenActor = m_entityActors[entityChosen.EntityID];
		if (m_chosenActor != null)
		{
			m_chosenActor.gameObject.SetActive(value: true);
		}
		ToggleTrinketShopActive(active: false);
		SetToggleButtonActive(m_discoverToggleButton, active: false);
		DeleteEntities(includeChosen: false);
		m_entityActors.Clear();
	}

	public void DeleteEntities(bool includeChosen)
	{
		if (includeChosen && m_chosenActor != null)
		{
			m_chosenActor.GetEntity().Destroy();
			m_chosenActor = null;
		}
		foreach (KeyValuePair<int, Actor> entityActor in m_entityActors)
		{
			if (m_chosenActor != entityActor.Value)
			{
				entityActor.Value.GetEntity().Destroy();
			}
		}
		m_discoverCardsState.m_cards.Clear();
	}

	public override void PostAddingEntitiesToViewer()
	{
		if (m_chosenActor != null)
		{
			m_chosenActor.GetEntity().Destroy();
			m_chosenActor = null;
		}
	}
}
