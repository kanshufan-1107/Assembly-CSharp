using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Store;
using PegasusClient;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class DraftManager : IService
{
	public delegate void DraftDeckSet(CollectionDeck deck);

	private CollectionDeck m_draftDeck;

	private bool m_hasReceivedSessionWinsLosses;

	private int m_currentSlot;

	private DraftSlotType m_currentSlotType;

	private List<DraftSlotType> m_uniqueDraftSlotTypesForDeck = new List<DraftSlotType>();

	private int m_validSlot;

	private int m_maxSlot;

	private int m_losses;

	private int m_wins;

	private int m_maxWins = int.MaxValue;

	private int m_numTicketsOwned;

	private bool m_isNewKey;

	private bool m_deckActiveDuringSession;

	private Network.RewardChest m_chest;

	private bool m_inRewards;

	private ArenaSession m_currentSession;

	private List<DraftDeckSet> m_draftDeckSetListeners = new List<DraftDeckSet>();

	private bool m_pendingRequestToDisablePremiums;

	private int m_chosenIndex;

	private ArenaSeasonInfo m_currentSeason;

	private static readonly AssetReference DEFAULT_DRAFT_PAPER_TEXTURE = "Forge_Main_Paper.psd:64b6646e1c591d545885572fccd74259";

	private static readonly AssetReference DEFAULT_DRAFT_PAPER_TEXTURE_PHONE = "Forge_Main_Paper_phone.psd:ab59053fdba3ebd40bfd6ced4fd246bc";

	public ulong SecondsUntilEndOfSeason
	{
		get
		{
			if (m_currentSeason != null)
			{
				return m_currentSeason.Season.GameContentSeason.EndSecondsFromNow;
			}
			return 0uL;
		}
	}

	public int CurrentSeasonId
	{
		get
		{
			if (m_currentSeason != null)
			{
				return m_currentSeason.Season.GameContentSeason.SeasonId;
			}
			return 0;
		}
	}

	public bool HasActiveRun
	{
		get
		{
			if (m_currentSession != null && m_currentSession.HasIsActive)
			{
				return m_currentSession.IsActive;
			}
			return false;
		}
	}

	public int ChosenIndex => m_chosenIndex;

	public bool CanShowWinsLosses => m_hasReceivedSessionWinsLosses;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<GameMgr>().RegisterFindGameEvent(OnFindGameEvent);
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(ArenaSessionResponse.PacketID.ID, OnArenaSessionResponse);
		network.RegisterNetHandler(DraftRewardsAcked.PacketID.ID, OnAckRewards);
		network.RegisterNetHandler(DraftError.PacketID.ID, OnError);
		network.RegisterNetHandler(DraftRemovePremiumsResponse.PacketID.ID, OnDraftRemovePremiumsResponse);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(GameMgr)
		};
	}

	private void WillReset()
	{
		ClearDeckInfo();
	}

	public void Shutdown()
	{
	}

	public static DraftManager Get()
	{
		return ServiceManager.Get<DraftManager>();
	}

	public void OnLoggedIn()
	{
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
	}

	public void RegisterDisplayHandlers()
	{
		Network network = Network.Get();
		network.RegisterNetHandler(DraftBeginning.PacketID.ID, OnBegin);
		network.RegisterNetHandler(DraftRetired.PacketID.ID, OnRetire);
		network.RegisterNetHandler(DraftChoicesAndContents.PacketID.ID, OnChoicesAndContents);
		network.RegisterNetHandler(DraftChosen.PacketID.ID, OnChosen);
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnDraftPurchaseAck);
		if (DemoMgr.Get().ArenaIs1WinMode())
		{
			StoreManager.Get().RegisterSuccessfulPurchaseListener(OnDraftPurchaseAck);
		}
	}

	public void UnregisterDisplayHandlers()
	{
		Network network = Network.Get();
		network.RemoveNetHandler(DraftBeginning.PacketID.ID, OnBegin);
		network.RemoveNetHandler(DraftRetired.PacketID.ID, OnRetire);
		network.RemoveNetHandler(DraftChoicesAndContents.PacketID.ID, OnChoicesAndContents);
		network.RemoveNetHandler(DraftChosen.PacketID.ID, OnChosen);
		StoreManager.Get().RemoveSuccessfulPurchaseAckListener(OnDraftPurchaseAck);
		if (DemoMgr.Get().ArenaIs1WinMode())
		{
			StoreManager.Get().RemoveSuccessfulPurchaseListener(OnDraftPurchaseAck);
		}
	}

	public void RegisterDraftDeckSetListener(DraftDeckSet dlg)
	{
		m_draftDeckSetListeners.Add(dlg);
	}

	public void RemoveDraftDeckSetListener(DraftDeckSet dlg)
	{
		m_draftDeckSetListeners.Remove(dlg);
	}

	public void RefreshCurrentSeasonFromServer()
	{
		Network.Get().SendArenaSessionRequest();
	}

	public CollectionDeck GetDraftDeck()
	{
		return m_draftDeck;
	}

	public int GetSlot()
	{
		return m_currentSlot;
	}

	public DraftSlotType GetSlotType()
	{
		return m_currentSlotType;
	}

	public bool HasSlotType(DraftSlotType slotType)
	{
		return m_uniqueDraftSlotTypesForDeck.Contains(slotType);
	}

	public int GetLosses()
	{
		return m_losses;
	}

	public int GetWins()
	{
		return m_wins;
	}

	public int GetMaxWins()
	{
		return m_maxWins;
	}

	public int GetNumTicketsOwned()
	{
		return m_numTicketsOwned;
	}

	public bool GetIsNewKey()
	{
		return m_isNewKey;
	}

	public AssetReference GetDraftPaperTexture()
	{
		string draftPaperTextureName = null;
		if (m_currentSeason != null)
		{
			draftPaperTextureName = ((!UniversalInputManager.UsePhoneUI) ? m_currentSeason.Season.DraftPaperTexture : m_currentSeason.Season.DraftPaperTexturePhone);
		}
		if (!string.IsNullOrEmpty(draftPaperTextureName))
		{
			return new AssetReference(draftPaperTextureName);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return DEFAULT_DRAFT_PAPER_TEXTURE_PHONE;
		}
		return DEFAULT_DRAFT_PAPER_TEXTURE;
	}

	public bool GetDraftPaperTextColorOverride(ref Color overrideColor)
	{
		if (m_currentSeason != null && !string.IsNullOrEmpty(m_currentSeason.Season.DraftPaperTextColor))
		{
			return ColorUtility.TryParseHtmlString(m_currentSeason.Season.DraftPaperTextColor, out overrideColor);
		}
		return false;
	}

	public AssetReference GetRewardPaperPrefab()
	{
		string rewardPaperPrefabName = null;
		if (m_currentSeason != null)
		{
			rewardPaperPrefabName = ((!UniversalInputManager.UsePhoneUI) ? m_currentSeason.Season.RewardPaperPrefab : m_currentSeason.Season.RewardPaperPrefabPhone);
		}
		if (!string.IsNullOrEmpty(rewardPaperPrefabName))
		{
			return new AssetReference(rewardPaperPrefabName);
		}
		return ArenaRewardPaper.GetDefaultRewardPaper();
	}

	public string GetSceneHeadlineText()
	{
		if (m_currentSeason != null && m_currentSeason.Season.Strings.Count > 0)
		{
			return GameStrings.FormatStringWithPlurals(m_currentSeason.Season.Strings, "SCENE_HEADLINE");
		}
		return string.Empty;
	}

	public bool ShouldActivateKey()
	{
		GameContentScenario scen = m_currentSeason.Season.GameContentSeason.Scenarios.FirstOrDefault();
		int maxWins = scen?.MaxWins ?? 0;
		int maxLosses = scen?.MaxLosses ?? 0;
		if (!m_deckActiveDuringSession)
		{
			if (m_inRewards && m_wins < maxWins)
			{
				return m_losses < maxLosses;
			}
			return false;
		}
		return true;
	}

	public List<RewardData> GetRewards()
	{
		if (m_chest != null)
		{
			return m_chest.Rewards;
		}
		return new List<RewardData>();
	}

	public void MakeChoice(int choiceNum, TAG_PREMIUM choicePremium)
	{
		m_chosenIndex = choiceNum;
		if (m_draftDeck == null)
		{
			Debug.LogWarning("DraftManager.MakeChoice(): Trying to make a draft choice while the draft deck is null");
		}
		else if (m_validSlot == m_currentSlot)
		{
			m_validSlot++;
			Network.Get().MakeDraftChoice(m_draftDeck.ID, m_currentSlot, choiceNum, (int)choicePremium);
		}
	}

	public void NotifyOfFinalGame(bool wonFinalGame)
	{
		if (wonFinalGame)
		{
			m_wins++;
		}
		else
		{
			m_losses++;
		}
	}

	public void FindGame()
	{
		int scenarioId = m_currentSeason.Season.GameContentSeason.Scenarios.FirstOrDefault()?.ScenarioId ?? 2;
		GameMgr.Get().FindGame(GameType.GT_ARENA, FormatType.FT_WILD, scenarioId, 0, 0L, null, CurrentSeasonId, restoreSavedGameState: false, null, null, 0L);
		if (m_draftDeck != null)
		{
			Log.Decks.PrintInfo("Starting Arena Game With Deck:");
			m_draftDeck.LogDeckStringInformation();
		}
	}

	public TAG_PREMIUM GetDraftPremium(string cardId)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return TAG_PREMIUM.NORMAL;
		}
		if (collectionManager.GetNumCopiesInCollection(cardId, TAG_PREMIUM.DIAMOND) > 0 && m_draftDeck != null && m_draftDeck.GetCardIdCount(cardId) == 0)
		{
			return TAG_PREMIUM.DIAMOND;
		}
		CollectibleCard card = collectionManager.GetCard(cardId, TAG_PREMIUM.NORMAL);
		bool cardIsLegendary = card != null && card.Rarity == TAG_RARITY.LEGENDARY;
		if (IsBestValidPremium(cardId, TAG_PREMIUM.SIGNATURE, cardIsLegendary))
		{
			return TAG_PREMIUM.SIGNATURE;
		}
		if (IsBestValidPremium(cardId, TAG_PREMIUM.GOLDEN, cardIsLegendary))
		{
			return TAG_PREMIUM.GOLDEN;
		}
		return TAG_PREMIUM.NORMAL;
	}

	private bool IsBestValidPremium(string cardId, TAG_PREMIUM premium, bool isLegendary)
	{
		int numOwned = CollectionManager.Get().GetNumCopiesInCollection(cardId, premium);
		if (numOwned <= 0)
		{
			return false;
		}
		if (isLegendary)
		{
			return true;
		}
		if (numOwned >= 2 || m_draftDeck.GetCardCountAllMatchingSlots(cardId, premium) < numOwned)
		{
			return true;
		}
		return false;
	}

	public bool ShouldShowFreeArenaWinScreen()
	{
		if (EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_FROST_FESTIVAL_FREE_ARENA_WIN) && !Options.Get().GetBool(Option.HAS_SEEN_FREE_ARENA_WIN_DIALOG_THIS_DRAFT))
		{
			return m_wins > 0;
		}
		return false;
	}

	public void PromptToDisablePremium()
	{
		if (!m_pendingRequestToDisablePremiums && !Options.Get().GetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT) && !m_inRewards)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_DRAFT_REMOVE_PREMIUMS_DIALOG_TITLE");
			info.m_text = GameStrings.Get("GLUE_DRAFT_REMOVE_PREMIUMS_DIALOG_BODY");
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES");
			info.m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO");
			info.m_responseCallback = OnDisablePremiumsConfirmationResponse;
			DialogManager.Get().ShowPopup(info);
			m_pendingRequestToDisablePremiums = true;
		}
	}

	private void OnDisablePremiumsConfirmationResponse(AlertPopup.Response response, object userData)
	{
		m_pendingRequestToDisablePremiums = false;
		if (response == AlertPopup.Response.CONFIRM)
		{
			Network.Get().DraftRequestDisablePremiums();
		}
	}

	private void OnDraftRemovePremiumsResponse()
	{
		Options.Get().SetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT, val: true);
		Network.DraftChoicesAndContents info = Network.Get().GetDraftRemovePremiumsResponse();
		m_draftDeck.GetSlots().Clear();
		foreach (Network.CardUserData card in info.DeckInfo.Cards)
		{
			string cardID = ((card.DbId == 0) ? string.Empty : GameUtils.TranslateDbIdToCardId(card.DbId));
			for (int i = 0; i < card.Count; i++)
			{
				if (!m_draftDeck.AddCard(cardID, card.Premium, false, null))
				{
					Debug.LogWarning($"DraftManager.OnDraftRemovePremiumsResponse() - Card {cardID} could not be added to draft deck");
				}
			}
		}
		DraftPhoneDeckTray.Get().GetCardsContent().UpdateCardList();
		InformDraftDisplayOfChoices(info.Choices);
	}

	public bool ShowArenaPopup_SeasonEndingSoon(long secondsToCurrentSeasonEnd, Action popupClosedCallback)
	{
		if (m_currentSeason == null || !m_currentSeason.HasSeasonEndingSoonPrefab || string.IsNullOrEmpty(m_currentSeason.SeasonEndingSoonPrefab) || !m_currentSeason.HasSeason || m_currentSeason.Season == null || m_currentSeason.Season.Strings.Count == 0)
		{
			Error.AddDevWarning("No Season Data", "Cannot show 'Ending Soon' dialog - the current Arena season={0} does not have the ENDING_SOON_PREFAB data or header/body strings.", CurrentSeasonId);
			return false;
		}
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = null,
			m_minutes = null,
			m_hours = "GLUE_ARENA_POPUP_ENDING_SOON_HEADER_HOURS",
			m_yesterday = null,
			m_days = "GLUE_ARENA_POPUP_ENDING_SOON_HEADER_DAYS",
			m_weeks = "GLUE_ARENA_POPUP_ENDING_SOON_HEADER_WEEKS",
			m_monthAgo = "GLUE_ARENA_POPUP_ENDING_SOON_HEADER_MONTHS"
		};
		BasicPopup.PopupInfo info = new BasicPopup.PopupInfo();
		info.m_prefabAssetRefs.Add(m_currentSeason.SeasonEndingSoonPrefab);
		info.m_prefabAssetRefs.Add(m_currentSeason.SeasonEndingSoonPrefabExtra);
		info.m_headerText = TimeUtils.GetElapsedTimeString(secondsToCurrentSeasonEnd, timeStringSet, roundUp: true);
		info.m_bodyText = GameStrings.FormatStringWithPlurals(m_currentSeason.Season.Strings, "ENDING_SOON_BODY");
		info.m_responseUserData = CurrentSeasonId;
		info.m_blurWhenShown = true;
		info.m_responseCallback = delegate
		{
			if (popupClosedCallback != null)
			{
				popupClosedCallback();
			}
		};
		return DialogManager.Get().ShowArenaSeasonPopup(UserAttentionBlocker.NONE, info);
	}

	public bool ShowArenaPopup_SeasonComingSoon(long secondsToNextSeasonStart, Action popupClosedCallback)
	{
		if (m_currentSeason == null || !m_currentSeason.HasNextSeasonComingSoonPrefab || string.IsNullOrEmpty(m_currentSeason.NextSeasonComingSoonPrefab) || m_currentSeason.NextSeasonStrings == null || m_currentSeason.NextSeasonStrings.Count == 0)
		{
			Error.AddDevWarning("No Season Data", "Cannot show 'Coming Soon' dialog - the season after current Arena season={0} does not have the COMING_SOON_PREFAB data or header/body strings.", CurrentSeasonId);
			return false;
		}
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = null,
			m_minutes = null,
			m_hours = "GLUE_ARENA_POPUP_COMING_SOON_HEADER_HOURS",
			m_yesterday = null,
			m_days = "GLUE_ARENA_POPUP_COMING_SOON_HEADER_DAYS",
			m_weeks = "GLUE_ARENA_POPUP_COMING_SOON_HEADER_WEEKS",
			m_monthAgo = "GLUE_ARENA_POPUP_COMING_SOON_HEADER_MONTHS"
		};
		BasicPopup.PopupInfo info = new BasicPopup.PopupInfo();
		info.m_prefabAssetRefs.Add(m_currentSeason.NextSeasonComingSoonPrefab);
		info.m_prefabAssetRefs.Add(m_currentSeason.NextSeasonComingSoonPrefabExtra);
		info.m_headerText = TimeUtils.GetElapsedTimeString(secondsToNextSeasonStart, timeStringSet, roundUp: true);
		info.m_bodyText = GameStrings.FormatStringWithPlurals(m_currentSeason.NextSeasonStrings, "COMING_SOON_BODY");
		info.m_blurWhenShown = true;
		info.m_responseUserData = m_currentSeason.NextSeasonId;
		info.m_responseCallback = delegate
		{
			if (popupClosedCallback != null)
			{
				popupClosedCallback();
			}
		};
		return DialogManager.Get().ShowArenaSeasonPopup(UserAttentionBlocker.NONE, info);
	}

	public bool ShowNextArenaPopup(Action popupClosedCallback)
	{
		if (m_currentSeason == null || PopupDisplayManager.Get().IsShowing)
		{
			return false;
		}
		if (ReturningPlayerMgr.Get().SuppressOldPopups)
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN && HasActiveRun)
		{
			bool seasonEnding = ShowSeasonEnding(popupClosedCallback);
			if (!seasonEnding)
			{
				return ShowSeasonStarting(popupClosedCallback);
			}
			return seasonEnding;
		}
		return false;
	}

	private bool ShowSeasonEnding(Action popupClosedCallback)
	{
		int lastSeenEndingSeasonId = Options.Get().GetInt(Option.LATEST_SEEN_ARENA_SEASON_ENDING);
		long? secondsToCurrentSeasonEnd = ((!m_currentSeason.HasSeason) ? ((long?)null) : new long?((long)m_currentSeason.Season.GameContentSeason.EndSecondsFromNow));
		if (secondsToCurrentSeasonEnd.HasValue && m_currentSeason.HasSeasonEndingSoonDays && secondsToCurrentSeasonEnd.Value <= m_currentSeason.SeasonEndingSoonDays * 86400 && lastSeenEndingSeasonId < CurrentSeasonId)
		{
			int seasonIdEnding = CurrentSeasonId;
			Action myCallback = delegate
			{
				Options.Get().SetInt(Option.LATEST_SEEN_ARENA_SEASON_ENDING, seasonIdEnding);
				if (popupClosedCallback != null)
				{
					popupClosedCallback();
				}
			};
			return ShowArenaPopup_SeasonEndingSoon(secondsToCurrentSeasonEnd.Value, myCallback);
		}
		return false;
	}

	private bool ShowSeasonStarting(Action popupClosedCallback)
	{
		int lastSeenStartingSeasonId = Options.Get().GetInt(Option.LATEST_SEEN_ARENA_SEASON_STARTING);
		long? secondsToNextSeasonStart = ((!m_currentSeason.HasNextStartSecondsFromNow) ? ((long?)null) : new long?((long)m_currentSeason.NextStartSecondsFromNow));
		if (secondsToNextSeasonStart.HasValue && m_currentSeason.HasNextSeasonComingSoonDays && secondsToNextSeasonStart.Value <= m_currentSeason.NextSeasonComingSoonDays * 86400 && m_currentSeason.HasNextSeasonId && lastSeenStartingSeasonId < m_currentSeason.NextSeasonId)
		{
			int seasonIdStarting = m_currentSeason.NextSeasonId;
			Action myCallback = delegate
			{
				Options.Get().SetInt(Option.LATEST_SEEN_ARENA_SEASON_STARTING, seasonIdStarting);
				if (popupClosedCallback != null)
				{
					popupClosedCallback();
				}
			};
			return ShowArenaPopup_SeasonComingSoon(secondsToNextSeasonStart.Value, myCallback);
		}
		return false;
	}

	public void ClearAllInnkeeperPopups()
	{
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_HERO_CHOICE);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_CARD_CHOICE);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_CARD_CHOICE2);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_PLAY_MODE);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_1WIN);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_2LOSS);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_RETIRE);
		Options.Get().DeleteOption(Option.HAS_SEEN_FORGE_MAX_WIN);
	}

	public void ClearAllSeenPopups()
	{
		Options.Get().DeleteOption(Option.LATEST_SEEN_SCHEDULED_ENTERED_ARENA_DRAFT);
		Options.Get().DeleteOption(Option.HAS_SEEN_FREE_ARENA_WIN_DIALOG_THIS_DRAFT);
		Options.Get().DeleteOption(Option.LATEST_SEEN_ARENA_SEASON_ENDING);
		Options.Get().DeleteOption(Option.LATEST_SEEN_ARENA_SEASON_STARTING);
	}

	private void ClearDeckInfo()
	{
		m_draftDeck = null;
		m_hasReceivedSessionWinsLosses = false;
		m_losses = 0;
		m_wins = 0;
		m_maxWins = int.MaxValue;
		m_isNewKey = false;
		m_chest = null;
		m_deckActiveDuringSession = false;
		Options.Get().SetBool(Option.HAS_SEEN_FREE_ARENA_WIN_DIALOG_THIS_DRAFT, val: false);
		Options.Get().SetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT, val: false);
	}

	private void OnBegin()
	{
		Options.Get().SetBool(Option.HAS_SEEN_FREE_ARENA_WIN_DIALOG_THIS_DRAFT, val: false);
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.DRAFT && (!SceneMgr.Get().IsTransitionNowOrPending() || SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.DRAFT))
		{
			m_hasReceivedSessionWinsLosses = true;
			Network.BeginDraft info = Network.Get().GetBeginDraft();
			m_draftDeck = new CollectionDeck
			{
				ID = info.DeckID,
				Type = DeckType.DRAFT_DECK,
				FormatType = FormatType.FT_WILD
			};
			m_wins = info.Wins;
			m_losses = 0;
			m_currentSlot = 0;
			m_currentSlotType = info.SlotType;
			m_uniqueDraftSlotTypesForDeck = info.UniqueSlotTypesForDraft;
			m_validSlot = 0;
			m_maxSlot = info.MaxSlot;
			m_chest = null;
			m_inRewards = false;
			m_currentSession = info.Session;
			Options.Get().SetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT, val: false);
			SessionRecord sessionRecord = new SessionRecord();
			sessionRecord.Wins = (uint)info.Wins;
			sessionRecord.Losses = 0u;
			sessionRecord.RunFinished = false;
			sessionRecord.SessionRecordType = SessionRecordType.ARENA;
			BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
			Log.Arena.Print($"DraftManager.OnBegin - Got new draft deck with ID: {m_draftDeck.ID}");
			InformDraftDisplayOfChoices(info.Heroes);
			FireDraftDeckSetEvent();
		}
	}

	private void OnRetire()
	{
		Network.DraftRetired retiredDraft = Network.Get().GetRetiredDraft();
		Log.Arena.Print($"DraftManager.OnRetire deckID={retiredDraft.Deck}");
		m_chest = retiredDraft.Chest;
		m_inRewards = true;
		InformDraftDisplayOfChoices(new List<NetCache.CardDefinition>());
	}

	private void OnAckRewards()
	{
		SessionRecord sessionRecord = new SessionRecord();
		sessionRecord.Wins = (uint)m_wins;
		sessionRecord.Losses = (uint)m_losses;
		sessionRecord.RunFinished = true;
		sessionRecord.SessionRecordType = SessionRecordType.ARENA;
		BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
		if (!Options.Get().GetBool(Option.HAS_ACKED_ARENA_REWARDS, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftManager.OnAckRewards:" + Option.HAS_ACKED_ARENA_REWARDS))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_ARENA_1ST_REWARD"), "VO_INNKEEPER_ARENA_1ST_REWARD.prefab:660e915849550ae4085735866647d529");
			Options.Get().SetBool(Option.HAS_ACKED_ARENA_REWARDS, val: true);
		}
		Network.Get().GetRewardsAckDraftID();
		ClearDeckInfo();
	}

	private void OnChoicesAndContents()
	{
		Network.DraftChoicesAndContents info = Network.Get().GetDraftChoicesAndContents();
		m_hasReceivedSessionWinsLosses = true;
		m_currentSlot = info.Slot;
		m_currentSlotType = info.SlotType;
		m_uniqueDraftSlotTypesForDeck = info.UniqueSlotTypesForDraft;
		m_validSlot = info.Slot;
		m_maxSlot = info.MaxSlot;
		m_draftDeck = new CollectionDeck
		{
			ID = info.DeckInfo.Deck,
			Type = DeckType.DRAFT_DECK,
			HeroCardID = info.Hero.Name,
			HeroPowerCardID = info.HeroPower.Name,
			FormatType = FormatType.FT_WILD
		};
		Log.Arena.Print($"DraftManager.OnChoicesAndContents - Draft Deck ID: {m_draftDeck.ID}, Hero Card = {m_draftDeck.HeroCardID}");
		foreach (Network.CardUserData card in info.DeckInfo.Cards)
		{
			string cardID = ((card.DbId == 0) ? string.Empty : GameUtils.TranslateDbIdToCardId(card.DbId));
			Log.Arena.Print($"DraftManager.OnChoicesAndContents - Draft deck contains card {cardID}");
			for (int i = 0; i < card.Count; i++)
			{
				if (!m_draftDeck.AddCard(cardID, card.Premium, false, null))
				{
					Debug.LogWarning($"DraftManager.OnChoicesAndContents() - Card {cardID} could not be added to draft deck");
				}
			}
		}
		m_losses = info.Losses;
		if (info.Wins > m_wins)
		{
			m_isNewKey = true;
		}
		else
		{
			m_isNewKey = false;
		}
		m_wins = info.Wins;
		m_maxWins = info.MaxWins;
		m_chest = info.Chest;
		m_inRewards = m_chest != null;
		m_currentSession = info.Session;
		if (m_losses > 0 && DemoMgr.Get().ArenaIs1WinMode())
		{
			Network.Get().DraftRetire(GetDraftDeck().ID, GetSlot(), CurrentSeasonId);
			return;
		}
		if (m_wins == 5 && DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2013)
		{
			DemoMgr.Get().CreateDemoText(GameStrings.Get("GLUE_BLIZZCON2013_ARENA_5_WINS"), unclickable: false, shouldDoArenaInstruction: false);
		}
		else if (m_losses == 3 && !Options.Get().GetBool(Option.HAS_LOST_IN_ARENA, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("DraftManager.OnChoicesAndContents:" + Option.HAS_LOST_IN_ARENA))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_ARENA_3RD_LOSS"), "VO_INNKEEPER_ARENA_3RD_LOSS.prefab:6b2af024c9980d344a087295afb5e3df");
			Options.Get().SetBool(Option.HAS_LOST_IN_ARENA, val: true);
		}
		InformDraftDisplayOfChoices(info.Choices);
	}

	private void InformDraftDisplayOfChoices(List<NetCache.CardDefinition> choices)
	{
		DraftDisplay draft = DraftDisplay.Get();
		if (draft == null)
		{
			return;
		}
		if (m_inRewards)
		{
			draft.SetDraftMode(DraftDisplay.DraftMode.IN_REWARDS);
			return;
		}
		if (choices.Count == 0)
		{
			m_deckActiveDuringSession = true;
			draft.SetDraftMode(DraftDisplay.DraftMode.ACTIVE_DRAFT_DECK);
			return;
		}
		if (!Options.Get().GetBool(Option.HAS_DISABLED_PREMIUMS_THIS_DRAFT) && GetSlotType() != DraftSlotType.DRAFT_SLOT_HERO_POWER)
		{
			foreach (NetCache.CardDefinition choice in choices)
			{
				choice.Premium = GetDraftPremium(choice.Name);
			}
		}
		draft.SetDraftMode(DraftDisplay.DraftMode.DRAFTING);
		draft.AcceptNewChoices(choices);
	}

	private void InformDraftDisplayOfSelectedChoice(int chosenIndex)
	{
		DraftDisplay draft = DraftDisplay.Get();
		if (!(draft == null))
		{
			draft.OnChoiceSelected(chosenIndex);
		}
	}

	private void OnChosen()
	{
		Network.DraftChosen chosen = Network.Get().GetDraftChosen();
		if (m_currentSlotType == DraftSlotType.DRAFT_SLOT_HERO)
		{
			Log.Arena.Print("DraftManager.OnChosen(): hero=" + chosen.ChosenCard.Name);
			m_draftDeck.HeroCardID = chosen.ChosenCard.Name;
		}
		else if (m_currentSlotType == DraftSlotType.DRAFT_SLOT_CARD)
		{
			m_draftDeck.AddCard(chosen.ChosenCard.Name, chosen.ChosenCard.Premium, false, null);
		}
		m_currentSlot++;
		m_currentSlotType = chosen.SlotType;
		if (m_currentSlot > m_maxSlot && DraftDisplay.Get() != null)
		{
			DraftDisplay.Get().DoDeckCompleteAnims();
		}
		InformDraftDisplayOfSelectedChoice(m_chosenIndex);
		InformDraftDisplayOfChoices(chosen.NextChoices);
	}

	private void OnError()
	{
		if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.DRAFT))
		{
			return;
		}
		DraftError draftError = Network.Get().GetDraftError();
		m_numTicketsOwned = (draftError.HasNumTicketsOwned ? draftError.NumTicketsOwned : 0);
		DraftDisplay draft = DraftDisplay.Get();
		switch (draftError.ErrorCode_)
		{
		case DraftError.ErrorCode.DE_SEASON_INCREMENTED:
			Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ARENA_SEASON_ERROR_NOT_ACTIVE");
			Get().RefreshCurrentSeasonFromServer();
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.DRAFT)
			{
				Navigation.GoBack();
			}
			break;
		case DraftError.ErrorCode.DE_NOT_IN_DRAFT_BUT_COULD_BE:
			if (Options.Get().GetBool(Option.HAS_SEEN_FORGE, defaultVal: false))
			{
				if (m_numTicketsOwned > 0)
				{
					DraftDisplay.Get().SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
				}
				else
				{
					RequestDraftBegin();
				}
			}
			else
			{
				DraftDisplay.Get().SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
			}
			break;
		case DraftError.ErrorCode.DE_NOT_IN_DRAFT:
			if (draft != null)
			{
				draft.SetDraftMode(DraftDisplay.DraftMode.NO_ACTIVE_DRAFT);
			}
			break;
		case DraftError.ErrorCode.DE_NO_LICENSE:
			Debug.LogWarning("DraftManager.OnError - No License.  What does this mean???");
			break;
		case DraftError.ErrorCode.DE_RETIRE_FIRST:
			Debug.LogError("DraftManager.OnError - You cannot start a new draft while one is in progress.");
			break;
		case DraftError.ErrorCode.DE_FEATURE_DISABLED:
			Debug.LogError("DraftManager.OnError - The Arena is currently disabled. Returning to the hub.");
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_FORGE");
			}
			break;
		case DraftError.ErrorCode.DE_NOT_ENOUGH_CLASSES:
			Debug.LogError("DraftManager.OnError - You cannot start a new draft with less than three heroes unlocked");
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			Error.AddWarningLoc("GLOBAL_ERROR_GENERIC_HEADER", "GLOBAL_ARENA_ERROR_NOT_ENOUGH_CLASSES");
			break;
		case DraftError.ErrorCode.DE_UNKNOWN:
			Debug.LogError("DraftManager.OnError - UNKNOWN EXCEPTION - See server logs for more info.");
			break;
		default:
			Debug.LogErrorFormat("DraftManager.onError - UNHANDLED ERROR - See server logs for more info. ERROR: {0}", draftError.ErrorCode_);
			break;
		}
	}

	private void OnArenaSessionResponse()
	{
		ArenaSessionResponse response = Network.Get().GetArenaSessionResponse();
		OnArenaSessionResponsePacket(response);
	}

	public void OnArenaSessionResponsePacket(ArenaSessionResponse response)
	{
		if (response != null && response.ErrorCode == ErrorCode.ERROR_OK && response.HasSession)
		{
			m_hasReceivedSessionWinsLosses = true;
			m_wins = (response.HasSession ? response.Session.Wins : 0);
			m_losses = (response.HasSession ? response.Session.Losses : 0);
			m_currentSession = (response.HasSession ? response.Session : null);
			if (response.HasCurrentSeason)
			{
				m_currentSeason = response.CurrentSeason;
			}
			if (GameMgr.Get().IsArena() || GameMgr.Get().IsNextArena())
			{
				SessionRecord sessionRecord = new SessionRecord();
				sessionRecord.Wins = (uint)m_wins;
				sessionRecord.Losses = (uint)m_losses;
				sessionRecord.RunFinished = false;
				sessionRecord.SessionRecordType = SessionRecordType.ARENA;
				BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
			}
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_CANCELED:
			if (DraftDisplay.Get() != null)
			{
				DraftDisplay.Get().HandleGameStartupFailure();
			}
			break;
		case FindGameState.SERVER_GAME_CONNECTING:
			if (GameMgr.Get().IsNextArena() && !m_hasReceivedSessionWinsLosses)
			{
				RefreshCurrentSeasonFromServer();
			}
			break;
		}
		return false;
	}

	private void OnDraftPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		if (m_draftDeck != null)
		{
			StoreManager.Get().HideStore(ShopType.ARENA_STORE);
		}
		else
		{
			RequestDraftBegin();
		}
	}

	public void RequestDraftBegin()
	{
		Network.Get().DraftBegin();
	}

	private void FireDraftDeckSetEvent()
	{
		DraftDeckSet[] array = m_draftDeckSetListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](m_draftDeck);
		}
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (GameMgr.Get().IsArena() && mode == SceneMgr.Mode.GAMEPLAY)
		{
			GameState.Get().RegisterGameOverListener(OnGameOver);
		}
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		switch (playState)
		{
		case TAG_PLAYSTATE.WON:
		{
			NetCache.NetCacheProfileProgress arenaProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
			if (arenaProgress == null || GetWins() >= arenaProgress.BestForgeWins)
			{
				NetCache.Get().RefreshNetObject<NetCache.NetCacheProfileProgress>();
			}
			if (GetWins() == 11)
			{
				NotifyOfFinalGame(wonFinalGame: true);
			}
			break;
		}
		case TAG_PLAYSTATE.LOST:
		case TAG_PLAYSTATE.TIED:
			if (GetLosses() == 2)
			{
				NotifyOfFinalGame(wonFinalGame: false);
			}
			break;
		}
	}
}
