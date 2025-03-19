using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public class ConvertFormatButton : PegUIElement
{
	[SerializeField]
	private UberText m_buttonText;

	public UberText ButtonText => m_buttonText;

	public FormatType Format { get; set; }

	private void Start()
	{
		AddEventListener(UIEventType.ROLLOVER, OnButtonOver);
		AddEventListener(UIEventType.ROLLOUT, OnButtonOut);
	}

	private void OnButtonOver(UIEvent e)
	{
		ShowTwistTooltipIfNecessary();
	}

	private void OnButtonOut(UIEvent e)
	{
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (!(tooltipZone == null))
		{
			tooltipZone.HideTooltip();
		}
	}

	private void ShowTwistTooltipIfNecessary()
	{
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (tooltipZone == null || Format != FormatType.FT_TWIST)
		{
			return;
		}
		if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			tooltipZone.ShowTooltip(GameStrings.Get("GLOBAL_TWIST_LOCKED"), GameStrings.Get("GLUE_CANNOT_CONVERT_TO_TWIST_USING_HEROIC_DECKS"), 4f);
			return;
		}
		if (!RankMgr.IsCurrentTwistSeasonActive())
		{
			tooltipZone.ShowTooltip(GameStrings.Get("GLOBAL_TWIST_LOCKED"), GameStrings.Get("GLUE_CANNOT_CONVERT_TO_TWIST_WITHOUT_SEASON"), 4f);
			return;
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && RankMgr.IsClassLockedForTwist(deck.GetClass()))
		{
			tooltipZone.ShowTooltip(GameStrings.Get("GLOBAL_TWIST_LOCKED"), GameStrings.Get("GLUE_CANNOT_CONVERT_TO_TWIST_INVALID_CLASS"), 4f);
		}
	}

	public void EnableButton(bool enable)
	{
		SetEnabled(UIEventType.PRESS, enable);
		SetEnabled(UIEventType.TAP, enable);
		SetEnabled(UIEventType.RELEASE, enable);
		GetComponent<Renderer>().GetMaterial().SetFloat("_Desaturate", (!enable) ? 1 : 0);
	}
}
