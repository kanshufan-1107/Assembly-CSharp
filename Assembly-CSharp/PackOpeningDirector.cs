using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Game.PackOpening;
using Hearthstone.Attribution;
using Hearthstone.DataModels;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusUtil;
using Shared.UI.Scripts.Carousel;
using UnityEngine;

[CustomEditClass]
public class PackOpeningDirector : MonoBehaviour
{
	private struct OriginalTransformValues
	{
		public Vector3 m_highlightedCardOriginalScale;

		public Vector3 m_highlightedCardOriginalEulerAngles;
	}

	private readonly Vector3 PACK_OPENING_FX_POSITION = Vector3.zero;

	public PackOpeningCard m_HiddenCard;

	public GameObject m_CardsInsidePack;

	public GameObject m_ClassName;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_DoneButtonPrefab;

	public Carousel m_Carousel;

	public int m_MaxCardsShownInCatchupCarousel;

	private HiddenCards m_hiddenCards = new HiddenCards();

	private NormalButton m_doneButton;

	private bool m_loadingDoneButton;

	private bool m_playing;

	private readonly Map<int, Spell> m_packFxSpells = new Map<int, Spell>();

	private Spell m_activePackFxSpell;

	private int m_cardsPendingReveal;

	private int m_effectsPendingFinish;

	private int m_effectsPendingDestroy;

	private int m_centerCardIndex;

	private bool m_doneButtonShown;

	private ScreenEffectsHandle m_effectHandle;

	private PackOpeningCard m_clickedCard;

	private int m_clickedPosition;

	private PackOpeningCard m_glowingCard;

	private float m_initializePackOpeningAnimationStartTime;

	private readonly MassPackOpeningHiddenCards m_massPackOpeningHiddenCards = new MassPackOpeningHiddenCards();

	public WidgetInstance m_massPackOpeningSummaryWidget;

	public WidgetInstance m_massPackOpeningHighlightsWidget;

	private WidgetInstance m_highlightCardsWidget;

	public float m_massPackOpeningHighlightedCardScaleUpFactor;

	public float m_massPackOpeningHighlightedCardScaleAnimationTime;

	private ActorStateType m_massPackOpeningHighlightedCardOldState;

	private MassPackOpeningHighlights m_massPackOpeningHighlights;

	private MassPackOpeningSummary m_massPackOpeningSummary;

	private Queue<List<NetCache.BoosterCard>> m_highlights;

	private int m_numPacksOpened = 1;

	private bool m_useMassPackOpeningFlow;

	private List<PegUIElement> m_massPackOpeningHighlightsClickables = new List<PegUIElement>();

	private const string CODE_HIDE_MPO_SUMMARY = "HIDE_MPO_SUMMARY";

	private const string CODE_MPO_HIGHLIGHTS_CONTINUE_PRESSED = "MPO_HIGHLIGHTS_CONTINUE_PRESSED";

	private const string CODE_MPO_HIGHLIGHTS_REVEAL_PRESSED = "MPO_HIGHLIGHTS_REVEAL_PRESSED";

	private const string CODE_MPO_FLY_IN = "PLAY_INTRO_ANIM";

	private const string CODE_DISMISS_MPO_BLUR = "DISMISS_MPO_BLUR";

	public WidgetInstance m_catchupPackPileWidget;

	public WidgetInstance m_catchupPackCarouselWidgetInstance;

	private CatchupCarouselWidget m_catchupCarouselWidgetComponent;

	private readonly HiddenCards m_catchupPackCarouselHiddenCards = new HiddenCards();

	private bool m_useCatchupPackOpeningFlow;

	private List<Clickable> m_catchupCardClickables = new List<Clickable>();

	private CatchupPackPileWidget m_catchupPackPileWidgetComponent;

	private const string CATCHUP_PACK_START_CARDS_FALLING = "START_FALL";

	private const string CATCHUP_PACK_CARDS_FALLING_FINISHED = "FALL_FINISHED";

	private const string CODE_CATCHUP_PACK_FINISHED = "CODE_CATCH_UP_DONE";

	private const string CATCHUP_PACK_DONE_BUTTON_PRESSED = "CATCHUP_DONE_BUTTON_PRESSED";

	private const string CATCHUP_PACK_CAROUSEL_SETUP = "SETUP";

	private const string CATCHUP_PACK_CAROUSEL_MOTE_OUT = "MOTE_OUT";

	private readonly Vector3 CATCHUP_PACK_HIGHLIGHTED_CARD_OFFSET = new Vector3(0f, 10f, 0f);

	private readonly Vector3 MASS_PACK_OPENING_HIGHLIGHTED_CARD_OFFSET = new Vector3(0f, 10f, 0f);

	public float m_catchupPackHighlightedCardScaleUpFactor;

	public float m_catchupPackHighlightedCardScaleAnimationTime;

	[CustomEditField(Sections = "Sound", T = EditType.SOUND_PREFAB)]
	public string m_onCatchupCardRolloverSound;

	private CameraOverridePass m_catchupPackCardPileCustomPass;

	private Dictionary<GameObject, OriginalTransformValues> m_cardOriginalTransformValues = new Dictionary<GameObject, OriginalTransformValues>();

	private const string EXPLODE_PACK_STATE = "Explode Pack";

	private bool m_massPackOpeningExplosionPlayed;

	private PlayMakerFSM m_spellPlayMaker;

	public bool IsDoneButtonShown => m_doneButtonShown;

	public int NumPacksOpened => m_numPacksOpened;

	public static bool QuickPackOpeningAllowed => NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>()?.QuickOpenEnabled ?? false;

	public event EventHandler OnFinishedEvent;

	public event Action OnDoneOpeningPack;

	private void Awake()
	{
		if (PackOpening.Get().MassPackOpeningEnabled())
		{
			if (m_massPackOpeningHighlightsWidget != null)
			{
				m_massPackOpeningHighlightsWidget.RegisterReadyListener(OnMassPackOpeningHighlightsWidgetReady);
			}
			if (m_massPackOpeningSummaryWidget != null)
			{
				m_massPackOpeningSummaryWidget.RegisterReadyListener(OnMassPackOpeningSummaryWidgetReady);
			}
		}
		m_hiddenCards.OnCardRevealedEvent += OnCardRevealed;
		m_hiddenCards.OnCardSpellFinishedEvent += OnHiddenCardSpellFinished;
		m_hiddenCards.OnCardSpellStateFinishedEvent += OnHiddenCardSpellStateFinished;
		m_hiddenCards.InitializeCards(m_HiddenCard, 5);
		m_effectHandle = new ScreenEffectsHandle(this);
		InitializeUI();
	}

	private void Update()
	{
		if ((bool)m_Carousel)
		{
			m_Carousel.UpdateUI(InputCollection.GetMouseButtonDown(0));
		}
		if (m_useMassPackOpeningFlow && m_spellPlayMaker != null && !m_massPackOpeningExplosionPlayed && m_spellPlayMaker.ActiveStateName == "Explode Pack")
		{
			m_massPackOpeningExplosionPlayed = true;
			StartCoroutine(DoMassPackOpeningInitialAnimations());
		}
	}

