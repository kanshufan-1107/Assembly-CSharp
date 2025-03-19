using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

public class ShopHeroPowerPreview : MonoBehaviour, IPopupRendering
{
	[SerializeField]
	private Actor m_heroPowerActor;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private string m_cardId;

	[Overridable]
	public string CardId
	{
		get
		{
			return m_cardId;
		}
		set
		{
			if (!(m_cardId == value))
			{
				m_cardId = value;
				SetHeroPower();
			}
		}
	}

	private void SetHeroPower()
	{
		if (string.IsNullOrEmpty(CardId))
		{
			return;
		}
		string heroPowerID = GameUtils.GetHeroPowerCardIdFromHero(CardId);
		if (string.IsNullOrEmpty(heroPowerID))
		{
			Log.CollectionManager.PrintError("ShopHeroPowerPreview.SetHeroPower: Could not find hero power for card: " + CardId);
			return;
		}
		DefLoader.DisposableFullDef heroPowerDef = DefLoader.Get().GetFullDef(heroPowerID, new CardPortraitQuality(3, TAG_PREMIUM.NORMAL));
		if (heroPowerDef == null)
		{
			return;
		}
		m_heroPowerActor.Show();
		m_heroPowerActor.SetFullDef(heroPowerDef);
		m_heroPowerActor.SetUnlit();
		m_heroPowerActor.UpdateAllComponents();
		m_heroPowerActor.GetCostTextObject()?.SetActive(value: false);
		m_heroPowerActor.m_manaObject?.SetActive(value: false);
		HeroSkinHeroPower hshp = m_heroPowerActor.gameObject.GetComponentInChildren<HeroSkinHeroPower>();
		if (hshp == null)
		{
			Log.CollectionManager.PrintError("ShopHeroPowerPreview.SetHeroPower: Could not find HeroSkinHeroPower component in hero power preview make sure it's assigned in the inspector.");
			return;
		}
		hshp.m_Actor.AlwaysRenderPremiumPortrait = !GameUtils.IsVanillaHero(m_heroPowerActor.GetEntityDef().GetCardId());
		hshp.m_Actor.UpdateMaterials();
		hshp.m_Actor.UpdateTextures();
		if (m_popupRoot != null)
		{
			EnablePopupRendering(m_popupRoot);
		}
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		if (m_popupRoot != popupRoot)
		{
			popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents);
		}
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}
}
