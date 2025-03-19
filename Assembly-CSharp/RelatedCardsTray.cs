using System;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RelatedCardsTray : MonoBehaviour
{
	public GameObject m_visualListParentObject;

	public Widget m_relatedCardsWidget;

	public Transform m_openBone;

	public Transform m_closedBone;

	public UberText m_trayTitle;

	private DataModelList<CardTileDataModel> m_cardList = new DataModelList<CardTileDataModel>();

	private CardDataModel m_selectedCard = new CardDataModel
	{
		CardId = ""
	};

	private UIBScrollable m_scrollbar;

	private Maskable m_maskable;

	private Listable m_listable;

	private bool m_open;

	private float m_openAmount;

	private const float SIZE_MOBILE = 0.7f;

	private const float SIZE_PC = 0.28f;

	private const float SPACE_BETWEEN_TILES = 3.75f;

	private void Start()
	{
		UpdateTransform();
		string trayTitle = GameStrings.Get("GLUE_COLLECTION_RELATED_CARDS");
		m_trayTitle.Text = trayTitle;
		BindDataModel();
		m_relatedCardsWidget.SetLayerOverride(GameLayer.IgnoreFullScreenEffects);
		m_relatedCardsWidget.RegisterReadyListener(OnWidgetReady);
	}

	private void Update()
	{
		float speed = 3f;
		float increment = Time.deltaTime * speed;
		if (m_open && IsListReady())
		{
			m_openAmount = Mathf.Clamp01(m_openAmount + increment);
		}
		else
		{
			m_openAmount = Mathf.Clamp01(m_openAmount - increment);
		}
		UpdateTransform();
	}

	private void UpdateTransform()
	{
		m_visualListParentObject.transform.localPosition = EaseOutBackEasingFunction(m_closedBone.localPosition, m_openBone.localPosition, m_openAmount);
		m_visualListParentObject.transform.localScale = EaseOutBackEasingFunction(m_closedBone.localScale, m_openBone.localScale, m_openAmount);
	}

	public void ShowTray()
	{
		m_open = true;
		m_openAmount = 0f;
		base.gameObject.SetActive(value: true);
		UpdateTransform();
		m_relatedCardsWidget.RegisterEventListener(HandleMouseEvents);
	}

	public void HideTray()
	{
		m_open = false;
		m_openAmount = 0f;
		base.gameObject.SetActive(value: false);
		UpdateTransform();
		m_relatedCardsWidget.RemoveEventListener(HandleMouseEvents);
		ClearAllCards();
		if (m_scrollbar != null)
		{
			m_scrollbar.SetScrollImmediate(0f);
		}
	}

	public void AddCard(string cardID, TAG_PREMIUM premium, bool offsetCardNameForRunes)
	{
		using (DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardID))
		{
			if (!fullDef.EntityDef.HasTag(GAME_TAG.HAS_DIAMOND_QUALITY) && premium == TAG_PREMIUM.DIAMOND)
			{
				premium = TAG_PREMIUM.SIGNATURE;
			}
			if (!fullDef.EntityDef.HasTag(GAME_TAG.HAS_SIGNATURE_QUALITY) && premium == TAG_PREMIUM.SIGNATURE)
			{
				premium = TAG_PREMIUM.GOLDEN;
			}
		}
		CardTileDataModel newTile = new CardTileDataModel
		{
			CardId = cardID,
			Count = 1,
			Premium = premium
		};
		m_cardList.Add(newTile);
	}

	private void ClearBigCard()
	{
		m_selectedCard.CardId = "";
		m_selectedCard.Premium = TAG_PREMIUM.NORMAL;
	}

	private void BindDataModel()
	{
		RelatedCardsDetailsDataModel dataModel = new RelatedCardsDetailsDataModel
		{
			CardTiles = m_cardList,
			SelectedCard = m_selectedCard
		};
		m_relatedCardsWidget.BindDataModel(dataModel);
	}

	public void ClearAllCards()
	{
		m_cardList.Clear();
	}

	private void HandleMouseEvents(string eventName)
	{
		if (m_scrollbar == null || m_scrollbar.IsTouchDragging())
		{
			ClearBigCard();
		}
		else if (!(eventName == "TILE_OVER_code"))
		{
			if (eventName == "TILE_OUT_code")
			{
				ClearBigCard();
			}
		}
		else
		{
			CardTileDataModel cardDataModel = m_relatedCardsWidget.GetDataModel<EventDataModel>().Payload as CardTileDataModel;
			m_selectedCard.CardId = cardDataModel.CardId;
			m_selectedCard.Premium = cardDataModel.Premium;
		}
	}

	private void OnWidgetReady(object unused)
	{
		m_scrollbar = GetComponentInChildren<UIBScrollable>(includeInactive: true);
		m_maskable = GetComponentInChildren<Maskable>(includeInactive: true);
		m_listable = GetComponentInChildren<Listable>(includeInactive: true);
		m_maskable.UpdateCameraClipping();
	}

	private bool IsListReady()
	{
		if (m_listable == null)
		{
			return false;
		}
		return m_listable.transform.childCount == m_cardList.Count;
	}

	private static Vector3 SpringEasingFunction(Vector3 start, Vector3 end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * (float)Math.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + 1.2f * (1f - value));
		return start + (end - start) * value;
	}

	private static Vector3 EaseOutBackEasingFunction(Vector3 start, Vector3 end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value = value / 1f - 1f;
		return end * (value * value * ((s + 1f) * value + s) + 1f) + start;
	}
}
