using System.Text;
using Hearthstone;
using UnityEngine;

public abstract class BaconBaseSkinInfoManager : BaseHeroSkinInfoManager
{
	public GameObject m_DebugTextWrapper;

	public UberText m_DebugText;

	public UberText m_RarityText;

	public override void EnterPreview(CollectionCardVisual cardVisual)
	{
		base.EnterPreview(cardVisual);
		SetSkinRarity();
		if (m_DebugTextWrapper != null)
		{
			if (HearthstoneApplication.IsInternal() && m_DebugText != null && Options.Get().GetBool(Option.DEBUG_SHOW_BATTLEGROUND_SKIN_IDS))
			{
				StringBuilder stringBuilder = new StringBuilder();
				AppendDebugTextForCurrentCard(stringBuilder);
				m_DebugText.Text = stringBuilder.ToString();
				m_DebugTextWrapper.SetActive(value: true);
			}
			else
			{
				m_DebugTextWrapper.SetActive(value: false);
			}
		}
	}

	protected virtual void AppendDebugTextForCurrentCard(StringBuilder builder)
	{
		builder.Append("Card Id: ");
		if (m_currentEntityDef != null)
		{
			builder.AppendLine();
			builder.Append(m_currentEntityDef.GetCardId());
			builder.AppendLine();
		}
		else
		{
			builder.Append("UNKNOWN");
		}
		builder.AppendLine();
	}

	private void SetSkinRarity()
	{
		if (m_RarityText == null)
		{
			Error.AddDevWarning("RarityText Reference Not Set", "The RarityText Reference Object was not set!");
		}
		else if (m_currentEntityDef == null)
		{
			Debug.LogError("Error [BaconBaseSkinInfoManager] SetSkinRarity currentEntity was null!");
		}
		else if (m_currentEntityDef.GetRarity() == TAG_RARITY.INVALID)
		{
			Debug.LogWarning("Warning [BaconBaseSkinInfoManager] SetSkinRarity rarity tag was INVALID!");
			m_RarityText.Text = "";
		}
		else
		{
			m_RarityText.Text = GameStrings.GetRarityText(m_currentEntityDef.GetRarity());
		}
	}
}