	public void Play(int boosterId, float timeToRegisterPackOpening, int packOpeningId)
	{
		if (!m_playing)
		{
			ShowMassPackOpeningHighlightsScreen();
			m_playing = true;
			EnableCardInput(enable: false);
			m_initializePackOpeningAnimationStartTime = Time.realtimeSinceStartup;
			StartCoroutine(PlayWhenReady(boosterId, timeToRegisterPackOpening, packOpeningId));
		}
	}

	public bool IsPlaying()
	{
		return m_playing;
	}

	public void OnBoosterOpened(List<NetCache.BoosterCard> cards, bool isCatchupPack)
	{
		if (isCatchupPack)
		{
			UseCatchupPackOpeningFlow();
		}
		else if (cards.Count > 5)
		{
			if (!PackOpening.Get().MassPackOpeningEnabled())
			{
				Debug.LogError($"PackOpeningDirector.OnBoosterOpened() - Not enough PackOpeningCards! Received {cards.Count} cards. There are only {5} hidden cards.");
				return;
			}
			UseMassPackOpeningFlow();
		}
		if (m_useMassPackOpeningFlow)
		{
			SetUpMassPackOpeningSummaryScreen(cards);
			SetUpMassPackOpeningHighlightsScreen(cards);
			if (m_massPackOpeningHighlights == null || m_massPackOpeningSummary == null)
			{
				Debug.LogError("PackOpeningDirector.OnBoosterOpened() - mass pack opening data was null");
				return;
			}
			m_highlights = m_massPackOpeningHighlights.GetHighights();
		}
		if (!m_useCatchupPackOpeningFlow)
		{
			int minCardCount = Mathf.Min(cards.Count, 5);
			m_cardsPendingReveal = minCardCount;
		}
		else
		{
			m_cardsPendingReveal = (UniversalInputManager.UsePhoneUI ? Mathf.Min(cards.Count, m_MaxCardsShownInCatchupCarousel) : cards.Count);
		}
		if (!m_useMassPackOpeningFlow && !m_useCatchupPackOpeningFlow)
		{
			StartCoroutine(m_hiddenCards.AttachBoosterCards(cards));
		}
		else if (m_useCatchupPackOpeningFlow)
		{
			cards.Sort(delegate(NetCache.BoosterCard lhs, NetCache.BoosterCard rhs)
			{
				EntityDef entityDef = DefLoader.Get().GetEntityDef(rhs.Def.Name);
				EntityDef entityDef2 = DefLoader.Get().GetEntityDef(lhs.Def.Name);
				return (entityDef != null && entityDef2 != null) ? (entityDef.GetRarity() - entityDef2.GetRarity()) : 0;
			});
			SetupCatchupPackCardsFalling(cards);
			SetupCatchupPackCarousel(cards);
			SetupCatchupPackExpansionCollectionStats(cards);
		}
	}

	public void OnMercenariesBoosterOpened(List<LettucePackComponent> packComponents)
	{
		if (packComponents.Count > 5)
		{
			Debug.LogError($"PackOpeningDirector.OnMercenariesBoosterOpened() - Not enough PackOpeningCards! Received {packComponents.Count} cards. There are only {5} hidden cards.");
			return;
		}
		int minCardCount = Mathf.Min(packComponents.Count, 5);
		m_cardsPendingReveal = minCardCount;
		StartCoroutine(m_hiddenCards.AttachBoosterMercenaries(packComponents));
	}

	public void HideCardsAndDoneButton()
	{
		m_hiddenCards.DeactivateCards();
		if (IsDoneButtonShown)
		{
			HideDoneButton();
		}
	}

	public void FinishPackOpen()
	{
		if (m_doneButtonShown)
		{
			m_activePackFxSpell.ActivateState(SpellStateType.DEATH);
			m_effectHandle.StopEffect();
			m_effectsPendingFinish = 1 + (m_useCatchupPackOpeningFlow ? (2 * m_hiddenCards.Count) : 10);
			m_effectsPendingDestroy = m_effectsPendingFinish;
			HideDoneButton();
			AchievementManager.Get().UnpauseToastNotifications();
			PopupDisplayManager.SuppressPopupsTemporarily = false;
			if (m_useCatchupPackOpeningFlow && (bool)UniversalInputManager.UsePhoneUI && m_catchupPackCarouselWidgetInstance != null)
			{
				m_catchupPackCarouselWidgetInstance.TriggerEvent("MOTE_OUT");
			}
			if (m_useMassPackOpeningFlow)
			{
				m_massPackOpeningHiddenCards.Dissipate();
			}
			else
			{
				m_hiddenCards.Dissipate();
			}
			this.OnDoneOpeningPack?.Invoke();
			HideKeywordTooltips();
		}
	}

	public void ForceRevealRandomCard()
	{
		m_hiddenCards.ForceRevealRandomCard();
	}

	private IEnumerator PlayWhenReady(int boosterId, float timeToRegisterPackOpening, int packOpeningId)
	{
		while (m_loadingDoneButton)
		{
			yield return null;
		}
		if (m_doneButton == null)
		{
			this.OnFinishedEvent?.Invoke(this, EventArgs.Empty);
			yield break;
		}
		if (!m_packFxSpells.TryGetValue(boosterId, out var spell))
		{
			BoosterDbfRecord record = GameDbf.Booster.GetRecord(boosterId);
			bool loading = true;
			PrefabCallback<GameObject> onPackOpeningFxPrefabLoadAttempted = delegate(AssetReference assetRef, GameObject go, object callbackData)
			{
				loading = false;
				m_packFxSpells[boosterId] = spell;
				if (go == null)
				{
					Error.AddDevFatal("PackOpeningDirector.onPackOpeningFxPrefabLoadAttempted() - Error loading {0} for booster id {1}", assetRef, boosterId);
				}
				else
				{
					spell = go.GetComponent<Spell>();
					go.transform.parent = base.transform;
					go.transform.localPosition = PACK_OPENING_FX_POSITION;
				}
			};
			AssetReference assetReference = record.PackOpeningFxPrefab;
			if (!AssetLoader.Get().InstantiatePrefab(assetReference, onPackOpeningFxPrefabLoadAttempted))
			{
				onPackOpeningFxPrefabLoadAttempted(assetReference, null, null);
			}
			while (loading)
			{
				yield return null;
			}
		}
		if (!spell)
		{
			this.OnFinishedEvent?.Invoke(this, EventArgs.Empty);
			yield break;
		}
		m_activePackFxSpell = spell;
		m_spellPlayMaker = m_activePackFxSpell.GetPlayMaker();
		if (m_useMassPackOpeningFlow)
		{
			PlayMakerFSM fsm = spell.GetComponent<PlayMakerFSM>();
			if (fsm != null)
			{
				fsm.FsmVariables.GetFsmGameObject("PackOpeningDirector").Value = base.gameObject;
			}
		}
		else
		{
			PlayMakerFSM fsm2 = spell.GetComponent<PlayMakerFSM>();
			if (fsm2 != null)
			{
				fsm2.FsmVariables.GetFsmGameObject("CardsInsidePack").Value = m_CardsInsidePack;
				fsm2.FsmVariables.GetFsmGameObject("ClassName").Value = m_ClassName;
				fsm2.FsmVariables.GetFsmGameObject("PackOpeningDirector").Value = base.gameObject;
			}
		}
		m_activePackFxSpell.AddFinishedCallback(OnSpellFinished);
		if (!Options.Get().GetBool(Option.SKIPPED_INITIAL_TUTORIAL) && !Options.Get().GetBool(Option.AF_FIRST_PACK_OPENED))
		{
			CreateSkipHelper.QueueSkipScreenAtBox();
		}
		float duration = Time.realtimeSinceStartup - m_initializePackOpeningAnimationStartTime;
		TelemetryManager.Client().SendPackOpening(timeToRegisterPackOpening, duration, packOpeningId, m_numPacksOpened);
		BlizzardAttributionManager.Get().SendEvent_PackOpen(packOpeningId);
		m_activePackFxSpell.ActivateState(SpellStateType.ACTION);
	}

