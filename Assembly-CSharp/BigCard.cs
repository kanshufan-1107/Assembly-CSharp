using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[CustomEditClass]
public class BigCard : MonoBehaviour
{
	[Serializable]
	public class LayoutData
	{
		public float m_ScaleSec = 0.15f;

		public float m_DriftSec = 10f;
	}

	[Serializable]
	public class SecretLayoutOffsets
	{
		public Vector3 m_InitialOffset = new Vector3(0.1f, 5f, 3.3f);

		public Vector3 m_OpponentInitialOffset = new Vector3(0.1f, 5f, -3.3f);

		public Vector3 m_HiddenInitialOffset = new Vector3(0f, 4f, 4f);

		public Vector3 m_HiddenOpponentInitialOffset = new Vector3(0f, 4f, -4f);
	}

	[Serializable]
	public class SecretLayoutData
	{
		public float m_ShowAnimTime = 0.15f;

		public float m_HideAnimTime = 0.15f;

		public float m_DeathShowAnimTime = 1f;

		public float m_TimeUntilDeathSpell = 1.5f;

		public float m_DriftSec = 5f;

		public Vector3 m_DriftOffset = new Vector3(0f, 0f, 0.05f);

		public Vector3 m_Spacing = new Vector3(2.1f, 0f, 0.7f);

		public Vector3 m_HiddenSpacing = new Vector3(2.4f, 0f, 0.7f);

		public int m_MinCardThreshold = 1;

		public int m_MaxCardThreshold = 5;

		public SecretLayoutOffsets m_MinCardOffsets = new SecretLayoutOffsets();

		public SecretLayoutOffsets m_MaxCardOffsets = new SecretLayoutOffsets();
	}

	private struct KeywordArgs
	{
		public Card card;

		public Actor actor;

		public TooltipPanelManager.TooltipBoneSource boneSource;
	}

	private enum BigCardDisplay_RelativeBoardPosition
	{
		INVALID,
		LEFT,
		RIGHT,
		MIDDLE,
		IRRELEVANT
	}

	public LayoutData m_LayoutData;

	public SecretLayoutData m_SecretLayoutData;

	private static readonly Vector3 INVISIBLE_SCALE = new Vector3(0.0001f, 0.0001f, 0.0001f);

	private const string GHOST_CARD_BOTTOM = "GhostedCard_Bottom";

	private const string SECOND_BUDDYBONE_NAME = "SecondBuddyBone";

	private const string EVOLVING_BUDDY_BONE = "EvolvingBuddyBone";

	private const string SECOND_EVOLVING_BUDDY_BONE = "SecondEvolvingBuddyBone";

	private static BigCard s_instance;

	private Card m_card;

	private Actor m_bigCardActor;

	private TooltipPanel m_bigCardAsTooltip;

	private List<Actor> m_phoneSecretActors;

	private List<Actor> m_phoneSideQuestActors;

	private List<Actor> m_phoneSigilActors;

	private List<Actor> m_phoneObjectivesActors;

	private List<EnchantmentBanner> m_phoneEnchantmentBanners;

	private readonly PlatformDependentValue<float> PLATFORM_SCALING_FACTOR;

	private EnchantmentBanner m_enchantmentBanner;

	private Actor m_extraBigCardActor;

	public BigCard()
	{
		PLATFORM_SCALING_FACTOR = new PlatformDependentValue<float>(PlatformCategory.Screen)
		{
			PC = 1f,
			Tablet = 1f,
			Phone = 1.3f,
			MiniTablet = 1f
		};
	}

	public PlatformDependentValue<float> GetPlatformScalingFactor()
	{
		return PLATFORM_SCALING_FACTOR;
	}

	private void Awake()
	{
		s_instance = this;
		m_enchantmentBanner = LoadEnchantmentBannerObject();
	}

