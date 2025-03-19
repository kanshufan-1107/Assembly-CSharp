using System.Collections;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class StoreCollectionManagerCardInfoLoader : MonoBehaviour
{
	[SerializeField]
	[Tooltip("When provided will set parent to loaded card info, when empty will load to world position as set in loaded asset.")]
	private BindLegendaryHeroToMaterial m_bindLegendaryToMaterial;

	[SerializeField]
	[Tooltip("Alternative to using legendary hero material binder to bind asset to material.")]
	private MeshRenderer m_standard2DMaterialRenderer;

	[SerializeField]
	[Tooltip("Alternative to using legendary hero material binder to bind asset to material.")]
	private UberShaderController m_standard2DMaterialAnimController;

	private AssetHandle<UberShaderAnimation> m_loadedUberAnimation;

	private StoreItemDisplayDef m_loadedStoreDef;

	private CollectionHeroDef m_loadedHeroDef;

	[Tooltip("The widget to receive the CODE_ART_FAILED_TO_LOAD event.")]
	[SerializeField]
	private WidgetTemplate m_widget;

	public const string CARD_ART_FAILED_TO_LOAD = "CODE_CARD_ART_FAILED_TO_LOAD";

	private string m_cardId;

	[Overridable]
	public string CardId
	{
		get
		{
			return m_cardId;
		}
		set
		{
			if (m_cardId == value)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				Cleanup();
				return;
			}
			m_cardId = value;
			if (!TryLoadObjectFromCardInfo())
			{
				StartCoroutine(SendFailedToLoadNextFrame());
				Cleanup();
			}
		}
	}

	private void Cleanup()
	{
		m_cardId = string.Empty;
		if (m_bindLegendaryToMaterial != null)
		{
			m_bindLegendaryToMaterial.Cleanup();
		}
		if (m_loadedStoreDef != null)
		{
			Object.Destroy(m_loadedStoreDef.gameObject);
			m_loadedStoreDef = null;
		}
		if (m_loadedHeroDef != null)
		{
			Object.Destroy(m_loadedHeroDef.gameObject);
			m_loadedHeroDef = null;
		}
		AssetHandle.SafeDispose(ref m_loadedUberAnimation);
	}

	private bool TryLoadObjectFromCardInfo()
	{
		using DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(m_cardId);
		if (def?.CardDef == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to get card def for card id: " + m_cardId);
			return false;
		}
		if (string.IsNullOrEmpty(def.CardDef.m_StoreItemDisplayPath))
		{
			return TryLoadNonLegendaryHeroSkin(def);
		}
		return TryLoadLegendaryHeroSkin(def);
	}

	private bool TryLoadLegendaryHeroSkin(DefLoader.DisposableCardDef def)
	{
		if (m_bindLegendaryToMaterial == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing BindLegendaryHeroToMaterial component!");
			return false;
		}
		if (m_standard2DMaterialRenderer == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing MeshRenderer component!");
			return false;
		}
		if (m_standard2DMaterialAnimController == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing UberShaderController component!");
			return false;
		}
		if (m_loadedStoreDef != null)
		{
			Object.Destroy(m_loadedStoreDef.gameObject);
		}
		m_loadedStoreDef = GameUtils.LoadGameObjectWithComponent<StoreItemDisplayDef>(def.CardDef.m_StoreItemDisplayPath);
		if (m_loadedStoreDef == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to pull StoreItemDisplayDef for card " + m_cardId + "!");
			return false;
		}
		if (!string.IsNullOrEmpty(m_loadedStoreDef.m_CustomCMPortraitScene))
		{
			m_bindLegendaryToMaterial.gameObject.SetActive(value: true);
			m_standard2DMaterialRenderer.gameObject.SetActive(value: false);
			m_bindLegendaryToMaterial.LegendaryHeroPrefab = GetHeroPrefabPortrait();
			m_bindLegendaryToMaterial.BindMaterial();
		}
		else
		{
			if (m_loadedStoreDef.m_CustomCmPortraitMaterial == null)
			{
				Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: Failed to loaded CM scene to store as card:" + m_cardId + " has no info prefab or material set in StoreItemDisplayDef!");
				return false;
			}
			m_bindLegendaryToMaterial.gameObject.SetActive(value: false);
			m_standard2DMaterialRenderer.gameObject.SetActive(value: true);
			m_standard2DMaterialRenderer.SetSharedMaterial(m_loadedStoreDef.m_CustomCmPortraitMaterial.GetMaterial());
			if (!string.IsNullOrEmpty(m_loadedStoreDef.m_materialAnimationPath))
			{
				AssetHandle.SafeDispose(ref m_loadedUberAnimation);
				AssetLoader.Get().LoadAsset(ref m_loadedUberAnimation, m_loadedStoreDef.m_materialAnimationPath);
				if (m_loadedUberAnimation == null)
				{
					Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: Failed to loaded CM scene to store as card:" + m_cardId + " Failed to load UberShaderAnimation!");
					return false;
				}
				m_standard2DMaterialAnimController.UberShaderAnimation = m_loadedUberAnimation;
			}
		}
		Object.Destroy(m_loadedStoreDef.gameObject);
		m_loadedStoreDef = null;
		return true;
	}

	private bool TryLoadNonLegendaryHeroSkin(DefLoader.DisposableCardDef def)
	{
		if (m_standard2DMaterialRenderer == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing MeshRenderer component!");
			return false;
		}
		if (m_standard2DMaterialAnimController == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing UberShaderController component!");
			return false;
		}
		if (m_loadedHeroDef != null)
		{
			Object.Destroy(m_loadedHeroDef.gameObject);
		}
		if (m_standard2DMaterialRenderer == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to use StoreItemDisplayDef data due to missing MeshRenderer component!");
			return false;
		}
		m_loadedHeroDef = GameUtils.LoadGameObjectWithComponent<CollectionHeroDef>(def.CardDef.m_CollectionHeroDefPath);
		if (m_loadedHeroDef == null)
		{
			Log.Store.PrintError("StoreCollectionManagerCardInfoLoader: Failed to pull StoreItemDisplayDef for card " + m_cardId + "!");
			return false;
		}
		try
		{
			m_bindLegendaryToMaterial.gameObject.SetActive(value: false);
			m_standard2DMaterialRenderer.gameObject.SetActive(value: true);
			if (m_loadedHeroDef.m_previewMaterial == null)
			{
				Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: failed to find a full art material for card " + m_cardId + "!");
				return false;
			}
			Material heroSkinFullArtMaterial = m_loadedHeroDef.m_previewMaterial.GetMaterial();
			if (heroSkinFullArtMaterial == null)
			{
				Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: failed to find a full art material for card " + m_cardId + "!");
				return false;
			}
			m_standard2DMaterialRenderer.SetSharedMaterial(heroSkinFullArtMaterial);
			if (!string.IsNullOrEmpty(m_loadedHeroDef.m_materialAnimationPath))
			{
				AssetHandle.SafeDispose(ref m_loadedUberAnimation);
				IAssetLoader assetLoader = AssetLoader.Get();
				if (assetLoader == null)
				{
					Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: Failed to loaded CM scene to store as card:" + m_cardId + ", error: AssetLoader is null");
					return false;
				}
				if (!assetLoader.LoadAsset(ref m_loadedUberAnimation, m_loadedHeroDef.m_materialAnimationPath))
				{
					Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: Failed to loaded CM scene to store as card:" + m_cardId + ", error: Failed to load asset: '" + m_loadedHeroDef.m_materialAnimationPath + "'");
					return false;
				}
				if (m_loadedUberAnimation == null)
				{
					Log.Store.PrintWarning("StoreCollectionManagerCardInfoLoader: Failed to loaded CM scene to store as card:" + m_cardId + " Failed to load UberShaderAnimation!");
					return false;
				}
				m_standard2DMaterialAnimController.UberShaderAnimation = m_loadedUberAnimation;
			}
			return true;
		}
		finally
		{
			Object.Destroy(m_loadedHeroDef.gameObject);
			m_loadedHeroDef = null;
		}
	}

	private string GetHeroPrefabPortrait()
	{
		if (PremiumHeroSkinUtil.ContainsMultipleHeros(m_widget) && !string.IsNullOrEmpty(m_loadedStoreDef.m_MultiHeroCMPortraitScene))
		{
			return m_loadedStoreDef.m_MultiHeroCMPortraitScene;
		}
		return m_loadedStoreDef.m_CustomCMPortraitScene;
	}

	private IEnumerator SendFailedToLoadNextFrame()
	{
		Log.All.Print($"[agamble] SendFailedToLoadNextFrame {m_widget != null} ");
		yield return null;
		if (m_widget != null)
		{
			m_widget.TriggerEvent("CODE_CARD_ART_FAILED_TO_LOAD", new TriggerEventParameters(null, null, noDownwardPropagation: true));
		}
	}
}
