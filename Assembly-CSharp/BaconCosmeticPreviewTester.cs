using System;
using System.Collections;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconCosmeticPreviewTester : MonoBehaviour
{
	public BaconCosmeticPreviewRunnerConfig m_config;

	[HideInInspector]
	public bool m_assetResolverServiceLoaded;

	[HideInInspector]
	public bool m_dbfLoaded;

	private bool m_widgetLoaded;

	private Widget m_widget;

	public static Action LoadSceneFromEditor;

	[SerializeField]
	private GameObject m_rtObject;

	private ProductPage m_productPage;

	private const int MAXIMUM_RT_DISPLAY_WIDTH = 1024;

	private const int MAXIMUM_RT_DISPLAY_HEIGHT = 1024;

	public static event Action RunCosmetic;

	public static event Action StopCosmetic;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		if (m_widget == null)
		{
			Log.CosmeticPreview.PrintError("Error [BaconCosmeticPreviewTester] Awake: m_widget not found!");
			return;
		}
		m_widget.RegisterDoneChangingStatesListener(OnWidgetDoneChangingStates);
		m_widget.RegisterEventListener(CosmeticActionEventListener);
		InitProductPage();
		if (BaconCosmeticPreviewLoadInfo.s_RT == null)
		{
			int rtWidth = 1024;
			int rtHeight = 1024;
			RenderTexture renderTexture = new RenderTexture(new RenderTextureDescriptor(rtWidth, rtHeight, RenderTextureFormat.ARGB32, 24, 0));
			renderTexture.autoGenerateMips = false;
			renderTexture.antiAliasing = 1;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Trilinear;
			renderTexture.anisoLevel = 0;
			renderTexture.Create();
			BaconCosmeticPreviewLoadInfo.s_RT = renderTexture;
		}
		m_rtObject.GetComponent<Renderer>().GetSharedMaterial().mainTexture = BaconCosmeticPreviewLoadInfo.s_RT;
	}

	private void OnEnable()
	{
		if (m_widget != null)
		{
			m_widgetLoaded = false;
		}
		LoadScene();
	}

	private void OnDisable()
	{
		if (m_widget != null)
		{
			m_widget.TriggerEvent("DISABLE_RT");
		}
	}

	private void OnDestroy()
	{
		if (m_productPage != null)
		{
			m_productPage.OnProductVariantSet -= OnProductChanged;
		}
	}

	private void CosmeticActionEventListener(string eventName)
	{
		if (!(eventName == "RunCosmetic"))
		{
			if (eventName == "StopCosmetic")
			{
				OnStopCosmetic();
			}
		}
		else
		{
			OnRunCosmetic();
		}
	}

	private void OnWidgetDoneChangingStates(object unused)
	{
		if (!m_widgetLoaded)
		{
			IDataModel boardSkinDataModel;
			IDataModel finisherDataModel;
			IDataModel luckyDrawDataModel;
			if (m_widget.GetDataModel(15, out var productDataModel))
			{
				GetPreviewConfigFromProductDatamodel((ProductDataModel)productDataModel);
			}
			else if (m_widget.GetDataModel(564, out boardSkinDataModel))
			{
				GetPreviewConfigFromBoardSkinDataModel((BattlegroundsBoardSkinDataModel)boardSkinDataModel);
			}
			else if (m_widget.GetDataModel(566, out finisherDataModel))
			{
				GetPreviewConfigFromFinisherDataModel((BattlegroundsFinisherDataModel)finisherDataModel);
			}
			else if (m_widget.GetDataModel(667, out luckyDrawDataModel))
			{
				GetPreviewConfigFromLuckyDrawRewardDataModel((LuckyDrawRewardDataModel)luckyDrawDataModel);
			}
		}
	}

	private void InitProductPage()
	{
		m_productPage = GetComponentInParent<ProductPage>();
		if (m_productPage == null)
		{
			Log.Store.PrintError("[BaconCosmeticPreviewTester] Product Page null in Awake. Not a child of a product page?");
		}
		else
		{
			m_productPage.OnProductVariantSet += OnProductChanged;
		}
	}

	private void OnProductChanged(ProductSelectionDataModel selection)
	{
		if (selection != null && selection.Variant != null)
		{
			GetPreviewConfigFromProductDatamodel(selection.Variant);
			OnRunCosmetic();
		}
	}

	private void OnRunCosmetic()
	{
		if (BaconCosmeticPreviewTester.RunCosmetic != null)
		{
			BaconCosmeticPreviewTester.RunCosmetic();
		}
	}

	private void OnStopCosmetic()
	{
		if (BaconCosmeticPreviewTester.StopCosmetic != null)
		{
			BaconCosmeticPreviewTester.StopCosmetic();
		}
	}

	private void GetPreviewConfigFromProductDatamodel(ProductDataModel targetProduct)
	{
		if (targetProduct.RewardList == null || targetProduct.RewardList.Items[0] == null)
		{
			Log.CosmeticPreview.PrintError("Error [BaconCosmeticPreviewTester] GetPreviewConfigFromProductDatamodel targetProduct has no valid target!");
			m_config = null;
		}
		else
		{
			RewardItemDataModel targetItem = targetProduct.RewardList.Items[0];
			GetPreviewConfigFromRewardItemDataModel(targetItem);
		}
	}

	private void GetPreviewConfigFromLuckyDrawRewardDataModel(LuckyDrawRewardDataModel targetReward)
	{
		if (targetReward.RewardList == null || targetReward.RewardList.Items[0] == null)
		{
			Log.CosmeticPreview.PrintError("Error [BaconCosmeticPreviewTester] GetPreviewConfigFromLuckyDrawRewardDataModel targetReward was null");
			m_config = null;
		}
		else
		{
			RewardItemDataModel targetItem = targetReward.RewardList.Items[0];
			GetPreviewConfigFromRewardItemDataModel(targetItem);
		}
	}

	private void GetPreviewConfigFromRewardItemDataModel(RewardItemDataModel rewardItem)
	{
		switch (rewardItem.ItemType)
		{
		case RewardItemType.BATTLEGROUNDS_FINISHER:
			GetPreviewConfigFromFinisherDataModel(rewardItem.BGFinisher);
			break;
		case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
			GetPreviewConfigFromBoardSkinDataModel(rewardItem.BGBoardSkin);
			break;
		}
	}

	private void GetPreviewConfigFromFinisherDataModel(BattlegroundsFinisherDataModel targetFinisher)
	{
		if (!string.IsNullOrEmpty(targetFinisher.DetailsRenderConfig))
		{
			m_config = CreateConfigAssetFromString(targetFinisher.DetailsRenderConfig);
		}
		m_widgetLoaded = true;
		BaconCosmeticPreviewLoadInfo.s_runnerConfig = m_config;
	}

	private void GetPreviewConfigFromBoardSkinDataModel(BattlegroundsBoardSkinDataModel targetBoardSkin)
	{
		if (!string.IsNullOrEmpty(targetBoardSkin.DetailsRenderConfig))
		{
			m_config = CreateConfigAssetFromString(targetBoardSkin.DetailsRenderConfig);
		}
		m_widgetLoaded = true;
		BaconCosmeticPreviewLoadInfo.s_runnerConfig = m_config;
	}

	public void LoadScene()
	{
		StartCoroutine(LoadSceneCoroutine());
	}

	public IEnumerator LoadSceneCoroutine()
	{
		while (m_widget != null && !m_widgetLoaded)
		{
			yield return null;
		}
		EnableRT();
	}

	private void EnableRT()
	{
		m_widget.TriggerEvent("ENABLE_RT");
	}

	private AssetHandle<BaconCosmeticPreviewRunnerConfig> CreateConfigAssetFromString(string configAssetPath)
	{
		AssetReference configReference = AssetReference.CreateFromAssetString(configAssetPath);
		return AssetLoader.Get().LoadAsset<BaconCosmeticPreviewRunnerConfig>(configReference);
	}
}
