using Hearthstone.UI;
using UnityEngine;

public class ShopSubTab : ShopTabBase
{
	[SerializeField]
	[Header("Sub-Tab Components")]
	private UIBButton m_button;

	[SerializeField]
	private Collider m_collider;

	[SerializeField]
	private Clickable m_hoverStateClickable;

	public override void Hide()
	{
		base.Hide();
		m_button.Deselect();
	}

	public override void Select()
	{
		base.Select();
		m_button.Select();
	}

	public override void Deselect()
	{
		base.Deselect();
		m_button.Deselect();
	}

	protected override void OnBlockChanged(bool blocked)
	{
		base.OnBlockChanged(blocked);
		if (m_collider != null)
		{
			m_collider.enabled = !blocked;
		}
		if (m_hoverStateClickable != null)
		{
			m_hoverStateClickable.Active = !blocked;
		}
	}

	protected override void GetTooltipData(out string title, out string body)
	{
		title = string.Empty;
		body = string.Empty;
	}
}
