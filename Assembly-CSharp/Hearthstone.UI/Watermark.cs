using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[AddComponentMenu("")]
[WidgetBehaviorDescription(Path = "Hearthstone/Watermark", UniqueWithinCategory = "asset")]
public class Watermark : CustomWidgetBehavior
{
	private struct TextureSource
	{
		public enum SourceType
		{
			NONE,
			ADVENTURE_ID,
			BOOSTER_ID,
			BOOSTER_CARD_SET_ID
		}

		public SourceType Type;

		public int Id;
	}

	private struct CallbackData
	{
		public bool m_localized;

		public int m_asyncOperationId;
	}

	[SerializeField]
	[Tooltip("Base Material to texture with")]
	private Material m_baseMaterial;

	[Header("Mutually Exclusive")]
	[Tooltip("If set, it will use this texture directly instead of deriving the texture from a bound data model.")]
	[SerializeField]
	private string m_directTextureReference;

	[SerializeField]
	[Tooltip("If true, it will use data model adventure/pack/product/rewardItem whenever bound.")]
	private bool m_useDataModel = true;

	[Tooltip("Name of Adventure (AdventureDbId) to use the watermark from")]
	[SerializeField]
	private AdventureDbId m_adventureDbId;

	[Tooltip("Name of Booster (BoosterDbId) to use the watermark from")]
	[SerializeField]
	private BoosterDbId m_boosterDbId;

	[Tooltip("Whether or not to use the BoosterDbId's mini-set watermark in Booster mode")]
	[SerializeField]
	private bool m_useMiniSetWatermark;

	private HashSet<int> m_dataModelIDs = new HashSet<int>();

	private GameObject m_quad;

	private Material m_material;

	private AssetHandle<Texture> m_texture;

	private TextureSource m_targetTextureSource;

	private TextureSource m_currentTextureSource;

	private AssetReference m_targetDirectAssetReference;

	private AssetReference m_currentDirectAssetReference;

	private int m_textureAsyncOperationId;

	private int m_lastDataVersion;

	[Overridable]
	public string AdventureIdName
	{
		get
		{
			return m_adventureDbId.ToString();
		}
		set
		{
			ResetSources();
			if (!EnumUtils.TryGetEnum<AdventureDbId>(value, out m_adventureDbId))
			{
				Debug.LogErrorFormat("Invalid AdventureDbId name: {0}; reseting to {1}", value, AdventureIdName);
			}
			UpdateTargetTextureSource();
		}
	}

	[Overridable]
	public string BoosterIdName
	{
		get
		{
			return m_boosterDbId.ToString();
		}
		set
		{
			ResetSources();
			if (!EnumUtils.TryGetEnum<BoosterDbId>(value, out m_boosterDbId))
			{
				Debug.LogErrorFormat("Invalid BoosterDbId name: {0}; reseting to {1}", value, BoosterIdName);
			}
			UpdateTargetTextureSource();
		}
	}

	[Overridable]
	public BoosterDbId BoosterDbId
	{
		get
		{
			return m_boosterDbId;
		}
		set
		{
			ResetSources();
			m_boosterDbId = value;
			UpdateTargetTextureSource();
		}
	}

	[Overridable]
	public bool UseDataModel
	{
		get
		{
			return m_useDataModel;
		}
		set
		{
			ResetSources();
			m_useDataModel = value;
			UpdateTargetTextureSource();
		}
	}

	[Overridable]
	public bool UseMiniSetWatermark
	{
		get
		{
			return m_useMiniSetWatermark;
		}
		set
		{
			m_useMiniSetWatermark = value;
			UpdateTargetTextureSource();
		}
	}