	private void OnSpellFinished(Spell spell, object userData)
	{
		if (m_useMassPackOpeningFlow && m_massPackOpeningHighlights != null)
		{
			PackOpening.MarkMassPackOpeningTooltipSeen();
			if (!m_massPackOpeningExplosionPlayed)
			{
				m_massPackOpeningExplosionPlayed = true;
				StartCoroutine(DoMassPackOpeningInitialAnimations());
			}
		}
		else
		{
			m_hiddenCards.SetInputEnabled(enable: true);
			m_hiddenCards.EnableReveal();
			AttachCardsToCarousel();
		}
	}

	private IEnumerator DoMassPackOpeningInitialAnimations()
	{
		if (m_useMassPackOpeningFlow && !(m_massPackOpeningHighlights == null))
		{
			m_massPackOpeningHighlights.TriggerCardExplosionAnimation();
			while (!m_massPackOpeningHighlights.IsInitialCardExplosionComplete())
			{
				yield return null;
			}
			m_massPackOpeningHighlights.HideCardExplosionAnimation();
			ShowNextSetOfMassPackOpeningHighlights();
		}
	}

	private void CameraBlurOn()
	{
		m_effectHandle.StartEffect(ScreenEffectParameters.BlurVignettePerspective);
	}

	private void CameraBlurOff()
	{
		m_effectHandle.StopEffect();
	}

	private void AttachCardsToCarousel()
	{
		if (!(m_Carousel == null))
		{
			m_hiddenCards.EnableCollision();
			if (QuickPackOpeningAllowed && (bool)UniversalInputManager.UsePhoneUI)
			{
				m_hiddenCards.ShowRarityGlow();
			}
			List<Carousel.Item> items = m_hiddenCards.ToCarouselItems().ToList();
			if (m_useCatchupPackOpeningFlow && m_catchupCarouselWidgetComponent != null && m_catchupCarouselWidgetComponent.m_extraCardsRootGameObject != null)
			{
				items.Add(new CatchupPackSummaryCardCarouselItem(m_catchupCarouselWidgetComponent.m_extraCardsRootGameObject));
				m_catchupCarouselWidgetComponent.m_extraCardsRootGameObject.SetActive(value: true);
				m_Carousel.m_maxPosition++;
			}
			m_Carousel.Initialize(items.ToArray());
			m_Carousel.UpdateVisibleItems(5, shouldTriggerOnCrossedCenterPosition: false);
			m_Carousel.OnSettled += CarouselSettled;
			m_Carousel.OnStartedScrolling += CarouselStartedScrolling;
			m_Carousel.OnItemClicked += CarouselItemClicked;
			m_Carousel.OnItemReleased += CarouselItemReleased;
			m_Carousel.OnItemCrossedCenterPosition += CarouselItemCrossedCenterPosition;
			CarouselSettled();
			CarouselItemCrossedCenterPosition(m_Carousel.CurrentItem, 0);
		}
	}

	private void CarouselItemCrossedCenterPosition(Carousel.Item item, int index)
	{
		if ((bool)UniversalInputManager.UsePhoneUI && item != null && QuickPackOpeningAllowed && item is PackOpeningCardCarouselItem)
		{
			PackOpeningCard card = ((PackOpeningCardCarouselItem)item).GetGameObject().GetComponent<PackOpeningCard>();
			if (!card.IsRevealed())
			{
				card.ForceReveal();
			}
		}
	}

	private void CarouselItemClicked(Carousel.Item item, int index)
	{
		m_clickedCard = item.GetGameObject().GetComponent<PackOpeningCard>();
		m_clickedPosition = index;
	}

	private void CarouselItemReleased()
	{
		if (m_Carousel.IsScrolling)
		{
			return;
		}
		bool allowTapCardToSetPosition = !UniversalInputManager.UsePhoneUI || !QuickPackOpeningAllowed;
		if (m_clickedPosition == m_Carousel.CurrentIndex)
		{
			if (!(m_clickedCard != null))
			{
				return;
			}
			if (m_clickedCard.IsRevealed())
			{
				if (allowTapCardToSetPosition && m_clickedPosition < 4)
				{
					m_Carousel.SetPosition(m_clickedPosition + 1, animate: true);
				}
			}
			else
			{
				m_clickedCard.ForceReveal();
			}
		}
		else if (allowTapCardToSetPosition)
		{
			m_Carousel.SetPosition(m_clickedPosition, animate: true);
		}
	}

	private void CarouselSettled()
	{
		Carousel.Item item = m_Carousel.CurrentItem;
		if (item is PackOpeningCardCarouselItem)
		{
			(m_glowingCard = ((PackOpeningCardCarouselItem)item).GetGameObject().GetComponent<PackOpeningCard>()).ShowRarityGlow();
		}
	}

	private void CarouselStartedScrolling()
	{
		if (m_glowingCard != null && m_glowingCard.GetEntityDef() != null && m_glowingCard.GetEntityDef().GetRarity() != TAG_RARITY.COMMON)
		{
			m_glowingCard.HideRarityGlow();
		}
	}

