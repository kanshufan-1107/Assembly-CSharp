using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CollectionHeroSkin : CollectibleSkin
{
	public MeshRenderer m_classIcon;

	public Spell m_socketFX;

	public GameObject m_meshRef;

	public Vector3 m_diamondHeroScale = new Vector3(0.925f, 0.925f, 0.925f);

	public UberText m_nameMythic;

	public GameObject m_nameShadowMythic;

	public MeshRenderer m_classIconMythic;

	public void SetClass(EntityDef entityDef)
	{
		bool isMythicHero = GameUtils.IsMythicHero(entityDef);
		UpdateProperties(isMythicHero);
		TAG_CLASS classTag = entityDef.GetClass();
		if (m_classIcon != null)
		{
			Vector2 textureOffset = CollectionPageManager.s_classTextureOffsets[classTag];
			Renderer renderer = m_classIcon.GetComponent<Renderer>();
			(Application.isPlaying ? renderer.GetMaterial() : renderer.GetSharedMaterial()).SetTextureOffset("_MainTex", textureOffset);
		}
		if (m_favoriteBannerText != null)
		{
			m_favoriteBannerText.Text = GameStrings.Format("GLUE_COLLECTION_MANAGER_FAVORITE_DEFAULT_TEXT", GameStrings.GetClassName(classTag));
		}
		if (m_meshRef != null && RewardUtils.IsShopPremiumHeroSkin(entityDef))
		{
			m_meshRef.transform.localScale = m_diamondHeroScale;
		}
	}

	public void ShowSocketFX()
	{
		if (!(m_socketFX == null) && m_socketFX.gameObject.activeInHierarchy)
		{
			m_socketFX.gameObject.SetActive(value: true);
			m_socketFX.Activate();
		}
	}

	public void HideSocketFX()
	{
		if (m_socketFX != null)
		{
			m_socketFX.Deactivate();
		}
	}

	protected void UpdateProperties(bool isMythicHero)
	{
		if (isMythicHero)
		{
			if (m_nameMythic != null)
			{
				m_nameMythic.gameObject.SetActive(value: true);
			}
			if (m_nameShadowMythic != null)
			{
				m_nameShadowMythic.gameObject.SetActive(value: true);
			}
			if (m_classIconMythic != null)
			{
				m_classIconMythic.gameObject.SetActive(value: true);
			}
			if (m_name != null)
			{
				m_name.gameObject.SetActive(value: false);
				m_name = m_nameMythic;
			}
			if (m_nameShadow != null)
			{
				m_nameShadow.gameObject.SetActive(value: false);
				m_nameShadow = m_nameShadowMythic;
			}
			if (m_classIcon != null)
			{
				m_classIcon.gameObject.SetActive(value: false);
				m_classIcon = m_classIconMythic;
			}
		}
	}
}
