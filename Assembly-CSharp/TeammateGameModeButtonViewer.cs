using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PegasusGame;
using UnityEngine;

public class TeammateGameModeButtonViewer : TeammateViewer
{
	private class CrystalLoadedData
	{
		public TeammateGameModeButtonViewer viewer;
	}

	private readonly int MAX_DISPLAYED_MANA_CRYSTALS = 10;

	private readonly int MAX_DISPLAYED_MANA_CRYSTALS_PHONE;

	private readonly int TAVERN_TIER_BUTTON_SLOT = 3;

	private readonly int REFRESH_BUTTON_SLOT = 2;

	private readonly int FREEZE_BUTTON_SLOT = 1;

	private readonly int TAVERN_TIER_2_BUTTON_DBID = 59561;

	private readonly int REFRESH_BUTTON_DBID = 59559;

	private readonly int FREEZE_BUTTON_DBID = 59560;

	private Vector3 m_tavernTierPos;

	private Vector3 m_tavernTierScale;

	private Vector3 m_refreshPos;

	private Vector3 m_refreshScale;

	private Vector3 m_freezePos;

	private Vector3 m_freezeScale;

	private int m_teammateGold;

	private int m_teammateMaxGold;

	private int m_teammateTavernTierCost;

	private int m_teammateRefreshCost;

	private List<ManaCrystal> m_teammateManaCrystals = new List<ManaCrystal>();

	private GameObject m_teammateManaCrystalContainer;

	private GameObject m_teammateManaCrystalCounter;

	private int m_numLoadingCrystals;

	private float m_manaCrystalWidth;

	private Vector3 m_refreshButtonPosition;

	private Vector3 m_tavernButtonPosition;

	private Vector3 m_freezeButtonPosition;

	private Vector3 m_manaCrystalPosition;

	private Vector3 m_manaCounterPosition;

	private Actor m_refreshButtonDummyActor;

	private Actor m_tavernButtonDummyActor;

	private Actor m_freezeButtonDummyActor;

	private int m_refreshEntityID;

	private int m_tavernEntityID;

	private int m_freezeEntityID;

	private Notification m_techLevelCounter;

	private int m_displayedTechLevelNumber = 1;

	private Vector3 m_techLevelCounterPosition;

	private bool zonesInit;

	public override void InitZones(Vector3 teammateBoardPos)
	{
		base.InitZones(teammateBoardPos);
		Zone zone = (from z in ZoneMgr.Get().FindZonesOfType<ZoneGameModeButton>(Player.Side.FRIENDLY)
			where z.m_ButtonSlot == TAVERN_TIER_BUTTON_SLOT
			select z).FirstOrDefault();
		m_tavernTierPos = zone.gameObject.transform.position + m_teammateBoardPosition;
		m_tavernTierScale = zone.gameObject.transform.localScale;
		zone = (from z in ZoneMgr.Get().FindZonesOfType<ZoneGameModeButton>(Player.Side.FRIENDLY)
			where z.m_ButtonSlot == REFRESH_BUTTON_SLOT
			select z).FirstOrDefault();
		m_refreshPos = zone.gameObject.transform.position + m_teammateBoardPosition;
		m_refreshScale = zone.gameObject.transform.localScale;
		zone = (from z in ZoneMgr.Get().FindZonesOfType<ZoneGameModeButton>(Player.Side.FRIENDLY)
			where z.m_ButtonSlot == FREEZE_BUTTON_SLOT
			select z).FirstOrDefault();
		m_freezePos = zone.gameObject.transform.position + m_teammateBoardPosition;
		m_freezeScale = zone.gameObject.transform.localScale;
		InitTechLevelCounter();
		zonesInit = true;
	}

	public override bool ShouldEntityBeInViewer(TeammateEntityData entityData)
	{
		if (entityData.Zone == 1)
		{
			return entityData.Type == 12;
		}
		return false;
	}

