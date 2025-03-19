using System.Collections;
using System.Collections.Generic;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class BaconCollectionPageDisplay : CollectiblePageDisplay
{
	public enum HEADER_CLASS
	{
		INVALID = -1,
		HEROSKINS,
		GUIDESKINS,
		BOARDSKINS,
		FINISHERS,
		EMOTES
	}

	public GameObject m_favoriteBanner;

	public GameObject m_heroSkinsDecor;

	public GameObject[] m_heroSkinFrames;

	public GameObject m_guideSkinsDecor;

	public GameObject[] m_guideSkinFrames;

	public GameObject m_noMatchFoundObject;

	public UberText m_noMatchExplanationText;

	public Widget m_BoardSkinsWidget;

	public AsyncReference m_boardDisplayReference;

	public Widget m_FinishersWidget;

	public AsyncReference m_finisherDisplayReference;

	public Widget m_EmotesWidget;

	public AsyncReference m_emoteDisplayReference;

	private Widget m_BoardSkinsWidgetInstance;

	private Widget m_FinishersWidgetInstance;

	private Widget m_EmotesWidgetInstance;

	private string kUpdatePageControllerWidgetEvent = "UPDATE";

	public void Start()
	{
		m_boardDisplayReference.RegisterReadyListener<Widget>(OnBoardDisplayReady);
		m_finisherDisplayReference.RegisterReadyListener<Widget>(OnFinisherDisplayReady);
		m_emoteDisplayReference.RegisterReadyListener<Widget>(OnEmoteDisplayReady);
	}

	public override void UpdateCollectionItems(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibles, CollectionUtils.ViewMode mode)
	{
		base.UpdateCollectionItems(actorList, nonActorCollectibles, mode);
		for (int i = 0; i < actorList.Count && i < CollectiblePageDisplay.GetMaxCardsPerPage(); i++)
		{
			CollectionCardVisual collectionCardVisual = GetCollectionCardVisual(i);
			if (mode == CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS || mode == CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS)
			{
				collectionCardVisual.SetHeroSkinBoxCollider();
			}
			else
			{
				collectionCardVisual.SetDefaultBoxCollider();
			}
		}
		List<CollectibleBattlegroundsBoard> boards = new List<CollectibleBattlegroundsBoard>();
		foreach (ICollectible nonActorCollectible in nonActorCollectibles)
		{
			if (nonActorCollectible is CollectibleBattlegroundsBoard board)
			{
				boards.Add(board);
			}
		}
		List<CollectibleBattlegroundsFinisher> finishers = new List<CollectibleBattlegroundsFinisher>();
		foreach (ICollectible nonActorCollectible2 in nonActorCollectibles)
		{
			if (nonActorCollectible2 is CollectibleBattlegroundsFinisher finisher)
			{
				finishers.Add(finisher);
			}
		}
		List<CollectibleBattlegroundsEmote> emotes = new List<CollectibleBattlegroundsEmote>();
		foreach (ICollectible nonActorCollectible3 in nonActorCollectibles)
		{
			if (nonActorCollectible3 is CollectibleBattlegroundsEmote emote)
			{
				emotes.Add(emote);
			}
		}
		UpdateFavoriteHeroSkins(mode);
		UpdateFavoriteGuideSkins(mode);
		UpdateCollectionBoards(boards, mode);
		UpdateCollectionFinishers(finishers, mode);
		UpdateCollectionEmotes(emotes, mode);
		UpdateHeroSkinNames(mode);
		UpdateGuideSkinNames(mode);
		UpdateHeroSkinHeroPowers(mode);
	}

	public void UpdateCollectionBoards(List<CollectibleBattlegroundsBoard> boardList, CollectionUtils.ViewMode mode, BookPageManager.PageTransitionType transitionType = BookPageManager.PageTransitionType.NONE)
	{
		bool isBoardSkinsMode = mode == CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS;
		m_BoardSkinsWidget.gameObject.SetActive(isBoardSkinsMode);
		if (!isBoardSkinsMode)
		{
			m_BoardSkinsWidget.UnbindDataModel(565);
			return;
		}
		BattlegroundsBoardSkinCollectionPageDataModel pageModel = GetOrCreateBoardCollectionPageDataModel();
		if (pageModel == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.UpdateCollectionBoards - could not find data model!");
			return;
		}
		DataModelList<BattlegroundsBoardSkinDataModel> boardSkinModelList = new DataModelList<BattlegroundsBoardSkinDataModel>();
		if (boardSkinModelList != null)
		{
			foreach (CollectibleBattlegroundsBoard board in boardList)
			{
				BattlegroundsBoardSkinDataModel boardSkinDataModel = board.CreateBoardDataModel();
				boardSkinModelList.Add(boardSkinDataModel);
			}
		}
		pageModel.BoardSkinList = boardSkinModelList;
		StartCoroutine(DelayedWidgetUpdate(pageModel));
	}

	private IEnumerator DelayedWidgetUpdate(BattlegroundsBoardSkinCollectionPageDataModel pageData)
	{
		yield return new WaitForEndOfFrame();
		m_BoardSkinsWidget.BindDataModel(pageData);
		m_BoardSkinsWidget.TriggerEvent(kUpdatePageControllerWidgetEvent);
	}

	public void UpdateCollectionFinishers(List<CollectibleBattlegroundsFinisher> finisherList, CollectionUtils.ViewMode mode, BookPageManager.PageTransitionType transitionType = BookPageManager.PageTransitionType.NONE)
	{
		bool isFinishersMode = mode == CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS;
		m_FinishersWidget.gameObject.SetActive(isFinishersMode);
		if (!isFinishersMode)
		{
			m_FinishersWidget.UnbindDataModel(568);
			return;
		}
		BattlegroundsFinisherCollectionPageDataModel pageModel = GetOrCreateFinisherCollectionPageDataModel();
		if (pageModel == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.UpdateCollectionFinishers - could not find data model!");
			return;
		}
		m_FinishersWidget.BindDataModel(pageModel);
		DataModelList<BattlegroundsFinisherDataModel> finisherModelList = new DataModelList<BattlegroundsFinisherDataModel>();
		if (finisherModelList != null)
		{
			foreach (CollectibleBattlegroundsFinisher finisher in finisherList)
			{
				BattlegroundsFinisherDataModel finisherDataModel = finisher.CreateFinisherDataModel();
				finisherModelList.Add(finisherDataModel);
			}
		}
		pageModel.FinisherList = finisherModelList;
	}

	public void UpdateCollectionEmotes(List<CollectibleBattlegroundsEmote> emoteList, CollectionUtils.ViewMode mode, BookPageManager.PageTransitionType transitionType = BookPageManager.PageTransitionType.NONE)
	{
		bool isEmotesMode = mode == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES;
		m_EmotesWidget.gameObject.SetActive(isEmotesMode);
		if (!isEmotesMode)
		{
			m_EmotesWidget.UnbindDataModel(639);
			return;
		}
		BattlegroundsEmoteCollectionPageDataModel pageModel = GetOrCreateEmoteCollectionPageDataModel();
		if (pageModel == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.UpdateCollectionEmotes - could not find data model!");
			return;
		}
		m_EmotesWidget.BindDataModel(pageModel);
		DataModelList<BattlegroundsEmoteDataModel> emoteModelList = new DataModelList<BattlegroundsEmoteDataModel>();
		if (emoteModelList != null)
		{
			foreach (CollectibleBattlegroundsEmote emote in emoteList)
			{
				BattlegroundsEmoteDataModel emoteDataModel = emote.CreateEmoteDataModel();
				emoteModelList.Add(emoteDataModel);
			}
		}
		pageModel.EmoteList = emoteModelList;
	}

	private BattlegroundsBoardSkinCollectionPageDataModel GetOrCreateBoardCollectionPageDataModel()
	{
		if (!m_BoardSkinsWidget.GetDataModel(565, out var dataModel))
		{
			dataModel = new BattlegroundsBoardSkinCollectionPageDataModel();
		}
		return dataModel as BattlegroundsBoardSkinCollectionPageDataModel;
	}

	private BattlegroundsFinisherCollectionPageDataModel GetOrCreateFinisherCollectionPageDataModel()
	{
		if (!m_FinishersWidget.GetDataModel(568, out var dataModel))
		{
			dataModel = new BattlegroundsFinisherCollectionPageDataModel();
		}
		return dataModel as BattlegroundsFinisherCollectionPageDataModel;
	}

	private BattlegroundsEmoteCollectionPageDataModel GetOrCreateEmoteCollectionPageDataModel()
	{
		if (!m_EmotesWidget.GetDataModel(639, out var dataModel))
		{
			dataModel = new BattlegroundsEmoteCollectionPageDataModel();
		}
		return dataModel as BattlegroundsEmoteCollectionPageDataModel;
	}

	public override void UpdateCurrentPageCardLocks(bool playSound = false)
	{
		base.UpdateCurrentPageCardLocks(playSound);
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NONE);
		}
	}

	public void UpdateFavoriteHeroSkins(CollectionUtils.ViewMode mode)
	{
		bool isHeroSkinsMode = mode == CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS;
		if (m_heroSkinsDecor != null)
		{
			m_heroSkinsDecor.SetActive(isHeroSkinsMode);
		}
		if (!isHeroSkinsMode)
		{
			return;
		}
		int heroFrameIdx = 0;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				Actor actor = cardVisual.GetActor();
				BaconCollectionHeroSkin heroSkin = actor.GetComponent<BaconCollectionHeroSkin>();
				if (heroSkin == null)
				{
					continue;
				}
				heroSkin.ShowShadow(actor.IsShown());
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null)
				{
					heroSkin.ShowFavoriteBanner(BaconHeroSkinUtils.IsBattlegroundsHeroSkinFavorited(entityDef));
				}
			}
			if (heroFrameIdx < m_heroSkinFrames.Length)
			{
				m_heroSkinFrames[heroFrameIdx++].SetActive(cardVisual.IsShown());
			}
		}
	}

	public void UpdateFavoriteGuideSkins(CollectionUtils.ViewMode mode)
	{
		bool isGuideSkinsMode = mode == CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS;
		if (m_guideSkinsDecor != null)
		{
			m_guideSkinsDecor.SetActive(isGuideSkinsMode);
		}
		if (!isGuideSkinsMode)
		{
			return;
		}
		int guideFrameIdx = 0;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				Actor actor = cardVisual.GetActor();
				BaconCollectionGuideSkin guideSkin = actor.GetComponent<BaconCollectionGuideSkin>();
				if (guideSkin == null)
				{
					continue;
				}
				guideSkin.ShowShadow(actor.IsShown());
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null)
				{
					guideSkin.ShowFavoriteBanner(BaconHeroSkinUtils.IsBattlegroundsGuideSkinFavorited(entityDef));
				}
			}
			if (guideFrameIdx < m_guideSkinFrames.Length)
			{
				m_guideSkinFrames[guideFrameIdx++].SetActive(cardVisual.IsShown());
			}
		}
	}

	public void UpdateFavoriteBoardSkins(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS)
		{
			return;
		}
		foreach (BattlegroundsBoardSkinDataModel boardSkinDataModel in m_BoardSkinsWidget.GetDataModel<BattlegroundsBoardSkinCollectionPageDataModel>().BoardSkinList)
		{
			boardSkinDataModel.IsFavorite = CollectionManager.Get().IsFavoriteBattlegroundsBoardSkin(BattlegroundsBoardSkinId.FromTrustedValue(boardSkinDataModel.BoardDbiId));
		}
	}

	public void UpdateFavoriteFinisherSkins(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS)
		{
			return;
		}
		foreach (BattlegroundsFinisherDataModel finisherDataModel in m_FinishersWidget.GetDataModel<BattlegroundsFinisherCollectionPageDataModel>().FinisherList)
		{
			finisherDataModel.IsFavorite = CollectionManager.Get().IsFavoriteBattlegroundsFinisher(BattlegroundsFinisherId.FromTrustedValue(finisherDataModel.FinisherDbiId));
		}
	}

	public void UpdateHeroSkinHeroPowers(CollectionUtils.ViewMode mode)
	{
		if (mode != CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS)
		{
			return;
		}
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (!cardVisual.IsShown())
			{
				continue;
			}
			Actor actor = cardVisual.GetActor();
			BaconCollectionHeroSkin heroSkin = actor.GetComponent<BaconCollectionHeroSkin>();
			if (!(heroSkin == null))
			{
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null)
				{
					string heroPowerId = GameUtils.GetHeroPowerCardIdFromHero(entityDef.GetCardId());
					heroSkin.SetHeroPower(heroPowerId);
				}
			}
		}
	}

	public void UpdateHeroSkinNames(CollectionUtils.ViewMode mode)
	{
		if (mode == CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS)
		{
			StartCoroutine(WaitThenUpdateHeroSkinNames(mode));
		}
	}

	private IEnumerator WaitThenUpdateHeroSkinNames(CollectionUtils.ViewMode mode)
	{
		yield return null;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				CollectionHeroSkin heroSkin = cardVisual.GetActor().GetComponent<CollectionHeroSkin>();
				if (!(heroSkin == null))
				{
					heroSkin.ShowCollectionManagerText();
				}
			}
		}
	}

	public void UpdateGuideSkinNames(CollectionUtils.ViewMode mode)
	{
		if (mode == CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS)
		{
			StartCoroutine(WaitThenUpdateGuideSkinNames(mode));
		}
	}

	public void SetEmoteEquippedState(BattlegroundsEmoteId emoteId, bool isEquipped)
	{
		foreach (BattlegroundsEmoteDataModel emoteModel in GetOrCreateEmoteCollectionPageDataModel().EmoteList)
		{
			if (emoteModel.EmoteDbiId.Equals(emoteId.ToValue()))
			{
				emoteModel.IsEquipped = isEquipped;
			}
		}
	}

	private IEnumerator WaitThenUpdateGuideSkinNames(CollectionUtils.ViewMode mode)
	{
		yield return null;
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown())
			{
				BaconCollectionGuideSkin guideSkin = cardVisual.GetActor().GetComponent<BaconCollectionGuideSkin>();
				if (!(guideSkin == null))
				{
					guideSkin.ShowCollectionManagerText();
				}
			}
		}
	}

	public override void ShowNoMatchesFound(bool show, CollectionManager.FindCardsResult findResults = null, bool showHints = true)
	{
		m_noMatchFoundObject.SetActive(show);
		string explanationTextKey = "GLUE_COLLECTION_NO_RESULTS";
		m_noMatchExplanationText.Text = GameStrings.Get(explanationTextKey);
	}

	public override void SetPageType(FormatType inputFormatType)
	{
	}

	public void SetHeroSkins()
	{
		SetPageNameText(GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.HEROSKINS);
	}

	public void SetGuideSkins()
	{
		SetPageNameText(GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_GUIDE_SKINS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.GUIDESKINS);
	}

	public void SetBoardSkins()
	{
		SetPageNameText(GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_BOARD_SKINS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.BOARDSKINS);
	}

	public void SetFinishers()
	{
		SetPageNameText(GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_FINISHERS_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.FINISHERS);
	}

	public void SetEmotes()
	{
		SetPageNameText(GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_EMOTES_TITLE"));
		SetPageFlavorTextures(m_pageFlavorHeader, HEADER_CLASS.EMOTES);
	}

	private void BoardSkinDisplayEventListener(string eventName)
	{
		EventDataModel eventDataModel = m_BoardSkinsWidgetInstance.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to BaconCollectionPageDisplay");
			return;
		}
		BattlegroundsBoardSkinDataModel boardData = (BattlegroundsBoardSkinDataModel)eventDataModel.Payload;
		if (!(eventName == "BOARD_SKIN_clicked"))
		{
			if (eventName == "BOARD_SKIN_hover_end")
			{
				MarkBoardSeen(boardData);
			}
			return;
		}
		BattlegroundsBoardSkinCollectionPageDataModel collectionDataModel = GetOrCreateBoardCollectionPageDataModel();
		BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		if (bcd == null)
		{
			Log.CollectionManager.PrintError("BaconCollectionPageDisplay.BOARD_SKIN_clicked - BaconCollectionDisplay is null!");
		}
		if (!string.IsNullOrEmpty(boardData.DetailsRenderConfig))
		{
			bcd.ShowBoardDetailsRendered(boardData, collectionDataModel);
		}
		else
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"BaconCollectionPageDisplay::Boardskin[{boardData.BoardDbiId}]");
			bcd.ShowBoardDetailsDisplay(boardData, collectionDataModel);
		}
		MarkBoardSeen(boardData);
	}

	private void FinisherDisplayEventListener(string eventName)
	{
		EventDataModel eventDataModel = m_FinishersWidgetInstance.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to BaconCollectionPageDisplay");
			return;
		}
		BattlegroundsFinisherDataModel finisherData = (BattlegroundsFinisherDataModel)eventDataModel.Payload;
		if (!(eventName == "FINISHER_clicked"))
		{
			if (eventName == "FINISHER_hover_end")
			{
				MarkFinisherSeen(finisherData);
			}
			return;
		}
		BattlegroundsFinisherCollectionPageDataModel collectionDataModel = GetOrCreateFinisherCollectionPageDataModel();
		BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		if (bcd == null)
		{
			Log.CollectionManager.PrintError("BaconCollectionPageDisplay.FINISHER_clicked - BaconCollectionDisplay is null!");
			return;
		}
		if (!string.IsNullOrEmpty(finisherData.DetailsRenderConfig))
		{
			bcd.ShowFinisherDetailsRendered(finisherData, collectionDataModel);
		}
		else
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"BaconCollectionPageDisplay::Finisher[{finisherData.FinisherDbiId}]");
			bcd.ShowFinisherDetailsDisplay(finisherData, collectionDataModel);
		}
		MarkFinisherSeen(finisherData);
	}

	private void EmoteDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "EMOTE_clicked":
		{
			if (CollectionInputMgr.Get().HasHeldEmote())
			{
				break;
			}
			BattlegroundsEmoteCollectionPageDataModel collectionDataModel = GetOrCreateEmoteCollectionPageDataModel();
			BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
			if (bcd == null)
			{
				Log.CollectionManager.PrintError("BaconCollectionPageDisplay.EMOTE_clicked - BaconCollectionDisplay is null!");
				break;
			}
			BattlegroundsEmoteDataModel emoteData2 = GetEventEmoteDataModel();
			if (emoteData2 == null)
			{
				Log.CollectionManager.PrintError("Unable to retrieve emote from event");
				break;
			}
			bcd.ShowEmoteDetailsDisplay(emoteData2, collectionDataModel);
			MarkEmoteSeen(emoteData2);
			break;
		}
		case "EMOTE_hover_end":
		{
			BattlegroundsEmoteDataModel emoteData = GetEventEmoteDataModel();
			if (emoteData == null)
			{
				Log.CollectionManager.PrintError("Unable to retrieve emote from event");
			}
			else
			{
				MarkEmoteSeen(emoteData);
			}
			break;
		}
		case "EMOTE_drag_started":
			OnEmoteDragStart();
			break;
		case "EMOTE_drag_released":
			CollectionInputMgr.Get().DropBattlegroundsEmote(dragCanceled: false);
			break;
		}
	}

	public override void MarkAllShownCardsSeen()
	{
		base.MarkAllShownCardsSeen();
		MarkAllShownBoardsSeen();
		MarkAllShownFinishersSeen();
		MarkAllShownEmotesSeen();
	}

	private static void MarkBoardSeen(BattlegroundsBoardSkinDataModel boardData)
	{
		if (boardData == null)
		{
			Error.AddDevFatal("BaconCollectionPageDisplay.MarkBoardSeen - null board data model!");
			return;
		}
		boardData.IsNew = false;
		CollectionManager.Get().MarkBattlegroundsBoardSkinSeen(BattlegroundsBoardSkinId.FromTrustedValue(boardData.BoardDbiId));
	}

	private static void MarkFinisherSeen(BattlegroundsFinisherDataModel finisherData)
	{
		if (finisherData == null)
		{
			Error.AddDevFatal("BaconCollectionPageDisplay.MarkFinisherSeen - null finisher data model");
			return;
		}
		finisherData.IsNew = false;
		CollectionManager.Get().MarkBattlegroundsFinisherSeen(BattlegroundsFinisherId.FromTrustedValue(finisherData.FinisherDbiId));
	}

	private static void MarkEmoteSeen(BattlegroundsEmoteDataModel emoteData)
	{
		if (emoteData == null)
		{
			Error.AddDevFatal("BaconCollectionPageDisplay.MarkEmoteSeen - null emote data model");
			return;
		}
		emoteData.IsNew = false;
		CollectionManager.Get().MarkBattlegroundsEmoteSeen(BattlegroundsEmoteId.FromTrustedValue(emoteData.EmoteDbiId));
	}

	private void OnBoardDisplayReady(Widget widget)
	{
		if (widget != null)
		{
			widget.RegisterEventListener(BoardSkinDisplayEventListener);
		}
		m_BoardSkinsWidgetInstance = widget;
	}

	private void OnFinisherDisplayReady(Widget widget)
	{
		if (widget != null)
		{
			widget.RegisterEventListener(FinisherDisplayEventListener);
		}
		m_FinishersWidgetInstance = widget;
	}

	private void OnEmoteDisplayReady(Widget widget)
	{
		if (widget != null)
		{
			widget.RegisterEventListener(EmoteDisplayEventListener);
		}
		m_EmotesWidgetInstance = widget;
	}

	private void MarkAllShownBoardsSeen()
	{
		if (!m_BoardSkinsWidget.GetDataModel(565, out var dataModel))
		{
			return;
		}
		if (!(dataModel is BattlegroundsBoardSkinCollectionPageDataModel pageModel))
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownBoardsSeen - data model of unexpected type!");
			return;
		}
		if (pageModel.BoardSkinList == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownBoardsSeen - data model list was null!");
			return;
		}
		foreach (BattlegroundsBoardSkinDataModel boardSkin in pageModel.BoardSkinList)
		{
			MarkBoardSeen(boardSkin);
		}
	}

	private void MarkAllShownFinishersSeen()
	{
		if (!m_FinishersWidget.GetDataModel(568, out var dataModel))
		{
			return;
		}
		if (!(dataModel is BattlegroundsFinisherCollectionPageDataModel pageModel))
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownFinishersSeen - data model of unexpected type!");
			return;
		}
		if (pageModel.FinisherList == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownFinishersSeen - data model list was null!");
			return;
		}
		foreach (BattlegroundsFinisherDataModel finisher in pageModel.FinisherList)
		{
			MarkFinisherSeen(finisher);
		}
	}

	private void MarkAllShownEmotesSeen()
	{
		if (!m_EmotesWidget.GetDataModel(639, out var dataModel))
		{
			return;
		}
		if (!(dataModel is BattlegroundsEmoteCollectionPageDataModel pageModel))
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownEmotesSeen - data model of unexpected type!");
			return;
		}
		if (pageModel.EmoteList == null)
		{
			Log.All.PrintError("BaconCollectionPageDisplay.MarkAllShownEmotesSeen - data model list was null!");
			return;
		}
		foreach (BattlegroundsEmoteDataModel emote in pageModel.EmoteList)
		{
			MarkEmoteSeen(emote);
		}
	}

	private void OnEmoteDragStart()
	{
		if (m_EmotesWidgetInstance == null)
		{
			return;
		}
		BaconCollectionDisplay bcd = CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay;
		if (!(bcd != null) || !bcd.IsEmoteDetailsShowing())
		{
			BattlegroundsEmoteDataModel emoteData = GetEventEmoteDataModel();
			if (emoteData == null)
			{
				Log.CollectionManager.PrintError("Unable to retrieve emote from event");
			}
			else if (!emoteData.IsOwned)
			{
				Log.CollectionManager.PrintError("Emote not owned");
			}
			else
			{
				CollectionInputMgr.Get().GrabBattlegroundsEmote(emoteData, CollectionUtils.BattlegroundsModeDraggableType.CollectionEmote);
			}
		}
	}

	private BattlegroundsEmoteDataModel GetEventEmoteDataModel()
	{
		EventDataModel eventDataModel = m_EmotesWidgetInstance.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.CollectionManager.PrintError("No event data model attached to BaconCollectionPageDisplay");
			return null;
		}
		return eventDataModel.Payload as BattlegroundsEmoteDataModel;
	}

	public static void SetPageFlavorTextures(GameObject header, HEADER_CLASS headerClass)
	{
		if (!(header == null))
		{
			float x = (float)((int)headerClass / 8) * 0.5f;
			float y = (float)headerClass / -8f;
			CollectiblePageDisplay.SetPageFlavorTextures(header, new UnityEngine.Vector2(x, y));
		}
	}
}
