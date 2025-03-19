using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

public class CustomFrameController : IDisposable
{
	private readonly MeshRenderer m_meshRenderer;

	private readonly int m_originalMatIdx;

	private readonly int m_originalFrameMatIdx;

	private GameObject m_customFrameObject;

	private GameObject m_diamondFrameObject;

	private AssetHandle<GameObject> m_frameHandle;

	private CustomFrameDef m_frameDef;

	private HighlightState m_highlightState;

	private HighlightRender m_highlightRender;

	private Texture2D m_originalHighlightSilouetteTexture;

	private Vector3 m_originalHighlightPosition;

	private Vector3 m_originalHighlightScale;

	private Material m_originalPortraitMaterial;

	private Vector3 m_originalAttackPosition;

	private Vector3 m_originalHealthPosition;

	private Vector3 m_originalArmorPosition;

	private static readonly int LightingBlendPropertyID = Shader.PropertyToID("_LightingBlend");

	public AssetReference FrameAssetReference { get; private set; }

	public LegendarySkinDynamicResController DynamicResolutionController { get; private set; }

	public int PortraitMatIdx
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return -1;
			}
			return m_frameDef.PortraitMatIdx;
		}
	}

	public int FrameMatIdx
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return -1;
			}
			return m_frameDef.FrameMatIdx;
		}
	}

	public float DecorationRootOffset
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return 0f;
			}
			return m_frameDef.DecorationRootOffset;
		}
	}

	public float HeroZonePositionOffset
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return 0f;
			}
			return m_frameDef.HeroZonePositionOffset;
		}
	}

	public float RaiseAndLowerLimit
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return 0f;
			}
			return m_frameDef.HeroPickerRaiseAndLowerLimit;
		}
	}

	public float HeroClassIconOffset
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return 0f;
			}
			return m_frameDef.HeroClassIconOffset;
		}
	}

	public float HeroPowerContainerOffset
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return 0f;
			}
			return m_frameDef.HeroPowerContainerOffset;
		}
	}

	public Material MissingDiamondPortraitMat
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return null;
			}
			return m_frameDef.MissingDiamondPortraitMat;
		}
	}

	public bool UseMetaCalibration
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return false;
			}
			return m_frameDef.UseMetaCalibration;
		}
	}

	public Vector3 DeckPickerXPBarPosition
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaDeckPickerXPBarPosition;
		}
	}

	public Vector3 DeckPickerXPBarScale
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaDeckPickerXPBarScale;
		}
	}

	public Vector3 CardRewardScalePC
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaRewardsHeroPortraitScalePC;
		}
	}

	public Vector3 CardRewardScalePhone
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaRewardsHeroPortraitScalePhone;
		}
	}

	public Vector3 EndGameXPBarScale
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaEndGameXPBarScale;
		}
	}

	public Vector3 EndGameVictoryBarPosition
	{
		get
		{
			if (!(m_frameDef != null))
			{
				return Vector3.zero;
			}
			return m_frameDef.MetaEndGameXPBarVictoryPosition;
		}
	}

	public CustomFrameController(GameObject frameObject)
		: this(frameObject, -1, -1)
	{
	}

	public CustomFrameController(GameObject frameObject, int matIdx, int frameMatIdx)
	{
		m_meshRenderer = frameObject.GetComponent<MeshRenderer>();
		m_originalMatIdx = matIdx;
		m_originalFrameMatIdx = frameMatIdx;
	}

	public void SetAssetHandle(AssetReference reference, AssetHandle<GameObject> handle)
	{
		FrameAssetReference = reference;
		AssetHandle.Set(ref m_frameHandle, handle);
		if ((bool)m_frameHandle && (bool)m_frameHandle.Asset)
		{
			m_frameDef = m_frameHandle.Asset.GetComponent<CustomFrameDef>();
		}
		else
		{
			m_frameDef = null;
		}
	}

	public void CacheHighlightState(HighlightState highlightState)
	{
		if (!(m_highlightState == null))
		{
			return;
		}
		m_highlightState = highlightState;
		m_highlightRender = null;
		if (highlightState != null)
		{
			m_originalHighlightSilouetteTexture = highlightState.m_StaticSilouetteTexture;
			m_highlightRender = highlightState.GetComponentInChildren<HighlightRender>();
			if (m_highlightRender != null)
			{
				m_originalHighlightPosition = m_highlightRender.transform.localPosition;
				m_originalHighlightScale = m_highlightRender.transform.localScale;
			}
		}
	}

	public void CacheInitialPortraitMaterial(Material material)
	{
		if (m_originalPortraitMaterial == null)
		{
			m_originalPortraitMaterial = material;
		}
	}

	public void RestoreInitialPortraitMaterial(ref Material material)
	{
		if (material != null)
		{
			material = m_originalPortraitMaterial;
		}
	}

	public void CacheInitialStatsPositions(GameObject attackObject, GameObject healthObject, GameObject armorSpellBoneObject)
	{
		if (attackObject != null)
		{
			m_originalAttackPosition = attackObject.transform.localPosition;
		}
		if (healthObject != null)
		{
			m_originalHealthPosition = healthObject.transform.localPosition;
		}
		if (armorSpellBoneObject != null)
		{
			m_originalArmorPosition = armorSpellBoneObject.transform.localPosition;
		}
	}

	public void RestoreInitialStatsPositions(ref GameObject attackObject, ref GameObject healthObject, ref GameObject armorSpellBoneObject)
	{
		if (attackObject != null)
		{
			attackObject.transform.localPosition = m_originalAttackPosition;
		}
		if (healthObject != null)
		{
			healthObject.transform.localPosition = m_originalHealthPosition;
		}
		if (armorSpellBoneObject != null)
		{
			armorSpellBoneObject.transform.localPosition = m_originalArmorPosition;
		}
	}

	private void CopyCustomFrameSubObjects()
	{
		for (int i = 0; i < m_frameHandle.Asset.transform.childCount; i++)
		{
			UnityEngine.Object.Instantiate(m_frameHandle.Asset.transform.GetChild(i), m_meshRenderer.transform).gameObject.SetActive(value: true);
		}
	}

	public void ApplyCustomMeshAndMaterials(out GameObject frameObject)
	{
		if (m_meshRenderer != null && m_meshRenderer.gameObject.name == "Custom Frame")
		{
			m_customFrameObject = m_meshRenderer.gameObject;
			frameObject = m_customFrameObject;
			return;
		}
		if (m_customFrameObject == null)
		{
			m_customFrameObject = new GameObject("Custom Frame", typeof(LegendarySkinDynamicResController));
		}
		Renderer customFrameMeshRenderer = null;
		if (m_frameDef.m_SkinnedFrameRoot == null)
		{
			customFrameMeshRenderer = m_customFrameObject.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = m_customFrameObject.AddComponent<MeshFilter>();
			if (meshFilter != null)
			{
				meshFilter.sharedMesh = m_frameDef.FrameMesh;
			}
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_frameDef.m_SkinnedFrameRoot, m_customFrameObject.transform);
			customFrameMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			Animator animator = gameObject.GetComponent<Animator>();
			if (animator != null)
			{
				animator.runtimeAnimatorController = m_frameDef.m_FrameAnimator.runtimeAnimatorController;
				animator.avatar = m_frameDef.m_FrameAnimator.avatar;
				animator.applyRootMotion = m_frameDef.m_FrameAnimator.applyRootMotion;
				animator.updateMode = m_frameDef.m_FrameAnimator.updateMode;
				animator.cullingMode = m_frameDef.m_FrameAnimator.cullingMode;
				animator.keepAnimatorStateOnDisable = m_frameDef.m_FrameAnimator.keepAnimatorStateOnDisable;
			}
		}
		Material[] materials = new Material[2];
		if (m_frameDef.PortraitMat != null && (uint)m_frameDef.PortraitMatIdx < 2u)
		{
			materials[m_frameDef.PortraitMatIdx] = UnityEngine.Object.Instantiate(m_frameDef.PortraitMat);
		}
		if (m_frameDef.FrameMat != null && (uint)m_frameDef.FrameMatIdx < 2u)
		{
			materials[m_frameDef.FrameMatIdx] = m_frameDef.FrameMat;
		}
		LegendarySkinDynamicResController resController = m_customFrameObject.GetComponent<LegendarySkinDynamicResController>();
		resController.CacheMaterialProperties(m_frameDef.PortraitMat);
		resController.Renderer = customFrameMeshRenderer;
		resController.MaterialIdx = m_frameDef.PortraitMatIdx;
		DynamicResolutionController = resController;
		customFrameMeshRenderer.SetSharedMaterials(materials);
		m_customFrameObject.transform.SetParent(m_meshRenderer.gameObject.transform);
		m_customFrameObject.transform.localPosition = new Vector3(0f, m_frameDef.AvoidShadowPlaneOffset, 0f);
		m_customFrameObject.transform.localRotation = Quaternion.identity;
		m_customFrameObject.transform.localScale = Vector3.one;
		m_customFrameObject.layer = m_meshRenderer.gameObject.layer;
		m_meshRenderer.enabled = false;
		if (m_highlightState != null)
		{
			if (m_originalHighlightSilouetteTexture != null)
			{
				m_highlightState.m_StaticSilouetteTexture = m_frameDef.Silhouette;
			}
			if (m_highlightRender != null)
			{
				HighlightRenderOverrides overrides = null;
				if (m_highlightState.GetComponentInParent<CollectionCardVisual>() != null)
				{
					overrides = m_frameDef.CollectionOverrides;
				}
				else
				{
					switch (m_highlightState.m_highlightType)
					{
					case HighlightStateType.CARD:
						overrides = m_frameDef.CardOverrides;
						break;
					case HighlightStateType.HIGHLIGHT:
						overrides = m_frameDef.HighlightOverrides;
						break;
					case HighlightStateType.CARD_PLAY:
						overrides = m_frameDef.CardPlayOverrides ?? m_frameDef.CardOverrides;
						break;
					}
				}
				m_highlightRender.SetRenderOverrides(overrides);
				if (overrides != null)
				{
					if (overrides.OverrideTransform)
					{
						m_highlightRender.transform.localPosition = overrides.Position;
						m_highlightRender.transform.localScale = Vector3.one * overrides.Scale;
					}
					else
					{
						m_highlightRender.transform.localPosition = m_originalHighlightPosition;
						m_highlightRender.transform.localScale = m_originalHighlightScale;
					}
				}
			}
			m_highlightState.ForceUpdate();
		}
		PopupRenderer popupRenderer = m_meshRenderer.GetComponent<PopupRenderer>();
		if ((bool)popupRenderer && (bool)popupRenderer.PopupRoot)
		{
			popupRenderer.PopupRoot.GetOrCreatePopupRenderer(m_customFrameObject);
		}
		CopyCustomFrameSubObjects();
		m_customFrameObject.AddComponent<HighlightOverrideSilhouetteMeshParent>();
		frameObject = customFrameMeshRenderer.gameObject;
		if (!(m_frameDef.ExtraDiamondPrefab == null) && m_diamondFrameObject == null && m_customFrameObject != null)
		{
			m_diamondFrameObject = UnityEngine.Object.Instantiate(m_frameDef.ExtraDiamondPrefab, m_customFrameObject.transform);
			LayerUtils.SetLayer(m_diamondFrameObject, m_meshRenderer.gameObject.layer, null);
		}
	}

	public void ApplyStatPositionOffsets(GameObject attackObject, GameObject healthObject, GameObject armorSpellBoneObject)
	{
		if (!(m_frameDef == null))
		{
			if (attackObject != null)
			{
				attackObject.transform.localPosition += m_frameDef.AttackOffset;
			}
			if (healthObject != null)
			{
				healthObject.transform.localPosition += m_frameDef.HealthOffset;
			}
			if (armorSpellBoneObject != null)
			{
				armorSpellBoneObject.transform.localPosition += m_frameDef.ArmorOffset;
			}
		}
	}

	public void ApplyMetaFrameOffsets(Transform transform)
	{
		if (transform == null || m_frameDef == null || !m_frameDef.UseMetaCalibration)
		{
			return;
		}
		SceneMgr.Mode? mode = SceneMgr.Get()?.GetMode();
		if (mode.HasValue && mode != SceneMgr.Mode.GAMEPLAY)
		{
			if (mode == SceneMgr.Mode.COLLECTIONMANAGER || mode == SceneMgr.Mode.BACON_COLLECTION || mode == SceneMgr.Mode.LETTUCE_COLLECTION)
			{
				transform.localScale = m_frameDef.MetaCollectionHeroPortraitScale;
				transform.localPosition += m_frameDef.MetaCollectionPositionOffset;
			}
			else if (mode == SceneMgr.Mode.TOURNAMENT || mode == SceneMgr.Mode.ADVENTURE)
			{
				transform.localScale = m_frameDef.MetaDeckPickerHeroPortraitScale;
				transform.localPosition += m_frameDef.MetaDeckPickerPositionOffset;
			}
		}
	}

	public void UpdateCustomDiamondMaterial()
	{
		if (m_diamondFrameObject == null || m_customFrameObject == null)
		{
			return;
		}
		CustomFrameDiamondPrefab diamondPrefab = m_diamondFrameObject.GetComponent<CustomFrameDiamondPrefab>();
		if (diamondPrefab == null)
		{
			Debug.LogWarning(m_diamondFrameObject.name + " is missing a CustomFrameDiamondPrefab component");
			return;
		}
		GameObject diamondRTT = diamondPrefab.PortraitRTT;
		if (diamondRTT == null)
		{
			return;
		}
		MeshRenderer meshRenderer = diamondRTT.GetComponent<MeshRenderer>();
		if (!(meshRenderer == null))
		{
			MeshRenderer portraitRenderer = m_customFrameObject.GetComponent<MeshRenderer>();
			if (!(portraitRenderer == null))
			{
				meshRenderer.SetSharedMaterial(portraitRenderer.GetSharedMaterial(m_frameDef.PortraitMatIdx));
			}
		}
	}

	public CustomFrameDiamondPrefab GetDiamondFramePrefab()
	{
		if (m_frameDef.ExtraDiamondPrefab == null)
		{
			return null;
		}
		if (m_diamondFrameObject == null)
		{
			return null;
		}
		CustomFrameDiamondPrefab diamondPrefab = m_diamondFrameObject.GetComponent<CustomFrameDiamondPrefab>();
		if (diamondPrefab == null)
		{
			Debug.LogWarning(m_diamondFrameObject.name + " is missing a CustomFrameDiamondPrefab component");
			return null;
		}
		return diamondPrefab;
	}

	public void ToggleDiamondRTTVisibility(bool active)
	{
		CustomFrameDiamondPrefab diamondFramePrefab = GetDiamondFramePrefab();
		if (!(diamondFramePrefab == null) && !(diamondFramePrefab.PortraitRTT == null))
		{
			diamondFramePrefab.PortraitRTT.SetActive(active);
		}
	}

	public void SetLightingBlendOnSubRenderers(float lightingBlend)
	{
		Renderer[] componentsInChildren = m_meshRenderer.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer == null)
			{
				continue;
			}
			List<Material> materials = new List<Material>();
			renderer.GetMaterials(materials);
			foreach (Material mat in materials)
			{
				if (mat.HasFloat(LightingBlendPropertyID))
				{
					mat.SetFloat(LightingBlendPropertyID, lightingBlend);
				}
			}
		}
	}

	public void RestoreMeshAndMaterials(ref GameObject frameObject)
	{
		if (m_meshRenderer != null)
		{
			m_meshRenderer.enabled = true;
		}
		if (m_customFrameObject != null)
		{
			UnityEngine.Object.Destroy(m_customFrameObject);
			m_customFrameObject = null;
		}
		if (m_diamondFrameObject != null)
		{
			UnityEngine.Object.Destroy(m_diamondFrameObject);
			m_diamondFrameObject = null;
		}
		if (m_highlightState != null)
		{
			m_highlightState.m_StaticSilouetteTexture = m_originalHighlightSilouetteTexture;
			if (m_highlightRender != null)
			{
				m_highlightRender.SetRenderOverrides(null);
				m_highlightRender.transform.localPosition = m_originalHighlightPosition;
				m_highlightRender.transform.localScale = m_originalHighlightScale;
			}
			m_highlightState.ForceUpdate();
		}
		if (m_meshRenderer != null)
		{
			frameObject = m_meshRenderer.gameObject;
		}
	}

	public void RestoreMeshAndMaterials(ref GameObject frameObject, ref int matIdx, ref int frameMatIdx)
	{
		RestoreMeshAndMaterials(ref frameObject);
		matIdx = m_originalMatIdx;
		frameMatIdx = m_originalFrameMatIdx;
	}

	public void ApplyCustomFrameNameShadowTexture(GameObject nameShadow)
	{
		if (!(m_frameDef.m_CustomNameShadowTexture == null))
		{
			if (nameShadow.TryGetComponent<MeshRenderer>(out var meshRenderer))
			{
				meshRenderer.GetMaterial().SetTexture("_MainTex", m_frameDef.m_CustomNameShadowTexture);
			}
			Transform nameShadowParentTransform = nameShadow.transform.parent;
			if (nameShadowParentTransform != null)
			{
				nameShadowParentTransform.localPosition += m_frameDef.m_CustomNameShadowOffset;
			}
		}
	}

	public Vector3 GetCustomFrameSecretZoneOffset(int index)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			if (index < m_frameDef.SecretZoneOffsetsMobile.Count)
			{
				return m_frameDef.SecretZoneOffsetsMobile[index];
			}
		}
		else if (index < m_frameDef.SecretZoneOffsetsPc.Count)
		{
			return m_frameDef.SecretZoneOffsetsPc[index];
		}
		return Vector3.zero;
	}

	public float GetCustomFrameSecretZoneScale()
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			return m_frameDef.SecretZoneScaleMobile;
		}
		return m_frameDef.SecretZoneScalePc;
	}

	public Vector3 GetCustomFrameMulliganHeroNameOffset(Player.Side side)
	{
		if (side == Player.Side.FRIENDLY)
		{
			return m_frameDef.MulliganHeroNameOffsetFriendly;
		}
		return m_frameDef.MulliganHeroNameOffsetEnemy;
	}

	public Vector3 GetCustomFrameMulliganHeroOffset(Player.Side side)
	{
		if (side == Player.Side.FRIENDLY)
		{
			return m_frameDef.MulliganHeroOffsetFriendly;
		}
		return m_frameDef.MulliganHeroOffsetEnemy;
	}

	void IDisposable.Dispose()
	{
		m_frameDef = null;
		AssetHandle.SafeDispose(ref m_frameHandle);
	}
}
