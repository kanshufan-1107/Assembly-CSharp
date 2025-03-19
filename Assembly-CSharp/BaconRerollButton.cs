using System.Collections.Generic;
using UnityEngine;

public class BaconRerollButton : UIBButton
{
	public TooltipZone m_tooltipZone;

	public PegUIElement m_tooltipListener;

	public UberText m_costText;

	private Entity m_entity;

	private readonly PlatformDependentVector3 REROLL_BUTTON_TOOLTIP_OFFSET = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 1f, 0.8f),
		Phone = new Vector3(0f, 1f, 0.8f)
	};

	private readonly PlatformDependentVector3 REROLL_BUTTON_TOOLTIP_SCALE = new PlatformDependentVector3(PlatformCategory.Screen)
	{
		PC = new Vector3(0.6f, 1f, 0.6f),
		Phone = new Vector3(0.75f, 1f, 0.75f)
	};

	private readonly List<float> xOffsetBasedOnZonePos = new List<float> { 0.5f, 0f, 0f, -0.5f };

	public void SetEntity(Entity entity)
	{
		m_entity = entity;
	}

	public void RegisterTooltipListeners()
	{
		if (m_tooltipListener != null)
		{
			m_tooltipListener.AddEventListener(UIEventType.ROLLOVER, OnMulliganHeroRerollButtonRollOver);
			m_tooltipListener.AddEventListener(UIEventType.ROLLOUT, OnMulliganHeroRerollButtonRollOut);
		}
	}

	private void OnMulliganHeroRerollButtonRollOver(UIEvent e)
	{
		if (m_entity != null && m_tooltipZone != null)
		{
			string title;
			string description;
			switch (m_entity.ShouldEnableRerollButton(null, null))
			{
			case Entity.RerollButtonEnableResult.FREE:
			case Entity.RerollButtonEnableResult.REROLL:
			case Entity.RerollButtonEnableResult.FREE_UNLOCK:
			case Entity.RerollButtonEnableResult.UNLOCK:
				Debug.LogWarning($"Reroll Button tooltip enabled for useable reroll button state: {m_entity.ShouldEnableRerollButton(null, null)}");
				return;
			case Entity.RerollButtonEnableResult.MULLIGAN_NOT_ACTIVE:
				Debug.LogWarning("Reroll Button tooltip enabled when mulligan is not active");
				return;
			case Entity.RerollButtonEnableResult.OUT_OF_CURRENCY:
				title = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_NOT_ENOUGH_CURRENCY_HEADER";
				description = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_NOT_ENOUGH_CURRENCY_DESC";
				break;
			case Entity.RerollButtonEnableResult.HERO_REROLL_LIMITATION_REACHED:
				title = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_LIMIT_REACHED_HEADER";
				description = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_LIMIT_REACHED_DESC";
				break;
			case Entity.RerollButtonEnableResult.INSUFFICIENT_MULLIGAN_TIME_LEFT:
				title = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_INSUFFICIENT_MULLIGAN_TIME_HEADER";
				description = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_INSUFFICIENT_MULLIGAN_TIME_DESC";
				break;
			case Entity.RerollButtonEnableResult.LOCKED:
				title = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_UNAVAILABLE_HEADER";
				description = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_UNAVAILABLE_DESC";
				break;
			default:
				Debug.LogWarning($"Unsupported Reroll Button State: {m_entity.ShouldEnableRerollButton(null, null)}");
				title = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_UNAVAILABLE_HEADER";
				description = "GLUE_BACON_TOOLTIP_REROLL_BUTTON_UNAVAILABLE_DESC";
				break;
			}
			ShowRerollButtonTooltip(m_tooltipZone, GameStrings.Get(title), GameStrings.Get(description));
		}
	}

	private void OnMulliganHeroRerollButtonRollOut(UIEvent e)
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}

	public void EnableTooltipListener(bool enabled)
	{
		if (!(m_tooltipListener == null))
		{
			BoxCollider collider = m_tooltipListener.GetComponent<BoxCollider>();
			if (collider != null)
			{
				collider.gameObject.SetActive(enabled);
			}
			m_tooltipListener.SetEnabled(enabled);
		}
	}

	public void SetCostText(int rerollCost)
	{
		m_costText.SetText(rerollCost.ToString());
		m_costText.TextColor = ((rerollCost > 0) ? Color.white : Color.green);
	}

	public void DisableCostText()
	{
		m_costText.enabled = false;
	}

	private void ShowRerollButtonTooltip(TooltipZone tooltipZone, string tooltipHeader, string tooltipDescription)
	{
		TooltipPanel tooltipPanel = tooltipZone.ShowTooltip(tooltipHeader, tooltipDescription, 0.7f);
		LayerUtils.SetLayer(tooltipPanel.gameObject, GameLayer.Tooltip);
		tooltipPanel.transform.localPosition = new Vector3(0f, 0f, 1.3f);
		tooltipPanel.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		tooltipPanel.transform.localScale = REROLL_BUTTON_TOOLTIP_SCALE;
		Entity entity = m_entity;
		int zonePos = ((entity == null) ? ((int?)null) : (entity.GetCard()?.GetZonePosition() - 1)) ?? 1;
		TransformUtil.SetPoint(offset: new Vector3(REROLL_BUTTON_TOOLTIP_OFFSET.Value.x + xOffsetBasedOnZonePos[zonePos], REROLL_BUTTON_TOOLTIP_OFFSET.Value.y, REROLL_BUTTON_TOOLTIP_OFFSET.Value.z), src: tooltipPanel, srcAnchor: Anchor.BOTTOM, dst: base.gameObject, dstAnchor: Anchor.TOP);
	}
}
