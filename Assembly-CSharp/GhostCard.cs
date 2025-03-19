using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Unity.Profiling;
using UnityEngine;

public class GhostCard : MonoBehaviour
{
	public enum Type
	{
		NONE,
		MISSING_UNCRAFTABLE,
		MISSING,
		NOT_VALID,
		DORMANT,
		PURCHASABLE_HERO_SKIN
	}

	public Actor m_Actor;

	public Vector3 m_CardOffset = Vector3.zero;

	public RenderToTexture m_R2T_EffectGhost;

	public GameObject m_EffectRoot;

	public GameObject m_GlowPlane;

	public GameObject m_GlowPlaneElite;

	public bool m_shouldShowText = true;

	private static GhostStyleDef s_ghostStyles;

	private static IMaterialService s_materialService;

	private static ProfilerMarker s_RenderGhost = new ProfilerMarker("RenderGhost");

	private static ProfilerMarker s_RenderGhostInit = new ProfilerMarker("RenderGhost_Init");

	private static ProfilerMarker s_RenderGhostStoreOrgMaterials = new ProfilerMarker("RenderGhost_StoreOrgMaterials");

	private static ProfilerMarker s_RenderGhostRestoreOrgMaterials = new ProfilerMarker("RenderGhost_RestoreOrgMaterials");

	private static ProfilerMarker s_RenderGhostApplyGhostMaterials = new ProfilerMarker("RenderGhost_ApplyGhostMaterials");

	private static ProfilerMarker s_RenderGhostSetupRTTOverrides = new ProfilerMarker("RenderGhost_SetupRTTOverrides");

	private bool m_isBigCard;

	private bool m_Init;

	private RenderToTexture m_R2T_BaseCard;

	private bool m_R2T_BaseCard_OrigHideRenderObject;

	private Type m_ghostType;

	private TAG_PREMIUM m_ghostPremium;

	public int m_renderQueue;

	private GameObject m_CardMesh;

	private int m_CardFrontIdx;

	private int m_PremiumRibbonIdx = -1;

	private GameObject m_PortraitMesh;

	private int m_PortraitFrameIdx;

	private GameObject m_NameMesh;

	private GameObject m_DescriptionMesh;

	private GameObject m_DescriptionTrimMesh;

	private GameObject m_RarityFrameMesh;

	private GameObject m_ManaCostMesh;

	private GameObject m_AttackMesh;

	private GameObject m_HealthMesh;

	private GameObject m_RacePlateMesh;

	private GameObject m_MultiRacePlateMesh;

	private GameObject m_EliteMesh;

	private GameObject m_mercenaryLevelMesh;

	private bool m_hasOriginalMaterialsStored;

	private Material m_OrgMat_CardFront;

	private Material m_OrgMat_PremiumRibbon;

	private Material m_OrgMat_PortraitFrame;

	private Material m_OrgMat_Name;

	private Material m_OrgMat_Description;

	private Material m_OrgMat_Description2;

	private Material m_OrgMat_DescriptionTrim;

	private Material m_OrgMat_RarityFrame;

	private Material m_OrgMat_ManaCost;

	private Material m_OrgMat_Attack;

	private Material m_OrgMat_Health;

	private Material m_OrgMat_RacePlate;

	private Material m_OrgMat_MultiRacePlate;

	private Material m_OrgMat_Elite;

	private Material m_OrgMat_mercenaryLevel;

	private RenderCommandLists.MatOverrideDictionary m_rttMatOverrides;

	public static Type GetGhostTypeFromSlot(CollectionDeck deck, CollectionDeckSlot slot)
	{
		if (deck == null || slot == null)
		{
			return Type.NONE;
		}
		return deck.GetSlotStatus(slot) switch
		{
			CollectionDeck.SlotStatus.MISSING => Type.MISSING, 
			CollectionDeck.SlotStatus.NOT_VALID => Type.NOT_VALID, 
			_ => Type.NONE, 
		};
	}

