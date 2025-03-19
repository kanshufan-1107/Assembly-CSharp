using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class ProductPageContainer : MonoBehaviour
{
	[SerializeField]
	private GameObject m_pageRoot;

	private Widget m_widget;

	private ProductDataModel m_product = ProductFactory.CreateEmptyProductDataModel();

	private MusicPlaylistBookmark m_musicPlaylistBookmark;

	private MusicPlaylistType m_musicOverride;

	private readonly List<ProductPage> m_pages = new List<ProductPage>();

	private List<WidgetInstance> m_tempInstances = new List<WidgetInstance>();

	private ProductPage m_currentProductPage;

	private bool m_tempInstancesHaveBeenInitialized;

	private const string OPEN = "OPEN";

	private const string CLOSED = "CLOSED";

	private const string EVENT_DISMISS = "CODE_DISMISS";

	private const string EVENT_NO_MUSIC = "NO_MUSIC";

	private readonly PlatformDependentValue<bool> UnloadUnusedAssetsOnClose = new PlatformDependentValue<bool>(PlatformCategory.Memory)
	{
		LowMemory = true,
		MediumMemory = true,
		HighMemory = false
	};

	private FrameTimer m_frameTimer;

	public bool IsOpen { get; private set; }

	public ProductDataModel Product => m_product;

	public ProductDataModel Variant { get; set; }

	[Overridable]
	public string MusicOverride
	{
		get
		{
			return m_musicOverride.ToString();
		}
		set
		{
			MusicPlaylistType playlist = MusicPlaylistType.Invalid;
			if (!string.IsNullOrEmpty(value))
			{
				try
				{
					object playlistObject = Enum.Parse(typeof(MusicPlaylistType), value, ignoreCase: true);
					if (playlistObject != null)
					{
						playlist = (MusicPlaylistType)playlistObject;
					}
				}
				catch (Exception)
				{
					Debug.LogErrorFormat("Invalid playlist name '{0}'", value);
				}
			}
			OverrideMusic(playlist);
		}
	}

	public event EventHandler OnOpened;

	public event EventHandler OnClosed;

	public event Action OnProductSet;

	protected virtual void Awake()
	{
		if (m_pageRoot != null)
		{
			m_pageRoot.SetActive(value: false);
		}
		else
		{
			Log.Store.PrintError("ProductPageContainer missing reference to product page root object. This may prevent pages from opening!");
		}
	}

	protected virtual void Start()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(delegate(string evt)
		{
			if (evt == "CODE_DISMISS")
			{
				DynamicVideoLoader componentInChildren = GetComponentInChildren<DynamicVideoLoader>();
				if (componentInChildren != null)
				{
					componentInChildren.OnClosed();
				}
				UIContext.GetRoot().DismissPopup(m_widget.gameObject);
				if (m_pageRoot != null)
				{
					m_pageRoot.SetActive(value: false);
				}
			}
		});
		GetComponentsInChildren(includeInactive: true, m_tempInstances);
		m_tempInstances.RemoveAll((WidgetInstance w) => !w.name.Contains("[temp]"));
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.OnOpened += HandleShopOpened;
			shop.OnCloseCompleted += HandleShopClosed;
		}
	}

	protected virtual void OnDestroy()
	{
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.OnOpened -= HandleShopOpened;
			shop.OnCloseCompleted -= HandleShopClosed;
		}
		DynamicVideoLoader player = GetComponentInChildren<DynamicVideoLoader>();
		if (player != null)
		{
			player.OnClosed();
		}
		foreach (ProductPage page in m_pages)
		{
			if ((object)page != null)
			{
				page.OnOpened -= HandleProductPageOpened;
				page.OnClosed -= HandleProductPageClosed;
			}
		}
		m_pages.Clear();
		m_tempInstances = null;
	}

	public void Open()
	{
		Open(m_product, Variant);
	}

	public void Open(ProductDataModel product, ProductDataModel variant = null)
	{
		if (product == null || product == ProductFactory.CreateEmptyProductDataModel())
		{
			Log.Store.PrintError("ProductPageContainer cannot open null or empty product");
			return;
		}
		base.gameObject.SetActive(value: true);
		if (IsOpen)
		{
			return;
		}
		if (m_pageRoot == null)
		{
			Log.Store.PrintError("ProductPageContainer missing reference to the product page root object");
			return;
		}
		m_frameTimer = new FrameTimer();
		m_frameTimer.StartRecording();
		m_pageRoot.SetActive(value: true);
		IsOpen = true;
		SetProduct(product, variant);
		if (m_musicOverride != 0)
		{
			OverrideMusic(m_musicOverride);
		}
		StartCoroutine(OpenProductPageCoroutine());
	}

	public void Close()
	{
		if (!IsOpen)
		{
			return;
		}
		if (m_currentProductPage != null)
		{
			if (m_currentProductPage.IsOpen)
			{
				m_currentProductPage.Close();
				return;
			}
			m_currentProductPage = null;
		}
		IsOpen = false;
		StopMusicOverride();
		SetProduct(null);
		m_widget.TriggerEvent("CLOSED", TriggerEventParameters.Standard);
		if (this.OnClosed != null)
		{
			this.OnClosed(this, new EventArgs());
		}
		if ((bool)UnloadUnusedAssetsOnClose && HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().UnloadUnusedAssets();
		}
	}

	public ProductPage GetCurrentProductPage()
	{
		return m_currentProductPage;
	}

	public void RegisterProductPage(ProductPage page)
	{
		m_pages.Add(page);
		page.OnOpened += HandleProductPageOpened;
		page.OnClosed += HandleProductPageClosed;
	}

	public void UnregisterProductPage(ProductPage page)
	{
		m_pages.Remove(page);
		page.OnOpened -= HandleProductPageOpened;
		page.OnClosed -= HandleProductPageClosed;
	}

	public void SetProduct(ProductDataModel product, ProductDataModel variant = null)
	{
		product = product ?? ProductFactory.CreateEmptyProductDataModel();
		Variant = variant ?? product;
		if (product != m_product)
		{
			m_product = product;
			BindCurrentProduct();
			if (this.OnProductSet != null)
			{
				this.OnProductSet();
			}
		}
	}

	public void OverrideMusic(MusicPlaylistType playlist)
	{
		if (m_musicOverride == playlist)
		{
			return;
		}
		m_musicOverride = playlist;
		if (!IsOpen)
		{
			return;
		}
		if (m_musicOverride == MusicPlaylistType.Invalid)
		{
			StopMusicOverride();
			return;
		}
		if (m_musicPlaylistBookmark == null)
		{
			m_musicPlaylistBookmark = MusicManager.Get().CreateBookmarkOfCurrentPlaylist();
		}
		MusicManager.Get().StartPlaylist(m_musicOverride);
	}

	public void StopMusicOverride()
	{
		if (m_musicPlaylistBookmark != null)
		{
			MusicManager musicMgr = MusicManager.Get();
			if (musicMgr != null)
			{
				musicMgr.StopPlaylist();
				musicMgr.PlayFromBookmark(m_musicPlaylistBookmark);
			}
			m_musicPlaylistBookmark = null;
		}
		m_musicOverride = MusicPlaylistType.Invalid;
	}

	public void InitializeTempInstances()
	{
		if (m_tempInstancesHaveBeenInitialized)
		{
			return;
		}
		m_tempInstancesHaveBeenInitialized = true;
		foreach (WidgetInstance instance in m_tempInstances)
		{
			ForceInitializeTempInstance(instance);
		}
	}

	protected void BindCurrentProduct()
	{
		m_widget.BindDataModel(m_product);
	}

	protected void HandleProductPageOpened(object sender, EventArgs e)
	{
		ProductPage page = sender as ProductPage;
		if (m_currentProductPage != null)
		{
			Log.Store.PrintError("Previous product page did not close properly: {0}", m_currentProductPage.gameObject.name);
		}
		PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = true;
		m_currentProductPage = page;
	}

	protected void HandleProductPageClosed(object sender, EventArgs e)
	{
		ProductPage page = sender as ProductPage;
		if (m_currentProductPage == page)
		{
			m_currentProductPage = null;
			Close();
		}
		else
		{
			Log.Store.PrintError("Product page closed but it is not the currently open page: {0}", page.gameObject.name);
		}
		PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = false;
	}

	protected void HandleShopOpened()
	{
		foreach (WidgetInstance instance in m_tempInstances)
		{
			StartCoroutine(PreloadPageInstanceCoroutine(instance));
		}
	}

	protected IEnumerator PreloadPageInstanceCoroutine(WidgetInstance instance)
	{
		yield return new WaitForSeconds(0.1f);
		Shop shop = Shop.Get();
		while (shop != null && !shop.IsBrowserReady && shop.IsOpen())
		{
			yield return null;
		}
		if (!(shop == null) && shop.IsOpen())
		{
			while (instance.IsChangingStates)
			{
				yield return null;
			}
			instance.Initialize();
			bool wasActive = instance.gameObject.activeSelf;
			instance.gameObject.SetActive(value: true);
			yield return null;
			instance.gameObject.SetActive(wasActive);
		}
	}

	protected void ForceInitializeTempInstance(WidgetInstance instance)
	{
		instance.Initialize();
		instance.gameObject.SetActive(value: true);
	}

	protected void HandleShopClosed()
	{
		m_tempInstancesHaveBeenInitialized = false;
		SetProduct(null);
		m_tempInstances.ForEach(delegate(WidgetInstance i)
		{
			i.Unload();
		});
		m_pages.Clear();
	}

	protected IEnumerator OpenProductPageCoroutine()
	{
		while (m_widget.IsChangingStates && IsOpen)
		{
			yield return null;
		}
		if (!IsOpen)
		{
			yield break;
		}
		UIContext.GetRoot().ShowPopup(m_widget.gameObject, UIContext.BlurType.Layered, UIContext.ProjectionType.Perspective);
		WidgetInstance activeInstance = m_tempInstances.FirstOrDefault((WidgetInstance i) => i.gameObject.activeInHierarchy);
		if (activeInstance == null)
		{
			Log.Store.PrintError("Failed to activate any product page for data model.");
			Close();
			yield break;
		}
		activeInstance.Initialize();
		while (IsOpen && (!activeInstance.IsReady || activeInstance.IsChangingStates))
		{
			yield return null;
		}
		ProductPage activePage = m_pages.FirstOrDefault((ProductPage p) => p.gameObject.activeInHierarchy);
		if (activePage == null)
		{
			Log.Store.PrintError("Failed to instantiate any product page for data model.");
			Close();
			yield break;
		}
		activePage.Open();
		while (activePage.WidgetComponent.IsChangingStates && IsOpen)
		{
			yield return null;
		}
		if (IsOpen)
		{
			m_widget.RegisterReadyListener(OnWidgetReady);
			m_frameTimer.StopRecording();
			TelemetryManager.Client().SendShopProductDetailOpened(m_product.PmtId, m_frameTimer.TimeTaken);
			this.OnOpened?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnWidgetReady(object _)
	{
		m_widget.TriggerEvent("OPEN", TriggerEventParameters.Standard);
		VisualController[] components = GetComponents<VisualController>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].SetState("OPEN");
		}
	}
}
