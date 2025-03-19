using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

internal class ShopButtonStatus : MonoBehaviour
{
	public ShopType ShopType;

	private ShopButtonDataModel m_shopButtonDataModel;

	private StoreManager m_storeManager;

	private Shop m_shop;

	private bool m_isBattlegroundsShopAvailable;

	private void Start()
	{
		m_storeManager = StoreManager.Get();
		m_shop = Shop.Get();
		m_shopButtonDataModel = new ShopButtonDataModel();
		GlobalDataContext.Get().BindDataModel(m_shopButtonDataModel);
		RegisterEvents();
		m_shopButtonDataModel.IsShopOpen = m_storeManager?.IsOpen() ?? false;
		m_shopButtonDataModel.CanDisplayNewItemsBadge = CanDisplayNewItemsBadge();
	}

	private void Update()
	{
		if (!m_isBattlegroundsShopAvailable && ServiceManager.TryGet<IGamemodeAvailabilityService>(out var gamemodeAvailabilityService) && gamemodeAvailabilityService.GetGamemodeStatus(IGamemodeAvailabilityService.Gamemode.BATTLEGROUNDS) == IGamemodeAvailabilityService.Status.READY && ServiceManager.TryGet<IProductDataService>(out var productDataService) && productDataService.TryRefreshStaleProductAvailability())
		{
			m_shopButtonDataModel.HasNewItems = HasNewItems();
			m_isBattlegroundsShopAvailable = true;
		}
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		GlobalDataContext.Get().UnbindDataModel(m_shopButtonDataModel.DataModelId);
	}

	private void RegisterEvents()
	{
		UnregisterEvents();
		if (m_storeManager != null)
		{
			m_storeManager.RegisterStatusChangedListener(OnStoreStatusChanged);
		}
		if ((bool)m_shop)
		{
			m_shop.OnOpenCompleted += OnStoreOpenComplete;
			m_shop.OnTiersChanged += OnTiersChanged;
			m_shop.OnShopButtonStatusChanged += OnShopButtonStatusChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (m_storeManager != null && m_storeManager.GetCurrentStore() != null)
		{
			m_storeManager.RemoveStatusChangedListener(OnStoreStatusChanged);
		}
		if ((bool)m_shop)
		{
			m_shop.OnOpenCompleted -= OnStoreOpenComplete;
			m_shop.OnTiersChanged -= OnTiersChanged;
		}
	}

	private bool CanDisplayNewItemsBadge()
	{
		BoxRailroadManager railroadManager = Box.Get().GetRailroadManager();
		if (m_storeManager == null || Box.Get() == null || !GameUtils.IsAnyTutorialComplete() || (railroadManager != null && railroadManager.ShouldDisableButtonType(Box.ButtonType.STORE)))
		{
			Log.Store.Print("[ShopButtonStatus::CanDisplayNewItemsBadge] Badge can not be displayed. Reason:" + $" Store Manager: {m_storeManager == null}" + $" Box: {Box.Get() == null}" + $" RailroadManager: {railroadManager == null}" + $" Should Disabled Button Type: {railroadManager != null && railroadManager.ShouldDisableButtonType(Box.ButtonType.STORE)}" + $" Is Any Tutorial Complete: {GameUtils.IsAnyTutorialComplete()}");
			return false;
		}
		return true;
	}

	private bool HasNewItems()
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var productDataService))
		{
			return false;
		}
		bool newItems = productDataService.CheckForNewItems();
		Log.Store.Print($"[ShopButtonStatus::HasNewItems] HasNewItems = {newItems}");
		return newItems;
	}

	private void OnStoreOpenComplete()
	{
		m_shopButtonDataModel.CanDisplayNewItemsBadge = CanDisplayNewItemsBadge();
		m_shopButtonDataModel.HasNewItems = HasNewItems();
	}

	private void OnTiersChanged()
	{
		m_shopButtonDataModel.CanDisplayNewItemsBadge = CanDisplayNewItemsBadge();
	}

	private void OnStoreStatusChanged(bool isOpen)
	{
		m_shopButtonDataModel.IsShopOpen = isOpen;
		if (isOpen)
		{
			m_shopButtonDataModel.HasNewItems = HasNewItems();
		}
	}

	private void OnShopButtonStatusChanged()
	{
		m_shopButtonDataModel.HasNewItems = HasNewItems();
	}
}