	private EnchantmentBanner LoadEnchantmentBannerObject()
	{
		return AssetLoader.Get().InstantiatePrefab("EnchantmentBanner.prefab:e7058664cd0b13f4bb45e5b5f0385f34", AssetLoadingOptions.IgnorePrefabPosition).GetComponent<EnchantmentBanner>();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static BigCard Get()
	{
		return s_instance;
	}

	public Card GetCard()
	{
		return m_card;
	}

	public Actor GetExtraBigCardActor()
	{
		return m_extraBigCardActor;
	}

	public Actor GetBigCardActor()
	{
		return m_bigCardActor;
	}

	public void Show(Card card)
	{
		m_card = card;
		if (GameState.Get() != null && !GameState.Get().GetGameEntity().NotifyOfCardTooltipDisplayShow(card))
		{
			return;
		}
		Zone zone = card.GetZone();
		if ((bool)UniversalInputManager.UsePhoneUI && zone is ZoneSecret)
		{
			if (card.GetEntity().IsBobQuest())
			{
				DisplayBigCardAsTooltip();
			}
			else if (card.GetEntity().IsSideQuest())
			{
				LoadAndDisplayTooltipPhoneSideQuests();
			}
			else if (card.GetEntity().IsSigil())
			{
				LoadAndDisplayTooltipPhoneSigils();
			}
			else if (card.GetEntity().IsObjective())
			{
				LoadAndDisplayTooltipPhoneObjectives();
			}
			else
			{
				LoadAndDisplayTooltipPhoneSecrets();
			}
		}
		else
		{
			LoadAndDisplayBigCard();
		}
	}

	public void Hide()
	{
		if (GameState.Get() != null)
		{
			GameState.Get().GetGameEntity().NotifyOfCardTooltipDisplayHide(m_card);
		}
		HideBigCard();
		HideTooltipPhoneSecrets();
		HideTooltipPhoneSideQuests();
		HideTooltipPhoneSigils();
		HideTooltipPhoneObjectives();
		HidePhoneEnchantmentBanners();
		m_card = null;
	}

	public bool Hide(Card card)
	{
		if (m_card != card)
		{
			return false;
		}
		Hide();
		return true;
	}

	public void ShowSecretDeaths(Map<Player, DeadSecretGroup> deadSecretMap)
	{
		if (deadSecretMap == null || deadSecretMap.Count == 0)
		{
			return;
		}
		int count = 0;
		foreach (DeadSecretGroup value in deadSecretMap.Values)
		{
			Card mainCard = value.GetMainCard();
			List<Card> cards = value.GetCards();
			List<Actor> actors = new List<Actor>();
			for (int i = 0; i < cards.Count; i++)
			{
				Card card = cards[i];
				Actor actor = LoadPhoneSecret(card);
				actors.Add(actor);
			}
			DisplayPhoneSecrets(mainCard, actors, showDeath: true);
			count++;
		}
	}

	private void LoadAndDisplayBigCard()
	{
		ResetAllEnchantmentBanners();
		if ((bool)m_extraBigCardActor)
		{
			m_extraBigCardActor.Destroy();
		}
		if ((bool)m_bigCardActor)
		{
			m_bigCardActor.Destroy();
		}
		if (ActorNames.ShouldDisplayTooltipInsteadOfBigCard(m_card.GetEntity()))
		{
			DisplayBigCardAsTooltip();
			return;
		}
		string assetRef = ActorNames.GetBigCardActor(m_card.GetEntity());
		if (assetRef == "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9")
		{
			return;
		}
		LoadPrimaryBigCard(assetRef);
		LoadSecondaryBigCardIfNeeded();
		if (CanUseBonesForBigCardPlacement())
		{
			try
			{
				DisplayBigCardWithBones();
				return;
			}
			catch (Exception e)
			{
				LogUncaughtBigCardException(e);
				return;
			}
		}
		LogDisplayBigCardError();
	}

	private void LogUncaughtBigCardException(Exception e)
	{
		string cardName = ((!(m_card == null) && !string.IsNullOrEmpty(m_card.name)) ? m_card.name : "[Unknown card name]");
		string message = "Uncaught exception while displaying big card for \"" + cardName + "\"";
		message = ((e == null) ? (message + ", but no exception was captured.") : (message + $"\n{e.GetType()}: {e.Message}\n{e.StackTrace}"));
		Log.Gameplay.PrintError(message);
	}

	private void LogDisplayBigCardError()
	{
		string cardName = ((!(m_card == null) && !string.IsNullOrEmpty(m_card.name)) ? m_card.name : "[Unknown card name]");
		string message = "Unable to display big card for \"" + cardName + "\".";
		Log.Gameplay.PrintError(message);
	}

	private void LoadPrimaryBigCard(string assetRef)
	{
		if (!string.IsNullOrEmpty(assetRef))
		{
			GameObject actorObject = AssetLoader.Get().InstantiatePrefab(assetRef, AssetLoadingOptions.IgnorePrefabPosition);
			m_bigCardActor = actorObject.GetComponent<Actor>();
			SetupActor(m_card, m_bigCardActor);
		}
	}

	private void LoadSecondaryBigCardIfNeeded()
	{
		Entity extraEntity = GameState.Get().GetGameEntity().GetExtraMouseOverBigCardEntity(m_card.GetEntity());
		if (extraEntity != null)
		{
			string extraEntityAssetRef = ActorNames.GetBigCardActor(extraEntity);
			if (extraEntityAssetRef != "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9")
			{
				GameObject extraEntityActorObject = AssetLoader.Get().InstantiatePrefab(extraEntityAssetRef, AssetLoadingOptions.IgnorePrefabPosition);
				m_extraBigCardActor = extraEntityActorObject.GetComponent<Actor>();
				SetupActor(extraEntity.GetCard(), m_extraBigCardActor);
				m_extraBigCardActor.transform.parent = m_bigCardActor.transform;
				Vector3 extraBigCardScale = new Vector3(0.75f, 1f, 0.75f);
				m_extraBigCardActor.transform.localScale = extraBigCardScale;
				return;
			}
		}
		int evolvingCardID = m_card.GetEntity().GetTag(GAME_TAG.BACON_EVOLUTION_CARD_ID);
		if (evolvingCardID <= 0)
		{
			return;
		}
		using DefLoader.DisposableFullDef evolvingCardDef = DefLoader.Get().GetFullDef(evolvingCardID);
		string evolvingCardAssetRef = GetEvolvingActorPrefab(evolvingCardDef);
		GameObject evolvingCardObject = AssetLoader.Get().InstantiatePrefab(evolvingCardAssetRef, AssetLoadingOptions.IgnorePrefabPosition);
		m_extraBigCardActor = evolvingCardObject.GetComponent<Actor>();
		SetupActor(m_card, m_extraBigCardActor);
		m_extraBigCardActor.SetEntity(null);
		m_extraBigCardActor.transform.parent = m_bigCardActor.transform;
		m_extraBigCardActor.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
		GameObject EvolutionVFX = GameObjectUtils.FindChildBySubstring(evolvingCardObject, "EvolutionVFX");
		if (EvolutionVFX != null)
		{
			EvolutionVFX.SetActive(value: true);
		}
		m_extraBigCardActor.SetFullDef(evolvingCardDef);
		m_extraBigCardActor.SetPremium(m_card.GetEntity().GetPremiumType());
		m_extraBigCardActor.SetCardBackSideOverride(m_card.GetEntity().GetControllerSide());
		m_extraBigCardActor.SetWatermarkCardSetOverride(m_card.GetEntity().GetWatermarkCardSetOverride());
		m_extraBigCardActor.UpdateAllComponents();
		ActivateBigCardStateSpells(m_card.GetEntity(), m_extraBigCardActor, m_extraBigCardActor, evolvingCardDef?.EntityDef);
	}

	private string GetEvolvingActorPrefab(DefLoader.DisposableFullDef evolvingCardDef)
	{
		if (evolvingCardDef == null || evolvingCardDef.EntityDef == null)
		{
			return string.Empty;
		}
		EntityDef entityDef = evolvingCardDef.EntityDef;
		if (entityDef.GetTag(GAME_TAG.CARDTYPE) != 10)
		{
			return ActorNames.GetHandActor(entityDef, (TAG_PREMIUM)entityDef.GetTag(GAME_TAG.PREMIUM));
		}
		bool isFriendly = m_card.GetEntity().IsControlledByFriendlySidePlayer();
		if (m_card.GetPremium() != 0)
		{
			if (isFriendly)
			{
				return "History_HeroPower_Premium.prefab:081da807b95b8495e9f16825c5164787";
			}
			return "History_HeroPower_Opponent_Premium.prefab:82e1456f33aae4b3d9b2dac73aaa3ffa";
		}
		if (isFriendly)
		{
			return "History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53";
		}
		return "History_HeroPower_Opponent.prefab:a99d23d6e8630f94b96a8e096fffb16f";
	}

	private bool CanUseBonesForBigCardPlacement()
	{
		if (m_card == null)
		{
			return false;
		}
		if (m_bigCardActor == null)
		{
			return false;
		}
		Actor inPlayActor = m_card.GetActor();
		if (inPlayActor == null)
		{
			return false;
		}
		if (m_card.GetEntity().IsLettuceAbility())
		{
			MercenariesAbilityTray tray = ZoneMgr.Get().GetLettuceZoneController().GetAbilityTray();
			if (tray == null)
			{
				return false;
			}
			tray.GetBigCardBones(out var leftBone, out var rightBone);
			if (leftBone == null || rightBone == null)
			{
				return false;
			}
		}
		else
		{
			BigCardDisplayBones bigCardBones = inPlayActor.GetComponentInChildren<BigCardDisplayBones>();
			if (bigCardBones == null)
			{
				return false;
			}
			if (!bigCardBones.HasBonesForCurrentPlatform())
			{
				return false;
			}
		}
		return true;
	}

	private void HideBigCard()
	{
		if ((bool)m_extraBigCardActor)
		{
			m_extraBigCardActor.Destroy();
			m_extraBigCardActor = null;
		}
		if ((bool)m_bigCardActor)
		{
			Card card = m_bigCardActor.GetCard();
			if (card != null)
			{
				Actor actor = card.GetActor();
				if (actor != null && actor.gameObject != null)
				{
					HeroBuddyWidgetProgressBar widget = actor.GetComponent<HeroBuddyWidgetProgressBar>();
					if (widget != null)
					{
						widget.ShowProgressText(value: false);
					}
				}
			}
			ResetAllEnchantmentBanners();
			iTween.Stop(m_bigCardActor.gameObject);
			m_bigCardActor.Destroy();
			m_bigCardActor = null;
			TooltipPanelManager.Get().HideKeywordHelp();
		}
		if ((bool)m_bigCardAsTooltip)
		{
			UnityEngine.Object.Destroy(m_bigCardAsTooltip.gameObject);
		}
	}

	private void ResetAllEnchantmentBanners()
	{
		m_enchantmentBanner?.ResetEnchantments();
		if (m_phoneEnchantmentBanners == null)
		{
			return;
		}
		foreach (EnchantmentBanner phoneEnchantmentBanner in m_phoneEnchantmentBanners)
		{
			phoneEnchantmentBanner?.ResetEnchantments();
		}
	}

	private void DisplayBigCardAsTooltip()
	{
		if (m_bigCardAsTooltip != null)
		{
			UnityEngine.Object.Destroy(m_bigCardAsTooltip.gameObject);
		}
		Entity entity = m_card.GetEntity();
		Vector3 cardOffset;
		if (entity != null && entity.IsBobQuest())
		{
			cardOffset = ((!UniversalInputManager.UsePhoneUI) ? new Vector3(0f, 0f, 1.33f) : new Vector3(0f, 0f, 2.478f));
			if (m_card.GetControllerSide() == Player.Side.OPPOSING)
			{
				cardOffset.z *= -1f;
			}
		}
		else if (entity != null && entity.IsBattlegroundTrinket() && entity.HasTag(GAME_TAG.BACON_IS_POTENTIAL_TRINKET))
		{
			cardOffset = ((!UniversalInputManager.UsePhoneUI) ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, 2f));
			if (m_card.GetControllerSide() == Player.Side.OPPOSING)
			{
				cardOffset.z *= -1f;
			}
		}
		else
		{
			cardOffset = new Vector3(2f, 0f, 0f);
		}
		Vector3 tipPosition = m_card.transform.position + cardOffset;
		m_bigCardAsTooltip = TooltipPanelManager.Get().CreateKeywordPanel(0);
		m_bigCardAsTooltip.Reset();
		m_bigCardAsTooltip.Initialize(m_card.GetEntity().GetName(), m_card.GetEntity().GetCardTextInHand());
		m_bigCardAsTooltip.SetScale(TooltipPanel.GAMEPLAY_SCALE);
		m_bigCardAsTooltip.transform.position = tipPosition;
		RenderUtils.SetAlpha(m_bigCardAsTooltip.gameObject, 0f);
		iTween.FadeTo(m_bigCardAsTooltip.gameObject, iTween.Hash("alpha", 1f, "time", 0.1f));
	}

	private BigCardDisplay_RelativeBoardPosition GetBoardPositionOfSecretZoneCard()
	{
		return BigCardDisplay_RelativeBoardPosition.LEFT;
	}

	private BigCardDisplay_RelativeBoardPosition GetBoardPositionOfPlayZoneCard()
	{
		Zone zone = m_card.GetZone();
		if ((zone is ZonePlay || zone is ZoneHeroPower || zone is ZoneWeapon || zone is ZoneBattlegroundTrinket || m_card.GetEntity().IsBattlegroundTrinket()) && !m_card.GetEntity().IsLocation() && !m_card.GetEntity().IsMinion() && !m_card.GetEntity().IsBaconSpell())
		{
			return BigCardDisplay_RelativeBoardPosition.IRRELEVANT;
		}
		if (m_card.GetZone() == null)
		{
			return BigCardDisplay_RelativeBoardPosition.RIGHT;
		}
		int zonePosition = m_card.GetZonePosition();
		float middlePosition = (float)(m_card.GetZone().GetCardCount() + 1) / 2f;
		float distanceToMiddleIndex = (float)zonePosition - middlePosition;
		if (Mathf.Abs(distanceToMiddleIndex) <= 0.5f)
		{
			return BigCardDisplay_RelativeBoardPosition.MIDDLE;
		}
		if (distanceToMiddleIndex < 0f)
		{
			return BigCardDisplay_RelativeBoardPosition.LEFT;
		}
		return BigCardDisplay_RelativeBoardPosition.RIGHT;
	}

