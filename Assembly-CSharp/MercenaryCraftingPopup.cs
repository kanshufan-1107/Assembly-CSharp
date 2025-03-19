using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

public class MercenaryCraftingPopup : MonoBehaviour
{
	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercCraftingPopupReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_mercCardReference;

	private Widget m_mercPopupManagerVisualController;

	private VisualController m_mercCraftingPopupVisualController;

	private Hearthstone.UI.Card m_mercCard;

	private MaterialDataModel m_materialData = new MaterialDataModel();

	private void Start()
	{
		m_mercPopupManagerVisualController = base.gameObject.GetComponent<Widget>();
		m_mercPopupManagerVisualController.RegisterEventListener(MercPopupEventListener);
		m_mercCraftingPopupReference.RegisterReadyListener(delegate(VisualController vc)
		{
			m_mercCraftingPopupVisualController = vc;
			m_mercCraftingPopupVisualController.BindDataModel(m_materialData);
		});
		m_mercCardReference.RegisterReadyListener(delegate(Hearthstone.UI.Card card)
		{
			m_mercCard = card;
		});
		Network.Get().RegisterNetHandler(CraftMercenaryResponse.PacketID.ID, OnCraftMercenaryNetworkResponse);
	}

	private void OnDestroy()
	{
		Network.Get()?.RemoveNetHandler(CraftMercenaryResponse.PacketID.ID, OnCraftMercenaryNetworkResponse);
	}

	private void MercPopupEventListener(string eventName)
	{
		if (!(eventName == "MERC_CRAFT_code"))
		{
			if (eventName == "MERC_CRAFT_COMPLETE_code")
			{
				OnCraftMercenaryComplete();
			}
		}
		else
		{
			CraftMercenary();
		}
	}

	public void ShowCraftingPopup(LettuceMercenaryDataModel mercData)
	{
		if (m_mercPopupManagerVisualController == null)
		{
			Log.Lettuce.PrintWarning("MercenaryCraftingPopup.ShowCraftingPopup - no merc popup manager visual controller found!");
			return;
		}
		m_mercPopupManagerVisualController.BindDataModel(mercData);
		m_mercPopupManagerVisualController.TriggerEvent("MERC_CRAFTING_POPUP_show");
	}

	private void CraftMercenary()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercPopupManagerVisualController.GetComponent<VisualController>());
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the LettuceMercCraftingPopup");
			return;
		}
		if (!(eventDataModel.Payload is LettuceMercenaryDataModel mercData))
		{
			Log.Lettuce.PrintError("Event data attached to LettuceMercCraftingPopup not of expected type!");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.CraftMercenary - no mercenary found with ID {0}", mercData.MercenaryId);
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		if (merc.m_currencyAmount < merc.GetCraftingCost())
		{
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_MERCENARY_CRAFTING_CONFIRMATION_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_MERCENARY_CRAFTING_NOT_ENOUGH_COIN_BODY");
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		}
		else
		{
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_MERCENARY_CRAFTING_CONFIRMATION_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_MERCENARY_CRAFTING_CONFIRMATION_BODY");
			info.m_showAlertIcon = false;
			info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_confirmText = GameStrings.Get("GLUE_LETTUCE_MERCENARY_CRAFT_TITLE");
			info.m_cancelText = GameStrings.Get("GLOBAL_CANCEL");
			info.m_responseCallback = OnMercenaryCraftingPopupResponse;
			info.m_responseUserData = mercData;
		}
		DialogManager.Get().ShowPopup(info);
	}

	private void OnMercenaryCraftingPopupResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL || !(userData is LettuceMercenaryDataModel mercData))
		{
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.OnMercenaryCraftingPopupResponse - no mercenary found with ID {0}", mercData.MercenaryId);
			return;
		}
		if (merc.m_owned)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.OnMercenaryCraftingPopupResponse - mercenary ID {0} in craft request already owned!", merc.ID);
			return;
		}
		if (merc.m_currencyAmount < merc.GetCraftingCost())
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.OnMercenaryCraftingPopupResponse - Mercenary ID {0} requires {1} coins to craft, but only has {2}", merc.ID, merc.GetCraftingCost(), merc.m_currencyAmount);
			return;
		}
		if (Network.IsLoggedIn())
		{
			Network.Get().CraftMercenary(merc.ID);
		}
		m_materialData.Material = m_mercCard.CardActor.GetPortraitMaterial();
		if (m_mercCraftingPopupVisualController != null)
		{
			m_mercCraftingPopupVisualController.OwningWidget.TriggerEvent("PLAY_EFFECTS");
		}
	}

	private void OnCraftMercenaryNetworkResponse()
	{
		CraftMercenaryResponse response = Network.Get().CraftMercenaryResponse();
		if (response.ErrorCode != 0)
		{
			Log.Lettuce.PrintError("LettuceCollectionDisplay.OnCraftMercenaryNetworkResponse - Error Code {0} crafting mercenary ID {1}", response.ErrorCode, response.MercenaryId);
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(response.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.OnCraftMercenaryNetworkResponse - No mercenary found with ID {0}.", response.MercenaryId);
		}
		else
		{
			merc.m_owned = true;
			merc.m_currencyAmount = response.CurrencyFinal;
		}
	}

	private void OnCraftMercenaryComplete()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_mercPopupManagerVisualController.GetComponent<VisualController>());
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the LettuceMercCraftingPopup");
			return;
		}
		if (!(eventDataModel.Payload is LettuceMercenaryDataModel mercData))
		{
			Log.Lettuce.PrintError("Event data attached to LettuceMercCraftingPopup not of expected type!");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercData.MercenaryId);
		if (merc == null)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.CraftMercenary - no mercenary found with ID {0}", mercData.MercenaryId);
			return;
		}
		LettuceCollectionPageManager lcpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager;
		if (lcpm == null)
		{
			Log.Lettuce.PrintWarning("LettuceCollectionDisplay.OnCraftMercenaryNetworkResponse - Unable to retrieve LettuceCollectionPageManager!");
		}
		else
		{
			lcpm.UpdatePageMercenary(MercenaryFactory.CreateMercenaryDataModelWithCoin(merc));
		}
	}
}
