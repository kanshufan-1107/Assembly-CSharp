using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class AdjustBuyingAmount : MonoBehaviour
{
	[Serializable]
	private struct HoldSpeedInterval
	{
		public float startTime;

		public float holdSpeed;
	}

	private const string CLICK_SOUND = "Click Sound";

	[SerializeField]
	private Clickable m_incrementClickable;

	[SerializeField]
	private Clickable m_decrementClickable;

	[SerializeField]
	private UberText m_buyingAmountText;

	[SerializeField]
	private List<HoldSpeedInterval> m_holdSpeedIntervals;

	private ProductPage m_productPage;

	private PlayMakerFSM m_clickSoundPlaymaker;

	private int m_buyingAmount = 1;

	private int m_minimumBuyingAmount = 1;

	private int m_maximumBuyingAmount = 1;

	private Coroutine m_coroutine;

	private int m_currentIntervalIndex = -1;

	private float m_timeBeforeLastTick;

	private float m_timeBeforeLastIndexChange;

	private string m_currentVariantName;

	private void Awake()
	{
		m_productPage = GetComponentInParent<ProductPage>();
		if (m_productPage == null)
		{
			Log.Store.PrintError("AdjustBuyingAmount - Product Page null in Awake. Not a child of a product page?");
			return;
		}
		m_clickSoundPlaymaker = GetComponent<PlayMakerFSM>();
		if (m_clickSoundPlaymaker == null)
		{
			Log.Store.PrintError("AdjustBuyingAmount - Playmaker object is null. Cannot play click feedback sound.");
		}
		m_productPage.OnProductVariantSet += OnProductChanged;
		m_productPage.OnClosed += OnProductPageClosed;
		m_incrementClickable.AddEventListener(UIEventType.PRESS, delegate
		{
			OnAdjustAmountButtonClicked(1);
		});
		m_decrementClickable.AddEventListener(UIEventType.PRESS, delegate
		{
			OnAdjustAmountButtonClicked(-1);
		});
		m_incrementClickable.AddEventListener(UIEventType.RELEASE, delegate
		{
			StopAdjustingAmount();
		});
		m_decrementClickable.AddEventListener(UIEventType.RELEASE, delegate
		{
			StopAdjustingAmount();
		});
		m_incrementClickable.AddEventListener(UIEventType.ROLLOUT, delegate
		{
			StopAdjustingAmount();
		});
		m_decrementClickable.AddEventListener(UIEventType.ROLLOUT, delegate
		{
			StopAdjustingAmount();
		});
		RefreshQuantity();
	}

	private void OnDestroy()
	{
		if (m_coroutine != null)
		{
			StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
	}

	private void RefreshQuantity()
	{
		ProductDataModel variant = m_productPage.GetSelectedVariant();
		m_minimumBuyingAmount = 1;
		m_maximumBuyingAmount = variant.GetMaxBulkPurchaseCount();
		m_buyingAmount = Mathf.Clamp(m_productPage.GetVariantQuantityByIndex(m_productPage.GetSelectedVariantIndex()), m_minimumBuyingAmount, m_maximumBuyingAmount);
		m_productPage.SetVariantQuantityAndUpdateDataModel(variant, m_buyingAmount);
		RefreshVisuals();
	}

	private void RefreshVisuals()
	{
		if (m_incrementClickable != null)
		{
			m_incrementClickable.gameObject.SetActive(m_buyingAmount < m_maximumBuyingAmount);
		}
		if (m_decrementClickable != null)
		{
			m_decrementClickable.gameObject.SetActive(m_buyingAmount > m_minimumBuyingAmount);
		}
		m_buyingAmountText.SetText(m_buyingAmount.ToString());
	}

	private void OnProductChanged(ProductSelectionDataModel selection)
	{
		if (selection != null && selection.Variant != null)
		{
			if (m_currentVariantName == selection.Variant.Name && selection.Quantity != m_buyingAmount)
			{
				m_buyingAmount = selection.Quantity;
			}
			m_currentVariantName = selection.Variant.Name;
			RefreshQuantity();
		}
	}

	private void OnProductPageClosed(object sender, EventArgs e)
	{
		StopAdjustingAmount();
		m_currentVariantName = null;
	}

	private void OnAdjustAmountButtonClicked(int direction)
	{
		if (m_clickSoundPlaymaker != null)
		{
			m_clickSoundPlaymaker.SendEvent("Click Sound");
		}
		StartAdjustingAmount(direction);
	}

	private void StartAdjustingAmount(int direction)
	{
		if (m_coroutine == null)
		{
			m_coroutine = StartCoroutine(AdjustBuyingAmountCoroutine(m_productPage.GetSelectedVariant(), direction));
		}
		RefreshVisuals();
	}

	private void StopAdjustingAmount()
	{
		if (m_coroutine != null)
		{
			m_currentIntervalIndex = -1;
			m_timeBeforeLastTick = 0f;
			m_timeBeforeLastIndexChange = 0f;
			StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
	}

	private IEnumerator AdjustBuyingAmountCoroutine(ProductDataModel variant, int direction)
	{
		if (variant == null)
		{
			Log.Store.PrintError("AdjustBuyingAmount - ProductDataModel variant is null");
			StopAdjustingAmount();
			yield break;
		}
		TickQuantity(variant, direction);
		while (true)
		{
			if (m_holdSpeedIntervals.Count > 0)
			{
				HoldSpeedInterval? currentInterval = null;
				if (m_currentIntervalIndex >= 0 && m_currentIntervalIndex < m_holdSpeedIntervals.Count)
				{
					currentInterval = m_holdSpeedIntervals[m_currentIntervalIndex];
				}
				int nextIntervalIndex = m_currentIntervalIndex + 1;
				if (nextIntervalIndex < m_holdSpeedIntervals.Count)
				{
					m_timeBeforeLastIndexChange += Time.deltaTime;
					HoldSpeedInterval nextInterval = m_holdSpeedIntervals[nextIntervalIndex];
					if (m_timeBeforeLastIndexChange >= nextInterval.startTime)
					{
						m_currentIntervalIndex++;
						currentInterval = nextInterval;
						m_timeBeforeLastIndexChange = 0f;
					}
				}
				m_timeBeforeLastTick += Time.deltaTime;
				if (currentInterval.HasValue && m_timeBeforeLastTick >= currentInterval.Value.holdSpeed)
				{
					TickQuantity(variant, direction);
					m_timeBeforeLastTick = 0f;
				}
			}
			yield return null;
		}
	}

	private void TickQuantity(ProductDataModel variant, int direction)
	{
		m_buyingAmount += direction;
		if (m_buyingAmount < m_minimumBuyingAmount || m_buyingAmount > m_maximumBuyingAmount)
		{
			m_buyingAmount = Mathf.Clamp(m_buyingAmount, m_minimumBuyingAmount, m_maximumBuyingAmount);
			StopAdjustingAmount();
		}
		if (m_productPage != null)
		{
			m_productPage.SetVariantQuantityAndUpdateDataModel(variant, m_buyingAmount);
		}
		else
		{
			StopAdjustingAmount();
		}
		RefreshVisuals();
	}
}
