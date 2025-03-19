using System;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using UnityEngine;

public static class CardTextureLoader
{
	private static bool ItNotDownloading
	{
		get
		{
			if (ServiceManager.TryGet<GameDownloadManager>(out var gameDownloadManager))
			{
				return gameDownloadManager.AssetDownloaderState != AssetDownloaderState.DOWNLOADING;
			}
			return false;
		}
	}

	public static bool Load(CardDef cardDef, CardPortraitQuality quality, bool prohibitRecursion = false)
	{
		if (cardDef == null)
		{
			return false;
		}
		bool loadedPortrait = false;
		bool loadedPremiumAnimation = false;
		cardDef.UpdateSpecialEvent();
		CardPortraitQuality currentQuality = cardDef.GetPortraitQuality();
		bool portraitUpgrade = currentQuality.TextureQuality < quality.TextureQuality;
		bool highResPossible = !PlatformSettings.ShouldFallbackToLowRes;
		bool needSignature = quality.PremiumType == TAG_PREMIUM.SIGNATURE && currentQuality.PremiumType != TAG_PREMIUM.SIGNATURE && SignatureMaterialExists(cardDef);
		bool num = (quality.PremiumType == TAG_PREMIUM.GOLDEN || quality.PremiumType == TAG_PREMIUM.DIAMOND || cardDef.m_AlwaysRenderPremiumPortrait) && currentQuality.PremiumType != TAG_PREMIUM.GOLDEN && PremiumAnimationExists(cardDef);
		bool num2 = quality.TextureQuality == 3 && portraitUpgrade && highResPossible;
		bool needAnyPortrait = currentQuality.TextureQuality == 0;
		if (num2)
		{
			if (HighQualityAvailable(cardDef))
			{
				LoadHighQuality(cardDef);
				loadedPortrait = true;
			}
			else if (!prohibitRecursion)
			{
				LoadDeferred(cardDef, HighQualityAvailable, quality).Forget();
				prohibitRecursion = true;
			}
		}
		if (needSignature)
		{
			LoadSignature(cardDef);
		}
		if (num)
		{
			if (PremiumAnimationAvailable(cardDef))
			{
				LoadGolden(cardDef);
				loadedPremiumAnimation = true;
			}
			else if (!prohibitRecursion)
			{
				LoadDeferred(cardDef, PremiumAnimationAvailable, quality).Forget();
				prohibitRecursion = true;
			}
		}
		if (needAnyPortrait && !loadedPortrait)
		{
			LoadLowQuality(cardDef);
			loadedPortrait = true;
		}
		return loadedPortrait || loadedPremiumAnimation;
	}

	private static bool HighQualityAvailable(CardDef cardDef)
	{
		if (!PlatformSettings.ShouldFallbackToLowRes)
		{
			return AssetLoader.Get().IsAssetAvailable(cardDef.GetPortraitRef());
		}
		return false;
	}

	public static bool PremiumAnimationAvailable(CardDef cardDef)
	{
		if (!cardDef)
		{
			return false;
		}
		IAssetLoader assetLoader = AssetLoader.Get();
		if (!assetLoader.IsAssetAvailable(cardDef.GetPremiumMaterialRef()))
		{
			return false;
		}
		AssetReference portraitRef = cardDef.GetPremiumPortraitRef();
		if (portraitRef != null && !string.IsNullOrEmpty(portraitRef.guid) && !assetLoader.IsRuntimeAssetVariantAvailable(portraitRef, GetCardTextureOptions(forceLowRes: false)))
		{
			return false;
		}
		AssetReference animationRef = cardDef.GetPremiumAnimationRef();
		if (animationRef != null && !string.IsNullOrEmpty(animationRef.guid) && !assetLoader.IsAssetAvailable(animationRef))
		{
			return false;
		}
		return true;
	}

	private static bool PremiumAnimationExists(CardDef cardDef)
	{
		if (!cardDef)
		{
			return false;
		}
		AssetReference premiumMaterialRef = cardDef.GetPremiumMaterialRef();
		if (premiumMaterialRef != null)
		{
			return !string.IsNullOrEmpty(premiumMaterialRef.guid);
		}
		return false;
	}

	private static bool SignatureMaterialExists(CardDef cardDef)
	{
		if (!cardDef)
		{
			return false;
		}
		AssetReference signatureMaterialRef = cardDef.GetSignatureMaterialRef();
		if (signatureMaterialRef != null)
		{
			return !string.IsNullOrEmpty(signatureMaterialRef.guid);
		}
		return false;
	}

