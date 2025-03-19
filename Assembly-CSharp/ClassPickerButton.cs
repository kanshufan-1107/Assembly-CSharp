using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ClassPickerButton : HeroPickerButton
{
	public GameObject m_questBang;

	private AssetHandle<Texture> m_portraitTexture;

	public override void UpdateDisplay(DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		m_heroClass = ((def?.EntityDef != null) ? def.EntityDef.GetClass() : TAG_CLASS.INVALID);
		base.UpdateDisplay(def, premium);
		SetClassname(GameStrings.GetClassName(m_heroClass));
		SetClassIcon(GetClassIconMaterial(m_heroClass));
	}

	protected override void UpdatePortrait()
	{
		if (UpdateLegendaryHeroPortrait() || m_fullDef?.CardDef == null)
		{
			return;
		}
		AssetHandle.Set(ref m_portraitTexture, m_fullDef.CardDef.GetPortraitTextureHandle());
		if (!m_portraitTexture)
		{
			return;
		}
		Material goldenMaterial = m_fullDef.CardDef.GetPremiumClassMaterial();
		DeckPickerHero deckPickerHero = GetComponent<DeckPickerHero>();
		Renderer component = m_buttonFrame.GetComponent<Renderer>();
		List<Material> mats = component.GetMaterials();
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.FRIENDLY:
			FriendChallengeMgr.Get().IsChallengeTavernBrawl();
			break;
		default:
			_ = 0;
			break;
		case SceneMgr.Mode.TAVERN_BRAWL:
			_ = 1;
			break;
		}
		bool isLockedForTwist = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST && RankMgr.IsClassLockedForTwist(m_heroClass);
		if (!GameUtils.HasUnlockedClass(m_heroClass) || isLockedForTwist)
		{
			if (m_fullDef.CardDef.m_LockedClassPortrait != null)
			{
				mats[deckPickerHero.m_PortraitMaterialIndex] = m_fullDef.CardDef.m_LockedClassPortrait;
			}
			else
			{
				TAG_CLASS heroClass = m_fullDef.EntityDef.GetClass();
				if (!GameUtils.IsVanillaHero(m_fullDef.EntityDef.GetCardId()) && heroClass != TAG_CLASS.WHIZBANG)
				{
					string heroCardID = CollectionManager.GetVanillaHero(heroClass);
					CollectionManager.Get().GetHeroPremium(heroClass);
					DefLoader.Get().LoadFullDef(heroCardID, OnHeroFullDefLoaded);
				}
			}
		}
		else if (m_premium == TAG_PREMIUM.GOLDEN && goldenMaterial != null)
		{
			mats[deckPickerHero.m_PortraitMaterialIndex] = goldenMaterial;
			if (!m_seed.HasValue)
			{
				m_seed = Random.value;
			}
			if (mats[deckPickerHero.m_PortraitMaterialIndex].HasProperty("_Seed"))
			{
				mats[deckPickerHero.m_PortraitMaterialIndex].SetFloat("_Seed", m_seed.Value);
			}
			UberShaderAnimation portraitAnimation = m_fullDef.CardDef.GetPortraitAnimation(m_premium);
			if (portraitAnimation != null)
			{
				UberShaderController uberController = m_buttonFrame.GetComponent<UberShaderController>();
				if (uberController == null)
				{
					uberController = m_buttonFrame.AddComponent<UberShaderController>();
				}
				uberController.UberShaderAnimation = Object.Instantiate(portraitAnimation);
				uberController.m_MaterialIndex = deckPickerHero.m_PortraitMaterialIndex;
			}
		}
		else
		{
			Material cachedMaterial = GetCachedMaterial(deckPickerHero.m_PortraitMaterialIndex);
			if (cachedMaterial != null)
			{
				mats[deckPickerHero.m_PortraitMaterialIndex] = Object.Instantiate(cachedMaterial);
			}
			mats[deckPickerHero.m_PortraitMaterialIndex].mainTexture = m_portraitTexture;
		}
		component.SetMaterials(mats);
	}

	private void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		using (fullDef)
		{
			SetFullDef(fullDef);
		}
	}

	public override void Lock()
	{
		base.Lock();
		ShowQuestBang(shown: true);
		m_heroClassIcon.SetActive(value: false);
		m_heroClassIconSepia.SetActive(value: true);
	}

	public override void Unlock()
	{
		bool num = IsLocked();
		base.Unlock();
		if (num)
		{
			UpdatePortrait();
		}
		ShowQuestBang(shown: false);
		m_heroClassIcon.SetActive(value: true);
		m_heroClassIconSepia.SetActive(value: false);
	}

	public void ShowQuestBang(bool shown)
	{
		m_questBang.SetActive(shown);
	}

	protected override void OnDestroy()
	{
		AssetHandle.SafeDispose(ref m_portraitTexture);
		base.OnDestroy();
	}
}