	private BigCardDisplay_RelativeBoardPosition GetBoardPositionOfSourceCard()
	{
		if (m_card == null)
		{
			return BigCardDisplay_RelativeBoardPosition.INVALID;
		}
		if (m_card.GetZone() is ZoneSecret)
		{
			return GetBoardPositionOfSecretZoneCard();
		}
		return GetBoardPositionOfPlayZoneCard();
	}

	private float GetScaleForCard(BigCardBoneLayout.ScaleSettings platformScale, Card card, bool queryForSelf)
	{
		if (platformScale == null || card == null)
		{
			return 1f;
		}
		float scalar = 1f;
		if (queryForSelf)
		{
			Entity ent = card.GetEntity();
			scalar = ((!ent.IsBattlegroundTrinket() || ent.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6) != 3) ? platformScale.m_BigCardScale_Self : platformScale.m_BigCardScale_BaconTrinketHeropower);
		}
		else
		{
			Entity ent2 = card.GetEntity();
			if (ent2.IsMinion())
			{
				scalar = platformScale.m_BigCardScale_Minion;
			}
			else if (ent2.IsLettuceAbility())
			{
				scalar = platformScale.m_BigCardScale_LettuceAbility;
			}
			else if (ent2.IsHero())
			{
				scalar = platformScale.m_BigCardScale_Hero;
			}
			else if (ent2.IsSpell())
			{
				scalar = platformScale.m_BigCardScale_Spell;
			}
			else if (ent2.IsWeapon())
			{
				scalar = platformScale.m_BigCardScale_Weapon;
			}
			else if (ent2.IsHeroPower())
			{
				scalar = platformScale.m_BigCardScale_HeroPower;
			}
			else if (ent2.IsLocation())
			{
				scalar = platformScale.m_BigCardScale_Location;
			}
			else if (ent2.IsBaconSpell())
			{
				scalar = platformScale.m_BigCardScale_TavernSpell;
			}
			else if (ent2.IsBattlegroundTrinket())
			{
				scalar = ((ent2.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6) != 3) ? platformScale.m_BigCardScale_BaconTrinket : platformScale.m_BigCardScale_BaconTrinketHeropower);
			}
			else
			{
				Log.All.PrintWarning("TODO: implement scale for cardtype " + card.gameObject.name + ".");
			}
		}
		return scalar;
	}

	private Vector3 AdjustYValueToBeLevelOnBoard(Vector3 bonePosition, Zone ownerZone)
	{
		ZonePlay play = ownerZone as ZonePlay;
		if (play == null)
		{
			return bonePosition;
		}
		float y = play.GetComponent<Collider>().bounds.center.y;
		float magicOffset = ((!UniversalInputManager.UsePhoneUI) ? 0.33f : 0.3f);
		float finalDesiredHeight = y + magicOffset;
		bonePosition.y = finalDesiredHeight;
		return bonePosition;
	}

	private void BigCardBones_UpdateEnchantmentBanner(BigCardBoneLayout.ScaleSettings platformScale, float cardScale)
	{
		if (m_bigCardActor.GetCard().GetZone() is ZoneHand)
		{
			m_bigCardActor.SetEntity(m_bigCardActor.GetEntity());
			m_bigCardActor.UpdateTextComponents(m_bigCardActor.GetEntity());
			return;
		}
		m_enchantmentBanner.UpdateEnchantments(m_card, m_bigCardActor, platformScale.m_EnchantmentBannerScale * cardScale);
		if ((bool)UniversalInputManager.UsePhoneUI && m_enchantmentBanner.IsBannerVisible())
		{
			int enchantmentCount = ((m_enchantmentBanner.GetEnchantmentCount() > 3) ? 3 : m_enchantmentBanner.GetEnchantmentCount());
			float shrinkScale = 1f - (float)enchantmentCount * platformScale.m_BigCardScaleFactorForEnchantments;
			m_bigCardActor.transform.localScale = m_bigCardActor.transform.localScale * shrinkScale;
		}
	}

	private void BigCardBones_ShowTooltips(TooltipPanelManager.TooltipBoneSource boneSource)
	{
		if (GameState.Get() != null)
		{
			GameState.Get().GetGameEntity().NotifyOfCardTooltipBigCardActorShow();
		}
		KeywordArgs keywordArgs = default(KeywordArgs);
		keywordArgs.card = m_card;
		keywordArgs.actor = m_bigCardActor;
		keywordArgs.boneSource = boneSource;
		KeywordArgs keywordArgs2 = keywordArgs;
		BigCardDisplayBones bigCardBones = m_card.GetActor().GetComponentInChildren<BigCardDisplayBones>();
		if (!(bigCardBones == null))
		{
			bigCardBones.GetRigForCurrentPlatform(out var _, out var platformScale);
			float? overrideKeywordScale = null;
			if (m_card.GetEntity().IsHeroPower())
			{
				overrideKeywordScale = 0.6f * platformScale.m_BigCardScale_Tooltip;
			}
			TooltipPanelManager.Get().UpdateKeywordHelp(keywordArgs2.card, keywordArgs2.actor, keywordArgs2.boneSource, overrideKeywordScale, null);
		}
	}

	private void BigCardBones_ShowStateSpells()
	{
		if (m_card.GetEntity().IsSilenced())
		{
			m_bigCardActor.ActivateSpellBirthState(SpellType.SILENCE);
		}
	}

	private void BigCardBones_ScaleAndPlaceBigCard(Actor bigCardActor, Zone actorZone, Vector3 scale, GameObject bone, bool cardIsInPlay)
	{
		if (!(bigCardActor == null) && !(bone == null))
		{
			bigCardActor.transform.position = AdjustYValueToBeLevelOnBoard(bone.transform.position, actorZone);
			bigCardActor.transform.localScale = scale;
			if (!cardIsInPlay)
			{
				bigCardActor.transform.rotation = Quaternion.identity;
			}
		}
	}

	private void BigCardBones_ActivateAndScaleIn(TooltipPanelManager.TooltipBoneSource tooltipBoneSource)
	{
		if (m_bigCardActor != null)
		{
			Vector3 bigCardOriginalScale = m_bigCardActor.transform.localScale;
			m_bigCardActor.transform.localScale = Vector3.one;
			TooltipPanelManager.TooltipBoneSource? onCompleteArgs = tooltipBoneSource;
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", bigCardOriginalScale);
			scaleArgs.Add("time", m_LayoutData.m_ScaleSec);
			scaleArgs.Add("oncompleteparams", onCompleteArgs);
			scaleArgs.Add("oncomplete", (Action<object>)delegate(object boneSource)
			{
				TooltipPanelManager.TooltipBoneSource? tooltipBoneSource2 = boneSource as TooltipPanelManager.TooltipBoneSource?;
				TooltipPanelManager.TooltipBoneSource boneSource2 = ((tooltipBoneSource2.HasValue && tooltipBoneSource2.HasValue) ? tooltipBoneSource2.Value : TooltipPanelManager.TooltipBoneSource.TOP_LEFT);
				BigCardBones_ShowTooltips(boneSource2);
			});
			iTween.ScaleTo(m_bigCardActor.gameObject, scaleArgs);
		}
		if (m_extraBigCardActor != null)
		{
			Vector3 extraCardOriginalScale = m_bigCardActor.transform.localScale;
			m_extraBigCardActor.transform.localScale = Vector3.one;
			iTween.ScaleTo(m_extraBigCardActor.gameObject, extraCardOriginalScale, m_LayoutData.m_ScaleSec);
		}
		if (m_bigCardActor != null)
		{
			m_bigCardActor.Show();
		}
	}

	private void DisplayCardInPlayWithBones(out TooltipPanelManager.TooltipBoneSource tooltipBoneSource)
	{
		Actor inPlayActor = m_card.GetActor();
		tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
		if (inPlayActor == null)
		{
			return;
		}
		BigCardDisplayBones bigCardBones = inPlayActor.GetComponentInChildren<BigCardDisplayBones>();
		if (bigCardBones == null)
		{
			return;
		}
		bigCardBones.GetRigForCurrentPlatform(out var boneRig, out var platformScale);
		if (boneRig == null)
		{
			return;
		}
		BigCardBoneLayout boneLayout = boneRig.GetComponent<BigCardBoneLayout>();
		if (boneLayout == null)
		{
			return;
		}
		GameObject mainCardBone;
		GameObject extraBigCardBone;
		switch (GetBoardPositionOfSourceCard())
		{
		case BigCardDisplay_RelativeBoardPosition.LEFT:
			mainCardBone = boneLayout.m_InnerRightBone;
			extraBigCardBone = boneLayout.m_OuterRightBone;
			tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
			break;
		case BigCardDisplay_RelativeBoardPosition.RIGHT:
			mainCardBone = boneLayout.m_InnerLeftBone;
			extraBigCardBone = boneLayout.m_OuterLeftBone;
			tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
			break;
		case BigCardDisplay_RelativeBoardPosition.MIDDLE:
			mainCardBone = boneLayout.m_InnerLeftBone;
			extraBigCardBone = boneLayout.m_InnerRightBone;
			tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
			break;
		case BigCardDisplay_RelativeBoardPosition.IRRELEVANT:
		{
			Entity ent = m_card.GetEntity();
			if (ent.IsWeapon())
			{
				mainCardBone = boneLayout.m_InnerRightBone;
				extraBigCardBone = boneLayout.m_OuterRightBone;
				tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
			}
			else if (ent.IsHeroPower())
			{
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					if (ent.IsExhausted())
					{
						mainCardBone = boneLayout.m_InnerRightBone;
						extraBigCardBone = boneLayout.m_OuterRightBone;
					}
					else
					{
						mainCardBone = boneLayout.m_InnerLeftBone;
						extraBigCardBone = boneLayout.m_OuterLeftBone;
					}
				}
				else if (ent.IsExhausted())
				{
					mainCardBone = boneLayout.m_InnerLeftBone;
					extraBigCardBone = boneLayout.m_OuterLeftBone;
				}
				else
				{
					mainCardBone = boneLayout.m_InnerRightBone;
					extraBigCardBone = boneLayout.m_OuterRightBone;
				}
				tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
			}
			else if (ent.IsBattlegroundTrinket())
			{
				if (ent.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6) == 3)
				{
					mainCardBone = boneLayout.m_InnerLeftBone;
					extraBigCardBone = boneLayout.m_OuterLeftBone;
					tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
				}
				else
				{
					mainCardBone = boneLayout.m_InnerRightBone;
					extraBigCardBone = boneLayout.m_OuterRightBone;
					tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
				}
			}
			else
			{
				if (!ent.IsAnomaly() && !ent.IsBattlegroundQuestReward() && !ent.IsBattlegroundHeroBuddy())
				{
					Log.Gameplay.PrintError($"Unknown card type ({ent.GetCardType()}) used in {MethodBase.GetCurrentMethod().Name} while trying to display big cards.");
					return;
				}
				mainCardBone = boneLayout.m_InnerLeftBone;
				extraBigCardBone = boneLayout.m_OuterLeftBone;
				tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
			}
			break;
		}
		default:
			Log.Gameplay.PrintError("Unknown value for BigCardDisplay_RelativeBoardPosition.");
			return;
		}
		CheckIfTooltipSideShouldBeFlipped(ref tooltipBoneSource);
		Transform boneRigPreviousParent = boneRig.transform.parent;
		Vector3 rigPreviousScale = boneRig.transform.localScale;
		boneRig.transform.parent = null;
		boneRig.transform.localScale = Vector3.one;
		Zone currentZone = m_card.GetZone();
		bool cardIsInBattlefield = currentZone is ZonePlay || currentZone is ZoneSecret || currentZone is ZoneTeammatePlay || currentZone is ZoneWeapon;
		bool scaleAndPositionExtraBigCardActor = GameMgr.Get().IsBattlegrounds() || cardIsInBattlefield;
		Zone mainActorZone = m_bigCardActor.GetCard().GetZone();
		if (m_bigCardActor != null)
		{
			float cardScale = GetScaleForCard(platformScale, m_bigCardActor.GetCard(), queryForSelf: true);
			Vector3 scaleVector = Vector3.one * cardScale;
			BigCardBones_ScaleAndPlaceBigCard(m_bigCardActor, mainActorZone, scaleVector, mainCardBone, cardIsInBattlefield);
			if (cardIsInBattlefield)
			{
				BigCardBones_UpdateEnchantmentBanner(platformScale, cardScale);
			}
		}
		if (m_extraBigCardActor != null && scaleAndPositionExtraBigCardActor)
		{
			float cardScale2 = GetScaleForCard(platformScale, m_extraBigCardActor.GetCard(), queryForSelf: false);
			Vector3 scaleVector2 = Vector3.one * cardScale2;
			BigCardBones_ScaleAndPlaceBigCard(m_extraBigCardActor, mainActorZone, scaleVector2, extraBigCardBone, cardIsInBattlefield);
		}
		boneRig.transform.localScale = rigPreviousScale;
		boneRig.transform.parent = boneRigPreviousParent;
	}

	private void CheckIfTooltipSideShouldBeFlipped(ref TooltipPanelManager.TooltipBoneSource boneSource)
	{
		if (!(m_extraBigCardActor != null) && !(m_card.GetZone() is ZoneSecret))
		{
			switch (boneSource)
			{
			case TooltipPanelManager.TooltipBoneSource.TOP_LEFT:
				boneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
				break;
			case TooltipPanelManager.TooltipBoneSource.TOP_RIGHT:
				boneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
				break;
			case TooltipPanelManager.TooltipBoneSource.BOTTOM_LEFT:
				boneSource = TooltipPanelManager.TooltipBoneSource.BOTTOM_RIGHT;
				break;
			case TooltipPanelManager.TooltipBoneSource.BOTTOM_RIGHT:
				boneSource = TooltipPanelManager.TooltipBoneSource.BOTTOM_LEFT;
				break;
			}
		}
	}

	private void DisplayLettuceAbilitiesWithBones(out TooltipPanelManager.TooltipBoneSource tooltipBoneSource)
	{
		MercenariesAbilityTray tray = ZoneMgr.Get().GetLettuceZoneController().GetAbilityTray();
		tray.GetBigCardBones(out var leftBone, out var rightBone);
		GameObject bigCardBone;
		if (tray.GetTrayPositionOfAbility(m_card) < 2)
		{
			bigCardBone = leftBone;
			tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
		}
		else
		{
			bigCardBone = rightBone;
			tooltipBoneSource = TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
		}
		if (!(bigCardBone == null))
		{
			float abilityCardScale = tray.GetAbilityPreviewScaleForCurrentPlatform();
			if (abilityCardScale <= 0f)
			{
				Debug.LogError($"Getting the ability card scale from the ability tray's scale settings returned an invalid scale value of {abilityCardScale} when it should be a positive value. Changing value to 1.0.");
				abilityCardScale = 1f;
			}
			Vector3 cardScale = Vector3.one * abilityCardScale;
			BigCardBones_ScaleAndPlaceBigCard(m_bigCardActor, m_bigCardActor.GetCard().GetZone(), cardScale, bigCardBone, cardIsInPlay: true);
		}
	}

	private void DisplayBigCardWithBones()
	{
		TooltipPanelManager.TooltipBoneSource tooltipSourceBone = TooltipPanelManager.TooltipBoneSource.INVALID;
		Entity entity = m_card.GetEntity();
		if (entity != null)
		{
			if (entity.IsLettuceAbility())
			{
				DisplayLettuceAbilitiesWithBones(out tooltipSourceBone);
			}
			else
			{
				DisplayCardInPlayWithBones(out tooltipSourceBone);
			}
			FitInsideScreenVerticalAxis();
			BigCardBones_ShowStateSpells();
			BigCardBones_ActivateAndScaleIn(tooltipSourceBone);
			(GameState.Get()?.GetGameEntity())?.NotifyOfBigCardForCardInPlayShown(m_bigCardActor, m_extraBigCardActor);
		}
	}

	private void FitInsideScreenVerticalAxis()
	{
		FitInsideScreenBottom();
		FitInsideScreenTop();
	}

	private Bounds CalculateBoundsOfSeveralMeshes(Actor actor)
	{
		if (actor == null)
		{
			return new Bounds(Vector3.zero, Vector3.zero);
		}
		Bounds actorBounds = actor.GetMeshRenderer().bounds;
		if (actor.m_meshesThatAffectBoundsCalculations != null)
		{
			foreach (MeshRenderer subMesh in actor.m_meshesThatAffectBoundsCalculations)
			{
				if (subMesh == null)
				{
					Debug.LogWarning("Actor \"" + actor.gameObject.name + "\" has a null entry in the m_meshesThatAffectBoundsCalculations array.");
				}
				else
				{
					actorBounds.Encapsulate(subMesh.bounds);
				}
			}
		}
		return actorBounds;
	}

	private Bounds CalculateLowerMeshBounds(Actor actor = null)
	{
		if (actor == null)
		{
			actor = m_bigCardActor;
		}
		if (m_enchantmentBanner.IsBannerVisible())
		{
			return m_enchantmentBanner.GetLowerMeshBounds();
		}
		if (actor.m_meshesThatAffectBoundsCalculations != null && actor.m_meshesThatAffectBoundsCalculations.Count > 0)
		{
			return CalculateBoundsOfSeveralMeshes(actor);
		}
		return actor.GetMeshRenderer().bounds;
	}

	private bool FitInsideScreenBottom()
	{
		Bounds bounds = CalculateLowerMeshBounds();
		Vector3 center = bounds.center;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			center.z -= 0.4f;
		}
		Vector3 bottom = new Vector3(center.x, center.y, center.z - bounds.extents.z);
		Ray downwardRay = new Ray(bottom, bottom - center);
		Plane bottomPlane = CameraUtils.CreateBottomPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip));
		float intersectDist = 0f;
		if (bottomPlane.Raycast(downwardRay, out intersectDist))
		{
			return false;
		}
		if (Mathf.Approximately(intersectDist, 0f))
		{
			return false;
		}
		TransformUtil.SetPosZ(m_bigCardActor.gameObject, m_bigCardActor.transform.position.z - intersectDist);
		return true;
	}

	private Bounds CalculateMeshBoundsIncludingGem(Actor actor = null)
	{
		if (actor == null)
		{
			actor = m_bigCardActor;
		}
		if (actor.m_meshesThatAffectBoundsCalculations != null && actor.m_meshesThatAffectBoundsCalculations.Count > 0)
		{
			return CalculateBoundsOfSeveralMeshes(actor);
		}
		Bounds actorBounds = actor.GetMeshRenderer().bounds;
		if (actor != null && actor.GetEntity() != null && (actor.GetEntity().IsSideQuest() || actor.GetEntity().IsSigil() || actor.GetEntity().IsObjective()))
		{
			MeshRenderer[] componentsInChildren = actor.GetRootObject().GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mesh in componentsInChildren)
			{
				if (mesh.gameObject.name.Equals("gem_mana", StringComparison.InvariantCultureIgnoreCase))
				{
					Bounds gemBounds = mesh.bounds;
					actorBounds.Encapsulate(gemBounds);
					break;
				}
			}
		}
		return actorBounds;
	}

	private bool FitInsideScreenTop()
	{
		Bounds actorBounds = CalculateMeshBoundsIncludingGem();
		Vector3 actorCenter = actorBounds.center;
		if ((bool)UniversalInputManager.UsePhoneUI && !(m_card.GetZone() is ZoneHeroPower) && !(m_card.GetZone() is ZoneBattlegroundHeroBuddy))
		{
			actorCenter.z += 1f;
		}
		Vector3 actorTop = new Vector3(actorCenter.x, actorCenter.y, actorCenter.z + actorBounds.extents.z);
		Ray actorUpwardRay = new Ray(actorTop, actorTop - actorCenter);
		Plane topPlane = CameraUtils.CreateTopPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip));
		float intersectDist = 0f;
		if (topPlane.Raycast(actorUpwardRay, out intersectDist))
		{
			return false;
		}
		if (Mathf.Approximately(intersectDist, 0f))
		{
			return false;
		}
		TransformUtil.SetPosZ(m_bigCardActor.gameObject, m_bigCardActor.transform.position.z + intersectDist);
		return true;
	}

	private void FitInsideScreenHorizontalAxis()
	{
		FitInsideScreenLeft();
		FitInsideScreenRight();
	}

	private bool FitInsideScreenLeft()
	{
		Bounds actorWithTooltipAndAbilityBounds = ComputeBoundsOfBigCardExtraCardAndTooltips();
		Vector3 actorCenter = actorWithTooltipAndAbilityBounds.center;
		Vector3 actorLeft = new Vector3(actorCenter.x - actorWithTooltipAndAbilityBounds.extents.x, actorCenter.y, actorCenter.z);
		Ray actorLeftwardRay = new Ray(actorLeft, actorLeft - actorCenter);
		if (CameraUtils.CreateLeftPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip)).Raycast(actorLeftwardRay, out var intersectDist))
		{
			return false;
		}
		if (Mathf.Approximately(intersectDist, 0f))
		{
			return false;
		}
		TransformUtil.SetPosX(m_bigCardActor.gameObject, m_bigCardActor.transform.position.x + intersectDist);
		return true;
	}

	private bool FitInsideScreenRight()
	{
		Bounds actorWithTooltipAndAbilityBounds = ComputeBoundsOfBigCardExtraCardAndTooltips();
		Vector3 actorCenter = actorWithTooltipAndAbilityBounds.center;
		Vector3 actorRight = new Vector3(actorCenter.x + actorWithTooltipAndAbilityBounds.extents.x, actorCenter.y, actorCenter.z);
		Ray actorRightwardRay = new Ray(actorRight, actorRight - actorCenter);
		if (CameraUtils.CreateRightPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip)).Raycast(actorRightwardRay, out var intersectDist))
		{
			return false;
		}
		if (Mathf.Approximately(intersectDist, 0f))
		{
			return false;
		}
		TransformUtil.SetPosX(m_bigCardActor.gameObject, m_bigCardActor.transform.position.x + intersectDist);
		return true;
	}

	private Bounds ComputeBoundsOfBigCardExtraCardAndTooltips()
	{
		Bounds actorBounds = m_bigCardActor.GetMeshRenderer().bounds;
		List<TooltipPanel> tooltipPanels = TooltipPanelManager.Get().GetCurrentTooltipPanels();
		if (tooltipPanels != null)
		{
			foreach (TooltipPanel item in tooltipPanels)
			{
				MeshRenderer[] renderers = item.gameObject.GetComponentsInChildren<MeshRenderer>();
				if (renderers != null)
				{
					MeshRenderer[] array = renderers;
					foreach (MeshRenderer renderer in array)
					{
						actorBounds.Encapsulate(renderer.bounds);
					}
				}
			}
		}
		if (m_extraBigCardActor != null)
		{
			MeshRenderer[] renderers2 = m_extraBigCardActor.GetComponentsInChildren<MeshRenderer>();
			if (renderers2 != null)
			{
				MeshRenderer[] array = renderers2;
				foreach (MeshRenderer renderer2 in array)
				{
					actorBounds.Encapsulate(renderer2.bounds);
				}
			}
		}
		return actorBounds;
	}

	public void ActivateBigCardStateSpells(Entity entity, Actor cardActor, Actor bigCardActor, EntityDef bigCardEntityDef = null)
	{
		if (cardActor == null)
		{
			return;
		}
		int techLevel = 0;
		if (bigCardEntityDef != null)
		{
			if (bigCardEntityDef.UseTechLevelManaGem())
			{
				techLevel = bigCardEntityDef.GetTechLevel();
			}
		}
		else if (cardActor.UseTechLevelManaGem())
		{
			techLevel = entity?.GetTechLevel() ?? 0;
		}
		if (techLevel != 0)
		{
			bigCardActor.m_manaObject.SetActive(value: false);
			Spell techLevelSpell = bigCardActor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
			if (techLevelSpell != null)
			{
				techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = techLevel;
				techLevelSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
		bool costHealthToBuy = false;
		if (cardActor.GetEntity() != null && cardActor.GetEntity().HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY) && !entity.IsControlledByFriendlySidePlayer() && ZoneMgr.Get().IsBattlegroundShoppingPhase())
		{
			costHealthToBuy = true;
			bigCardActor.ActivateSpellBirthState(SpellType.COST_HEALTH_TO_BUY);
		}
		if (!cardActor.UseCoinManaGem() || (entity != null && entity.IsAnomaly()) || (cardActor.GetEntity() != null && cardActor.GetEntity().IsBaconSpell() && costHealthToBuy))
		{
			return;
		}
		if (entity != null && entity.IsBaconSpell())
		{
			bigCardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM_BACON_SPELL);
		}
		else if (bigCardEntityDef != null && bigCardEntityDef.IsBaconSpell())
		{
			bigCardActor.SetShowCostOverride(-1);
			if (entity.IsBattlegroundTrinket() || entity.IsHeroPower())
			{
				bigCardActor.HideCoinManaGem();
			}
		}
		else if (techLevel == 0)
		{
			bigCardActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
	}

	private void LoadAndDisplayTooltipPhoneSigils()
	{
		if (m_phoneSigilActors == null)
		{
			m_phoneSigilActors = new List<Actor>();
		}
		else
		{
			foreach (Actor phoneSigilActor in m_phoneSigilActors)
			{
				phoneSigilActor.Destroy();
			}
			m_phoneSigilActors.Clear();
		}
		ZoneSecret zone = m_card.GetZone() as ZoneSecret;
		if (zone == null)
		{
			Log.Gameplay.PrintError("BigCard.LoadAndDisplayTooltipPhoneSigils() called for a card that is not in a Secret Zone.");
			return;
		}
		List<Card> cards = zone.GetSigilCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Actor actor = LoadPhoneSecret(cards[i]);
			m_phoneSigilActors.Add(actor);
		}
		DisplayPhoneSecrets(m_card, m_phoneSigilActors, showDeath: false);
	}

	private void HideTooltipPhoneSigils()
	{
		if (m_phoneSigilActors == null)
		{
			return;
		}
		foreach (Actor actor in m_phoneSigilActors)
		{
			HidePhoneSecret(actor);
		}
		m_phoneSigilActors.Clear();
	}

	private void LoadAndDisplayTooltipPhoneObjectives()
	{
		if (m_phoneObjectivesActors == null)
		{
			m_phoneObjectivesActors = new List<Actor>();
		}
		else
		{
			foreach (Actor phoneObjectivesActor in m_phoneObjectivesActors)
			{
				phoneObjectivesActor.Destroy();
			}
			m_phoneObjectivesActors.Clear();
		}
		ZoneSecret zone = m_card.GetZone() as ZoneSecret;
		if (zone == null)
		{
			Log.Gameplay.PrintError("BigCard.LoadAndDisplayTooltipPhoneObjectives() called for a card that is not in a Secret Zone.");
			return;
		}
		List<Card> cards = zone.GetObjectiveCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Actor actor = LoadPhoneSecret(cards[i]);
			m_phoneObjectivesActors.Add(actor);
		}
		DisplayPhoneSecrets(m_card, m_phoneObjectivesActors, showDeath: false);
	}

	private void HideTooltipPhoneObjectives()
	{
		if (m_phoneObjectivesActors == null)
		{
			return;
		}
		foreach (Actor actor in m_phoneObjectivesActors)
		{
			HidePhoneSecret(actor);
		}
		m_phoneObjectivesActors.Clear();
	}

	private void HidePhoneEnchantmentBanners()
	{
		if (m_phoneEnchantmentBanners == null)
		{
			return;
		}
		foreach (EnchantmentBanner eb in m_phoneEnchantmentBanners)
		{
			if (!(eb == null))
			{
				eb.ResetEnchantments();
				eb.gameObject.SetActive(value: false);
			}
		}
	}

	private void LoadAndDisplayTooltipPhoneSecrets()
	{
		ResetAllEnchantmentBanners();
		if (m_phoneSecretActors == null)
		{
			m_phoneSecretActors = new List<Actor>();
			m_phoneSecretActors.Capacity = 5;
		}
		else
		{
			foreach (Actor actor in m_phoneSecretActors)
			{
				if (actor != null)
				{
					actor.Destroy();
				}
			}
			m_phoneSecretActors.Clear();
		}
		ZoneSecret zone = m_card.GetZone() as ZoneSecret;
		if (zone == null)
		{
			Log.Gameplay.PrintError("BigCard.LoadAndDisplayTooltipPhoneSecrets() called for a card that is not in a Secret Zone.");
			return;
		}
		List<Card> cards = zone.GetSecretCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Actor actor2 = LoadPhoneSecret(cards[i]);
			m_phoneSecretActors.Add(actor2);
		}
		DisplayPhoneSecrets(m_card, m_phoneSecretActors, showDeath: false);
	}

	private void HideTooltipPhoneSecrets()
	{
		if (m_phoneSecretActors == null)
		{
			return;
		}
		foreach (Actor actor in m_phoneSecretActors)
		{
			HidePhoneSecret(actor);
		}
	}

	private void LoadAndDisplayTooltipPhoneSideQuests()
	{
		if (m_phoneSideQuestActors == null)
		{
			m_phoneSideQuestActors = new List<Actor>();
		}
		else
		{
			foreach (Actor phoneSideQuestActor in m_phoneSideQuestActors)
			{
				phoneSideQuestActor.Destroy();
			}
			m_phoneSideQuestActors.Clear();
		}
		ZoneSecret zone = m_card.GetZone() as ZoneSecret;
		if (zone == null)
		{
			Log.Gameplay.PrintError("BigCard.LoadAndDisplayTooltipPhoneSideQuests() called for a card that is not in a Secret Zone.");
			return;
		}
		List<Card> cards = zone.GetSideQuestCards();
		for (int i = 0; i < cards.Count; i++)
		{
			Actor actor = LoadPhoneSecret(cards[i]);
			m_phoneSideQuestActors.Add(actor);
		}
		DisplayPhoneSecrets(m_card, m_phoneSideQuestActors, showDeath: false);
	}

	private void HideTooltipPhoneSideQuests()
	{
		if (m_phoneSideQuestActors == null)
		{
			return;
		}
		foreach (Actor actor in m_phoneSideQuestActors)
		{
			HidePhoneSecret(actor);
		}
		m_phoneSideQuestActors.Clear();
	}

	private Actor LoadPhoneSecret(Card card)
	{
		string assetRef = ActorNames.GetBigCardActor(card.GetEntity());
		Actor actor = AssetLoader.Get().InstantiatePrefab(assetRef, AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
		SetupActor(card, actor);
		return actor;
	}

	private Vector3 PhoneMoveSideQuestBigCardToTopOfScreen(Actor actor, Vector3 initialPosition)
	{
		if (actor == null || !UniversalInputManager.UsePhoneUI)
		{
			return initialPosition;
		}
		Vector3 actorStartingPosition = actor.transform.position;
		try
		{
			actor.transform.position = initialPosition;
			Bounds actorBounds = CalculateMeshBoundsIncludingGem(actor);
			Vector3 actorCenter = actorBounds.center;
			Vector3 actorTop = new Vector3(actorCenter.x, actorCenter.y, actorCenter.z + actorBounds.extents.z);
			Ray actorUpwardRay = new Ray(actorTop, actorTop - actorCenter);
			Plane topPlane = CameraUtils.CreateTopPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip));
			float intersectDist = 0f;
			topPlane.Raycast(actorUpwardRay, out intersectDist);
			return initialPosition + new Vector3(0f, 0f, intersectDist);
		}
		finally
		{
			actor.transform.position = actorStartingPosition;
		}
	}

	private void InitializeSecretZoneEnchantmentBannerListIfNecessary(List<Actor> actors)
	{
		if (m_phoneEnchantmentBanners == null)
		{
			m_phoneEnchantmentBanners = new List<EnchantmentBanner>(5);
			for (int i = 0; i < 5; i++)
			{
				EnchantmentBanner banner = LoadEnchantmentBannerObject();
				banner.gameObject.SetActive(value: false);
				m_phoneEnchantmentBanners.Add(banner);
			}
		}
		if (m_phoneEnchantmentBanners.Count < actors.Count)
		{
			int numToAdd = actors.Count - m_phoneEnchantmentBanners.Count;
			for (int j = 0; j < numToAdd; j++)
			{
				EnchantmentBanner banner2 = LoadEnchantmentBannerObject();
				banner2.gameObject.SetActive(value: false);
				m_phoneEnchantmentBanners.Add(banner2);
			}
		}
		for (int k = 0; k < m_phoneEnchantmentBanners.Count; k++)
		{
			if (m_phoneEnchantmentBanners[k] == null)
			{
				m_phoneEnchantmentBanners[k] = LoadEnchantmentBannerObject();
			}
		}
	}

	private void DisplayPhoneSecrets(Card mainCard, List<Actor> actors, bool showDeath)
	{
		if (actors == null)
		{
			return;
		}
		InitializeSecretZoneEnchantmentBannerListIfNecessary(actors);
		DetermineSecretLayoutOffsets(mainCard, actors, out var initialOffset, out var spacing, out var drift);
		bool totalIsOdd = GeneralUtils.IsOdd(actors.Count);
		Player controller = mainCard.GetController();
		Zone secretZone = ((!(TeammateBoardViewer.Get() != null) || !TeammateBoardViewer.Get().IsViewingTeammate()) ? controller.GetSecretZone() : ZoneMgr.Get().FindZonesOfType<ZoneSecret>(Player.Side.TEAMMATE_FRIENDLY).FirstOrDefault());
		Actor mainActor = mainCard.GetActor();
		Vector3 initialPos = secretZone.transform.position + initialOffset;
		for (int i = 0; i < actors.Count; i++)
		{
			Actor actor = actors[i];
			Vector3 pos;
			if (i == 0 && totalIsOdd)
			{
				pos = ((actors.Count != 1 || !actor.GetCard().GetEntity().IsSideQuest() || !controller.IsFriendlySide()) ? initialPos : PhoneMoveSideQuestBigCardToTopOfScreen(actor, initialPos));
			}
			else
			{
				bool currentIsOdd = GeneralUtils.IsOdd(i);
				bool pyramidLeftSide = totalIsOdd == currentIsOdd;
				float num = (totalIsOdd ? Mathf.Ceil(0.5f * (float)i) : Mathf.Floor(0.5f * (float)i));
				float offsetX = num * spacing.x;
				if (!totalIsOdd)
				{
					offsetX += 0.5f * spacing.x;
				}
				if (pyramidLeftSide)
				{
					offsetX = 0f - offsetX;
				}
				float offsetZ = num * spacing.z;
				pos = new Vector3(initialPos.x + offsetX, initialPos.y, initialPos.z + offsetZ);
			}
			BigCardDisplayBones bigCardBones = actor.GetComponentInChildren<BigCardDisplayBones>();
			if (bigCardBones != null)
			{
				bigCardBones.GetRigForCurrentPlatform(out var _, out var platformScaleSettings);
				if (platformScaleSettings != null)
				{
					float cardScale = GetScaleForCard(platformScaleSettings, actor.GetCard(), queryForSelf: true);
					float enchantmentBannerScale = platformScaleSettings.m_EnchantmentBannerScale * cardScale;
					m_phoneEnchantmentBanners[i].gameObject.SetActive(value: true);
					DisplayPhoneEnchantmentBanner(actor.GetCard(), actor, m_phoneEnchantmentBanners[i], enchantmentBannerScale);
				}
			}
			actor.transform.position = mainActor.transform.position;
			actor.transform.rotation = mainActor.transform.rotation;
			actor.transform.localScale = INVISIBLE_SCALE;
			float showSec = (showDeath ? m_SecretLayoutData.m_DeathShowAnimTime : m_SecretLayoutData.m_ShowAnimTime);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("position", pos - drift);
			moveArgs.Add("time", showSec);
			moveArgs.Add("easetype", iTween.EaseType.easeOutExpo);
			iTween.MoveTo(actor.gameObject, moveArgs);
			Hashtable driftArgs = iTweenManager.Get().GetTweenHashTable();
			driftArgs.Add("position", pos);
			driftArgs.Add("delay", showSec);
			driftArgs.Add("time", m_SecretLayoutData.m_DriftSec);
			driftArgs.Add("easetype", iTween.EaseType.easeOutExpo);
			iTween.MoveTo(actor.gameObject, driftArgs);
			iTween.ScaleTo(actor.gameObject, base.transform.localScale, showSec);
			if (showDeath)
			{
				ShowPhoneSecretDeath(actor);
			}
		}
	}

	private void DisplayPhoneEnchantmentBanner(Card card, Actor actor, EnchantmentBanner phoneEnchantmentBanner, float enchantmentBannerScale)
	{
		if (!(actor == null) && !(phoneEnchantmentBanner == null))
		{
			if (enchantmentBannerScale <= 0f)
			{
				Log.Gameplay.PrintWarning("Scale for enchantment banner under a secret-zone big card is invalid. Scale changed to 1.0 to prevent display issues.");
				enchantmentBannerScale = 1f;
			}
			phoneEnchantmentBanner.UpdateEnchantments(card, actor, enchantmentBannerScale);
		}
	}

	private void DetermineSecretLayoutOffsets(Card mainCard, List<Actor> actors, out Vector3 initialOffset, out Vector3 spacing, out Vector3 drift)
	{
		Player controller = mainCard.GetController();
		bool friendly = controller.IsFriendlySide();
		bool num = controller.IsRevealed();
		int minCardThreshold = m_SecretLayoutData.m_MinCardThreshold;
		int maxThreshold = m_SecretLayoutData.m_MaxCardThreshold;
		SecretLayoutOffsets minOffsets = m_SecretLayoutData.m_MinCardOffsets;
		SecretLayoutOffsets maxOffsets = m_SecretLayoutData.m_MaxCardOffsets;
		float t = Mathf.InverseLerp(minCardThreshold, maxThreshold, actors.Count);
		if (num)
		{
			if (friendly)
			{
				initialOffset = Vector3.Lerp(minOffsets.m_InitialOffset, maxOffsets.m_InitialOffset, t);
			}
			else
			{
				initialOffset = Vector3.Lerp(minOffsets.m_OpponentInitialOffset, maxOffsets.m_OpponentInitialOffset, t);
			}
			spacing = m_SecretLayoutData.m_Spacing;
		}
		else
		{
			if (friendly)
			{
				initialOffset = Vector3.Lerp(minOffsets.m_HiddenInitialOffset, maxOffsets.m_HiddenInitialOffset, t);
			}
			else
			{
				initialOffset = Vector3.Lerp(minOffsets.m_HiddenOpponentInitialOffset, maxOffsets.m_HiddenOpponentInitialOffset, t);
			}
			spacing = m_SecretLayoutData.m_HiddenSpacing;
		}
		if (friendly)
		{
			spacing.z = 0f - spacing.z;
			drift = m_SecretLayoutData.m_DriftOffset;
		}
		else
		{
			drift = -m_SecretLayoutData.m_DriftOffset;
		}
	}

	private void ShowPhoneSecretDeath(Actor actor)
	{
		ISpellCallbackHandler<Spell>.StateFinishedCallback deathSpellStateFinished = delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (spell.GetActiveState() != 0)
			{
				actor.Destroy();
			}
		};
		Action<object> onTimerComplete = delegate
		{
			Spell spell2 = actor.GetSpell(SpellType.DEATH);
			spell2.AddStateFinishedCallback(deathSpellStateFinished);
			spell2.Activate();
		};
		Hashtable timerArgs = iTweenManager.Get().GetTweenHashTable();
		timerArgs.Add("time", m_SecretLayoutData.m_TimeUntilDeathSpell);
		timerArgs.Add("oncomplete", onTimerComplete);
		iTween.Timer(actor.gameObject, timerArgs);
	}

	private void HidePhoneSecret(Actor actor)
	{
		if (!(actor == null) && !(m_card == null))
		{
			Actor mainActor = m_card.GetActor();
			if (mainActor != null)
			{
				iTween.MoveTo(actor.gameObject, mainActor.transform.position, m_SecretLayoutData.m_HideAnimTime);
			}
			iTween.ScaleTo(actor.gameObject, INVISIBLE_SCALE, m_SecretLayoutData.m_HideAnimTime);
			Action<object> onHideComplete = delegate
			{
				actor.Destroy();
			};
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("time", m_SecretLayoutData.m_HideAnimTime);
			args.Add("oncomplete", onHideComplete);
			iTween.Timer(actor.gameObject, args);
		}
	}

	private void SetupActor(Card card, Actor actor)
	{
		bool ignoreHideStats = false;
		Entity entity = card.GetEntity();
		if (ShouldActorUseEntity(entity))
		{
			actor.SetEntity(entity);
			ignoreHideStats = entity.HasTag(GAME_TAG.IGNORE_HIDE_STATS_FOR_BIG_CARD) || (entity.IsDormant() && !entity.HasTag(GAME_TAG.HIDE_STATS));
		}
		GhostCard.Type desiredGhostCardAppearance = (GhostCard.Type)entity.GetTag(GAME_TAG.MOUSE_OVER_CARD_APPEARANCE);
		if (card.GetEntity().IsDormant())
		{
			desiredGhostCardAppearance = GhostCard.Type.DORMANT;
		}
		actor.GhostCardEffect(desiredGhostCardAppearance, entity.GetPremiumType(), update: false);
		EntityDef desiredEntityDef = entity.GetEntityDef();
		DefLoader.DisposableCardDef desiredCardDef = card.ShareDisposableCardDef();
		int alternateMouseOverCardID = entity.GetTag(GAME_TAG.ALTERNATE_MOUSE_OVER_CARD);
		bool isBattlegroundHeroBuddyMeter = entity.GetCardType() == TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY;
		bool isCoinBasedBattlegroundHeroBuddyMeter = entity.IsCoinBasedHeroBuddy();
		if (isBattlegroundHeroBuddyMeter)
		{
			alternateMouseOverCardID = entity.GetTag(GAME_TAG.BACON_COMPANION_ID);
			actor.SetEntity(null);
		}
		EntityDef alternateCardEntityDef = null;
		if (alternateMouseOverCardID != 0)
		{
			alternateCardEntityDef = DefLoader.Get().GetEntityDef(alternateMouseOverCardID);
			if (alternateCardEntityDef == null)
			{
				Log.Gameplay.PrintError("BigCard.SetupActor(): Unable to load EntityDef for card ID {0}.", alternateMouseOverCardID);
			}
			else
			{
				desiredEntityDef = alternateCardEntityDef;
			}
			DefLoader.DisposableCardDef alternateCardDef = DefLoader.Get().GetCardDef(alternateMouseOverCardID);
			if (alternateCardDef == null)
			{
				Log.Spells.PrintError("BigCard.SetupActor(): Unable to load CardDef for card ID {0}.", alternateMouseOverCardID);
			}
			else
			{
				desiredCardDef?.Dispose();
				desiredCardDef = alternateCardDef;
			}
		}
		using (desiredCardDef)
		{
			if (ShouldActorUseEntityDef(entity))
			{
				actor.SetEntityDef(desiredEntityDef);
				ignoreHideStats = desiredEntityDef.HasTag(GAME_TAG.IGNORE_HIDE_STATS_FOR_BIG_CARD) || desiredEntityDef.IsDormant();
			}
			actor.SetPremium(entity.GetPremiumType());
			actor.SetCard(card);
			actor.SetCardDef(desiredCardDef);
			actor.SetIgnoreHideStats(ignoreHideStats);
			actor.SetWatermarkCardSetOverride(entity.GetWatermarkCardSetOverride());
			actor.EnableAlternateCostTextPosition(entity.IsBaconSpell());
			actor.UpdateAllComponents();
			ActivateBigCardStateSpells(entity, card.GetActor(), actor, isBattlegroundHeroBuddyMeter ? alternateCardEntityDef : null);
			if (isBattlegroundHeroBuddyMeter)
			{
				if (!isCoinBasedBattlegroundHeroBuddyMeter)
				{
					GameObject ghostCard = GameObjectUtils.FindChildBySubstring(m_bigCardActor.gameObject, "GhostCard");
					GameObject objectToRender = GameObjectUtils.FindChildBySubstring(m_bigCardActor.gameObject, "RootObject");
					if (ghostCard != null && objectToRender != null)
					{
						GhostCard component = ghostCard.GetComponent<GhostCard>();
						component.enabled = true;
						component.SetGhostType(GhostCard.Type.DORMANT);
						component.RenderGhostCard(forceRender: true);
						actor.GhostCardEffect(GhostCard.Type.DORMANT);
					}
					GameObject bottomGhostCard = GameObjectUtils.FindChildBySubstring(actor.gameObject, "GhostedCard_Bottom");
					if (bottomGhostCard != null)
					{
						bool active = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_2) != 0;
						bottomGhostCard.SetActive(active);
					}
					else
					{
						Debug.LogWarning("BigCard.SetupActor - Bottom ghost card is missing");
					}
				}
				else
				{
					GameObject bottomGhostCard2 = GameObjectUtils.FindChildBySubstring(actor.gameObject, "GhostedCard_Bottom");
					if (bottomGhostCard2 != null)
					{
						entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_2);
						bottomGhostCard2.SetActive(value: false);
					}
					Entity hero = (entity.IsControlledByOpposingSidePlayer() ? GameState.Get().GetOpposingSidePlayer() : GameState.Get().GetFriendlySidePlayer())?.GetHero();
					int numBuddiesGained = hero?.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED) ?? 0;
					if (actor.GetCard().GetZone() is ZoneTeammatePlay)
					{
						numBuddiesGained = entity.GetTag(GAME_TAG.BACON_PLAYER_NUM_HERO_BUDDIES_GAINED);
					}
					Actor buddyEvolvingCardActor = null;
					if (alternateCardEntityDef.HasTag(GAME_TAG.BACON_SHOW_HEROPOWER_BUDDY_AS_EVOLVING_BIG_CARD) && hero != null && !(actor.GetCard().GetZone() is ZoneTeammatePlay))
					{
						Entity heroPower = hero.GetHeroPower();
						if (heroPower != null)
						{
							int baseHeroID = heroPower.GetTag(GAME_TAG.BACON_HEROPOWER_BASE_HERO_ID);
							EntityDef baseHeroEntityDef = DefLoader.Get().GetEntityDef(baseHeroID);
							if (baseHeroEntityDef == null)
							{
								Log.Gameplay.PrintError("BigCard.SetupActor(): Unable to load EntityDef for card ID {0}.", baseHeroID);
							}
							else
							{
								int desiredHeroBuddyID = baseHeroEntityDef.GetTag(GAME_TAG.BACON_COMPANION_ID);
								EntityDef desiredHeroBuddyEntityDef = DefLoader.Get().GetEntityDef(desiredHeroBuddyID);
								if (desiredHeroBuddyEntityDef == null)
								{
									Log.Gameplay.PrintError("BigCard.SetupActor(): Unable to load EntityDef for card ID {0}.", desiredHeroBuddyID);
								}
								else
								{
									DefLoader.DisposableCardDef desiredHeroBuddyCardDef = DefLoader.Get().GetCardDef(desiredHeroBuddyID);
									if (desiredHeroBuddyCardDef == null)
									{
										Log.Spells.PrintError("BigCard.SetupActor(): Unable to load CardDef for card ID {0}.", desiredHeroBuddyID);
									}
									else
									{
										using (desiredHeroBuddyCardDef)
										{
											buddyEvolvingCardActor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetBigCardActor(m_card.GetEntity()), AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
											buddyEvolvingCardActor.DeactivateAllSpells();
											buddyEvolvingCardActor.SetEntityDef(desiredHeroBuddyEntityDef);
											buddyEvolvingCardActor.SetPremium(TAG_PREMIUM.NORMAL);
											buddyEvolvingCardActor.SetCard(card);
											buddyEvolvingCardActor.SetCardDef(desiredHeroBuddyCardDef);
											buddyEvolvingCardActor.SetIgnoreHideStats(ignore: false);
											buddyEvolvingCardActor.SetWatermarkCardSetOverride(entity.GetWatermarkCardSetOverride());
											buddyEvolvingCardActor.UpdateAllComponents();
											GameObjectUtils.FindChildBySubstring(buddyEvolvingCardActor.gameObject, "BGRewardVFX")?.SetActive(value: true);
											GameObject evolvingBuddyActorBone = ((numBuddiesGained > 0) ? actor.FindBone("SecondEvolvingBuddyBone") : actor.FindBone("EvolvingBuddyBone"));
											buddyEvolvingCardActor.transform.parent = actor.gameObject.transform;
											buddyEvolvingCardActor.transform.localPosition = evolvingBuddyActorBone.transform.localPosition;
											buddyEvolvingCardActor.transform.localRotation = evolvingBuddyActorBone.transform.localRotation;
											buddyEvolvingCardActor.transform.localScale = evolvingBuddyActorBone.transform.localScale;
											ActivateBigCardStateSpells(entity, card.GetActor(), buddyEvolvingCardActor, desiredHeroBuddyEntityDef);
										}
									}
								}
							}
						}
					}
					if (numBuddiesGained == 1)
					{
						if (buddyEvolvingCardActor != null)
						{
							buddyEvolvingCardActor.gameObject.SetActive(value: false);
						}
						Actor secondBuddyActor = UnityEngine.Object.Instantiate(actor);
						GameObject secondActorBone = actor.FindBone("SecondBuddyBone");
						secondBuddyActor.transform.parent = actor.gameObject.transform;
						secondBuddyActor.transform.localPosition = secondActorBone.transform.localPosition;
						secondBuddyActor.transform.localRotation = secondActorBone.transform.localRotation;
						secondBuddyActor.transform.localScale = secondActorBone.transform.localScale;
						ActivateBigCardStateSpells(entity, card.GetActor(), secondBuddyActor, alternateCardEntityDef);
						if (buddyEvolvingCardActor != null)
						{
							buddyEvolvingCardActor.gameObject.SetActive(value: true);
						}
					}
				}
			}
			BoxCollider collider = actor.GetComponentInChildren<BoxCollider>();
			if (collider != null)
			{
				collider.enabled = false;
			}
			actor.name = "BigCard_" + actor.name;
			LayerUtils.SetLayer(actor, GameLayer.Tooltip);
		}
	}

	private bool ShouldActorUseEntity(Entity entity)
	{
		if (entity.IsHidden())
		{
			return true;
		}
		if (entity.GetZone() == TAG_ZONE.PLAY || entity.GetZone() == TAG_ZONE.SECRET)
		{
			return true;
		}
		if (entity.GetZone() == TAG_ZONE.HAND && entity.GetCardTextBuilder().ShouldUseEntityForTextInHand())
		{
			return true;
		}
		if (entity.IsDormant())
		{
			return true;
		}
		if (entity.IsSideQuest() || entity.IsSigil() || entity.IsSecret() || entity.IsObjective())
		{
			return true;
		}
		if (entity.IsCardButton() && !entity.IsBattlegroundHeroBuddy())
		{
			return true;
		}
		if (GameMgr.Get().IsSpectator() && entity.GetZone() == TAG_ZONE.HAND && entity.GetController().IsOpposingSide())
		{
			return true;
		}
		if (entity.GetZone() == TAG_ZONE.HAND && TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsActorTeammates(entity.GetCard().GetActor()))
		{
			return true;
		}
		if (entity.HasTag(GAME_TAG.CARD_NAME_DATA_1))
		{
			return true;
		}
		return false;
	}

	private bool ShouldActorUseEntityDef(Entity entity)
	{
		if (entity.IsHidden())
		{
			return false;
		}
		if (entity.IsCardButton() && !entity.IsBattlegroundHeroBuddy())
		{
			return false;
		}
		if (entity.IsDormant())
		{
			return false;
		}
		if (entity.GetZone() == TAG_ZONE.SECRET)
		{
			return false;
		}
		if (GameMgr.Get().IsSpectator() && entity.GetZone() == TAG_ZONE.HAND && entity.GetController().IsOpposingSide())
		{
			return false;
		}
		if (entity.GetZone() == TAG_ZONE.HAND && TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsActorTeammates(entity.GetCard().GetActor()))
		{
			return false;
		}
		if (entity.HasTag(GAME_TAG.CARD_NAME_DATA_1))
		{
			return false;
		}
		return true;
	}
}
