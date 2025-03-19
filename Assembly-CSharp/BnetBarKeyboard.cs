using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class BnetBarKeyboard : PegUIElement
{
	public Color m_highlight;

	public Color m_origColor;

	private List<OnKeyboardPressed> m_keyboardPressedListeners = new List<OnKeyboardPressed>();

	public void ShowHighlight(bool show)
	{
		Color color = m_origColor;
		if (show)
		{
			color = m_highlight;
		}
		base.gameObject.GetComponent<Renderer>().GetMaterial().SetColor("_Color", color);
	}

	protected override void OnPress()
	{
		ServiceManager.Get<ITouchScreenService>().ShowKeyboard();
		OnKeyboardPressed[] array = m_keyboardPressedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		ShowHighlight(show: true);
	}

	protected override void OnOut(InteractionState oldState)
	{
		ShowHighlight(show: false);
	}

	public void RegisterKeyboardPressedListener(OnKeyboardPressed listener)
	{
		if (!m_keyboardPressedListeners.Contains(listener))
		{
			m_keyboardPressedListeners.Add(listener);
		}
	}

	public void UnregisterKeyboardPressedListener(OnKeyboardPressed listener)
	{
		m_keyboardPressedListeners.Remove(listener);
	}
}
