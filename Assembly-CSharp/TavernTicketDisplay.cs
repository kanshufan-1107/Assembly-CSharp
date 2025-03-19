using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class TavernTicketDisplay : MonoBehaviour
{
	public Widget m_widget;

	private const string PURCHASE_TAVERN_TICKET = "PURCHASE_TAVERN_TICKET";

	private void OnEnable()
	{
		if (m_widget != null)
		{
			m_widget.RegisterReadyListener(OnWidgetReady);
		}
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
	}

	private void UnregisterEvents()
	{
		if (m_widget != null)
		{
			m_widget.RemoveReadyListener(OnWidgetReady);
		}
		StoreManager.Get()?.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
	}

	private void OnWidgetReady(object unused)
	{
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(HandleTavernTicketDisplayEvent);
		}
		StoreManager.Get()?.RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
	}

	private void HandleTavernTicketDisplayEvent(string eventName)
	{
		if (eventName == "PURCHASE_TAVERN_TICKET")
		{
			OpenTavernTicketProductPage();
		}
	}

	private void OpenTavernTicketProductPage()
	{
		ProductPageJobs.OpenToProductPageWhenReady(327L, dontFullyOpenShop: true);
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		Shop shop = Shop.Get();
		if (!(shop == null) && shop.IsTavernTicketProduct(bundle) && shop.IsTavernTicketProductPageOpen())
		{
			shop.Close(forceClose: true);
		}
	}
}