	public override void AddEntityToViewer(TeammateEntityData entityData)
	{
		if (entityData.CardDBID == REFRESH_BUTTON_DBID)
		{
			InitGameModeButton(ref m_refreshButtonDummyActor, REFRESH_BUTTON_DBID, m_refreshPos, m_refreshScale);
			m_refreshEntityID = entityData.EntityID;
			UpdateGameModeButton(m_refreshButtonDummyActor, m_teammateRefreshCost, m_refreshEntityID);
		}
		else if (entityData.CardDBID == FREEZE_BUTTON_DBID)
		{
			InitGameModeButton(ref m_freezeButtonDummyActor, FREEZE_BUTTON_DBID, m_freezePos, m_freezeScale);
			m_freezeEntityID = entityData.EntityID;
			UpdateGameModeButton(m_freezeButtonDummyActor, 0, m_freezeEntityID);
		}
		else
		{
			InitGameModeButton(ref m_tavernButtonDummyActor, TAVERN_TIER_2_BUTTON_DBID, m_tavernTierPos, m_tavernTierScale);
			m_tavernEntityID = entityData.EntityID;
			UpdateGameModeButton(m_tavernButtonDummyActor, m_teammateTavernTierCost, m_tavernEntityID);
		}
	}

	public override bool IsActorInViewer(Actor actor)
	{
		if (!(m_refreshButtonDummyActor == actor) && !(m_tavernButtonDummyActor == actor))
		{
			return m_freezeButtonDummyActor == actor;
		}
		return true;
	}

	public override bool GetTeammateEntity(int entityID, out Entity teammateEntity)
	{
		teammateEntity = null;
		if (m_refreshButtonDummyActor != null && m_refreshButtonDummyActor.GetEntity() != null && m_refreshButtonDummyActor.GetEntity().GetEntityId() == entityID)
		{
			teammateEntity = m_refreshButtonDummyActor.GetEntity();
			return true;
		}
		if (m_tavernButtonDummyActor != null && m_tavernButtonDummyActor.GetEntity() != null && m_tavernButtonDummyActor.GetEntity().GetEntityId() == entityID)
		{
			teammateEntity = m_tavernButtonDummyActor.GetEntity();
			return true;
		}
		if (m_freezeButtonDummyActor != null && m_freezeButtonDummyActor.GetEntity() != null && m_freezeButtonDummyActor.GetEntity().GetEntityId() == entityID)
		{
			teammateEntity = m_freezeButtonDummyActor.GetEntity();
			return true;
		}
		return false;
	}

	public override void UpdateTeammateEntities(TeammatesEntities teammatesEntities)
	{
		m_teammateGold = teammatesEntities.Gold;
		m_teammateMaxGold = teammatesEntities.MaxGold;
		m_teammateRefreshCost = teammatesEntities.RefreshCost;
		m_teammateTavernTierCost = teammatesEntities.TavernCost;
		UpdateGameModeButton(m_freezeButtonDummyActor, 0, m_freezeEntityID);
		UpdateGameModeButton(m_refreshButtonDummyActor, m_teammateRefreshCost, m_refreshEntityID);
		UpdateGameModeButton(m_tavernButtonDummyActor, m_teammateTavernTierCost, m_tavernEntityID);
		if (m_tavernButtonDummyActor != null && GetTeammateTechLevelInt() >= GetMaxTechLevel())
		{
			m_tavernButtonDummyActor.gameObject.SetActive(value: false);
		}
		if (m_teammateManaCrystalCounter != null)
		{
			ManaCounter manaCounter = m_teammateManaCrystalCounter.GetComponent<ManaCounter>();
			if (manaCounter != null)
			{
				manaCounter.UpdateText(m_teammateGold, m_teammateMaxGold);
			}
		}
		int maxManaCrystals = (UniversalInputManager.UsePhoneUI ? MAX_DISPLAYED_MANA_CRYSTALS_PHONE : MAX_DISPLAYED_MANA_CRYSTALS);
		for (int i = 0; i < maxManaCrystals && i < m_teammateManaCrystals.Count; i++)
		{
			ManaCrystal manaCrystal = m_teammateManaCrystals[i];
			if (manaCrystal == null)
			{
				continue;
			}
			if (i < Mathf.Max(m_teammateMaxGold, m_teammateGold))
			{
				manaCrystal.Show();
				if (i < m_teammateGold)
				{
					manaCrystal.state = ManaCrystal.State.READY;
				}
				else
				{
					manaCrystal.state = ManaCrystal.State.USED;
				}
			}
			else
			{
				manaCrystal.Hide();
			}
		}
	}

