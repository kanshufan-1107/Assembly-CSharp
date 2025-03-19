using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.Telemetry.WTCG.Client;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditClass]
public class CollectionCardVisual : PegUIElement
{
	public enum LockType
	{
		NONE,
		MAX_COPIES_IN_DECK,
		NO_MORE_INSTANCES,
		NOT_PLAYABLE,
		BANNED
	}

	public CollectionCardCount m_count;

	public CollectionCardLock m_cardLock;

	public GameObject m_newCardCallout;

	public GameObject m_favoriteBanner;

	public Vector3 m_boxColliderCenter = new Vector3(0f, 0.14f, 0f);

	public Vector3 m_boxColliderSize = new Vector3(2f, 0.21f, 2.7f);

	public Vector3 m_heroSkinBoxColliderCenter = new Vector3(0f, 0.14f, -0.58f);

	public Vector3 m_heroSkinBoxColliderSize = new Vector3(2f, 0.21f, 2f);

	[CustomEditField(Sections = "Diamond")]
	public Vector3_MobileOverride m_diamondScale;

	[CustomEditField(Sections = "Diamond")]
	public Vector3_MobileOverride m_diamondPositionOffset;

	[CustomEditField(Sections = "Signature")]
	public Vector3_MobileOverride m_signatureScale;

	[CustomEditField(Sections = "Signature")]
	public Vector3_MobileOverride m_signaturePositionOffset;

	private const string ADD_CARD_TO_DECK_SOUND = "collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef";

	private const string CARD_LIMIT_UNLOCK_SOUND = "card_limit_unlock.prefab:83ffc974654bdd84f84ecbbaf7ba8e5e";

	private const string CARD_LIMIT_LOCK_SOUND = "card_limit_lock.prefab:68e3525ae3fa8634ab19fde893d7e15b";

	private const string CARD_MOUSE_OVER_SOUND = "collection_manager_card_mouse_over.prefab:0d4e20bc78956bc48b5e2963ec39211c";

	private const string CARD_MOVE_INVALID_OR_CLICK_SOUND = "collection_manager_card_move_invalid_or_click.prefab:777caa6f44f027747a03f3d85bcc897c";

	private CollectionCardActors m_actors;

	private LockType m_lockType;

	private bool m_shown;

	private CollectionUtils.ViewMode m_visualType;

	private int m_cmRow;

	private bool m_lastClickLeft;

	private Transform m_clickedActorTransform;

	private Vector3 m_originalScale;

	private Vector3 m_currentPositionOffset = Vector3.zero;

	private bool m_isScaled;

	private TAG_PREMIUM m_cardVisualPremium;

	private List<Renderer> m_cacheActorRenderers;

	public string CardId
	{
		get
		{
			if (m_actors == null)
			{
				return string.Empty;
			}
			Actor actor = m_actors.GetPreferredActor();
			if (actor == null)
			{
				return string.Empty;
			}
			EntityDef entityDef = actor.GetEntityDef();
			if (entityDef == null)
			{
				return string.Empty;
			}
			return entityDef.GetCardId();
		}
	}

	public TAG_PREMIUM Premium
	{
		get
		{
			if (m_actors == null)
			{
				return TAG_PREMIUM.NORMAL;
			}
			Actor actor = m_actors.GetPreferredActor();
			if (actor == null)
			{
				return TAG_PREMIUM.NORMAL;
			}
			return actor.GetPremium();
		}
	}

	public static event Action<CollectionCardVisual> CollectionCardOver;

	public static event Action<CollectionCardVisual> CollectionCardOut;

	public static event Action<CollectionCardVisual> CollectionCardReleased;

	public static event Action<CollectionCardVisual> CollectionCardEnteredCraftingMode;

	public Vector3 GetRuneBannerPosition()
	{
		Actor actor = GetActor();
		if (!actor)
		{
			return base.transform.position;
		}
		CardRuneBanner runeBanner = actor.GetRuneBanner();
		if (!(runeBanner == null))
		{
			return runeBanner.transform.position;
		}
		return Vector3.zero;
	}

