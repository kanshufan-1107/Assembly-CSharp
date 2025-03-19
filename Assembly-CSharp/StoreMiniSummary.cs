using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.Store;
using UnityEngine;

public class StoreMiniSummary : MonoBehaviour
{
	public UberText m_headlineText;

	public UberText m_itemsHeadlineText;

	public UberText m_itemsText;

	private void Awake()
	{
		m_headlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_HEADLINE");
		m_itemsHeadlineText.Text = GameStrings.Get("GLUE_STORE_SUMMARY_ITEMS_ORDERED_HEADLINE");
	}

	public void SetDetails(ProductId productId, int quantity)
	{
		m_itemsText.Text = GetItemsText(productId, quantity);
	}

	private string GetItemsText(ProductId productId, int quantity)
	{
		IProductDataService dataService;
		ProductInfo product;
		string productName = ((!ServiceManager.TryGet<IProductDataService>(out dataService) || !dataService.TryGetProduct(productId, out product)) ? GameStrings.Get("GLUE_STORE_PRODUCT_NAME_MOBILE_UNKNOWN") : product.Title);
		return GameStrings.Format("GLUE_STORE_SUMMARY_ITEM_ORDERED", quantity, productName);
	}
}
