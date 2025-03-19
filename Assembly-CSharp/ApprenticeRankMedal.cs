using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

public class ApprenticeRankMedal : MonoBehaviour
{
	public MeshRenderer m_rankedMedalMeshRenderer;

	private Widget m_rankMedalWidgetInstance;

	private DefLoader.DisposableCardDef m_cardDef;

	private const string FOUL_EGG_CARD_ID = "RLK_833";

	public void Awake()
	{
		m_rankMedalWidgetInstance = base.gameObject.GetComponent<Widget>();
		if (m_rankMedalWidgetInstance != null)
		{
			m_rankMedalWidgetInstance.RegisterEventListener(UIEventHandler);
		}
	}

	public void OnDestroy()
	{
		if (m_rankMedalWidgetInstance != null)
		{
			m_rankMedalWidgetInstance.RemoveEventListener(UIEventHandler);
		}
		m_cardDef?.Dispose();
		m_cardDef = null;
	}

	private void UIEventHandler(string eventName)
	{
		if (eventName == "CODE_SHOW_APPRENTICE")
		{
			ShowApprenticeMedal();
		}
	}

	private void ShowApprenticeMedal()
	{
		m_cardDef?.Dispose();
		m_cardDef = DefLoader.Get().GetCardDef("RLK_833");
		if (m_rankedMedalMeshRenderer != null && m_cardDef != null)
		{
			Texture portraitMaterial = m_cardDef.CardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL);
			m_rankedMedalMeshRenderer.GetMaterial(0).SetTexture("_MainTex", portraitMaterial);
		}
	}
}
