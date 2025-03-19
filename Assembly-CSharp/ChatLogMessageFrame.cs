using UnityEngine;

public class ChatLogMessageFrame : MonoBehaviour
{
	public GameObject m_Background;

	public UberText m_Text;

	private float m_initialPadding;

	private float m_initialBackgroundHeight;

	private float m_initialBackgroundLocalScaleY;

	private void Awake()
	{
		Bounds backgroundBounds = m_Background.GetComponent<Collider>().bounds;
		Bounds textBounds = m_Text.GetTextWorldSpaceBounds();
		m_initialPadding = backgroundBounds.size.y - textBounds.size.y;
		m_initialBackgroundHeight = backgroundBounds.size.y;
		m_initialBackgroundLocalScaleY = m_Background.transform.localScale.y;
	}

	public string GetMessage()
	{
		return m_Text.Text;
	}

	public void SetMessage(string message)
	{
		m_Text.Text = message;
		UpdateText();
		UpdateBackground();
	}

	public Color GetColor()
	{
		return m_Text.TextColor;
	}

	public void SetColor(Color color)
	{
		m_Text.TextColor = color;
	}

	private void UpdateText()
	{
		m_Text.UpdateNow();
	}

	private void UpdateBackground()
	{
		float backgroundHeight = m_Text.GetTextWorldSpaceBounds().size.y + m_initialPadding;
		float scaleY = m_initialBackgroundLocalScaleY;
		if (backgroundHeight > m_initialBackgroundHeight)
		{
			scaleY *= backgroundHeight / m_initialBackgroundHeight;
		}
		TransformUtil.SetLocalScaleY(m_Background, scaleY);
	}
}
