using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class QuestlineProgressUI : MonoBehaviour
{
	public Transform m_QuestCardBone;

	public Transform m_QuestRewardBone;

	public UberText m_ProgressText;

	public UberText m_QuestDetailTextLeft;

	public UberText m_QuestDetailTextRight;

	public UberText m_QuestRequirementText1;

	public UberText m_QuestRequirementText2;

	public UberText m_QuestRequirementText3;

	public GameObject m_RequirementCheckmark1;

	public GameObject m_RequirementCheckmark2;

	public GameObject m_RequirementCheckmark3;

	public GameObject m_QuestlinePart1FXReference;

	public GameObject m_QuestlinePart2FXReference;

	public GameObject m_QuestlineFinalFXReference;

	[Header("Reward Overlay Reference Settings")]
	public MeshRenderer m_RewardOverlayRenderer;

	public Texture m_MinionRewardOverlayTexture;

	public Texture m_LegendaryMinionRewardOverlayTexture;

	public Texture m_SpellRewardOverlayTexture;

	public Texture m_GoldenSpellRewardOverlayTexture;

	public Texture m_WeaponRewardOverlayTexture;

	public Texture m_LegendaryWeaponRewardOverlayTexture;

	public Texture m_HeroPowerRewardOverlayTexture;

	[Header("Reward Background Glow Reference Settings")]
	public MeshRenderer m_RewardPartBackGlowRenderer;

	public Material m_DefaultRewardPartBackGlowMaterial;

	public Material m_HeroPowerRewardPartBackGlowMaterial;

	public MeshRenderer m_RewardFinalBackGlowRenderer;

	public Material m_DefaultRewardFinalBackGlowMaterial;

	public Material m_HeroPowerRewardFinalBackGlowMaterial;

	private Actor m_originalQuestActor;

	private Actor m_questCardActor;

	private Actor m_questRewardActor;

	private bool m_isShown;

	private bool m_isResaturating;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public const string SEEK_GUIDANCE = "SW_433";

	public const string DISCOVER_THE_VOID_SHARD = "SW_433t";

	public const string ILLUMINATE_THE_VOID = "SW_433t2";

	public const string SORCERERS_GAMBIT = "SW_450";

	public const string STALL_FOR_TIME = "SW_450t";

	public const string REACH_THE_PORTAL_ROOM = "SW_450t2";

	private void Awake()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		if (m_isResaturating)
		{
			m_screenEffectsHandle.ClearCallbacks();
		}
	}

	public void SetOriginalQuestActor(Actor actor)
	{
		m_originalQuestActor = actor;
	}

	public void Show()
	{
		m_isShown = true;
		base.gameObject.SetActive(value: true);
		UpdateActors();
		DesaturateBoard();
	}

	public void Hide()
	{
		m_isShown = false;
		base.gameObject.SetActive(value: false);
		StopDesaturate();
	}

	public void UpdateText(int currentQuestProgress, int questProgressTotal)
	{
		UpdateProgressText(currentQuestProgress, questProgressTotal);
		UpdateQuestDetailText();
		UpdateQuestRequirementText();
	}

	private void UpdateProgressText(int currentQuestProgress, int questProgressTotal)
	{
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", currentQuestProgress, questProgressTotal);
	}

	private void UpdateQuestDetailText()
	{
		Entity questBaubleEntity = m_originalQuestActor.GetEntity();
		if (questBaubleEntity.HasTag(GAME_TAG.QUESTLINE))
		{
			int partNum = questBaubleEntity.GetTag(GAME_TAG.QUESTLINE_PART);
			if (partNum >= 1 && partNum <= 3)
			{
				m_QuestDetailTextLeft.Text = GameStrings.Get("GAMEPLAY_QUESTLINE_PART_" + partNum);
				m_QuestDetailTextRight.Text = GameStrings.Get("GAMEPLAY_QUESTLINE_PART_" + (partNum + 1));
				m_QuestDetailTextLeft.gameObject.SetActive(value: true);
				m_QuestDetailTextRight.gameObject.SetActive(value: true);
				m_QuestlinePart1FXReference.SetActive(partNum == 1);
				m_QuestlinePart2FXReference.SetActive(partNum == 2);
				m_QuestlineFinalFXReference.SetActive(partNum == 3);
				return;
			}
		}
		m_QuestDetailTextLeft.gameObject.SetActive(value: false);
		m_QuestDetailTextRight.gameObject.SetActive(value: false);
	}

	private void UpdateQuestRequirementText()
	{
		Entity questBaubleEntity = m_originalQuestActor.GetEntity();
		switch (questBaubleEntity.GetCardId())
		{
		case "SW_450":
		case "SW_450t":
		case "SW_450t2":
			m_QuestRequirementText1.Text = GameStrings.Get("GLOBAL_SPELL_SCHOOL_FIRE");
			m_QuestRequirementText2.Text = GameStrings.Get("GLOBAL_SPELL_SCHOOL_FROST");
			m_QuestRequirementText3.Text = GameStrings.Get("GLOBAL_SPELL_SCHOOL_ARCANE");
			break;
		case "SW_433":
			m_QuestRequirementText1.Text = GameStrings.Get("GAMEPLAY_COST_2");
			m_QuestRequirementText2.Text = GameStrings.Get("GAMEPLAY_COST_3");
			m_QuestRequirementText3.Text = GameStrings.Get("GAMEPLAY_COST_4");
			break;
		case "SW_433t":
			m_QuestRequirementText1.Text = GameStrings.Get("GAMEPLAY_COST_5");
			m_QuestRequirementText2.Text = GameStrings.Get("GAMEPLAY_COST_6");
			m_QuestRequirementText3.Text = "";
			break;
		case "SW_433t2":
			m_QuestRequirementText1.Text = GameStrings.Get("GAMEPLAY_COST_7");
			m_QuestRequirementText2.Text = GameStrings.Get("GAMEPLAY_COST_8");
			m_QuestRequirementText3.Text = "";
			break;
		default:
			m_ProgressText.gameObject.SetActive(value: true);
			m_QuestRequirementText1.gameObject.SetActive(value: false);
			m_QuestRequirementText2.gameObject.SetActive(value: false);
			m_QuestRequirementText3.gameObject.SetActive(value: false);
			m_RequirementCheckmark1.SetActive(value: false);
			m_RequirementCheckmark2.SetActive(value: false);
			m_RequirementCheckmark3.SetActive(value: false);
			return;
		}
		m_ProgressText.gameObject.SetActive(value: false);
		m_QuestRequirementText1.gameObject.SetActive(value: true);
		m_QuestRequirementText2.gameObject.SetActive(value: true);
		m_QuestRequirementText3.gameObject.SetActive(value: true);
		m_RequirementCheckmark1.SetActive(questBaubleEntity.HasTag(GAME_TAG.QUESTLINE_REQUIREMENT_MET_1));
		m_RequirementCheckmark2.SetActive(questBaubleEntity.HasTag(GAME_TAG.QUESTLINE_REQUIREMENT_MET_2));
		m_RequirementCheckmark3.SetActive(questBaubleEntity.HasTag(GAME_TAG.QUESTLINE_REQUIREMENT_MET_3));
	}

	private void Update()
	{
		if (!m_isShown || m_originalQuestActor.GetEntity().GetControllerSide() != Player.Side.FRIENDLY)
		{
			return;
		}
		foreach (Card handCard in GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards())
		{
			if (handCard.GetEntity().HasTag(GAME_TAG.QUEST_CONTRIBUTOR))
			{
				LayerUtils.SetLayer(handCard.gameObject, GameLayer.IgnoreFullScreenEffects);
			}
		}
	}

	private void DesaturateBoard()
	{
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.DesaturatePerspective);
	}

	private void StopDesaturate()
	{
		m_isResaturating = true;
		m_screenEffectsHandle.StopEffect(OnStopDesaturateFinished);
	}

	private void OnStopDesaturateFinished()
	{
		if (m_originalQuestActor.GetEntity().GetControllerSide() == Player.Side.FRIENDLY)
		{
			foreach (Card handCard in GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetCards())
			{
				if (!handCard.IsMousedOver())
				{
					LayerUtils.SetLayer(handCard.gameObject, GameLayer.Default);
				}
			}
		}
		m_isResaturating = false;
	}

	private void UpdateActors()
	{
		UpdateQuestActor();
		UpdateRewardActor();
	}

	private void UpdateQuestActor()
	{
		if (m_questCardActor == null || m_questCardActor.GetEntityDef() != m_originalQuestActor.GetEntityDef())
		{
			if (m_questCardActor != null)
			{
				m_questCardActor.Destroy();
			}
			GameObject questCardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(m_originalQuestActor.GetEntity()), AssetLoadingOptions.IgnorePrefabPosition);
			if (questCardActorGO == null)
			{
				Log.Gameplay.PrintError("QuestlineProgressUI.UpdateQuestCard(): Unable to load hand actor for entity {0}.", m_originalQuestActor);
				return;
			}
			LayerUtils.SetLayer(questCardActorGO, m_QuestCardBone.gameObject.layer, null);
			questCardActorGO.transform.parent = m_QuestCardBone;
			TransformUtil.Identity(questCardActorGO);
			m_questCardActor = questCardActorGO.GetComponentInChildren<Actor>();
			m_questCardActor.SetEntity(m_originalQuestActor.GetEntity());
			m_questCardActor.SetCardDefFromActor(m_originalQuestActor);
			m_questCardActor.SetPremium(m_originalQuestActor.GetEntity().GetPremiumType());
			m_questCardActor.SetWatermarkCardSetOverride(m_originalQuestActor.GetEntity().GetWatermarkCardSetOverride());
			m_questCardActor.UpdateAllComponents();
		}
	}

	private void UpdateRewardActor()
	{
		Entity originalQuestEntity = m_originalQuestActor.GetEntity();
		string questRewardCardID = QuestlineController.GetRewardCardIDFromQuestCardID(originalQuestEntity);
		if (string.IsNullOrEmpty(questRewardCardID))
		{
			Log.Gameplay.PrintError("QuestlineProgressUI.UpdateRewardCard(): No reward card ID found for quest card ID {0}.", originalQuestEntity.GetCardId());
			return;
		}
		if (m_questRewardActor == null || m_questRewardActor.GetEntityDef().GetCardId() != questRewardCardID)
		{
			if (m_questRewardActor != null)
			{
				m_questRewardActor.Destroy();
			}
			using DefLoader.DisposableCardDef questRewardCardDef = DefLoader.Get().GetCardDef(questRewardCardID);
			if (questRewardCardDef == null)
			{
				Log.Gameplay.PrintError("QuestlineProgressUI.UpdateRewardCard(): Unable to load CardDef for card ID {0}.", questRewardCardID);
				return;
			}
			EntityDef questRewardEntityDef = DefLoader.Get().GetEntityDef(questRewardCardID);
			if (questRewardEntityDef == null)
			{
				Log.Gameplay.PrintError("QuestlineProgressUI.UpdateRewardCard(): Unable to load EntityDef for card ID {0}.", questRewardCardID);
				return;
			}
			GameObject questRewardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(questRewardEntityDef, originalQuestEntity.GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
			if (questRewardActorGO == null)
			{
				Log.Gameplay.PrintError("QuestlineProgressUI.UpdateRewardCard(): Unable to load Hand Actor for entity def {0}.", questRewardEntityDef);
				return;
			}
			LayerUtils.SetLayer(questRewardActorGO, m_QuestRewardBone.gameObject.layer, null);
			questRewardActorGO.transform.parent = m_QuestRewardBone;
			TransformUtil.Identity(questRewardActorGO);
			m_questRewardActor = questRewardActorGO.GetComponentInChildren<Actor>();
			m_questRewardActor.SetEntityDef(questRewardEntityDef);
			m_questRewardActor.SetCardDef(questRewardCardDef);
			m_questRewardActor.SetPremium(m_originalQuestActor.GetEntity().GetPremiumType());
			m_questRewardActor.SetWatermarkCardSetOverride(m_originalQuestActor.GetEntity().GetWatermarkCardSetOverride());
			m_questRewardActor.UpdateAllComponents();
			UpdateRewardOverlayTexture(questRewardEntityDef);
			UpdateRewardBackgroundGlowTexture(questRewardEntityDef);
		}
		if (originalQuestEntity.HasTag(GAME_TAG.QUESTLINE) && originalQuestEntity.GetTag(GAME_TAG.QUESTLINE_PART) < 3)
		{
			m_questRewardActor.ActivateSpellBirthState(SpellType.GHOSTMODE);
		}
	}

	private void UpdateRewardOverlayTexture(EntityDef questRewardEntityDef)
	{
		if (!(m_RewardOverlayRenderer == null))
		{
			Texture overlayTexture = null;
			if (questRewardEntityDef.IsMinion())
			{
				overlayTexture = (questRewardEntityDef.IsElite() ? m_LegendaryMinionRewardOverlayTexture : m_MinionRewardOverlayTexture);
			}
			else if (questRewardEntityDef.IsSpell())
			{
				overlayTexture = ((m_questRewardActor.GetPremium() == TAG_PREMIUM.NORMAL) ? m_SpellRewardOverlayTexture : m_GoldenSpellRewardOverlayTexture);
			}
			else if (questRewardEntityDef.IsWeapon())
			{
				overlayTexture = (questRewardEntityDef.IsElite() ? m_LegendaryWeaponRewardOverlayTexture : m_WeaponRewardOverlayTexture);
			}
			else if (questRewardEntityDef.IsHeroPower())
			{
				overlayTexture = m_HeroPowerRewardOverlayTexture;
			}
			if (!(overlayTexture == null))
			{
				Material material = m_RewardOverlayRenderer.GetMaterial();
				material.SetTexture("_MainTex", overlayTexture);
				material.SetTexture("_AddTex", overlayTexture);
			}
		}
	}

	private void UpdateRewardBackgroundGlowTexture(EntityDef questRewardEntityDef)
	{
		if (m_RewardPartBackGlowRenderer != null)
		{
			Material material = m_DefaultRewardPartBackGlowMaterial;
			if (questRewardEntityDef.IsHeroPower())
			{
				material = m_HeroPowerRewardPartBackGlowMaterial;
			}
			m_RewardPartBackGlowRenderer.SetMaterial(material);
		}
		if (m_RewardFinalBackGlowRenderer != null)
		{
			Material material2 = m_DefaultRewardFinalBackGlowMaterial;
			if (questRewardEntityDef.IsHeroPower())
			{
				material2 = m_HeroPowerRewardFinalBackGlowMaterial;
			}
			m_RewardFinalBackGlowRenderer.SetMaterial(material2);
		}
	}
}