	private static async UniTaskVoid LoadDeferred(CardDef cardDef, Func<CardDef, bool> toWaitFor, CardPortraitQuality quality)
	{
		while (!toWaitFor(cardDef))
		{
			if (!cardDef || ItNotDownloading)
			{
				return;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896));
		}
		if (!Load(cardDef, quality, prohibitRecursion: true))
		{
			return;
		}
		Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>();
		foreach (Actor actor in array)
		{
			if (actor.HasSameCardDef(cardDef))
			{
				actor.UpdateAllComponents();
			}
		}
	}

	private static void LoadLowQuality(CardDef cardDef)
	{
		AssetReference portraitRef = cardDef.GetPortraitRef();
		if (portraitRef == null)
		{
			return;
		}
		using AssetHandle<Texture> portrait = AssetLoader.Get().LoadAsset<Texture>(portraitRef, GetCardTextureOptions(forceLowRes: true));
		if (!portrait)
		{
			Error.AddDevFatalUnlessWorkarounds("CardTextureLoader.LoadLowQuality - Failed to load asset for card {0}.  Portrait: {1}", cardDef.name, (portrait == null) ? "missing" : "loaded");
		}
		else
		{
			cardDef.OnPortraitLoaded(portrait, 1);
		}
	}

	private static bool LoadHighQuality(CardDef cardDef)
	{
		AssetReference portraitRef = cardDef.GetPortraitRef();
		if (portraitRef == null)
		{
			return false;
		}
		using (AssetHandle<Texture> hqPortrait = AssetLoader.Get().LoadAsset<Texture>(portraitRef, GetCardTextureOptions(forceLowRes: false)))
		{
			if ((bool)hqPortrait)
			{
				cardDef.OnPortraitLoaded(hqPortrait, 3);
				return true;
			}
		}
		using (AssetHandle<Texture> lqPortrait = AssetLoader.Get().LoadAsset<Texture>(portraitRef, GetCardTextureOptions(forceLowRes: true)))
		{
			if ((bool)lqPortrait)
			{
				cardDef.OnPortraitLoaded(lqPortrait, 1);
			}
			else
			{
				Error.AddDevFatalUnlessWorkarounds("CardTextureLoader.LoadHighQuality - Failed to load asset for card {0}.  Portrait: {1}", cardDef.name, "missing");
			}
		}
		return false;
	}

	private static void LoadGolden(CardDef cardDef)
	{
		if (cardDef == null)
		{
			return;
		}
		AssetReference premiumMaterialRef = cardDef.GetPremiumMaterialRef();
		AssetReference premiumPortraitRef = cardDef.GetPremiumPortraitRef();
		AssetReference premiumAnimationRef = cardDef.GetPremiumAnimationRef();
		if (premiumMaterialRef == null)
		{
			return;
		}
		using AssetHandle<Material> premiumMaterial = AssetLoader.Get().LoadAsset<Material>(premiumMaterialRef);
		using AssetHandle<UberShaderAnimation> premiumAnimation = ((premiumAnimationRef != null) ? AssetLoader.Get().LoadAsset<UberShaderAnimation>(premiumAnimationRef) : null);
		using AssetHandle<Texture> premiumPortrait = ((premiumPortraitRef != null) ? AssetLoader.Get().LoadAsset<Texture>(premiumPortraitRef, GetCardTextureOptions(forceLowRes: false)) : null);
		if (!premiumMaterial)
		{
			Error.AddDevFatalUnlessWorkarounds("CardTextureLoader.LoadGolden - Failed to load asset for card {0}.  Material: {1}, Premium Portrait: {2}, Animation: {3}", cardDef.name, (premiumMaterial == null) ? "missing" : "loaded", (premiumPortrait == null) ? "missing" : "loaded", (premiumAnimation == null) ? "missing" : "loaded");
		}
		else
		{
			cardDef.OnPremiumMaterialLoaded(premiumMaterial, premiumPortrait, premiumAnimation);
		}
	}

	private static void LoadSignature(CardDef cardDef)
	{
		if (cardDef == null)
		{
			return;
		}
		AssetReference signatureMaterialRef = cardDef.GetSignatureMaterialRef();
		AssetReference signaturePortraitRef = cardDef.GetSignaturePortraitRef();
		AssetReference signatureAnimationRef = cardDef.GetSignatureAnimationRef();
		if (signatureMaterialRef == null)
		{
			return;
		}
		using AssetHandle<Material> signatureMaterial = AssetLoader.Get().LoadAsset<Material>(signatureMaterialRef);
		using AssetHandle<UberShaderAnimation> signatureAnimation = ((signatureAnimationRef != null) ? AssetLoader.Get().LoadAsset<UberShaderAnimation>(signatureAnimationRef) : null);
		using AssetHandle<Texture> signaturePortrait = ((signaturePortraitRef != null) ? AssetLoader.Get().LoadAsset<Texture>(signaturePortraitRef, GetCardTextureOptions(forceLowRes: false)) : null);
		if (!signatureMaterial)
		{
			Error.AddDevFatalUnlessWorkarounds("CardTextureLoader.LoadSignature - Failed to load asset for card " + cardDef.name + ". Material missing,Texture " + ((signaturePortrait == null) ? "missing" : "loaded") + "Animation " + ((signatureAnimation == null) ? "missing" : "loaded"));
		}
		else
		{
			cardDef.OnSignatureMaterialLoaded(signatureMaterial, signaturePortrait, signatureAnimation);
		}
	}

	private static AssetLoadingOptions GetCardTextureOptions(bool forceLowRes)
	{
		if (forceLowRes || PlatformSettings.ShouldFallbackToLowRes)
		{
			return AssetLoadingOptions.UseLowQuality;
		}
		return AssetLoadingOptions.None;
	}
}
