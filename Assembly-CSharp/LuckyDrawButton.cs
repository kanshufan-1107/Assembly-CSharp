using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class LuckyDrawButton : MonoBehaviour
{
	[SerializeField]
	private Clickable m_luckyDrawButtonClickable;

	private Widget m_widget;

	private VisualController m_visualController;

	private const string ENABLED = "ENABLED";

	private const string DISABLED = "DISABLED";

	private const string SHOWHIGHLIGHT = "SHOW_BLUE_HIGHLIGHT_RING_CODE";

	private const string HIDEHIGHLIGHT = "HIDE_BLUE_HIGHLIGHT_RING_CODE";

	private TooltipZone m_tooltipZone;

	[SerializeField]
	private float m_toolTipScale = 6f;

	private void Awake()
	{
		m_tooltipZone = GetComponent<TooltipZone>();
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			LuckyDrawManager.Get().LogError("Error: [LuckyDrawButton] No Component of type WidgetTemplate found on " + base.gameObject.name + " cannot instantiate LuckyDrawButton");
			return;
		}
		m_visualController = GetComponent<VisualController>();
		StartCoroutine(BindLuckyDrawDataModel());
	}

	private IEnumerator BindLuckyDrawDataModel()
	{
		LuckyDrawManager luckyDrawManager = LuckyDrawManager.Get();
		while (luckyDrawManager.IsDataDirty())
		{
			yield return new WaitForSeconds(0.1f);
		}
		luckyDrawManager.BindAllLuckyDrawDataModelToWidget(m_widget);
	}

	public void SetUserInteractionEnabled(bool enabled)
	{
		m_luckyDrawButtonClickable.enabled = enabled;
	}

	public TooltipZone GetTooltipZone()
	{
		return m_tooltipZone;
	}

	public float GetToolTipScale()
	{
		return m_toolTipScale;
	}
}
