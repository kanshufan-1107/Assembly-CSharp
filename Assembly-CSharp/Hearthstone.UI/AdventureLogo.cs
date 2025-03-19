using System;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[AddComponentMenu("")]
[WidgetBehaviorDescription(Path = "Hearthstone/AdventureLogo", UniqueWithinCategory = "asset")]
public class AdventureLogo : CustomWidgetBehavior
{
	private class AdventureLogoRenderData
	{
		public Renderer m_renderer;

		public Material m_material;

		public Material m_baseMaterial;
	}

	private class AdventureLogoCallbackData
	{
		public AdventureLogoRenderData m_renderData;

		public AdventureDbId m_adventureDbId;

		public bool m_useLocalizationFallback;

		public AdventureLogoCallbackData(AdventureLogoRenderData renderData, AdventureDbId adventureDbId, bool useLocalizationFallback)
		{
			m_renderData = renderData;
			m_adventureDbId = adventureDbId;
			m_useLocalizationFallback = useLocalizationFallback;
		}
	}

	private const AdventureDbId FIRST_ADVENTURE = AdventureDbId.NAXXRAMAS;

	[SerializeField]
	[Tooltip("This is the adventure displayed by default. INVALID means nothing will be displayed.")]
	private AdventureDbId m_defaultAdventure = AdventureDbId.NAXXRAMAS;

	[SerializeField]
	[Tooltip("Try to load the localized version of this texture first.")]
	private bool m_localizeTexture = true;

	[Tooltip("Display a shadow.")]
	[SerializeField]
	private bool m_useShadow = true;

	[Tooltip("Logo Material to texture with.")]
	[SerializeField]
	private Material m_logoBaseMaterial;

	[SerializeField]
	[Tooltip("Shadow Material to texture with.")]
	private Material m_shadowBaseMaterial;

	private AdventureLogoRenderData m_logoRenderData = new AdventureLogoRenderData();

	private AdventureLogoRenderData m_shadowRenderData = new AdventureLogoRenderData();

	private AdventureDbId m_displayedAdventure;

	private AdventureDbId m_desiredAdventure;

	private bool m_isUsingShadow;

	[Overridable]
	public long AdventureID
	{
		get
		{
			return (long)((m_desiredAdventure != 0) ? m_desiredAdventure : m_defaultAdventure);
		}
		set
		{
			m_desiredAdventure = (AdventureDbId)value;
		}
	}

	[Overridable]
	public bool ShowShadow
	{
		get
		{
			return m_useShadow;
		}
		set
		{
			m_useShadow = value;
		}
	}

	[Overridable]
	public float ShadowOffset_X
	{
		get
		{
			if (!(m_shadowRenderData.m_renderer != null))
			{
				return 0f;
			}
			return m_shadowRenderData.m_renderer.transform.localPosition.x;
		}
		set
		{
			if (m_shadowRenderData.m_renderer != null)
			{
				Vector3 position = m_shadowRenderData.m_renderer.transform.localPosition;
				position.x = value;
				m_shadowRenderData.m_renderer.transform.localPosition = position;
			}
		}
	}

	[Overridable]
	public float ShadowOffset_Y
	{
		get
		{
			if (!(m_shadowRenderData.m_renderer != null))
			{
				return 0f;
			}
			return m_shadowRenderData.m_renderer.transform.localPosition.y;
		}
		set
		{
			if (m_shadowRenderData.m_renderer != null)
			{
				Vector3 position = m_shadowRenderData.m_renderer.transform.localPosition;
				position.y = value;
				m_shadowRenderData.m_renderer.transform.localPosition = position;
			}
		}
	}

