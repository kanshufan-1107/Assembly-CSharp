using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class DeckCopyPasteButton : PegUIElement
{
	public UberText ButtonText;

	public string TooltipMessage { get; set; }

	public string TooltipHeaderString { get; set; }

	private void Start()
	{
		AddEventListener(UIEventType.ROLLOVER, OnButtonOver);
		AddEventListener(UIEventType.ROLLOUT, OnButtonOut);
	}

	private void OnButtonOver(UIEvent e)
	{
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (!(tooltipZone == null) && !string.IsNullOrEmpty(TooltipMessage))
		{
			tooltipZone.ShowTooltip(TooltipHeaderString, TooltipMessage, 4f);
		}
	}

	private void OnButtonOut(UIEvent e)
	{
		TooltipZone tooltipZone = GetComponent<TooltipZone>();
		if (tooltipZone != null)
		{
			tooltipZone.HideTooltip();
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
