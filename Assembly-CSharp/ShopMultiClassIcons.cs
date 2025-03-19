using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ShopMultiClassIcons : MonoBehaviour
{
	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private ShopSwipeAnim m_shopSwipe;

	[SerializeField]
	private float m_transitionDelay;

	[SerializeField]
	private float m_classIconSwapDelay = 0.3f;

	[SerializeField]
	private List<MeshRenderer> m_classIconRenderers;

	private List<TAG_CLASS> m_heroClasses;

	private RewardItemDataModel m_rewardItem;

	private bool m_initialized;

	private void Start()
	{
		Initialize();
		ShowClassIcons();
	}

	private void OnEnable()
	{
		Initialize();
		ShowClassIcons();
	}

	private void OnDisable()
	{
		m_initialized = false;
		if (m_shopSwipe != null)
		{
			m_shopSwipe.ResetAnim();
		}
	}

	public void Initialize()
	{
		Initialize(GetCardId());
	}

	public bool Initialize(string cardId)
	{
		if (!SetMultiClassIcons(cardId))
		{
			return false;
		}
		if (m_classIconRenderers == null || m_classIconRenderers.Count == 0)
		{
			return false;
		}
		if (m_shopSwipe != null)
		{
			m_shopSwipe.SetDefaultDelay(m_transitionDelay);
			m_shopSwipe.InitAnimation(GetProductDataModel());
			m_shopSwipe.ResetAnim();
		}
		m_initialized = true;
		return true;
	}

	public void ShowClassIcons()
	{
		if (m_initialized)
		{
			if (m_classIconRenderers.Count == 1 && m_heroClasses.Count > 1)
			{
				StartCoroutine(CycleClassIcons());
			}
			else
			{
				UpdateClassIcons();
			}
		}
	}

	private ProductDataModel GetProductDataModel()
	{
		if (m_widget == null)
		{
			return null;
		}
		ProductDataModel product = m_widget.GetDataModel<ProductDataModel>();
		if (product == null || product.Items == null || product.Items.Count == 0 || !product.Tags.Contains("shop_swipe"))
		{
			return null;
		}
		return product;
	}

	private string GetCardId()
	{
		if (m_widget == null)
		{
			return null;
		}
		m_rewardItem = m_widget.GetDataModel<RewardItemDataModel>();
		if (m_rewardItem == null || m_rewardItem.Card == null || m_rewardItem.ItemType != RewardItemType.HERO_SKIN)
		{
			return null;
		}
		return m_rewardItem.Card.CardId;
	}

	private bool SetMultiClassIcons(string cardId)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return false;
		}
		List<TAG_CLASS> multiClassList = PremiumHeroSkinUtil.GetMulticlassList(cardId);
		if (multiClassList == null || multiClassList.Count == 0)
		{
			return false;
		}
		m_heroClasses = multiClassList;
		return true;
	}

	private void UpdateClassIcons()
	{
		for (int i = 0; i < m_classIconRenderers.Count && i < m_heroClasses.Count; i++)
		{
			if (m_heroClasses[i] != 0)
			{
				SetIcon(m_heroClasses[i], i);
			}
		}
	}

	private void SetIcon(TAG_CLASS classIcon, int rendererIndex)
	{
		Vector2 textureOffset = CollectionPageManager.s_classTextureOffsets[classIcon];
		m_classIconRenderers[rendererIndex].GetComponent<Renderer>().GetMaterial().SetTextureOffset("_MainTex", textureOffset);
	}

	private IEnumerator CycleClassIcons()
	{
		if (m_shopSwipe == null || !m_shopSwipe.IsReady)
		{
			yield break;
		}
		WaitForSeconds swapDelay = new WaitForSeconds(m_classIconSwapDelay);
		int currentIndex = 0;
		while (base.gameObject.activeInHierarchy)
		{
			if (m_heroClasses != null && currentIndex < m_heroClasses.Count)
			{
				yield return swapDelay;
				SetIcon(m_heroClasses[currentIndex], 0);
				currentIndex = (currentIndex + 1) % m_heroClasses.Count;
			}
			yield return StartCoroutine(m_shopSwipe.PlayShopSwipeWithDelay());
		}
	}
}