	[Overridable]
	public string DirectTextureReference
	{
		get
		{
			return m_directTextureReference;
		}
		set
		{
			ResetSources();
			m_directTextureReference = value;
			UpdateTargetTextureSource();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		UpdateTargetTextureSource();
		CreatePreviewableObject();
	}

	public override bool TryIncrementDataVersion(int id)
	{
		if (UseDataModel && m_dataModelIDs.Contains(id))
		{
			IncrementLocalDataVersion();
			return true;
		}
		return false;
	}

	protected override void OnDestroy()
	{
		CleanUpMaterial();
		base.OnDestroy();
	}

	private void ResetSources()
	{
		m_lastDataVersion = 0;
		m_useDataModel = false;
		m_adventureDbId = AdventureDbId.INVALID;
		m_boosterDbId = BoosterDbId.INVALID;
		m_directTextureReference = null;
	}

	private void CleanUpMaterial()
	{
		AssetHandle.SafeDispose(ref m_texture);
	}

	private void CreatePreviewableObject()
	{
		CreatePreviewableObject(delegate(IPreviewableObject previewable, Action<GameObject> callback)
		{
			CleanUpMaterial();
			if (AssetLoader.Get() == null)
			{
				Debug.LogWarning("Hearthstone.UI.Watermark.OnInitialize() - AssetLoader not available");
				callback(null);
			}
			else if (m_baseMaterial == null)
			{
				Debug.LogWarning("Hearthstone.UI.Watermark.OnInitialize() - No material found");
				callback(null);
			}
			else
			{
				m_currentTextureSource = m_targetTextureSource;
				m_currentDirectAssetReference = m_targetDirectAssetReference;
				string textureName;
				if (string.IsNullOrWhiteSpace(m_targetDirectAssetReference))
				{
					textureName = GetTextureName(m_currentTextureSource);
				}
				else
				{
					textureName = m_targetDirectAssetReference;
				}
				if (string.IsNullOrEmpty(textureName))
				{
					callback(null);
				}
				else
				{
					AssetHandleCallback<Texture> onTextureLoaded = null;
					onTextureLoaded = delegate(AssetReference name, AssetHandle<Texture> tex, object data)
					{
						CallbackData callbackData = (CallbackData)data;
						if (m_quad == null || callbackData.m_asyncOperationId != m_textureAsyncOperationId)
						{
							AssetHandle.SafeDispose(ref tex);
						}
						else if (tex == null)
						{
							if (callbackData.m_localized)
							{
								callbackData.m_localized = false;
								Error.AddDevFatal("Loading localized logo failed.  This is normal if we're on android and just switched.  Trying unlocalized.");
								if (!AssetLoader.Get().LoadAsset(textureName, onTextureLoaded, callbackData, AssetLoadingOptions.DisableLocalization))
								{
									onTextureLoaded(textureName, null, callbackData);
								}
							}
							else
							{
								Debug.LogError($"Failed to load unlocalized texture {base.name}!");
							}
						}
						else
						{
							AssetHandle.Take(ref m_texture, tex);
							m_material = new Material(m_baseMaterial);
							m_material.name = $"{m_baseMaterial.name}({m_texture.Asset.name})";
							m_material.mainTexture = m_texture;
							Renderer component = m_quad.GetComponent<Renderer>();
							component.SetSharedMaterial(m_material);
							component.enabled = true;
						}
					};
					m_quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
					m_quad.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
					m_quad.GetComponent<Renderer>().enabled = false;
					m_textureAsyncOperationId++;
					CallbackData callbackData2 = default(CallbackData);
					callbackData2.m_localized = true;
					callbackData2.m_asyncOperationId = m_textureAsyncOperationId;
					CallbackData callbackData3 = callbackData2;
					if (!AssetLoader.Get().LoadAsset(textureName, onTextureLoaded, callbackData3))
					{
						onTextureLoaded(textureName, null, callbackData3);
					}
					callback(m_quad);
				}
			}
		}, (IPreviewableObject o) => NeedsToRecreate());
	}

	private bool NeedsToRecreate()
	{
		if (UseDataModel && m_lastDataVersion != GetLocalDataVersion())
		{
			UpdateTargetTextureSource();
		}
		if (m_targetDirectAssetReference == m_currentDirectAssetReference && m_targetTextureSource.Type == m_currentTextureSource.Type)
		{
			return m_targetTextureSource.Id != m_currentTextureSource.Id;
		}
		return true;
	}

	private TextureSource GetTextureSourceFromDataModel()
	{
		if (UseDataModel)
		{
			if (GetDataModel(7, out var dataModel))
			{
				m_dataModelIDs.Add(7);
				AdventureDataModel adventure = dataModel as AdventureDataModel;
				if (adventure.SelectedAdventure != 0)
				{
					TextureSource result = default(TextureSource);
					result.Type = TextureSource.SourceType.ADVENTURE_ID;
					result.Id = (int)adventure.SelectedAdventure;
					return result;
				}
			}
			if (GetDataModel(25, out dataModel))
			{
				m_dataModelIDs.Add(25);
				PackDataModel pack = dataModel as PackDataModel;
				m_boosterDbId = pack.Type;
				if (m_boosterDbId != 0)
				{
					TextureSource result = default(TextureSource);
					result.Type = TextureSource.SourceType.BOOSTER_ID;
					result.Id = (int)pack.Type;
					return result;
				}
			}
			if (GetDataModel(17, out dataModel))
			{
				m_dataModelIDs.Add(17);
				RewardItemDataModel item = dataModel as RewardItemDataModel;
				if (GetTextureSourceFromRewardItemDataModel(item, out var source))
				{
					return source;
				}
			}
			if (GetDataModel(15, out dataModel))
			{
				m_dataModelIDs.Add(15);
				foreach (RewardItemDataModel item2 in (dataModel as ProductDataModel).Items)
				{
					if (GetTextureSourceFromRewardItemDataModel(item2, out var source2))
					{
						return source2;
					}
				}
			}
		}
		return default(TextureSource);
	}

	private bool GetTextureSourceFromRewardItemDataModel(RewardItemDataModel item, out TextureSource source)
	{
		switch (item.ItemType)
		{
		case RewardItemType.ADVENTURE:
			source = new TextureSource
			{
				Type = TextureSource.SourceType.ADVENTURE_ID,
				Id = item.ItemId
			};
			return true;
		case RewardItemType.BOOSTER:
			source = new TextureSource
			{
				Type = TextureSource.SourceType.BOOSTER_ID,
				Id = item.ItemId
			};
			return true;
		case RewardItemType.RANDOM_CARD:
			source = new TextureSource
			{
				Type = TextureSource.SourceType.BOOSTER_CARD_SET_ID,
				Id = item.ItemId
			};
			return true;
		default:
			source = default(TextureSource);
			return false;
		}
	}

	private void UpdateTargetTextureSource()
	{
		m_targetDirectAssetReference = m_directTextureReference;
		if (string.IsNullOrWhiteSpace(m_directTextureReference))
		{
			if (UseDataModel)
			{
				m_targetTextureSource = GetTextureSourceFromDataModel();
				m_lastDataVersion = GetLocalDataVersion();
			}
			else if (m_adventureDbId != 0)
			{
				m_targetTextureSource.Type = TextureSource.SourceType.ADVENTURE_ID;
				m_targetTextureSource.Id = (int)m_adventureDbId;
			}
			else if (m_boosterDbId != 0)
			{
				m_targetTextureSource.Type = TextureSource.SourceType.BOOSTER_ID;
				m_targetTextureSource.Id = (int)m_boosterDbId;
			}
			else
			{
				m_targetTextureSource.Type = TextureSource.SourceType.NONE;
			}
		}
	}

	private string GetTextureName(TextureSource source)
	{
		switch (source.Type)
		{
		case TextureSource.SourceType.ADVENTURE_ID:
		{
			using AssetHandle<GameObject> adventureDefPrefab = ShopUtils.LoadStoreAdventurePrefab((AdventureDbId)source.Id);
			StoreAdventureDef adventureDef = (adventureDefPrefab ? adventureDefPrefab.Asset.GetComponent<StoreAdventureDef>() : null);
			return (adventureDef != null) ? adventureDef.GetAccentTextureName() : string.Empty;
		}
		case TextureSource.SourceType.BOOSTER_ID:
		{
			using AssetHandle<GameObject> packDefPrefab = ShopUtils.LoadStorePackPrefab((BoosterDbId)source.Id);
			StorePackDef packDef = (packDefPrefab ? packDefPrefab.Asset.GetComponent<StorePackDef>() : null);
			return (!(packDef != null)) ? string.Empty : (m_useMiniSetWatermark ? packDef.GetMiniSetAccentTextureName() : packDef.GetAccentTextureName());
		}
		case TextureSource.SourceType.BOOSTER_CARD_SET_ID:
		{
			BoosterCardSetDbfRecord boosterCardSetRecord = GameDbf.BoosterCardSet?.GetRecord(source.Id);
			if (boosterCardSetRecord == null)
			{
				return string.Empty;
			}
			if (!string.IsNullOrEmpty(boosterCardSetRecord.WatermarkTextureOverride))
			{
				return boosterCardSetRecord.WatermarkTextureOverride;
			}
			if (boosterCardSetRecord.CardSetRecord == null)
			{
				return string.Empty;
			}
			if (boosterCardSetRecord.CardSetRecord.IsCoreCardSet)
			{
				return SetRotationIcon.GetYearIconWatermark();
			}
			if (boosterCardSetRecord.CardSetRecord.CardWatermarkTexture != null)
			{
				return boosterCardSetRecord.CardSetRecord.CardWatermarkTexture;
			}
			return string.Empty;
		}
		default:
			return string.Empty;
		}
	}
}
