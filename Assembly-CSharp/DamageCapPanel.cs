using UnityEngine;

public class DamageCapPanel : MonoBehaviour
{
	public UberText m_text;

	public void SetText(string text)
	{
		if (text == "")
		{
			ClearText();
		}
		else
		{
			m_text.gameObject.SetActive(value: true);
		}
		m_text.Text = text;
	}

	public void ClearText()
	{
		m_text.Text = "";
		m_text.gameObject.SetActive(value: false);
	}
}