	private void InitializeUI()
	{
		HideMassPackOpeningHighlightsScreen();
		if (m_massPackOpeningSummaryWidget != null)
		{
			m_massPackOpeningSummaryWidget.Hide();
		}
		if (m_catchupPackPileWidget != null)
		{
			m_catchupPackPileWidget.Hide();
		}
		if (m_catchupPackCarouselWidgetInstance != null)
		{
			m_catchupPackCarouselWidgetInstance.Hide();
		}
		m_loadingDoneButton = true;
		AssetReference assetReference = m_DoneButtonPrefab;
		if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnDoneButtonLoadAttempted, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			OnDoneButtonLoadAttempted(assetReference, null, null);
		}
	}

	private void OnDoneButtonLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_loadingDoneButton = false;
		if (go == null)
		{
			Debug.LogError($"PackOpeningDirector.OnDoneButtonLoadAttempted() - FAILED to load \"{assetRef}\"");
			return;
		}
		m_doneButton = go.GetComponent<NormalButton>();
		if (m_doneButton == null)
		{
			Debug.LogError($"PackOpeningDirector.OnDoneButtonLoadAttempted() - ERROR \"{assetRef}\" has no {typeof(NormalButton)} component");
			return;
		}
		LayerUtils.SetLayer(m_doneButton.gameObject, GameLayer.IgnoreFullScreenEffects);
		m_doneButton.transform.parent = base.transform;
		TransformUtil.CopyWorld(m_doneButton, PackOpening.Get().m_Bones.m_DoneButton);
		RenderUtils.EnableRenderersAndColliders(m_doneButton.gameObject, enable: false);
	}

	private void ShowDoneButton()
	{
		m_doneButtonShown = true;
		RenderUtils.EnableRenderersAndColliders(m_doneButton.gameObject, enable: true);
		Spell component = m_doneButton.m_button.GetComponent<Spell>();
		component.AddFinishedCallback(OnDoneButtonShown);
		component.ActivateState(SpellStateType.BIRTH);
	}

	private void OnDoneButtonShown(Spell spell, object userData)
	{
		m_doneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
	}

	private void HideDoneButton()
	{
		m_doneButtonShown = false;
		RenderUtils.EnableColliders(m_doneButton.gameObject, enable: false);
		m_doneButton.RemoveEventListener(UIEventType.RELEASE, OnDoneButtonPressed);
		Spell doneButtonSpell = m_doneButton.m_button.GetComponent<Spell>();
		if (!m_useMassPackOpeningFlow)
		{
			doneButtonSpell.AddFinishedCallback(OnDoneButtonHidden);
		}
		doneButtonSpell.ActivateState(SpellStateType.DEATH);
	}

	private void OnDoneButtonHidden(Spell spell, object userData)
	{
		OnEffectFinished();
		OnEffectDone();
	}

	private void OnDoneButtonPressed(UIEvent e)
	{
		HideKeywordTooltips();
		if (!m_useMassPackOpeningFlow)
		{
			FinishPackOpen();
		}
	}

	private void HideKeywordTooltips()
	{
		if (m_useMassPackOpeningFlow && m_massPackOpeningHighlights != null)
		{
			m_massPackOpeningHiddenCards.RemoveOnOverWhileFlippedListeners();
			m_massPackOpeningHighlightsClickables.Clear();
			m_cardOriginalTransformValues.Clear();
		}
		else if (m_useCatchupPackOpeningFlow)
		{
			if (!UniversalInputManager.UsePhoneUI)
			{
				foreach (Clickable catchupCardClickable in m_catchupCardClickables)
				{
					catchupCardClickable.RemoveEventListener(UIEventType.ROLLOVER, OnCatchupCardRolloverEvent);
					catchupCardClickable.RemoveEventListener(UIEventType.ROLLOUT, OnCatchupCardRolloutEvent);
				}
			}
			m_catchupCardClickables.Clear();
			m_cardOriginalTransformValues.Clear();
		}
		else
		{
			m_hiddenCards.RemoveOnOverWhileFlippedListeners();
		}
		TooltipPanelManager.Get().HideKeywordHelp();
	}

	private void EnableCardInput(bool enable)
	{
		m_hiddenCards.SetInputEnabled(enable);
	}

	private void OnCardRevealed(object userData, EventArgs eventArgs)
	{
		PackOpeningCard openedCard = (PackOpeningCard)userData;
		if (openedCard.GetEntityDef().GetRarity() == TAG_RARITY.LEGENDARY && openedCard.GetActor() != null)
		{
			if (openedCard.GetActor().GetPremium() == TAG_PREMIUM.GOLDEN)
			{
				BnetPresenceMgr.Get().SetGameField(4u, openedCard.GetCardId() + ",1");
			}
			else
			{
				BnetPresenceMgr.Get().SetGameField(4u, openedCard.GetCardId() + ",0");
			}
		}
		if (m_useMassPackOpeningFlow && (bool)UniversalInputManager.UsePhoneUI)
		{
			SetUpRolloverEventsOnHighlightCard(openedCard);
		}
		m_cardsPendingReveal--;
		if (m_cardsPendingReveal > 0)
		{
			return;
		}
		if (m_useMassPackOpeningFlow)
		{
			if (m_massPackOpeningHighlights != null && m_massPackOpeningHiddenCards.AreAllCardsRevealed())
			{
				m_massPackOpeningHighlights.ShowContinueButton(show: true);
			}
		}
		else
		{
			ShowDoneButton();
		}
	}

	private void OnHiddenCardSpellFinished(object userData, EventArgs eventArgs)
	{
		OnEffectFinished();
	}

	private void OnHiddenCardSpellStateFinished(object sender, Spell spell)
	{
		if (!(spell != null) || spell.GetActiveState() == SpellStateType.NONE)
		{
			OnEffectDone();
		}
	}

	private void SetUpMassPackOpeningHighlightsScreen(List<NetCache.BoosterCard> cards)
	{
		if (m_useMassPackOpeningFlow && !(m_massPackOpeningHighlights == null) && !(m_massPackOpeningHighlightsWidget == null))
		{
			m_massPackOpeningHighlightsWidget.RegisterEventListener(HandleMassPackOpeningHighlightsEvent);
			m_massPackOpeningHighlights.SetNumPacksOpened(m_numPacksOpened);
			m_massPackOpeningHighlights.DetermineMassPackOpeningHighlights(cards);
		}
	}

	private void InitializeHighlightCards()
	{
		if (m_useMassPackOpeningFlow && !(m_massPackOpeningHighlights == null))
		{
			m_highlightCardsWidget = m_massPackOpeningHighlights.SpawnNewHighlightCards();
			m_highlightCardsWidget.RegisterReadyListener(delegate
			{
				StartCoroutine(NewHighlightsReadyImpl());
			});
		}
	}

	private IEnumerator NewHighlightsReadyImpl()
	{
		m_massPackOpeningHiddenCards.InitializeCards(m_massPackOpeningHighlights.GetPackOpeningCards());
		m_massPackOpeningHiddenCards.AttachMassPackOpeningHighlightCards(m_highlights.Dequeue());
		m_massPackOpeningHiddenCards.SetHiddenCardMeshVisible();
		m_massPackOpeningHiddenCards.OnCardRevealedEvent += OnCardRevealed;
		m_massPackOpeningHiddenCards.OnCardSpellFinishedEvent += OnHiddenCardSpellFinished;
		m_massPackOpeningHiddenCards.OnCardSpellStateFinishedEvent += OnHiddenCardSpellStateFinished;
		m_massPackOpeningHighlights.SetBannerActive(active: true);
		TriggerCardFlyIn();
		yield return new WaitForSeconds(0.5f);
		m_massPackOpeningHighlights.ShowContinueButton(show: false);
		m_cardsPendingReveal = 5;
		m_massPackOpeningHiddenCards.SetInputEnabled(enable: true);
		m_massPackOpeningHiddenCards.EnableReveal();
	}

	public void TriggerCardFlyIn()
	{
		VisualController[] componentsInChildren = m_highlightCardsWidget.GetComponentsInChildren<VisualController>();
		foreach (VisualController vc in componentsInChildren)
		{
			if (vc.HasState("PLAY_INTRO_ANIM"))
			{
				vc.SetState("PLAY_INTRO_ANIM");
			}
		}
	}

	private void ShowNextSetOfMassPackOpeningHighlights()
	{
		if (m_useMassPackOpeningFlow && !(m_massPackOpeningHighlights == null))
		{
			if (m_highlights != null && m_highlights.Count == 0)
			{
				FinishSeeingMassPackOpeningHighlights();
			}
			else
			{
				InitializeHighlightCards();
			}
		}
	}

	private void ShowMassPackOpeningHighlightsScreen()
	{
		if (m_useMassPackOpeningFlow && m_massPackOpeningHighlightsWidget != null && m_massPackOpeningHighlights != null)
		{
			m_massPackOpeningHighlightsWidget.gameObject.SetActive(value: true);
			m_massPackOpeningHighlightsWidget.Show();
		}
	}

	private void HideMassPackOpeningHighlightsScreen()
	{
		if (m_massPackOpeningHighlightsWidget != null && m_massPackOpeningHighlights != null)
		{
			m_massPackOpeningHighlightsWidget.gameObject.SetActive(value: false);
			m_massPackOpeningHighlightsWidget.Hide();
		}
	}

	private void HandleMassPackOpeningHighlightsEvent(string eventName)
	{
		if (!(eventName == "MPO_HIGHLIGHTS_CONTINUE_PRESSED"))
		{
			if (eventName == "MPO_HIGHLIGHTS_REVEAL_PRESSED")
			{
				MassPackOpeningRevealAllPressed();
			}
		}
		else
		{
			MassPackOpeningContinuePressed();
		}
	}

	public void MassPackOpeningContinuePressed()
	{
		TooltipPanelManager.Get().HideTooltipPanels();
		m_massPackOpeningHiddenCards.ResetCards();
		RemoveRolloverEventsOnHighlightCards();
		ShowNextSetOfMassPackOpeningHighlights();
	}

	public void MassPackOpeningRevealAllPressed()
	{
		m_massPackOpeningHiddenCards.ForceRevealAllCards();
	}

	public void MassPackOpeningRevealRandomCard()
	{
		m_massPackOpeningHiddenCards.ForceRevealRandomCard();
	}

	public void MassPackOpeningDonePressed()
	{
		TooltipPanelManager.Get().HideTooltipPanels();
		if (m_massPackOpeningSummary != null && m_massPackOpeningSummaryWidget != null)
		{
			m_massPackOpeningSummary.PressDoneButton();
		}
	}

	private void ShowMassPackOpeningSummaryScreen()
	{
		if (m_massPackOpeningSummaryWidget != null && m_massPackOpeningSummary != null)
		{
			MassPackOpeningSummaryDataModel dataModel = m_massPackOpeningSummary.GetDataModel();
			if (dataModel != null)
			{
				m_massPackOpeningSummaryWidget.BindDataModel(dataModel);
			}
			m_massPackOpeningSummaryWidget.Show();
		}
	}

	private void SetUpMassPackOpeningSummaryScreen(List<NetCache.BoosterCard> cards)
	{
		if (!(m_massPackOpeningSummary == null))
		{
			m_massPackOpeningSummary.SetNumPacksOpened(m_numPacksOpened);
			m_massPackOpeningSummary.SetCardsOpened(cards);
			m_massPackOpeningSummary.InitDataModel();
			if (m_massPackOpeningSummaryWidget != null)
			{
				m_massPackOpeningSummaryWidget.RegisterEventListener(HandleMassPackOpeningSummaryEvent);
			}
		}
	}

	public void SetNumPacksOpened(int numPacks)
	{
		if (numPacks != 0 && numPacks != m_numPacksOpened)
		{
			m_numPacksOpened = numPacks;
			if (m_numPacksOpened > 1)
			{
				UseMassPackOpeningFlow();
			}
		}
	}

	private void UseMassPackOpeningFlow()
	{
		m_useMassPackOpeningFlow = true;
		m_useCatchupPackOpeningFlow = false;
		DeactivateHiddenCards();
		DeactivateCarousel();
	}

	private void DeactivateHiddenCards()
	{
		if (m_hiddenCards != null)
		{
			m_hiddenCards.DeactivateCards();
			m_hiddenCards.OnCardRevealedEvent -= OnCardRevealed;
			m_hiddenCards.OnCardSpellFinishedEvent -= OnHiddenCardSpellFinished;
			m_hiddenCards.OnCardSpellStateFinishedEvent -= OnHiddenCardSpellStateFinished;
		}
	}

	public bool IsMassPackOpening()
	{
		return m_useMassPackOpeningFlow;
	}

	public bool IsCatchupPackOpening()
	{
		return m_useCatchupPackOpeningFlow;
	}

	private void HandleMassPackOpeningSummaryEvent(string eventName)
	{
		if (!(eventName == "HIDE_MPO_SUMMARY"))
		{
			if (eventName == "DISMISS_MPO_BLUR")
			{
				CameraBlurOff();
			}
		}
		else
		{
			m_massPackOpeningSummaryWidget.Hide();
			FinishMassPackOpen();
		}
	}

	private void OnMassPackOpeningSummaryWidgetReady(object unused)
	{
		m_massPackOpeningSummary = m_massPackOpeningSummaryWidget.GetComponentInChildren<MassPackOpeningSummary>(includeInactive: true);
		m_massPackOpeningSummaryWidget.Hide();
	}

	private void OnMassPackOpeningHighlightsWidgetReady(object unused)
	{
		m_massPackOpeningHighlights = m_massPackOpeningHighlightsWidget.GetComponentInChildren<MassPackOpeningHighlights>(includeInactive: true);
		HideMassPackOpeningHighlightsScreen();
	}

	public void FinishSeeingMassPackOpeningHighlights()
	{
		HideMassPackOpeningHighlightsScreen();
		FinishPackOpen();
		m_massPackOpeningHiddenCards.DeactivateCards();
		ShowMassPackOpeningSummaryScreen();
	}

	public void FinishMassPackOpen()
	{
		this.OnFinishedEvent?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
		AchievementManager.Get().UnpauseToastNotifications();
		PopupDisplayManager.SuppressPopupsTemporarily = false;
		PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = false;
	}

	public bool IsMassPackOpeningHighlightsContinueButtonShowing()
	{
		if (m_massPackOpeningHighlights == null || m_massPackOpeningHighlightsWidget == null)
		{
			return false;
		}
		if (!m_massPackOpeningHighlightsWidget.IsActive)
		{
			return false;
		}
		return m_massPackOpeningHighlights.IsContinueButtonShowing();
	}

	public bool IsMassPackOpeningHighlightsRevealButtonShowing()
	{
		if (m_massPackOpeningHighlights == null || m_massPackOpeningHighlightsWidget == null)
		{
			return false;
		}
		if (!m_massPackOpeningHighlightsWidget.IsActive)
		{
			return false;
		}
		return m_massPackOpeningHighlights.IsRevealButtonShowing();
	}

	public bool IsMassPackOpeningSummaryDoneButtonShowing()
	{
		if (m_massPackOpeningSummary == null || m_massPackOpeningSummaryWidget == null)
		{
			return false;
		}
		if (!m_massPackOpeningSummaryWidget.IsActive)
		{
			return false;
		}
		if (IsMassPackOpeningHighlightsContinueButtonShowing() || IsMassPackOpeningHighlightsRevealButtonShowing())
		{
			return false;
		}
		return m_massPackOpeningSummary.IsDoneButtonShowing();
	}

	public bool IsCatchupPackOpeningDoneButtonShowing()
	{
		if (m_catchupPackPileWidget == null || m_catchupPackPileWidgetComponent == null)
		{
			return false;
		}
		if (!m_catchupPackPileWidget.IsActive)
		{
			return false;
		}
		return m_catchupPackPileWidgetComponent.IsDoneButtonShowing();
	}

	private void SetUpRolloverEventsOnHighlightCard(PackOpeningCard card)
	{
		if (!(card == null))
		{
			PegUIElement clickableToSubscribeTo = card.GetCardRevealButton();
			if (clickableToSubscribeTo != null)
			{
				clickableToSubscribeTo.AddEventListener(UIEventType.ROLLOVER, OnMassPackOpeningHighlightRolloverEvent);
				clickableToSubscribeTo.AddEventListener(UIEventType.ROLLOUT, OnMassPackOpeningHighlightRolloutEvent);
				m_massPackOpeningHighlightsClickables.Add(clickableToSubscribeTo);
			}
		}
	}

	private void RemoveRolloverEventsOnHighlightCards()
	{
		if (m_massPackOpeningHighlightsClickables == null)
		{
			return;
		}
		foreach (PegUIElement massPackOpeningHighlightsClickable in m_massPackOpeningHighlightsClickables)
		{
			massPackOpeningHighlightsClickable.RemoveEventListener(UIEventType.ROLLOVER, OnMassPackOpeningHighlightRolloverEvent);
			massPackOpeningHighlightsClickable.RemoveEventListener(UIEventType.ROLLOUT, OnMassPackOpeningHighlightRolloutEvent);
		}
		m_massPackOpeningHighlightsClickables.Clear();
	}

	private void OnMassPackOpeningHighlightRolloverEvent(UIEvent e)
	{
		PackOpeningCard hoveredCard = e.GetElement().gameObject.GetComponentInParent<PackOpeningCard>();
		if (!(hoveredCard != null))
		{
			return;
		}
		Actor cardActor = hoveredCard.GetActor();
		if (!(cardActor != null))
		{
			return;
		}
		ActorStateType actorStateType = cardActor.GetActorStateType();
		if (actorStateType != ActorStateType.CARD_IDLE && actorStateType != ActorStateType.CARD_RECENTLY_ACQUIRED)
		{
			return;
		}
		m_massPackOpeningHighlightedCardOldState = actorStateType;
		cardActor.SetActorState(ActorStateType.CARD_HISTORY);
		if (!m_cardOriginalTransformValues.ContainsKey(hoveredCard.gameObject))
		{
			m_cardOriginalTransformValues.Add(hoveredCard.gameObject, new OriginalTransformValues
			{
				m_highlightedCardOriginalScale = hoveredCard.gameObject.transform.localScale
			});
		}
		OriginalTransformValues transformValues = m_cardOriginalTransformValues[hoveredCard.gameObject];
		iTween.ScaleTo(hoveredCard.gameObject, iTween.Hash("scale", transformValues.m_highlightedCardOriginalScale * m_massPackOpeningHighlightedCardScaleUpFactor, "time", m_massPackOpeningHighlightedCardScaleAnimationTime, "oncomplete", (Action<object>)delegate
		{
			if (cardActor.GetActorStateType() == ActorStateType.CARD_HISTORY)
			{
				bool flag = UniversalInputManager.UsePhoneUI;
				bool flag2 = flag && hoveredCard.IsCardOnTheLeftOfTheMassPackOpeningHighlightScreen();
				TooltipPanelManager.Get().UpdateKeywordHelpForPackOpening(cardActor.GetEntityDef(), cardActor, flag2 ? TooltipPanelManager.TooltipBoneSource.TOP_RIGHT : TooltipPanelManager.TooltipBoneSource.TOP_LEFT, flag);
			}
		}));
		floatyObj floatyObjComponent = hoveredCard.gameObject.GetComponentInParent<floatyObj>();
		if (floatyObjComponent != null)
		{
			floatyObjComponent.Enabled = true;
		}
	}

	private void OnMassPackOpeningHighlightRolloutEvent(UIEvent e)
	{
		PackOpeningCard hoveredCard = e.GetElement().gameObject.GetComponentInParent<PackOpeningCard>();
		if (!(hoveredCard != null))
		{
			return;
		}
		Actor cardActor = hoveredCard.GetActor();
		if (cardActor != null && m_cardOriginalTransformValues.ContainsKey(hoveredCard.gameObject))
		{
			iTween.Stop(hoveredCard.gameObject);
			OriginalTransformValues transformValues = m_cardOriginalTransformValues[hoveredCard.gameObject];
			TooltipPanelManager.Get().HideKeywordHelp();
			iTween.ScaleTo(hoveredCard.gameObject, iTween.Hash("scale", transformValues.m_highlightedCardOriginalScale, "time", m_massPackOpeningHighlightedCardScaleAnimationTime, "oncomplete", (Action<object>)delegate
			{
				cardActor.SetActorState(m_massPackOpeningHighlightedCardOldState);
			}));
			floatyObj floatyObjComponent = hoveredCard.gameObject.GetComponentInParent<floatyObj>();
			if (floatyObjComponent != null)
			{
				floatyObjComponent.Enabled = false;
			}
		}
	}

	private void UseCatchupPackOpeningFlow()
	{
		m_useCatchupPackOpeningFlow = true;
		m_useMassPackOpeningFlow = false;
		if (!UniversalInputManager.UsePhoneUI && m_catchupPackPileWidget != null)
		{
			m_catchupPackPileWidget.RegisterEventListener(HandleCatchupPackOpeningEvent);
		}
		DeactivateHiddenCards();
		DeactivateCarousel();
	}

	private void HandleCatchupPackOpeningEvent(string eventName)
	{
		switch (eventName)
		{
		case "CATCHUP_DONE_BUTTON_PRESSED":
			HideKeywordTooltips();
			AchievementManager.Get().UnpauseToastNotifications();
			PopupDisplayManager.SuppressPopupsTemporarily = false;
			PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = false;
			break;
		case "CODE_CATCH_UP_DONE":
			FinishCatchupPackOpen();
			break;
		case "FALL_FINISHED":
			OnCatchupPackCardsFallingFinished();
			break;
		}
	}

	public void TriggerCatchupPackOpeningDoneButtonPressed()
	{
		if (!(m_catchupPackPileWidget == null) && !(m_catchupPackPileWidgetComponent == null))
		{
			m_catchupPackPileWidgetComponent.TriggerDoneButtonPressed();
		}
	}

	private void OnCatchupPackCardsFallingFinished()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		Hearthstone.UI.Card[] componentsInChildren = m_catchupPackPileWidget.gameObject.GetComponentsInChildren<Hearthstone.UI.Card>();
		foreach (Hearthstone.UI.Card card in componentsInChildren)
		{
			Clickable clickableToSubscribeTo = card.GetComponentInParent<Clickable>();
			clickableToSubscribeTo.AddEventListener(UIEventType.ROLLOVER, OnCatchupCardRolloverEvent);
			clickableToSubscribeTo.AddEventListener(UIEventType.ROLLOUT, OnCatchupCardRolloutEvent);
			m_catchupCardClickables.Add(clickableToSubscribeTo);
			ApplySettingsToRenderers(card.gameObject.transform, GameLayer.Default, m_catchupPackCardPileCustomPass.renderLayerMaskOverride);
		}
		m_catchupPackPileWidgetComponent = m_catchupPackPileWidget.gameObject.GetComponentInChildren<CatchupPackPileWidget>();
		if (m_catchupPackPileWidgetComponent != null)
		{
			if (m_catchupPackPileWidgetComponent.SummaryCard != null)
			{
				ApplySettingsToRenderers(m_catchupPackPileWidgetComponent.SummaryCard.transform, GameLayer.Default, m_catchupPackCardPileCustomPass.renderLayerMaskOverride);
			}
			if (m_catchupPackPileWidgetComponent.TitleBanner != null)
			{
				ApplySettingsToRenderers(m_catchupPackPileWidgetComponent.TitleBanner.transform, GameLayer.Default, m_catchupPackCardPileCustomPass.renderLayerMaskOverride);
			}
		}
	}

	private void ApplySettingsToRenderers(Transform objectRoot, GameLayer layer, uint renderingLayerMask)
	{
		Renderer[] componentsInChildren = objectRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer obj in componentsInChildren)
		{
			obj.gameObject.layer = (int)layer;
			obj.renderingLayerMask = renderingLayerMask;
		}
		UberText[] componentsInChildren2 = objectRoot.GetComponentsInChildren<UberText>(includeInactive: true);
		foreach (UberText obj2 in componentsInChildren2)
		{
			obj2.gameObject.layer = (int)layer;
			obj2.SetRenderingLayerMaskOverride(renderingLayerMask);
		}
	}

	private TooltipPanelManager.TooltipBoneSource GetFocusedCardTooltipBone(Actor actor)
	{
		if (actor == null)
		{
			return TooltipPanelManager.TooltipBoneSource.INVALID;
		}
		if (m_catchupPackPileWidgetComponent == null || m_catchupPackPileWidgetComponent.TitleBanner == null)
		{
			return TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
		}
		Vector3 centralBannerPosition = m_catchupPackPileWidgetComponent.TitleBanner.gameObject.transform.position;
		if (actor.transform.position.x > centralBannerPosition.x)
		{
			return TooltipPanelManager.TooltipBoneSource.TOP_LEFT;
		}
		return TooltipPanelManager.TooltipBoneSource.TOP_RIGHT;
	}

	private void OnCatchupCardRolloverEvent(UIEvent e)
	{
		GameObject hoveredGameObject = e.GetElement().gameObject;
		Hearthstone.UI.Card hoveredCard = hoveredGameObject.GetComponentInChildren<Hearthstone.UI.Card>();
		if (!(hoveredCard != null))
		{
			return;
		}
		Actor cardActor = hoveredCard.CardActor;
		if (!(cardActor != null) || cardActor.GetActorStateType() != ActorStateType.CARD_IDLE)
		{
			return;
		}
		cardActor.SetActorState(ActorStateType.CARD_HISTORY);
		Vector3 elementTransformNewPos = hoveredGameObject.transform.position + CATCHUP_PACK_HIGHLIGHTED_CARD_OFFSET;
		hoveredGameObject.transform.position = elementTransformNewPos;
		if (!m_cardOriginalTransformValues.ContainsKey(hoveredGameObject))
		{
			m_cardOriginalTransformValues.Add(hoveredGameObject, new OriginalTransformValues
			{
				m_highlightedCardOriginalEulerAngles = hoveredGameObject.transform.eulerAngles,
				m_highlightedCardOriginalScale = hoveredGameObject.transform.localScale
			});
		}
		OriginalTransformValues transformValues = m_cardOriginalTransformValues[hoveredGameObject];
		iTween.ScaleTo(hoveredGameObject, iTween.Hash("scale", transformValues.m_highlightedCardOriginalScale * m_catchupPackHighlightedCardScaleUpFactor, "time", m_catchupPackHighlightedCardScaleAnimationTime, "oncomplete", (Action<object>)delegate
		{
			if (cardActor.GetActorStateType() == ActorStateType.CARD_HISTORY)
			{
				TooltipPanelManager.TooltipBoneSource focusedCardTooltipBone = GetFocusedCardTooltipBone(cardActor);
				TooltipPanelManager.Get().UpdateKeywordHelpForPackOpening(cardActor.GetEntityDef(), cardActor, focusedCardTooltipBone);
			}
		}));
		iTween.RotateTo(hoveredGameObject, Vector3.zero, m_catchupPackHighlightedCardScaleAnimationTime);
		floatyObj floatyObjComponent = hoveredGameObject.GetComponent<floatyObj>();
		if (floatyObjComponent != null)
		{
			floatyObjComponent.Enabled = true;
		}
		SoundManager.Get().LoadAndPlay(m_onCatchupCardRolloverSound);
		ApplySettingsToRenderers(hoveredGameObject.transform, GameLayer.IgnoreFullScreenEffects, 1u);
	}

	private void OnCatchupCardRolloutEvent(UIEvent e)
	{
		GameObject hoveredGameObject = e.GetElement().gameObject;
		Hearthstone.UI.Card hoveredCard = hoveredGameObject.GetComponentInChildren<Hearthstone.UI.Card>();
		if (!(hoveredCard != null))
		{
			return;
		}
		Actor cardActor = hoveredCard.CardActor;
		if (cardActor != null && cardActor.GetActorStateType() == ActorStateType.CARD_HISTORY)
		{
			iTween.Stop(hoveredGameObject);
			cardActor.SetActorState(ActorStateType.CARD_IDLE);
			TooltipPanelManager.Get().HideKeywordHelp();
			Vector3 elementTransformNewPos = hoveredGameObject.transform.position - CATCHUP_PACK_HIGHLIGHTED_CARD_OFFSET;
			hoveredGameObject.transform.position = elementTransformNewPos;
			OriginalTransformValues transformValues = m_cardOriginalTransformValues[hoveredGameObject];
			iTween.ScaleTo(hoveredGameObject, iTween.Hash("scale", transformValues.m_highlightedCardOriginalScale, "time", m_catchupPackHighlightedCardScaleAnimationTime));
			iTween.RotateTo(hoveredGameObject, transformValues.m_highlightedCardOriginalEulerAngles, m_catchupPackHighlightedCardScaleAnimationTime);
			floatyObj floatyObjComponent = hoveredGameObject.GetComponent<floatyObj>();
			if (floatyObjComponent != null)
			{
				floatyObjComponent.Enabled = false;
			}
			ApplySettingsToRenderers(hoveredGameObject.transform, GameLayer.Default, m_catchupPackCardPileCustomPass.renderLayerMaskOverride);
		}
	}

	private CardListDataModel CreateCatchupPackCardListDataModelAndSendSocialToasts(List<NetCache.BoosterCard> cards)
	{
		CardListDataModel cardList = new CardListDataModel();
		foreach (NetCache.BoosterCard currentCard in cards)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(currentCard.Def.Name);
			if (entityDef.GetRarity() == TAG_RARITY.LEGENDARY)
			{
				if (currentCard.Def.Premium == TAG_PREMIUM.GOLDEN)
				{
					BnetPresenceMgr.Get().SetGameField(4u, entityDef.GetCardId() + ",1");
				}
				else
				{
					BnetPresenceMgr.Get().SetGameField(4u, entityDef.GetCardId() + ",0");
				}
			}
			CardDataModel currentCardDataModel = new CardDataModel
			{
				CardId = currentCard.Def.Name,
				Premium = currentCard.Def.Premium,
				Rarity = GameStrings.Get(GameStrings.GetRarityTextKey(entityDef.GetRarity()))
			};
			cardList.Cards.Add(currentCardDataModel);
		}
		return cardList;
	}

	private void SetupCatchupPackCardsFalling(List<NetCache.BoosterCard> cards)
	{
		if (m_catchupPackPileWidget != null)
		{
			CardListDataModel cardList = CreateCatchupPackCardListDataModelAndSendSocialToasts(cards);
			m_catchupPackPileWidget.BindDataModel(cardList);
			m_catchupPackPileWidget.Show();
			m_catchupPackPileWidget.TriggerEvent("START_FALL");
			CustomViewEntryPoint entryPoint = CustomViewEntryPoint.PerspectivePostFullscreenFX;
			GameLayer targetLayer = GameLayer.Default;
			if (m_catchupPackCardPileCustomPass == null)
			{
				m_catchupPackCardPileCustomPass = CameraPassProvider.RequestPass("CatchupPackCardPile", 1 << (int)targetLayer, entryPoint);
			}
			if (!m_catchupPackCardPileCustomPass.isScheduled)
			{
				m_catchupPackCardPileCustomPass.Schedule(entryPoint);
			}
			LayerUtils.SetLayer(m_catchupPackPileWidget.gameObject, GameLayer.IgnoreFullScreenEffects);
		}
	}

	private void SetupCatchupPackCarousel(List<NetCache.BoosterCard> cards)
	{
		if (!(m_catchupPackCarouselWidgetInstance != null))
		{
			return;
		}
		m_catchupCarouselWidgetComponent = m_catchupPackCarouselWidgetInstance.gameObject.GetComponentInChildren<CatchupCarouselWidget>();
		CardListDataModel cardList = CreateCatchupPackCardListDataModelAndSendSocialToasts(cards);
		m_catchupPackCarouselWidgetInstance.BindDataModel(cardList);
		if (!(m_catchupCarouselWidgetComponent != null))
		{
			return;
		}
		Carousel catchupPackCarousel = m_catchupCarouselWidgetComponent.Carousel;
		if (catchupPackCarousel != null)
		{
			m_Carousel = catchupPackCarousel;
			m_hiddenCards = m_catchupPackCarouselHiddenCards;
			m_catchupPackCarouselHiddenCards.OnCardRevealedEvent += OnCardRevealed;
			m_catchupPackCarouselHiddenCards.OnCardSpellFinishedEvent += OnHiddenCardSpellFinished;
			m_catchupPackCarouselHiddenCards.OnCardSpellStateFinishedEvent += OnHiddenCardSpellStateFinished;
			m_HiddenCard.gameObject.SetActive(value: true);
			List<NetCache.BoosterCard> cardsToUse = cards.GetRange(0, Math.Min(cards.Count, m_MaxCardsShownInCatchupCarousel));
			catchupPackCarousel.m_maxPosition = cardsToUse.Count - 1;
			m_catchupPackCarouselHiddenCards.InitializeCards(m_HiddenCard, cardsToUse.Count);
			m_catchupPackCarouselHiddenCards.SetCardsPosition(new Vector3(9000f, 9000f, 9000f), 5);
			if (m_catchupCarouselWidgetComponent != null && m_catchupCarouselWidgetComponent.m_extraCardsRootGameObject != null)
			{
				m_catchupCarouselWidgetComponent.m_extraCardsRootGameObject.SetActive(value: false);
			}
			LayerUtils.SetLayer(m_catchupPackCarouselWidgetInstance, GameLayer.IgnoreFullScreenEffects);
			StartCoroutine(m_catchupPackCarouselHiddenCards.AttachBoosterCards(cardsToUse));
			m_catchupPackCarouselWidgetInstance.Show();
			m_catchupPackCarouselWidgetInstance.TriggerEvent("SETUP");
		}
	}

	private void SetupCatchupPackExpansionCollectionStats(List<NetCache.BoosterCard> cards)
	{
		List<CollectibleCard> collectibleCards = new List<CollectibleCard>();
		foreach (NetCache.BoosterCard boosterCard in cards)
		{
			collectibleCards.Add(CollectionManager.Get().GetCard(boosterCard.Def.Name, boosterCard.Def.Premium));
		}
		List<ExpansionCollectionStats> list = Network.Get().ExpansionCollectionStats();
		ExpansionCollectionStatsDataModel expansionCollectionDataModel = new ExpansionCollectionStatsDataModel();
		foreach (ExpansionCollectionStats expansionStats in list)
		{
			float collectionCompletion = (float)expansionStats.CardsInCollection / (float)expansionStats.CardsInExpansion * 100f;
			int numNewCardsThisExpansion = collectibleCards.Count((CollectibleCard card) => card.SeenCount < 1 && card.OwnedCount < 2 && card.Set == (TAG_CARD_SET)expansionStats.CardSetId);
			string currentExpansionString = "";
			string formattedCollectionCompletion = collectionCompletion.ToString("0.0");
			string cardSetName = GameStrings.GetCardSetNameShortened((TAG_CARD_SET)expansionStats.CardSetId);
			currentExpansionString = ((numNewCardsThisExpansion != 0) ? GameStrings.Format("GLUE_CATCHUP_SUMMARY_EXPANSION_COLLECTION_STATS_WITH_NEW_CARDS", cardSetName, formattedCollectionCompletion, numNewCardsThisExpansion) : GameStrings.Format("GLUE_CATCHUP_SUMMARY_EXPANSION_COLLECTION_STATS", cardSetName, formattedCollectionCompletion));
			expansionCollectionDataModel.ExpansionCollectionString.Add(currentExpansionString);
		}
		if (m_catchupPackPileWidget != null)
		{
			m_catchupPackPileWidget.BindDataModel(expansionCollectionDataModel);
		}
		if (m_catchupPackCarouselWidgetInstance != null)
		{
			m_catchupPackCarouselWidgetInstance.BindDataModel(expansionCollectionDataModel);
		}
	}

	public void FinishCatchupPackOpen()
	{
		if (m_catchupPackCardPileCustomPass != null)
		{
			m_catchupPackCardPileCustomPass.Unschedule();
			CameraPassProvider.ReleasePass(m_catchupPackCardPileCustomPass);
			m_catchupPackCardPileCustomPass = null;
		}
		this.OnFinishedEvent?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnEffectFinished()
	{
		m_effectsPendingFinish--;
		if (m_effectsPendingFinish <= 0)
		{
			this.OnFinishedEvent?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnEffectDone()
	{
		m_effectsPendingDestroy--;
		if (m_effectsPendingDestroy <= 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void DeactivateCarousel()
	{
		if (m_Carousel != null)
		{
			m_Carousel.gameObject.SetActive(value: false);
			m_Carousel = null;
		}
	}
}
