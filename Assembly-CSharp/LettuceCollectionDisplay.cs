using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.Util;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class LettuceCollectionDisplay : CollectibleDisplay
{
	public interface ITeamCopyingModule
	{
		void CheckClipboardAndPromptPlayerToPaste();
	}

	public class TeamCopyingModule : ITeamCopyingModule
	{
		public struct TeamFill
		{
			public EntityDef m_addCard;

			public LettuceMercenary.Loadout m_addLoadout;

			public string m_reason;
		}

		private LettuceCollectionDisplay m_display;

		private ShareableMercenariesTeam m_cachedShareableTeam;

		private bool IsInEditingMode => CollectionManager.Get().IsInEditTeamMode();

		private bool IsCorrectMode => SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_COLLECTION;

		private LettuceTeam EditedTeam => CollectionManager.Get().GetEditingTeam();

		public TeamCopyingModule(LettuceCollectionDisplay display)
		{
			m_display = display;
		}

		public void CheckClipboardAndPromptPlayerToPaste()
		{
			if (!CheckIfClipboardNotificationHasBeenShown() || m_display.m_mercenaryDetailDisplay.DisplayVisible)
			{
				return;
			}
			if (!TryCacheClipboardDataAndGetValidityMessaging(out var alertMessage))
			{
				if (alertMessage != string.Empty)
				{
					AlertPlayerOnInvalidPaste(alertMessage);
				}
				return;
			}
			string popupBodyMessage = GameStrings.Get("GLUE_COLLECTION_TEAM_VALID_PASTE_BODY");
			string popupBodyHeader = GameStrings.Get("GLUE_COLLECTION_TEAM_VALID_PASTE_HEADER");
			if (IsInEditingMode && EditedTeam.GetMercCount() > 0)
			{
				popupBodyMessage = GameStrings.Get("GLUE_COLLECTION_TEAM_OVERWRITE_BODY");
				popupBodyHeader = GameStrings.Get("GLUE_COLLECTION_TEAM_OVERWRITE_HEADER");
			}
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
			{
				m_headerText = popupBodyHeader,
				m_text = popupBodyMessage,
				m_cancelText = GameStrings.Get("GLUE_COLLECTION_TEAM_SAVE_ANYWAY"),
				m_confirmText = GameStrings.Get("GLUE_COLLECTION_TEAM_FINISH_FOR_ME"),
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_responseCallback = delegate(AlertPopup.Response response, object userData)
				{
					if (response == AlertPopup.Response.CANCEL)
					{
						RejectTeamFromClipboard();
					}
					else
					{
						CreateOrUpdateExistingTeamFromClipboard(m_cachedShareableTeam);
					}
				}
			};
			DialogManager.Get().ShowPopup(popup);
		}

		private bool CheckIfClipboardNotificationHasBeenShown()
		{
			if (PlatformSettings.OS == OSCategory.iOS && !Options.Get().GetBool(Option.HAS_SEEN_CLIPBOARD_NOTIFICATION, defaultVal: false))
			{
				DialogManager dialogMgr = DialogManager.Get();
				if (dialogMgr.ShowingDialog())
				{
					return false;
				}
				AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_COLLECTION_TEAM_CLIPBOARD_ACCESS_HEADER"),
					m_text = GameStrings.Get("GLUE_COLLECTION_TEAM_CLIPBOARD_ACCESS_BODY"),
					m_showAlertIcon = false,
					m_responseCallback = delegate
					{
						Options.Get().SetBool(Option.HAS_SEEN_CLIPBOARD_NOTIFICATION, val: true);
					}
				};
				dialogMgr.ShowPopup(popup);
				return false;
			}
			return true;
		}

		private bool TryCacheClipboardDataAndGetValidityMessaging(out string message)
		{
			message = string.Empty;
			ShareableMercenariesTeam teamInClipboard = ShareableMercenariesTeam.DeserializeFromClipboard();
			if (teamInClipboard == null)
			{
				return false;
			}
			DialogManager dialogMgr = DialogManager.Get();
			if (dialogMgr.ShowingDialog())
			{
				if ((m_cachedShareableTeam != null && m_cachedShareableTeam.Equals(teamInClipboard)) || !CanPasteShareableTeam(teamInClipboard))
				{
					return false;
				}
				dialogMgr.ClearAllImmediately();
			}
			if (!CanPasteShareableTeam(teamInClipboard, out message))
			{
				return false;
			}
			m_cachedShareableTeam = teamInClipboard;
			return true;
		}

		private bool CanPasteShareableTeam(ShareableMercenariesTeam shareableTeam)
		{
			string alertMessage;
			return CanPasteShareableTeam(shareableTeam, out alertMessage);
		}

		private bool CanPasteShareableTeam(ShareableMercenariesTeam shareableTeam, out string alertMessage)
		{
			alertMessage = string.Empty;
			if (IsCorrectMode && !IsInEditingMode && !CollectionDeckTray.Get().GetTeamsContent().CanShowNewTeamButton())
			{
				return false;
			}
			if (CraftingTray.Get() != null && CraftingTray.Get().IsShown())
			{
				return false;
			}
			return true;
		}

		private void CreateOrUpdateExistingTeamFromClipboard(ShareableMercenariesTeam shareableTeam)
		{
			if (!IsInEditingMode)
			{
				CollectionManager collectionManager = CollectionManager.Get();
				collectionManager.RegisterTeamCreatedListener(OnTeamCreatedFromClipboard);
				collectionManager.RemoveTeamCreatedListener(m_display.OnTeamCreatedByPlayer);
				CollectionDeckTray.Get().GetTeamsContent().CreateNewTeam(shareableTeam.Team.Name, shareableTeam.Serialize(includeComments: false));
			}
			else
			{
				OnTeamCreatedFromClipboard(EditedTeam.ID);
			}
		}

		private void OnTeamCreatedFromClipboard(long teamId)
		{
			CollectionManager collectionManager = CollectionManager.Get();
			collectionManager.RemoveTeamCreatedListener(OnTeamCreatedFromClipboard);
			collectionManager.RegisterTeamCreatedListener(m_display.OnTeamCreatedByPlayer);
			CollectionDeckTray deckTray = CollectionDeckTray.Get();
			if (deckTray.GetCurrentContentType() != DeckTray.DeckContentTypes.Mercs)
			{
				deckTray.RegisterModeSwitchedListener(OnCollectionDeckTrayModeSwitched);
				m_display.ShowTeam(teamId, isNewTeam: true, null);
			}
			else
			{
				PasteContentsIntoDeckTray();
			}
		}

		private void OnCollectionDeckTrayModeSwitched()
		{
			CollectionDeckTray.Get().UnregisterModeSwitchedListener(OnCollectionDeckTrayModeSwitched);
			PasteContentsIntoDeckTray();
		}

		private void PasteContentsIntoDeckTray()
		{
			if (m_cachedShareableTeam != null)
			{
				PasteTeamInEditModeFromShareableTeamInternal(m_cachedShareableTeam);
			}
			else
			{
				ShareableMercenariesTeam shareableTeam = ShareableMercenariesTeam.DeserializeFromClipboard();
				if (shareableTeam == null)
				{
					return;
				}
				PasteTeamInEditModeFromShareableTeamInternal(shareableTeam);
			}
			ClipboardUtils.CopyToClipboard(string.Empty);
			m_cachedShareableTeam = null;
		}

		private void RejectTeamFromClipboard()
		{
			ClipboardUtils.CopyToClipboard(string.Empty);
			m_cachedShareableTeam = null;
		}

		private void PasteTeamInEditModeFromShareableTeamInternal(ShareableMercenariesTeam shareableTeam)
		{
			if (!IsInEditingMode)
			{
				Debug.LogError("Error trying to paste team. Collection Manager is not in edit mode.");
			}
			else
			{
				if (m_display.GetMercenaryDetailsDisplay().DisplayVisible)
				{
					return;
				}
				LettuceTeam editedTeam = EditedTeam;
				CollectionDeckTray deckTray = CollectionDeckTray.Get();
				CollectionManager collectionMgr = CollectionManager.Get();
				string teamName = shareableTeam.Team.Name;
				if (!string.IsNullOrEmpty(teamName))
				{
					editedTeam.Name = teamName;
					deckTray.GetTeamsContent().UpdateTeamName(teamName);
				}
				DefLoader defLoader = DefLoader.Get();
				List<TeamFill> pastedTeamFill = new List<TeamFill>();
				foreach (LettuceMercenary sourceMerc in shareableTeam.Team.GetMercs())
				{
					LettuceMercenary targetMerc = collectionMgr.GetMercenary(sourceMerc.ID);
					if (targetMerc == null || !targetMerc.m_owned)
					{
						continue;
					}
					LettuceMercenary.Loadout sourceMercLoadout = shareableTeam.Team.GetLoadout(sourceMerc);
					LettuceMercenary.Loadout targetMercLoadout = new LettuceMercenary.Loadout();
					if (sourceMercLoadout.m_equipmentRecord != null)
					{
						List<LettuceEquipmentTierDbfRecord> tiers = sourceMercLoadout.m_equipmentRecord.LettuceEquipmentTiers;
						for (int i = tiers.Count - 1; i >= 0; i--)
						{
							LettuceEquipmentTierDbfRecord tier = tiers[i];
							LettuceEquipmentDbfRecord fallbackEquipment = GameDbf.LettuceEquipment.GetRecord(tier.LettuceEquipmentId);
							if (targetMerc.CanSlotEquipment(fallbackEquipment.ID))
							{
								targetMercLoadout.m_equipmentRecord = fallbackEquipment;
								break;
							}
						}
					}
					if (sourceMercLoadout.m_artVariationRecord != null && targetMerc.IsArtVariationUnlocked(sourceMercLoadout.m_artVariationRecord.ID, sourceMercLoadout.m_artVariationPremium))
					{
						targetMercLoadout.m_artVariationRecord = sourceMercLoadout.m_artVariationRecord;
						targetMercLoadout.m_artVariationPremium = sourceMercLoadout.m_artVariationPremium;
					}
					else
					{
						LettuceMercenary.ArtVariation defaultArtVariation = targetMerc.GetDefaultOrFirstAvailableArtVariation();
						targetMercLoadout.m_artVariationRecord = defaultArtVariation.m_record;
						targetMercLoadout.m_artVariationPremium = defaultArtVariation.m_premium;
					}
					pastedTeamFill.Add(new TeamFill
					{
						m_addCard = defLoader.GetEntityDef(targetMerc.GetCardId()),
						m_addLoadout = targetMercLoadout
					});
				}
				deckTray.PopulateTeam(pastedTeamFill, null);
			}
		}

		private void AlertPlayerOnInvalidPaste(string errorReason)
		{
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_COLLECTION_TEAM_INVALID_POPUP_HEADER"),
				m_text = errorReason,
				m_okText = GameStrings.Get("GLOBAL_OKAY"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(popup);
		}
	}

	[CustomEditField(Sections = "Bones")]
	public Transform m_setFilterTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_showMercDetailsTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_hoverCardTopBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_hoverCardBottomBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_showMaxedOutTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_filterTrayTutorialBone;

	[CustomEditField(Sections = "Objects")]
	public LettuceCollectionPageManager m_pageManager;

	[CustomEditField(Sections = "Objects")]
	public NestedPrefab m_setFilterTrayContainer;

	[CustomEditField(Sections = "Objects")]
	public TooltipZone m_tooltipZone;

	[CustomEditField(Sections = "Objects")]
	public PositionTweenerComponent[] m_tuckTweens;

	[CustomEditField(Sections = "Objects")]
	public SlidingTray m_filterSlidingTray;

	[CustomEditField(Sections = "Controls")]
	public Texture m_allSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_allSetsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_wildSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_wildSetsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_featuredCardsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_featuredCardsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public float m_deckTrayAbilitySlotTooltipXOffset;

	[CustomEditField(Sections = "Settings")]
	public float m_secondsDelayBeforeTutorialPopups = 1f;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercDetailsDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercHoverCardReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_abilityHoverCardReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercsPopupReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_campfireButtonReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_maxedoutFilterButtonReference;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_collectionSpinnerPrefab;

	private Map<TAG_CLASS, Texture> m_loadedClassTextures = new Map<TAG_CLASS, Texture>();

	private Map<TAG_CLASS, TextureRequests> m_requestedClassTextures = new Map<TAG_CLASS, TextureRequests>();

	private long m_showTeamContentsRequest;

	private Notification m_helpPopup;

	private Notification m_innkeeperLClickReminder;

	private List<FilterStateListener> m_setFilterListeners = new List<FilterStateListener>();

	private Notification m_setFilterTutorialPopup;

	private IEnumerator m_showSetFilterTutorialCoroutine;

	private EventTimingType m_currentActiveFeaturedCardsEvent;

	private bool m_mercDetailsDisplayFinishedLoading;

	private MercenaryDetailDisplay m_mercenaryDetailDisplay;

	private Widget m_mercHoverCard;

	private Widget m_abilityHoverCard;

	private MercenaryCraftingPopup m_mercCraftingPopup;

	private bool m_catalogueVisible;

	private Coroutine m_ShowCollectionTipsCoroutine;

	private Coroutine m_ShowCampfireButtonCoroutine;

	private Coroutine m_ShowMaxedoutFilterButtonCoroutine;

	private Widget m_campfireButton;

	private Widget m_maxedoutFilterButton;

	private bool m_isExiting;

	private HashSet<string> m_blockTooltipRequests = new HashSet<string>();

	private CollectionManagerSpinnerPopup m_collectionSpinner;

	private static readonly string CAMPFIRE_CLICKED_EVENT = "Campfire_Button_Clicked";

	private static readonly string MAXEDOUT_FILTER_CLICKED_EVENT = "Maxedout_Filter_Button_Clicked";

	private static readonly string CAMPFIRE_BUTTON_SHOW_EVENT = "SHOW";

	private static readonly string MAXEDOUT_FILTER_BUTTON_SHOW_EVENT = "SHOW";

	private static readonly string MAXEDOUT_FILTER_BUTTON_TOGGLE_ON = "TOGGLE_ON";

	private static readonly string MAXEDOUT_FILTER_BUTTON_TOGGLE_OFF = "TOGGLE_OFF";

	private bool IsCollectionTipsBlocked => m_blockTooltipRequests.Count > 0;

	public ITeamCopyingModule TeamCopying { get; private set; }

	public override void Start()
	{
		NetCache.Get().RegisterScreenCollectionManager(OnNetCacheReady);
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		CollectionManager.Get().RegisterCollectionNetHandlers();
		CollectionManager.Get().RegisterCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RegisterCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RegisterTeamCreatedListener(OnTeamCreatedByPlayer);
		CollectionManager.Get().RegisterTeamContentsListener(OnTeamContents);
		CollectionManager.Get().RegisterNewCardSeenListener(OnNewCardSeen);
		CollectionManager.Get().RegisterCardRewardsInsertedListener(OnCardRewardsInserted);
		CollectionManager.Get().MercenaryArtVariationChangedEvent += OnMercArtVariationChanged;
		CardBackManager.Get().SetSearchText(null);
		m_mercDetailsDisplayReference.RegisterReadyListener<Widget>(OnMercDetailsDisplayReady);
		m_mercHoverCardReference.RegisterReadyListener(delegate(Widget w)
		{
			m_mercHoverCard = w;
		});
		m_abilityHoverCardReference.RegisterReadyListener(delegate(Widget w)
		{
			m_abilityHoverCard = w;
		});
		m_mercsPopupReference.RegisterReadyListener(delegate(Widget w)
		{
			m_mercCraftingPopup = w.GetComponentInChildren<MercenaryCraftingPopup>();
		});
		m_campfireButtonReference.RegisterReadyListener(delegate(Widget w)
		{
			m_campfireButton = w;
			m_campfireButton?.RegisterEventListener(OnCampfireButtonEvent);
		});
		m_maxedoutFilterButtonReference.RegisterReadyListener(delegate(Widget w)
		{
			m_maxedoutFilterButton = w;
			m_maxedoutFilterButton?.RegisterEventListener(OnMaxedoutFilterButtonEvent);
		});
		base.Start();
		bool showAdvancedCM = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false);
		ShowAdvancedCollectionManager(showAdvancedCM);
		if (!showAdvancedCM)
		{
			Options.Get().RegisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		}
		DoEnterCollectionManagerEvents();
		if (CollectionManager.Get().ShouldShowWildToStandardTutorial())
		{
			UserAttentionManager.StartBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
		}
		CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded();
		StartCoroutine(WaitUntilReady());
		if (RegionUtils.IsCNLegalRegion)
		{
			GameObject obj = AssetLoader.Get().InstantiatePrefab(m_collectionSpinnerPrefab);
			m_collectionSpinner = ((obj != null) ? obj.GetComponent<CollectionManagerSpinnerPopup>() : null);
			if (m_collectionSpinner == null)
			{
				Log.CollectionManager.Print("Unable to create collection spinner");
				return;
			}
			CollectionManager.Get().RegisterRenameFinishedListener(OnRenameFinished);
			CollectionManager.Get().RegisterRenameValidatedListener(OnRenameValidated);
		}
	}

	private void OnRenameFinished()
	{
		if (!(m_collectionSpinner == null))
		{
			m_collectionSpinner.Show(null);
		}
	}

	private void OnRenameValidated(bool successful)
	{
		if (!(m_collectionSpinner == null))
		{
			m_collectionSpinner.UpdateSuccessOrFail(successful);
		}
	}

	protected override void Awake()
	{
		TeamCopying = new TeamCopyingModule(this);
		if (ServiceManager.Get<IGraphicsManager>().RenderQualityLevel != 0 && PlatformSettings.Memory == MemoryCategory.High && m_cover == null)
		{
			m_isBookCoverLoading = true;
			AssetLoader.Get().InstantiatePrefab("MercenariesBookCover.prefab:a9002069cee6a9a47beb0d2687aa83c5", OnBookCoverLoaded);
		}
		base.Awake();
		StartCoroutine(InitCollectionWhenReady());
	}

	protected override void OnDestroy()
	{
		UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
		base.OnDestroy();
	}

	public override CollectiblePageManager GetPageManager()
	{
		return m_pageManager;
	}

	public override void Unload()
	{
		m_unloading = true;
		UnloadAllTextures();
		CollectionDeckTray.Get().GetCardsContent().UnregisterCardTileRightClickedListener(OnCardTileRightClicked);
		CollectionDeckTray.Get().Unload();
		CollectionInputMgr.Get().Unload();
		m_mercenaryDetailDisplay.Unload();
		CollectionManager.Get().MercenaryArtVariationChangedEvent -= OnMercArtVariationChanged;
		CollectionManager.Get().RemoveCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RemoveCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RemoveTeamCreatedListener(OnTeamCreatedByPlayer);
		CollectionManager.Get().RemoveTeamContentsListener(OnTeamContents);
		CollectionManager.Get().RemoveNewCardSeenListener(OnNewCardSeen);
		CollectionManager.Get().RemoveCardRewardsInsertedListener(OnCardRewardsInserted);
		CollectionManager.Get().RemoveCollectionNetHandlers();
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		m_unloading = false;
	}

	public override void Exit()
	{
		m_isExiting = true;
		EnableInput(enable: false);
		NotificationManager.Get().DestroyAllPopUps();
		if (m_pageManager != null)
		{
			m_pageManager.Exit();
		}
		SceneMgr.Mode nextMode = SceneMgr.Get().GetPrevMode();
		if (!Network.IsLoggedIn() && nextMode != SceneMgr.Mode.HUB)
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			nextMode = SceneMgr.Mode.HUB;
			Navigation.Clear();
		}
		SceneMgr.TransitionHandlerType handler = SceneMgr.TransitionHandlerType.NEXT_SCENE;
		if (nextMode == SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			handler = SceneMgr.TransitionHandlerType.CURRENT_SCENE;
		}
		SetNextModeAndHandleTransition(nextMode, handler, SceneMgr.Get().GetScene().GetSceneTransitionPayload());
		LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupShown));
		LettuceVillagePopupManager lettuceVillagePopupManager2 = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(lettuceVillagePopupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupClosed));
		if (m_filterSlidingTray != null)
		{
			m_filterSlidingTray.OnTransitionComplete -= OnFilterSlidingTrayTransitionCompleted;
		}
	}

	protected override void OnBookCoverLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_isBookCoverLoading = false;
		if (m_root != null)
		{
			go.transform.SetParent(m_root.transform, worldPositionStays: false);
		}
		m_cover = go.GetComponent<CollectionCoverDisplay>();
		m_cover.DisplayCover();
	}

	public override void CollectionPageContentsChanged<TCollectible>(ICollection<TCollectible> collectiblesToDisplay, CollectionActorsReadyCallback callback, object callbackData)
	{
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1}", m_pageManager.GetTransitionPageId(), m_pageManager.ArePagesTurning());
		bool displayEmptyPage = false;
		if (collectiblesToDisplay == null)
		{
			Log.CollectionManager.Print("collectiblesToDisplay is null!");
			displayEmptyPage = true;
		}
		else if (collectiblesToDisplay.Count == 0)
		{
			Log.CollectionManager.Print("collectiblesToDisplay has a count of 0!");
			displayEmptyPage = true;
		}
		if (displayEmptyPage)
		{
			callback(new List<CollectionCardActors>(), new List<ICollectible>(), callbackData);
		}
	}

	public void UpdateCollectionMercenary(LettuceMercenary merc)
	{
		m_mercenaryDetailDisplay.UpdateMercenaryData(merc);
	}

	public void RequestContentsToShowTeam(long teamID)
	{
		m_showTeamContentsRequest = teamID;
		CollectionManager.Get().RequestTeamContents(m_showTeamContentsRequest);
	}

	public override void SetViewMode(CollectionUtils.ViewMode mode, bool triggerResponse, CollectionUtils.ViewModeData userdata = null)
	{
		Log.CollectionManager.Print("mode={0}-->{1} triggerResponse={2} isUpdatingTrayMode={3}", m_currentViewMode, mode, triggerResponse, CollectionDeckTray.Get().IsUpdatingTrayMode());
		if (m_currentViewMode != mode && ((mode != CollectionUtils.ViewMode.HERO_SKINS && mode != CollectionUtils.ViewMode.CARD_BACKS) || !CollectionDeckTray.Get().IsUpdatingTrayMode()))
		{
			CollectionUtils.ViewMode prevMode = m_currentViewMode;
			m_currentViewMode = mode;
			OnSwitchViewModeResponse(triggerResponse, prevMode, mode, userdata);
		}
	}

	public void OnDoneEditingTeam()
	{
		ShowAppropriateSetFilters();
		m_pageManager.OnDoneEditingTeam();
	}

	public override void FilterBySearchText(string newSearchText)
	{
		string oldSearchText = m_search.GetText();
		base.FilterBySearchText(newSearchText);
		OnSearchDeactivated_Internal(oldSearchText, newSearchText, updateManaFilterToMatchSearchText: true);
	}

	public override void HideAllTips()
	{
		if (m_innkeeperLClickReminder != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_innkeeperLClickReminder);
		}
		HideHelpPopups();
	}

	public void HideHelpPopups()
	{
		if (m_ShowCollectionTipsCoroutine != null)
		{
			StopCoroutine(m_ShowCollectionTipsCoroutine);
			m_ShowCollectionTipsCoroutine = null;
		}
		if (m_helpPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_helpPopup);
		}
	}

	public override void ShowInnkeeperLClickHelp(EntityDef entityDef)
	{
		bool isHeroSkin = entityDef?.IsHeroSkin() ?? false;
		ShowInnkeeperLClickHelp(isHeroSkin);
	}

	private void ShowInnkeeperLClickHelp(bool isHero)
	{
		if (!CollectionDeckTray.Get().IsShowingDeckContents())
		{
			if (isHero)
			{
				m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_LCLICK_HERO"), "", 3f);
			}
			else
			{
				m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_LCLICK"), "", 3f);
			}
		}
	}

	private void FeaturedCardsSetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, FormatType formatType, SetFilterItem item, bool transitionPage)
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_ITEM);
		item.SetIconFxActive(active: false);
		SetFilterCallback(cardSets, specificCards, formatType, item, transitionPage);
	}

	public override void SetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, FormatType formatType, SetFilterItem item, bool transitionPage)
	{
		if (formatType != FormatType.FT_STANDARD)
		{
			Log.CollectionManager.PrintWarning("LettuceCollectionDisplay only supports the Standard format, please add support to the class for other formats if needed.");
		}
		m_search.SetWildModeActive(active: false);
		ShowSetFilterCards(cardSets, specificCards, transitionPage);
	}

	private void ShowSetFilterCards(List<TAG_CARD_SET> cardSets, List<int> specificCards, bool transitionPage = true)
	{
		if (specificCards != null)
		{
			ShowSpecificCards(specificCards);
		}
		else
		{
			ShowSets(cardSets, transitionPage);
		}
	}

	private void ShowSets(List<TAG_CARD_SET> cardSets, bool transitionPage = true)
	{
		m_pageManager.FilterByCardSets(cardSets, transitionPage);
		NotifyFilterUpdate(m_setFilterListeners, cardSets != null, null);
	}

	protected override void ShowSpecificCards(List<int> specificCards)
	{
		base.ShowSpecificCards(specificCards);
		NotifyFilterUpdate(m_setFilterListeners, specificCards != null, null);
	}

	public override void ResetFilters(bool updateVisuals = true)
	{
		base.ResetFilters(updateVisuals);
		if (m_setFilterTray != null)
		{
			m_setFilterTray.ClearFilter();
		}
	}

	public void ShowAppropriateSetFilters()
	{
		bool inCraftingMode = InCraftingMode();
		if (CollectionManager.Get().IsInEditMode())
		{
			CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
			if (deck != null)
			{
				_ = deck.FormatType == FormatType.FT_WILD;
			}
			else
				_ = 0;
			return;
		}
		bool hasUnlockedAllClasses = GameModeUtils.HasUnlockedAllDefaultHeroes();
		if (RankMgr.Get().WildCardsAllowedInCurrentLeague())
		{
			if (CollectionManager.Get().ShouldAccountSeeStandardWild())
			{
				_ = 1;
			}
			else
				_ = inCraftingMode && hasUnlockedAllClasses;
		}
		else if (!inCraftingMode)
		{
			if (hasUnlockedAllClasses)
			{
				CollectionManager.Get().AccountHasUnlockedWild();
			}
			else
				_ = 0;
		}
	}

	public void UpdateSetFilters(bool showWild, bool editingDeck, bool showUnownedSets = false)
	{
		m_setFilterTray.UpdateSetFilters(showWild ? FormatType.FT_WILD : FormatType.FT_STANDARD, editingDeck, showUnownedSets);
	}

	private void OnCatalogueButtonReleased(UIEvent e)
	{
		bool enable = !m_catalogueVisible;
		EnableCatalogue(enable, (!enable) ? BookPageManager.PageTransitionType.SINGLE_PAGE_RIGHT : BookPageManager.PageTransitionType.SINGLE_PAGE_LEFT);
	}

	public void EnableCatalogue(bool enable, BookPageManager.PageTransitionType? pageTransition = null)
	{
		m_catalogueVisible = enable;
		m_craftingModeButton.ShowActiveGlow(m_catalogueVisible);
		if (enable)
		{
			GetPageManager().ShowCardsNotOwned(includePremiums: true, pageTransition);
		}
		else
		{
			GetPageManager().ShowOnlyCardsIOwn(pageTransition);
		}
	}

	public void ShowMercenaryDetailsDisplay(LettuceMercenary merc)
	{
		if (!(m_mercenaryDetailDisplay == null))
		{
			BlockCollectionTips(isBlocked: true, "MERC_DETAILS");
			CollectionDeckTray.Get()?.GetTeamsContent()?.CancelRenameEditingTeam();
			HideHelpPopups();
			for (int i = 0; i < m_tuckTweens.Length; i++)
			{
				m_tuckTweens[i].PlayForward();
			}
			m_pageManager.PlayTabTuckAnimation(forward: true, animate: true, allowSFX: false);
			m_mercenaryDetailDisplay.Show(merc);
		}
	}

	public void OnReturnFromMercenaryDetailsDisplay()
	{
		BlockCollectionTips(isBlocked: false, "MERC_DETAILS");
		for (int i = 0; i < m_tuckTweens.Length; i++)
		{
			m_tuckTweens[i].PlayReverse();
		}
		m_pageManager.PlayTabTuckAnimation(forward: false, animate: true, allowSFX: false);
		TryShowButtons();
	}

	public void HideMercenaryDetailsDisplay()
	{
		if (!(m_mercenaryDetailDisplay == null) && m_mercenaryDetailDisplay.DisplayVisible)
		{
			m_mercenaryDetailDisplay.Hide();
		}
	}

	public MercenaryDetailDisplay GetMercenaryDetailsDisplay()
	{
		return m_mercenaryDetailDisplay;
	}

	public bool IsMercenaryDetailsDisplayActive()
	{
		if (!(m_mercenaryDetailDisplay != null))
		{
			return false;
		}
		return m_mercenaryDetailDisplay.DisplayVisible;
	}

	public void SlotEquipmentOnActiveMercenary(string cardId)
	{
		m_mercenaryDetailDisplay?.SlotSelectedEquipment(cardId);
	}

	public void HandleTileHoverEvents(string eventName, VisualController vc)
	{
		if (!(eventName == "MERC_OVER_code"))
		{
			if (eventName == "MERC_OUT_code")
			{
				HideMercHoverCard();
			}
		}
		else
		{
			ShowMercHoverCard(vc);
		}
	}

	public void HideHoverCards()
	{
		HideMercHoverCard();
	}

	public void ShowMercCraftingPopup(LettuceMercenaryDataModel mercData)
	{
		if (m_mercCraftingPopup == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionDisplay.ShowMercCraftingPopup - merc crafting popup is null!");
			return;
		}
		CollectionDeckTray.Get()?.GetTeamsContent()?.CancelRenameEditingTeam();
		m_mercCraftingPopup.ShowCraftingPopup(mercData);
	}

	public bool TutorialShouldShowAbilityUpgrade()
	{
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP))
		{
			return !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END);
		}
		return false;
	}

	public bool ShouldShowCampfireButton()
	{
		if (m_campfireButton != null && LettuceVillage.TaskboardIsOkayToShowVisitors())
		{
			return LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_END);
		}
		return false;
	}

	public bool ShouldShowMaxedOutMercsFilter()
	{
		if (m_maxedoutFilterButton != null)
		{
			return CollectionManager.Get().HasFullyUpgradedAnyCollectibleMercenary();
		}
		return false;
	}

	private void OnTeamContents(long teamID)
	{
		if (teamID == m_showTeamContentsRequest)
		{
			m_showTeamContentsRequest = 0L;
			ShowTeam(teamID, isNewTeam: false, null);
		}
		else
		{
			CollectionDeckTray.Get().GetTeamsContent().OnTeamContentsUpdated(teamID);
		}
	}

	private void OnTeamCreatedByPlayer(long teamID)
	{
		ShowTeam(teamID, isNewTeam: true, null);
		TeamCopying.CheckClipboardAndPromptPlayerToPaste();
	}

	private void OnNewCardSeen(string cardID, TAG_PREMIUM premium)
	{
		m_pageManager?.UpdateTabNewCardCounts();
	}

	private void OnCardRewardsInserted(List<string> cardID, List<TAG_PREMIUM> premium)
	{
		m_pageManager?.RefreshCurrentPageContents();
	}

	protected override void OnCollectionChanged()
	{
		m_pageManager?.NotifyOfCollectionChanged();
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded);
		if (m_sceneTransitionPayload != null)
		{
			long teamID = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_TeamId;
			if (teamID > 0 && CollectionManager.Get().GetTeam(teamID) != null)
			{
				RequestContentsToShowTeam(teamID);
			}
		}
	}

	private void OnMercArtVariationChanged(int mercenaryDbId, int artVariationId, TAG_PREMIUM premium)
	{
		LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(mercenaryDbId);
		LettuceCollectionPageManager lcpm = GetPageManager() as LettuceCollectionPageManager;
		if (lcpm != null)
		{
			lcpm.UpdatePageMercenary(MercenaryFactory.CreateMercenaryDataModelWithCoin(mercenary));
			lcpm.UpdateCurrentPageCardLocks(playSound: false);
		}
		CollectionDeckTray.Get().GetTeamsContent().UpdateTeamTrayVisuals(suppressFX: true);
		CollectionDeckTray.Get().GetMercsContent().ChangeMercenaryArtVariation(mercenary.ID, mercenary.GetEquippedArtVariation());
		CollectionUtils.PopulateMercenaryCardDataModel(GetMercenaryDetailsDisplay().GetMercenaryDisplayDataModel(), mercenary.GetEquippedArtVariation());
	}

	protected override bool ShouldStartShown()
	{
		return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_VILLAGE;
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_netCacheReady && Network.IsLoggedIn())
		{
			yield return 0;
		}
		while (!m_mercDetailsDisplayFinishedLoading || m_mercHoverCard == null || m_abilityHoverCard == null)
		{
			yield return 0;
		}
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		collectionDeckTray.Initialize();
		collectionDeckTray.RegisterModeSwitchedListener(delegate
		{
			UpdateCurrentPageCardLocks();
		});
		collectionDeckTray.GetCardsContent().RegisterCardTileRightClickedListener(OnCardTileRightClicked);
		LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager.OnPopupShown = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(lettuceVillagePopupManager.OnPopupShown, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupShown));
		LettuceVillagePopupManager lettuceVillagePopupManager2 = LettuceVillagePopupManager.Get();
		lettuceVillagePopupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(lettuceVillagePopupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnVillagePopupClosed));
		if (m_filterSlidingTray != null)
		{
			m_filterSlidingTray.OnTransitionComplete += OnFilterSlidingTrayTransitionCompleted;
		}
		HideMercHoverCard();
		m_isReady = true;
	}

	private IEnumerator InitCollectionWhenReady()
	{
		if (m_pageManager == null)
		{
			Log.CollectionManager.PrintError("LettuceCollectionDisplay.InitCollectionWhenReady - m_pageManager null!");
			yield break;
		}
		while (!m_pageManager.IsFullyLoaded())
		{
			yield return null;
		}
		m_pageManager.OnCollectionLoaded();
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection.Manager)
		{
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_COLLECTION");
			}
		}
		else
		{
			m_netCacheReady = true;
		}
	}

	private void OnShowAdvancedCMChanged(Option option, object prevValue, bool existed, object userData)
	{
		bool showAdvancedCM = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false);
		if (showAdvancedCM)
		{
			Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		}
		ShowAdvancedCollectionManager(showAdvancedCM);
	}

	private void OnCardTileRightClicked(DeckTrayDeckTileVisual cardTile)
	{
		if (GetViewMode() != CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			if (!cardTile.GetSlot().Owned)
			{
				CraftingManager.Get().EnterCraftMode(cardTile.GetActor());
			}
			GoToPageWithCard(cardTile.GetCardID(), cardTile.GetPremium());
		}
	}

	private void OnMercDetailsDisplayReady(Widget widget)
	{
		m_mercenaryDetailDisplay = widget.GetComponentInChildren<MercenaryDetailDisplay>();
		m_mercDetailsDisplayFinishedLoading = true;
	}

	private void OnCampfireButtonEvent(string eventName)
	{
		if (eventName == CAMPFIRE_CLICKED_EVENT)
		{
			LettuceVillagePopupManager.Get().Show(LettuceVillagePopupManager.PopupType.TASKBOARD);
		}
	}

	private void OnMaxedoutFilterButtonEvent(string eventName)
	{
		if (eventName == MAXEDOUT_FILTER_CLICKED_EVENT)
		{
			bool newValue = !m_pageManager.GetShowOnlyFullyUpgradedMercs();
			SetMaxedoutFilterButtonToggleState(newValue);
			NotifyFilterUpdate(m_searchFilterListeners, newValue, newValue);
			m_pageManager.SetOnlyShowFullyUpgradedMercs(newValue, base.OnSearchFilterComplete, null);
			TryShowCollectionTips();
			if (PlatformSettings.Screen == ScreenCategory.Phone && m_filterSlidingTray != null)
			{
				m_filterSlidingTray.HideTray();
			}
		}
	}

	private void OnVillagePopupShown(LettuceVillagePopupManager.PopupType type)
	{
		if (type != LettuceVillagePopupManager.PopupType.TASKBOARD && type != LettuceVillagePopupManager.PopupType.RENOWNCONVERSION)
		{
			return;
		}
		Box box = Box.Get();
		if (box != null)
		{
			foreach (Collider outerPanelCollider in box.m_outerPanelColliders)
			{
				outerPanelCollider.enabled = false;
			}
		}
		BlockCollectionTips(isBlocked: true, "VILLAGE_POPUP");
	}

	private void OnVillagePopupClosed(LettuceVillagePopupManager.PopupType type)
	{
		if (type != LettuceVillagePopupManager.PopupType.TASKBOARD && type != LettuceVillagePopupManager.PopupType.RENOWNCONVERSION)
		{
			return;
		}
		Box box = Box.Get();
		if (box != null)
		{
			foreach (Collider outerPanelCollider in box.m_outerPanelColliders)
			{
				outerPanelCollider.enabled = true;
			}
		}
		BlockCollectionTips(isBlocked: false, "VILLAGE_POPUP");
		TryShowButtons();
	}

	protected override void LoadAllTextures()
	{
	}

	protected override void UnloadAllTextures()
	{
	}

	private void ShowTeam(long teamID, bool isNewTeam, CollectionUtils.ViewMode? setNewViewMode = null)
	{
		if (CollectionManager.Get().GetTeam(teamID) != null)
		{
			CollectionManager.Get().StartEditingTeam(teamID, isNewTeam);
			CollectionDeckTray.Get().ShowTeam(setNewViewMode ?? GetViewMode());
			CollectionDeckTray.Get().UpdateDoneButtonText();
			if (setNewViewMode.HasValue)
			{
				SetViewMode(setNewViewMode.Value);
			}
		}
	}

	private void ShowHoverCard(Widget widget, IDataModel dataModel)
	{
		if (!CollectionInputMgr.Get().HasHeldCard())
		{
			widget.BindDataModel(dataModel);
			float z = PegUI.Get().GetMousedOverElement().transform.position.z;
			z = Mathf.Clamp(z, m_hoverCardBottomBone.position.z, m_hoverCardTopBone.position.z);
			TransformUtil.SetPosZ(widget.transform, z);
		}
	}

	private void ShowMercHoverCard(VisualController vc)
	{
		if (WidgetUtils.GetEventDataModel(vc).Payload is LettuceMercenaryDataModel mercDataModel)
		{
			LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
			if (!(lcd != null) || !lcd.IsMercenaryDetailsDisplayActive() || lcd.GetMercenaryDetailsDisplay().GetCurrentlyDisplayedMercenary().ID != mercDataModel.MercenaryId)
			{
				ShowHoverCard(m_mercHoverCard, mercDataModel);
			}
		}
	}

	private void HideMercHoverCard()
	{
		TransformUtil.SetPosZ(m_mercHoverCard, 5000f);
	}

	private IEnumerator DoBookOpeningAnimations()
	{
		while (m_isBookCoverLoading)
		{
			yield return null;
		}
		if (m_cover != null)
		{
			m_cover.Open(base.OnCoverOpened);
		}
		else
		{
			OnCoverOpened();
		}
	}

	private IEnumerator SetBookToOpen()
	{
		while (m_isBookCoverLoading)
		{
			yield return null;
		}
		if (m_cover != null)
		{
			m_cover.SetOpenState();
		}
	}

	private void ShowAdvancedCollectionManager(bool show)
	{
		show |= (bool)UniversalInputManager.UsePhoneUI;
		if (m_setFilterTray != null)
		{
			bool showSetFilterButton = show && !UniversalInputManager.UsePhoneUI;
			m_setFilterTray.SetButtonShown(showSetFilterButton);
		}
		if (m_craftingTray == null)
		{
			AssetLoader.Get().LoadGameObject(UniversalInputManager.UsePhoneUI ? "MercenariesCraftingTray_phone.prefab:0bd8ce68ce0ff094ba1786d1c62781ea" : "MercenariesCraftingTray.prefab:d0be6526e15d50a46868c8f503298a0b", OnCraftingTrayLoaded, null, autoInstantiateOnLoad: true, usePrefabPosition: false);
		}
		m_craftingModeButton.gameObject.SetActive(value: true);
		m_craftingModeButton.AddEventListener(UIEventType.RELEASE, OnCatalogueButtonReleased);
		if (m_setFilterTray != null && show && !m_setFilterTrayInitialized)
		{
			m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_ALL_STANDARD_CARDS"), m_allSetsTexture, m_allSetsIconOffset, SetFilterCallback, new List<TAG_CARD_SET>(GameUtils.GetStandardSets()), null, FormatType.FT_STANDARD, isAllStandard: true, tooltipActive: false, GameStrings.Get("GLUE_TOOLTIP_HEADER_ALL_STANDARD_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_ALL_STANDARD_CARDS"));
			m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_ALL_CARDS"), m_wildSetsTexture, m_wildSetsIconOffset, SetFilterCallback, null, null, FormatType.FT_WILD, isAllStandard: false, tooltipActive: false, GameStrings.Get("GLUE_TOOLTIP_HEADER_ALL_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_ALL_CARDS"));
			List<int> featuredCards = CollectionManager.GetFeaturedCards();
			if (featuredCards.Any())
			{
				SetFilterItem updatedCardsItem = m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_FEATURED_CARDS"), m_featuredCardsTexture, m_featuredCardsIconOffset, FeaturedCardsSetFilterCallback, null, featuredCards, FormatType.FT_STANDARD, isAllStandard: false, tooltipActive: false, GameStrings.Get("GLUE_TOOLTIP_HEADER_FEATURED_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_FEATURED_CARDS"));
				m_currentActiveFeaturedCardsEvent = GameDbf.Card.GetRecord(featuredCards.First()).FeaturedCardsEvent;
				StartCoroutine(SetIconFxIfFeaturedCardsEventNotSeen(updatedCardsItem, m_currentActiveFeaturedCardsEvent));
				StartCoroutine(SetFeaturedCardsSetFilterGlowIfNotSeen(m_currentActiveFeaturedCardsEvent));
			}
			m_setFilterTray.AddHeader(GameStrings.Get("GLUE_COLLECTION_STANDARD_SETS"), FormatType.FT_STANDARD);
			AddSetFilters(isWild: false);
			m_setFilterTray.AddHeader(GameStrings.Get("GLUE_COLLECTION_WILD_SETS"), FormatType.FT_WILD);
			AddSetFilters(isWild: true);
			AddSetFilter(TAG_CARD_SET.HOF);
			if (CollectionManager.Get().GetDisplayableCardSets().Contains(TAG_CARD_SET.SLUSH))
			{
				AddSetFilter(TAG_CARD_SET.SLUSH);
			}
			m_setFilterTray.SelectFirstItem();
			m_setFilterTrayInitialized = true;
		}
		else if (!show)
		{
			ShowSets(new List<TAG_CARD_SET>(GameUtils.GetStandardSets()));
		}
		ShowAppropriateSetFilters();
	}

	private void AddSetFilters(bool isWild)
	{
		foreach (TAG_CARD_SET cardSetId2 in from cardSetId in CollectionManager.Get().GetDisplayableCardSets()
			where cardSetId != TAG_CARD_SET.HOF && cardSetId != TAG_CARD_SET.SLUSH && cardSetId != TAG_CARD_SET.NONE && GameUtils.IsSetRotated(cardSetId) == isWild
			orderby GameDbf.GetIndex().GetCardSet(cardSetId)?.ReleaseOrder ?? 0 descending
			select cardSetId)
		{
			AddSetFilter(cardSetId2);
		}
	}

	private void AddSetFilter(TAG_CARD_SET cardSet)
	{
		List<TAG_CARD_SET> sets = new List<TAG_CARD_SET>();
		sets.Add(cardSet);
		string iconTextureAssetRef = null;
		UnityEngine.Vector2? iconOffset = null;
		CardSetDbfRecord cardSetRecord = GameDbf.GetIndex().GetCardSet(cardSet);
		if (cardSetRecord != null)
		{
			iconTextureAssetRef = cardSetRecord.FilterIconTexture;
			iconOffset = new UnityEngine.Vector2((float)cardSetRecord.FilterIconOffsetX, (float)cardSetRecord.FilterIconOffsetY);
		}
		m_setFilterTray.AddItem(GameStrings.GetCardSetNameShortened(cardSet), iconTextureAssetRef, iconOffset, SetFilterCallback, sets, GameUtils.GetCardSetFormat(cardSet));
	}

	private long GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId gameSaveSubkeyId)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.COLLECTION_MANAGER, gameSaveSubkeyId, out List<long> lastSeenFeaturedCardsEventList);
		long lastSeenFeaturedCardsEventId = 0L;
		if (lastSeenFeaturedCardsEventList != null && lastSeenFeaturedCardsEventList.Any())
		{
			lastSeenFeaturedCardsEventId = lastSeenFeaturedCardsEventList.First();
		}
		return lastSeenFeaturedCardsEventId;
	}

	private IEnumerator SetIconFxIfFeaturedCardsEventNotSeen(SetFilterItem setFilterItem, EventTimingType currentActiveFeaturedCardsEvent)
	{
		while (!m_isReady)
		{
			yield return null;
		}
		long lastSeenFeaturedCardsEventId = GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_ITEM);
		long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
		if (currentActiveEventId != -1 && currentActiveEventId != lastSeenFeaturedCardsEventId)
		{
			setFilterItem.SetIconFxActive(active: true);
		}
	}

	private IEnumerator SetFeaturedCardsSetFilterGlowIfNotSeen(EventTimingType currentActiveFeaturedCardsEvent)
	{
		while (!m_isReady)
		{
			yield return null;
		}
		long lastSeenFeaturedCardsEventId = GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
		if (currentActiveEventId != -1 && currentActiveEventId != lastSeenFeaturedCardsEventId)
		{
			m_setFilterTray.SetFilterButtonGlowActive(active: true);
			if (m_filterButtonGlow != null)
			{
				m_filterButtonGlow.SetActive(value: true);
			}
		}
	}

	private void SetLastSeenFeaturedCardsEvent(EventTimingType currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId subkeyId)
	{
		if (currentActiveFeaturedCardsEvent != EventTimingType.UNKNOWN)
		{
			long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
			if (currentActiveEventId != -1 && GetLastSeenFeaturedCardsEvent(subkeyId) != currentActiveEventId)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.COLLECTION_MANAGER, subkeyId, currentActiveEventId));
			}
		}
	}

	protected override void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		OnSearchDeactivated_Internal(oldSearchText, newSearchText, updateManaFilterToMatchSearchText: true);
	}

	private void OnSearchDeactivated_Internal(string oldSearchText, string newSearchText, bool updateManaFilterToMatchSearchText)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			EnableInput(enable: true);
		}
		if (oldSearchText == newSearchText)
		{
			OnSearchFilterComplete();
			return;
		}
		string[] source = newSearchText.ToLower().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		string missingString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
		if (source.Contains(missingString))
		{
			EnableCatalogue(enable: true, null);
			m_searchTriggeredCrafting = true;
		}
		else
		{
			ResetFilterSettingsFromSearch();
		}
		NotifyFilterUpdate(m_searchFilterListeners, !string.IsNullOrEmpty(newSearchText), newSearchText);
		m_pageManager.ChangeSearchTextFilter(newSearchText, base.OnSearchFilterComplete, null);
	}

	protected override void OnSearchCleared(bool transitionPage)
	{
		ResetFilterSettingsFromSearch();
		NotifyFilterUpdate(m_searchFilterListeners, active: false, "");
		m_pageManager.ChangeSearchTextFilter("", transitionPage);
		base.OnSearchCleared(transitionPage);
	}

	private void ResetFilterSettingsFromSearch()
	{
		if (m_searchTriggeredCrafting)
		{
			EnableCatalogue(enable: false, null);
		}
		m_searchTriggeredCrafting = false;
	}

	private void DoEnterCollectionManagerEvents()
	{
		if (CollectionManager.Get().HasVisitedCollection() || SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.LETTUCE_BOUNTY_BOARD)
		{
			EnableInput(enable: true);
			OpenBookImmediately();
		}
		else
		{
			CollectionManager.Get().SetHasVisitedCollection(enable: true);
			EnableInput(enable: false);
			StartCoroutine(OpenBookWhenReady());
		}
	}

	private void OpenBookImmediately()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_COLLECTION);
		}
		StartCoroutine(SetBookToOpen());
		TryShowCollectionTips();
		TryShowButtons();
	}

	private IEnumerator OpenBookWhenReady()
	{
		while (SceneMgr.Get().IsTransitioning())
		{
			yield return null;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_COLLECTION)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_COLLECTION);
		}
		m_pageManager.OnBookOpening();
		StartCoroutine(DoBookOpeningAnimations());
		TryShowCollectionTips();
		TryShowButtons();
	}

	public void TryShowCollectionTips()
	{
		if (!IsCollectionTipsBlocked)
		{
			if (m_ShowCollectionTipsCoroutine != null)
			{
				StopCoroutine(m_ShowCollectionTipsCoroutine);
				m_ShowCollectionTipsCoroutine = null;
			}
			m_ShowCollectionTipsCoroutine = StartCoroutine(ShowCollectionTipsIfNeeded());
		}
	}

	public void BlockCollectionTips(bool isBlocked, string blockId)
	{
		if (isBlocked)
		{
			if (m_blockTooltipRequests.Add(blockId) && m_blockTooltipRequests.Count == 1)
			{
				HideHelpPopups();
			}
		}
		else if (m_blockTooltipRequests.Remove(blockId) && m_blockTooltipRequests.Count == 0)
		{
			TryShowCollectionTips();
		}
	}

	public void TryShowButtons()
	{
		if (m_ShowCampfireButtonCoroutine != null)
		{
			StopCoroutine(m_ShowCampfireButtonCoroutine);
			m_ShowCampfireButtonCoroutine = null;
		}
		m_ShowCampfireButtonCoroutine = StartCoroutine(ShowCampfireButtonIfNeeded());
		if (m_ShowMaxedoutFilterButtonCoroutine != null)
		{
			StopCoroutine(m_ShowMaxedoutFilterButtonCoroutine);
			m_ShowMaxedoutFilterButtonCoroutine = null;
		}
		m_ShowMaxedoutFilterButtonCoroutine = StartCoroutine(ShowMaxedoutFilterButtonIfNeeded());
	}

	private IEnumerator ShowCollectionTipsIfNeeded()
	{
		while (CollectionManager.Get().IsWaitingForBoxTransition())
		{
			yield return null;
		}
		yield return new WaitForSeconds(m_secondsDelayBeforeTutorialPopups);
		if (m_isExiting)
		{
			yield break;
		}
		LettuceCollectionPageManager lcpm = GetPageManager() as LettuceCollectionPageManager;
		while (!lcpm.IsFullyLoaded())
		{
			yield return null;
		}
		if (TutorialShouldShowAbilityUpgrade() && UserAttentionManager.CanShowAttentionGrabber("LettuceCollectionDisplay.ShowCollectionTipsIfNeeded:HAS_SEEN_SHOW_MERC_DETAILS_TUTORIAL"))
		{
			m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showMercDetailsTutorialBone.position, m_showMercDetailsTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL01"));
			m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			m_helpPopup.PulseReminderEveryXSeconds(3f);
			for (int i = 0; i < m_tuckTweens.Length; i++)
			{
				m_tuckTweens[i].PlayForward();
			}
		}
		else if (!AttemptToShowMaxedOutTutorialToolTip())
		{
			AttemptToShowAppearceTip();
		}
	}

	private IEnumerator ShowCampfireButtonIfNeeded()
	{
		while (!m_campfireButtonReference.IsReady)
		{
			yield return null;
		}
		if (!m_isExiting && ShouldShowCampfireButton())
		{
			m_campfireButton.TriggerEvent(CAMPFIRE_BUTTON_SHOW_EVENT);
		}
	}

	private IEnumerator ShowMaxedoutFilterButtonIfNeeded()
	{
		while (!m_maxedoutFilterButtonReference.IsReady)
		{
			yield return null;
		}
		if (!m_isExiting && ShouldShowMaxedOutMercsFilter())
		{
			m_maxedoutFilterButton.TriggerEvent(MAXEDOUT_FILTER_BUTTON_SHOW_EVENT);
			SetMaxedoutFilterButtonToggleState(m_pageManager.GetShowOnlyFullyUpgradedMercs());
		}
	}

	private void SetMaxedoutFilterButtonToggleState(bool isToggled)
	{
		if (isToggled)
		{
			m_maxedoutFilterButton.TriggerEvent(MAXEDOUT_FILTER_BUTTON_TOGGLE_ON);
		}
		else
		{
			m_maxedoutFilterButton.TriggerEvent(MAXEDOUT_FILTER_BUTTON_TOGGLE_OFF);
		}
	}

	public bool CanShowAppearanceTip(bool checkForMercOnPage = true)
	{
		if (Options.Get().GetBool(Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL, defaultVal: false))
		{
			return false;
		}
		if (!UserAttentionManager.CanShowAttentionGrabber("LettuceCollectionDisplay.ShowCollectionTipsIfNeeded:" + Option.HAS_SEEN_MERC_APPEARANCE_TUTORIAL))
		{
			return false;
		}
		if (m_filterSlidingTray != null && m_filterSlidingTray.IsShown())
		{
			return false;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(18L);
		if (!merc.HasUnlockedGoldenOrBetter())
		{
			return false;
		}
		if (checkForMercOnPage)
		{
			LettuceCollectionPageManager lcpm = GetPageManager() as LettuceCollectionPageManager;
			if (lcpm == null)
			{
				return false;
			}
			if (lcpm.GetMercenaryOnPage(merc.ID) == null)
			{
				return false;
			}
		}
		return true;
	}

	protected bool AttemptToShowAppearceTip()
	{
		if (CanShowAppearanceTip())
		{
			m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showMercDetailsTutorialBone.position, m_showMercDetailsTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_PORTRAIT_01"));
			m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			m_helpPopup.PulseReminderEveryXSeconds(3f);
			return true;
		}
		return false;
	}

	public bool CanShowMaxedOutToolTutorailTip()
	{
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START))
		{
			return !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END);
		}
		return false;
	}

	protected bool AttemptToShowMaxedOutTutorialToolTip()
	{
		if (CanShowMaxedOutToolTutorailTip())
		{
			HideHelpPopups();
			if (m_pageManager.GetShowOnlyFullyUpgradedMercs())
			{
				if ((PlatformSettings.Screen == ScreenCategory.Phone && m_filterSlidingTray != null && !m_filterSlidingTray.IsShown()) || PlatformSettings.Screen != ScreenCategory.Phone)
				{
					m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showMercDetailsTutorialBone.position, m_showMercDetailsTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_MAXED_OUT_COLLECTION"));
					m_helpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
				}
			}
			else
			{
				if (PlatformSettings.Screen != ScreenCategory.Phone || (PlatformSettings.Screen == ScreenCategory.Phone && m_filterSlidingTray != null && m_filterSlidingTray.IsShown()))
				{
					m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_showMaxedOutTutorialBone.position, m_showMaxedOutTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_MAXED_OUT_FILTER"));
				}
				else
				{
					m_helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_filterTrayTutorialBone.position, m_filterTrayTutorialBone.localScale, GameStrings.Get("GLUE_LETTUCE_COLLECTION_TUTORIAL_MAXED_OUT_FILTER"));
				}
				m_helpPopup.ShowPopUpArrow((PlatformSettings.Screen != ScreenCategory.Phone) ? Notification.PopUpArrowDirection.Left : Notification.PopUpArrowDirection.Up);
			}
			m_helpPopup.PulseReminderEveryXSeconds(3f);
			return true;
		}
		return false;
	}

	protected override void OnSwitchViewModeResponse(bool triggerResponse, CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode newMode, CollectionUtils.ViewModeData userdata)
	{
		base.OnSwitchViewModeResponse(triggerResponse, prevMode, newMode, userdata);
		EnableSetAndManaFiltersByViewMode(newMode);
	}

	private void EnableSetAndManaFiltersByViewMode(CollectionUtils.ViewMode viewMode)
	{
		bool enableUI = viewMode == CollectionUtils.ViewMode.CARDS;
		EnableSetAndManaFilters(enableUI);
	}

	private void EnableSetAndManaFilters(bool enabled)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_craftingModeButton.Enable(enabled);
		}
		if (m_setFilterTray != null)
		{
			m_setFilterTray.SetButtonEnabled(enabled);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_setFilterTray.gameObject.SetActive(enabled);
			}
		}
		m_search.SetEnabled(enabled: true);
	}

	private void OnSetFilterButtonPressed()
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		m_setFilterTray.SetFilterButtonGlowActive(active: false);
	}

	private void OnPhoneFilterButtonPressed()
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		m_filterButtonGlow.SetActive(value: false);
	}

	private void OnFilterSlidingTrayTransitionCompleted()
	{
		HideAllTips();
		TryShowCollectionTips();
	}

	protected override CraftingTrayBase GetCraftingTrayComponent(GameObject go)
	{
		return go.GetComponent<MercenariesCraftingTray>();
	}

	protected override void OnCraftingTrayLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		base.OnCraftingTrayLoaded(assetRef, go, callbackData);
	}

	public override void ShowCraftingTray(bool? includeCraftable = null, bool? showOnlyPromotable = null, bool? unused1 = null, bool? unused2 = null, bool? unused3 = null, bool updatePage = true)
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			DeckTrayTeamListContent teamsContent = deckTray.GetTeamsContent();
			if (teamsContent != null)
			{
				teamsContent.CancelRenameEditingTeam();
			}
		}
		bool updatePage2 = updatePage;
		base.ShowCraftingTray(includeCraftable, showOnlyPromotable, null, null, null, updatePage2);
		ShowAppropriateSetFilters();
	}

	protected override CollectionUtils.ViewMode GetInitialViewMode()
	{
		return CollectionUtils.ViewMode.CARDS;
	}

	public override void HideCraftingTray()
	{
		base.HideCraftingTray();
		ShowAppropriateSetFilters();
	}

	public void Dev_ShowTutorialPopups()
	{
		if (IsMercenaryDetailsDisplayActive())
		{
			GetMercenaryDetailsDisplay().Dev_ShowTutorialPopups();
			return;
		}
		List<Transform> bones = new List<Transform>();
		bones.Add(m_setFilterTutorialBone);
		bones.Add(m_showMercDetailsTutorialBone);
		bones.Add(m_showMaxedOutTutorialBone);
		if (PlatformSettings.Screen == ScreenCategory.Phone)
		{
			bones.Add(m_filterTrayTutorialBone);
		}
		foreach (Transform bone in bones)
		{
			NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, bone.position, bone.localScale, bone.name);
		}
	}
}
