using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ShopMultiPortraitHeroSkin : MonoBehaviour
{
	[Serializable]
	public class DualPortraitSkin
	{
		public string m_key;

		public List<string> m_cardIds;
	}

	[SerializeField]
	private float m_heroPortraitSwapDelay;

	[SerializeField]
	private List<DualPortraitSkin> m_dualPortraitSkins;

	[SerializeField]
	private ShopSwipeAnim m_shopSwipe;

	private Widget m_widget;

	private ProductDataModel m_product;

	private CardDataModel m_card;

	private Coroutine m_swapHeroCoroutine;

	private List<string> m_cardIds = new List<string>();

	private const string HeroSkinChangedEvent = "HERO_SKIN_CHANGED";

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(HandleWidgetEvent);
		}
		InitDataModels();
		if (ValidateHeroSkins())
		{
			InitAnimation();
		}
	}

	private void OnDestroy()
	{
		if (m_widget != null)
		{
			m_widget.RemoveEventListener(HandleWidgetEvent);
		}
	}

	private void OnDisable()
	{
		if (m_widget != null)
		{
			m_widget.UnbindDataModel(27);
		}
		ResetAnim();
	}

	private void HandleWidgetEvent(string eventName)
	{
		if (eventName == "HERO_SKIN_CHANGED")
		{
			InitDataModels();
			if (ValidateHeroSkins())
			{
				InitAnimation();
			}
		}
	}

	private void InitDataModels()
	{
		if (m_widget == null)
		{
			return;
		}
		m_product = m_widget.GetDataModel<ProductDataModel>();
		if (m_product == null || m_product.Items == null || m_product.Items.Count == 0 || !m_product.Tags.Contains("shop_swipe"))
		{
			return;
		}
		foreach (RewardItemDataModel item in m_product.Items)
		{
			if (item != null && item.ItemType == RewardItemType.HERO_SKIN && item.Card != null)
			{
				m_card = CreateDeepCopy(item.Card);
				m_widget.BindDataModel(m_card);
				break;
			}
		}
	}

	private void InitAnimation()
	{
		if (m_shopSwipe != null)
		{
			m_shopSwipe.InitAnimation(m_product);
			m_shopSwipe.ResetAnim();
		}
		PlayAnim();
	}

	private void ResetAnim()
	{
		if (m_shopSwipe != null)
		{
			m_shopSwipe.ResetAnim();
		}
		if (m_swapHeroCoroutine != null)
		{
			StopCoroutine(m_swapHeroCoroutine);
			m_swapHeroCoroutine = null;
		}
	}

	private void PlayAnim()
	{
		if (m_shopSwipe != null && m_swapHeroCoroutine == null && base.gameObject.activeInHierarchy)
		{
			m_swapHeroCoroutine = StartCoroutine(SwapHeroPortraitAnim());
		}
	}

	private CardDataModel CreateDeepCopy(CardDataModel model)
	{
		if (model == null)
		{
			return new CardDataModel();
		}
		return new CardDataModel
		{
			CardId = model.CardId,
			Premium = model.Premium,
			FlavorText = model.FlavorText,
			Name = model.Name,
			Owned = model.Owned,
			Rarity = model.Rarity,
			Class = model.Class,
			IsShopPremiumHeroSkin = model.IsShopPremiumHeroSkin,
			CardText = model.CardText
		};
	}

	private bool ValidateHeroSkins()
	{
		if (TryLoadCyclingHeroSkins(out var cardIds))
		{
			foreach (string card in cardIds)
			{
				if (ValidateHeroSkin(card))
				{
					m_cardIds.Add(card);
				}
			}
		}
		return m_cardIds.Count > 0;
	}

	private bool TryLoadCyclingHeroSkins(out List<string> cardIds)
	{
		cardIds = null;
		if (m_product == null)
		{
			return false;
		}
		if (!m_product.Tags.Contains("illidan") || !TryGetCyclingCardIds("illidan", out cardIds))
		{
			return false;
		}
		return true;
	}

	private bool ValidateHeroSkin(string cardId)
	{
		if (string.IsNullOrEmpty(cardId) || GameDbf.GetIndex().GetCardRecord(cardId) == null)
		{
			return false;
		}
		return DefLoader.Get().GetEntityDef(cardId)?.IsHeroSkin() ?? false;
	}

	private bool TryGetCyclingCardIds(string key, out List<string> cardIds)
	{
		cardIds = null;
		foreach (DualPortraitSkin skin in m_dualPortraitSkins)
		{
			if (skin.m_key == key)
			{
				cardIds = skin.m_cardIds;
				return true;
			}
		}
		return false;
	}

	private IEnumerator SwapHeroPortraitAnim()
	{
		if (m_shopSwipe == null || !m_shopSwipe.IsReady)
		{
			yield break;
		}
		WaitForSeconds swapDelay = new WaitForSeconds(m_heroPortraitSwapDelay);
		int currentIndex = 0;
		while (base.gameObject.activeInHierarchy)
		{
			yield return StartCoroutine(m_shopSwipe.PlayShopSwipeWithDelay());
			if (m_card != null && m_cardIds != null && m_cardIds.Count > 0)
			{
				yield return swapDelay;
				m_card.CardId = m_cardIds[currentIndex];
				currentIndex = (currentIndex + 1) % m_cardIds.Count;
			}
		}
	}
}
