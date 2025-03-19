using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class CardBackInfoManager : MonoBehaviour, IStore
{
	private const string STATE_MAKE_FAVORITE = "MAKE_FAVORITE";

	private const string STATE_SUFFICIENT_CURRENCY = "SUFFICIENT_CURRENCY";

	private const string STATE_INSUFFICIENT_CURRENCY = "INSUFFICIENT_CURRENCY";

	private const string STATE_DISABLED = "DISABLED";

	private const string STATE_VISIBLE = "VISIBLE";

	private const string STATE_HIDDEN = "HIDDEN";

	private const string STATE_BLOCK_SCREEN = "BLOCK_SCREEN";

	private const string STATE_UNBLOCK_SCREEN = "UNBLOCK_SCREEN";

	public GameObject m_previewPane;

	public GameObject m_cardBackContainer;

	public UberText m_title;

	public UberText m_description;

	public UIBButton m_buyButton;

	public UIBButton m_favoriteButton;

	public PegUIElement m_offClicker;

	public float m_animationTime = 0.5f;

	public AsyncReference m_userActionVisualControllerReference;

	public AsyncReference m_visibilityVisualControllerReference;

	public AsyncReference m_fullScreenBlockerWidgetReference;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_enterPreviewSound;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_exitPreviewSound;

	private int? m_currentCardBackIdx;

	private GameObject m_currentCardBack;

	private bool m_animating;

	private VisualController m_userActionVisualController;

	private VisualController m_visibilityVisualController;

	private Widget m_fullScreenBlockerWidget;

	private bool m_isStoreOpen;

	private bool m_isStoreTransactionActive;

	private static CardBackInfoManager s_instance;

	private static bool s_isReadyingInstance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	public bool IsPreviewing { get; private set; }

	public event Action OnOpened;

	public event Action<StoreClosedArgs> OnClosed;

	public event Action OnReady;

	public event Action<BuyProductEventArgs> OnProductPurchaseAttempt;

	public event Action OnProductOpened;

	public static CardBackInfoManager Get()
	{
		return s_instance;
	}

	public static void EnterPreviewWhenReady(CollectionCardVisual cardVisual)
	{
		CardBackInfoManager infoManager = Get();
		if (infoManager != null)
		{
			infoManager.EnterPreview(cardVisual);
			return;
		}
		if (s_isReadyingInstance)
		{
			Debug.LogWarning("CardBackInfoManager:EnterPreviewWhenReady called while the info manager instance was being readied");
			return;
		}
		string cardBackInfoManagerPrefab = "CardBackInfoManager.prefab:d53d863de659e4cce97ba2ce0107fb49";
		Widget widget = WidgetInstance.Create(cardBackInfoManagerPrefab);
		if (widget == null)
		{
			Debug.LogError("CardBackInfoManager:EnterPreviewWhenReady failed to create widget instance");
			return;
		}
		s_isReadyingInstance = true;
		widget.RegisterReadyListener(delegate
		{
			s_instance = widget.GetComponentInChildren<CardBackInfoManager>();
			s_isReadyingInstance = false;
			if (s_instance == null)
			{
				Debug.LogError("CardBackInfoManager:EnterPreviewWhenReady created widget instance but failed to get CardBackInfoManager component");
			}
			else
			{
				s_instance.EnterPreview(cardVisual);
			}
		});
	}

	public static bool IsLoadedAndShowingPreview()
	{
		if (!s_instance)
		{
			return false;
		}
		return s_instance.IsPreviewing;
	}

	private void Awake()
	{
		m_previewPane.SetActive(value: false);
		SetupUI();
	}

	private void Start()
	{
		m_userActionVisualControllerReference.RegisterReadyListener<VisualController>(OnUserActionVisualControllerReady);
		m_visibilityVisualControllerReference.RegisterReadyListener<VisualController>(OnVisibilityVisualControllerReady);
		m_fullScreenBlockerWidgetReference.RegisterReadyListener<Widget>(OnFullScreenBlockerWidgetReady);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnDestroy()
	{
		if (CardBackManager.Get() != null)
		{
			CardBackManager.Get().OnFavoriteCardBacksChanged -= OnFavoriteCardBackChanged;
		}
		s_instance = null;
	}

	public void EnterPreview(CollectionCardVisual cardVisual)
	{
		this.OnProductOpened?.Invoke();
		Actor actor = cardVisual.GetActor();
		if (actor == null)
		{
			Debug.LogError("Unable to obtain actor from card visual.");
			return;
		}
		CollectionCardBack ccb = actor.GetComponent<CollectionCardBack>();
		if (ccb == null)
		{
			Debug.LogError("Actor does not contain a CollectionCardBack component!");
		}
		else
		{
			EnterPreview(ccb.GetCardBackId(), cardVisual);
		}
	}

	public void EnterPreview(int cardBackIdx, CollectionCardVisual cardVisual)
	{
		if (!m_animating)
		{
			if (m_currentCardBack != null)
			{
				UnityEngine.Object.Destroy(m_currentCardBack);
				m_currentCardBack = null;
			}
			m_animating = true;
			CollectionManagerDisplay cmd = CollectionManager.Get()?.GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.HideCardBackTips();
			}
			CardBackDbfRecord cardBackRecord = GameDbf.CardBack.GetRecord(cardBackIdx);
			m_title.Text = cardBackRecord.Name;
			m_description.Text = cardBackRecord.Description;
			m_currentCardBackIdx = cardBackIdx;
			IsPreviewing = true;
			SetupCardBackStore();
			UpdateView();
			if (!CardBackManager.Get().LoadCardBackByIndex(cardBackIdx, delegate(CardBackManager.LoadCardBackData cardBackData)
			{
				GameObject gameObject = cardBackData.m_GameObject;
				gameObject.name = "CARD_BACK_" + cardBackIdx;
				LayerUtils.SetLayer(gameObject, m_cardBackContainer.gameObject.layer, null);
				GameUtils.SetParent(gameObject, m_cardBackContainer);
				m_currentCardBack = gameObject;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					m_currentCardBack.transform.localPosition = Vector3.zero;
				}
				else
				{
					m_currentCardBack.transform.position = cardVisual.transform.position;
					Hashtable tweenHashTable = iTweenManager.Get().GetTweenHashTable();
					tweenHashTable.Add("name", "FinishBigCardMove");
					tweenHashTable.Add("position", m_cardBackContainer.transform.position);
					tweenHashTable.Add("time", m_animationTime);
					iTween.MoveTo(m_currentCardBack.gameObject, tweenHashTable);
					Hashtable tweenHashTable2 = iTweenManager.Get().GetTweenHashTable();
					tweenHashTable2.Add("scale", Vector3.one);
					tweenHashTable2.Add("time", m_animationTime);
					tweenHashTable2.Add("easetype", iTween.EaseType.easeOutQuad);
					iTween.ScaleTo(m_currentCardBack.gameObject, tweenHashTable2);
					Hashtable tweenHashTable3 = iTweenManager.Get().GetTweenHashTable();
					tweenHashTable3.Add("amount", new Vector3(0f, 0f, 75f));
					tweenHashTable3.Add("time", 2.5f);
					iTween.PunchRotation(m_currentCardBack, tweenHashTable3);
				}
				m_currentCardBack.transform.localScale = Vector3.one;
				m_currentCardBack.transform.localRotation = Quaternion.identity;
				m_previewPane.SetActive(value: true);
				m_offClicker.gameObject.SetActive(value: true);
				Hashtable tweenHashTable4 = iTweenManager.Get().GetTweenHashTable();
				tweenHashTable4.Add("scale", new Vector3(0.01f, 0.01f, 0.01f));
				tweenHashTable4.Add("time", m_animationTime);
				tweenHashTable4.Add("easetype", iTween.EaseType.easeOutCirc);
				tweenHashTable4.Add("oncomplete", (Action<object>)delegate
				{
					m_animating = false;
				});
				iTween.ScaleFrom(m_previewPane, tweenHashTable4);
			}))
			{
				Debug.LogError($"Unable to load card back ID {cardBackIdx} for preview.");
				m_animating = false;
			}
			if (!string.IsNullOrEmpty(m_enterPreviewSound))
			{
				SoundManager.Get().LoadAndPlay(m_enterPreviewSound);
			}
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = m_animationTime;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
	}

	public void CancelPreview()
	{
		if (!m_animating)
		{
			ShutDownCardBackStore();
			Vector3 origScale = m_previewPane.transform.localScale;
			IsPreviewing = false;
			m_animating = true;
			iTween.ScaleTo(m_previewPane, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
			{
				m_animating = false;
				m_previewPane.transform.localScale = origScale;
				m_previewPane.SetActive(value: false);
				m_offClicker.gameObject.SetActive(value: false);
			}));
			iTween.ScaleTo(m_currentCardBack, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
			{
				m_currentCardBack.SetActive(value: false);
			}));
			if (!string.IsNullOrEmpty(m_exitPreviewSound))
			{
				SoundManager.Get().LoadAndPlay(m_exitPreviewSound);
			}
			m_screenEffectsHandle.StopEffect();
		}
	}

	private void OnUserActionVisualControllerReady(VisualController visualController)
	{
		m_userActionVisualController = visualController;
		UpdateView();
		if (this.OnReady != null)
		{
			this.OnReady();
		}
	}

	private void OnVisibilityVisualControllerReady(VisualController visualController)
	{
		m_visibilityVisualController = visualController;
		UpdateView();
	}

	private void OnFullScreenBlockerWidgetReady(Widget fullScreenBlockerWidget)
	{
		m_fullScreenBlockerWidget = fullScreenBlockerWidget;
		UpdateView();
	}

	private void SetupUI()
	{
		m_buyButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnBuyButtonReleased();
		});
		m_favoriteButton.GetComponentInChildren<UberText>(includeInactive: true).Text = (CardBackManager.Get().MultipleFavoriteCardBacksEnabled() ? "GLUE_COLLECTION_MANAGER_FAVORITE_BUTTON_MULTIPLE" : "GLUE_COLLECTION_MANAGER_FAVORITE_BUTTON");
		m_favoriteButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			if (!m_currentCardBackIdx.HasValue)
			{
				Debug.LogError("CardBackInfoManager:FavoriteButtonRelease: m_currentCardBackIdx did not have a value");
			}
			else
			{
				CardBackManager.Get().HandleFavoriteToggle(m_currentCardBackIdx.Value);
				CancelPreview();
			}
		});
		m_offClicker.AddEventListener(UIEventType.RELEASE, delegate
		{
			CancelPreview();
		});
		m_offClicker.AddEventListener(UIEventType.RIGHTCLICK, delegate
		{
			CancelPreview();
		});
		CardBackManager.Get().OnFavoriteCardBacksChanged += OnFavoriteCardBackChanged;
	}

	public void OnFavoriteCardBackChanged(int cardBackId, bool isFavorite)
	{
		UpdateView();
	}

	private void OnBuyButtonReleased()
	{
		if (!m_currentCardBackIdx.HasValue)
		{
			Debug.LogError("CardBackInfoManager:OnBuyButtonReleased: m_currentCardBackIdx did not have a value");
			return;
		}
		m_visibilityVisualController.SetState("HIDDEN");
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Format("GLUE_CARD_BACK_PURCHASE_CONFIRMATION_HEADER");
		info.m_text = GameStrings.Format("GLUE_CARD_BACK_PURCHASE_CONFIRMATION_MESSAGE", m_title.Text);
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		AlertPopup.ResponseCallback callback = delegate(AlertPopup.Response response, object userdata)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				StartPurchaseTransaction();
			}
			else
			{
				m_visibilityVisualController.SetState("VISIBLE");
			}
		};
		info.m_responseCallback = callback;
		DialogManager.Get().ShowPopup(info);
	}

	private void UpdateView()
	{
		if (m_userActionVisualController == null || m_visibilityVisualController == null || m_fullScreenBlockerWidget == null || !m_currentCardBackIdx.HasValue)
		{
			return;
		}
		CardBackManager cardBackManager = CardBackManager.Get();
		bool canBuyCardBack = false;
		if (cardBackManager.IsCardBackOwned(m_currentCardBackIdx.Value))
		{
			m_userActionVisualController.SetState("MAKE_FAVORITE");
		}
		else if (!cardBackManager.IsCardBackPurchasableFromCollectionManager(m_currentCardBackIdx.Value))
		{
			m_userActionVisualController.SetState("DISABLED");
		}
		else
		{
			PriceDataModel priceDataModel = cardBackManager.GetCollectionManagerCardBackPriceDataModel(m_currentCardBackIdx.Value);
			m_userActionVisualController.BindDataModel(priceDataModel);
			if (!cardBackManager.CanBuyCardBackFromCollectionManager(m_currentCardBackIdx.Value))
			{
				m_userActionVisualController.SetState("INSUFFICIENT_CURRENCY");
			}
			else
			{
				m_userActionVisualController.SetState("SUFFICIENT_CURRENCY");
				canBuyCardBack = true;
			}
		}
		bool canToggleFavorite = cardBackManager.CanToggleFavoriteCardBack(m_currentCardBackIdx.Value);
		m_favoriteButton.SetEnabled(canToggleFavorite);
		m_favoriteButton.Flip(canToggleFavorite);
		m_buyButton.SetEnabled(canBuyCardBack);
		m_buyButton.Flip(faceUp: true);
	}

	private void BlockInputs(bool blocked)
	{
		if (m_fullScreenBlockerWidget == null)
		{
			Debug.LogError("Failed to toggle interface blocker from Duels Popup Manager");
		}
		else if (blocked)
		{
			m_fullScreenBlockerWidget.TriggerEvent("BLOCK_SCREEN", TriggerEventParameters.StandardPropagateDownward);
		}
		else
		{
			m_fullScreenBlockerWidget.TriggerEvent("UNBLOCK_SCREEN", TriggerEventParameters.StandardPropagateDownward);
		}
	}

	private void SetupCardBackStore()
	{
		if (m_isStoreOpen)
		{
			Debug.LogError("CardBackInfoManager:SetupCardBackStore called when the store was already open");
			return;
		}
		if (!m_currentCardBackIdx.HasValue)
		{
			Debug.LogError("CardBackInfoManager:SetupCardBackStore: m_currentCardBackIdx did not have a value");
			return;
		}
		StoreManager storeManager = StoreManager.Get();
		if (storeManager.IsOpen())
		{
			storeManager.SetupCardBackStore(this, m_currentCardBackIdx.Value);
			storeManager.RegisterSuccessfulPurchaseListener(OnSuccessfulPurchase);
			storeManager.RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
			storeManager.RegisterFailedPurchaseAckListener(OnFailedPurchaseAck);
			BnetBar.Get()?.RefreshCurrency();
		}
	}

	private void ShutDownCardBackStore()
	{
		if (m_isStoreOpen)
		{
			CancelPurchaseTransaction();
			this.OnClosed?.Invoke(new StoreClosedArgs());
			StoreManager storeManager = StoreManager.Get();
			storeManager.RemoveFailedPurchaseAckListener(OnFailedPurchaseAck);
			storeManager.RemoveSuccessfulPurchaseListener(OnSuccessfulPurchase);
			storeManager.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
			storeManager.ShutDownCardBackStore();
			this.OnProductPurchaseAttempt = null;
			BnetBar.Get()?.RefreshCurrency();
			BlockInputs(blocked: false);
			m_isStoreOpen = false;
		}
	}

	private void OnSuccessfulPurchase(ProductInfo bundle, PaymentMethod paymentMethod)
	{
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		EndPurchaseTransaction();
		CardBackManager.Get().AddNewCardBack(m_currentCardBackIdx.Value);
		CollectionManager.Get().RefreshCurrentPageContents();
		m_visibilityVisualController.SetState("VISIBLE");
		UpdateView();
	}

	private void OnFailedPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		EndPurchaseTransaction();
		m_visibilityVisualController.SetState("VISIBLE");
		UpdateView();
	}

	private void StartPurchaseTransaction()
	{
		if (!m_currentCardBackIdx.HasValue)
		{
			Debug.LogError("CardBackInfoManager:StartPurchaseTransaction: m_currentCardBackIdx did not have a value");
		}
		else if (m_isStoreTransactionActive)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_CARD_BACK_PURCHASE_ERROR_HEADER");
			info.m_text = GameStrings.Get("GLUE_CHECKOUT_ERROR_GENERIC_FAILURE");
			info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
			Debug.LogWarning("CardBackInfoManager:StartPurchaseTransaction: Attempted to start a card back transaction while an existing transaction was in progress");
		}
		else if (this.OnProductPurchaseAttempt == null)
		{
			Debug.LogError("CardBackInfoManager:StartPurchaseTransaction: Attempted to start a card back purchase transaction while OnProductPurchaseAttempt was null");
		}
		else
		{
			m_isStoreTransactionActive = true;
			ProductInfo bundle = CardBackManager.Get().GetCollectionManagerCardBackProductBundle(m_currentCardBackIdx.Value);
			if (bundle == null)
			{
				Debug.LogError("CardBackInfoManager:StartPurchaseTransaction: Attempted to start a card back purchase transaction with a null product bundle for card back " + m_currentCardBackIdx.Value);
			}
			else
			{
				this.OnProductPurchaseAttempt(new BuyPmtProductEventArgs(bundle, CurrencyType.GOLD, 1));
			}
		}
	}

	private void CancelPurchaseTransaction()
	{
		EndPurchaseTransaction();
	}

	private void EndPurchaseTransaction()
	{
		if (m_isStoreTransactionActive)
		{
			m_isStoreTransactionActive = false;
		}
	}

	void IStore.Open()
	{
		Shop.Get().RefreshDataModel();
		m_isStoreOpen = true;
		this.OnOpened?.Invoke();
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.RefreshCurrency();
		}
		else
		{
			Debug.LogError("CardBackInfoManager:IStore.Open: Could not get the Bnet bar to reflect the required currency");
		}
	}

	void IStore.Close()
	{
		if (m_isStoreTransactionActive)
		{
			CancelPurchaseTransaction();
		}
	}

	void IStore.BlockInterface(bool blocked)
	{
		BlockInputs(blocked);
	}

	bool IStore.IsReady()
	{
		return true;
	}

	bool IStore.IsOpen()
	{
		return m_isStoreOpen;
	}

	void IStore.Unload()
	{
	}

	IEnumerable<CurrencyType> IStore.GetVisibleCurrencies()
	{
		return new CurrencyType[1] { CurrencyType.GOLD };
	}
}
