using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.UI;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
	public enum CanCraftCardResult
	{
		CanCraft,
		CanUpgrade,
		TooManyCopies,
		NotEnoughCopies,
		NoCardValue,
		NotEnoughDust,
		CardLockedInCurrentLeague,
		FeatureFlagOff,
		NotUpgradable,
		NotCraftable
	}

	[Header("Bones")]
	public Transform m_floatingCardBone;

	public Transform m_faceDownCardBone;

	public Transform m_cardInfoPaneBone;

	public Transform m_cardCounterBone;

	public Transform m_signature25CardCounterBone;

	public Transform m_diamondCardCounterBone;

	public Transform m_showCraftingUIBone;

	public Transform m_hideCraftingUIBone;

	public Transform m_showUpgradeToGoldenPopupBone;

	public Transform m_signatureTooltipBone;

	[Header("UI")]
	public BoxCollider m_offClickCatcher;

	public CraftCardCountTab m_cardCountTab;

	public Vector3 m_cardCountTabShowScale = Vector3.one;

	public Vector3 m_cardCountTabHideScale = new Vector3(1f, 1f, 0f);

	public PegUIElement m_switchPremiumButton;

	public QuestCardRewardOverlay m_questCardRewardOverlay;

	public RelatedCardsTray m_relatedCardsTray;

	[Header("Animation")]
	public float m_timeForCardToFlipUp;

	public float m_timeForBackCardToMoveUp;

	public float m_delayBeforeBackCardMovesUp;

	public iTween.EaseType m_easeTypeForCardFlip;

	public iTween.EaseType m_easeTypeForCardMoveUp;

	public Vector3 m_utgAlertPopupOffset = new Vector3(-5f, 0f, 0f);

	private static CraftingManager s_instance;

	public CraftingUI m_craftingUI;

	private Widget m_upgradeToGoldenWidget;

	private bool m_upgradeToGoldenWidgetShown;

	private Actor m_currentBigActor;

	private bool m_isCurrentActorAGhost;

	private Actor m_upsideDownActor;

	private Actor m_currentRelatedCardActor;

	private Actor m_ghostWeaponActor;

	private Actor m_ghostMinionActor;

	private Actor m_ghostSpellActor;

	private Actor m_ghostHeroActor;

	private Actor m_ghostHeroPowerActor;

	private Actor m_ghostLocationActor;

	private Actor m_templateWeaponActor;

	private Actor m_templateSpellActor;

	private Actor m_templateMinionActor;

	private Actor m_templateHeroActor;

	private Actor m_templateHeroPowerActor;

	private Actor m_templateLocationActor;

	private Actor m_hiddenActor;

	private CardInfoPane m_cardInfoPane;

	private Actor m_templateGoldenWeaponActor;

	private Actor m_templateGoldenSpellActor;

	private Actor m_templateGoldenMinionActor;

	private Actor m_templateGoldenHeroActor;

	private Actor m_templateGoldenHeroPowerActor;

	private Actor m_templateDiamondMinionActor;

	private Dictionary<int, Actor> m_templateSignatureHandActors;

	private Actor m_templateGoldenLocationActor;

	private Actor m_ghostGoldenWeaponActor;

	private Actor m_ghostGoldenSpellActor;

	private Actor m_ghostGoldenMinionActor;

	private Actor m_ghostGoldenHeroActor;

	private Actor m_ghostGoldenHeroPowerActor;

	private Actor m_ghostDiamondMinionActor;

	private Dictionary<int, Actor> m_ghostSignatureHandActors;

	private Actor m_ghostGoldenLocationActor;

	private bool m_cancellingCraftMode;

	private long m_unCommitedArcaneDustAdjustments;

	private CraftingPendingTransaction m_pendingClientTransaction;

	private CraftingPendingTransaction m_pendingServerTransaction;

	private Vector3 m_craftSourcePosition;

	private Vector3 m_craftSourceScale;

	private CollectionCardActors m_cardActors;

	private Actor m_collectionCardActor;

	private bool m_elementsLoaded;

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_START_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, -0.5f, 0f),
		Phone = new Vector3(0f, -0.5f, 0f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_START_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.1f, 0.1f, 0.1f),
		Phone = new Vector3(0.1f, 0.1f, 0.1f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_POSITION = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-2.11f, -0.010312f, -0.06f),
		Phone = new Vector3(-1.97f, -0.0006f, -0.033f)
	};

	private static readonly PlatformDependentValue<Vector3> HERO_POWER_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.85f, 0.85f, 0.85f),
		Phone = new Vector3(0.76637f, 0.76637f, 0.76637f)
	};

	private static readonly float HERO_POWER_TWEEN_TIME = 0.5f;

	private static readonly AssetReference UPGRADE_TO_GOLDEN_WIDGET_PREFAB = new AssetReference("UpgradeToGoldenPopup.prefab:15b90c0a0040d1445a44a463626214bc");

	private ScreenEffectsHandle m_screenEffectsHandle;

	public static bool IsInitialized => s_instance != null;

	private bool IsInCraftingMode { get; set; }

	private void Awake()
	{
		CollectionManager.Get()?.RegisterMassDisenchantListener(OnMassDisenchant);
		if (m_upgradeToGoldenWidget != null)
		{
			m_upgradeToGoldenWidget.Hide();
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		if (CollectionManager.Get() != null)
		{
			CollectionManager.Get().RemoveMassDisenchantListener(OnMassDisenchant);
		}
		s_instance = null;
	}

	private void Start()
	{
		LoadElements();
	}

	private void LoadElements()
	{
		if (!m_elementsLoaded)
		{
			LoadActor("Card_Hand_Weapon.prefab:30888a1fdca5c6c43abcc5d9dca55783", ref m_ghostWeaponActor, ref m_templateWeaponActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.WEAPON, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenWeaponActor, ref m_templateGoldenWeaponActor);
			LoadActor("Card_Hand_Ally.prefab:d00eb0f79080e0749993fe4619e9143d", ref m_ghostMinionActor, ref m_templateMinionActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.MINION, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenMinionActor, ref m_templateGoldenMinionActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.MINION, TAG_PREMIUM.DIAMOND), ref m_ghostDiamondMinionActor, ref m_templateDiamondMinionActor);
			LoadActor("Card_Hand_Ability.prefab:3c3f5189f0d0b3745a1c1ca21d41efe0", ref m_ghostSpellActor, ref m_templateSpellActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.SPELL, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenSpellActor, ref m_templateGoldenSpellActor);
			LoadActor("Card_Hand_Hero.prefab:a977c49edb5fb5d4c8dee4d2344d1395", ref m_ghostHeroActor, ref m_templateHeroActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.HERO, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenHeroActor, ref m_templateGoldenHeroActor);
			LoadActor("History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53", ref m_ghostHeroPowerActor, ref m_templateHeroPowerActor);
			LoadActor(ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenHeroPowerActor, ref m_templateGoldenHeroPowerActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.LOCATION, TAG_PREMIUM.NORMAL), ref m_ghostLocationActor, ref m_templateLocationActor);
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.LOCATION, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenLocationActor, ref m_templateGoldenLocationActor);
			LoadActor("Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", ref m_hiddenActor);
			LoadSignatureActors();
			m_hiddenActor.GetMeshRenderer().transform.localEulerAngles = new Vector3(0f, 180f, 180f);
			LayerUtils.SetLayer(m_hiddenActor.gameObject, GameLayer.IgnoreFullScreenEffects);
			SoundManager.Get().Load("Card_Transition_Out.prefab:aecf5b5837772844b9d2db995744df82");
			SoundManager.Get().Load("Card_Transition_In.prefab:3f3fbe896b8b260448e8c7e5d028d971");
			LoadRandomCardBack();
			m_elementsLoaded = true;
		}
	}

	private void LoadSignatureActors()
	{
		Dictionary<int, string> signatureHand = ActorNames.SignatureHand;
		m_templateSignatureHandActors = new Dictionary<int, Actor>();
		m_ghostSignatureHandActors = new Dictionary<int, Actor>();
		foreach (KeyValuePair<int, string> signatureFrameActorPair in signatureHand)
		{
			Actor templateSignatureActor = null;
			Actor ghostSignatureActor = null;
			LoadActor(signatureFrameActorPair.Value, ref ghostSignatureActor, ref templateSignatureActor);
			m_templateSignatureHandActors.Add(signatureFrameActorPair.Key, templateSignatureActor);
			m_ghostSignatureHandActors.Add(signatureFrameActorPair.Key, ghostSignatureActor);
		}
	}

	public void SwitchPremiumView(TAG_PREMIUM premium)
	{
		if (premium != GetShownActor().GetPremium())
		{
			if (m_upsideDownActor != null)
			{
				UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
				m_upsideDownActor = null;
			}
			if (m_currentBigActor != null)
			{
				m_currentBigActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
				UnityEngine.Object.Destroy(m_currentBigActor.gameObject);
				m_currentBigActor = null;
			}
			string cardId = m_collectionCardActor.GetEntityDef().GetCardId();
			if (GameUtils.IsClassicCard(cardId))
			{
				m_pendingClientTransaction.CardID = GameUtils.GetLegacyCounterpartCardId(m_pendingClientTransaction.CardID);
			}
			else
			{
				m_pendingClientTransaction.CardID = cardId;
			}
			m_pendingClientTransaction.Premium = premium;
			NetCache.CardValue cardValue = GetCardValue(m_pendingClientTransaction.CardID, premium);
			if (cardValue != null)
			{
				m_pendingClientTransaction.CardValueOverridden = cardValue.IsOverrideActive();
			}
			MoveCardToBigSpot(m_collectionCardActor, premium);
			if (m_craftingUI != null)
			{
				m_craftingUI.Enable(m_showCraftingUIBone.position, m_hideCraftingUIBone.position);
			}
		}
	}

	public static CraftingManager Get()
	{
		if (s_instance == null)
		{
			string craftingManagerPrefab = (UniversalInputManager.UsePhoneUI ? "CraftingManager_phone.prefab:d28ac29ae64f14e649186d0d1fe5f7e8" : "CraftingManager.prefab:9dc2dd187dd914959b311d326c3fd5b2");
			s_instance = AssetLoader.Get().InstantiatePrefab(craftingManagerPrefab).GetComponent<CraftingManager>();
			s_instance.LoadElements();
		}
		return s_instance;
	}

	public static NetCache.CardValue GetCardValue(string cardID, TAG_PREMIUM premium)
	{
		CardValueDbfRecord cardValueDbfRecord;
		return GetCardValue(cardID, premium, out cardValueDbfRecord);
	}

	public static NetCache.CardValue GetCardValue(string cardID, TAG_PREMIUM premium, out CardValueDbfRecord cardValueDbfRecord)
	{
		cardValueDbfRecord = null;
		NetCache.CardValue cardValue = new NetCache.CardValue();
		string ID = cardID;
		if (GameUtils.IsClassicCard(cardID))
		{
			ID = GameUtils.GetLegacyCounterpartCardId(cardID);
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(ID);
		int assetCardId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars == null)
		{
			return null;
		}
		cardValueDbfRecord = GameDbf.CardValue.GetRecord((CardValueDbfRecord x) => assetCardId == x.AssetCardId && x.Premium == (int)premium && EventTimingManager.Get().IsEventActive(x.OverrideEvent) && x.OverrideRegion == (CardValue.OverrideRegion)guardianVars.CardValueOverrideServerRegion);
		if (cardValueDbfRecord == null)
		{
			cardValueDbfRecord = GameDbf.CardValue.GetRecord((CardValueDbfRecord x) => assetCardId == x.AssetCardId && x.Premium == (int)premium && EventTimingManager.Get().IsEventActive(x.OverrideEvent) && x.OverrideRegion == CardValue.OverrideRegion.NONE);
		}
		if (IsLegacyCardValueCacheEnabled())
		{
			NetCache.CardDefinition cardDef = new NetCache.CardDefinition
			{
				Name = ID,
				Premium = premium
			};
			NetCache.NetCacheCardValues cardValues = NetCache.Get().GetNetObject<NetCache.NetCacheCardValues>();
			if (cardValues == null || !cardValues.Values.TryGetValue(cardDef, out cardValue))
			{
				return null;
			}
			return cardValue;
		}
		InitCardValueDbfRecord initCardValueRecord = GameDbf.InitCardValue.GetRecord((InitCardValueDbfRecord x) => entityDef.GetRarity() == (TAG_RARITY)x.Rarity && x.Premium == (int)premium);
		if (initCardValueRecord == null)
		{
			return null;
		}
		cardValue.BaseBuyValue = initCardValueRecord.Buy;
		cardValue.BaseSellValue = initCardValueRecord.Sell;
		cardValue.BaseUpgradeValue = initCardValueRecord.Upgrade;
		if (cardValueDbfRecord != null)
		{
			cardValue.BuyValueOverride = cardValueDbfRecord.Buy;
			cardValue.OverrideEvent = cardValueDbfRecord.OverrideEvent;
			if (cardValueDbfRecord.SellState == CardValue.SellState.RECENTLY_NERFED_USE_BUY_VALUE)
			{
				cardValue.SellValueOverride = cardValue.BaseBuyValue;
			}
			else if (cardValueDbfRecord.SellState == CardValue.SellState.RECENTLY_NERFED_USE_CUSTOM_VALUE || cardValueDbfRecord.SellState == CardValue.SellState.PERMANENT_OVERRIDE_USE_CUSTOM_VALUE)
			{
				cardValue.SellValueOverride = cardValueDbfRecord.Sell;
			}
			else
			{
				cardValue.SellValueOverride = cardValue.BaseSellValue;
			}
		}
		return cardValue;
	}

	public CanCraftCardResult CanUpgradeCardToGolden(string cardID, TAG_PREMIUM premium, EntityDef entityDef = null)
	{
		if (!HasUpgradeToGoldenEnabled())
		{
			return CanCraftCardResult.FeatureFlagOff;
		}
		if (entityDef != null && GameUtils.IsSavedZilliaxVersion(entityDef))
		{
			return CanCraftCardResult.NotUpgradable;
		}
		if (premium != 0 && premium != TAG_PREMIUM.GOLDEN)
		{
			return CanCraftCardResult.NotUpgradable;
		}
		CollectibleCard normalCard = CollectionManager.Get().GetCard(cardID, TAG_PREMIUM.NORMAL);
		CollectibleCard goldenCard = CollectionManager.Get().GetCard(cardID, TAG_PREMIUM.GOLDEN);
		if (normalCard == null || goldenCard == null)
		{
			return CanCraftCardResult.NotUpgradable;
		}
		if (!normalCard.IsCraftable || !goldenCard.IsCraftable)
		{
			return CanCraftCardResult.NotCraftable;
		}
		if (!TryGetCardUpgradeValue(cardID, out var upgradeValue))
		{
			return CanCraftCardResult.NoCardValue;
		}
		CollectibleCard card = ((premium == TAG_PREMIUM.NORMAL) ? normalCard : goldenCard);
		bool isLegendary = card.Rarity == TAG_RARITY.LEGENDARY;
		int normalCount = GetNumOwnedIncludePending(cardID, TAG_PREMIUM.NORMAL);
		int goldenCount = GetNumOwnedIncludePending(cardID, TAG_PREMIUM.GOLDEN);
		if (NetCache.Get().GetArcaneDustBalance() < upgradeValue)
		{
			if (goldenCount >= card.DefaultMaxCopiesPerDeck)
			{
				return CanCraftCardResult.TooManyCopies;
			}
			return CanCraftCardResult.NotEnoughDust;
		}
		if (goldenCount >= card.DefaultMaxCopiesPerDeck)
		{
			return CanCraftCardResult.TooManyCopies;
		}
		if (!HasEnoughCopiesToUpgrade(card, isLegendary, normalCount, goldenCount, premium))
		{
			return CanCraftCardResult.NotEnoughCopies;
		}
		return CanCraftCardResult.CanUpgrade;
	}

	public bool HasEnoughCopiesToUpgrade(string cardID, TAG_PREMIUM premium)
	{
		if (!HasUpgradeToGoldenEnabled())
		{
			return false;
		}
		CollectibleCard normalCard = CollectionManager.Get().GetCard(cardID, TAG_PREMIUM.NORMAL);
		CollectibleCard goldenCard = CollectionManager.Get().GetCard(cardID, TAG_PREMIUM.GOLDEN);
		if (normalCard == null || goldenCard == null)
		{
			return false;
		}
		CollectibleCard card = ((premium == TAG_PREMIUM.NORMAL) ? normalCard : goldenCard);
		bool isLegendary = card.Rarity == TAG_RARITY.LEGENDARY;
		int normalCount = GetNumOwnedIncludePending(cardID, TAG_PREMIUM.NORMAL);
		int goldenCount = GetNumOwnedIncludePending(cardID, TAG_PREMIUM.GOLDEN);
		return HasEnoughCopiesToUpgrade(card, isLegendary, normalCount, goldenCount, premium);
	}

	public bool HasEnoughCopiesToUpgrade(CollectibleCard card, bool isLegendary, int normalCount, int goldenCount, TAG_PREMIUM premium)
	{
		if (!HasUpgradeToGoldenEnabled())
		{
			return false;
		}
		if (normalCount < card.DefaultMaxCopiesPerDeck)
		{
			if (!isLegendary && normalCount == 1 && goldenCount == 1)
			{
				return true;
			}
			if (!isLegendary && normalCount == 1 && premium == TAG_PREMIUM.GOLDEN)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool HasUpgradeToGoldenEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnableUpgradeToGolden;
	}

	public static bool IsLegacyCardValueCacheEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().LegacyCardValueCacheEnabled;
	}

	public CanCraftCardResult CanCraftCardRightNow(EntityDef entityDef, TAG_PREMIUM premium)
	{
		if (premium == TAG_PREMIUM.SIGNATURE || premium == TAG_PREMIUM.DIAMOND)
		{
			return CanCraftCardResult.NotCraftable;
		}
		if (GameUtils.IsSavedZilliaxVersion(entityDef))
		{
			return CanCraftCardResult.NotCraftable;
		}
		NetCache.CardDefinition cardDef = new NetCache.CardDefinition
		{
			Name = entityDef.GetCardId(),
			Premium = premium
		};
		int numOwnedIncludePending = GetNumOwnedIncludePending(cardDef.Name, cardDef.Premium);
		int maxCopies = (entityDef.IsElite() ? 1 : 2);
		if (numOwnedIncludePending >= maxCopies)
		{
			return CanCraftCardResult.TooManyCopies;
		}
		if (!TryGetCardBuyValue(cardDef.Name, cardDef.Premium, out var buyValue))
		{
			return CanCraftCardResult.NoCardValue;
		}
		if (NetCache.Get().GetArcaneDustBalance() < buyValue)
		{
			return CanCraftCardResult.NotEnoughDust;
		}
		if (RankMgr.Get().IsCardLockedInCurrentLeague(entityDef))
		{
			return CanCraftCardResult.CardLockedInCurrentLeague;
		}
		return CanCraftCardResult.CanCraft;
	}

	public bool TryGetCardBuyValue(string cardID, TAG_PREMIUM premium, out int buyValue)
	{
		NetCache.CardValue cardValue = GetCardValue(cardID, premium);
		if (cardValue == null)
		{
			buyValue = 0;
			return false;
		}
		if (GetNumClientTransactions() >= 0)
		{
			buyValue = cardValue.GetBuyValue();
			return true;
		}
		buyValue = cardValue.GetSellValue();
		return true;
	}

	public bool TryGetCardSellValue(string cardID, TAG_PREMIUM premium, out int sellValue)
	{
		NetCache.CardValue cardValue = GetCardValue(cardID, premium);
		if (cardValue == null)
		{
			sellValue = 0;
			return false;
		}
		if (GetNumClientTransactions() <= 0)
		{
			sellValue = cardValue.GetSellValue();
			return true;
		}
		sellValue = cardValue.GetBuyValue();
		return true;
	}

	public bool TryGetCardUpgradeValue(string cardID, out int upgradeValue)
	{
		NetCache.CardValue cardValue = GetCardValue(cardID, TAG_PREMIUM.NORMAL);
		if (cardValue == null)
		{
			upgradeValue = 0;
			return false;
		}
		upgradeValue = cardValue.GetUpgradeValue();
		return true;
	}

	public bool IsCardShowing()
	{
		return m_currentBigActor != null;
	}

	public static bool GetIsInCraftingMode()
	{
		if (s_instance != null)
		{
			return s_instance.IsInCraftingMode;
		}
		return false;
	}

	public bool GetShownCardInfo(out EntityDef entityDef, out TAG_PREMIUM premium)
	{
		entityDef = null;
		premium = TAG_PREMIUM.NORMAL;
		if (m_currentBigActor == null)
		{
			return false;
		}
		entityDef = m_currentBigActor.GetEntityDef();
		premium = m_currentBigActor.GetPremium();
		if (entityDef == null)
		{
			return false;
		}
		return true;
	}

	public Actor GetShownActor()
	{
		return m_currentBigActor;
	}

	public void OnMassDisenchant(int amount)
	{
		if (!MassDisenchant.Get())
		{
			m_craftingUI.UpdateBankText();
		}
	}

	public long GetUnCommitedArcaneDustChanges()
	{
		return m_unCommitedArcaneDustAdjustments;
	}

	public void AdjustUnCommitedArcaneDustChanges(int amount)
	{
		m_unCommitedArcaneDustAdjustments += amount;
	}

	public void ResetUnCommitedArcaneDustChanges()
	{
		m_unCommitedArcaneDustAdjustments = 0L;
	}

	public int GetNumClientTransactions()
	{
		if (m_pendingClientTransaction == null)
		{
			return 0;
		}
		return m_pendingClientTransaction.GetTransactionAmount(GetShownActor().GetPremium());
	}

	public void NotifyOfTransaction(int amt)
	{
		if (m_pendingClientTransaction == null)
		{
			return;
		}
		if (amt > 0)
		{
			if (GetPendingClientTransaction().GetLastTransactionWasDisenchant())
			{
				GetPendingClientTransaction().Undo();
				return;
			}
			if (m_craftingUI.m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Create)
			{
				if (GetShownActor().GetPremium() == TAG_PREMIUM.NORMAL)
				{
					GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.NormalCreate);
				}
				else if (GetShownActor().GetPremium() == TAG_PREMIUM.GOLDEN)
				{
					GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.GoldenCreate);
				}
			}
			else if (m_craftingUI.m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Upgrade)
			{
				GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.UpgradeToGoldenFromNormal);
				SwitchPremiumView(TAG_PREMIUM.GOLDEN);
			}
		}
		if (amt < 0)
		{
			if (GetPendingClientTransaction().GetLastTransactionWasCrafting())
			{
				GetPendingClientTransaction().Undo();
			}
			else if (GetShownActor().GetPremium() == TAG_PREMIUM.NORMAL)
			{
				GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.NormalDisenchant);
			}
			else if (GetShownActor().GetPremium() == TAG_PREMIUM.GOLDEN)
			{
				GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.GoldenDisenchant);
			}
			else if (GetShownActor().GetPremium() == TAG_PREMIUM.SIGNATURE)
			{
				GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.SignatureDisenchant);
			}
			else if (GetShownActor().GetPremium() == TAG_PREMIUM.DIAMOND)
			{
				GetPendingClientTransaction().Add(CraftingPendingTransaction.Operation.DiamondDisenchant);
			}
		}
	}

	public bool IsCancelling()
	{
		return m_cancellingCraftMode;
	}

	private Actor CreateActorCopy(Actor actor, TAG_PREMIUM premium)
	{
		string cardPath = ActorNames.GetHeroSkinOrHandActor(actor.GetEntityDef(), premium);
		Actor component = AssetLoader.Get().InstantiatePrefab(cardPath, AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
		component.SetFullDefFromActor(actor);
		component.SetEntity(actor.GetEntity());
		component.SetPremium(premium);
		component.UpdateAllComponents();
		return component;
	}

	public void EnterCraftMode(Actor collectionCardActor, Action callback = null)
	{
		m_collectionCardActor = collectionCardActor;
		if (m_collectionCardActor == null)
		{
			return;
		}
		m_cardActors = new CollectionCardActors();
		switch (collectionCardActor.GetPremium())
		{
		case TAG_PREMIUM.DIAMOND:
			m_cardActors.AddCardActor(CreateActorCopy(collectionCardActor, TAG_PREMIUM.DIAMOND));
			break;
		case TAG_PREMIUM.SIGNATURE:
			m_cardActors.AddCardActor(CreateActorCopy(collectionCardActor, TAG_PREMIUM.SIGNATURE));
			break;
		default:
			m_cardActors.AddCardActor(CreateActorCopy(collectionCardActor, TAG_PREMIUM.NORMAL));
			m_cardActors.AddCardActor(CreateActorCopy(collectionCardActor, TAG_PREMIUM.GOLDEN));
			break;
		}
		if (m_cancellingCraftMode || CollectionDeckTray.Get().IsWaitingToDeleteDeck())
		{
			return;
		}
		CollectionManager.Get().GetCollectibleDisplay().HideAllTips();
		m_offClickCatcher.enabled = true;
		TooltipPanelManager.Get().HideKeywordHelp();
		SetupActor(m_collectionCardActor, m_collectionCardActor.GetPremium());
		if (m_cardInfoPane == null && !UniversalInputManager.UsePhoneUI)
		{
			GameObject go = AssetLoader.Get().InstantiatePrefab("CardInfoPane.prefab:b9220edd61d504be38fab162c18e56f1");
			m_cardInfoPane = go.GetComponent<CardInfoPane>();
		}
		if (m_cardInfoPane != null)
		{
			m_cardInfoPane.UpdateContent();
		}
		if (m_craftingUI == null)
		{
			string assetName = (UniversalInputManager.UsePhoneUI ? "CraftingUI_Phone.prefab:3119329ada4ac4a8888187b5b2d373f5" : "CraftingUI.prefab:ef05b5bf5ebb14a22919f0095d75f0b2");
			GameObject go2 = AssetLoader.Get().InstantiatePrefab(assetName);
			m_craftingUI = go2.GetComponent<CraftingUI>();
			m_craftingUI.SetStartingActive();
			GameUtils.SetParent(m_craftingUI, m_showCraftingUIBone.gameObject);
		}
		m_craftingUI.gameObject.SetActive(value: true);
		m_switchPremiumButton.gameObject.SetActive(value: false);
		m_craftingUI.Enable(m_showCraftingUIBone.position, m_hideCraftingUIBone.position);
		if (m_upgradeToGoldenWidget == null)
		{
			m_upgradeToGoldenWidget = WidgetInstance.Create(UPGRADE_TO_GOLDEN_WIDGET_PREFAB);
			m_upgradeToGoldenWidget.Hide();
			m_upgradeToGoldenWidget.RegisterReadyListener(delegate
			{
				GameUtils.SetParent(m_upgradeToGoldenWidget, m_showCraftingUIBone.gameObject);
			});
		}
		m_upgradeToGoldenWidget.Hide();
		FadeEffectsIn();
		UpdateCardInfoPane();
		ShowLeagueLockedCardPopup();
		IsInCraftingMode = true;
		Navigation.Push(delegate
		{
			bool result = CancelCraftMode();
			if (callback != null)
			{
				callback();
			}
			return result;
		});
	}

	private void SetupActor(Actor collectionCardActor, TAG_PREMIUM premium)
	{
		if (m_upsideDownActor != null)
		{
			UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
		}
		if (m_currentBigActor != null)
		{
			UnityEngine.Object.Destroy(m_currentBigActor.gameObject);
		}
		Debug.Log("setting up actor " + collectionCardActor.GetEntityDef()?.ToString() + " " + premium);
		MoveCardToBigSpot(collectionCardActor, premium);
		string cardId = collectionCardActor.GetEntityDef().GetCardId();
		m_pendingClientTransaction = new CraftingPendingTransaction();
		if (GameUtils.IsClassicCard(cardId))
		{
			m_pendingClientTransaction.CardID = GameUtils.GetLegacyCounterpartCardId(cardId);
		}
		else
		{
			m_pendingClientTransaction.CardID = cardId;
		}
		m_pendingClientTransaction.Premium = premium;
		m_pendingClientTransaction.ResetTransactionAmount();
		NetCache.CardValue cardValue = GetCardValue(m_pendingClientTransaction.CardID, premium);
		if (cardValue != null)
		{
			m_pendingClientTransaction.CardValueOverridden = cardValue.IsOverrideActive();
		}
		if (m_craftingUI != null)
		{
			m_craftingUI.Enable(m_showCraftingUIBone.position, m_hideCraftingUIBone.position);
		}
	}

	public bool CancelCraftMode()
	{
		if (m_upgradeToGoldenWidget != null && m_upgradeToGoldenWidgetShown)
		{
			HideUpgradeToGoldenWidget();
			return false;
		}
		StopAllCoroutines();
		m_offClickCatcher.enabled = false;
		m_cancellingCraftMode = true;
		Actor upsideDownActor = m_upsideDownActor;
		Actor currentBigActor = m_currentBigActor;
		if (currentBigActor == null && upsideDownActor != null)
		{
			currentBigActor = upsideDownActor;
			upsideDownActor = null;
		}
		float CANCEL_TIME = 0.2f;
		if (currentBigActor != null)
		{
			iTween.Stop(currentBigActor.gameObject);
			iTween.RotateTo(currentBigActor.gameObject, Vector3.zero, CANCEL_TIME);
			currentBigActor.ToggleForceIdle(bOn: false);
			if (upsideDownActor != null)
			{
				iTween.Stop(upsideDownActor.gameObject);
				upsideDownActor.transform.parent = currentBigActor.transform;
			}
			SoundManager.Get().LoadAndPlay("Card_Transition_In.prefab:3f3fbe896b8b260448e8c7e5d028d971");
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("name", "CancelCraftMode");
			moveArgs.Add("position", m_craftSourcePosition);
			moveArgs.Add("time", CANCEL_TIME);
			moveArgs.Add("oncomplete", "FinishActorMoveAway");
			moveArgs.Add("oncompletetarget", base.gameObject);
			moveArgs.Add("easetype", iTween.EaseType.linear);
			iTween.MoveTo(currentBigActor.gameObject, moveArgs);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", m_craftSourceScale);
			scaleArgs.Add("time", CANCEL_TIME);
			scaleArgs.Add("easetype", iTween.EaseType.linear);
			iTween.ScaleTo(currentBigActor.gameObject, scaleArgs);
		}
		iTween.Stop(m_cardCountTab.gameObject);
		if (GetNumOwnedIncludePending() > 0)
		{
			Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
			moveArgs2.Add("position", m_craftSourcePosition - new Vector3(0f, 12f, 0f));
			moveArgs2.Add("time", 3f * CANCEL_TIME);
			moveArgs2.Add("oncomplete", iTween.EaseType.easeInQuad);
			iTween.MoveTo(m_cardCountTab.gameObject, moveArgs2);
			Hashtable scaleArgs2 = iTweenManager.Get().GetTweenHashTable();
			scaleArgs2.Add("scale", 0.1f * Vector3.one);
			scaleArgs2.Add("time", 3f * CANCEL_TIME);
			scaleArgs2.Add("oncomplete", iTween.EaseType.easeInQuad);
			iTween.ScaleTo(m_cardCountTab.gameObject, scaleArgs2);
		}
		if (upsideDownActor != null)
		{
			iTween.RotateTo(upsideDownActor.gameObject, new Vector3(0f, 359f, 180f), CANCEL_TIME);
			Hashtable moveArgs3 = iTweenManager.Get().GetTweenHashTable();
			moveArgs3.Add("name", "CancelCraftMode2");
			moveArgs3.Add("position", new Vector3(0f, -1f, 0f));
			moveArgs3.Add("time", CANCEL_TIME);
			moveArgs3.Add("islocal", true);
			iTween.MoveTo(upsideDownActor.gameObject, moveArgs3);
			iTween.ScaleTo(upsideDownActor.gameObject, new Vector3(upsideDownActor.transform.localScale.x * 0.8f, upsideDownActor.transform.localScale.y * 0.8f, upsideDownActor.transform.localScale.z * 0.8f), CANCEL_TIME);
		}
		HideAndDestroyRelatedInfo();
		if (m_craftingUI != null && m_craftingUI.IsEnabled())
		{
			m_craftingUI.Disable(m_hideCraftingUIBone.position);
		}
		m_cardCountTab.m_shadow.GetComponent<Animation>().Play("Crafting2ndCardShadowOff");
		FadeEffectsOut();
		if (m_cardInfoPane != null)
		{
			iTween.Stop(m_cardInfoPane.gameObject);
			m_cardInfoPane.gameObject.SetActive(value: false);
		}
		if (m_upgradeToGoldenWidget != null)
		{
			m_upgradeToGoldenWidget.Hide();
			m_upgradeToGoldenWidget.gameObject.SetActive(value: false);
		}
		iTween.ScaleTo(m_switchPremiumButton.gameObject, m_cardCountTabHideScale, 0.4f);
		TellServerAboutWhatUserDid();
		IsInCraftingMode = false;
		return true;
	}

	public void CreateButtonPressed()
	{
		HideAndDestroyRelatedInfo();
		if (m_craftingUI.m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.CreateUpgrade)
		{
			ShowUpgradeToGoldenWidget();
		}
		else if (m_craftingUI.m_buttonCreate.GetCraftingState() == CraftingButton.CraftingState.Upgrade)
		{
			if (!GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_SEEN_UTG_ALERT))
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_offset = m_utgAlertPopupOffset;
				info.m_headerText = GameStrings.Format("GLUE_CRAFTING_UTG_ALERT_HEADER");
				info.m_text = GameStrings.Format("GLUE_CRAFTING_UTG_ALERT_BODY");
				info.m_confirmText = GameStrings.Format("GLUE_CRAFTING_UTG_ALERT_CONFIRM");
				info.m_cancelText = GameStrings.Format("GLUE_CRAFTING_UTG_ALERT_CANCEL");
				info.m_showAlertIcon = false;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
				info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
				AlertPopup.ResponseCallback callback = delegate(AlertPopup.Response response, object userdata)
				{
					GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_SEEN_UTG_ALERT, enableFlag: true);
					SetCraftingRelatedActorsActiveForUpgradeToGoldenPopup(active: true);
					if (response == AlertPopup.Response.CONFIRM)
					{
						m_craftingUI.DoCreate(isUpgrade: true);
					}
				};
				info.m_responseCallback = callback;
				SetCraftingRelatedActorsActiveForUpgradeToGoldenPopup(active: false);
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				m_craftingUI.DoCreate(isUpgrade: true);
			}
		}
		else
		{
			m_craftingUI.DoCreate(isUpgrade: false);
		}
	}

	public void DisenchantButtonPressed()
	{
		HideAndDestroyRelatedInfo();
		m_craftingUI.DoDisenchant();
	}

	public void UpdateBankText()
	{
		if (m_craftingUI != null)
		{
			m_craftingUI.UpdateBankText();
		}
	}

	private void TellServerAboutWhatUserDid()
	{
		if (GetCurrentActor() == null)
		{
			return;
		}
		string cardId = m_pendingClientTransaction.CardID;
		TAG_PREMIUM premium = m_pendingClientTransaction.Premium;
		GameUtils.TranslateCardIdToDbId(cardId);
		if (m_pendingClientTransaction.HasPendingTransactions())
		{
			m_pendingServerTransaction = m_pendingClientTransaction.ShallowCopy();
		}
		int normalOwned = CollectionManager.Get().GetNumCopiesInCollection(cardId, TAG_PREMIUM.NORMAL);
		int goldenOwned = CollectionManager.Get().GetNumCopiesInCollection(cardId, TAG_PREMIUM.GOLDEN);
		int signatureOwned = CollectionManager.Get().GetNumCopiesInCollection(cardId, TAG_PREMIUM.SIGNATURE);
		int diamondOwned = CollectionManager.Get().GetNumCopiesInCollection(cardId, TAG_PREMIUM.DIAMOND);
		NetCache.CardValue cardValue = GetCardValue(cardId, premium);
		if (cardValue == null)
		{
			return;
		}
		if (cardValue.IsOverrideActive() == m_pendingClientTransaction.CardValueOverridden)
		{
			if (m_pendingClientTransaction.HasPendingTransactions())
			{
				int expectedCost = m_pendingClientTransaction.GetExpectedTransactionCost(cardId);
				Network.Get().CraftingTransaction(m_pendingClientTransaction, expectedCost, normalOwned, goldenOwned, signatureOwned, diamondOwned);
			}
		}
		else
		{
			OnCardValueChangedError(null);
		}
		m_pendingClientTransaction = null;
		ResetUnCommitedArcaneDustChanges();
		BnetBar.Get().RefreshCurrency();
	}

	public void OnCardGenericError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_GENERIC_ERROR");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardPermissionError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_CARD_PERMISSION_ERROR");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardDisenchantSoulboundError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_CARD_SOULBOUND");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardCountError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_GENERIC_ERROR");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardCraftingEventNotActiveError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_CARD_CRAFTING_EVENT_NOT_ACTIVE");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardUnknownError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Format("GLUE_COLLECTION_CARD_UNKNOWN_ERROR", sale.Action);
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardValueChangedError(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_ERROR_HEADER");
		info.m_text = GameStrings.Get("GLUE_COLLECTION_CARD_VALUE_CHANGED_ERROR");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	public void OnCardDisenchanted(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		CollectionCardVisual cardVis = CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.GetCardVisual(sale.AssetName, sale.Premium);
		if (cardVis != null && cardVis.IsShown())
		{
			cardVis.OnDoneCrafting();
		}
	}

	public void OnCardCreated(Network.CardSaleResult sale)
	{
		m_pendingServerTransaction = null;
		CollectionCardVisual cardVis = CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.GetCardVisual(sale.AssetName, sale.Premium);
		if (!(cardVis != null) || !cardVis.IsShown())
		{
			return;
		}
		cardVis.OnDoneCrafting();
		if (TemporaryAccountManager.IsTemporaryAccount() && cardVis.GetActor() != null && sale.Action == Network.CardSaleResult.SaleResult.CARD_WAS_BOUGHT)
		{
			EntityDef entityDef = cardVis.GetActor().GetEntityDef();
			if (entityDef != null && (entityDef.GetRarity() == TAG_RARITY.EPIC || entityDef.GetRarity() == TAG_RARITY.LEGENDARY))
			{
				TemporaryAccountManager.Get().ShowEarnCardEventHealUpDialog(TemporaryAccountManager.HealUpReason.CRAFT_CARD);
			}
		}
	}

	public void OnCardUpgraded(Network.CardSaleResult result)
	{
		m_pendingServerTransaction = null;
		CollectiblePageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager();
		CollectionCardVisual normalCardVis = pageManager.GetCardVisual(result.AssetName, TAG_PREMIUM.NORMAL);
		if (normalCardVis != null && normalCardVis.IsShown())
		{
			normalCardVis.OnDoneCrafting();
		}
		CollectionCardVisual goldenCardVis = pageManager.GetCardVisual(result.AssetName, TAG_PREMIUM.GOLDEN);
		if (goldenCardVis != null && goldenCardVis.IsShown())
		{
			goldenCardVis.OnDoneCrafting();
		}
	}

	public void LoadGhostActorIfNecessary()
	{
		if (m_cancellingCraftMode)
		{
			return;
		}
		if (GetNumOwnedIncludePending() > 0)
		{
			if (m_upsideDownActor == null)
			{
				m_currentBigActor = GetAndPositionNewActor(m_currentBigActor, 1);
				m_currentBigActor.name = "CurrentBigActor";
				m_currentBigActor.transform.position = m_floatingCardBone.position;
				m_currentBigActor.transform.localScale = m_floatingCardBone.localScale;
				SetBigActorLayer(inCraftingMode: true);
			}
			else
			{
				m_upsideDownActor.transform.parent = null;
				m_currentBigActor = m_upsideDownActor;
				m_currentBigActor.name = "CurrentBigActor";
				m_currentBigActor.transform.position = m_faceDownCardBone.position;
				m_currentBigActor.transform.localScale = m_faceDownCardBone.localScale;
				m_upsideDownActor = null;
			}
		}
		else
		{
			if (m_upsideDownActor != null)
			{
				Log.Crafting.Print("Deleting rogue m_upsideDownActor!");
				UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
			}
			m_currentBigActor = GetAndPositionNewActor(m_currentBigActor, 0);
			m_currentBigActor.name = "CurrentBigActor";
			m_currentBigActor.transform.position = m_floatingCardBone.position;
			m_currentBigActor.transform.localScale = m_floatingCardBone.localScale;
			iTween.ScaleTo(m_cardCountTab.gameObject, m_cardCountTabHideScale, 0.4f);
			m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
			SetBigActorLayer(inCraftingMode: true);
		}
	}

	public Actor LoadNewActorAndConstructIt()
	{
		if (m_cancellingCraftMode)
		{
			return null;
		}
		if (!m_isCurrentActorAGhost)
		{
			if (m_currentBigActor == null)
			{
				m_currentBigActor = GetAndPositionNewActor(m_upsideDownActor, 0);
			}
			else
			{
				Actor currentBigActor = m_currentBigActor;
				m_currentBigActor = GetAndPositionNewActor(m_currentBigActor, 0);
				Debug.LogWarning("Destroying unexpected m_currentBigActor to prevent a lost reference");
				UnityEngine.Object.Destroy(currentBigActor.gameObject);
			}
			m_isCurrentActorAGhost = false;
			m_currentBigActor.name = "CurrentBigActor";
			m_currentBigActor.transform.position = m_floatingCardBone.position;
			m_currentBigActor.transform.localScale = m_floatingCardBone.localScale;
			SetBigActorLayer(inCraftingMode: true);
		}
		SpellType constructSpell = SpellType.CONSTRUCT;
		EntityDef def = m_collectionCardActor.GetEntityDef();
		if (def != null && def.HasClass(TAG_CLASS.DEATHKNIGHT) && def.HasRuneCost)
		{
			constructSpell = SpellType.DEATH_KNIGHT_CONSTRUCT;
		}
		m_currentBigActor.ActivateSpellBirthState(constructSpell);
		return m_currentBigActor;
	}

	public void ForceNonGhostFlagOn()
	{
		m_isCurrentActorAGhost = false;
	}

	public void FinishCreateAnims(bool showRelatedCards = true)
	{
		if (!(m_currentBigActor == null) && !m_cancellingCraftMode)
		{
			iTween.ScaleTo(m_cardCountTab.gameObject, m_cardCountTabShowScale, 0.4f);
			m_currentBigActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
			m_isCurrentActorAGhost = false;
			int numOwnedCopies = GetNumOwnedIncludePending();
			m_cardCountTab.UpdateText(numOwnedCopies);
			m_cardCountTab.transform.position = GetCardCountPosition();
			if (showRelatedCards)
			{
				ShowRelatedInfo(m_currentBigActor.GetPremium());
			}
		}
	}

	public void FlipCurrentActor()
	{
		if (!(m_currentBigActor == null) && !m_isCurrentActorAGhost && (m_currentBigActor.GetPremium() != TAG_PREMIUM.GOLDEN || GetNumOwnedIncludePending(TAG_PREMIUM.GOLDEN) > 1) && GetNumOwnedIncludePending(m_currentBigActor.GetPremium()) > 1)
		{
			m_cardCountTab.transform.localScale = m_cardCountTabHideScale;
			if (m_upsideDownActor != null)
			{
				Debug.LogError("m_upsideDownActor was not null, destroying object to prevent lost reference");
				UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
				m_upsideDownActor = null;
			}
			m_upsideDownActor = m_currentBigActor;
			m_upsideDownActor.name = "UpsideDownActor";
			m_upsideDownActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
			m_currentBigActor = null;
			iTween.Stop(m_upsideDownActor.gameObject);
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", new Vector3(0f, 350f, 180f));
			rotateArgs.Add("time", 1f);
			iTween.RotateTo(m_upsideDownActor.gameObject, rotateArgs);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("name", "FlipCurrentActor");
			moveArgs.Add("position", m_faceDownCardBone.position);
			moveArgs.Add("time", 1f);
			iTween.MoveTo(m_upsideDownActor.gameObject, moveArgs);
			StartCoroutine(ReplaceFaceDownActorWithHiddenCard());
		}
	}

	public void FinishFlipCurrentActorEarly()
	{
		StopAllCoroutines();
		if (m_currentBigActor != null)
		{
			iTween.Stop(m_currentBigActor.gameObject);
		}
		if (m_upsideDownActor != null)
		{
			iTween.Stop(m_upsideDownActor.gameObject);
		}
		m_currentBigActor.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		m_currentBigActor.transform.position = m_floatingCardBone.position;
		m_currentBigActor.Show();
		GameObject standIn = m_currentBigActor.GetHiddenStandIn();
		if (!(standIn == null))
		{
			standIn.SetActive(value: false);
			UnityEngine.Object.Destroy(standIn);
		}
	}

	public void FlipUpsideDownCard(Actor oldActor)
	{
		if (!m_cancellingCraftMode)
		{
			int numOwnedCopies = GetNumOwnedIncludePending();
			if (numOwnedCopies > 1)
			{
				m_upsideDownActor = GetAndPositionNewUpsideDownActor(m_currentBigActor, fromPage: false);
				m_upsideDownActor.name = "UpsideDownActor";
				StartCoroutine(ReplaceFaceDownActorWithHiddenCard());
			}
			if (numOwnedCopies >= 1)
			{
				iTween.ScaleTo(m_cardCountTab.gameObject, m_cardCountTabShowScale, 0.4f);
				m_cardCountTab.transform.position = GetCardCountPosition();
				m_cardCountTab.UpdateText(numOwnedCopies);
			}
			if (m_isCurrentActorAGhost)
			{
				m_currentBigActor.gameObject.transform.position = m_floatingCardBone.position;
			}
			else
			{
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("name", "FlipUpsideDownCard");
				moveArgs.Add("position", m_floatingCardBone.position);
				moveArgs.Add("time", m_timeForCardToFlipUp);
				moveArgs.Add("easetype", m_easeTypeForCardFlip);
				iTween.MoveTo(m_currentBigActor.gameObject, moveArgs);
			}
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
			rotateArgs.Add("time", m_timeForCardToFlipUp);
			rotateArgs.Add("easetype", m_easeTypeForCardFlip);
			rotateArgs.Add("oncomplete", "OnCardFlipComplete");
			rotateArgs.Add("oncompletetarget", base.gameObject);
			iTween.RotateTo(m_currentBigActor.gameObject, rotateArgs);
			StartCoroutine(ReplaceHiddenCardwithRealActor(m_currentBigActor));
		}
	}

	private IEnumerator ReplaceFaceDownActorWithHiddenCard()
	{
		while (m_upsideDownActor != null && m_upsideDownActor.transform.localEulerAngles.z < 90f)
		{
			yield return null;
		}
		if (!(m_upsideDownActor == null))
		{
			GameObject hiddenBuddy = UnityEngine.Object.Instantiate(m_hiddenActor.gameObject);
			hiddenBuddy.GetComponent<Actor>().UpdateCardBack();
			hiddenBuddy.transform.parent = m_upsideDownActor.transform;
			hiddenBuddy.transform.localScale = new Vector3(1f, 1f, 1f);
			hiddenBuddy.transform.localPosition = new Vector3(0f, 0f, 0f);
			hiddenBuddy.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			m_upsideDownActor.Hide();
			m_upsideDownActor.SetHiddenStandIn(hiddenBuddy);
		}
	}

	private IEnumerator ReplaceHiddenCardwithRealActor(Actor actor)
	{
		while (actor != null && actor.transform.localEulerAngles.z > 90f && actor.transform.localEulerAngles.z < 270f)
		{
			yield return null;
		}
		if (!(actor == null))
		{
			actor.Show();
			GameObject standIn = actor.GetHiddenStandIn();
			if (!(standIn == null))
			{
				standIn.SetActive(value: false);
				UnityEngine.Object.Destroy(standIn);
			}
		}
	}

	private void OnCardFlipComplete()
	{
		ShowRelatedInfo(m_currentBigActor.GetPremium());
	}

	public CraftingPendingTransaction GetPendingClientTransaction()
	{
		return m_pendingClientTransaction;
	}

	public CraftingPendingTransaction GetPendingServerTransaction()
	{
		return m_pendingServerTransaction;
	}

	public void ShowCraftingUI(UIEvent e)
	{
		if (m_craftingUI.IsEnabled())
		{
			m_craftingUI.Disable(m_hideCraftingUIBone.position);
		}
		else
		{
			m_craftingUI.Enable(m_showCraftingUIBone.position, m_hideCraftingUIBone.position);
		}
	}

	public void SetCraftingUIActive(bool active)
	{
		if (m_craftingUI != null)
		{
			m_craftingUI.gameObject.SetActive(active);
		}
	}

	public void ShowUpgradeToGoldenWidget()
	{
		if (m_upgradeToGoldenWidget != null)
		{
			m_upgradeToGoldenWidget.gameObject.SetActive(value: true);
			UpgradeToGoldenPopup popup = m_upgradeToGoldenWidget.GetComponentInChildren<UpgradeToGoldenPopup>();
			if (popup != null)
			{
				popup.SetInfo(m_pendingClientTransaction, m_craftingUI, m_showUpgradeToGoldenPopupBone);
				StartCoroutine(popup.ShowWhenReadyRoutine());
			}
			m_upgradeToGoldenWidgetShown = true;
		}
	}

	public void HideUpgradeToGoldenWidget()
	{
		if (m_upgradeToGoldenWidget != null)
		{
			UpgradeToGoldenPopup popup = m_upgradeToGoldenWidget.GetComponentInChildren<UpgradeToGoldenPopup>();
			if (popup != null)
			{
				popup.OnHide();
			}
			m_upgradeToGoldenWidgetShown = false;
		}
		ShowRelatedInfo(m_currentBigActor.GetPremium());
	}

	public void SetCraftingRelatedActorsActiveForUpgradeToGoldenPopup(bool active)
	{
		if (!HasUpgradeToGoldenEnabled())
		{
			return;
		}
		if (m_craftingUI != null)
		{
			if (active)
			{
				m_craftingUI.m_buttonDisenchant.GetComponent<Collider>().enabled = true;
				m_craftingUI.m_buttonCreate.GetComponent<Collider>().enabled = true;
				if (GetNumOwnedIncludePending() == 0)
				{
					LoadGhostActorIfNecessary();
				}
				m_currentBigActor.Show();
				m_craftingUI.Enable(m_showCraftingUIBone.position, m_hideCraftingUIBone.position);
			}
			else
			{
				FinishCreateAnims(active);
				m_craftingUI.Disable(m_hideCraftingUIBone.position);
			}
		}
		if (m_cardInfoPane != null)
		{
			m_cardInfoPane.gameObject.SetActive(active);
		}
		if (m_currentBigActor != null)
		{
			m_currentBigActor.gameObject.SetActive(active);
		}
		if (m_upsideDownActor != null)
		{
			m_upsideDownActor.gameObject.SetActive(active);
		}
		if (m_cardCountTab != null)
		{
			m_cardCountTab.gameObject.SetActive(active);
		}
	}

	private Actor GetCurrentActor()
	{
		if (m_currentBigActor != null)
		{
			return m_currentBigActor;
		}
		if (m_upsideDownActor != null)
		{
			return m_upsideDownActor;
		}
		return null;
	}

	private void MoveCardToBigSpot(Actor collectionCardActor, TAG_PREMIUM premium)
	{
		if (collectionCardActor == null)
		{
			return;
		}
		EntityDef entityDef = collectionCardActor.GetEntityDef();
		if (entityDef == null)
		{
			return;
		}
		int numOwnedCopies = GetNumOwnedIncludePending(entityDef.GetCardId(), premium);
		if (m_currentBigActor != null)
		{
			Debug.LogError("m_currentBigActor was not null, destroying object before we lose the reference");
			UnityEngine.Object.Destroy(m_currentBigActor.gameObject);
			m_currentBigActor = null;
		}
		m_currentBigActor = GetAndPositionNewActor(m_cardActors.GetActor(premium), numOwnedCopies);
		if (m_currentBigActor == null)
		{
			Debug.LogError("CraftingManager.MoveCardToBigSpot - GetAndPositionNewActor returned null");
			return;
		}
		m_currentBigActor.name = "CurrentBigActor";
		m_craftSourcePosition = collectionCardActor.transform.position;
		m_craftSourceScale = collectionCardActor.transform.lossyScale;
		m_craftSourceScale = Vector3.one * Mathf.Min(m_craftSourceScale.x, m_craftSourceScale.y, m_craftSourceScale.z);
		m_currentBigActor.transform.position = m_craftSourcePosition;
		TransformUtil.SetWorldScale(m_currentBigActor, m_craftSourceScale);
		SetBigActorLayer(inCraftingMode: true);
		m_currentBigActor.ToggleForceIdle(bOn: true);
		m_currentBigActor.SetActorState(ActorStateType.CARD_IDLE);
		if (entityDef.IsHeroSkin())
		{
			m_cardCountTab.gameObject.SetActive(value: false);
		}
		else
		{
			m_cardCountTab.gameObject.SetActive(value: true);
			if (numOwnedCopies > 1)
			{
				if (m_upsideDownActor != null)
				{
					Debug.LogError("m_upsideDownActor was not null, destroying object before we lose the reference");
					UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
					m_upsideDownActor = null;
				}
				m_upsideDownActor = GetAndPositionNewUpsideDownActor(collectionCardActor, fromPage: true);
				m_upsideDownActor.name = "UpsideDownActor";
				StartCoroutine(ReplaceFaceDownActorWithHiddenCard());
			}
			if (numOwnedCopies > 0)
			{
				m_cardCountTab.UpdateText(numOwnedCopies);
				m_cardCountTab.transform.position = new Vector3(collectionCardActor.transform.position.x, collectionCardActor.transform.position.y - 2f, collectionCardActor.transform.position.z);
			}
		}
		FinishBigCardMove();
	}

	private static bool IncludesRuneCards(List<RelatedCardsDbfRecord> relatedCards)
	{
		if (relatedCards == null)
		{
			return false;
		}
		foreach (RelatedCardsDbfRecord card in relatedCards)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(card.RelatedCardDatabaseId);
			if (entityDef != null && entityDef.HasRuneCost)
			{
				return true;
			}
		}
		return false;
	}

	private List<string> GetRelatedCardIds(EntityDef def, out bool offsetCardNameForRunes)
	{
		List<string> result = new List<string>();
		string relatedCardID = "";
		offsetCardNameForRunes = false;
		int databaseID = GameUtils.TranslateCardIdToDbId(def.GetCardId());
		CardDbfRecord record = GameDbf.Card.GetRecord(databaseID);
		if (record != null)
		{
			List<RelatedCardsDbfRecord> relatedCards = record.RelatedCards;
			if (IncludesRuneCards(relatedCards))
			{
				offsetCardNameForRunes = true;
			}
			foreach (RelatedCardsDbfRecord item in relatedCards)
			{
				int relatedCardDbId = item.RelatedCardDatabaseId;
				relatedCardID = GameDbf.Card.GetRecord(relatedCardDbId)?.NoteMiniGuid;
				if (!string.IsNullOrEmpty(relatedCardID))
				{
					result.Add(relatedCardID);
				}
			}
			if (result.Count > 0)
			{
				return result;
			}
		}
		int relatedCardId = def.GetTag(GAME_TAG.COLLECTION_RELATED_CARD_DATABASE_ID);
		relatedCardID = GameDbf.Card.GetRecord(relatedCardId)?.NoteMiniGuid;
		if (!string.IsNullOrEmpty(relatedCardID))
		{
			result.Add(relatedCardID);
			return result;
		}
		if (def.IsHero())
		{
			relatedCardID = GameUtils.GetHeroPowerCardIdFromHero(def.GetCardId());
			if (!string.IsNullOrEmpty(relatedCardID))
			{
				result.Add(relatedCardID);
				return result;
			}
		}
		if (def.IsQuest())
		{
			int cardDBId = def.GetTag(GAME_TAG.QUEST_REWARD_DATABASE_ID);
			relatedCardID = GameDbf.Card.GetRecord(cardDBId)?.NoteMiniGuid;
			if (!string.IsNullOrEmpty(relatedCardID))
			{
				result.Add(relatedCardID);
				return result;
			}
		}
		if (def.IsQuestline())
		{
			int cardDBId2 = def.GetTag(GAME_TAG.QUESTLINE_FINAL_REWARD_DATABASE_ID);
			relatedCardID = GameDbf.Card.GetRecord(cardDBId2)?.NoteMiniGuid;
			if (!string.IsNullOrEmpty(relatedCardID))
			{
				result.Add(relatedCardID);
				return result;
			}
		}
		return result;
	}

	private void ShowRelatedInfo(TAG_PREMIUM premium)
	{
		if (m_currentBigActor == null)
		{
			Debug.LogError("Unexpected error in ShowRelatedBigCard. Current big actor was null");
			return;
		}
		EntityDef currentEntityDef = m_currentBigActor.GetEntityDef();
		if (currentEntityDef == null)
		{
			Debug.LogError("Unexpected error in ShowRelatedBigCard. Current big actor's entity def was null");
			return;
		}
		int signatureId = ActorNames.GetSignatureFrameId(currentEntityDef.GetCardId());
		if (m_currentBigActor.GetPremium() == TAG_PREMIUM.SIGNATURE && signatureId >= 2)
		{
			TooltipPanelManager.Get().UpdateSignatureTooltipAtCustomTransform(currentEntityDef, m_currentBigActor, m_signatureTooltipBone);
			return;
		}
		bool offsetCardNameForRunes;
		List<string> relatedCardIds = GetRelatedCardIds(currentEntityDef, out offsetCardNameForRunes);
		if (relatedCardIds.Count == 0)
		{
			return;
		}
		if (relatedCardIds.Count > 1)
		{
			foreach (string relatedCardId in relatedCardIds)
			{
				AddRelatedCard(relatedCardId, premium, offsetCardNameForRunes);
			}
			ShowRelatedCards(open: true);
		}
		else
		{
			ShowSingleRelatedCard(currentEntityDef, relatedCardIds[0], premium);
		}
	}

	private void ShowSingleRelatedCard(EntityDef currentEntityDef, string relatedCardId, TAG_PREMIUM premium)
	{
		int numOwnedCopies = GetNumOwnedIncludePending();
		Actor actorToClone = GetTemplateActorForType(currentEntityDef.GetCardType(), premium, currentEntityDef.GetCardId());
		if (actorToClone.GetEntityDef() == null || actorToClone.GetEntityDef().GetCardId() != relatedCardId || actorToClone.GetPremium() != premium || premium == TAG_PREMIUM.SIGNATURE)
		{
			using DefLoader.DisposableFullDef relatedCardDef = DefLoader.Get().GetFullDef(relatedCardId, m_currentBigActor.CardPortraitQuality);
			actorToClone.SetEntityDef(relatedCardDef.EntityDef);
			if (premium == TAG_PREMIUM.SIGNATURE)
			{
				premium = ((!actorToClone.HasSignaturePortraitTexture()) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.SIGNATURE);
			}
			actorToClone.SetPremium(premium);
		}
		if (m_currentRelatedCardActor != null)
		{
			Debug.LogWarning("Current related card actor was not new when creating a new one. Ensure we cleanup this actor");
			HideAndDestroyRelatedInfo();
		}
		m_currentRelatedCardActor = GetAndPositionNewActor(actorToClone, numOwnedCopies);
		if (currentEntityDef.IsQuest() || currentEntityDef.IsQuestline())
		{
			EntityDef questDef = DefLoader.Get().GetEntityDef(relatedCardId);
			if (questDef != null)
			{
				bool isPremium = m_currentRelatedCardActor.GetPremium() == TAG_PREMIUM.GOLDEN;
				QuestCardRewardOverlay overlay = AddQuestOverlay(questDef, isPremium, m_currentRelatedCardActor.gameObject);
				if (currentEntityDef.IsQuestline())
				{
					overlay?.EnableRewardObjects();
				}
			}
		}
		LayerUtils.SetLayer(m_currentRelatedCardActor.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_currentRelatedCardActor.gameObject.transform.parent = m_currentBigActor.transform;
		StartCoroutine(RevealRelatedCard(m_currentRelatedCardActor));
	}

	private IEnumerator RevealRelatedCard(Actor actor)
	{
		if (actor == null)
		{
			yield break;
		}
		Spell ghostSpell = actor.GetSpellIfLoaded(SpellType.GHOSTMODE);
		if (ghostSpell != null)
		{
			while (!ghostSpell.IsFinished())
			{
				yield return null;
			}
		}
		if (!(actor.gameObject == null))
		{
			actor.Show();
			GameObject obj = actor.gameObject;
			Transform obj2 = obj.transform;
			obj2.localPosition = HERO_POWER_START_POSITION;
			obj2.localScale = HERO_POWER_START_SCALE;
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("position", HERO_POWER_POSITION.Value);
			moveArgs.Add("islocal", true);
			moveArgs.Add("time", HERO_POWER_TWEEN_TIME);
			iTween.MoveTo(obj, moveArgs);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", HERO_POWER_SCALE.Value);
			scaleArgs.Add("islocal", true);
			scaleArgs.Add("time", HERO_POWER_TWEEN_TIME);
			iTween.ScaleTo(obj, scaleArgs);
		}
	}

	public void HideAndDestroyRelatedInfo()
	{
		TooltipPanelManager.Get().HideKeywordHelp();
		if (m_currentRelatedCardActor != null)
		{
			UnityEngine.Object.Destroy(m_currentRelatedCardActor.gameObject);
			m_currentRelatedCardActor = null;
		}
		ShowRelatedCards(open: false);
	}

	private void FinishBigCardMove()
	{
		if (!(m_currentBigActor == null))
		{
			int numOwnedIncludePending = GetNumOwnedIncludePending();
			SoundManager.Get().LoadAndPlay("Card_Transition_Out.prefab:aecf5b5837772844b9d2db995744df82");
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("name", "FinishBigCardMove");
			moveArgs.Add("position", m_floatingCardBone.position);
			moveArgs.Add("time", 0.4f);
			moveArgs.Add("oncomplete", "FinishActorMoveTowardsScreen");
			moveArgs.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_currentBigActor.gameObject, moveArgs);
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", m_floatingCardBone.localScale);
			scaleArgs.Add("time", 0.4f);
			scaleArgs.Add("easetype", iTween.EaseType.easeOutQuad);
			iTween.ScaleTo(m_currentBigActor.gameObject, scaleArgs);
			if (numOwnedIncludePending > 0)
			{
				m_cardCountTab.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				iTween.MoveTo(m_cardCountTab.gameObject, GetCardCountPosition(), 0.4f);
				iTween.ScaleTo(m_cardCountTab.gameObject, m_cardCountTabShowScale, 0.4f);
			}
		}
	}

	private void UpdateCardInfoPane()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			if (m_currentBigActor == null)
			{
				Debug.LogError("CraftingManager.UpdateCardInfoPane -  m_currentBigActor is null");
				return;
			}
			if (m_cardInfoPane == null)
			{
				Debug.LogError("CraftingManager.UpdateCardInfoPane - m_cardInfoPane is null");
				return;
			}
			m_cardInfoPane.gameObject.SetActive(value: true);
			m_cardInfoPane.UpdateContent();
			m_cardInfoPane.transform.position = m_currentBigActor.transform.position - new Vector3(0f, 1f, 0f);
			Vector3 orgScale = m_cardInfoPaneBone.localScale;
			m_cardInfoPane.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			iTween.MoveTo(m_cardInfoPane.gameObject, m_cardInfoPaneBone.position, 0.5f);
			iTween.ScaleTo(m_cardInfoPane.gameObject, orgScale, 0.5f);
		}
	}

	private void FinishActorMoveTowardsScreen()
	{
		ShowRelatedInfo(m_currentBigActor.GetPremium());
	}

	private void FinishActorMoveAway()
	{
		m_cancellingCraftMode = false;
		iTween.Stop(m_cardCountTab.gameObject);
		m_cardCountTab.transform.position = new Vector3(0f, 307f, -10f);
		if (m_upsideDownActor != null)
		{
			UnityEngine.Object.Destroy(m_upsideDownActor.gameObject);
		}
		if (m_currentBigActor != null)
		{
			UnityEngine.Object.Destroy(m_currentBigActor.gameObject);
		}
		LoadRandomCardBack();
	}

	private void FadeEffectsIn()
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
		screenEffectParameters.Blur = new BlurParameters(1f, 1f);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void FadeEffectsOut()
	{
		m_screenEffectsHandle.StopEffect();
	}

	private void OnVignetteFinished()
	{
		SetBigActorLayer(inCraftingMode: false);
		if (GetCurrentCardVisual() != null)
		{
			GetCurrentCardVisual().OnDoneCrafting();
		}
		if (m_currentBigActor != null)
		{
			m_currentBigActor.name = "USED_TO_BE_CurrentBigActor";
			StartCoroutine(MakeSureActorIsCleanedUp(m_currentBigActor));
		}
		m_currentBigActor = null;
		m_craftingUI.gameObject.SetActive(value: false);
	}

	private IEnumerator MakeSureActorIsCleanedUp(Actor oldActor)
	{
		yield return new WaitForSeconds(1f);
		if (!(oldActor == null))
		{
			UnityEngine.Object.DestroyImmediate(oldActor);
		}
	}

	private Actor GetAndPositionNewUpsideDownActor(Actor oldActor, bool fromPage)
	{
		Actor newActor = GetAndPositionNewActor(oldActor, 1);
		LayerUtils.SetLayer(newActor.gameObject, GameLayer.IgnoreFullScreenEffects);
		if (fromPage)
		{
			newActor.transform.position = oldActor.transform.position + new Vector3(0f, -2f, 0f);
			newActor.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
			iTween.RotateTo(newActor.gameObject, new Vector3(0f, 350f, 180f), 0.4f);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("name", "GetAndPositionNewUpsideDownActor");
			moveArgs.Add("position", m_faceDownCardBone.position);
			moveArgs.Add("time", 0.4f);
			iTween.MoveTo(newActor.gameObject, moveArgs);
			iTween.ScaleTo(newActor.gameObject, m_faceDownCardBone.localScale, 0.4f);
		}
		else
		{
			newActor.transform.localEulerAngles = new Vector3(0f, 350f, 180f);
			newActor.transform.position = m_faceDownCardBone.position + new Vector3(0f, -6f, 0f);
			newActor.transform.localScale = m_faceDownCardBone.localScale;
			Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
			moveArgs2.Add("name", "GetAndPositionNewUpsideDownActor");
			moveArgs2.Add("position", m_faceDownCardBone.position);
			moveArgs2.Add("time", m_timeForBackCardToMoveUp);
			moveArgs2.Add("easetype", m_easeTypeForCardMoveUp);
			moveArgs2.Add("delay", m_delayBeforeBackCardMovesUp);
			iTween.MoveTo(newActor.gameObject, moveArgs2);
		}
		return newActor;
	}

	private Actor GetAndPositionNewActor(Actor oldActor, int numCopies)
	{
		bool isZilliaxCard = GameUtils.IsZilliaxCard(oldActor.GetEntityDef());
		Actor newActor = ((numCopies != 0 || isZilliaxCard) ? GetNonGhostActor(oldActor) : GetGhostActor(oldActor));
		if (newActor != null)
		{
			newActor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		}
		return newActor;
	}

	private Actor GetGhostActor(Actor actor)
	{
		m_isCurrentActorAGhost = true;
		bool isGolden = actor.GetPremium() == TAG_PREMIUM.GOLDEN;
		bool isSignature = actor.GetPremium() == TAG_PREMIUM.SIGNATURE;
		bool isDiamond = actor.GetPremium() == TAG_PREMIUM.DIAMOND;
		Actor actorToUse = m_ghostMinionActor;
		EntityDef actorEntityDef = actor.GetEntityDef();
		if (actorEntityDef == null)
		{
			Debug.LogError("CraftingManager::GetGhostActor - EntityDef is null");
		}
		switch (actorEntityDef.GetCardType())
		{
		case TAG_CARDTYPE.SPELL:
			if (isGolden)
			{
				actorToUse = m_ghostGoldenSpellActor;
			}
			else if (isSignature)
			{
				int frameId5 = ActorNames.GetSignatureFrameId(actorEntityDef.GetCardId());
				m_ghostSignatureHandActors.TryGetValue(frameId5, out actorToUse);
			}
			else
			{
				actorToUse = m_ghostSpellActor;
			}
			break;
		case TAG_CARDTYPE.WEAPON:
			if (isGolden)
			{
				actorToUse = m_ghostGoldenWeaponActor;
			}
			else if (isSignature)
			{
				int frameId3 = ActorNames.GetSignatureFrameId(actorEntityDef.GetCardId());
				m_ghostSignatureHandActors.TryGetValue(frameId3, out actorToUse);
			}
			else
			{
				actorToUse = m_ghostWeaponActor;
			}
			break;
		case TAG_CARDTYPE.MINION:
			if (isGolden)
			{
				actorToUse = m_ghostGoldenMinionActor;
			}
			else if (isSignature)
			{
				int frameId2 = ActorNames.GetSignatureFrameId(actorEntityDef.GetCardId());
				m_ghostSignatureHandActors.TryGetValue(frameId2, out actorToUse);
			}
			else
			{
				actorToUse = ((!isDiamond) ? m_ghostMinionActor : m_ghostDiamondMinionActor);
			}
			break;
		case TAG_CARDTYPE.HERO:
			if (isGolden)
			{
				actorToUse = m_ghostGoldenHeroActor;
			}
			else if (isSignature)
			{
				int frameId4 = ActorNames.GetSignatureFrameId(actorEntityDef.GetCardId());
				m_ghostSignatureHandActors.TryGetValue(frameId4, out actorToUse);
			}
			else
			{
				actorToUse = m_ghostHeroActor;
			}
			break;
		case TAG_CARDTYPE.HERO_POWER:
			actorToUse = ((!isGolden) ? m_ghostHeroPowerActor : m_ghostGoldenHeroPowerActor);
			break;
		case TAG_CARDTYPE.LOCATION:
			if (isGolden)
			{
				actorToUse = m_ghostGoldenLocationActor;
			}
			else if (isSignature)
			{
				int frameId = ActorNames.GetSignatureFrameId(actorEntityDef.GetCardId());
				m_ghostSignatureHandActors.TryGetValue(frameId, out actorToUse);
			}
			else
			{
				actorToUse = m_ghostLocationActor;
			}
			break;
		default:
			Debug.LogError("CraftingManager.GetGhostActor() - tried to get a ghost actor for a cardtype that we haven't anticipated!!");
			break;
		}
		return SetUpGhostActor(actorToUse, actor);
	}

	private Actor GetNonGhostActor(Actor actor)
	{
		m_isCurrentActorAGhost = false;
		return SetUpNonGhostActor(GetTemplateActor(actor), actor);
	}

	private Actor GetTemplateActorForType(TAG_CARDTYPE type, TAG_PREMIUM premium, string cardId)
	{
		bool isGolden = premium == TAG_PREMIUM.GOLDEN;
		bool isSignature = premium == TAG_PREMIUM.SIGNATURE;
		bool isDiamond = premium == TAG_PREMIUM.DIAMOND;
		switch (type)
		{
		case TAG_CARDTYPE.SPELL:
			if (isGolden)
			{
				return m_templateGoldenSpellActor;
			}
			if (isSignature)
			{
				int frameId4 = 0;
				if (cardId == null)
				{
					Debug.LogError("Crafting Manager: Signature template actor requested without cardId.");
					return null;
				}
				frameId4 = ActorNames.GetSignatureFrameId(cardId);
				m_templateSignatureHandActors.TryGetValue(frameId4, out var templateSignatureActor4);
				return templateSignatureActor4;
			}
			return m_templateSpellActor;
		case TAG_CARDTYPE.WEAPON:
			if (isGolden)
			{
				return m_templateGoldenWeaponActor;
			}
			return m_templateWeaponActor;
		case TAG_CARDTYPE.MINION:
			if (isGolden)
			{
				return m_templateGoldenMinionActor;
			}
			if (isSignature)
			{
				int frameId2 = 0;
				if (cardId == null)
				{
					Debug.LogError("Crafting Manager: Signature template actor requested without cardId.");
					return null;
				}
				frameId2 = ActorNames.GetSignatureFrameId(cardId);
				m_templateSignatureHandActors.TryGetValue(frameId2, out var templateSignatureActor2);
				return templateSignatureActor2;
			}
			if (isDiamond)
			{
				return m_templateDiamondMinionActor;
			}
			return m_templateMinionActor;
		case TAG_CARDTYPE.HERO:
			if (isGolden)
			{
				return m_templateGoldenHeroActor;
			}
			if (isSignature)
			{
				int frameId3 = 0;
				if (cardId == null)
				{
					Debug.LogError("Crafting Manager: Signature template actor requested without cardId.");
					return null;
				}
				frameId3 = ActorNames.GetSignatureFrameId(cardId);
				m_templateSignatureHandActors.TryGetValue(frameId3, out var templateSignatureActor3);
				return templateSignatureActor3;
			}
			return m_templateHeroActor;
		case TAG_CARDTYPE.HERO_POWER:
			if (isGolden)
			{
				return m_templateGoldenHeroPowerActor;
			}
			return m_templateHeroPowerActor;
		case TAG_CARDTYPE.LOCATION:
			if (isGolden || isDiamond)
			{
				return m_templateGoldenLocationActor;
			}
			if (isSignature)
			{
				int frameId = 0;
				if (cardId == null)
				{
					Debug.LogError("Crafting Manager: Signature template actor requested without cardId.");
					return null;
				}
				frameId = ActorNames.GetSignatureFrameId(cardId);
				m_templateSignatureHandActors.TryGetValue(frameId, out var templateSignatureActor);
				return templateSignatureActor;
			}
			return m_templateLocationActor;
		default:
			Debug.LogError("CraftingManager.GetTemplateActorForType() - tried to get a actor for a cardtype that we haven't anticipated!!");
			return m_templateMinionActor;
		}
	}

	private Actor GetTemplateActor(Actor actor)
	{
		EntityDef actorEntityDef = actor.GetEntityDef();
		return GetTemplateActorForType(actorEntityDef.GetCardType(), actor.GetPremium(), actorEntityDef.GetCardId());
	}

	private Actor SetUpNonGhostActor(Actor templateActor, Actor actor)
	{
		Actor actor2 = UnityEngine.Object.Instantiate(templateActor);
		actor2.SetFullDefFromActor(actor);
		actor2.SetPremium(actor.GetPremium());
		actor2.SetUnlit();
		actor2.UpdateAllComponents();
		return actor2;
	}

	private Actor SetUpGhostActor(Actor templateActor, Actor actor)
	{
		if (templateActor == null || actor == null)
		{
			Debug.LogError("CraftingManager.SetUpGhostActor - passed arguments are null");
			return null;
		}
		Actor newActor = UnityEngine.Object.Instantiate(templateActor);
		newActor.SetFullDefFromActor(actor);
		newActor.SetPremium(actor.GetPremium());
		newActor.UpdateAllComponents();
		newActor.UpdatePortraitTexture();
		newActor.UpdateCardColor();
		newActor.SetUnlit();
		newActor.Hide();
		if (actor.isMissingCard())
		{
			newActor.ActivateSpellBirthState(SpellType.MISSING_BIGCARD);
		}
		else
		{
			newActor.ActivateSpellBirthState(SpellType.GHOSTMODE);
		}
		StartCoroutine(ShowAfterTwoFrames(newActor));
		return newActor;
	}

	private IEnumerator ShowAfterTwoFrames(Actor actorToShow)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (!(actorToShow != m_currentBigActor))
		{
			actorToShow.Show();
		}
	}

	private void SetBigActorLayer(bool inCraftingMode)
	{
		if (!(m_currentBigActor == null))
		{
			GameLayer layer = (inCraftingMode ? GameLayer.IgnoreFullScreenEffects : GameLayer.CardRaycast);
			LayerUtils.SetLayer(m_currentBigActor.gameObject, layer);
		}
	}

	private CollectionCardVisual GetCurrentCardVisual()
	{
		if (!GetShownCardInfo(out var entityDef, out var premium))
		{
			return null;
		}
		return CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.GetCardVisual(entityDef.GetCardId(), premium);
	}

	public int GetNumOwnedIncludePending(TAG_PREMIUM? premium)
	{
		string cardId = m_collectionCardActor.GetEntityDef().GetCardId();
		return GetNumOwnedIncludePending(cardId, premium);
	}

	public int GetNumOwnedIncludePending(string cardId, TAG_PREMIUM? premium)
	{
		int numOwned = 0;
		numOwned = ((!premium.HasValue) ? CollectionManager.Get().GetTotalNumCopiesInCollection(cardId) : CollectionManager.Get().GetNumCopiesInCollection(cardId, premium.Value));
		if (IsPendingTransactionForCard(cardId))
		{
			if (!premium.HasValue)
			{
				numOwned += m_pendingClientTransaction.GetTransactionAmount(TAG_PREMIUM.NORMAL);
				numOwned += m_pendingClientTransaction.GetTransactionAmount(TAG_PREMIUM.GOLDEN);
				numOwned += m_pendingClientTransaction.GetTransactionAmount(TAG_PREMIUM.SIGNATURE);
			}
			else
			{
				numOwned += m_pendingClientTransaction.GetTransactionAmount(premium.Value);
			}
		}
		return numOwned;
	}

	public int GetNumOwnedIncludePending()
	{
		if (m_pendingClientTransaction != null)
		{
			return GetNumOwnedIncludePending(m_pendingClientTransaction.Premium);
		}
		return GetNumOwnedIncludePending(m_collectionCardActor.GetPremium());
	}

	public bool IsPendingTransactionForCard(string cardId)
	{
		if (m_pendingClientTransaction == null)
		{
			return false;
		}
		if (m_pendingClientTransaction.CardID == cardId)
		{
			return true;
		}
		if (GameUtils.IsClassicCard(cardId))
		{
			string couterpartCardId = GameUtils.GetLegacyCounterpartCardId(cardId);
			if (m_pendingClientTransaction.CardID == couterpartCardId)
			{
				return true;
			}
		}
		return false;
	}

	private QuestCardRewardOverlay AddQuestOverlay(EntityDef def, bool isPremium, GameObject parent)
	{
		if (m_questCardRewardOverlay == null)
		{
			Debug.LogWarning("Attempted to add quest overlay to a card, but no prefab was set on CraftinManager");
			return null;
		}
		GameObject go = UnityEngine.Object.Instantiate(m_questCardRewardOverlay.gameObject, parent.transform);
		if (go == null)
		{
			Debug.LogError("Could not instantiate a new quest reward overlay from prefab");
			return null;
		}
		QuestCardRewardOverlay questOverlay = go.GetComponent<QuestCardRewardOverlay>();
		if (questOverlay == null)
		{
			Debug.LogError("Newly instantiated quest reward overlay game object does not contain a QuestCardRewardOverlay component.");
			UnityEngine.Object.Destroy(go);
			return null;
		}
		questOverlay.SetEntityType(def, isPremium);
		return questOverlay;
	}

	private void LoadActor(string actorPath, ref Actor actor)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		go.transform.position = new Vector3(-99999f, 99999f, 99999f);
		actor = go.GetComponent<Actor>();
		actor.TurnOffCollider();
	}

	private void LoadActor(string actorPath, ref Actor actor, ref Actor actorCopy)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		go.transform.position = new Vector3(-99999f, 99999f, 99999f);
		actor = go.GetComponent<Actor>();
		actorCopy = UnityEngine.Object.Instantiate(actor);
		actor.TurnOffCollider();
		actorCopy.TurnOffCollider();
	}

	private void ShowLeagueLockedCardPopup()
	{
		if (GetShownCardInfo(out var entityDef, out var _) && RankMgr.Get().IsCardLockedInCurrentLeague(entityDef))
		{
			RankMgr rankMgr = RankMgr.Get();
			LeagueDbfRecord leagueRecord = rankMgr.GetLeagueRecordForType(GameUtils.HasCompletedApprentice() ? League.LeagueType.NORMAL : League.LeagueType.NEW_PLAYER, rankMgr.GetLocalPlayerMedalInfo().GetCurrentSeasonId());
			if (!string.IsNullOrEmpty(leagueRecord.LockedCardPopupTitleText) && !string.IsNullOrEmpty(leagueRecord.LockedCardPopupBodyText))
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_headerText = leagueRecord.LockedCardPopupTitleText;
				info.m_text = leagueRecord.LockedCardPopupBodyText;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				info.m_layerToUse = GameLayer.UI;
				info.m_showAlertIcon = false;
				DialogManager.Get().ShowPopup(info);
			}
		}
	}

	private void LoadRandomCardBack()
	{
		CardBackManager.Get().LoadRandomCardBackIntoFavoriteSlot(updateScene: true);
	}

	private Vector3 GetCardCountPosition()
	{
		if (m_currentBigActor.GetPremium() != TAG_PREMIUM.SIGNATURE)
		{
			if (m_currentBigActor.GetPremium() == TAG_PREMIUM.DIAMOND && m_diamondCardCounterBone != null)
			{
				return m_diamondCardCounterBone.position;
			}
			return m_cardCounterBone.position;
		}
		EntityDef currentEntityDef = m_currentBigActor.GetEntityDef();
		if (currentEntityDef == null)
		{
			Debug.LogError("Unexpected error in GetCardCountPosition. Current big actor's entity def was null");
			return m_cardCounterBone.position;
		}
		if (ActorNames.GetSignatureFrameId(currentEntityDef.GetCardId()) == 1 && m_signature25CardCounterBone != null)
		{
			return m_signature25CardCounterBone.position;
		}
		return m_cardCounterBone.position;
	}

	public void ShowRelatedCards(bool open)
	{
		if (m_relatedCardsTray != null)
		{
			if (open)
			{
				m_relatedCardsTray.ShowTray();
			}
			else
			{
				m_relatedCardsTray.HideTray();
			}
		}
	}

	public void AddRelatedCard(string cardID, TAG_PREMIUM premium, bool offsetCardNameForRunes)
	{
		if (m_relatedCardsTray != null)
		{
			m_relatedCardsTray.AddCard(cardID, premium, offsetCardNameForRunes);
		}
	}
}
