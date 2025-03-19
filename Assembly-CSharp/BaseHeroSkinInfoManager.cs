using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public abstract class BaseHeroSkinInfoManager : MonoBehaviour, IStore
{
	protected const string STATE_SHOW_VANILLA_HERO = "SHOW_VANILLA_HERO";

	protected const string STATE_SHOW_NEW_HERO = "SHOW_NEW_HERO";

	protected const string STATE_SHOW_CUSTOM_HERO = "SHOW_CUSTOM_HERO";

	protected const string STATE_INVALID_HERO = "INVALID_HERO";

	protected const string STATE_HIDDEN = "HIDDEN";

	protected const string MAKE_FAVORITE_STATE = "MAKE_FAVORITE";

	protected const string SUFFICIENT_CURRENCY_STATE = "SUFFICIENT_CURRENCY";

	protected const string INSUFFICIENT_CURRENCY_STATE = "INSUFFICIENT_CURRENCY";

	protected const string DISABLED_STATE = "DISABLED";

	protected const string STATE_BLOCK_SCREEN = "BLOCK_SCREEN";

	protected const string STATE_UNBLOCK_SCREEN = "UNBLOCK_SCREEN";

	public GameObject m_previewPane;

	public GameObject m_vanillaHeroFrame;

	public MeshRenderer m_vanillaHeroPreviewQuad;

	public UberText m_vanillaHeroTitle;

	public UberText m_vanillaHeroDescription;

	public UIBButton m_vanillaHeroFavoriteButton;

	public UIBButton m_vanillaHeroBuyButton;

	public GameObject m_newHeroFrame;

	public MeshRenderer m_newHeroPreviewQuad;

	public UberText m_newHeroTitle;

	public UberText m_newHeroDescription;

	public UIBButton m_newHeroFavoriteButton;

	public UIBButton m_newHeroBuyButton;

	public GameObject m_customHeroFrameRoot;

	protected GameObject m_customHeroFrameInstance;

	public PegUIElement m_offClicker;

	public float m_animationTime = 0.5f;

	public Material m_defaultPreviewMaterial;

	public Material m_vanillaHeroNonPremiumMaterial;

	public AsyncReference m_visibilityVisualControllerReference;

	public AsyncReference m_userActionVisualControllerReference;

	public AsyncReference m_vanillaHeroCurrencyIconWidgetReference;

	public AsyncReference m_newHeroCurrencyIconWidgetReference;

	public AsyncReference m_fullScreenBlockerWidgetReference;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_enterPreviewSound;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_exitPreviewSound;

	public MusicPlaylistType m_defaultHeroMusic = MusicPlaylistType.UI_CMHeroSkinPreview;

	protected WidgetTemplate m_widget;

	protected string m_currentCardId;

	protected DefLoader.DisposableCardDef m_currentHeroCardDef;

	protected CollectionHeroDef m_currentHeroDef;

	protected AssetHandle<UberShaderAnimation> m_currentHeroGoldenAnimation;

	protected CardHeroDbfRecord m_currentHeroRecord;

	protected EntityDef m_currentEntityDef;

	protected TAG_PREMIUM m_currentPremium;

	protected bool m_animating;

	protected bool m_hasEnteredHeroSkinPreview;

	protected MusicPlaylistType m_prevPlaylist;

	protected string m_desiredVisibilityState = "INVALID_HERO";

	protected VisualController m_visibilityVisualController;

	protected VisualController m_userActionVisualController;

	protected Widget m_fullScreenBlockerWidget;

	protected Widget m_vanillaHeroCurrencyButtonWidget;

	protected Widget m_newHeroCurrencyButtonWidget;

	protected bool m_isStoreOpen;

	protected bool m_isStoreTransactionActive;

	private readonly HashSet<CurrencyType> m_activeHeroCardCurrencyTypes = new HashSet<CurrencyType> { CurrencyType.GOLD };

	private ScreenEffectsHandle m_screenEffectsHandle;

	private CardSoundSpell m_previewEmoteCardSoundSpell;

	private const string BATTLEGROUNDS_DEFAULT_COLLECTION_HERO_DEF_PATH = "CollectionHeroDef_08.prefab:21f2d3e73f8bd8d43bd2a7a2d8d97adc";

	public bool IsShowingPreview
	{
		get
		{
			if (!m_animating)
			{
				return m_hasEnteredHeroSkinPreview;
			}
			return true;
		}
	}

	public event Action OnOpened;

	public event Action<StoreClosedArgs> OnClosed;

	public event Action OnReady;

	public event Action<BuyProductEventArgs> OnProductPurchaseAttempt;

	public event Action OnProductOpened;

	protected virtual void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_previewPane.SetActive(value: false);
		SetupUI();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Start()
	{
		m_visibilityVisualControllerReference.RegisterReadyListener<VisualController>(OnVisibilityVisualControllerReady);
		m_userActionVisualControllerReference.RegisterReadyListener<VisualController>(OnUserActionVisualControllerReady);
		m_fullScreenBlockerWidgetReference.RegisterReadyListener<Widget>(OnFullScreenBlockerWidgetReady);
		m_vanillaHeroCurrencyIconWidgetReference.RegisterReadyListener<Widget>(OnVanillaHeroCurrencyButtonWidgetReady);
		m_newHeroCurrencyIconWidgetReference.RegisterReadyListener<Widget>(OnNewHeroCurrencyButtonWidgetReady);
	}

	public virtual void EnterPreview(CollectionCardVisual cardVisual)
	{
		if (m_animating)
		{
			return;
		}
		m_activeHeroCardCurrencyTypes.Clear();
		BnetBar.Get()?.RefreshCurrency();
		this.OnProductOpened?.Invoke();
		Actor cardVisualActor = cardVisual.GetActor();
		if (cardVisualActor == null)
		{
			Log.CollectionManager.PrintError("BaseHeroSkinInfoManager.EnterPreview - Could not get actor from card visual. Not displaying preview");
			return;
		}
		m_currentEntityDef = cardVisualActor.GetEntityDef();
		m_currentPremium = cardVisualActor.GetPremium();
		if (m_currentEntityDef == null)
		{
			Log.CollectionManager.PrintError("BaseHeroSkinInfoManager.EnterPreview - Actor entity def not set. Not displaying preview");
			return;
		}
		CollectionManagerDisplay cmd = CollectionManager.Get()?.GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideHeroTips();
		}
		CardDataModel cardDataModel = new CardDataModel();
		cardDataModel.CardId = m_currentCardId;
		cardDataModel.ArtistCredit = (string.IsNullOrWhiteSpace(m_currentEntityDef.GetArtistName(m_currentPremium)) ? string.Empty : GameStrings.Format("GLUE_COLLECTION_ARTIST", m_currentEntityDef.GetArtistName(m_currentPremium)));
		cardDataModel.Rarity = cardVisualActor.GetRarity().ToString();
		CardDataModel cardDataModel2 = cardDataModel;
		m_widget.BindDataModel(cardDataModel2);
		PushNavigateBack();
		string cardId = m_currentEntityDef.GetCardId();
		m_currentHeroRecord = GameUtils.GetCardHeroRecordForCardId(GameUtils.TranslateCardIdToDbId(cardId));
		bool num = CollectionManager.GetHeroCardId(m_currentEntityDef.GetClass(), CardHero.HeroType.HONORED) == cardId;
		bool isVanillaHero = GameUtils.IsVanillaHero(cardId);
		bool isBattlegroundsHero = m_currentHeroRecord.HeroType == CardHero.HeroType.BATTLEGROUNDS_HERO || m_currentHeroRecord.HeroType == CardHero.HeroType.BATTLEGROUNDS_GUIDE;
		bool isEligibleForUpgradedVisual = (cardVisualActor.PremiumAnimationAvailable && isVanillaHero && m_currentPremium == TAG_PREMIUM.GOLDEN) || !isVanillaHero;
		bool useVanillaPortrait = num || isVanillaHero || isBattlegroundsHero;
		if (LoadHeroDef(cardId))
		{
			if (m_currentHeroDef.m_collectionManagerPreviewEmote != 0)
			{
				GameUtils.LoadCardDefEmoteSound(cardVisual.GetActor().EmoteDefs, m_currentHeroDef.m_collectionManagerPreviewEmote, delegate(CardSoundSpell cardSpell)
				{
					m_previewEmoteCardSoundSpell = cardSpell;
					if (cardSpell != null)
					{
						cardSpell.AddFinishedCallback(delegate
						{
							UnityEngine.Object.Destroy(cardSpell.gameObject);
						});
						cardSpell.Reactivate();
					}
				});
			}
			useVanillaPortrait = useVanillaPortrait || !m_currentHeroDef.m_previewMaterial.IsInitialized();
		}
		else
		{
			if (!isBattlegroundsHero)
			{
				Debug.LogError("BaseHeroSkinInfoManager::EnterPreview Could not load entity def for hero skin " + cardId + ". Check the CollectionHeroDefPath on the CardDef under the Hero section.", this);
				m_desiredVisibilityState = "INVALID_HERO";
				SetupHeroSkinStore();
				UpdateView();
				return;
			}
			m_currentHeroCardDef?.Dispose();
			m_currentHeroCardDef = DefLoader.Get().GetCardDef(cardId);
			m_currentHeroDef = GameUtils.LoadGameObjectWithComponent<CollectionHeroDef>("CollectionHeroDef_08.prefab:21f2d3e73f8bd8d43bd2a7a2d8d97adc");
		}
		ClearCustomFrame();
		if (m_customHeroFrameRoot != null && !string.IsNullOrEmpty(m_currentHeroCardDef.CardDef.m_CustomHeroInfoFramePrefab))
		{
			m_customHeroFrameInstance = AssetLoader.Get().InstantiatePrefab(m_currentHeroCardDef.CardDef.m_CustomHeroInfoFramePrefab);
		}
		if (m_customHeroFrameInstance != null)
		{
			m_customHeroFrameInstance.transform.SetParent(m_customHeroFrameRoot.transform);
			TransformUtil.Identity(m_customHeroFrameInstance);
			CustomFrameButtonReskinController reskinController = m_customHeroFrameRoot.GetComponent<CustomFrameButtonReskinController>();
			if (reskinController != null)
			{
				CustomFrameButtonReskinData reskinData = m_customHeroFrameInstance.GetComponent<CustomFrameButtonReskinData>();
				reskinController.UpdateMaterials(reskinData);
			}
			m_vanillaHeroTitle.Text = m_currentEntityDef.GetName();
			m_vanillaHeroDescription.Text = m_currentHeroRecord.Description;
			m_desiredVisibilityState = "SHOW_CUSTOM_HERO";
		}
		else
		{
			if ((isEligibleForUpgradedVisual ? cardVisual.GetActor().PremiumPortraitMaterial : null) == null)
			{
				m_currentHeroDef.m_previewMaterial.GetMaterial();
				useVanillaPortrait = true;
			}
			Material heroMaterial = null;
			if (!useVanillaPortrait)
			{
				heroMaterial = m_currentHeroDef.m_previewMaterial.GetMaterial();
			}
			Texture portraitTexture = ((cardVisualActor.LegendaryHeroPortrait == null) ? cardVisualActor.GetPortraitTexture() : cardVisualActor.GetStaticPortraitTexture());
			useVanillaPortrait = useVanillaPortrait || heroMaterial == null;
			if (!useVanillaPortrait)
			{
				string uberAnimationAssetPath = m_currentHeroDef.GetHeroUberShaderAnimationPath();
				bool num2 = !string.IsNullOrWhiteSpace(uberAnimationAssetPath);
				if (num2)
				{
					AssetHandle.SafeDispose(ref m_currentHeroGoldenAnimation);
					AssetLoader.Get().LoadAsset(ref m_currentHeroGoldenAnimation, uberAnimationAssetPath);
				}
				if (num2 && m_currentHeroGoldenAnimation == null)
				{
					Debug.LogWarning($"BaseHeroSkinInfoManager.EnterPreview - {cardId} hero shader could not be loaded {uberAnimationAssetPath}");
				}
			}
			if (useVanillaPortrait)
			{
				m_vanillaHeroTitle.Text = m_currentEntityDef.GetName();
				m_vanillaHeroDescription.Text = m_currentHeroRecord.Description;
				heroMaterial = m_vanillaHeroNonPremiumMaterial;
				if (isEligibleForUpgradedVisual)
				{
					Material premiumPortraitMaterial = cardVisual.GetActor().PremiumPortraitMaterial;
					if (premiumPortraitMaterial != null)
					{
						heroMaterial = premiumPortraitMaterial;
						portraitTexture = heroMaterial.mainTexture;
					}
					else
					{
						Log.CollectionManager.PrintWarning($"BaseHeroSkinInfoManager.EnterPreview - premium material missing for {cardId}");
					}
				}
				AssignVanillaHeroPreviewMaterial(heroMaterial, portraitTexture, cardVisual.GetActor().PortraitAnimation, cardVisual.GetActor().m_portraitMatIdx);
			}
			else
			{
				m_newHeroTitle.Text = m_currentEntityDef.GetName();
				m_newHeroDescription.Text = m_currentHeroRecord.Description;
				AssignNewHeroPreviewMaterial(heroMaterial, portraitTexture, m_currentHeroGoldenAnimation);
			}
			m_desiredVisibilityState = (useVanillaPortrait ? "SHOW_VANILLA_HERO" : "SHOW_NEW_HERO");
		}
		m_hasEnteredHeroSkinPreview = true;
		m_previewPane.SetActive(value: true);
		m_offClicker.gameObject.SetActive(value: true);
		m_animating = true;
		iTween.ScaleFrom(m_previewPane, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
		{
			m_animating = false;
		}));
		SetupHeroSkinStore();
		UpdateView();
		if (!string.IsNullOrEmpty(m_enterPreviewSound))
		{
			SoundManager.Get().LoadAndPlay(m_enterPreviewSound);
		}
		PlayHeroMusic();
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = m_animationTime;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	public virtual void CancelPreview()
	{
		RemoveNavigateBack();
		if (!m_animating && m_hasEnteredHeroSkinPreview)
		{
			m_hasEnteredHeroSkinPreview = false;
			ShutDownHeroSkinStore();
			Vector3 origScale = m_previewPane.transform.localScale;
			if (m_previewEmoteCardSoundSpell != null)
			{
				AudioSource cardSoundSpellAudioSource = m_previewEmoteCardSoundSpell.GetActiveAudioSource();
				SoundManager.Get().FadeTrackOut(cardSoundSpellAudioSource, m_animationTime);
			}
			m_animating = true;
			iTween.ScaleTo(m_previewPane, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", m_animationTime, "easetype", iTween.EaseType.easeOutCirc, "oncomplete", (Action<object>)delegate
			{
				OnFinishedClosing(origScale);
			}));
			if (!string.IsNullOrEmpty(m_exitPreviewSound))
			{
				SoundManager.Get()?.LoadAndPlay(m_exitPreviewSound);
			}
			StopHeroMusic();
			m_screenEffectsHandle.StopEffect();
		}
	}

	protected virtual void OnFinishedClosing(Vector3 originalScale)
	{
		m_animating = false;
		if ((bool)m_previewPane)
		{
			m_previewPane.transform.localScale = originalScale;
			m_previewPane.SetActive(value: false);
		}
		if ((bool)m_offClicker)
		{
			m_offClicker.gameObject.SetActive(value: false);
		}
		ClearCustomFrame();
		m_previewEmoteCardSoundSpell = null;
	}

	protected void OnVisibilityVisualControllerReady(VisualController visualController)
	{
		m_visibilityVisualController = visualController;
		UpdateView();
		if (this.OnReady != null)
		{
			this.OnReady();
		}
	}

	protected void OnUserActionVisualControllerReady(VisualController visualController)
	{
		m_userActionVisualController = visualController;
		UpdateView();
	}

	protected void OnFullScreenBlockerWidgetReady(Widget fullScreenBlockerWidget)
	{
		m_fullScreenBlockerWidget = fullScreenBlockerWidget;
		UpdateView();
	}

	protected void OnVanillaHeroCurrencyButtonWidgetReady(Widget currencyButtonWidget)
	{
		m_vanillaHeroCurrencyButtonWidget = currencyButtonWidget;
		UpdateView();
	}

	protected void OnNewHeroCurrencyButtonWidgetReady(Widget currencyButtonWidget)
	{
		m_newHeroCurrencyButtonWidget = currencyButtonWidget;
		UpdateView();
	}

	public void OnFavoriteHeroChanged(TAG_CLASS heroClass, NetCache.CardDefinition favoriteHero, bool isFavorite, object userData)
	{
		UpdateFavoriteButton();
	}

	protected abstract bool CanToggleFavorite();

	protected void UpdateFavoriteButton()
	{
		bool shouldShowButton = CanToggleFavorite();
		UIBButton favoriteButton = ((m_desiredVisibilityState == "SHOW_VANILLA_HERO") ? m_vanillaHeroFavoriteButton : m_newHeroFavoriteButton);
		if (favoriteButton.IsEnabled() != shouldShowButton)
		{
			favoriteButton.SetEnabled(shouldShowButton);
			favoriteButton.Flip(shouldShowButton);
		}
	}

	protected void UpdateView()
	{
		if (m_visibilityVisualController == null || m_userActionVisualController == null || m_fullScreenBlockerWidget == null || m_vanillaHeroCurrencyButtonWidget == null || m_newHeroCurrencyButtonWidget == null || m_currentEntityDef == null || m_currentHeroRecord == null || string.IsNullOrEmpty(m_currentCardId))
		{
			return;
		}
		m_activeHeroCardCurrencyTypes.Clear();
		m_activeHeroCardCurrencyTypes.Add(CurrencyType.GOLD);
		m_visibilityVisualController.SetState(m_desiredVisibilityState);
		bool canBuyHeroSkin = false;
		if (HeroSkinUtils.IsHeroSkinOwned(m_currentEntityDef.GetCardId()))
		{
			m_userActionVisualController.SetState("MAKE_FAVORITE");
		}
		else if (HeroSkinUtils.IsHeroSkinPurchasableFromCollectionManager(m_currentEntityDef.GetCardId()))
		{
			PriceDataModel priceDataModel = HeroSkinUtils.GetCollectionManagerHeroSkinPriceDataModel(m_currentEntityDef.GetCardId());
			if (priceDataModel != null)
			{
				if (priceDataModel.Currency != 0 && priceDataModel.Currency != CurrencyType.REAL_MONEY)
				{
					m_activeHeroCardCurrencyTypes.Add(priceDataModel.Currency);
				}
				m_userActionVisualController.BindDataModel(priceDataModel);
				if (!HeroSkinUtils.CanBuyHeroSkinFromCollectionManager(m_currentEntityDef.GetCardId(), priceDataModel.Currency, priceDataModel))
				{
					m_userActionVisualController.SetState("INSUFFICIENT_CURRENCY");
				}
				else
				{
					m_userActionVisualController.SetState("SUFFICIENT_CURRENCY");
					canBuyHeroSkin = true;
				}
			}
			else
			{
				m_userActionVisualController.SetState("DISABLED");
			}
		}
		else
		{
			m_userActionVisualController.SetState("DISABLED");
		}
		BnetBar.Get()?.RefreshCurrency();
		UpdateFavoriteButton();
		UIBButton obj = ((m_desiredVisibilityState == "SHOW_VANILLA_HERO") ? m_vanillaHeroBuyButton : m_newHeroBuyButton);
		obj.SetEnabled(canBuyHeroSkin);
		obj.Flip(faceUp: true);
	}

	protected abstract void PushNavigateBack();

	protected abstract void RemoveNavigateBack();

	protected bool LoadHeroDef(string cardId)
	{
		if (m_currentCardId == cardId && string.IsNullOrEmpty(cardId))
		{
			return true;
		}
		m_currentHeroCardDef?.Dispose();
		m_currentHeroCardDef = DefLoader.Get().GetCardDef(cardId);
		if (m_currentHeroCardDef?.CardDef == null || string.IsNullOrEmpty(m_currentHeroCardDef.CardDef.m_CollectionHeroDefPath))
		{
			return false;
		}
		CollectionHeroDef heroDef = GameUtils.LoadGameObjectWithComponent<CollectionHeroDef>(m_currentHeroCardDef.CardDef.m_CollectionHeroDefPath);
		if (heroDef == null)
		{
			Debug.LogWarning($"Hero def does not exist on object: {m_currentHeroCardDef.CardDef.m_CollectionHeroDefPath}");
			return false;
		}
		if (m_currentHeroDef != null)
		{
			UnityEngine.Object.Destroy(m_currentHeroDef.gameObject);
		}
		m_currentCardId = cardId;
		m_currentHeroDef = heroDef;
		return true;
	}

	protected void SetupUI()
	{
		m_newHeroFavoriteButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ToggleFavoriteSkin();
			CancelPreview();
		});
		if (m_vanillaHeroFavoriteButton != null && m_vanillaHeroFavoriteButton != m_newHeroFavoriteButton)
		{
			m_vanillaHeroFavoriteButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ToggleFavoriteSkin();
				CancelPreview();
			});
		}
		m_newHeroBuyButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnBuyButtonReleased();
		});
		if (m_vanillaHeroBuyButton != null && m_vanillaHeroBuyButton != m_newHeroBuyButton)
		{
			m_vanillaHeroBuyButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnBuyButtonReleased();
			});
		}
		m_offClicker.AddEventListener(UIEventType.RELEASE, delegate
		{
			CancelPreview();
		});
		m_offClicker.AddEventListener(UIEventType.RIGHTCLICK, delegate
		{
			CancelPreview();
		});
		CollectionManager.Get().RegisterFavoriteHeroChangedListener(OnFavoriteHeroChanged);
	}

	private void OnBuyButtonReleased()
	{
		if (!IsHeroSkinCardIdValid())
		{
			Debug.LogError("BaseHeroSkinInfoManager:OnBuyButtonReleased called when the hero skin card id was invalid");
			return;
		}
		m_visibilityVisualController.SetState("HIDDEN");
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Format("GLUE_HERO_SKIN_PURCHASE_CONFIRMATION_HEADER");
		info.m_text = GameStrings.Format("GLUE_HERO_SKIN_PURCHASE_CONFIRMATION_MESSAGE", m_currentEntityDef.GetName());
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
				UpdateView();
			}
		};
		info.m_responseCallback = callback;
		DialogManager.Get().ShowPopup(info);
	}

	protected abstract void ToggleFavoriteSkin();

	private void AssignVanillaHeroPreviewMaterial(Material material, Texture portraitTexture, UberShaderAnimation portraitAnimation, int portraitMaterialIndex)
	{
		Renderer vanillaHeroPreviewQuadRenderer = m_vanillaHeroPreviewQuad.GetComponent<Renderer>();
		if (portraitTexture != null)
		{
			vanillaHeroPreviewQuadRenderer.SetMaterial(portraitMaterialIndex, material);
			vanillaHeroPreviewQuadRenderer.GetMaterial(portraitMaterialIndex).SetTexture("_MainTex", portraitTexture);
		}
		else
		{
			vanillaHeroPreviewQuadRenderer.SetMaterial(portraitMaterialIndex, material);
		}
		AssignVanillaHeroUberAnimation(portraitAnimation, portraitMaterialIndex);
	}

	private void AssignNewHeroPreviewMaterial(Material material, Texture portraitTexture, UberShaderAnimation portraitAnimation)
	{
		Renderer heroPreviewQuadRenderer = m_newHeroPreviewQuad.GetComponent<Renderer>();
		if (material != null)
		{
			heroPreviewQuadRenderer.SetMaterial(material);
		}
		else
		{
			heroPreviewQuadRenderer.SetMaterial(m_defaultPreviewMaterial);
			heroPreviewQuadRenderer.GetMaterial().mainTexture = portraitTexture;
		}
		AssignNewHeroUberAnimation(portraitAnimation);
	}

	private void AssignVanillaHeroUberAnimation(UberShaderAnimation portraitAnimation, int portraitMaterialIndex)
	{
		UberShaderController uberShaderController = m_vanillaHeroPreviewQuad.GetComponent<UberShaderController>();
		if (portraitAnimation != null)
		{
			if (uberShaderController == null)
			{
				uberShaderController = m_vanillaHeroPreviewQuad.gameObject.AddComponent<UberShaderController>();
			}
			uberShaderController.UberShaderAnimation = UnityEngine.Object.Instantiate(portraitAnimation);
			uberShaderController.m_MaterialIndex = portraitMaterialIndex;
			uberShaderController.enabled = true;
		}
		else if (uberShaderController != null)
		{
			UnityEngine.Object.Destroy(uberShaderController);
		}
	}

	private void AssignNewHeroUberAnimation(UberShaderAnimation portraitAnimation)
	{
		UberShaderController uberShaderController = m_newHeroPreviewQuad.GetComponent<UberShaderController>();
		if (portraitAnimation != null)
		{
			if (uberShaderController == null)
			{
				uberShaderController = m_newHeroPreviewQuad.gameObject.AddComponent<UberShaderController>();
			}
			uberShaderController.UberShaderAnimation = UnityEngine.Object.Instantiate(portraitAnimation);
			uberShaderController.m_MaterialIndex = 0;
			uberShaderController.enabled = true;
		}
		else if (uberShaderController != null)
		{
			UnityEngine.Object.Destroy(uberShaderController);
		}
	}

	private void PlayHeroMusic()
	{
		MusicPlaylistType heroMusic = MusicPlaylistType.Invalid;
		if (m_currentHeroDef != null && m_currentHeroDef.m_heroPlaylist != 0)
		{
			heroMusic = m_currentHeroDef.m_heroPlaylist;
		}
		if (heroMusic != 0)
		{
			m_prevPlaylist = MusicManager.Get().GetCurrentPlaylist();
			MusicManager.Get().StartPlaylist(heroMusic);
		}
	}

	private void StopHeroMusic()
	{
		if (MusicManager.Get() != null && m_prevPlaylist != 0)
		{
			MusicManager.Get().StartPlaylist(m_prevPlaylist);
		}
	}

	private void BlockInputs(bool blocked)
	{
		if (m_fullScreenBlockerWidget == null)
		{
			Debug.LogError("Failed to toggle interface blocker from Duels Popup Manager");
		}
		else
		{
			m_fullScreenBlockerWidget.TriggerEvent(blocked ? "BLOCK_SCREEN" : "UNBLOCK_SCREEN", TriggerEventParameters.StandardPropagateDownward);
		}
	}

	private bool IsHeroSkinCardIdValid()
	{
		if (m_currentEntityDef != null)
		{
			return !string.IsNullOrEmpty(m_currentEntityDef.GetCardId());
		}
		return false;
	}

	private void ClearCustomFrame()
	{
		if (m_customHeroFrameInstance != null)
		{
			UnityEngine.Object.Destroy(m_customHeroFrameInstance);
			m_customHeroFrameInstance = null;
		}
		if (m_customHeroFrameRoot != null)
		{
			CustomFrameButtonReskinController reskinController = m_customHeroFrameRoot.GetComponent<CustomFrameButtonReskinController>();
			if (reskinController != null)
			{
				reskinController.UpdateMaterials(null);
			}
		}
	}

	protected virtual void SetupHeroSkinStore()
	{
	}

	protected void ShutDownHeroSkinStore()
	{
		if (m_isStoreOpen)
		{
			CancelPurchaseTransaction();
			this.OnClosed?.Invoke(new StoreClosedArgs());
			StoreManager storeManager = StoreManager.Get();
			storeManager.RemoveFailedPurchaseAckListener(OnFailedPurchaseAck);
			storeManager.RemoveSuccessfulPurchaseListener(OnSuccessfulPurchase);
			storeManager.RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
			storeManager.ShutDownHeroSkinStore();
			this.OnProductPurchaseAttempt = null;
			m_activeHeroCardCurrencyTypes.Clear();
			BnetBar.Get()?.RefreshCurrency();
			BlockInputs(blocked: false);
			m_isStoreOpen = false;
		}
	}

	protected void OnSuccessfulPurchase(ProductInfo bundle, PaymentMethod paymentMethod)
	{
	}

	protected void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		EndPurchaseTransaction();
		UpdateView();
	}

	protected void OnFailedPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		EndPurchaseTransaction();
		UpdateView();
	}

	protected void StartPurchaseTransaction()
	{
		if (!IsHeroSkinCardIdValid())
		{
			Debug.LogError("BaseHeroSkinInfoManager:StartPurchaseTransaction called when the hero skin card id was invalid");
			return;
		}
		if (m_isStoreTransactionActive)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_HERO_SKIN_PURCHASE_ERROR_HEADER");
			info.m_text = GameStrings.Get("GLUE_CHECKOUT_ERROR_GENERIC_FAILURE");
			info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
			Debug.LogWarning("Attempted to start a hero skin purchase transaction while an existing transaction was in progress (CardId = " + m_currentEntityDef.GetCardId() + ")");
			return;
		}
		if (this.OnProductPurchaseAttempt == null)
		{
			Debug.LogError("Attempted to start a hero skin purchase transaction while OnProductPurchaseAttempt was null (CardId = " + m_currentEntityDef.GetCardId() + ")");
			return;
		}
		ProductInfo bundle = HeroSkinUtils.GetCollectionManagerHeroSkinProductBundle(m_currentEntityDef.GetCardId());
		if (bundle == null)
		{
			Debug.LogError("Attempted to start a hero skin purchase transaction with a null bundle (CardId = " + m_currentEntityDef.GetCardId() + ")");
			return;
		}
		PriceDataModel priceDataModel = HeroSkinUtils.GetCollectionManagerHeroSkinPriceDataModel(m_currentEntityDef.GetCardId());
		if (priceDataModel.Currency == CurrencyType.NONE || priceDataModel.Amount == 0f)
		{
			Debug.LogError("Attempted to start a hero skin purchase transaction with " + $"Currency: {priceDataModel.Currency} Amount: {priceDataModel.Amount} for card {m_currentEntityDef.GetCardId()}");
			return;
		}
		m_isStoreTransactionActive = true;
		this.OnProductPurchaseAttempt(new BuyPmtProductEventArgs(bundle, priceDataModel.Currency, 1));
	}

	protected void CancelPurchaseTransaction()
	{
		EndPurchaseTransaction();
	}

	protected void EndPurchaseTransaction()
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
			Debug.LogError("BaseHeroSkinInfoManager:IStore.Open: Could not get the Bnet bar to reflect the required currency");
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
		m_activeHeroCardCurrencyTypes.Add(CurrencyType.GOLD);
		return m_activeHeroCardCurrencyTypes;
	}
}
