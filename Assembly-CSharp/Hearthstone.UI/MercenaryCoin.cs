using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

public class MercenaryCoin : MonoBehaviour
{
	public GameObject m_portraitObject;

	public int m_portraitMaterialIndex;

	private int m_mercenaryId = -1;

	private DefLoader.DisposableCardDef m_cardDef;

	[Overridable]
	public int MercenaryId
	{
		set
		{
			UpdateVisuals(value);
		}
	}

	private void OnDestroy()
	{
		m_cardDef?.Dispose();
		m_cardDef = null;
	}

	private void UpdateVisuals(int mercenaryId)
	{
		if (m_mercenaryId != mercenaryId)
		{
			MercenaryArtVariationDbfRecord artRecord = LettuceMercenary.GetDefaultArtVariationRecord(mercenaryId);
			if (artRecord != null)
			{
				m_cardDef?.Dispose();
				m_cardDef = DefLoader.Get().GetCardDef(artRecord.CardId);
				m_mercenaryId = mercenaryId;
				m_portraitObject.GetComponent<Renderer>().SetSharedMaterial(m_portraitMaterialIndex, m_cardDef.CardDef.m_MercenaryCoinPortrait);
			}
		}
	}
}
