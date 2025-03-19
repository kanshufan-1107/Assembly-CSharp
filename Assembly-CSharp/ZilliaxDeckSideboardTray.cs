using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

public class ZilliaxDeckSideboardTray : DeckSideboardTray
{
	private enum FTUEStep
	{
		INVALID,
		MODULE,
		PREVIEW,
		PORTRAIT,
		SAVED,
		COMPLETE
	}

	[SerializeField]
	private GameObject m_zilliaxPortraitDropArea;

	[SerializeField]
	private WidgetInstance m_modulesTooltip;

	[SerializeField]
	private WidgetInstance m_previewTooltip;

	[SerializeField]
	private WidgetInstance m_portraitTooltip;

	[SerializeField]
	private WidgetInstance m_savedTooltip;

	[SerializeField]
	private Clickable m_previewButton;

	[SerializeField]
	private List<Actor> m_ZilliaxInPlayPreviewActors;

	[SerializeField]
	private List<Actor> m_NormalZilliaxPreviewActors;

	[SerializeField]
	private List<Actor> m_GoldenZilliaxPreviewActors;

	[SerializeField]
	private Clickable m_portraitClickable;

	private const string LOADED_SAVED_VERSION_VC_EVENT = "SET_VERSION";

	[SerializeField]
	private GameObject m_savedVersionLoadedVC;

	private const string NEW_PORTRAIT_VC_EVENT = "CHANGE_PORTRAIT";

	[SerializeField]
	private GameObject m_newCosmeticModuleVC;

	private ZilliaxSideboardDeck m_zilliaxSideboardDeck;

	private bool m_isDragging;

	private const string DRAG_SOURCE_COLLECTION = "collection";

	private const string DRAG_SOURCE_DECK_TRAY = "decktray";

	private const string DRAG_SOURCE_PORTRAIT = "portrait";

	private string m_currentDragSource = "";

	private bool m_hasConfirmedIncompleteExit;

	private HashSet<int> m_startingModules = new HashSet<int>(3);

	private bool m_hasSeenFTUE;

	private bool m_isFTUEInitialized;

	private Dictionary<FTUEStep, WidgetInstance> m_FTUETooltips = new Dictionary<FTUEStep, WidgetInstance>();

	private FTUEStep m_currentStep;

	private FTUEStep m_stepQueuedAfterDrag;

	private List<CollectibleCard> m_savedZilliaxVersions = new List<CollectibleCard>();

	private GetCustomizedCardHistoryResponse m_savedZilliaxResponse;

	private CardTextBuilder m_zilliaxCardTextBuilder = CardTextBuilderFactory.Create(Assets.Card.CardTextBuilderType.ZILLIAX_DELUXE_3000);

	private const string CLOSE_CARD_PREVIEW_EVENT = "CARD_PREVIEW_OFF";

	[SerializeField]
	private VisualController m_cardPreviewVC;

	private Actor m_ghostMinionActor;

	private Actor m_ghostGoldenMinionActor;

	private Actor m_constructionAnimationActor;

	private bool m_isAnimating;

	private bool m_isCancellingAnimation;

	private Coroutine m_constructionAnimationCoroutine;

	private ScreenEffectsHandle m_screenEffectsHandle;

	[SerializeField]
	private Transform m_floatingCardBone;

	[SerializeField]
	private BoxCollider m_offClickCatcher;

	public List<CollectibleCard> SavedZilliaxVersions => m_savedZilliaxVersions;

	private event Action OnInteractableElementClicked;

	private event Action OnPreviewButtonHovered;