	private void Awake()
	{
		m_R2T_BaseCard = GetComponent<RenderToTexture>();
		m_R2T_BaseCard_OrigHideRenderObject = m_R2T_BaseCard.m_HideRenderObject;
		if (s_ghostStyles == null && AssetLoader.Get() != null)
		{
			s_ghostStyles = AssetLoader.Get().InstantiatePrefab("GhostStyleDef.prefab:932fbc50238e04673aeb0f59c9cfaed1").GetComponent<GhostStyleDef>();
		}
	}

	private void OnDisable()
	{
		Disable();
	}

	private void OnDestroy()
	{
		DropMaterialReferences();
		DestroyRTTOverrides();
		if ((bool)m_EffectRoot)
		{
			ParticleSystem particle = m_EffectRoot.GetComponentInChildren<ParticleSystem>();
			if ((bool)particle)
			{
				particle.Stop();
			}
		}
	}

	public void SetBigCard(bool isBigCard)
	{
		m_isBigCard = isBigCard;
	}

	public void SetGhostType(Type ghostType)
	{
		if (m_ghostType != ghostType && (ghostType == Type.DORMANT || m_ghostType == Type.DORMANT))
		{
			Reset();
		}
		m_ghostType = ghostType;
	}

	public void SetPremium(TAG_PREMIUM premium)
	{
		m_ghostPremium = premium;
	}

	public void SetRenderQueue(int renderQueue)
	{
		m_renderQueue = renderQueue;
	}

	public void SetRTTDirty()
	{
		if (m_R2T_BaseCard != null)
		{
			m_R2T_BaseCard.SetDirty();
		}
	}

	public void RenderGhostCard()
	{
		RenderGhostCard(forceRender: false);
	}

	public void RenderGhostCard(bool forceRender)
	{
		RenderGhost(forceRender);
	}

	public void Reset()
	{
		m_Init = false;
	}

	private void RenderGhost()
	{
		RenderGhost(forceRender: false);
	}

	private void RenderGhost(bool forceRender)
	{
		bool shouldUseRenderToTexture = m_ghostType != Type.DORMANT;
		Init(forceRender, shouldUseRenderToTexture);
		if (shouldUseRenderToTexture)
		{
			m_R2T_BaseCard.enabled = true;
			m_R2T_BaseCard.m_HideRenderObject = m_R2T_BaseCard_OrigHideRenderObject;
		}
		else
		{
			m_R2T_BaseCard.enabled = false;
			m_R2T_BaseCard.m_HideRenderObject = false;
		}
		m_R2T_BaseCard.m_RenderQueue = m_renderQueue;
		if ((bool)m_R2T_EffectGhost)
		{
			m_R2T_EffectGhost.enabled = true;
			m_R2T_EffectGhost.m_RenderQueue = m_renderQueue;
		}
		m_Actor.m_ghostCardActive = true;
		m_R2T_BaseCard.m_ObjectToRender = m_Actor.GetRootObject();
		m_Actor.GetRootObject().transform.localPosition = m_CardOffset;
		if (m_shouldShowText)
		{
			m_Actor.ShowAllText();
		}
		if (shouldUseRenderToTexture)
		{
			SetupRTTOverrides();
			m_R2T_BaseCard.SetMaterialOverrides(m_rttMatOverrides);
			m_R2T_BaseCard.RenderNow();
		}
		else
		{
			ApplyGhostMaterials();
		}
		Renderer glowPlaneRenderer = null;
		if ((bool)m_GlowPlane)
		{
			glowPlaneRenderer = m_GlowPlane.GetComponent<Renderer>();
			glowPlaneRenderer.enabled = false;
		}
		Renderer glowPlaneEliteRenderer = null;
		if ((bool)m_GlowPlaneElite)
		{
			glowPlaneEliteRenderer = m_GlowPlaneElite.GetComponent<Renderer>();
			glowPlaneEliteRenderer.enabled = false;
		}
		if ((bool)glowPlaneRenderer && !m_Actor.IsElite())
		{
			glowPlaneRenderer.enabled = true;
			glowPlaneRenderer.GetMaterial().renderQueue = 3000 + GetGlowPlaneRenderOrderAdjustment();
			glowPlaneRenderer.sortingOrder = GetGlowPlaneRenderOrderAdjustment();
		}
		if ((bool)glowPlaneEliteRenderer && m_Actor.IsElite())
		{
			glowPlaneEliteRenderer.enabled = true;
			glowPlaneEliteRenderer.GetMaterial().renderQueue = 3000 + GetGlowPlaneRenderOrderAdjustment();
			glowPlaneEliteRenderer.sortingOrder = GetGlowPlaneRenderOrderAdjustment();
		}
		if (!m_EffectRoot)
		{
			return;
		}
		m_EffectRoot.transform.parent = null;
		m_EffectRoot.transform.position = new Vector3(-500f, -500f, -500f);
		m_EffectRoot.transform.localScale = Vector3.one;
		if ((bool)m_R2T_EffectGhost)
		{
			m_R2T_EffectGhost.enabled = true;
			RenderTexture effectTexture = m_R2T_EffectGhost.RenderNow();
			if (effectTexture != null)
			{
				m_R2T_BaseCard.GetRenderMaterial().SetTexture("_FxTex", effectTexture);
			}
		}
		ParticleSystem particle = m_EffectRoot.GetComponentInChildren<ParticleSystem>();
		if ((bool)particle)
		{
			Renderer particleRenderer = particle.GetComponent<Renderer>();
			if ((bool)particleRenderer)
			{
				particleRenderer.enabled = true;
			}
			particle.Play();
		}
	}

