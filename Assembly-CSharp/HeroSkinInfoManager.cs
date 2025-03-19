using Blizzard.T5.AssetManager;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class HeroSkinInfoManager : BaseHeroSkinInfoManager
{
	private static HeroSkinInfoManager s_instance;

	private static bool s_isReadyingInstance;

	public static HeroSkinInfoManager Get()
	{
		return s_instance;
	}

	public static void EnterPreviewWhenReady(CollectionCardVisual cardVisual)
	{
		HeroSkinInfoManager infoManager = Get();
		if (infoManager != null)
		{
			infoManager.EnterPreview(cardVisual);
			return;
		}
		if (s_isReadyingInstance)
		{
			Debug.LogWarning("HeroSkinInfoManager:EnterPreviewWhenReady called while the info manager instance was being readied");
			return;
		}
		string heroSkinInfoManagerPrefab = "HeroSkinInfoManager.prefab:9d5b641eb672c491f8cbd2f20d2cbb61";
		Widget widget = WidgetInstance.Create(heroSkinInfoManagerPrefab);
		if (widget == null)
		{
			Debug.LogError("HeroSkinInfoManager:EnterPreviewWhenReady failed to create widget instance");
			return;
		}
		s_isReadyingInstance = true;
		widget.RegisterReadyListener(delegate
		{
			s_instance = widget.GetComponentInChildren<HeroSkinInfoManager>();
			s_isReadyingInstance = false;
			if (s_instance == null)
			{
				Debug.LogError("HeroSkinInfoManager:EnterPreviewWhenReady created widget instance but failed to get HeroSkinInfoManager component");
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
		return s_instance.IsShowingPreview;
	}

	private void OnDestroy()
	{
		m_currentHeroCardDef?.Dispose();
		m_currentHeroCardDef = null;
		AssetHandle.SafeDispose(ref m_currentHeroGoldenAnimation);
		CancelPreview();
		s_instance = null;
	}

	protected override void PushNavigateBack()
	{
		Navigation.PushUnique(OnNavigateBack);
	}

	protected override void RemoveNavigateBack()
	{
		Navigation.RemoveHandler(OnNavigateBack);
	}

	private static bool OnNavigateBack()
	{
		HeroSkinInfoManager hsim = Get();
		if (hsim != null)
		{
			hsim.CancelPreview();
		}
		return true;
	}

	protected override void ToggleFavoriteSkin()
	{
		string cardId = m_currentEntityDef.GetCardId();
		TAG_CLASS heroClass = m_currentEntityDef.GetClass();
		NetCache.CardDefinition favoriteHero = CollectionManager.Get().GetFavoriteHero(cardId);
		bool isFavoriting = favoriteHero == null;
		if (isFavoriting)
		{
			favoriteHero = new NetCache.CardDefinition
			{
				Name = cardId,
				Premium = m_currentPremium
			};
		}
		Network.Get().SetFavoriteHero(heroClass, favoriteHero, isFavoriting);
		if (!Network.IsLoggedIn())
		{
			CollectionManager.Get().UpdateFavoriteHero(heroClass, cardId, m_currentPremium, isFavoriting);
		}
	}

	protected override bool CanToggleFavorite()
	{
		return HeroSkinUtils.CanToggleFavoriteHeroSkin(m_currentEntityDef.GetClass(), m_currentEntityDef.GetCardId());
	}

	protected override void SetupHeroSkinStore()
	{
		if (m_isStoreOpen)
		{
			Debug.LogError("CardBackInfoManager:SetupHeroSkinStore called when the store was already open");
			return;
		}
		if (m_currentHeroRecord == null)
		{
			Debug.LogError("CardBackInfoManager:SetupHeroSkinStore: m_currentHeroRecord was null");
			return;
		}
		StoreManager storeManager = StoreManager.Get();
		if (storeManager.IsOpen())
		{
			storeManager.SetupHeroSkinStore(this, m_currentHeroRecord.CardId);
			storeManager.RegisterSuccessfulPurchaseListener(base.OnSuccessfulPurchase);
			storeManager.RegisterSuccessfulPurchaseAckListener(base.OnSuccessfulPurchaseAck);
			storeManager.RegisterFailedPurchaseAckListener(base.OnFailedPurchaseAck);
			BnetBar.Get()?.RefreshCurrency();
		}
	}
}