	public void Awake()
	{
		Network network = Network.Get();
		network.RegisterNetHandler(GetCustomizedCardHistoryResponse.PacketID.ID, OnSavedZilliaxResponse);
		network.GetCustomizedCardHistoryRequest((uint)CollectiblePageDisplay.GetMaxCardsPerPage());
		foreach (Actor zilliaxInPlayPreviewActor in m_ZilliaxInPlayPreviewActors)
		{
			UberText[] componentsInChildren = zilliaxInPlayPreviewActor.GetComponentsInChildren<UberText>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].AmbientLightBlend = 0f;
			}
		}
		if (m_zilliaxPortraitDropArea != null)
		{
			m_zilliaxPortraitDropArea.SetActive(base.IsShowing);
		}
		if (m_ghostMinionActor == null)
		{
			LoadActor("Card_Hand_Ally.prefab:d00eb0f79080e0749993fe4619e9143d", ref m_ghostMinionActor);
		}
		if (m_ghostGoldenMinionActor == null)
		{
			LoadActor(ActorNames.GetHandActor(TAG_CARDTYPE.MINION, TAG_PREMIUM.GOLDEN), ref m_ghostGoldenMinionActor);
		}
		SetIsOffClickCatcherEnabled(isEnabled: false);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public void OnDestroy()
	{
		Network.Get()?.RemoveNetHandler(GetCustomizedCardHistoryResponse.PacketID.ID, OnSavedZilliaxResponse);
	}

	public override void Show(SideboardDeck sideboardDeck)
	{
		if (base.IsShowing)
		{
			return;
		}
		base.Show(sideboardDeck);
		m_zilliaxSideboardDeck = sideboardDeck as ZilliaxSideboardDeck;
		UpdatePreviewActors(m_zilliaxSideboardDeck);
		if (m_zilliaxSideboardDeck != null)
		{
			m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated += UpdatePreviewActors;
		}
		if (m_savedZilliaxVersions != null)
		{
			BuildSavedZilliaxVersions();
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ZILLIAX_CUSTOMIZABLE_FTUE, out long hasSeenZilliaxTutorial);
		m_hasSeenFTUE = hasSeenZilliaxTutorial > 0;
		CollectionDeck mainDeck = m_zilliaxSideboardDeck.MainDeck;
		if (mainDeck.CreatedFromShareableDeck == null && !mainDeck.IsCreatedWithDeckComplete)
		{
			if (!m_hasSeenFTUE)
			{
				if (!m_isFTUEInitialized)
				{
					SetupFTUEStep(FTUEStep.MODULE, m_modulesTooltip);
					SetupFTUEStep(FTUEStep.PREVIEW, m_previewTooltip);
					SetupFTUEStep(FTUEStep.PORTRAIT, m_portraitTooltip);
					SetupFTUEStep(FTUEStep.SAVED, m_savedTooltip);
					m_isFTUEInitialized = true;
				}
				if (m_previewButton != null)
				{
					m_previewButton.AddEventListener(UIEventType.PRESS, OnButtonPressed);
					m_previewButton.AddEventListener(UIEventType.ROLLOVER, OnPreviewButtonOver);
				}
				SetCurrentFTUEStep(FTUEStep.MODULE);
			}
			else
			{
				GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ZILLIAX_CUSTOMIZABLE_SAVED_VERSIONS_FTUE, out long hasSeenSavedZilliaxVersionsTutorial);
				if (hasSeenSavedZilliaxVersionsTutorial > 0)
				{
					SetCurrentFTUEStep(FTUEStep.COMPLETE);
				}
				else
				{
					SetupFTUEStep(FTUEStep.SAVED, m_savedTooltip);
					m_isFTUEInitialized = true;
					SetCurrentFTUEStep(FTUEStep.SAVED);
				}
			}
		}
		else
		{
			SetCurrentFTUEStep(FTUEStep.COMPLETE);
		}
		m_zilliaxSideboardDeck.ZilliaxDataModel.IsZilliaxAlreadyCrafted = m_zilliaxSideboardDeck.IsZilliaxComplete();
		StoreOriginalZilliaxModules();
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleAdded += OnZilliaxModuleChangedCallback;
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleRemoved += OnZilliaxModuleChangedCallback;
		if (m_zilliaxPortraitDropArea != null)
		{
			m_zilliaxPortraitDropArea.SetActive(value: true);
		}
		if (m_portraitClickable != null)
		{
			m_portraitClickable.AddEventListener(UIEventType.RELEASE, HandlePortraitPress);
		}
		m_hasConfirmedIncompleteExit = false;
		CollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		pageManager.OnZilliaxTabPressed += OnZilliaxTabPressed;
		if (pageManager.m_classFilterHeader != null)
		{
			pageManager.m_classFilterHeader.OnPressed += OnZilliaxTabPressed;
		}
		CollectionCardVisual.CollectionCardEnteredCraftingMode += OnZilliaxCardInspected;
		m_zilliaxSideboardDeck.OnSavedZilliaxVersionLoaded += OnSavedZilliaxVersionLoaded;
	}

	private void StoreOriginalZilliaxModules()
	{
		ClearOriginalZilliaxModules();
		if (m_zilliaxSideboardDeck == null)
		{
			return;
		}
		foreach (int cardId in m_zilliaxSideboardDeck.GetCards())
		{
			m_startingModules.Add(cardId);
		}
		m_zilliaxSideboardDeck.ZilliaxDataModel.DoesZilliaxMatchStart = true;
	}

	private void ClearOriginalZilliaxModules()
	{
		m_startingModules.Clear();
	}

	private bool DoesCurrentZilliaxMatchStartingZilliax()
	{
		List<int> sideboardCards = m_zilliaxSideboardDeck.GetCards();
		return m_startingModules.SetEquals(sideboardCards);
	}

	private bool DoesCurrentZilliaxFunctionalModulesMatchStartingZilliax()
	{
		List<int> functionalModules = m_zilliaxSideboardDeck.GetFunctionalModules();
		return m_startingModules.IsSupersetOf(functionalModules);
	}

	private bool DoesCurrentZilliaxFunctionalModulesMatchAnySavedZilliax()
	{
		List<int> functionalModules = m_zilliaxSideboardDeck.GetFunctionalModules();
		HashSet<int> savedFunctionalModules = new HashSet<int>(2);
		foreach (CollectibleCard savedZilliaxVersion in m_savedZilliaxVersions)
		{
			savedFunctionalModules.Clear();
			EntityDef savedZilliaxEntityDef = savedZilliaxVersion.GetEntityDef();
			savedFunctionalModules.Add(savedZilliaxEntityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1));
			savedFunctionalModules.Add(savedZilliaxEntityDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2));
			if (savedFunctionalModules.IsSupersetOf(functionalModules))
			{
				return true;
			}
		}
		return false;
	}

	private void OnSavedZilliaxResponse()
	{
		m_savedZilliaxResponse = Network.Get().GetCustomizedCardHistoryResponse();
		if (base.IsShowing)
		{
			BuildSavedZilliaxVersions();
		}
	}

	private void BuildSavedZilliaxVersions()
	{
		if (m_savedZilliaxResponse == null)
		{
			return;
		}
		m_savedZilliaxVersions.Clear();
		EntityDef[] functionalEntityDefs = new EntityDef[2];
		DefLoader defLoader = DefLoader.Get();
		TAG_PREMIUM premiumToUse = m_zilliaxSideboardDeck.DataModel.Premium;
		foreach (CustomizedCard savedZilliaxCard in m_savedZilliaxResponse.Cards)
		{
			string cosmeticCardId = GameUtils.TranslateDbIdToCardId(savedZilliaxCard.PortraitCardId);
			CardDbfRecord dbfRecord = GameDbf.GetIndex().GetCardRecord(cosmeticCardId);
			EntityDef savedZilliaxEntityDef = defLoader.GetEntityDef(savedZilliaxCard.PortraitCardId).Clone();
			functionalEntityDefs[0] = defLoader.GetEntityDef(savedZilliaxCard.ModuleCardId1);
			functionalEntityDefs[1] = defLoader.GetEntityDef(savedZilliaxCard.ModuleCardId2);
			int cost = 0;
			int attack = 0;
			int health = 0;
			Map<GAME_TAG, int> tagMap = new Map<GAME_TAG, int>();
			EntityDef[] array = functionalEntityDefs;
			foreach (EntityDef functionalModuleEntityDef in array)
			{
				cost += functionalModuleEntityDef.GetCost();
				attack += functionalModuleEntityDef.GetATK();
				health += functionalModuleEntityDef.GetHealth();
				foreach (KeyValuePair<int, int> tag in functionalModuleEntityDef.GetTags().GetMap())
				{
					tagMap[(GAME_TAG)tag.Key] = tag.Value;
				}
			}
			tagMap[GAME_TAG.COST] = cost;
			tagMap[GAME_TAG.ATK] = attack;
			tagMap[GAME_TAG.HEALTH] = health;
			tagMap[GAME_TAG.HIDE_STATS] = 0;
			tagMap[GAME_TAG.HIDE_COST] = 0;
			tagMap[GAME_TAG.HIDE_ATTACK_NUMBER] = 0;
			tagMap[GAME_TAG.HIDE_HEALTH_NUMBER] = 0;
			tagMap[GAME_TAG.MODULAR_ENTITY_PART_1] = savedZilliaxCard.ModuleCardId1;
			tagMap[GAME_TAG.MODULAR_ENTITY_PART_2] = savedZilliaxCard.ModuleCardId2;
			tagMap[GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE] = 0;
			tagMap[GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE] = 0;
			tagMap[GAME_TAG.ZILLIAX_CUSTOMIZABLE_LINKED_COSMETICMOUDLE] = 0;
			tagMap[GAME_TAG.ZILLIAX_CUSTOMIZABLE_LINKED_FUNCTIONALMOUDLE] = 0;
			tagMap[GAME_TAG.ZILLIAX_CUSTOMIZABLE_SAVED_VERSION] = 1;
			tagMap[GAME_TAG.CLASS] = 12;
			savedZilliaxEntityDef.SetTags(tagMap);
			m_zilliaxCardTextBuilder.BuildCardTextInHand(savedZilliaxEntityDef);
			CollectibleCard collectibleCard = new CollectibleCard(dbfRecord, savedZilliaxEntityDef, premiumToUse);
			m_savedZilliaxVersions.Add(collectibleCard);
		}
	}

	private void OnButtonPressed(UIEvent e)
	{
		this.OnInteractableElementClicked?.Invoke();
	}

	private void OnPreviewButtonOver(UIEvent e)
	{
		this.OnPreviewButtonHovered?.Invoke();
	}

	private void OnZilliaxModuleAdded(ZilliaxSideboardDeck sideboardDeck, EntityDef entityDef)
	{
		HideCurrentTooltip(entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE));
		if (m_currentStep == FTUEStep.MODULE && entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
		{
			SetCurrentFTUEStep(FTUEStep.PREVIEW);
		}
		else if (m_currentStep == FTUEStep.PREVIEW && sideboardDeck.ZilliaxDataModel.FunctionalModuleCardCount >= 2)
		{
			SetCurrentFTUEStep(FTUEStep.PORTRAIT);
		}
	}

	private void OnZilliaxModuleRemoved(ZilliaxSideboardDeck sideboardDeck, EntityDef entityDef)
	{
		HideCurrentTooltip();
	}

	private void OnZilliaxModuleChangedCallback(ZilliaxSideboardDeck sideboardDeck, EntityDef entityDef)
	{
		sideboardDeck.ZilliaxDataModel.DoesZilliaxMatchStart = DoesCurrentZilliaxFunctionalModulesMatchStartingZilliax();
		sideboardDeck.ZilliaxDataModel.DoesZilliaxMatchASavedVersion = DoesCurrentZilliaxFunctionalModulesMatchAnySavedZilliax();
		if (!entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE) || !(m_newCosmeticModuleVC != null))
		{
			return;
		}
		VisualController[] components = m_savedVersionLoadedVC.GetComponents<VisualController>();
		foreach (VisualController vc in components)
		{
			if (vc.HasState("CHANGE_PORTRAIT"))
			{
				vc.SetState("CHANGE_PORTRAIT");
			}
		}
	}

	private void SetupFTUEStep(FTUEStep ftueStep, WidgetInstance ftuePopup)
	{
		if (!m_isFTUEInitialized && ftuePopup != null)
		{
			m_FTUETooltips.TryAdd(ftueStep, ftuePopup);
			ftuePopup.RegisterReadyListener(delegate
			{
				LayerUtils.SetLayer(ftuePopup, GameLayer.Tooltip);
			});
		}
	}

	public override void Hide()
	{
		if (m_zilliaxSideboardDeck != null)
		{
			m_zilliaxSideboardDeck.OnDynamicZilliaxDefUpdated -= UpdatePreviewActors;
		}
		if (m_zilliaxSideboardDeck != null && m_zilliaxSideboardDeck.ZilliaxDataModel != null)
		{
			m_zilliaxSideboardDeck.ZilliaxDataModel.IsDragging = false;
		}
		m_isDragging = false;
		m_zilliaxSideboardDeck.ZilliaxDataModel.DraggingSource = m_currentDragSource;
		if (m_zilliaxSideboardDeck.ZilliaxDataModel.FunctionalModuleCardCount == 2)
		{
			m_zilliaxSideboardDeck.FillDefaultCosmeticMoudleIfNeeded();
		}
		if (m_zilliaxSideboardDeck != null && m_zilliaxSideboardDeck.IsZilliaxComplete())
		{
			CustomizedCard customizedCard = new CustomizedCard();
			customizedCard.ModuleCardId1 = m_zilliaxSideboardDeck.DynamicZilliaxDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
			customizedCard.ModuleCardId2 = m_zilliaxSideboardDeck.DynamicZilliaxDef.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
			customizedCard.PortraitCardId = GameUtils.TranslateCardIdToDbId(m_zilliaxSideboardDeck.GetCosmeticModuleCollectionDeckSlot().CardID);
			Network.Get().UpdateCustomizedCard(customizedCard);
			bool isUsedAlready = false;
			if (m_savedZilliaxResponse != null && m_savedZilliaxResponse.Cards != null)
			{
				foreach (CustomizedCard card in m_savedZilliaxResponse.Cards)
				{
					if (card.PortraitCardId == customizedCard.PortraitCardId && ((card.ModuleCardId1 == customizedCard.ModuleCardId1 && card.ModuleCardId2 == customizedCard.ModuleCardId2) || (card.ModuleCardId1 == customizedCard.ModuleCardId2 && card.ModuleCardId2 == customizedCard.ModuleCardId1)))
					{
						isUsedAlready = true;
						break;
					}
				}
			}
			if (m_savedZilliaxResponse.Cards.Count == CollectiblePageDisplay.GetMaxCardsPerPage())
			{
				m_savedZilliaxResponse.Cards.RemoveAt(CollectiblePageDisplay.GetMaxCardsPerPage() - 1);
			}
			if (!isUsedAlready)
			{
				m_savedZilliaxResponse.Cards.Insert(0, customizedCard);
			}
		}
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleAdded -= OnZilliaxModuleChangedCallback;
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleRemoved -= OnZilliaxModuleChangedCallback;
		if (m_zilliaxPortraitDropArea != null)
		{
			m_zilliaxPortraitDropArea.SetActive(value: false);
		}
		if (m_portraitClickable != null)
		{
			m_portraitClickable.RemoveEventListener(UIEventType.RELEASE, HandlePortraitPress);
		}
		if (m_cardPreviewVC != null)
		{
			m_cardPreviewVC.SetState("CARD_PREVIEW_OFF");
		}
		CollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		pageManager.OnZilliaxTabPressed -= OnZilliaxTabPressed;
		if (pageManager.m_classFilterHeader != null)
		{
			pageManager.m_classFilterHeader.OnPressed -= OnZilliaxTabPressed;
		}
		CollectionCardVisual.CollectionCardEnteredCraftingMode -= OnZilliaxCardInspected;
		m_zilliaxSideboardDeck.OnSavedZilliaxVersionLoaded -= OnSavedZilliaxVersionLoaded;
		base.Hide();
	}

	public void Update()
	{
		if (!base.IsShowing || m_zilliaxSideboardDeck == null || m_zilliaxSideboardDeck.ZilliaxDataModel == null)
		{
			return;
		}
		m_zilliaxSideboardDeck.ZilliaxDataModel.IsDragging = m_isDragging;
		if (!m_isDragging && !string.IsNullOrEmpty(m_currentDragSource))
		{
			m_currentDragSource = "";
			m_zilliaxSideboardDeck.ZilliaxDataModel.DraggingSource = m_currentDragSource;
			if (m_stepQueuedAfterDrag != 0)
			{
				SetCurrentFTUEStep(m_stepQueuedAfterDrag);
				m_stepQueuedAfterDrag = FTUEStep.INVALID;
			}
		}
		m_isDragging = false;
	}

	public override bool UpdateHeldCardVisual(CollectionDraggableCardVisual collectionDraggableCardVisual)
	{
		m_isDragging = true;
		if (IsMouseOverPortrait(Box.Get().GetCamera()))
		{
			collectionDraggableCardVisual.UpdateVisual(CollectionDraggableCardVisual.ActorVisualMode.IN_PLAY_ZILLIAX_SIDEBOARD);
			return true;
		}
		return false;
	}

	public override void StartDragWithActor(Actor actor, CollectionUtils.ViewMode viewMode, bool showVisual = true, CollectionDeckSlot slot = null)
	{
		m_currentDragSource = "";
		if (slot == null)
		{
			m_currentDragSource = "collection";
		}
		else
		{
			EntityDef draggedEntityDef = slot.GetEntityDef();
			if (draggedEntityDef != null)
			{
				bool isCosmeticModule = draggedEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE);
				m_currentDragSource = (isCosmeticModule ? "portrait" : "decktray");
			}
		}
		m_isDragging = true;
		m_zilliaxSideboardDeck.ZilliaxDataModel.DraggingSource = m_currentDragSource;
		m_zilliaxSideboardDeck.ZilliaxDataModel.IsDragging = m_isDragging;
		this.OnInteractableElementClicked?.Invoke();
	}

	private bool IsMouseOverPortrait(Camera camera)
	{
		RaycastHit hit;
		return UniversalInputManager.Get().ForcedUnblockableInputIsOver(camera, m_zilliaxPortraitDropArea, out hit);
	}

	private void UpdatePreviewActors(ZilliaxSideboardDeck zilliaxSideboardDeck)
	{
		if (zilliaxSideboardDeck == null)
		{
			return;
		}
		TAG_PREMIUM premium = zilliaxSideboardDeck.DataModel.Premium;
		foreach (Actor zilliaxPreviewActor in (premium == TAG_PREMIUM.NORMAL) ? m_NormalZilliaxPreviewActors : m_GoldenZilliaxPreviewActors)
		{
			zilliaxPreviewActor.SetPremium(premium);
			zilliaxPreviewActor.SetEntityDef(zilliaxSideboardDeck.DynamicZilliaxDef);
			CardDefHandle zilliaxCardDefHandle = zilliaxSideboardDeck.DynamicZilliaxCardDefHandle;
			using (DefLoader.DisposableCardDef entityCardDef = zilliaxCardDefHandle.Share())
			{
				zilliaxPreviewActor.SetCardDef(entityCardDef);
			}
			zilliaxCardDefHandle.ReleaseCardDef();
			zilliaxPreviewActor.UpdateAllComponents(needsGhostUpdate: false);
		}
	}

	private void SetCurrentFTUEStep(FTUEStep ftueStep)
	{
		OnInteractableElementClicked -= HideCurrentTooltip;
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleAdded -= OnZilliaxModuleAdded;
		m_zilliaxSideboardDeck.OnZilliaxSideboardModuleRemoved -= OnZilliaxModuleRemoved;
		OnPreviewButtonHovered -= HideCurrentTooltip;
		foreach (var (step, tooltip) in m_FTUETooltips)
		{
			if (!(tooltip == null))
			{
				SetTooltipVisibility(tooltip, step == ftueStep);
			}
		}
		m_currentStep = ftueStep;
		if (m_currentStep != 0 && m_currentStep != FTUEStep.COMPLETE)
		{
			OnInteractableElementClicked += HideCurrentTooltip;
			m_zilliaxSideboardDeck.OnZilliaxSideboardModuleAdded += OnZilliaxModuleAdded;
			m_zilliaxSideboardDeck.OnZilliaxSideboardModuleRemoved += OnZilliaxModuleRemoved;
			if (m_currentStep == FTUEStep.PREVIEW)
			{
				OnPreviewButtonHovered += HideCurrentTooltip;
			}
		}
	}

	private void SetTooltipVisibility(WidgetInstance tooltip, bool isVisible)
	{
		if (tooltip == null || tooltip.gameObject == null)
		{
			return;
		}
		tooltip.gameObject.SetActive(isVisible);
		Notification popupText = tooltip.gameObject.GetComponentInChildren<Notification>();
		if (isVisible)
		{
			tooltip.Show();
			if (popupText != null)
			{
				popupText.PlayBirth();
				popupText.PulseReminderEveryXSeconds(3f);
				return;
			}
			tooltip.RegisterReadyListener(delegate
			{
				Notification componentInChildren = tooltip.gameObject.GetComponentInChildren<Notification>();
				if (componentInChildren != null)
				{
					componentInChildren.PlayBirth();
					componentInChildren.PulseReminderEveryXSeconds(3f);
				}
			});
		}
		else
		{
			tooltip.Hide();
			if (popupText != null)
			{
				popupText.PlayDeathNoDestroy();
			}
		}
	}

	private void HideCurrentTooltip()
	{
		HideCurrentTooltip(shouldClearPortraitStep: false);
	}

	private void HideCurrentTooltip(bool shouldClearPortraitStep)
	{
		if (m_currentStep == FTUEStep.PORTRAIT && !shouldClearPortraitStep)
		{
			return;
		}
		if (m_FTUETooltips.TryGetValue(m_currentStep, out var tooltipToHide))
		{
			SetTooltipVisibility(tooltipToHide, isVisible: false);
		}
		if (m_currentStep == FTUEStep.PORTRAIT)
		{
			m_hasSeenFTUE = true;
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ZILLIAX_CUSTOMIZABLE_FTUE, 1L));
			if (m_isDragging)
			{
				m_stepQueuedAfterDrag = FTUEStep.SAVED;
			}
			else
			{
				SetCurrentFTUEStep(FTUEStep.SAVED);
			}
		}
		else if (m_currentStep == FTUEStep.SAVED)
		{
			SetCurrentFTUEStep(FTUEStep.COMPLETE);
			m_hasSeenFTUE = true;
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ZILLIAX_CUSTOMIZABLE_SAVED_VERSIONS_FTUE, 1L));
		}
	}

	public override bool UpdateCurrentPageCardLocks(IEnumerable<CollectionCardVisual> collectionCardVisuals)
	{
		foreach (CollectionCardVisual collectionCardVisual in collectionCardVisuals)
		{
			CollectionCardVisual.LockType lockType = CollectionCardVisual.LockType.NONE;
			Actor collectionCardVisualActor = collectionCardVisual.GetActor();
			if (collectionCardVisualActor != null)
			{
				EntityDef collectionCardEntityDef = collectionCardVisualActor.GetEntityDef();
				if (GameUtils.IsBannedBySideBoardDenylist(m_zilliaxSideboardDeck, collectionCardEntityDef.GetCardId()))
				{
					lockType = CollectionCardVisual.LockType.BANNED;
				}
				else if (collectionCardEntityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE) && m_zilliaxSideboardDeck.GetCardIdCount(collectionCardVisual.CardId) > 0)
				{
					lockType = CollectionCardVisual.LockType.MAX_COPIES_IN_DECK;
				}
			}
			collectionCardVisual.ShowLock(lockType, GameStrings.Get("GLUE_COLLECTION_LOCK_ZILLIAX_MODULE_USED"), playSound: true);
		}
		return true;
	}

	private void HandlePortraitPress(UIEvent e)
	{
		CollectionDeckSlot cosmeticDeckSlot = m_zilliaxSideboardDeck.GetCosmeticModuleCollectionDeckSlot();
		if (cosmeticDeckSlot != null)
		{
			m_zilliaxSideboardDeck.RemoveCard(cosmeticDeckSlot.CardID, m_zilliaxSideboardDeck.DataModel.Premium, valid: true, enforceRemainingDeckRuleset: false);
		}
	}

	public override bool OnSideboardDoneButtonPressed()
	{
		if (m_hasConfirmedIncompleteExit)
		{
			return true;
		}
		bool hasDifferentFunctionalModules = !DoesCurrentZilliaxFunctionalModulesMatchStartingZilliax();
		bool matchesAnySavedZilliax = DoesCurrentZilliaxFunctionalModulesMatchAnySavedZilliax();
		bool hasTwoFunctionalModules = m_zilliaxSideboardDeck.ZilliaxDataModel.FunctionalModuleCardCount == 2;
		if (!hasTwoFunctionalModules)
		{
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
			{
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_responseCallback = delegate(AlertPopup.Response response, object userData)
				{
					if (response == AlertPopup.Response.CONFIRM)
					{
						m_hasConfirmedIncompleteExit = true;
						CollectionDeckTray.Get().OnSideboardDoneButtonPressed();
					}
				},
				m_headerText = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_ZILLIAX_HEADER"),
				m_text = GameStrings.Get("GLUE_COLLECTION_INCOMPLETE_ZILLIAX_SAVE_WARNING_DESCRIPTION")
			};
			DialogManager.Get().ShowPopup(popup);
			return false;
		}
		if (hasDifferentFunctionalModules && hasTwoFunctionalModules && !matchesAnySavedZilliax)
		{
			if (!m_isAnimating)
			{
				m_constructionAnimationCoroutine = StartCoroutine(DoCreateAnims());
			}
			return false;
		}
		if (m_isAnimating || m_isCancellingAnimation)
		{
			return false;
		}
		return true;
	}

	private IEnumerator DoCreateAnims()
	{
		m_isAnimating = true;
		SetIsOffClickCatcherEnabled(isEnabled: true);
		FadeEffectsIn();
		Navigation.Push(delegate
		{
			if (m_isAnimating)
			{
				CancelBuildAnimation();
				return false;
			}
			EndBuildAnimation();
			return true;
		});
		PlayMakerFSM playmaker = GetComponent<PlayMakerFSM>();
		if (playmaker != null)
		{
			playmaker.SendEvent("Birth");
			yield return new WaitForSeconds(1f);
		}
		LoadNewActorAndConstructIt();
		yield return new WaitForSeconds(1.5f);
		if (m_constructionAnimationActor.HasCardDef && m_constructionAnimationActor.PlayEffectDef != null)
		{
			GameUtils.PlayCardEffectDefSounds(m_constructionAnimationActor.PlayEffectDef);
		}
		yield return new WaitForSeconds(1f);
		if (m_constructionAnimationActor != null)
		{
			m_constructionAnimationActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
			m_constructionAnimationActor.ShowAllText();
		}
		m_isAnimating = false;
		m_constructionAnimationCoroutine = null;
	}

	public void LoadNewActorAndConstructIt()
	{
		if (m_constructionAnimationActor == null)
		{
			m_constructionAnimationActor = GetAndPositionNewActor();
		}
		else
		{
			Actor constructionAnimationActor = m_constructionAnimationActor;
			m_constructionAnimationActor = GetAndPositionNewActor();
			Debug.LogWarning("Destroying unexpected m_constructionAnimationActor to prevent a lost reference");
			UnityEngine.Object.Destroy(constructionAnimationActor.gameObject);
		}
		GhostCard ghostCard = m_constructionAnimationActor.gameObject.GetComponentInChildren<GhostCard>();
		if (ghostCard != null)
		{
			ghostCard.m_shouldShowText = false;
		}
		m_constructionAnimationActor.name = "CurrentBigActor";
		m_constructionAnimationActor.transform.position = m_floatingCardBone.position;
		m_constructionAnimationActor.transform.localScale = m_floatingCardBone.localScale;
		m_constructionAnimationActor.SetPremium(m_zilliaxSideboardDeck.DataModel.Premium);
		m_constructionAnimationActor.SetEntityDef(m_zilliaxSideboardDeck.DynamicZilliaxDef);
		CardDefHandle zilliaxCardDefHandle = m_zilliaxSideboardDeck.DynamicZilliaxCardDefHandle;
		using (DefLoader.DisposableCardDef constructionCardDef = zilliaxCardDefHandle.Share())
		{
			m_constructionAnimationActor.SetCardDef(constructionCardDef);
		}
		zilliaxCardDefHandle.ReleaseCardDef();
		m_constructionAnimationActor.UpdateAllComponents();
		SetBigActorLayer(inCraftingMode: true);
		SpellType constructSpell = SpellType.CONSTRUCT;
		m_constructionAnimationActor.ActivateSpellBirthState(constructSpell);
		m_constructionAnimationActor.HideAllText();
	}

	private Actor GetAndPositionNewActor()
	{
		Actor newActor = UnityEngine.Object.Instantiate((m_zilliaxSideboardDeck.DataModel.Premium == TAG_PREMIUM.NORMAL) ? m_ghostMinionActor : m_ghostGoldenMinionActor);
		if (newActor != null)
		{
			newActor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			newActor.UpdateAllComponents();
			newActor.UpdatePortraitTexture();
			newActor.UpdateCardColor();
			newActor.SetUnlit();
			newActor.Hide();
			newActor.ActivateSpellBirthState(SpellType.GHOSTMODE);
			StartCoroutine(ShowAfterTime(newActor, 0.25f));
		}
		return newActor;
	}

	private IEnumerator ShowAfterTime(Actor actorToShow, float numSeconds)
	{
		yield return new WaitForSeconds(numSeconds);
		if (!(actorToShow != m_constructionAnimationActor))
		{
			actorToShow.Show();
		}
	}

	private void LoadActor(string actorPath, ref Actor actor)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
		go.transform.position = new Vector3(-99999f, 99999f, 99999f);
		actor = go.GetComponent<Actor>();
		actor.TurnOffCollider();
	}

	private void SetBigActorLayer(bool inCraftingMode)
	{
		if (!(m_constructionAnimationActor == null))
		{
			GameLayer layer = (inCraftingMode ? GameLayer.IgnoreFullScreenEffects : GameLayer.CardRaycast);
			LayerUtils.SetLayer(m_constructionAnimationActor.gameObject, layer);
		}
	}

	public void FinishCreateAnims()
	{
		if (m_constructionAnimationActor != null)
		{
			m_constructionAnimationActor.GetSpell(SpellType.GHOSTMODE).GetComponent<PlayMakerFSM>().SendEvent("Cancel");
			UnityEngine.Object.Destroy(m_constructionAnimationActor.gameObject);
			m_constructionAnimationActor = null;
		}
	}

	private void EndBuildAnimation()
	{
		SetIsOffClickCatcherEnabled(isEnabled: false);
		FinishCreateAnims();
		StoreOriginalZilliaxModules();
		m_zilliaxSideboardDeck.ZilliaxDataModel.IsZilliaxAlreadyCrafted = true;
		FadeEffectsOut();
		SoundManager.Get().LoadAndPlay("banner_shrink_quieter.prefab:9fcb0b9e932629044af081408280c374");
		CollectionDeckTray.Get().CloseSideboardTray();
	}

	private void EndConstructionAnimationCoroutine()
	{
		if (m_constructionAnimationCoroutine != null)
		{
			StopCoroutine(m_constructionAnimationCoroutine);
			m_constructionAnimationCoroutine = null;
		}
	}

	private void CancelBuildAnimation()
	{
		if (m_isCancellingAnimation || !m_isAnimating)
		{
			return;
		}
		EndConstructionAnimationCoroutine();
		float CANCEL_TIME = 0.2f;
		if (m_constructionAnimationActor != null)
		{
			m_isCancellingAnimation = true;
			m_constructionAnimationActor.ActivateSpellCancelState(SpellType.CONSTRUCT);
			iTween.Stop(m_constructionAnimationActor.gameObject);
			iTween.RotateTo(m_constructionAnimationActor.gameObject, Vector3.zero, CANCEL_TIME);
			m_constructionAnimationActor.ToggleForceIdle(bOn: false);
			if (m_constructionAnimationActor != null)
			{
				iTween.Stop(m_constructionAnimationActor.gameObject);
				m_constructionAnimationActor.transform.parent = m_constructionAnimationActor.transform;
			}
			SoundManager.Get().LoadAndPlay("Card_Transition_In.prefab:3f3fbe896b8b260448e8c7e5d028d971");
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			Vector3 scaleTo = new Vector3(0.1f, 0.1f, 0.1f);
			scaleArgs.Add("name", "CancelCraftMode");
			scaleArgs.Add("scale", scaleTo);
			scaleArgs.Add("time", CANCEL_TIME);
			scaleArgs.Add("easetype", iTween.EaseType.linear);
			scaleArgs.Add("oncompletetarget", base.gameObject);
			scaleArgs.Add("oncomplete", "FinishCloseConstructAnimation");
			iTween.ScaleTo(m_constructionAnimationActor.gameObject, scaleArgs);
		}
		else
		{
			FinishCloseConstructAnimation();
		}
	}

	private void SetIsOffClickCatcherEnabled(bool isEnabled)
	{
		if (m_offClickCatcher != null)
		{
			m_offClickCatcher.enabled = isEnabled;
		}
	}

	private void FinishCloseConstructAnimation()
	{
		m_isAnimating = false;
		m_isCancellingAnimation = false;
		EndConstructionAnimationCoroutine();
		if (m_constructionAnimationActor != null)
		{
			UnityEngine.Object.Destroy(m_constructionAnimationActor.gameObject);
			m_constructionAnimationActor = null;
		}
		SetIsOffClickCatcherEnabled(isEnabled: false);
		StoreOriginalZilliaxModules();
		m_zilliaxSideboardDeck.ZilliaxDataModel.IsZilliaxAlreadyCrafted = true;
		Navigation.Pop();
		FadeEffectsOut();
		CollectionDeckTray.Get().CloseSideboardTray();
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

	private void OnZilliaxTabPressed()
	{
		HideCurrentTooltip();
	}

	private void OnZilliaxCardInspected(CollectionCardVisual collectionCardVisual)
	{
		HideCurrentTooltip();
	}

	private void OnSavedZilliaxVersionLoaded()
	{
		if (!(m_savedVersionLoadedVC != null))
		{
			return;
		}
		VisualController[] components = m_savedVersionLoadedVC.GetComponents<VisualController>();
		foreach (VisualController vc in components)
		{
			if (vc.HasState("SET_VERSION"))
			{
				vc.SetState("SET_VERSION");
			}
		}
	}
}
