using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ConstructCard : MonoBehaviour
{
	private class AnimationData
	{
		public string Name;

		public Transform AnimateTransform;

		public Transform StartTransform;

		public Transform TargetTransform;

		public float AnimationTime = 1f;

		public float StartDelay;

		public GameObject GlowObject;

		public GameObject GlowObjectStandard;

		public GameObject GlowObjectUnique;

		public ParticleSystem HitBlastParticle;

		public Vector3 ImpactRotation;

		public string OnComplete = string.Empty;
	}

	private readonly Vector3 IMPACT_CAMERA_SHAKE_AMOUNT = new Vector3(0.35f, 0.35f, 0.35f);

	private readonly float IMPACT_CAMERA_SHAKE_TIME = 0.25f;

	public Material m_GhostMaterial;

	public Material m_GhostMaterialTransparent;

	public float m_ImpactRotationTime = 0.5f;

	public float m_RandomDelayVariance = 0.2f;

	public float m_AnimationRarityScaleCommon = 1f;

	public float m_AnimationRarityScaleRare = 0.9f;

	public float m_AnimationRarityScaleEpic = 0.8f;

	public float m_AnimationRarityScaleLegendary = 0.7f;

	public GameObject m_GhostGlow;

	public Texture m_GhostTextureUnique;

	public GameObject m_FuseGlow;

	public ParticleSystem m_RarityBurstCommon;

	public ParticleSystem m_RarityBurstRare;

	public ParticleSystem m_RarityBurstEpic;

	public ParticleSystem m_RarityBurstLegendary;

	public Transform m_ManaGemStartPosition;

	public Transform m_ManaGemTargetPosition;

	public float m_ManaGemStartDelay;

	public float m_ManaGemAnimTime = 1f;

	public GameObject m_ManaGemGlow;

	public ParticleSystem m_ManaGemHitBlastParticle;

	public Vector3 m_ManaGemImpactRotation = new Vector3(20f, 0f, 20f);

	public Transform m_DescriptionStartPosition;

	public Transform m_DescriptionTargetPosition;

	public float m_DescriptionStartDelay;

	public float m_DescriptionAnimTime = 1f;

	public GameObject m_DescriptionGlow;

	public ParticleSystem m_DescriptionHitBlastParticle;

	public Vector3 m_DescriptionImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_AttackStartPosition;

	public Transform m_AttackTargetPosition;

	public float m_AttackStartDelay;

	public float m_AttackAnimTime = 1f;

	public GameObject m_AttackGlow;

	public ParticleSystem m_AttackHitBlastParticle;

	public Vector3 m_AttackImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_HealthStartPosition;

	public Transform m_HealthTargetPosition;

	public float m_HealthStartDelay;

	public float m_HealthAnimTime = 1f;

	public GameObject m_HealthGlow;

	public ParticleSystem m_HealthHitBlastParticle;

	public Vector3 m_HealthImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_ArmorStartPosition;

	public Transform m_ArmorTargetPosition;

	public float m_ArmorStartDelay;

	public float m_ArmorAnimTime = 1f;

	public GameObject m_ArmorGlow;

	public ParticleSystem m_ArmorHitBlastParticle;

	public Vector3 m_ArmorImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_PortraitStartPosition;

	public Transform m_PortraitTargetPosition;

	public float m_PortraitStartDelay;

	public float m_PortraitAnimTime = 1f;

	public GameObject m_PortraitGlow;

	public GameObject m_PortraitGlowStandard;

	public GameObject m_PortraitGlowUnique;

	public ParticleSystem m_PortraitHitBlastParticle;

	public Vector3 m_PortraitImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_NameStartPosition;

	public Transform m_NameTargetPosition;

	public float m_NameStartDelay;

	public float m_NameAnimTime = 1f;

	public GameObject m_NameGlow;

	public ParticleSystem m_NameHitBlastParticle;

	public Vector3 m_NameImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_RarityStartPosition;

	public Transform m_RarityTargetPosition;

	public float m_RarityStartDelay;

	public float m_RarityAnimTime = 1f;

	public GameObject m_RarityGlowCommon;

	public GameObject m_RarityGlowRare;

	public GameObject m_RarityGlowEpic;

	public GameObject m_RarityGlowLegendary;

	public ParticleSystem m_RarityHitBlastParticle;

	public Vector3 m_RarityImpactRotation = new Vector3(-15f, 0f, 0f);

	public Transform m_DkRunesStartPosition;

	public Transform m_DkRunesTargetPosition;

	public float m_DkRuneStartDelay;

	public float m_DkRuneAnimTime = 1f;

	public GameObject m_DkRunes;

	public ParticleSystem m_DkRunesHitBlastParticle;

	public Vector3 m_DkRuneImpactRotation = new Vector3(-15f, 0f, 0f);

	private Actor m_Actor;

	private Spell m_GhostSpell;

	private float m_AnimationScale = 1f;

	private bool isInit;

	private GameObject m_ManaGemInstance;

	private GameObject m_DescriptionInstance;

	private GameObject m_AttackInstance;

	private GameObject m_HealthInstance;

	private GameObject m_ArmorInstance;

	private GameObject m_PortraitInstance;

	private GameObject m_NameInstance;

	private GameObject m_RarityInstance;

	private GameObject m_DkRunesInstance;

	private GameObject m_CardMesh;

	private int m_CardFrontIdx;

	private GameObject m_PortraitMesh;

	private int m_PortraitFrameIdx;

	private GameObject m_NameMesh;

	private GameObject m_DescriptionMesh;

	private GameObject m_DescriptionTrimMesh;

	private GameObject m_RarityGemMesh;

	private GameObject m_RarityFrameMesh;

	private GameObject m_ManaCostMesh;

	private GameObject m_AttackMesh;

	private GameObject m_HealthMesh;

	private GameObject m_ArmorMesh;

	private GameObject m_RacePlateMesh;

	private GameObject m_EliteMesh;

	private GameObject m_DkRunesMesh;

	private GameObject m_ManaGemClone;

	private Material m_OrgMat_CardFront;

	private Material m_OrgMat_PortraitFrame;

	private Material m_OrgMat_Name;

	private Material m_OrgMat_Description;

	private Material m_OrgMat_Description2;

	private Material m_OrgMat_DescriptionTrim;

	private Material m_OrgMat_RarityFrame;

	private Material m_OrgMat_ManaCost;

	private Material m_OrgMat_Attack;

	private Material m_OrgMat_Health;

	private Material m_OrgMat_Armor;

	private Material m_OrgMat_RacePlate;

	private Material m_OrgMat_Elite;

	private List<ParticleSystem> m_tempParticleSystems = new List<ParticleSystem>();

	private List<Renderer> m_tempRenderers = new List<Renderer>();

	private void OnDisable()
	{
		Cancel();
	}

	private void OnDestroy()
	{
		m_tempParticleSystems = null;
		m_tempRenderers = null;
	}

	public void Construct()
	{
		StartCoroutine(DoConstruct());
	}

	private IEnumerator DoConstruct()
	{
		m_Actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
		if (m_Actor == null)
		{
			Debug.LogError($"{base.transform.root.name} Ghost card effect failed to find Actor!");
			base.enabled = false;
			yield break;
		}
		m_Actor.HideAllText();
		m_GhostSpell = m_Actor.GetSpell(SpellType.GHOSTMODE);
		m_GhostSpell.ActivateState(SpellStateType.CANCEL);
		m_Actor.ActivateSpellDeathState(SpellType.GHOSTMODE);
		while (m_GhostSpell.IsActive() || m_Actor.m_ghostCardActive)
		{
			yield return new WaitForEndOfFrame();
		}
		m_Actor.HideAllText();
		Init();
		CreateInstances();
		if ((bool)m_GhostGlow)
		{
			Renderer ghostGlowRenderer = m_GhostGlow.GetComponent<Renderer>();
			if (m_Actor.IsElite() && (bool)m_GhostTextureUnique)
			{
				ghostGlowRenderer.GetMaterial().mainTexture = m_GhostTextureUnique;
			}
			ghostGlowRenderer.enabled = true;
			m_GhostGlow.GetComponent<Animation>().Play("GhostModeHot", PlayMode.StopAll);
		}
		if ((bool)m_RarityGemMesh)
		{
			m_RarityGemMesh.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_RarityFrameMesh)
		{
			m_RarityFrameMesh.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_DkRunesMesh)
		{
			Renderer[] componentsInChildren = m_DkRunesMesh.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		if ((bool)m_ManaGemStartPosition && (bool)m_ManaGemInstance)
		{
			AnimateManaGem();
		}
		if ((bool)m_DescriptionStartPosition && (bool)m_DescriptionInstance)
		{
			AnimateDescription();
		}
		if ((bool)m_AttackStartPosition && (bool)m_AttackInstance)
		{
			AnimateAttack();
		}
		if ((bool)m_HealthStartPosition && (bool)m_HealthInstance)
		{
			AnimateHealth();
		}
		if ((bool)m_ArmorStartPosition && (bool)m_ArmorInstance)
		{
			AnimateArmor();
		}
		if ((bool)m_PortraitStartPosition && (bool)m_PortraitInstance)
		{
			AnimatePortrait();
		}
		if ((bool)m_NameStartPosition && (bool)m_NameInstance)
		{
			AnimateName();
		}
		if ((bool)m_RarityStartPosition)
		{
			AnimateRarity();
		}
		if ((bool)m_DkRunesStartPosition)
		{
			AnimateDkRunes();
		}
	}

	private void Init()
	{
		if (isInit)
		{
			return;
		}
		m_Actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
		if (m_Actor == null)
		{
			Debug.LogError($"{base.transform.root.name} Ghost card effect failed to find Actor!");
			base.enabled = false;
			return;
		}
		m_CardMesh = m_Actor.m_cardMesh;
		m_CardFrontIdx = m_Actor.m_cardFrontMatIdx;
		m_PortraitMesh = m_Actor.m_portraitMesh;
		m_PortraitFrameIdx = m_Actor.m_portraitFrameMatIdx;
		m_NameMesh = m_Actor.m_nameBannerMesh;
		m_DescriptionMesh = m_Actor.m_descriptionMesh;
		m_DescriptionTrimMesh = m_Actor.m_descriptionTrimMesh;
		m_RarityGemMesh = m_Actor.m_rarityGemMesh;
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
				m_Actor.m_attackObject.GetComponentsInChildren(m_tempRenderers);
				foreach (Renderer mesh in m_tempRenderers)
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
				m_Actor.m_healthObject.GetComponentsInChildren(m_tempRenderers);
				foreach (Renderer mesh2 in m_tempRenderers)
				{
					if (!mesh2.GetComponent<UberText>())
					{
						m_HealthMesh = mesh2.gameObject;
					}
				}
			}
		}
		if ((bool)m_Actor.m_armorObject)
		{
			Renderer armorMesh = m_Actor.m_armorObject.GetComponent<Renderer>();
			if (armorMesh != null)
			{
				m_ArmorMesh = armorMesh.gameObject;
			}
			if (m_ArmorMesh == null)
			{
				m_Actor.m_armorObject.GetComponentsInChildren(m_tempRenderers);
				foreach (Renderer mesh3 in m_tempRenderers)
				{
					if (!mesh3.GetComponent<UberText>())
					{
						m_ArmorMesh = mesh3.gameObject;
					}
				}
			}
		}
		m_ManaCostMesh = m_Actor.m_manaObject;
		m_RacePlateMesh = m_Actor.m_racePlateObject;
		m_EliteMesh = m_Actor.m_eliteObject;
		CardRuneBanner runeBanner = m_Actor.m_cardRuneBanner;
		if (runeBanner != null)
		{
			m_DkRunesMesh = runeBanner.gameObject;
			runeBanner.Hide();
		}
		StoreOrgMaterials();
		switch (m_Actor.GetRarity())
		{
		case TAG_RARITY.RARE:
			m_AnimationScale = m_AnimationRarityScaleRare;
			break;
		case TAG_RARITY.EPIC:
			m_AnimationScale = m_AnimationRarityScaleEpic;
			break;
		case TAG_RARITY.LEGENDARY:
			m_AnimationScale = m_AnimationRarityScaleLegendary;
			break;
		default:
			m_AnimationScale = m_AnimationRarityScaleCommon;
			break;
		}
		isInit = true;
	}

	private void Cancel()
	{
		StopAllCoroutines();
		RestoreOrgMaterials();
		DisableManaGem();
		DisableDescription();
		DisableAttack();
		DisableHealth();
		DisableArmor();
		DisablePortrait();
		DisableName();
		DisableRarity();
		DestroyInstances();
		StopAllParticles();
		HideAllMeshObjects();
		if ((bool)m_Actor)
		{
			m_Actor.ShowAllText();
		}
		if (m_Actor != null)
		{
			iTween.StopByName(m_Actor.gameObject, "CardConstructImpactRotation");
		}
	}

	private void StopAllParticles()
	{
		GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem ps in m_tempParticleSystems)
		{
			if (ps.isPlaying)
			{
				ps.Stop();
			}
		}
	}

	private void HideAllMeshObjects()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetComponent<Renderer>().enabled = false;
		}
	}

	private void CreateInstances()
	{
		Vector3 offScreen = new Vector3(0f, -5000f, 0f);
		if ((bool)m_RarityGemMesh)
		{
			m_RarityGemMesh.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_RarityFrameMesh)
		{
			m_RarityFrameMesh.GetComponent<Renderer>().enabled = false;
		}
		if ((bool)m_ManaGemStartPosition && (bool)m_ManaCostMesh)
		{
			m_ManaGemInstance = Object.Instantiate(m_ManaCostMesh);
			m_ManaGemInstance.transform.parent = base.transform.parent;
			m_ManaGemInstance.transform.position = offScreen;
		}
		if ((bool)m_DescriptionStartPosition && (bool)m_DescriptionMesh)
		{
			m_DescriptionInstance = Object.Instantiate(m_DescriptionMesh);
			m_DescriptionInstance.transform.parent = base.transform.parent;
			m_DescriptionInstance.transform.position = offScreen;
		}
		if ((bool)m_AttackStartPosition && (bool)m_AttackMesh)
		{
			m_AttackInstance = Object.Instantiate(m_AttackMesh);
			m_AttackInstance.transform.parent = base.transform.parent;
			m_AttackInstance.transform.position = offScreen;
		}
		if ((bool)m_HealthStartPosition && (bool)m_HealthMesh)
		{
			m_HealthInstance = Object.Instantiate(m_HealthMesh);
			m_HealthInstance.transform.parent = base.transform.parent;
			m_HealthInstance.transform.position = offScreen;
		}
		if ((bool)m_ArmorStartPosition && (bool)m_ArmorMesh)
		{
			m_ArmorInstance = Object.Instantiate(m_ArmorMesh);
			m_ArmorInstance.transform.parent = base.transform.parent;
			m_ArmorInstance.transform.position = offScreen;
		}
		if ((bool)m_PortraitStartPosition && (bool)m_PortraitMesh)
		{
			m_PortraitInstance = Object.Instantiate(m_PortraitMesh);
			m_PortraitInstance.transform.parent = base.transform.parent;
			m_PortraitInstance.transform.position = offScreen;
		}
		if ((bool)m_NameStartPosition && (bool)m_NameMesh)
		{
			m_NameInstance = Object.Instantiate(m_NameMesh);
			m_NameInstance.transform.parent = base.transform.parent;
			m_NameInstance.transform.position = offScreen;
		}
		if ((bool)m_RarityStartPosition && (bool)m_RarityGemMesh)
		{
			m_RarityInstance = Object.Instantiate(m_RarityGemMesh);
			m_RarityInstance.transform.parent = base.transform.parent;
			m_RarityInstance.transform.position = offScreen;
		}
		if ((bool)m_DkRunesStartPosition && (bool)m_DkRunes)
		{
			m_DkRunesInstance = Object.Instantiate(m_DkRunesMesh);
			m_DkRunesInstance.transform.parent = base.transform.parent;
			m_DkRunesInstance.transform.position = offScreen;
		}
	}

	private void DestroyInstances()
	{
		if ((bool)m_ManaGemInstance)
		{
			Object.Destroy(m_ManaGemInstance);
		}
		if ((bool)m_DescriptionInstance)
		{
			Object.Destroy(m_DescriptionInstance);
		}
		if ((bool)m_AttackInstance)
		{
			Object.Destroy(m_AttackInstance);
		}
		if ((bool)m_HealthInstance)
		{
			Object.Destroy(m_HealthInstance);
		}
		if ((bool)m_ArmorInstance)
		{
			Object.Destroy(m_ArmorInstance);
		}
		if ((bool)m_PortraitInstance)
		{
			Object.Destroy(m_PortraitInstance);
		}
		if ((bool)m_NameInstance)
		{
			Object.Destroy(m_NameInstance);
		}
		if ((bool)m_RarityInstance)
		{
			Object.Destroy(m_RarityInstance);
		}
		if ((bool)m_DkRunesInstance)
		{
			Object.Destroy(m_DkRunesInstance);
		}
	}

	private void AnimateManaGem()
	{
		GameObject animObj = m_ManaGemInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_ManaCostMesh.transform.lossyScale;
		animObj.transform.position = m_ManaGemStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_ManaCost);
		float delay = Random.Range(m_ManaGemStartDelay - m_ManaGemStartDelay * m_RandomDelayVariance, m_ManaGemStartDelay + m_ManaGemStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "ManaGem",
			AnimateTransform = animObj.transform,
			StartTransform = m_ManaGemStartPosition.transform,
			TargetTransform = m_ManaGemTargetPosition.transform,
			HitBlastParticle = m_ManaGemHitBlastParticle,
			AnimationTime = m_ManaGemAnimTime,
			StartDelay = delay,
			GlowObject = m_ManaGemGlow,
			ImpactRotation = m_ManaGemImpactRotation,
			OnComplete = "ManaGemOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator ManaGemOnComplete()
	{
		DisableManaGem();
		yield break;
	}

	private void DisableManaGem()
	{
		if (!m_ManaGemGlow)
		{
			return;
		}
		m_ManaGemGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateDescription()
	{
		GameObject animObj = m_DescriptionInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_DescriptionMesh.transform.lossyScale;
		animObj.transform.position = m_DescriptionStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_Description);
		float delay = Random.Range(m_DescriptionStartDelay - m_DescriptionStartDelay * m_RandomDelayVariance, m_DescriptionStartDelay + m_DescriptionStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Description",
			AnimateTransform = animObj.transform,
			StartTransform = m_DescriptionStartPosition.transform,
			TargetTransform = m_DescriptionTargetPosition.transform,
			HitBlastParticle = m_DescriptionHitBlastParticle,
			AnimationTime = m_DescriptionAnimTime,
			StartDelay = delay,
			GlowObject = m_DescriptionGlow,
			ImpactRotation = m_DescriptionImpactRotation,
			OnComplete = "DescriptionOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator DescriptionOnComplete()
	{
		DisableDescription();
		yield break;
	}

	private void DisableDescription()
	{
		if (!m_DescriptionGlow)
		{
			return;
		}
		m_DescriptionGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateAttack()
	{
		GameObject animObj = m_AttackInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_AttackMesh.transform.lossyScale;
		animObj.transform.position = m_AttackStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_Attack);
		float delay = Random.Range(m_AttackStartDelay - m_AttackStartDelay * m_RandomDelayVariance, m_AttackStartDelay + m_AttackStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Attack",
			AnimateTransform = animObj.transform,
			StartTransform = m_AttackStartPosition.transform,
			TargetTransform = m_AttackTargetPosition.transform,
			HitBlastParticle = m_AttackHitBlastParticle,
			AnimationTime = m_AttackAnimTime,
			StartDelay = delay,
			GlowObject = m_AttackGlow,
			ImpactRotation = m_AttackImpactRotation,
			OnComplete = "AttackOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator AttackOnComplete()
	{
		DisableAttack();
		yield break;
	}

	private void DisableAttack()
	{
		if (!m_AttackGlow)
		{
			return;
		}
		m_AttackGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateHealth()
	{
		GameObject animObj = m_HealthInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_HealthMesh.transform.lossyScale;
		animObj.transform.position = m_HealthStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_Health);
		float delay = Random.Range(m_HealthStartDelay - m_HealthStartDelay * m_RandomDelayVariance, m_HealthStartDelay + m_HealthStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Health",
			AnimateTransform = animObj.transform,
			StartTransform = m_HealthStartPosition.transform,
			TargetTransform = m_HealthTargetPosition.transform,
			HitBlastParticle = m_HealthHitBlastParticle,
			AnimationTime = m_HealthAnimTime,
			StartDelay = delay,
			GlowObject = m_HealthGlow,
			ImpactRotation = m_HealthImpactRotation,
			OnComplete = "HealthOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator HealthOnComplete()
	{
		DisableHealth();
		yield break;
	}

	private void DisableHealth()
	{
		if (!m_HealthGlow)
		{
			return;
		}
		m_HealthGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateArmor()
	{
		GameObject animObj = m_ArmorInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_ArmorMesh.transform.lossyScale;
		animObj.transform.position = m_ArmorStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_Armor);
		float delay = Random.Range(m_ArmorStartDelay - m_ArmorStartDelay * m_RandomDelayVariance, m_ArmorStartDelay + m_ArmorStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Armor",
			AnimateTransform = animObj.transform,
			StartTransform = m_ArmorStartPosition.transform,
			TargetTransform = m_ArmorTargetPosition.transform,
			HitBlastParticle = m_ArmorHitBlastParticle,
			AnimationTime = m_ArmorAnimTime,
			StartDelay = delay,
			GlowObject = m_ArmorGlow,
			ImpactRotation = m_ArmorImpactRotation,
			OnComplete = "ArmorOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator ArmorOnComplete()
	{
		DisableArmor();
		yield break;
	}

	private void DisableArmor()
	{
		if (!m_ArmorGlow)
		{
			return;
		}
		m_ArmorGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimatePortrait()
	{
		GameObject animObj = m_PortraitInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_PortraitMesh.transform.lossyScale;
		animObj.transform.position = m_PortraitStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		float delay = Random.Range(m_PortraitStartDelay - m_PortraitStartDelay * m_RandomDelayVariance, m_PortraitStartDelay + m_PortraitStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Portrait",
			AnimateTransform = animObj.transform,
			StartTransform = m_PortraitStartPosition.transform,
			TargetTransform = m_PortraitTargetPosition.transform,
			HitBlastParticle = m_PortraitHitBlastParticle,
			AnimationTime = m_PortraitAnimTime,
			StartDelay = delay,
			GlowObject = m_PortraitGlow,
			GlowObjectStandard = m_PortraitGlowStandard,
			GlowObjectUnique = m_PortraitGlowUnique,
			ImpactRotation = m_PortraitImpactRotation,
			OnComplete = "PortraitOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator PortraitOnComplete()
	{
		DisablePortrait();
		yield break;
	}

	private void DisablePortrait()
	{
		if (!m_PortraitGlow)
		{
			return;
		}
		m_PortraitGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateName()
	{
		GameObject animObj = m_NameInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_NameMesh.transform.lossyScale;
		animObj.transform.position = m_NameStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		animObj.GetComponent<Renderer>().SetMaterial(m_OrgMat_Name);
		float delay = Random.Range(m_NameStartDelay - m_NameStartDelay * m_RandomDelayVariance, m_NameStartDelay + m_NameStartDelay * m_RandomDelayVariance);
		AnimationData animData = new AnimationData
		{
			Name = "Name",
			AnimateTransform = animObj.transform,
			StartTransform = m_NameStartPosition.transform,
			TargetTransform = m_NameTargetPosition.transform,
			HitBlastParticle = m_NameHitBlastParticle,
			AnimationTime = m_NameAnimTime,
			StartDelay = delay,
			GlowObject = m_NameGlow,
			ImpactRotation = m_NameImpactRotation,
			OnComplete = "NameOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator NameOnComplete()
	{
		DisableName();
		yield break;
	}

	private void DisableName()
	{
		if (!m_NameGlow)
		{
			return;
		}
		m_NameGlow.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateRarity()
	{
		if (m_Actor.GetRarity() != TAG_RARITY.FREE)
		{
			GameObject animObj = m_RarityInstance;
			animObj.transform.parent = null;
			animObj.transform.localScale = m_RarityGemMesh.transform.lossyScale;
			animObj.transform.position = m_RarityStartPosition.transform.position;
			animObj.transform.parent = base.transform.parent;
			m_RarityInstance.GetComponent<Renderer>().enabled = true;
			GameObject glow = m_RarityGlowCommon;
			switch (m_Actor.GetRarity())
			{
			case TAG_RARITY.RARE:
				glow = m_RarityGlowRare;
				break;
			case TAG_RARITY.EPIC:
				glow = m_RarityGlowEpic;
				break;
			case TAG_RARITY.LEGENDARY:
				glow = m_RarityGlowLegendary;
				break;
			}
			float delay = Random.Range(m_RarityStartDelay - m_RarityStartDelay * m_RandomDelayVariance, m_RarityStartDelay + m_RarityStartDelay * m_RandomDelayVariance);
			AnimationData animData = new AnimationData
			{
				Name = "Rarity",
				AnimateTransform = animObj.transform,
				StartTransform = m_RarityStartPosition.transform,
				TargetTransform = m_RarityTargetPosition.transform,
				HitBlastParticle = m_RarityHitBlastParticle,
				AnimationTime = m_RarityAnimTime,
				StartDelay = delay,
				GlowObject = glow,
				ImpactRotation = m_RarityImpactRotation,
				OnComplete = "RarityOnComplete"
			};
			StartCoroutine("AnimateObject", animData);
		}
	}

	private IEnumerator RarityOnComplete()
	{
		DisableRarity();
		if (m_Actor.GetRarity() != TAG_RARITY.FREE)
		{
			if ((bool)m_RarityGemMesh)
			{
				m_RarityGemMesh.GetComponent<Renderer>().enabled = true;
			}
			if ((bool)m_RarityFrameMesh)
			{
				m_RarityFrameMesh.GetComponent<Renderer>().enabled = true;
			}
		}
		StartCoroutine(EndAnimation());
		yield break;
	}

	private void DisableRarity()
	{
		if (!m_RarityGlowCommon)
		{
			return;
		}
		m_RarityGlowCommon.GetComponentsInChildren(m_tempParticleSystems);
		foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
		{
			tempParticleSystem.Stop();
		}
	}

	private void AnimateDkRunes()
	{
		GameObject animObj = m_DkRunesInstance;
		animObj.transform.parent = null;
		animObj.transform.localScale = m_DkRunesMesh.transform.lossyScale;
		animObj.transform.position = m_DkRunesStartPosition.transform.position;
		animObj.transform.parent = base.transform.parent;
		float delay = Random.Range(m_DkRuneStartDelay - m_DkRuneStartDelay * m_RandomDelayVariance, m_DkRuneStartDelay + m_DkRuneStartDelay * m_RandomDelayVariance);
		CardRuneBanner bannerInstance = animObj.GetComponent<CardRuneBanner>();
		CardRuneBanner bannerOriginal = m_DkRunesMesh.GetComponent<CardRuneBanner>();
		if (bannerInstance != null && bannerOriginal != null)
		{
			RunePattern originalPattern = bannerOriginal.GetCurrentRunePattern();
			bannerInstance.Show(originalPattern);
		}
		AnimationData animData = new AnimationData
		{
			Name = "Runes",
			AnimateTransform = animObj.transform,
			StartTransform = m_DkRunesStartPosition.transform,
			TargetTransform = m_DkRunesTargetPosition.transform,
			HitBlastParticle = m_DkRunesHitBlastParticle,
			AnimationTime = m_DkRuneAnimTime,
			StartDelay = delay,
			GlowObject = null,
			ImpactRotation = m_DkRuneImpactRotation,
			OnComplete = "RunesOnComplete"
		};
		StartCoroutine("AnimateObject", animData);
	}

	private IEnumerator RunesOnComplete()
	{
		if (m_DkRunesMesh != null)
		{
			CardRuneBanner runeBanner = m_DkRunesMesh.GetComponent<CardRuneBanner>();
			if (runeBanner != null)
			{
				runeBanner.ShowLastShownRuneBanner();
			}
		}
		yield break;
	}

	private IEnumerator EndAnimation()
	{
		ParticleSystem burst = m_RarityBurstCommon;
		TAG_RARITY rarity = m_Actor.GetRarity();
		switch (rarity)
		{
		case TAG_RARITY.RARE:
			burst = m_RarityBurstRare;
			break;
		case TAG_RARITY.EPIC:
			burst = m_RarityBurstEpic;
			break;
		case TAG_RARITY.LEGENDARY:
			burst = m_RarityBurstLegendary;
			break;
		}
		if ((bool)burst)
		{
			burst.GetComponentsInChildren(m_tempRenderers);
			foreach (Renderer tempRenderer in m_tempRenderers)
			{
				tempRenderer.enabled = true;
			}
			burst.Play(withChildren: true);
		}
		string fuseAnimation = "CardFuse_Common";
		switch (rarity)
		{
		case TAG_RARITY.RARE:
			fuseAnimation = "CardFuse_Rare";
			break;
		case TAG_RARITY.EPIC:
			fuseAnimation = "CardFuse_Epic";
			break;
		case TAG_RARITY.LEGENDARY:
			fuseAnimation = "CardFuse_Legendary";
			break;
		}
		if ((bool)m_FuseGlow)
		{
			m_FuseGlow.GetComponent<Renderer>().enabled = true;
			m_FuseGlow.GetComponent<Animation>().Play(fuseAnimation, PlayMode.StopAll);
		}
		yield return new WaitForSeconds(0.25f);
		DestroyInstances();
		m_Actor.ShowAllText();
		RestoreOrgMaterials();
	}

	private IEnumerator AnimateObject(AnimationData animData)
	{
		yield return new WaitForSeconds(animData.StartDelay);
		float animPos = 0f;
		float rate = 1f / (animData.AnimationTime * m_AnimationScale);
		Quaternion currCardRot = m_Actor.transform.rotation;
		m_Actor.transform.rotation = Quaternion.identity;
		Vector3 startPosition = animData.StartTransform.position;
		Quaternion startRotation = animData.StartTransform.rotation;
		m_Actor.transform.rotation = currCardRot;
		if ((bool)animData.GlowObject)
		{
			GameObject glowObj = animData.GlowObject;
			glowObj.transform.parent = animData.AnimateTransform;
			glowObj.transform.localPosition = Vector3.zero;
			glowObj.GetComponentsInChildren(m_tempParticleSystems);
			foreach (ParticleSystem tempParticleSystem in m_tempParticleSystems)
			{
				tempParticleSystem.Play();
			}
			if ((bool)animData.GlowObjectStandard && (bool)animData.GlowObjectUnique)
			{
				if (m_Actor.IsElite())
				{
					animData.GlowObjectUnique.GetComponent<Renderer>().enabled = true;
				}
				else
				{
					animData.GlowObjectStandard.GetComponent<Renderer>().enabled = true;
				}
			}
			else
			{
				glowObj.GetComponentsInChildren(m_tempRenderers);
				foreach (Renderer tempRenderer in m_tempRenderers)
				{
					tempRenderer.enabled = true;
				}
			}
		}
		while (animPos < 1f)
		{
			Vector3 currentTargetPosition = animData.TargetTransform.position;
			Quaternion currentTargetRotation = animData.TargetTransform.rotation;
			animPos += rate * Time.deltaTime;
			Vector3 position = Vector3.Lerp(startPosition, currentTargetPosition, animPos);
			Quaternion rotation = Quaternion.Lerp(startRotation, currentTargetRotation, animPos);
			animData.AnimateTransform.position = position;
			animData.AnimateTransform.rotation = rotation;
			yield return null;
		}
		if ((bool)animData.HitBlastParticle)
		{
			animData.HitBlastParticle.transform.position = animData.TargetTransform.position;
			animData.HitBlastParticle.GetComponent<Renderer>().enabled = true;
			animData.HitBlastParticle.Play();
		}
		animData.AnimateTransform.parent = animData.TargetTransform;
		animData.AnimateTransform.position = animData.TargetTransform.position;
		animData.AnimateTransform.rotation = animData.TargetTransform.rotation;
		if ((bool)animData.GlowObject)
		{
			foreach (ParticleSystem tempParticleSystem2 in m_tempParticleSystems)
			{
				tempParticleSystem2.Stop();
			}
		}
		if (!(m_Actor.gameObject == null))
		{
			m_Actor.gameObject.transform.localRotation = Quaternion.Euler(animData.ImpactRotation);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("rotation", Vector3.zero);
			args.Add("time", m_ImpactRotationTime);
			args.Add("easetype", iTween.EaseType.easeOutQuad);
			args.Add("space", Space.Self);
			args.Add("name", "CardConstructImpactRotation" + animData.Name);
			iTween.StopByName(m_Actor.gameObject, "CardConstructImpactRotation" + animData.Name);
			iTween.RotateTo(m_Actor.gameObject, args);
			CameraShakeMgr.Shake(Camera.main, IMPACT_CAMERA_SHAKE_AMOUNT, IMPACT_CAMERA_SHAKE_TIME);
			if (animData.OnComplete != string.Empty)
			{
				StartCoroutine(animData.OnComplete);
			}
		}
	}

	private void StoreOrgMaterials()
	{
		if ((bool)m_CardMesh)
		{
			m_OrgMat_CardFront = m_CardMesh.GetComponent<Renderer>().GetMaterial(m_CardFrontIdx);
		}
		if ((bool)m_PortraitMesh)
		{
			m_OrgMat_PortraitFrame = m_PortraitMesh.GetComponent<Renderer>().GetSharedMaterial(m_PortraitFrameIdx);
		}
		if ((bool)m_NameMesh)
		{
			m_OrgMat_Name = m_NameMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_ManaCostMesh)
		{
			m_OrgMat_ManaCost = m_ManaCostMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_AttackMesh)
		{
			m_OrgMat_Attack = m_AttackMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_HealthMesh)
		{
			m_OrgMat_Health = m_HealthMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_ArmorMesh)
		{
			m_OrgMat_Armor = m_ArmorMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_RacePlateMesh)
		{
			m_OrgMat_RacePlate = m_RacePlateMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_RarityFrameMesh)
		{
			m_OrgMat_RarityFrame = m_RarityFrameMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_DescriptionMesh)
		{
			List<Material> descMats = m_DescriptionMesh.GetComponent<Renderer>().GetMaterials();
			if (m_DescriptionMesh.GetComponent<Renderer>() != null)
			{
				if (descMats.Count > 1)
				{
					m_OrgMat_Description = descMats[0];
					m_OrgMat_Description2 = descMats[1];
				}
				else
				{
					m_OrgMat_Description = m_DescriptionMesh.GetComponent<Renderer>().GetMaterial();
				}
			}
		}
		if ((bool)m_DescriptionTrimMesh)
		{
			m_OrgMat_DescriptionTrim = m_DescriptionTrimMesh.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_EliteMesh)
		{
			m_OrgMat_Elite = m_EliteMesh.GetComponent<Renderer>().GetMaterial();
		}
	}

	private void RestoreOrgMaterials()
	{
		ApplyMaterialByIdx(m_CardMesh, m_OrgMat_CardFront, m_CardFrontIdx);
		ApplySharedMaterialByIdx(m_PortraitMesh, m_OrgMat_PortraitFrame, m_PortraitFrameIdx);
		ApplyMaterialByIdx(m_DescriptionMesh, m_OrgMat_Description, 0);
		ApplyMaterialByIdx(m_DescriptionMesh, m_OrgMat_Description2, 1);
		ApplyMaterial(m_NameMesh, m_OrgMat_Name);
		ApplyMaterial(m_ManaCostMesh, m_OrgMat_ManaCost);
		ApplyMaterial(m_AttackMesh, m_OrgMat_Attack);
		ApplyMaterial(m_HealthMesh, m_OrgMat_Health);
		ApplyMaterial(m_ArmorMesh, m_OrgMat_Armor);
		ApplyMaterial(m_RacePlateMesh, m_OrgMat_RacePlate);
		ApplyMaterial(m_RarityFrameMesh, m_OrgMat_RarityFrame);
		ApplyMaterial(m_DescriptionTrimMesh, m_OrgMat_DescriptionTrim);
		ApplyMaterial(m_EliteMesh, m_OrgMat_Elite);
	}

	private void ApplyMaterial(GameObject go, Material mat)
	{
		if (!(go == null))
		{
			Renderer component = go.GetComponent<Renderer>();
			Texture orgTexture = component.GetMaterial().mainTexture;
			component.SetMaterial(mat);
			component.GetMaterial().mainTexture = orgTexture;
		}
	}

	private void ApplyMaterialByIdx(GameObject go, Material mat, int idx)
	{
		if (!(go == null) && !(mat == null) && idx >= 0)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			List<Material> objMaterials = renderer.GetMaterials();
			if (idx < objMaterials.Count)
			{
				Texture orgTexture = renderer.GetMaterial(idx).mainTexture;
				renderer.SetMaterial(idx, mat);
				renderer.GetMaterial(idx).mainTexture = orgTexture;
			}
		}
	}

	private void ApplySharedMaterialByIdx(GameObject go, Material mat, int idx)
	{
		if (!(go == null) && !(mat == null) && idx >= 0)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			List<Material> objMaterials = renderer.GetSharedMaterials();
			if (idx < objMaterials.Count)
			{
				Texture orgTexture = renderer.GetSharedMaterial(idx).mainTexture;
				renderer.SetSharedMaterial(idx, mat);
				renderer.GetSharedMaterial(idx).mainTexture = orgTexture;
			}
		}
	}
}
