using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DeckPouchReward : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer m_deckArt;

	public Material m_portraitMaterial;

	private Widget m_widget;

	private Material m_temporaryPortraitMaterial;

	private AssetHandle<Texture> m_portraitTextureHandle;

	private AssetHandle<Material> m_portraitMaterialHandle;

	private int m_currentDisplayCardId;

	private const string CODE_UPDATE_REWARD_DECK = "CODE_UPDATE_REWARD_DECK";

	private const string CODE_UPDATE_PORTRAIT = "CODE_UPDATE_PORTRAIT";

	public void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	public void OnDestroy()
	{
		SafelyDisposeTempPortrait();
		AssetHandle.SafeDispose(ref m_portraitMaterialHandle);
		AssetHandle.SafeDispose(ref m_portraitTextureHandle);
	}

	private Material GetPortraitMaterialFromDeckTemplateRecord(DeckTemplateDbfRecord record)
	{
		if (record == null)
		{
			return null;
		}
		if (m_portraitMaterial != null && record.DisplayCardId != 0)
		{
			using (DefLoader.DisposableCardDef displayCard = DefLoader.Get().GetCardDef(record.DisplayCardId))
			{
				if (m_temporaryPortraitMaterial == null || m_currentDisplayCardId != record.DisplayCardId)
				{
					SafelyDisposeTempPortrait();
					m_temporaryPortraitMaterial = new Material(m_portraitMaterial);
				}
				AssetHandle.Set(ref m_portraitTextureHandle, displayCard?.CardDef.GetPortraitTextureHandle());
				m_temporaryPortraitMaterial.mainTexture = m_portraitTextureHandle;
				m_currentDisplayCardId = record.DisplayCardId;
			}
			return m_temporaryPortraitMaterial;
		}
		AssetLoader.Get().LoadAsset(ref m_portraitMaterialHandle, record.DisplayTexture);
		return m_portraitMaterialHandle;
	}

	private void SafelyDisposeTempPortrait()
	{
		if (m_temporaryPortraitMaterial != null)
		{
			Object.Destroy(m_temporaryPortraitMaterial);
			m_temporaryPortraitMaterial = null;
		}
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "CODE_UPDATE_PORTRAIT"))
		{
			if (eventName == "CODE_UPDATE_REWARD_DECK")
			{
				RewardItemDataModel rewardItemDatamodel = m_widget.GetDataModel<RewardItemDataModel>();
				CreateDataModelFromRewardItem(rewardItemDatamodel);
			}
		}
		else
		{
			DeckPouchDataModel deckPouchDatamodel = m_widget.GetDataModel<DeckPouchDataModel>();
			UpdatePortrait(deckPouchDatamodel);
		}
	}

	private void UpdatePortrait(DeckPouchDataModel deckPouchDatamodel)
	{
		DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord(deckPouchDatamodel.DeckTemplateId);
		if (deckTemplateRecord != null)
		{
			deckPouchDatamodel.Pouch.DisplayTexture = GetPortraitMaterialFromDeckTemplateRecord(deckTemplateRecord);
		}
	}

	private void CreateDataModelFromRewardItem(RewardItemDataModel rewardItemDatamodel)
	{
		if (rewardItemDatamodel != null)
		{
			RewardItemDbfRecord rewardItemRecord = GameDbf.RewardItem.GetRecord(rewardItemDatamodel.AssetId);
			if (rewardItemRecord != null)
			{
				DeckPouchDataModel deckPouchDataModel = ShopDeckPouchDisplay.CreateDeckPouchDataModelFromDeckTemplate(rewardItemRecord.DeckRecord, null);
				m_widget.BindDataModel(deckPouchDataModel);
			}
		}
	}
}