	protected override void Awake()
	{
		base.Awake();
		if (base.gameObject.GetComponent<AudioSource>() == null)
		{
			base.gameObject.AddComponent<AudioSource>();
		}
		SetDragTolerance(5f);
		SoundManager.Get().Load("collection_manager_card_add_to_deck_instant.prefab:06df359c4026d7e47b06a4174f33e3ef");
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public void ShowLock(LockType type)
	{
		ShowLock(type, null, playSound: false);
	}

	public void ShowLock(LockType lockType, string reason, bool playSound)
	{
		LockType prevLockType = m_lockType;
		m_lockType = lockType;
		UpdateCardCountVisibility();
		if (m_actors == null)
		{
			return;
		}
		Actor actor = m_actors.GetPreferredActor();
		if (m_cardLock != null)
		{
			m_cardLock.UpdateLockVisual(actor, lockType, reason);
		}
		if (playSound)
		{
			if (m_lockType == LockType.NONE && prevLockType != 0)
			{
				SoundManager.Get().LoadAndPlay("card_limit_unlock.prefab:83ffc974654bdd84f84ecbbaf7ba8e5e");
			}
			if (m_lockType != 0 && prevLockType == LockType.NONE)
			{
				SoundManager.Get().LoadAndPlay("card_limit_lock.prefab:68e3525ae3fa8634ab19fde893d7e15b");
			}
		}
	}

	public void OnDoneCrafting()
	{
		UpdateCardCount();
	}

	private void HidePreferredActorIfNecessary(CollectionCardActors actors)
	{
		if (actors == null)
		{
			return;
		}
		Actor actor = actors.GetPreferredActor();
		if (actor != null && actor.transform.parent == base.transform)
		{
			if (actor.GetEntityDef() != null)
			{
				actor.ReleaseCardDef();
			}
			actor.Hide();
		}
	}

	public void SetActors(CollectionCardActors actors, CollectionUtils.ViewMode type = CollectionUtils.ViewMode.CARDS)
	{
		HidePreferredActorIfNecessary(m_actors);
		m_actors = actors;
		UpdateCardCount();
		m_visualType = type;
		if (actors != null)
		{
			Actor actor = m_actors.GetPreferredActor();
			HidePreferredActorIfNecessary(m_actors);
			if (!(actor == null))
			{
				GameUtils.SetParent(actor, this);
				ActorStateType currentStateType = actor.GetActorStateMgr().GetActiveStateType();
				ShowNewCardCallout(currentStateType == ActorStateType.CARD_RECENTLY_ACQUIRED);
			}
		}
	}

	public Actor GetActor()
	{
		if (m_actors == null)
		{
			return null;
		}
		return m_actors.GetPreferredActor();
	}

	public CollectionCardActors GetCollectionCardActors()
	{
		return m_actors;
	}

	public CollectionUtils.ViewMode GetVisualType()
	{
		return m_visualType;
	}

	public void SetCMRow(int rowNum)
	{
		m_cmRow = rowNum;
	}

	public int GetCMRow()
	{
		return m_cmRow;
	}

	public static void ShowActorShadow(Actor actor, bool show)
	{
		string shadowTag = "FakeShadow";
		string uniqueShadowTag = "FakeShadowUnique";
		GameObject shadowObject = GameObjectUtils.FindChildByTag(actor.gameObject, shadowTag);
		GameObject uniqueShadowObject = GameObjectUtils.FindChildByTag(actor.gameObject, uniqueShadowTag);
		EntityDef entityDef = actor.GetEntityDef();
		if (show)
		{
			if (entityDef != null && entityDef.IsElite())
			{
				if (shadowObject != null)
				{
					shadowObject.GetComponent<Renderer>().enabled = false;
				}
				if (uniqueShadowObject != null)
				{
					uniqueShadowObject.GetComponent<Renderer>().enabled = true;
				}
			}
			else
			{
				if (shadowObject != null)
				{
					shadowObject.GetComponent<Renderer>().enabled = true;
				}
				if (uniqueShadowObject != null)
				{
					uniqueShadowObject.GetComponent<Renderer>().enabled = false;
				}
			}
		}
		else
		{
			if (shadowObject != null)
			{
				shadowObject.GetComponent<Renderer>().enabled = false;
			}
			if (uniqueShadowObject != null)
			{
				uniqueShadowObject.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void Show()
	{
		m_shown = true;
		SetEnabled(enabled: true);
		GetComponent<Collider>().enabled = true;
		if (m_actors == null)
		{
			return;
		}
		Actor actor = m_actors.GetPreferredActor();
		if (actor == null || actor.GetEntityDef() == null)
		{
			return;
		}
		bool showNewItemGlow = ShouldShowNewItemGlow(actor);
		ShowNewCardCallout(showNewItemGlow);
		actor.Show();
		ActorStateType actorState = ((!showNewItemGlow) ? ActorStateType.CARD_IDLE : ActorStateType.CARD_RECENTLY_ACQUIRED);
		actor.SetActorState(actorState);
		if (m_cacheActorRenderers == null)
		{
			m_cacheActorRenderers = new List<Renderer>();
		}
		actor.gameObject.GetComponentsInChildren(m_cacheActorRenderers);
		foreach (Renderer cacheActorRenderer in m_cacheActorRenderers)
		{
			cacheActorRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
		EntityDef entityDef = actor.GetEntityDef();
		bool inCollection = CollectionManager.Get().IsCardInCollection(entityDef.GetCardId(), actor.GetPremium()) || IsInCollection(actor.GetPremium());
		ShowActorShadow(actor, inCollection);
		UberText text = GetComponentInChildren<UberText>();
		if (text != null)
		{
			text.Show();
		}
	}

	protected virtual bool ShouldShowNewItemGlow(Actor actor)
	{
		if (m_visualType == CollectionUtils.ViewMode.CARDS)
		{
			string cardID = actor.GetEntityDef().GetCardId();
			TAG_PREMIUM premium = actor.GetPremium();
			return CollectionManager.Get().GetCollectibleDisplay().ShouldShowNewCardGlow(cardID, premium);
		}
		return false;
	}

	public void Hide()
	{
		m_shown = false;
		SetEnabled(enabled: false);
		GetComponent<Collider>().enabled = false;
		ShowLock(LockType.NONE);
		ShowNewCardCallout(show: false);
		if (m_count != null)
		{
			m_count.Hide();
		}
		if (m_actors != null)
		{
			Actor preferredActor = m_actors.GetPreferredActor();
			if (preferredActor != null)
			{
				preferredActor.Hide();
			}
			UberText text = GetComponentInChildren<UberText>();
			if (text != null)
			{
				text.Hide();
			}
			PegUI.Get().RemoveAsMouseDownElement(this);
		}
	}

	public void UpdateSpecialCaseTransform()
	{
		if (m_actors == null)
		{
			return;
		}
		Actor actor = m_actors.GetPreferredActor();
		if (actor == null)
		{
			return;
		}
		TAG_PREMIUM premium = actor.GetPremium();
		if (premium == m_cardVisualPremium)
		{
			return;
		}
		if (m_isScaled)
		{
			SetOriginalCardTransform();
		}
		if (m_visualType == CollectionUtils.ViewMode.CARDS)
		{
			switch (premium)
			{
			case TAG_PREMIUM.DIAMOND:
				SetDiamondCardTransform();
				break;
			case TAG_PREMIUM.SIGNATURE:
				SetSignatureCardTransform();
				break;
			}
		}
		m_cardVisualPremium = premium;
	}

	public void SetHeroSkinBoxCollider()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		component.center = m_heroSkinBoxColliderCenter;
		component.size = m_heroSkinBoxColliderSize;
	}

	public void SetDefaultBoxCollider()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		component.center = m_boxColliderCenter;
		component.size = m_boxColliderSize;
	}

	private void SetDiamondCardTransform()
	{
		m_originalScale = base.gameObject.transform.localScale;
		base.gameObject.transform.localScale = m_diamondScale;
		base.gameObject.transform.localPosition -= (Vector3)m_diamondPositionOffset;
		m_currentPositionOffset = m_diamondPositionOffset;
		m_isScaled = true;
	}

	private void SetSignatureCardTransform()
	{
		m_originalScale = base.gameObject.transform.localScale;
		base.gameObject.transform.localScale = m_signatureScale;
		base.gameObject.transform.localPosition -= (Vector3)m_signaturePositionOffset;
		m_currentPositionOffset = m_signaturePositionOffset;
		m_isScaled = true;
	}

	private void SetOriginalCardTransform()
	{
		base.gameObject.transform.localPosition += m_currentPositionOffset;
		base.gameObject.transform.localScale = m_originalScale;
		m_currentPositionOffset = Vector3.zero;
		m_isScaled = false;
	}

	private bool CheckCardSeen()
	{
		if (m_actors == null)
		{
			return false;
		}
		bool num = m_actors.GetPreferredActor().GetActorStateMgr().GetActiveStateType() == ActorStateType.CARD_RECENTLY_ACQUIRED;
		if (num)
		{
			MarkAsSeen();
		}
		return num;
	}

	public virtual void MarkAsSeen()
	{
		string cardId = CardId;
		if (!string.IsNullOrEmpty(cardId))
		{
			CollectionManager.Get().MarkAllInstancesAsSeen(cardId, Premium);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		if (ShouldIgnoreAllInput() || m_actors == null)
		{
			return;
		}
		Actor actor = m_actors.GetPreferredActor();
		EntityDef entityDef = actor.GetEntityDef();
		if (entityDef != null)
		{
			TooltipPanelManager.Orientation orientation = ((m_cmRow > 0) ? TooltipPanelManager.Orientation.RightBottom : TooltipPanelManager.Orientation.RightTop);
			TooltipPanelManager.Get().UpdateKeywordHelpForCollectionManager(entityDef, actor, orientation);
		}
		SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over.prefab:0d4e20bc78956bc48b5e2963ec39211c", base.gameObject);
		if (IsInCollection(actor.GetPremium()))
		{
			ActorStateType overStateType = ActorStateType.CARD_MOUSE_OVER;
			if (CheckCardSeen())
			{
				overStateType = ActorStateType.CARD_RECENTLY_ACQUIRED_MOUSE_OVER;
			}
			actor.SetActorState(overStateType);
			if (m_visualType == CollectionUtils.ViewMode.CARDS)
			{
				CollectionCardVisual.CollectionCardOver?.Invoke(this);
			}
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		TooltipPanelManager.Get().HideKeywordHelp();
		if (ShouldIgnoreAllInput() || m_actors == null)
		{
			return;
		}
		Actor actor = m_actors.GetPreferredActor();
		if (IsInCollection(actor.GetPremium()))
		{
			CheckCardSeen();
			actor.SetActorState(ActorStateType.CARD_IDLE);
			ShowNewCardCallout(show: false);
			if (m_visualType == CollectionUtils.ViewMode.CARDS)
			{
				CollectionCardVisual.CollectionCardOut?.Invoke(this);
			}
		}
	}

	protected override void OnDrag()
	{
		if (CanPickUpCard())
		{
			CollectionInputMgr.Get().GrabCardVisual(this);
		}
	}

	protected override void OnRelease()
	{
		if (IsTransactionPendingOnThisCard() || CollectionInputMgr.Get().HasHeldCard())
		{
			return;
		}
		if (m_visualType == CollectionUtils.ViewMode.CARDS)
		{
			CollectionCardVisual.CollectionCardReleased?.Invoke(this);
		}
		Actor actor = m_actors.GetPreferredActor();
		if (UniversalInputManager.Get().IsTouchMode() || (CraftingTray.Get() != null && CraftingTray.Get().IsShown()))
		{
			CheckCardSeen();
			ShowNewCardCallout(show: false);
			actor.SetActorState(ActorStateType.CARD_IDLE);
			m_clickedActorTransform = actor.transform;
			EnterCraftingMode();
			return;
		}
		Spell spell = actor.GetSpell(SpellType.DEATHREVERSE);
		if (spell != null)
		{
			ParticleSystem.MainModule mainModule = spell.gameObject.GetComponentInChildren<ParticleSystem>().main;
			mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
		}
		if (m_visualType == CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS || m_visualType == CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS)
		{
			EnterCraftingMode();
		}
		else if (!CanPickUpCard())
		{
			m_lastClickLeft = true;
			SoundManager.Get().LoadAndPlay("collection_manager_card_move_invalid_or_click.prefab:777caa6f44f027747a03f3d85bcc897c");
			if (spell != null)
			{
				spell.ActivateState(SpellStateType.BIRTH);
			}
			CollectionManager.Get().GetCollectibleDisplay().ShowInnkeeperLClickHelp(actor.GetEntityDef());
			if (m_visualType != 0)
			{
				return;
			}
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			if (deck == null)
			{
				return;
			}
			EntityDef entityDef = actor.GetEntityDef();
			if (entityDef != null)
			{
				RunePattern runePattern = new RunePattern(entityDef);
				if (runePattern.HasRunes && !deck.CanAddRunes(runePattern, DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
				{
					GameplayErrorManager.Get().DisplayMessage(GameStrings.Get("GLUE_COLLECTION_INCOMPATIBLE_RUNES_HEADER"));
					TutorialDeathKnightDeckBuilding.ShowTutorial(UIVoiceLinesManager.TriggerType.CANNOT_ADD_RUNES);
				}
			}
		}
		else if (m_visualType == CollectionUtils.ViewMode.CARDS)
		{
			EntityDef entityDef2 = actor.GetEntityDef();
			if (entityDef2 != null)
			{
				if (spell != null)
				{
					spell.ActivateState(SpellStateType.BIRTH);
				}
				if (CollectionDeckTray.Get().AddCard(entityDef2, actor.GetPremium(), false, actor, DeckRule.RuleType.DEATHKNIGHT_RUNE_LIMIT))
				{
					CollectionDeckTray.Get().OnCardManuallyAddedByUser_CheckSuggestions(entityDef2);
				}
				else
				{
					SoundManager.Get().LoadAndPlay("collection_manager_card_move_invalid_or_click.prefab:777caa6f44f027747a03f3d85bcc897c");
				}
			}
		}
		else if (m_visualType == CollectionUtils.ViewMode.COINS)
		{
			CollectionDeckTray.Get().AnimateInCosmeticCoin(actor);
		}
		else if (m_visualType == CollectionUtils.ViewMode.CARD_BACKS)
		{
			CollectionDeckTray.Get().AnimateInCardBack(actor);
		}
		else if (m_visualType == CollectionUtils.ViewMode.HERO_SKINS)
		{
			CollectionDeckTray.Get().SetHeroSkin(actor);
		}
	}

	protected override void OnRightClick()
	{
		if (!IsTransactionPendingOnThisCard())
		{
			if (!Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false))
			{
				Options.Get().SetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, val: true);
			}
			Actor actor = m_actors.GetPreferredActor();
			if (m_lastClickLeft)
			{
				m_lastClickLeft = false;
				SendLeftRightClickTelemetry(actor);
			}
			ShowNewCardCallout(show: false);
			actor.SetActorState(ActorStateType.CARD_IDLE);
			m_clickedActorTransform = actor.transform;
			EnterCraftingMode();
		}
	}

	private void EnterCraftingMode()
	{
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		if (m_visualType != viewMode)
		{
			return;
		}
		switch (viewMode)
		{
		case CollectionUtils.ViewMode.CARDS:
			if (CraftingManager.Get() != null)
			{
				CraftingManager.Get().EnterCraftMode(GetActor());
			}
			break;
		case CollectionUtils.ViewMode.HERO_SKINS:
			HeroSkinInfoManager.EnterPreviewWhenReady(this);
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			BaconHeroSkinInfoManager.EnterPreviewWhenReady(this);
			break;
		case CollectionUtils.ViewMode.CARD_BACKS:
			CardBackInfoManager.EnterPreviewWhenReady(this);
			break;
		case CollectionUtils.ViewMode.COINS:
			CosmeticCoinManager.Get()?.ShowCoinPreview(CardId, m_clickedActorTransform);
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			BaconGuideSkinInfoManager.EnterPreviewWhenReady(this);
			break;
		}
		CollectionDeckTray.Get()?.CancelRenamingDeck();
		CollectionCardVisual.CollectionCardEnteredCraftingMode?.Invoke(this);
	}

	private bool IsTransactionPendingOnThisCard()
	{
		if (m_actors == null)
		{
			return false;
		}
		Actor actor = m_actors.GetPreferredActor();
		CraftingManager craftingManager = CraftingManager.Get();
		if (craftingManager == null)
		{
			return false;
		}
		CraftingPendingTransaction transaction = craftingManager.GetPendingServerTransaction();
		if (transaction == null)
		{
			return false;
		}
		EntityDef entityDef = actor.GetEntityDef();
		if (entityDef == null)
		{
			return false;
		}
		if (transaction.CardID != entityDef.GetCardId())
		{
			return false;
		}
		if (transaction.Premium != actor.GetPremium())
		{
			return false;
		}
		return true;
	}

	private bool ShouldIgnoreAllInput()
	{
		if (!m_shown)
		{
			return true;
		}
		if (CollectionInputMgr.Get() != null && CollectionInputMgr.Get().IsDraggingScrollbar())
		{
			return true;
		}
		if (CraftingManager.Get() != null && CraftingManager.Get().IsCardShowing())
		{
			return true;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.ArePagesTurning())
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsInCollection(TAG_PREMIUM premium)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.COINS)
		{
			return CosmeticCoinManager.Get().IsOwnedCoinCard(CardId);
		}
		if (m_actors != null)
		{
			CollectionCardBack ccb = m_actors.GetPreferredActor().GetComponent<CollectionCardBack>();
			if (ccb != null && CardBackManager.Get().IsCardBackOwned(ccb.GetCardBackId()))
			{
				return true;
			}
		}
		int count = 0;
		if (m_count != null)
		{
			count = m_count.GetCount(premium);
		}
		return count > 0;
	}

	private bool IsUnlocked()
	{
		Actor cardActor = m_actors.GetPreferredActor();
		if (RankMgr.Get().IsCardLockedInCurrentLeague(cardActor.GetEntityDef()))
		{
			return false;
		}
		return m_lockType == LockType.NONE;
	}

	private bool CanPickUpCard()
	{
		if (ShouldIgnoreAllInput())
		{
			return false;
		}
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() != m_visualType)
		{
			return false;
		}
		if (CollectionDeckTray.Get() == null)
		{
			return false;
		}
		if (!CollectionDeckTray.Get().CanPickupCard())
		{
			return false;
		}
		switch (m_visualType)
		{
		case CollectionUtils.ViewMode.CARDS:
		{
			if (m_actors == null)
			{
				return false;
			}
			Actor actor = m_actors.GetPreferredActor();
			if ((bool)actor && GameUtils.IsCardCollectible(actor.GetEntityDef().GetCardId()) && !IsInCollection(actor.GetPremium()))
			{
				return false;
			}
			if (actor.GetEntityDef().HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
			{
				return true;
			}
			if (!IsUnlocked())
			{
				return false;
			}
			break;
		}
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			return false;
		case CollectionUtils.ViewMode.HERO_SKINS:
			if (HeroSkinInfoManager.IsLoadedAndShowingPreview())
			{
				return false;
			}
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			return false;
		case CollectionUtils.ViewMode.CARD_BACKS:
			if (CardBackInfoManager.IsLoadedAndShowingPreview())
			{
				return false;
			}
			break;
		}
		return true;
	}

	public void ShowNewCardCallout(bool show)
	{
		if (!(m_newCardCallout == null))
		{
			m_newCardCallout.SetActive(show);
		}
	}

	private void UpdateCardCount()
	{
		int normalCount = 0;
		int goldenCount = 0;
		int signatureCount = 0;
		int diamondCount = 0;
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		if (m_actors != null)
		{
			Actor actor = m_actors.GetPreferredActor();
			EntityDef entityDef = actor.GetEntityDef();
			if (entityDef != null)
			{
				premium = actor.GetPremium();
				CollectibleCard normalCard = CollectionManager.Get().GetCard(entityDef.GetCardId(), TAG_PREMIUM.NORMAL);
				if (normalCard != null)
				{
					normalCount = normalCard.OwnedCount;
				}
				CollectibleCard goldenCard = CollectionManager.Get().GetCard(entityDef.GetCardId(), TAG_PREMIUM.GOLDEN);
				if (goldenCard != null)
				{
					goldenCount = goldenCard.OwnedCount;
				}
				CollectibleCard signatureCard = CollectionManager.Get().GetCard(entityDef.GetCardId(), TAG_PREMIUM.SIGNATURE);
				if (signatureCard != null)
				{
					signatureCount = signatureCard.OwnedCount;
				}
				CollectibleCard diamondCard = CollectionManager.Get().GetCard(entityDef.GetCardId(), TAG_PREMIUM.DIAMOND);
				if (diamondCard != null)
				{
					diamondCount = diamondCard.OwnedCount;
				}
			}
		}
		if (m_count != null)
		{
			int signatureFrameId = 0;
			if (m_actors != null)
			{
				Actor actor2 = m_actors.GetPreferredActor();
				if (actor2 != null && actor2.GetPremium() == TAG_PREMIUM.SIGNATURE)
				{
					signatureFrameId = ActorNames.GetSignatureFrameId(actor2.GetEntityDef().GetCardId());
				}
			}
			m_count.SetData(normalCount, goldenCount, signatureCount, diamondCount, premium, signatureFrameId);
		}
		UpdateCardCountVisibility();
	}

	private void UpdateCardCountVisibility()
	{
		if (m_count != null)
		{
			if ((m_lockType == LockType.NONE || m_lockType == LockType.BANNED) && (m_visualType == CollectionUtils.ViewMode.CARDS || m_visualType == CollectionUtils.ViewMode.COINS))
			{
				m_count.Show();
			}
			else
			{
				m_count.Hide();
			}
		}
	}

	private void SendLeftRightClickTelemetry(Actor actor)
	{
		CollectionLeftRightClick.Target target = CollectionLeftRightClick.Target.CARD;
		EntityDef entityDef = actor.GetEntityDef();
		if (entityDef == null)
		{
			target = CollectionLeftRightClick.Target.CARD_BACK;
		}
		else if (entityDef.IsHeroSkin())
		{
			target = CollectionLeftRightClick.Target.HERO_SKIN;
		}
		TelemetryManager.Client().SendCollectionLeftRightClick(target);
	}

	protected override void OnDestroy()
	{
		m_cacheActorRenderers = null;
	}

	public void SetRuneBannerHighlighted(bool highlight)
	{
		CardRuneBanner runeBanner = GetActor().GetRuneBanner();
		if ((bool)runeBanner)
		{
			runeBanner.SetHighlighted(highlight);
		}
	}

	public void SetFavoriteBannerActive(bool isActive)
	{
		if (m_favoriteBanner != null)
		{
			m_favoriteBanner.SetActive(isActive);
		}
	}
}
