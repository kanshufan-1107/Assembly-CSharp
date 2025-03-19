using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class LettuceCollectionPageDisplay : CollectiblePageDisplay
{
	public enum HEADER_ROLE
	{
		INVALID,
		FIGHTER,
		CASTER,
		TANK
	}

	public GameObject m_noMatchFoundObject;

	public UberText m_noMatchExplanationText;

	public GameObject m_noMatchSetHintObject;

	public GameObject m_noMatchManaHintObject;

	public GameObject m_noMatchCraftingHintObject;

	public AsyncReference m_cardDisplayReference;

	private VisualController m_cardDisplayVisualController;

	private bool m_cardDisplayFinishedLoading;

	public void Start()
	{
		m_cardDisplayReference.RegisterReadyListener<VisualController>(OnCardDisplayReady);
	}

	public override bool IsLoaded()
	{
		return m_cardDisplayFinishedLoading;
	}

	public override void UpdateCollectionItems(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectionList, CollectionUtils.ViewMode mode)
	{
		base.UpdateCollectionItems(actorList, nonActorCollectionList, mode);
	}

	public void UpdateCollectionMercs(List<LettuceMercenary> mercList, BookPageManager.PageTransitionType transitionType = BookPageManager.PageTransitionType.NONE)
	{
		if (!m_cardDisplayFinishedLoading)
		{
			return;
		}
		LettuceCollectionPageDataModel dataModel = GetCollectionPageDataModel();
		if (dataModel == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionPageDisplay.UpdateCollectionMercs - could not find data model!");
			return;
		}
		DataModelList<LettuceMercenaryDataModel> mercDataList = mercList?.Select(MercenaryFactory.CreateMercenaryDataModelWithCoin)?.ToDataModelList();
		if (mercDataList == null)
		{
			mercDataList = new DataModelList<LettuceMercenaryDataModel>();
		}
		dataModel.MercenaryList = mercDataList;
		dataModel.CraftingModeActive = CollectionManager.Get().GetCollectibleDisplay().InCraftingMode();
		m_cardDisplayVisualController.OwningWidget.TriggerEvent("SHOW_COIN_TRAY");
	}

	public void UpdateMercenaryOnPage(LettuceMercenaryDataModel dataModel)
	{
		LettuceCollectionPageDataModel pageData = GetCollectionPageDataModel();
		if (pageData == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionPageDisplay.UpdateMercenaryOnPage - could not find data model!");
			return;
		}
		for (int i = 0; i < pageData.MercenaryList.Count; i++)
		{
			LettuceMercenaryDataModel mercData = pageData.MercenaryList[i];
			if (mercData != null && mercData.MercenaryId == dataModel.MercenaryId)
			{
				pageData.MercenaryList[i] = dataModel.CloneDataModel();
				m_cardDisplayVisualController.SetState("UPDATE_UPGRADE_STATUS");
				break;
			}
		}
	}

	public void UpdateAcknowledgeStatusForMercenaryOnPage(int mercId, bool status)
	{
		LettuceMercenaryDataModel mercModel = GetMercenaryOnPage(mercId);
		if (mercModel != null)
		{
			mercModel.ShowAsNew = status;
			if (!status)
			{
				mercModel.NumNewPortraits = 0;
			}
		}
	}

	public LettuceMercenaryDataModel GetMercenaryOnPage(int mercenaryId)
	{
		LettuceCollectionPageDataModel pageData = GetCollectionPageDataModel();
		if (pageData == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionPageDisplay.UpdateMercenaryOnPage - could not find data model!");
			return null;
		}
		for (int i = 0; i < pageData.MercenaryList.Count; i++)
		{
			LettuceMercenaryDataModel mercData = pageData.MercenaryList[i];
			if (mercData != null && mercData.MercenaryId == mercenaryId)
			{
				return mercData;
			}
		}
		return null;
	}

	public override void ShowNoMatchesFound(bool show, CollectionManager.FindCardsResult findResults = null, bool showHints = true)
	{
		m_noMatchFoundObject.SetActive(show);
		m_noMatchCraftingHintObject.SetActive(value: false);
		m_noMatchSetHintObject.SetActive(value: false);
		m_noMatchManaHintObject.SetActive(value: false);
		string explanationTextKey = "GLUE_COLLECTION_NO_RESULTS";
		if (show && showHints && findResults != null)
		{
			if (findResults.m_resultsWithoutManaFilterExist)
			{
				m_noMatchManaHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_SELECTED_COST";
			}
			else if (findResults.m_resultsWithoutSetFilterExist)
			{
				m_noMatchSetHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_CURRENT_SET";
			}
			else if (findResults.m_resultsUnownedExist)
			{
				m_noMatchCraftingHintObject.SetActive(value: true);
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_BUT_CRAFTABLE";
			}
			else if (findResults.m_resultsInWildExist)
			{
				explanationTextKey = "GLUE_COLLECTION_NO_RESULTS_IN_STANDARD";
			}
		}
		m_noMatchExplanationText.Text = GameStrings.Get(explanationTextKey);
	}

	public override void UpdateCurrentPageCardLocks(bool playSound = false)
	{
		base.UpdateCurrentPageCardLocks(playSound);
		DeckTrayMercListContent mercsContent = CollectionDeckTray.Get().GetMercsContent();
		if (!mercsContent.IsModeTryingOrActive())
		{
			return;
		}
		LettuceTeamDataModel teamDataModel = mercsContent.SelectedTeamDataModel;
		if (teamDataModel == null)
		{
			return;
		}
		foreach (LettuceMercenaryDataModel pageMerc in GetCollectionPageDataModel().MercenaryList)
		{
			pageMerc.InCurrentTeam = false;
			foreach (LettuceMercenaryDataModel teamMerc in teamDataModel.MercenaryList)
			{
				if (pageMerc.MercenaryId == teamMerc.MercenaryId)
				{
					pageMerc.InCurrentTeam = true;
				}
			}
		}
	}

	public void ClearCurrentPageCardLocks()
	{
		LettuceCollectionPageDataModel pageDataModel = GetCollectionPageDataModel();
		if (pageDataModel == null)
		{
			return;
		}
		foreach (LettuceMercenaryDataModel mercenary in pageDataModel.MercenaryList)
		{
			mercenary.InCurrentTeam = false;
		}
	}

	public override void SetPageType(FormatType formatType)
	{
	}

	public void SetRole(TAG_ROLE? roleTag)
	{
		if (!roleTag.HasValue)
		{
			SetPageNameText("");
			if (m_pageFlavorHeader != null)
			{
				m_pageFlavorHeader.SetActive(value: false);
			}
		}
		else
		{
			TAG_ROLE pageRole = roleTag.Value;
			SetPageNameText(GameStrings.GetRoleName(pageRole));
			SetPageFlavorTextures(m_pageFlavorHeader, TagRoleToHeaderRole(pageRole));
		}
	}

	public void WaitForPageUpdate(Action<object> listener, object payload)
	{
		if (m_cardDisplayVisualController == null)
		{
			listener(payload);
		}
		else
		{
			m_cardDisplayVisualController.OwningWidget.RegisterDoneChangingStatesListener(listener, payload, callImmediatelyIfSet: true, doOnce: true);
		}
	}

	private void OnCardDisplayReady(VisualController visualController)
	{
		if (visualController != null)
		{
			visualController.GetComponent<Widget>().RegisterEventListener(CardDisplayEventListener);
		}
		m_cardDisplayVisualController = visualController;
		m_cardDisplayFinishedLoading = true;
	}

	private LettuceCollectionPageDataModel GetCollectionPageDataModel()
	{
		if (m_cardDisplayVisualController == null)
		{
			return null;
		}
		Widget owner = m_cardDisplayVisualController.Owner;
		if (!owner.GetDataModel(259, out var dataModel))
		{
			dataModel = new LettuceCollectionPageDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceCollectionPageDataModel;
	}

	private void CardDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "MERCENARY_released":
			OnMercenaryReleased();
			break;
		case "MERCENARY_drag_started":
			OnMercenaryDragStart();
			break;
		case "MERCENARY_drag_released":
			CollectionInputMgr.Get().DropMercenariesModeCard(dragCanceled: false);
			break;
		}
	}

	private void OnMercenaryReleased()
	{
		if (CollectionInputMgr.Get().HasHeldCard())
		{
			return;
		}
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_cardDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to LettuceCollectionPageDisplay");
			return;
		}
		LettuceMercenaryDataModel mercData = (LettuceMercenaryDataModel)eventDataModel.Payload;
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		LettuceCollectionDisplay lcd = CollectionManager.Get().GetCollectibleDisplay() as LettuceCollectionDisplay;
		if (lcd == null)
		{
			Log.Lettuce.PrintError("LettuceCollectionPageDisplay.OnMercenaryReleased - LettuceCollectionDisplay is null!");
		}
		else if (merc.m_owned)
		{
			lcd.ShowMercenaryDetailsDisplay(merc);
		}
		else
		{
			lcd.ShowMercCraftingPopup(mercData);
		}
	}

	private void OnMercenaryDragStart()
	{
		if (CollectionDeckTray.Get().GetCurrentContentType() != DeckTray.DeckContentTypes.Mercs)
		{
			return;
		}
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_cardDisplayVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to LettuceCollectionPageDisplay");
			return;
		}
		LettuceMercenaryDataModel mercData = (LettuceMercenaryDataModel)eventDataModel.Payload;
		if (CanPickUpMercenary(mercData))
		{
			CollectionInputMgr.Get().GrabMercenariesModeCard(mercData, CollectionUtils.MercenariesModeCardType.Mercenary);
		}
	}

	private bool CanPickUpMercenary(LettuceMercenaryDataModel mercData)
	{
		if (mercData.InCurrentTeam)
		{
			return false;
		}
		if (ShouldIgnoreAllInput())
		{
			return false;
		}
		if (!CollectionDeckTray.Get().CanPickupCard())
		{
			return false;
		}
		if (!CollectionManager.Get().GetMercenary(mercData.MercenaryId).m_owned)
		{
			return false;
		}
		return true;
	}

	private bool ShouldIgnoreAllInput()
	{
		if (!base.IsShown)
		{
			return true;
		}
		if (CollectionInputMgr.Get() != null && CollectionInputMgr.Get().IsDraggingScrollbar())
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

	public static HEADER_ROLE TagRoleToHeaderRole(TAG_ROLE roleTag)
	{
		string tagRole = roleTag.ToString();
		if (Enum.IsDefined(typeof(HEADER_ROLE), tagRole))
		{
			return (HEADER_ROLE)Enum.Parse(typeof(HEADER_ROLE), tagRole);
		}
		return HEADER_ROLE.INVALID;
	}

	public static void SetPageFlavorTextures(GameObject header, HEADER_ROLE headerRole)
	{
		if (!(header == null))
		{
			float y = (0f - (float)(headerRole - 1)) / 4f;
			CollectiblePageDisplay.SetPageFlavorTextures(header, new UnityEngine.Vector2(0f, y));
		}
	}
}
