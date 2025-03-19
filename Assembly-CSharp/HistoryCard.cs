using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class HistoryCard : HistoryItem
{
	public UberText m_createdByText;

	public static readonly Color OPPONENT_COLOR = new Color(0.7137f, 0.2f, 0.1333f, 1f);

	public static readonly Color FRIENDLY_COLOR = new Color(0.6509f, 0.6705f, 0.9843f, 1f);

	private const float ABILITY_CARD_ANIMATE_TO_BIG_CARD_AREA_TIME = 1f;

	private const float LETTUCE_ABILITY_ANIMATE_TO_BIG_CARD_AREA_TIME = 0.5f;

	private const float BIG_CARD_SCALE = 1.03f;

	private const float MOUSE_OVER_Z_OFFSET_TOP = -1.404475f;

	private const float MOUSE_OVER_Z_OFFSET_BOTTOM = 0.1681719f;

	private const float MOUSE_OVER_Z_OFFSET_PHONE = -4.75f;

	private const float MOUSE_OVER_Z_OFFSET_SIGNATURE_PHONE = -3.9f;

	private const float MOUSE_OVER_Z_OFFSET_SECRET_PHONE = -4.3f;

	private const float MOUSE_OVER_Z_OFFSET_WITH_CREATOR_PHONE = -4.3f;

	private const float MOUSE_OVER_HEIGHT_OFFSET = 7.524521f;

	private PlatformDependentValue<float> MOUSE_OVER_X_OFFSET = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 4.326718f,
		Tablet = 4.7f,
		Phone = 5.4f
	};

	private PlatformDependentValue<float> MOUSE_OVER_SCALE = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 1f,
		Tablet = 1f,
		Phone = 1f
	};

	private PlatformDependentValue<float> X_SIZE_OF_MOUSE_OVER_CHILD = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 2.5f,
		Tablet = 2.5f,
		Phone = 2.5f
	};

	private const float MAX_WIDTH_OF_CHILDREN = 5f;

	private const string CREATED_BY_BONE_NAME = "HistoryCreatedByBone";

	private Material m_fullTileMaterial;

	private Material m_halfTileMaterial;

	private bool m_mousedOver;

	private bool m_halfSize;

	private bool m_hasBeenShown;

	private Actor m_separator;

	private bool m_haveDisplayedCreator;

	private bool m_gameEntityMousedOver;

	private List<HistoryInfo> m_childInfos;

	private List<HistoryChildCard> m_historyChildren = new List<HistoryChildCard>();

	private HistoryInfo m_ownerInfo;

	private HistoryChildCard m_owner;

	private bool m_bigCardFinishedCallbackHasRun;

	private HistoryManager.BigCardFinishedCallback m_bigCardFinishedCallback;

	private bool m_bigCardCountered;

	private bool m_bigCardWaitingForSecret;

	private bool m_bigCardFromMetaData;

	private Entity m_bigCardPostTransformedEntity;

	private float m_tileSize;

	private int m_displayTimeMS;

	private HistoryInfoType m_historyInfoType;

	public void LoadMainCardActor()
	{
		string actorPath = (m_fatigue ? "Card_Hand_Fatigue.prefab:ae394ca0bb29a964eb4c7eeb555f2fae" : ((!m_burned) ? ActorNames.GetHistoryActor(m_entity, m_historyInfoType) : "Card_Hand_BurnAway.prefab:869912636c30bc244bace332571afc94"));
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("HistoryCard.LoadMainCardActor() - FAILED to load actor \"{0}\"", actorPath);
			return;
		}
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("HistoryCard.LoadMainCardActor() - ERROR actor \"{0}\" has no Actor component", actorPath);
			return;
		}
		m_mainCardActor = actor;
		if (m_fatigue)
		{
			m_mainCardActor.GetPowersTextObject().Text = GameStrings.Get("GAMEPLAY_FATIGUE_HISTORY_TEXT");
		}
		else if (m_burned)
		{
			m_mainCardActor.GetPowersTextObject().Text = GameStrings.Get("GAMEPLAY_BURNED_CARDS_HISTORY_TEXT");
		}
		else
		{
			if (m_entity == null)
			{
				string cardObjectName = ((m_mainCardActor.GetCard() == null) ? "" : m_mainCardActor.GetCard().name);
				Debug.LogWarningFormat("HistoryCard.LoadMainCardActor() - [HSTN-210987] Disgnostic info. m_entity not defined. m_mainCardActor.GetCard().name = '" + cardObjectName + "'");
				return;
			}
			m_mainCardActor.SetCardDefFromEntity(m_entity);
			m_mainCardActor.SetPremium(m_entity.GetPremiumType());
			m_mainCardActor.SetWatermarkCardSetOverride(m_entity.GetWatermarkCardSetOverride());
		}
		if (m_entity != null && m_entity.IsStarship())
		{
			m_entity.SetTag(GAME_TAG.HIDE_COST, 1);
			m_entity.SetTag(GAME_TAG.HIDE_STATS, 0);
		}
		m_mainCardActor.SetHistoryItem(this);
		m_mainCardActor.UpdateAllComponents();
		if (m_mainCardActor.UseCoinManaGem())
		{
			m_mainCardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		int signatureFrameId = ((m_entity != null) ? ActorNames.GetSignatureFrameId(m_entity.GetCardId()) : 0);
		if (!m_fatigue && !m_burned && m_entity != null && m_entity.GetPremiumType() == TAG_PREMIUM.SIGNATURE && signatureFrameId >= 2)
		{
			BigCard.Get()?.Hide();
			TooltipPanelManager.Get().UpdateKeywordHelpForHistoryCard(m_entity, m_mainCardActor, m_createdByText, TooltipPanelManager.TooltipBoneSource.TOP_RIGHT, signatureOnly: true);
		}
		InitDisplayedCreator();
	}

	private void InitDisplayedCreator()
	{
		if (m_entity == null)
		{
			return;
		}
		string name = m_entity.GetDisplayedCreatorName();
		GameObject bone = m_mainCardActor.FindBone("HistoryCreatedByBone");
		if (string.IsNullOrEmpty(name))
		{
			if (!(m_owner != null) || m_owner.GetEntity() == null)
			{
				return;
			}
			name = m_owner.GetEntity().GetDisplayedCreatorName();
			bone = m_owner.m_mainCardActor.FindBone("HistoryCreatedByBone");
			if (string.IsNullOrEmpty(name))
			{
				return;
			}
		}
		if (!bone)
		{
			Error.AddDevWarning("Missing Bone", "Missing {0} on {1}", "HistoryCreatedByBone", m_mainCardActor);
		}
		else
		{
			m_createdByText.Text = GameStrings.Format("GAMEPLAY_HISTORY_CREATED_BY", name);
			m_createdByText.transform.parent = m_mainCardActor.GetRootObject().transform;
			m_haveDisplayedCreator = true;
		}
	}

	private void ShowDisplayedCreator()
	{
		m_createdByText.gameObject.SetActive(m_haveDisplayedCreator);
		if (m_entity == null)
		{
			return;
		}
		string displayedCreatorName = m_entity.GetDisplayedCreatorName();
		GameObject bone = m_mainCardActor.FindBone("HistoryCreatedByBone");
		if (string.IsNullOrEmpty(displayedCreatorName))
		{
			if (!(m_owner != null) || m_owner.GetEntity() == null)
			{
				return;
			}
			string displayedCreatorName2 = m_owner.GetEntity().GetDisplayedCreatorName();
			bone = m_owner.m_mainCardActor.FindBone("HistoryCreatedByBone");
			if (string.IsNullOrEmpty(displayedCreatorName2))
			{
				return;
			}
		}
		TransformUtil.SetPoint(m_createdByText, new Vector3(0.5f, 0f, 1f), bone, new Vector3(0.5f, 0f, 0f));
	}

	public bool HasBeenShown()
	{
		return m_hasBeenShown;
	}

	public void MarkAsShown()
	{
		if (!m_hasBeenShown)
		{
			m_hasBeenShown = true;
		}
	}

	public bool IsHalfSize()
	{
		return m_halfSize;
	}

	public float GetTileSize()
	{
		return m_tileSize;
	}

	public void LoadTile(HistoryTileInitInfo info)
	{
		m_childInfos = info.m_childInfos;
		m_ownerInfo = info.m_ownerInfo;
		if (info.m_fatigueTexture != null)
		{
			m_portraitTexture = info.m_fatigueTexture;
			m_fatigue = true;
		}
		else if (info.m_burnedCardsTexture != null)
		{
			m_portraitTexture = info.m_burnedCardsTexture;
			m_burned = true;
		}
		else
		{
			m_entity = info.m_entity;
			m_portraitTexture = info.m_portraitTexture;
			m_portraitGoldenMaterial = info.m_portraitGoldenMaterial;
			SetCardDef(info.m_cardDef);
			m_fullTileMaterial = info.m_fullTileMaterial;
			m_halfTileMaterial = info.m_halfTileMaterial;
			m_splatAmount = info.m_splatAmount;
			m_isPoisonous = info.m_isPoisonous;
			m_isCriticalHit = info.m_isCriticalHit;
			m_splatType = info.m_splatType;
			m_dead = info.m_dead;
		}
		m_historyInfoType = info.m_type;
		switch (info.m_type)
		{
		case HistoryInfoType.NONE:
		case HistoryInfoType.WEAPON_PLAYED:
		case HistoryInfoType.CARD_PLAYED:
		case HistoryInfoType.FATIGUE:
		case HistoryInfoType.BURNED_CARDS:
			LoadPlayTile();
			break;
		case HistoryInfoType.ATTACK:
			LoadAttackTile();
			break;
		case HistoryInfoType.TRIGGER:
			LoadTriggerTile();
			break;
		case HistoryInfoType.WEAPON_BREAK:
			LoadWeaponBreak();
			break;
		}
	}

	public void NotifyMousedOver()
	{
		if (m_mousedOver || this == HistoryManager.Get().GetCurrentBigCard())
		{
			return;
		}
		LoadChildCardsFromInfos();
		LoadOwnerFromInfo();
		m_mousedOver = true;
		SoundManager.Get().LoadAndPlay("history_event_mouseover.prefab:0bc4f1638257a264a9b02e811c0a61b5", m_tileActor.gameObject);
		if (!m_mainCardActor)
		{
			LoadMainCardActor();
			if (m_mainCardActor != null)
			{
				LayerUtils.SetLayer(m_mainCardActor, GameLayer.Tooltip);
			}
		}
		ShowTile();
	}

	public void NotifyMousedOut()
	{
		if (!m_mousedOver)
		{
			return;
		}
		m_mousedOver = false;
		if (m_gameEntityMousedOver)
		{
			GameState.Get().GetGameEntity().NotifyOfHistoryTokenMousedOut();
			m_gameEntityMousedOver = false;
		}
		TooltipPanelManager.Get().HideKeywordHelp();
		if (m_owner != null && m_owner.m_mainCardActor != null)
		{
			m_owner.m_mainCardActor.ActivateAllSpellsDeathStates();
			m_owner.m_mainCardActor.Hide();
		}
		if ((bool)m_mainCardActor)
		{
			m_mainCardActor.ActivateAllSpellsDeathStates();
			m_mainCardActor.Hide();
		}
		for (int i = 0; i < m_historyChildren.Count; i++)
		{
			if (!(m_historyChildren[i].m_mainCardActor == null))
			{
				m_historyChildren[i].m_mainCardActor.ActivateAllSpellsDeathStates();
				m_historyChildren[i].m_mainCardActor.Hide();
			}
		}
		if ((bool)m_separator)
		{
			m_separator.Hide();
		}
		HistoryManager.Get().UpdateLayout();
	}

	private void LoadPlayTile()
	{
		m_halfSize = false;
		LoadTileImpl("HistoryTile_Card.prefab:df3002d4532e4dd40b37101e83202db4");
		LoadArrowSeparator();
	}

	private void LoadAttackTile()
	{
		m_halfSize = true;
		LoadTileImpl("HistoryTile_Attack.prefab:816bc6c1f4d8f0c439e981d30bf5b57b");
		LoadSwordsSeparator();
	}

	private void LoadWeaponBreak()
	{
		m_halfSize = true;
		LoadTileImpl("HistoryTile_Attack.prefab:816bc6c1f4d8f0c439e981d30bf5b57b");
	}

	private void LoadTriggerTile()
	{
		m_halfSize = true;
		LoadTileImpl("HistoryTile_Trigger.prefab:14cb236519ac3744b8c7c1274a379c94");
		LoadArrowSeparator();
	}

	private void LoadTileImpl(string actorPath)
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarningFormat("HistoryCard.LoadTileImpl() - FAILED to load actor \"{0}\"", actorPath);
			return;
		}
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarningFormat("HistoryCard.LoadTileImpl() - ERROR actor \"{0}\" has no Actor component", actorPath);
			return;
		}
		m_tileActor = actor;
		m_tileActor.transform.parent = base.transform;
		TransformUtil.Identity(m_tileActor.transform);
		m_tileActor.transform.localScale = HistoryManager.Get().transform.localScale;
		Material[] newMaterials = new Material[2]
		{
			m_tileActor.GetMeshRenderer().GetMaterial(),
			null
		};
		if (m_halfSize)
		{
			if (m_halfTileMaterial != null)
			{
				newMaterials[1] = m_halfTileMaterial;
				m_tileActor.GetMeshRenderer().SetMaterials(newMaterials);
			}
			else
			{
				m_tileActor.GetMeshRenderer().GetMaterial(1).mainTexture = m_portraitTexture;
			}
		}
		else if (m_fullTileMaterial != null)
		{
			newMaterials[1] = m_fullTileMaterial;
			m_tileActor.GetMeshRenderer().SetMaterials(newMaterials);
		}
		else
		{
			m_tileActor.GetMeshRenderer().GetMaterial(1).mainTexture = m_portraitTexture;
		}
		Color tileColor = Color.white;
		if (Board.Get() != null)
		{
			tileColor = Board.Get().m_HistoryTileColor;
		}
		if (!m_fatigue && !m_burned)
		{
			if (m_entity.IsControlledByFriendlySidePlayer())
			{
				tileColor *= FRIENDLY_COLOR;
			}
			else
			{
				tileColor *= OPPONENT_COLOR;
			}
		}
		else if (AffectsFriendlySidePlayer())
		{
			tileColor *= FRIENDLY_COLOR;
		}
		else
		{
			tileColor *= OPPONENT_COLOR;
		}
		Renderer[] componentsInChildren = m_tileActor.GetMeshRenderer().GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!renderer.CompareTag(HistoryItem.RENDERER_TAG))
			{
				renderer.GetMaterial().color = Board.Get().m_HistoryTileColor;
			}
		}
		List<Material> materials = m_tileActor.GetMeshRenderer().GetMaterials();
		materials[0].color = tileColor;
		materials[1].color = Board.Get().m_HistoryTileColor;
		if (GetTileCollider() != null)
		{
			m_tileSize = GetTileCollider().bounds.size.z;
		}
	}

	private bool AffectsFriendlySidePlayer()
	{
		if (m_childInfos == null)
		{
			return false;
		}
		if (m_childInfos.Count == 0)
		{
			return false;
		}
		if (m_childInfos[0] == null)
		{
			return false;
		}
		if (m_childInfos[0].GetDuplicatedEntity() != null && m_childInfos[0].GetDuplicatedEntity().IsControlledByFriendlySidePlayer())
		{
			return true;
		}
		return false;
	}

	private void LoadSwordsSeparator()
	{
		LoadSeparator("History_Swords.prefab:361feac100313e443b68055167e5088c");
	}

	private void LoadArrowSeparator()
	{
		if (m_childInfos != null && m_childInfos.Count != 0)
		{
			LoadSeparator("History_Arrow.prefab:a9ef1ff267ab0a24c9cdef7f3678b5a4");
		}
	}

	private void LoadSeparator(string actorPath)
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorObject == null)
		{
			Debug.LogWarning($"HistoryCard.LoadSeparator() - FAILED to load actor \"{actorPath}\"");
			return;
		}
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"HistoryCard.LoadSeparator() - ERROR actor \"{actorPath}\" has no Actor component");
			return;
		}
		m_separator = actor;
		MeshRenderer blueSeparator = m_separator.GetRootObject().transform.Find("Blue").gameObject.GetComponent<MeshRenderer>();
		MeshRenderer redSeparator = m_separator.GetRootObject().transform.Find("Red").gameObject.GetComponent<MeshRenderer>();
		if (m_fatigue || m_burned)
		{
			redSeparator.enabled = true;
			blueSeparator.enabled = false;
		}
		else
		{
			bool isFriendlySide = (blueSeparator.enabled = m_entity.IsControlledByFriendlySidePlayer());
			redSeparator.enabled = !isFriendlySide;
		}
		m_separator.transform.parent = base.transform;
		TransformUtil.Identity(m_separator.transform);
		if (m_separator.GetRootObject() != null)
		{
			TransformUtil.Identity(m_separator.GetRootObject().transform);
		}
		m_separator.Hide();
	}

	private void LoadOwnerFromInfo()
	{
		if (m_ownerInfo != null)
		{
			m_owner = LoadHistoryChildCard(m_ownerInfo);
			m_ownerInfo = null;
		}
	}

	private void LoadChildCardsFromInfos()
	{
		if (m_childInfos == null)
		{
			return;
		}
		foreach (HistoryInfo info in m_childInfos)
		{
			HistoryChildCard childCard = LoadHistoryChildCard(info);
			if (childCard != null)
			{
				m_historyChildren.Add(childCard);
			}
		}
		m_childInfos = null;
	}

	private HistoryChildCard LoadHistoryChildCard(HistoryInfo info)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab("HistoryChildCard.prefab:f85dbd296f9764f4e9c6a2c638a024d3", AssetLoadingOptions.IgnorePrefabPosition);
		HistoryChildCard childCard = go.GetComponent<HistoryChildCard>();
		Entity entity = info.GetDuplicatedEntity();
		if (entity == null)
		{
			Log.Gameplay.PrintError(string.Format("{0}.{1}: {2} has a null duplicated entity!", "HistoryCard", "LoadHistoryChildCard", info));
			return null;
		}
		using DefLoader.DisposableCardDef def = entity.ShareDisposableCardDef();
		if (def?.CardDef == null)
		{
			return null;
		}
		childCard.SetCardInfo(entity, def, info);
		childCard.transform.parent = base.transform;
		childCard.LoadMainCardActor();
		Actor actor = go.GetComponentInChildren<Actor>();
		if (actor == null)
		{
			return null;
		}
		actor.SetEntity(entity);
		actor.SetCardDef(def);
		actor.UpdateAllComponents();
		return childCard;
	}

	private void ShowTile()
	{
		if (!m_mousedOver)
		{
			m_mainCardActor.Hide();
			return;
		}
		m_mainCardActor.Show();
		InitializeMainCardActor();
		DisplaySpells();
		float mainCardX = base.transform.position.x + (float)MOUSE_OVER_X_OFFSET;
		float mainCardY = base.transform.position.y + 7.524521f;
		float mainCardZ = (UniversalInputManager.UsePhoneUI ? GetZOffsetForThisTilesMouseOverCard(m_mainCardActor) : (base.transform.position.z + GetZOffsetForThisTilesMouseOverCard()));
		if (m_owner != null)
		{
			m_owner.m_mainCardActor.Show();
			m_owner.InitializeMainCardActor();
			m_owner.DisplaySpells();
			m_owner.m_mainCardActor.UpdateAllComponents();
			m_owner.m_mainCardActor.transform.position = new Vector3(mainCardX, mainCardY, mainCardZ);
			m_owner.m_mainCardActor.transform.localScale = new Vector3(MOUSE_OVER_SCALE, 1f, MOUSE_OVER_SCALE);
			mainCardX += (float)X_SIZE_OF_MOUSE_OVER_CHILD;
		}
		m_mainCardActor.transform.position = new Vector3(mainCardX, mainCardY, mainCardZ);
		m_mainCardActor.transform.localScale = new Vector3(MOUSE_OVER_SCALE, 1f, MOUSE_OVER_SCALE);
		if ((bool)UniversalInputManager.UsePhoneUI && (m_fatigue || m_burned))
		{
			m_mainCardActor.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		ShowDisplayedCreator();
		if (!m_gameEntityMousedOver)
		{
			m_gameEntityMousedOver = true;
			GameState.Get().GetGameEntity().NotifyOfHistoryTokenMousedOver(base.gameObject);
		}
		if (!m_fatigue && !m_burned)
		{
			TooltipPanelManager.TooltipBoneSource boneSource = TooltipPanelManager.TooltipBoneSource.BOTTOM_MIDDLE_GOING_RIGHT;
			TooltipPanelManager.Get().UpdateKeywordHelpForHistoryCard(m_entity, m_mainCardActor, m_createdByText, boneSource);
		}
		if (m_historyChildren.Count <= 0)
		{
			return;
		}
		int singleRowCardCountLimit = 4;
		int doubleRowCardCountLimit = 8;
		if (m_owner != null)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				singleRowCardCountLimit = 1;
				doubleRowCardCountLimit = 4;
			}
			else
			{
				singleRowCardCountLimit = 3;
				doubleRowCardCountLimit = 8;
			}
		}
		int numRows = 3;
		if (m_historyChildren.Count <= singleRowCardCountLimit)
		{
			numRows = 1;
		}
		else if (m_historyChildren.Count <= doubleRowCardCountLimit)
		{
			numRows = 2;
		}
		float maximumScale = 1f;
		switch (numRows)
		{
		case 2:
			maximumScale = 0.5f;
			break;
		case 3:
			maximumScale = 0.3f;
			break;
		}
		int maxNumCardsPerRow = Mathf.CeilToInt((float)m_historyChildren.Count / (float)numRows);
		float totalWidthOfChildren = (float)maxNumCardsPerRow * (float)X_SIZE_OF_MOUSE_OVER_CHILD;
		float newCardScale = 5f / totalWidthOfChildren;
		newCardScale = Mathf.Clamp(newCardScale, 0.1f, maximumScale);
		int numCardsInThisRow = 0;
		int currentRow = 1;
		for (int i = 0; i < m_historyChildren.Count; i++)
		{
			m_historyChildren[i].m_mainCardActor.Show();
			m_historyChildren[i].InitializeMainCardActor();
			m_historyChildren[i].DisplaySpells();
			m_historyChildren[i].m_mainCardActor.UpdateAllComponents();
			float zPosition = m_mainCardActor.transform.position.z;
			switch (numRows)
			{
			case 2:
				zPosition = ((currentRow != 1) ? (zPosition - 0.78f) : (zPosition + 0.78f));
				break;
			case 3:
				switch (currentRow)
				{
				case 1:
					zPosition += 0.98f;
					break;
				case 3:
					zPosition -= 0.93f;
					break;
				}
				break;
			}
			float firstCardX = m_mainCardActor.transform.position.x + (float)X_SIZE_OF_MOUSE_OVER_CHILD * (1f + newCardScale) / 2f;
			m_historyChildren[i].m_mainCardActor.transform.position = new Vector3(firstCardX + (float)X_SIZE_OF_MOUSE_OVER_CHILD * (float)numCardsInThisRow * newCardScale, m_mainCardActor.transform.position.y, zPosition);
			m_historyChildren[i].m_mainCardActor.transform.localScale = new Vector3(newCardScale, newCardScale, newCardScale);
			numCardsInThisRow++;
			if (numCardsInThisRow >= maxNumCardsPerRow)
			{
				numCardsInThisRow = 0;
				currentRow++;
			}
		}
		if (m_separator != null)
		{
			float EXTRA_HEIGHT = 0.4f;
			float EXTRA_X_SPACE = (float)X_SIZE_OF_MOUSE_OVER_CHILD / 2f;
			m_separator.Show();
			m_separator.transform.position = new Vector3(m_mainCardActor.transform.position.x + EXTRA_X_SPACE, m_mainCardActor.transform.position.y + EXTRA_HEIGHT, m_mainCardActor.transform.position.z);
		}
	}

	private float GetZOffsetForThisTilesMouseOverCard(Actor mainCard = null)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_entity != null && m_entity.IsHiddenSecret())
			{
				return -4.3f;
			}
			if (m_entity != null && m_entity.IsHiddenForge())
			{
				return -4.3f;
			}
			if (m_haveDisplayedCreator)
			{
				return -4.3f;
			}
			if (mainCard != null && mainCard.GetPremium() == TAG_PREMIUM.SIGNATURE)
			{
				return -3.9f;
			}
			return -4.75f;
		}
		float num = Mathf.Abs(-1.5726469f);
		HistoryManager theManager = HistoryManager.Get();
		float offsetForEachTile = num / (float)theManager.GetNumHistoryTiles();
		int positionOfTile = theManager.GetNumHistoryTiles() - theManager.GetIndexForTile(this) - 1;
		return -1.404475f + offsetForEachTile * (float)positionOfTile;
	}

	public void LoadBigCard(HistoryBigCardInitInfo info)
	{
		m_entity = info.m_entity;
		m_historyInfoType = info.m_historyInfoType;
		m_portraitTexture = info.m_portraitTexture;
		SetCardDef(info.m_cardDef);
		m_portraitGoldenMaterial = info.m_portraitGoldenMaterial;
		m_bigCardFinishedCallback = info.m_finishedCallback;
		m_bigCardCountered = info.m_countered;
		m_bigCardWaitingForSecret = info.m_waitForSecretSpell;
		m_bigCardFromMetaData = info.m_fromMetaData;
		m_bigCardPostTransformedEntity = info.m_postTransformedEntity;
		m_displayTimeMS = info.m_displayTimeMS;
		LoadMainCardActor();
	}

	public void LoadBigCardPostTransformedEntity()
	{
		if (m_bigCardPostTransformedEntity != null)
		{
			m_entity = m_bigCardPostTransformedEntity;
			Card postTransformedCard = m_entity.GetCard();
			m_portraitTexture = postTransformedCard.GetPortraitTexture(m_entity.GetPremiumType());
			m_portraitGoldenMaterial = postTransformedCard.GetGoldenMaterial();
			using (DefLoader.DisposableCardDef cardDef = postTransformedCard.ShareDisposableCardDef())
			{
				SetCardDef(cardDef);
			}
			LoadMainCardActor();
		}
	}

	public HistoryManager.BigCardFinishedCallback GetBigCardFinishedCallback()
	{
		return m_bigCardFinishedCallback;
	}

	public void RunBigCardFinishedCallback()
	{
		if (!m_bigCardFinishedCallbackHasRun)
		{
			m_bigCardFinishedCallbackHasRun = true;
			if (m_bigCardFinishedCallback != null)
			{
				m_bigCardFinishedCallback();
			}
		}
	}

	public bool WasBigCardCountered()
	{
		return m_bigCardCountered;
	}

	public int GetDisplayTimeMS()
	{
		return m_displayTimeMS;
	}

	public bool IsCastedByLettuceCharacter()
	{
		return m_entity.GetLettuceAbilityOwner() != null;
	}

	public bool IsBigCardWaitingForSecret()
	{
		return m_bigCardWaitingForSecret;
	}

	public bool IsBigCardFromMetaData()
	{
		return m_bigCardFromMetaData;
	}

	public Entity GetBigCardPostTransformedEntity()
	{
		return m_bigCardPostTransformedEntity;
	}

	public bool HasBigCardPostTransformedEntity()
	{
		return m_bigCardPostTransformedEntity != null;
	}

	public void ShowBigCard(Vector3[] pathToFollow)
	{
		m_mainCardActor.transform.localScale = new Vector3(1.03f, 1.03f, 1.03f);
		Entity entity = m_entity;
		if (HasBigCardPostTransformedEntity())
		{
			entity = m_bigCardPostTransformedEntity;
		}
		if (entity != null)
		{
			float time = 1f;
			if (entity.IsLettuceAbility())
			{
				time = 0.5f;
			}
			if (m_displayTimeMS > 0)
			{
				float timeToDisplay = (float)m_displayTimeMS / 1000f;
				time = Mathf.Min(time, timeToDisplay);
			}
			if (entity.IsSpell() || entity.IsHeroPower() || entity.IsLettuceAbility() || m_bigCardFromMetaData)
			{
				pathToFollow[0] = m_mainCardActor.transform.position;
				Hashtable args = iTweenManager.Get().GetTweenHashTable();
				args.Add("path", pathToFollow);
				args.Add("time", time);
				args.Add("oncomplete", "OnBigCardPathComplete");
				args.Add("oncompletetarget", base.gameObject);
				iTween.MoveTo(m_mainCardActor.gameObject, args);
				iTween.ScaleTo(base.gameObject, new Vector3(1f, 1f, 1f), time);
				SoundManager.Get().LoadAndPlay("play_card_from_hand_1.prefab:ac4be75e319a97947a68308a08e54e88");
			}
			else
			{
				ShowDisplayedCreator();
			}
		}
	}

	private void OnBigCardPathComplete()
	{
		ShowDisplayedCreator();
	}
}