	private int GetGlowPlaneRenderOrderAdjustment()
	{
		if (m_ghostType == Type.DORMANT)
		{
			return 51;
		}
		return m_renderQueue + 1;
	}

	public void ShowRenderers()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in componentsInChildren)
		{
			bool isEnabled = true;
			if (m_GlowPlane != m_GlowPlaneElite)
			{
				if (m_Actor.IsElite() && renderer.gameObject == m_GlowPlane)
				{
					isEnabled = false;
				}
				else if (!m_Actor.IsElite() && renderer.gameObject == m_GlowPlaneElite)
				{
					isEnabled = false;
				}
			}
			renderer.enabled = isEnabled;
		}
	}

	public void DisableGhost()
	{
		Disable();
		base.enabled = false;
	}

	private void Init(bool forceRender, bool usingRenderToTexture)
	{
		if (m_Init && !forceRender)
		{
			return;
		}
		if (m_Actor == null)
		{
			m_Actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
			if (m_Actor == null)
			{
				Debug.LogError($"{base.transform.root.name} Ghost card effect failed to find Actor!");
				base.enabled = false;
				return;
			}
		}
		m_CardMesh = m_Actor.m_cardMesh;
		m_CardFrontIdx = m_Actor.m_cardFrontMatIdx;
		m_PremiumRibbonIdx = m_Actor.m_premiumRibbon;
		m_PortraitMesh = m_Actor.m_portraitMesh;
		m_PortraitFrameIdx = m_Actor.m_portraitFrameMatIdx;
		m_NameMesh = m_Actor.m_nameBannerMesh;
		m_DescriptionMesh = m_Actor.m_descriptionMesh;
		m_DescriptionTrimMesh = m_Actor.m_descriptionTrimMesh;
		m_RarityFrameMesh = m_Actor.m_rarityFrameMesh;
		if ((bool)m_Actor.m_attackObject)
		{
			Renderer actorMesh = m_Actor.m_attackObject.GetComponent<Renderer>();
			if (actorMesh != null)
			{
				m_AttackMesh = actorMesh.gameObject;
			}
			if (m_AttackMesh == null)
			{
				Renderer[] componentsInChildren = m_Actor.m_attackObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer mesh in componentsInChildren)
				{
					if (!mesh.GetComponent<UberText>())
					{
						m_AttackMesh = mesh.gameObject;
					}
				}
			}
		}
		if ((bool)m_Actor.m_healthObject)
		{
			Renderer healthMesh = m_Actor.m_healthObject.GetComponent<Renderer>();
			if (healthMesh != null)
			{
				m_HealthMesh = healthMesh.gameObject;
			}
			if (m_HealthMesh == null)
			{
				Renderer[] componentsInChildren = m_Actor.m_healthObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer mesh2 in componentsInChildren)
				{
					if (!mesh2.GetComponent<UberText>())
					{
						m_HealthMesh = mesh2.gameObject;
					}
				}
			}
		}
		m_ManaCostMesh = m_Actor.m_manaObject;
		m_MultiRacePlateMesh = m_Actor.m_multiRacePlateObject;
		m_RacePlateMesh = m_Actor.m_racePlateObject;
		m_EliteMesh = m_Actor.m_eliteObject;
		m_mercenaryLevelMesh = m_Actor.m_mercenaryLevelObject?.m_xpBarBacking;
		if (!usingRenderToTexture)
		{
			StoreOrgMaterials();
		}
		m_R2T_BaseCard.m_ObjectToRender = m_Actor.GetRootObject();
		if ((bool)m_R2T_BaseCard.m_Material && m_R2T_BaseCard.m_Material.HasProperty("_Seed"))
		{
			m_R2T_BaseCard.m_Material.SetFloat("_Seed", Random.Range(0f, 1f));
		}
		m_Init = true;
	}

	private void StoreOrgMaterials()
	{
		if (m_hasOriginalMaterialsStored)
		{
			return;
		}
		m_hasOriginalMaterialsStored = true;
		IMaterialService materialService = GetMaterialService();
		if ((bool)m_CardMesh)
		{
			if (m_CardFrontIdx > -1)
			{
				m_OrgMat_CardFront = m_CardMesh.GetComponent<Renderer>().GetMaterial(m_CardFrontIdx);
				materialService?.KeepMaterial(m_OrgMat_CardFront);
			}
			if (m_PremiumRibbonIdx > -1)
			{
				m_OrgMat_PremiumRibbon = m_CardMesh.GetComponent<Renderer>().GetMaterial(m_PremiumRibbonIdx);
				materialService?.KeepMaterial(m_OrgMat_PremiumRibbon);
			}
		}
		if ((bool)m_PortraitMesh && m_PortraitFrameIdx > -1)
		{
			m_OrgMat_PortraitFrame = m_PortraitMesh.GetComponent<Renderer>().GetMaterial(m_PortraitFrameIdx);
			materialService?.KeepMaterial(m_OrgMat_PortraitFrame);
		}
		if ((bool)m_NameMesh)
		{
			m_OrgMat_Name = m_NameMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_Name);
		}
		if ((bool)m_ManaCostMesh)
		{
			m_OrgMat_ManaCost = m_ManaCostMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_ManaCost);
		}
		if ((bool)m_AttackMesh)
		{
			m_OrgMat_Attack = m_AttackMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_Attack);
		}
		if ((bool)m_HealthMesh)
		{
			m_OrgMat_Health = m_HealthMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_Health);
		}
		if ((bool)m_RacePlateMesh)
		{
			m_OrgMat_RacePlate = m_RacePlateMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_RacePlate);
		}
		if ((bool)m_MultiRacePlateMesh)
		{
			m_OrgMat_MultiRacePlate = m_MultiRacePlateMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_MultiRacePlate);
		}
		if ((bool)m_mercenaryLevelMesh)
		{
			m_OrgMat_mercenaryLevel = m_RacePlateMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_mercenaryLevel);
		}
		if ((bool)m_RarityFrameMesh)
		{
			Renderer renderer = m_RarityFrameMesh.GetComponent<Renderer>();
			if (renderer != null)
			{
				m_OrgMat_RarityFrame = renderer.GetMaterial();
				materialService?.KeepMaterial(m_OrgMat_RarityFrame);
			}
		}
		if ((bool)m_DescriptionMesh && m_DescriptionMesh.GetComponent<Renderer>() != null)
		{
			List<Material> descMats = m_DescriptionMesh.GetComponent<Renderer>().GetMaterials();
			if (descMats.Count > 0)
			{
				m_OrgMat_Description = descMats[0];
				materialService?.KeepMaterial(m_OrgMat_Description);
				if (descMats.Count > 1)
				{
					m_OrgMat_Description2 = descMats[1];
					materialService?.KeepMaterial(m_OrgMat_Description2);
				}
			}
		}
		if ((bool)m_DescriptionTrimMesh)
		{
			m_OrgMat_DescriptionTrim = m_DescriptionTrimMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_DescriptionTrim);
		}
		if ((bool)m_EliteMesh)
		{
			m_OrgMat_Elite = m_EliteMesh.GetComponent<Renderer>().GetMaterial();
			materialService?.KeepMaterial(m_OrgMat_Elite);
		}
	}

	private void RestoreOrgMaterials()
	{
		if (m_hasOriginalMaterialsStored)
		{
			ApplyMaterialByIdx(m_CardMesh, m_OrgMat_CardFront, m_CardFrontIdx);
			ApplyMaterialByIdx(m_CardMesh, m_OrgMat_PremiumRibbon, m_PremiumRibbonIdx);
			ApplyMaterialByIdx(m_PortraitMesh, m_OrgMat_PortraitFrame, m_PortraitFrameIdx);
			ApplyMaterialByIdx(m_DescriptionMesh, m_OrgMat_Description, 0);
			ApplyMaterialByIdx(m_DescriptionMesh, m_OrgMat_Description2, 1);
			ApplyMaterial(m_NameMesh, m_OrgMat_Name);
			ApplyMaterial(m_ManaCostMesh, m_OrgMat_ManaCost);
			ApplyMaterial(m_AttackMesh, m_OrgMat_Attack);
			ApplyMaterial(m_HealthMesh, m_OrgMat_Health);
			ApplyMaterial(m_RacePlateMesh, m_OrgMat_RacePlate);
			ApplyMaterial(m_MultiRacePlateMesh, m_OrgMat_MultiRacePlate);
			ApplyMaterial(m_RarityFrameMesh, m_OrgMat_RarityFrame);
			ApplyMaterial(m_DescriptionTrimMesh, m_OrgMat_DescriptionTrim);
			ApplyMaterial(m_EliteMesh, m_OrgMat_Elite);
			ApplyMaterial(m_mercenaryLevelMesh, m_OrgMat_mercenaryLevel);
		}
	}

	private void DropMaterialReferences()
	{
		if (m_hasOriginalMaterialsStored)
		{
			IMaterialService materialService = GetMaterialService();
			materialService?.DropMaterial(m_OrgMat_CardFront);
			materialService?.DropMaterial(m_OrgMat_PremiumRibbon);
			materialService?.DropMaterial(m_OrgMat_PortraitFrame);
			materialService?.DropMaterial(m_OrgMat_Description);
			materialService?.DropMaterial(m_OrgMat_Description2);
			materialService?.DropMaterial(m_OrgMat_Name);
			materialService?.DropMaterial(m_OrgMat_ManaCost);
			materialService?.DropMaterial(m_OrgMat_Attack);
			materialService?.DropMaterial(m_OrgMat_Health);
			materialService?.DropMaterial(m_OrgMat_RacePlate);
			materialService?.DropMaterial(m_OrgMat_MultiRacePlate);
			materialService?.DropMaterial(m_OrgMat_RarityFrame);
			materialService?.DropMaterial(m_OrgMat_DescriptionTrim);
			materialService?.DropMaterial(m_OrgMat_Elite);
			materialService?.DropMaterial(m_OrgMat_mercenaryLevel);
		}
	}

	private GhostStyle GetGhostStyle()
	{
		switch (m_ghostType)
		{
		case Type.NOT_VALID:
			if (m_ghostPremium == TAG_PREMIUM.DIAMOND)
			{
				return s_ghostStyles.m_invalidDiamond;
			}
			if (m_ghostPremium == TAG_PREMIUM.SIGNATURE)
			{
				return s_ghostStyles.m_invalidSignature;
			}
			return s_ghostStyles.m_invalid;
		case Type.DORMANT:
			if (m_ghostPremium == TAG_PREMIUM.DIAMOND)
			{
				return s_ghostStyles.m_dormantDiamond;
			}
			if (m_ghostPremium == TAG_PREMIUM.SIGNATURE)
			{
				return s_ghostStyles.m_dormantSignature;
			}
			return s_ghostStyles.m_dormant;
		case Type.PURCHASABLE_HERO_SKIN:
			return s_ghostStyles.m_purchasableHeroSkin;
		default:
			if (m_ghostPremium == TAG_PREMIUM.DIAMOND)
			{
				return s_ghostStyles.m_missingDiamond;
			}
			if (m_ghostPremium == TAG_PREMIUM.SIGNATURE)
			{
				return s_ghostStyles.m_missingSignature;
			}
			return s_ghostStyles.m_missing;
		}
	}

	private void SetupGhostPlane(GhostStyle ghostStyle)
	{
		if ((bool)m_GlowPlane)
		{
			if (m_AttackMesh != null)
			{
				m_GlowPlane.GetComponent<Renderer>().SetMaterial(ghostStyle.m_GhostMaterialGlowPlane);
			}
			else
			{
				m_GlowPlane.GetComponent<Renderer>().SetMaterial(ghostStyle.m_GhostMaterialAbilityGlowPlane);
			}
		}
		if ((bool)m_GlowPlaneElite)
		{
			if (m_AttackMesh != null)
			{
				m_GlowPlaneElite.GetComponent<Renderer>().SetMaterial(ghostStyle.m_GhostMaterialGlowPlane);
			}
			else
			{
				m_GlowPlaneElite.GetComponent<Renderer>().SetMaterial(ghostStyle.m_GhostMaterialAbilityGlowPlane);
			}
		}
	}

	private void ApplyGhostMaterials()
	{
		GhostStyle ghostStyle = GetGhostStyle();
		SetupGhostPlane(ghostStyle);
		ApplyMaterialByIdx(m_CardMesh, ghostStyle.m_GhostMaterial, m_CardFrontIdx);
		ApplyMaterialByIdx(m_CardMesh, ghostStyle.m_GhostMaterial, m_PremiumRibbonIdx);
		ApplyMaterialByIdx(m_PortraitMesh, ghostStyle.m_GhostMaterial, m_PortraitFrameIdx);
		ApplyMaterialByIdx(m_DescriptionMesh, ghostStyle.m_GhostMaterialMod2x, 0);
		ApplyMaterialByIdx(m_DescriptionMesh, ghostStyle.m_GhostMaterial, 1);
		ApplyMaterial(m_NameMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_ManaCostMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_AttackMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_HealthMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_RacePlateMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_MultiRacePlateMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_RarityFrameMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_DescriptionTrimMesh, ghostStyle.m_GhostMaterialTransparent);
		ApplyMaterial(m_EliteMesh, ghostStyle.m_GhostMaterial);
		ApplyMaterial(m_mercenaryLevelMesh, ghostStyle.m_GhostMaterial);
		RenderUtils.SetRenderQueue(base.gameObject, m_R2T_BaseCard.m_RenderQueueOffset + m_renderQueue, includeInactive: true);
	}

	private void ApplyMaterial(GameObject go, Material mat)
	{
		if (!(go == null) && !(mat == null))
		{
			Renderer component = go.GetComponent<Renderer>();
			Texture orgTexture = component.GetMaterial().mainTexture;
			Vector2 offset = component.GetMaterial().mainTextureOffset;
			Vector2 scale = component.GetMaterial().mainTextureScale;
			component.SetMaterial(mat);
			component.GetMaterial().mainTexture = orgTexture;
			component.GetMaterial().mainTextureOffset = offset;
			component.GetMaterial().mainTextureScale = scale;
		}
	}

	private void ApplyMaterialByIdx(GameObject go, Material mat, int idx)
	{
		if (go == null || mat == null || idx < 0)
		{
			return;
		}
		Renderer renderer = go.GetComponent<Renderer>();
		if (!renderer)
		{
			return;
		}
		List<Material> objMaterials = renderer.GetMaterials();
		if (idx >= objMaterials.Count)
		{
			return;
		}
		Texture orgTexture = objMaterials[idx].mainTexture;
		Vector2 offset = objMaterials[idx].mainTextureOffset;
		Vector2 scale = objMaterials[idx].mainTextureScale;
		Texture secondTexture = null;
		Material oldmat = objMaterials[idx];
		if (!(oldmat == null))
		{
			if (oldmat.HasProperty("_SecondTex"))
			{
				secondTexture = oldmat.GetTexture("_SecondTex");
			}
			Color secondTint = Color.clear;
			bool num = oldmat.HasProperty("_SecondTint");
			if (num)
			{
				secondTint = oldmat.GetColor("_SecondTint");
			}
			objMaterials[idx] = mat;
			renderer.SetMaterials(objMaterials);
			Material material = renderer.GetMaterial(idx);
			material.mainTexture = orgTexture;
			material.mainTextureOffset = offset;
			material.mainTextureScale = scale;
			if (secondTexture != null)
			{
				material.SetTexture("_SecondTex", secondTexture);
			}
			if (num)
			{
				material.SetColor("_SecondTint", secondTint);
			}
		}
	}

	private void AddRTTMaterialOverride(RenderCommandLists.MatOverrideDictionary dict, GameObject go, Material mat)
	{
		if (go == null || mat == null)
		{
			return;
		}
		Renderer renderer = go.GetComponent<Renderer>();
		if ((bool)renderer)
		{
			Material origMaterial = ((!GetMaterialService().HasCustomMaterial(renderer)) ? renderer.GetSharedMaterial() : renderer.GetMaterial());
			if (origMaterial != null)
			{
				Texture orgTexture = origMaterial.mainTexture;
				Vector2 offset = origMaterial.mainTextureOffset;
				Vector2 scale = origMaterial.mainTextureScale;
				Material newMaterial = new Material(mat);
				newMaterial.mainTexture = orgTexture;
				newMaterial.mainTextureOffset = offset;
				newMaterial.mainTextureScale = scale;
				dict.Add(renderer, new RenderCommandLists.MaterialOveride(newMaterial));
			}
		}
	}

	private void AddRTTMaterialOverideByIdx(RenderCommandLists.MatOverrideDictionary dict, GameObject go, Material mat, int idx)
	{
		if (go == null || mat == null || idx < 0)
		{
			return;
		}
		Renderer renderer = go.GetComponent<Renderer>();
		if (!renderer)
		{
			return;
		}
		List<Material> objMaterials = ((!GetMaterialService().HasCustomMaterial(renderer)) ? renderer.GetSharedMaterials() : renderer.GetMaterials());
		if (idx >= objMaterials.Count)
		{
			return;
		}
		Texture orgTexture = objMaterials[idx].mainTexture;
		Vector2 offset = objMaterials[idx].mainTextureOffset;
		Vector2 scale = objMaterials[idx].mainTextureScale;
		Texture secondTexture = null;
		Material oldmat = objMaterials[idx];
		if (!(oldmat == null))
		{
			if (oldmat.HasProperty("_SecondTex"))
			{
				secondTexture = oldmat.GetTexture("_SecondTex");
			}
			Color secondTint = Color.clear;
			bool num = oldmat.HasProperty("_SecondTint");
			if (num)
			{
				secondTint = oldmat.GetColor("_SecondTint");
			}
			Material material = new Material(mat)
			{
				mainTexture = orgTexture,
				mainTextureOffset = offset,
				mainTextureScale = scale
			};
			if (secondTexture != null)
			{
				material.SetTexture("_SecondTex", secondTexture);
			}
			if (num)
			{
				material.SetColor("_SecondTint", secondTint);
			}
			dict.Add(renderer, new RenderCommandLists.MaterialOveride(material, idx));
		}
	}

	private void SetupRTTOverrides()
	{
		Material renderTextureMaterial = null;
		GhostStyle ghostStyle = GetGhostStyle();
		SetupGhostPlane(ghostStyle);
		if (ghostStyle.m_GhostCardMaterial != null && !m_isBigCard)
		{
			renderTextureMaterial = Object.Instantiate(ghostStyle.m_GhostCardMaterial);
		}
		else if (ghostStyle.m_GhostBigCardMaterial != null && m_isBigCard)
		{
			renderTextureMaterial = Object.Instantiate(ghostStyle.m_GhostBigCardMaterial);
		}
		m_R2T_BaseCard.m_Material = renderTextureMaterial;
		if ((bool)m_R2T_EffectGhost)
		{
			m_R2T_EffectGhost.m_Material = renderTextureMaterial;
		}
		if (m_rttMatOverrides != null)
		{
			DestroyRTTOverrides();
		}
		m_rttMatOverrides = new RenderCommandLists.MatOverrideDictionary();
		AddRTTMaterialOverideByIdx(m_rttMatOverrides, m_CardMesh, ghostStyle.m_GhostMaterial, m_CardFrontIdx);
		AddRTTMaterialOverideByIdx(m_rttMatOverrides, m_CardMesh, ghostStyle.m_GhostMaterial, m_PremiumRibbonIdx);
		AddRTTMaterialOverideByIdx(m_rttMatOverrides, m_PortraitMesh, ghostStyle.m_GhostMaterial, m_PortraitFrameIdx);
		AddRTTMaterialOverideByIdx(m_rttMatOverrides, m_DescriptionMesh, ghostStyle.m_GhostMaterialMod2x, 0);
		AddRTTMaterialOverideByIdx(m_rttMatOverrides, m_DescriptionMesh, ghostStyle.m_GhostMaterial, 1);
		AddRTTMaterialOverride(m_rttMatOverrides, m_NameMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_ManaCostMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_AttackMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_HealthMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_RacePlateMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_MultiRacePlateMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_RarityFrameMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_DescriptionTrimMesh, ghostStyle.m_GhostMaterialTransparent);
		AddRTTMaterialOverride(m_rttMatOverrides, m_EliteMesh, ghostStyle.m_GhostMaterial);
		AddRTTMaterialOverride(m_rttMatOverrides, m_mercenaryLevelMesh, ghostStyle.m_GhostMaterial);
		RenderUtils.SetRenderQueue(base.gameObject, m_R2T_BaseCard.m_RenderQueueOffset + m_renderQueue, includeInactive: true);
	}

	private void DestroyRTTOverrides()
	{
		if (m_rttMatOverrides != null)
		{
			foreach (KeyValuePair<Renderer, List<RenderCommandLists.MaterialOveride>> rttMatOverride in m_rttMatOverrides)
			{
				foreach (RenderCommandLists.MaterialOveride item in rttMatOverride.Value)
				{
					Object.Destroy(item.materialToUse);
				}
			}
		}
		m_rttMatOverrides = null;
	}

	private void Disable()
	{
		RestoreOrgMaterials();
		if ((bool)m_R2T_BaseCard)
		{
			m_R2T_BaseCard.enabled = false;
		}
		if ((bool)m_R2T_EffectGhost)
		{
			m_R2T_EffectGhost.enabled = false;
		}
		if ((bool)m_GlowPlane)
		{
			m_GlowPlane.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_GlowPlaneElite)
		{
			m_GlowPlaneElite.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_EffectRoot)
		{
			ParticleSystem particle = m_EffectRoot.GetComponentInChildren<ParticleSystem>();
			if ((bool)particle)
			{
				particle.Stop();
				particle.GetComponent<Renderer>().enabled = false;
			}
		}
		if (m_Actor != null)
		{
			m_Actor.m_ghostCardActive = false;
		}
	}

	private static IMaterialService GetMaterialService()
	{
		if (s_materialService == null)
		{
			s_materialService = ServiceManager.Get<IMaterialService>();
		}
		return s_materialService;
	}
}
