using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class DynamicVideoAssigner : MonoBehaviour
{
	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private string m_VideoIdToLoad;

	[SerializeField]
	[Header("Display Methods")]
	private DynamicVideoLoader m_DynamicVideo;

	[SerializeField]
	private DynamicVideoCaptionDriver m_dynamicCaptionDriver;

	[SerializeField]
	private CutsceneSceneLoader m_cutsceneSceneLoader;

	[SerializeField]
	[Header("Display Settings")]
	private MeshRenderer m_videoRenderer;

	[SerializeField]
	private MeshRenderer m_cutsceneRenderer;

	private StoreItemDisplayDef m_LoadedStoreDef;

	private CutsceneSceneDef m_loadedCutsceneDef;

	private string m_loadedCutsceneDefAssetRef;

	private string m_CardId;

	[Overridable]
	public string CardId
	{
		get
		{
			return m_CardId;
		}
		set
		{
			if (string.IsNullOrEmpty(value) || m_DynamicVideo == null)
			{
				Cleanup();
				return;
			}
			m_CardId = value;
			if (!TryLoadVideosOnCardSet())
			{
				Cleanup();
			}
		}
	}

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
		CutsceneManager manager = CutsceneManager.Get();
		if (manager != null && m_loadedCutsceneDef != null)
		{
			SafeSetDisplayRenderer(isVideo: false);
			manager.Play(m_loadedCutsceneDef);
		}
	}

	private void OnDisable()
	{
		CutsceneManager manager = CutsceneManager.Get();
		if (manager != null)
		{
			manager.Stop();
		}
		if (m_cutsceneSceneLoader != null)
		{
			m_cutsceneSceneLoader.Cleanup();
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		m_CardId = string.Empty;
		if (m_LoadedStoreDef != null)
		{
			Object.Destroy(m_LoadedStoreDef.gameObject);
			m_LoadedStoreDef = null;
		}
		CutsceneManager cutSceneManager = CutsceneManager.Get();
		if (cutSceneManager != null)
		{
			cutSceneManager.Stop();
		}
		if (m_loadedCutsceneDef != null)
		{
			Object.Destroy(m_loadedCutsceneDef.gameObject);
		}
		m_loadedCutsceneDefAssetRef = string.Empty;
		if (m_DynamicVideo != null)
		{
			m_DynamicVideo.OnClosed();
			m_DynamicVideo.VideoLocation = string.Empty;
			m_DynamicVideo.FallbackTextureLocation = string.Empty;
		}
		if (m_dynamicCaptionDriver != null)
		{
			m_dynamicCaptionDriver.VideoCaptionKeys = null;
		}
	}

	private void Init()
	{
		if (!(m_widget == null))
		{
			m_widget.GetDataModel(15, out var model);
			ProductDataModel productDataModel = model as ProductDataModel;
			SetCardId(productDataModel);
		}
	}

	private void SetCardId(ProductDataModel product)
	{
		if (product == null)
		{
			return;
		}
		foreach (RewardItemDataModel item in product.Items)
		{
			if (item.ItemType == RewardItemType.HERO_SKIN)
			{
				CardId = item.Card.CardId;
				break;
			}
		}
	}

	private bool TryLoadVideosOnCardSet()
	{
		using DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(m_CardId);
		if (def?.CardDef == null)
		{
			Debug.LogError("Failed to assign dynamic video(s) for card id: " + m_CardId);
			return false;
		}
		if (string.IsNullOrEmpty(def.CardDef.m_StoreItemDisplayPath))
		{
			return false;
		}
		if (m_LoadedStoreDef != null)
		{
			Object.Destroy(m_LoadedStoreDef.gameObject);
		}
		m_LoadedStoreDef = GameUtils.LoadGameObjectWithComponent<StoreItemDisplayDef>(def.CardDef.m_StoreItemDisplayPath);
		if (m_LoadedStoreDef == null)
		{
			Debug.LogError("Failed to pull StoreItemDisplayDef for card " + m_CardId + "!");
			return false;
		}
		bool isSuccess = false;
		if (m_LoadedStoreDef.ShouldUseCutsceneInsteadOfVideo())
		{
			string cutsceneDef = (PremiumHeroSkinUtil.ContainsMultipleHeros(m_widget) ? m_LoadedStoreDef.m_MultiHeroCutsceneSceneDef : m_LoadedStoreDef.m_CutsceneSceneDef);
			isSuccess = TryLoadCutscene(cutsceneDef);
		}
		else
		{
			isSuccess = TrySetVideoToPlayer(m_LoadedStoreDef.GetStoreVideoDisplay(m_VideoIdToLoad));
		}
		Object.Destroy(m_LoadedStoreDef.gameObject);
		m_LoadedStoreDef = null;
		return isSuccess;
	}

	private bool TrySetVideoToPlayer(StoreItemDisplayDef.StoreVideoDisplay displayVideo)
	{
		if (displayVideo == null)
		{
			Debug.LogError("StoreItemDisplayDef returned null videos!");
			return false;
		}
		if (m_DynamicVideo == null)
		{
			Debug.LogError("StoreItemDisplayDef has no DynamicVideoLoader to assign videos to!");
			return false;
		}
		SafeSetDisplayRenderer(isVideo: true);
		m_DynamicVideo.VideoLocation = displayVideo.VideoPath;
		m_DynamicVideo.FallbackTextureLocation = displayVideo.FallbackTexturePath;
		if ((bool)m_dynamicCaptionDriver && displayVideo.VideoCaptions != null && displayVideo.VideoCaptions.Count > 0)
		{
			m_dynamicCaptionDriver.VideoCaptionKeys = displayVideo.VideoCaptions;
		}
		return true;
	}

	private bool TryLoadCutscene(string cutsceneDefAssetRef)
	{
		if (string.IsNullOrEmpty(cutsceneDefAssetRef))
		{
			Debug.LogError("Failed to load and apply cutscene from DynamicVideoAssigner for " + m_CardId + " due to empty CutsceneSceneDef");
			return false;
		}
		if (m_cutsceneSceneLoader.LoadCutscene())
		{
			if (cutsceneDefAssetRef != m_loadedCutsceneDefAssetRef || m_loadedCutsceneDef == null)
			{
				if (m_loadedCutsceneDef != null)
				{
					Object.Destroy(m_loadedCutsceneDef.gameObject);
				}
				m_loadedCutsceneDef = GameUtils.LoadGameObjectWithComponent<CutsceneSceneDef>(cutsceneDefAssetRef);
				if (m_LoadedStoreDef == null)
				{
					Debug.LogError("Failed to pull CutsceneSceneDef for card " + m_CardId + "!");
					return false;
				}
				m_loadedCutsceneDefAssetRef = cutsceneDefAssetRef;
			}
			CutsceneManager manager = CutsceneManager.Get();
			if (m_cutsceneRenderer == null || manager == null)
			{
				return false;
			}
			SafeSetDisplayRenderer(isVideo: false);
			m_cutsceneRenderer.GetSharedMaterial().mainTexture = manager.GetTexture();
			manager.Play(m_loadedCutsceneDef);
			return true;
		}
		return false;
	}

	private void SafeSetDisplayRenderer(bool isVideo)
	{
		if (m_videoRenderer != null)
		{
			m_videoRenderer.enabled = isVideo;
		}
		if (m_cutsceneRenderer != null)
		{
			m_cutsceneRenderer.enabled = !isVideo;
		}
	}
}
