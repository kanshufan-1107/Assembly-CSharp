using Hearthstone.UI.Core;
using UnityEngine;

public class UtilityTextComponent : MonoBehaviour
{
	[SerializeField]
	private UberText m_uberText;

	[Header("Updatable Components")]
	[SerializeField]
	private MultiSliceElement m_multiSlice;

	[SerializeField]
	private ContentFitter m_contentFitter;

	[Overridable]
	public string Text
	{
		set
		{
			SetText(value);
		}
	}

	public void SetText(string text)
	{
		if (m_uberText == null)
		{
			Log.UIFramework.PrintError("No valid text component in UtilityTextComponent - " + base.gameObject.name);
			return;
		}
		m_uberText.Text = text;
		if ((bool)m_multiSlice)
		{
			m_multiSlice.UpdateSlices();
		}
		if ((bool)m_contentFitter)
		{
			m_contentFitter.Refresh();
		}
	}
}
