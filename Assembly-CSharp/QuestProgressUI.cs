using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class QuestProgressUI : MonoBehaviour
{
	public Transform m_QuestCardBone;

	public Transform m_QuestRewardBone;

	public Transform m_EvolutionCardBone;

	public UberText m_ProgressText;

	public UberText m_QuestDetailText;

	public GameObject m_Standard_Arrow;

	public GameObject m_Battlegrounds_Arrow;

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
	public MeshRenderer m_RewardBackGlowRenderer;

	public Material m_DefaultRewardBackGlowMaterial;

	public Material m_HeroPowerRewardBackGlowMaterial;

	private Actor m_originalQuestActor;

	private Actor m_questCardActor;

	private Actor m_questRewardActor;

	private Actor m_evolutionCardActor;

	private bool m_isShown;

	private bool m_isResaturating;

	private ScreenEffectsHandle m_screenEffectsHandle;

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
	}

	private void UpdateProgressText(int currentQuestProgress, int questProgressTotal)
	{
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", currentQuestProgress, questProgressTotal);
	}

	private void UpdateQuestDetailText()
	{
		Entity questBaubleEntity = m_originalQuestActor.GetEntity();
		if (questBaubleEntity.HasTag(GAME_TAG.QUEST_CONTRIBUTOR))
		{
			int databaseId = questBaubleEntity.GetTag(GAME_TAG.QUEST_CONTRIBUTOR);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseId);
			if (entityDef != null)
			{
				m_QuestDetailText.Text = entityDef.GetName();
				m_QuestDetailText.gameObject.SetActive(value: true);
				return;
			}
		}
		m_QuestDetailText.gameObject.SetActive(value: false);
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
		UpdateEvolutionCardActor();
		UpdateArrow();
	}

	private void UpdateArrow()
	{
		if (m_Battlegrounds_Arrow != null && GameMgr.Get() != null && GameMgr.Get().IsBattlegrounds())
		{
			m_Battlegrounds_Arrow.SetActive(value: true);
			m_Standard_Arrow?.SetActive(value: false);
		}
		else
		{
			m_Standard_Arrow?.SetActive(value: true);
			m_Battlegrounds_Arrow?.SetActive(value: false);
		}
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
				Log.Gameplay.PrintError("QuestProgressUI.UpdateQuestCard(): Unable to load hand actor for entity {0}.", m_originalQuestActor);
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
		string questRewardCardID = QuestController.GetRewardCardIDFromQuestCardID(originalQuestEntity);
		if (string.IsNullOrEmpty(questRewardCardID))
		{
			Log.Gameplay.PrintError("QuestProgressUI.UpdateRewardCard(): No reward card ID found for quest card ID {0}.", originalQuestEntity.GetCardId());
		}
		else
		{
			if (!(m_questRewardActor == null) && !(m_questRewardActor.GetEntityDef().GetCardId() != questRewardCardID))
			{
				return;
			}
			if (m_questRewardActor != null)
			{
				m_questRewardActor.Destroy();
			}
			using DefLoader.DisposableCardDef questRewardCardDef = DefLoader.Get().GetCardDef(questRewardCardID);
			if (questRewardCardDef == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateRewardCard(): Unable to load CardDef for card ID {0}.", questRewardCardID);
				return;
			}
			EntityDef questRewardEntityDef = DefLoader.Get().GetEntityDef(questRewardCardID);
			if (questRewardEntityDef == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateRewardCard(): Unable to load EntityDef for card ID {0}.", questRewardCardID);
				return;
			}
			GameObject questRewardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(questRewardEntityDef, originalQuestEntity.GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
			if (questRewardActorGO == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateRewardCard(): Unable to load Hand Actor for entity def {0}.", questRewardEntityDef);
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
			m_questRewardActor.UpdateDynamicTextFromQuestEntity(m_originalQuestActor.GetEntity());
			if (m_questRewardActor.UseCoinManaGem())
			{
				m_questRewardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			m_questRewardActor.UpdateAllComponents();
			UpdateRewardOverlayTexture(questRewardEntityDef);
			UpdateRewardBackgroundGlowTexture(questRewardEntityDef);
		}
	}

	private void UpdateEvolutionCardActor()
	{
		Entity originalQuestEntity = m_originalQuestActor.GetEntity();
		int databaseID = originalQuestEntity.GetTag(GAME_TAG.BACON_EVOLUTION_CARD_ID);
		if (databaseID == 0)
		{
			if (m_evolutionCardActor != null)
			{
				m_evolutionCardActor.Destroy();
			}
			return;
		}
		string cardID = GameUtils.TranslateDbIdToCardId(databaseID);
		if (string.IsNullOrEmpty(cardID))
		{
			if (m_evolutionCardActor != null)
			{
				m_evolutionCardActor.Destroy();
			}
		}
		else
		{
			if (!(m_evolutionCardActor == null) && !(m_evolutionCardActor.GetEntityDef().GetCardId() != cardID))
			{
				return;
			}
			if (m_evolutionCardActor != null)
			{
				m_evolutionCardActor.Destroy();
			}
			using DefLoader.DisposableCardDef evolutionCardDef = DefLoader.Get().GetCardDef(cardID);
			if (evolutionCardDef == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateEvolutionCardActor(): Unable to load CardDef for card ID {0}.", cardID);
				return;
			}
			EntityDef evolutionCardEntityDef = DefLoader.Get().GetEntityDef(cardID);
			if (evolutionCardEntityDef == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateEvolutionCardActor(): Unable to load EntityDef for card ID {0}.", cardID);
				return;
			}
			GameObject evolutionCardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(evolutionCardEntityDef, originalQuestEntity.GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
			if (evolutionCardActorGO == null)
			{
				Log.Gameplay.PrintError("QuestProgressUI.UpdateEvolutionCardActor(): Unable to load Hand Actor for entity def {0}.", evolutionCardEntityDef);
				return;
			}
			LayerUtils.SetLayer(evolutionCardActorGO, m_EvolutionCardBone.gameObject.layer, null);
			evolutionCardActorGO.transform.parent = m_EvolutionCardBone;
			TransformUtil.Identity(evolutionCardActorGO);
			m_evolutionCardActor = evolutionCardActorGO.GetComponentInChildren<Actor>();
			m_evolutionCardActor.SetEntityDef(evolutionCardEntityDef);
			m_evolutionCardActor.SetCardDef(evolutionCardDef);
			m_evolutionCardActor.SetPremium(m_originalQuestActor.GetEntity().GetPremiumType());
			m_evolutionCardActor.SetWatermarkCardSetOverride(m_originalQuestActor.GetEntity().GetWatermarkCardSetOverride());
			m_evolutionCardActor.UpdateDynamicTextFromQuestEntity(m_originalQuestActor.GetEntity());
			if (m_evolutionCardActor.UseTechLevelManaGem())
			{
				Spell techLevelSpell = m_evolutionCardActor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = evolutionCardEntityDef.GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else if (m_evolutionCardActor.UseCoinManaGem())
			{
				m_evolutionCardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			GameObject EvolutionVFX = GameObjectUtils.FindChildBySubstring(evolutionCardActorGO, "EvolutionVFX");
			if (EvolutionVFX != null)
			{
				EvolutionVFX.SetActive(value: true);
			}
			m_evolutionCardActor.UpdateAllComponents();
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
		if (!(m_RewardBackGlowRenderer == null))
		{
			Material material = m_DefaultRewardBackGlowMaterial;
			if (questRewardEntityDef.IsHeroPower())
			{
				material = m_HeroPowerRewardBackGlowMaterial;
			}
			m_RewardBackGlowRenderer.SetMaterial(material);
		}
	}
}