	[Overridable]
	public float ShadowOffset_Z
	{
		get
		{
			if (!(m_shadowRenderData.m_renderer != null))
			{
				return 0f;
			}
			return m_shadowRenderData.m_renderer.transform.localPosition.z;
		}
		set
		{
			if (m_shadowRenderData.m_renderer != null)
			{
				Vector3 position = m_shadowRenderData.m_renderer.transform.localPosition;
				position.z = value;
				m_shadowRenderData.m_renderer.transform.localPosition = position;
			}
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		m_displayedAdventure = AdventureDbId.INVALID;
		if (m_logoRenderData.m_renderer == null)
		{
			m_logoRenderData.m_renderer = CreateRenderObject("logo");
		}
		if (m_logoBaseMaterial == null)
		{
			m_logoBaseMaterial = m_logoRenderData.m_renderer.GetSharedMaterial() ?? m_logoRenderData.m_renderer.GetMaterial();
		}
		m_logoRenderData.m_baseMaterial = m_logoBaseMaterial;
		if (m_shadowRenderData.m_renderer == null)
		{
			m_shadowRenderData.m_renderer = CreateRenderObject("shadow");
		}
		if (m_shadowBaseMaterial == null)
		{
			m_shadowBaseMaterial = m_shadowRenderData.m_renderer.GetSharedMaterial() ?? m_shadowRenderData.m_renderer.GetMaterial();
		}
		m_shadowRenderData.m_baseMaterial = m_shadowBaseMaterial;
		CreatePreviewableObject();
	}

	private void CreatePreviewableObject()
	{
		CreatePreviewableObject(delegate(IPreviewableObject previewable, Action<GameObject> callback)
		{
			m_displayedAdventure = AdventureDbId.INVALID;
			m_isUsingShadow = false;
			m_logoRenderData.m_renderer.enabled = false;
			m_shadowRenderData.m_renderer.enabled = false;
			if (AssetLoader.Get() == null)
			{
				Debug.LogWarning("Hearthstone.UI.AdventureLogo.OnInitialize() - AssetLoader not available");
				callback(null);
			}
			else if (m_logoBaseMaterial == null)
			{
				Debug.LogWarning("Hearthstone.UI.AdventureLogo.OnInitialize() - No material found");
				callback(null);
			}
			else
			{
				m_displayedAdventure = (AdventureDbId)AdventureID;
				if (m_displayedAdventure != 0)
				{
					using (AssetHandle<GameObject> assetHandle = ShopUtils.LoadStoreAdventurePrefab(m_displayedAdventure))
					{
						StoreAdventureDef storeAdventureDef = (assetHandle ? assetHandle.Asset.GetComponent<StoreAdventureDef>() : null);
						if (storeAdventureDef == null)
						{
							callback(null);
						}
						else
						{
							string textureName = storeAdventureDef.GetLogoTextureName();
							if (string.IsNullOrEmpty(textureName))
							{
								Debug.LogWarning("Hearthstone.UI.AdventureLogo.OnInitialize() - No logo texture defined");
								callback(null);
							}
							else
							{
								AssetHandleCallback<Texture> onTextureLoaded = null;
								onTextureLoaded = delegate(AssetReference name, AssetHandle<Texture> loadedTexture, object data)
								{
									if (!(data is AdventureLogoCallbackData adventureLogoCallbackData))
									{
										Debug.LogError($"Invalid callback data provided for {base.name}. Must be AdventureLogoCallbackData!");
										loadedTexture?.Dispose();
									}
									else if (adventureLogoCallbackData.m_adventureDbId != m_displayedAdventure)
									{
										loadedTexture?.Dispose();
									}
									else if (!loadedTexture)
									{
										if (adventureLogoCallbackData.m_useLocalizationFallback)
										{
											Error.AddDevFatal("Loading localized logo failed. This is normal if we're on android and just switched. Trying unlocalized.");
											AdventureLogoCallbackData callbackData = new AdventureLogoCallbackData(adventureLogoCallbackData.m_renderData, adventureLogoCallbackData.m_adventureDbId, useLocalizationFallback: true);
											if (!AssetLoader.Get().LoadAsset(textureName, onTextureLoaded, callbackData, AssetLoadingOptions.DisableLocalization))
											{
												onTextureLoaded(textureName, null, callbackData);
											}
										}
										else
										{
											Debug.LogError($"Failed to load texture {base.name} for {adventureLogoCallbackData.m_renderData.m_renderer.name}!");
										}
									}
									else
									{
										AdventureLogoRenderData renderData = adventureLogoCallbackData.m_renderData;
										Renderer renderer = renderData.m_renderer;
										Material baseMaterial = renderData.m_baseMaterial;
										renderer.enabled = true;
										ServiceManager.Get<DisposablesCleaner>()?.Attach(renderer, loadedTexture);
										renderData.m_material = new Material(baseMaterial);
										renderData.m_material.mainTexture = loadedTexture;
										renderData.m_renderer.SetMaterial(0, renderData.m_material);
									}
								};
								AdventureLogoCallbackData callbackData2 = new AdventureLogoCallbackData(m_logoRenderData, m_displayedAdventure, useLocalizationFallback: true);
								if (!AssetLoader.Get().LoadAsset(textureName, onTextureLoaded, callbackData2, (!m_localizeTexture) ? AssetLoadingOptions.DisableLocalization : AssetLoadingOptions.None))
								{
									onTextureLoaded(textureName, null, callbackData2);
								}
								if (m_useShadow)
								{
									string logoShadowTextureName = storeAdventureDef.GetLogoShadowTextureName();
									if (string.IsNullOrEmpty(logoShadowTextureName))
									{
										Debug.LogWarning("Hearthstone.UI.AdventureLogo.OnInitialize() - No logo SHADOW texture defined");
									}
									else
									{
										AdventureLogoCallbackData callbackData3 = new AdventureLogoCallbackData(m_shadowRenderData, m_displayedAdventure, useLocalizationFallback: false);
										if (!AssetLoader.Get().LoadAsset(logoShadowTextureName, onTextureLoaded, callbackData3))
										{
											onTextureLoaded(textureName, null, callbackData3);
										}
										m_isUsingShadow = true;
									}
								}
								callback(null);
							}
						}
						return;
					}
				}
				callback(null);
			}
		}, (IPreviewableObject o) => m_displayedAdventure != (AdventureDbId)AdventureID || m_useShadow != m_isUsingShadow);
	}

	private Renderer CreateRenderObject(string suffix)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
		go.transform.SetParent(base.transform, worldPositionStays: false);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		Transform parent = go.transform.parent;
		if (parent != null)
		{
			go.layer = parent.gameObject.layer;
		}
		go.name = $"{base.name}_{suffix}(Dynamic)";
		go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		if (go.GetComponent<OwnedByWidgetBehavior>() == null)
		{
			go.AddComponent<OwnedByWidgetBehavior>().Owner = this;
		}
		MeshRenderer component = go.GetComponent<MeshRenderer>();
		component.enabled = false;
		return component;
	}
}