	public IEnumerator WaitForManaCrystalManagerToBeReadyThenInitTeammateManaCrystals()
	{
		while (ManaCrystalMgr.Get() == null || !zonesInit)
		{
			yield return null;
		}
		ManaCrystalAssetPaths assetPaths = ManaCrystalMgr.Get().GetManaCrystalAssetPaths(ManaCrystalType.COIN);
		int maxManaCrystals = (UniversalInputManager.UsePhoneUI ? MAX_DISPLAYED_MANA_CRYSTALS_PHONE : MAX_DISPLAYED_MANA_CRYSTALS);
		CrystalLoadedData data = new CrystalLoadedData();
		data.viewer = this;
		for (int i = 0; i < maxManaCrystals; i++)
		{
			m_numLoadingCrystals++;
			AssetLoader.Get().InstantiatePrefab(assetPaths.m_ResourcePath, OnCrystalLoaded, data, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if (maxManaCrystals == 0)
		{
			InitManaCrystalLayout();
		}
	}

	private static void OnCrystalLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		((CrystalLoadedData)callbackData)?.viewer?.LoadCrystalCallback(assetRef, go, callbackData);
	}

	private void LoadCrystalCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_numLoadingCrystals--;
		if (m_manaCrystalWidth <= 0f)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_manaCrystalWidth = 0.33f;
			}
			else
			{
				m_manaCrystalWidth = go.transform.Find("Gem_Mana").GetComponent<Renderer>().bounds.size.x;
			}
		}
		ManaCrystal manaCrystal = go.GetComponent<ManaCrystal>();
		m_teammateManaCrystals.Add(manaCrystal);
		if (m_numLoadingCrystals == 0)
		{
			InitManaCrystalLayout();
		}
	}

	private void InitManaCrystalLayout()
	{
		if (m_teammateManaCrystalContainer == null)
		{
			m_teammateManaCrystalContainer = new GameObject();
		}
		m_manaCrystalPosition = ManaCrystalMgr.Get().gameObject.transform.position + m_teammateBoardPosition;
		m_teammateManaCrystalContainer.transform.SetParent(ManaCrystalMgr.Get().gameObject.transform.parent);
		m_teammateManaCrystalContainer.transform.position = m_manaCrystalPosition;
		m_teammateManaCrystalContainer.transform.localScale = ManaCrystalMgr.Get().gameObject.transform.localScale;
		m_teammateManaCrystalContainer.transform.rotation = ManaCrystalMgr.Get().gameObject.transform.rotation;
		m_manaCounterPosition = ManaCrystalMgr.Get().friendlyManaCounter.transform.position + m_teammateBoardPosition;
		m_teammateManaCrystalCounter = ManaCrystalMgr.Get().teammateManaCounter;
		m_teammateManaCrystalCounter.transform.position = m_manaCounterPosition;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ManaCrystalAssetPaths assetPaths = ManaCrystalMgr.Get().GetManaCrystalAssetPaths(ManaCrystalType.COIN);
			m_teammateManaCrystalCounter.GetComponent<ManaCounter>().InitializeLargeResourceGameObject(assetPaths.m_phoneLargeResource);
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			_ = MAX_DISPLAYED_MANA_CRYSTALS;
		}
		else
		{
			_ = MAX_DISPLAYED_MANA_CRYSTALS_PHONE;
		}
		Vector3 position = new Vector3(0f, 0.08f, 0f);
		for (int i = 0; i < m_teammateManaCrystals.Count; i++)
		{
			ManaCrystal manaCrystal = m_teammateManaCrystals[i];
			manaCrystal.MarkAsNotInGame();
			manaCrystal.gameObject.transform.SetParent(m_teammateManaCrystalContainer.transform);
			manaCrystal.gameObject.transform.localPosition = position;
			position.x += m_manaCrystalWidth;
		}
	}

	private Actor CreateGameModeButtonActor(int dbid)
	{
		using DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(dbid);
		if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
		{
			Log.Gameplay.PrintError("TeammateBoardViewer.CreateGameModeButtonActor(): Unable to load def for card db ID {0}.", dbid);
			return null;
		}
		GameObject actorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(cardDef.EntityDef, TAG_ZONE.PLAY, GameState.Get().GetFriendlySidePlayer(), TAG_PREMIUM.NORMAL), AssetLoadingOptions.IgnorePrefabPosition);
		if (actorGO == null)
		{
			Log.Gameplay.PrintError("TeammateBoardViewer.CreateGameModeButtonActor(): Unable to load Actor for entity def {0}.", cardDef.EntityDef);
			return null;
		}
		Actor entityActor = actorGO.GetComponentInChildren<Actor>();
		Entity dummyEntity = new Entity();
		dummyEntity.InitCard();
		dummyEntity.LoadCard(cardDef.EntityDef.GetCardId());
		dummyEntity.SetTag(GAME_TAG.ZONE, TAG_ZONE.PLAY);
		dummyEntity.SetTag(GAME_TAG.CONTROLLER, GameState.Get().GetFriendlyPlayerId());
		dummyEntity.SetTag(GAME_TAG.CARDTYPE, TAG_CARDTYPE.GAME_MODE_BUTTON);
		entityActor.SetEntity(dummyEntity);
		entityActor.SetCard(dummyEntity.GetCard());
		entityActor.SetCardDef(cardDef.DisposableCardDef);
		entityActor.SetTeammateActor(isTeamamteActor: true);
		dummyEntity.GetCard().SetActor(entityActor);
		if (entityActor.UseCoinManaGem())
		{
			entityActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		entityActor.UpdateAllComponents();
		return entityActor;
	}

	private void UpdateGameModeButton(Actor actor, int cost, int entityID)
	{
		if (!(actor == null))
		{
			actor.GetEntity().SetTag(GAME_TAG.ENTITY_ID, entityID);
			actor.GetEntity().SetTag(GAME_TAG.COST, cost);
			if (actor.UseCoinManaGem())
			{
				actor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			actor.UpdateAllComponents();
		}
	}

	public void InitGameModeButtons()
	{
		InitGameModeButton(ref m_refreshButtonDummyActor, REFRESH_BUTTON_DBID, m_refreshPos, m_refreshScale);
		InitGameModeButton(ref m_tavernButtonDummyActor, TAVERN_TIER_2_BUTTON_DBID, m_tavernTierPos, m_tavernTierScale);
		InitGameModeButton(ref m_freezeButtonDummyActor, FREEZE_BUTTON_DBID, m_freezePos, m_freezeScale);
	}

	private void InitGameModeButton(ref Actor buttonActor, int buttonDBID, Vector3 position, Vector3 scale)
	{
		if (!(buttonActor != null))
		{
			buttonActor = CreateGameModeButtonActor(buttonDBID);
			if (buttonActor != null)
			{
				buttonActor.transform.position = position;
				buttonActor.transform.localScale = scale;
			}
		}
	}

	private void InitTechLevelCounter()
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("BaconTechLevelRibbon.prefab:ad60cd0fe1c8eea4bb2f12cc280acda8");
		m_techLevelCounter = turnCounterGo.GetComponent<Notification>();
		m_techLevelCounter.gameObject.AddComponent<TeammateGameObject>();
		PlayMakerFSM component = m_techLevelCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmInt("TechLevel").Value = GetTeammateTechLevelInt();
		component.SendEvent("Birth");
		m_displayedTechLevelNumber = 0;
		Zone opposingHeroZone = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING);
		m_techLevelCounterPosition = opposingHeroZone.transform.position + new Vector3(-1.294f, 0.21f, -0.152f) + m_teammateBoardPosition;
		m_techLevelCounter.transform.position = m_techLevelCounterPosition;
		m_techLevelCounter.transform.localScale = Vector3.one * 0.58f;
	}

	private int GetMaxTechLevel()
	{
		if (GameState.Get() == null || GameState.Get().GetFriendlySidePlayer() == null)
		{
			return 6;
		}
		return GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_MAX_PLAYER_TECH_LEVEL);
	}

	private int GetTeammateTechLevelInt()
	{
		if (GameState.Get() == null || GameState.Get().GetFriendlySidePlayer() == null)
		{
			return 1;
		}
		int teammateId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		if (GameState.Get().GetPlayerInfoMap().ContainsKey(teammateId) && GameState.Get().GetPlayerInfoMap()[teammateId].GetPlayerHero() != null)
		{
			return GameState.Get().GetPlayerInfoMap()[teammateId].GetPlayerHero().GetRealTimePlayerTechLevel();
		}
		return 1;
	}

	public IEnumerator KeepTechLevelUpToDateCoroutine()
	{
		while (true)
		{
			if (!m_techLevelCounter.gameObject.activeInHierarchy)
			{
				yield return null;
			}
			int techLevel = GetTeammateTechLevelInt();
			if (techLevel != m_displayedTechLevelNumber)
			{
				PlayMakerFSM component = m_techLevelCounter.GetComponent<PlayMakerFSM>();
				component.FsmVariables.GetFsmInt("TechLevel").Value = techLevel;
				component.SendEvent("Action");
				string counterName = GameStrings.Get("GAMEPLAY_BACON_TAVERN_TIER");
				m_techLevelCounter.ChangeDialogText(counterName, "", "", "");
				m_displayedTechLevelNumber = techLevel;
			}
			yield return null;
		}
	}
}
