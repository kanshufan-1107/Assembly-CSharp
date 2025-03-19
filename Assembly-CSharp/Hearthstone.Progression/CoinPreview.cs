using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class CoinPreview : MonoBehaviour
{
	public UIBButton m_favoriteButton;

	public GameObject m_cardTargetTransformMarker;

	public GameObject m_cardDisplay;

	public Clickable m_clickable;

	private WidgetTemplate m_widget;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private Transform m_startTransform;

	private bool m_hidingAnimTriggered;

	private int? m_currentCoinId = -1;

	private CardDataModel m_coinCardDatamodel;

	private const string CODE_HIDE = "CODE_HIDE_COIN_PREVIEW";

	private const string START_HIDE = "START_HIDE_COIN_PREVIEW";

	private const string START_SHOW = "START_SHOW_COIN_PREVIEW";

	private void Awake()
	{
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	public void EnterPreviewWhenReady(CardDataModel cardDataModel, int coinId, Transform startTransform)
	{
		m_startTransform = startTransform;
		m_currentCoinId = coinId;
		m_coinCardDatamodel = cardDataModel;
		UpdateView();
		CosmeticCoinManager.Get().OnFavoriteCoinsChanged += OnFavoriteCoinsChanged;
		m_widget.Hide();
		m_hidingAnimTriggered = false;
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (!m_widget.IsReady || m_widget.IsChangingStates)
		{
			yield return null;
		}
		if (m_startTransform != null)
		{
			m_cardTargetTransformMarker.transform.position = m_cardDisplay.transform.position;
			TransformUtil.SetWorldScale(m_cardTargetTransformMarker, m_cardDisplay.transform.lossyScale);
			m_cardDisplay.transform.position = m_startTransform.position;
			TransformUtil.SetWorldScale(m_cardDisplay, m_startTransform.lossyScale);
		}
		m_widget.TriggerEvent("START_SHOW_COIN_PREVIEW");
		m_widget.Show();
	}

	private void UpdateView()
	{
		UpdateCoinCard(m_coinCardDatamodel);
		CosmeticCoinManager coinManager = CosmeticCoinManager.Get();
		bool num = coinManager.GetCoinsOwned()?.Contains(m_currentCoinId.Value) ?? false;
		bool coinFavorited = coinManager.IsFavoriteCoin(m_currentCoinId.Value);
		if (!num || (coinManager.GetTotalFavoriteCoins() == 1 && coinFavorited))
		{
			m_favoriteButton.Flip(faceUp: false, forceImmediate: true);
			m_favoriteButton.SetEnabled(enabled: false);
			m_favoriteButton.m_WiggleAmount = Vector3.zero;
		}
		else
		{
			m_favoriteButton.AddEventListener(UIEventType.RELEASE, OnFavoriteButtonClicked);
		}
	}

	public void OnFavoriteCoinsChanged(int coinId, bool isFavorite)
	{
		UpdateView();
	}

	public void OnFinishHideAnim()
	{
		CosmeticCoinManager.Get().OnFavoriteCoinsChanged -= OnFavoriteCoinsChanged;
		UIContext.GetRoot().DismissPopup(base.gameObject);
		if (m_widget != null)
		{
			m_widget.Hide();
		}
		Object.Destroy(base.transform.parent.gameObject);
	}

	private void UpdateCoinCard(CardDataModel cardDataModel)
	{
		m_widget.BindDataModel(cardDataModel);
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "CODE_HIDE_COIN_PREVIEW")
		{
			BeginHideAnim();
		}
	}

	private void OnFavoriteButtonClicked(object e)
	{
		CosmeticCoinManager cosmeticCoinManager = CosmeticCoinManager.Get();
		cosmeticCoinManager.RequestSetFavoriteCosmeticCoin(isFavorite: !cosmeticCoinManager.IsFavoriteCoin(m_currentCoinId.Value), newFavoriteCoinID: m_currentCoinId.Value);
		BeginHideAnim();
	}

	private void BeginHideAnim()
	{
		if (!m_hidingAnimTriggered)
		{
			m_widget?.TriggerEvent("START_HIDE_COIN_PREVIEW");
			m_hidingAnimTriggered = true;
		}
	}
}
