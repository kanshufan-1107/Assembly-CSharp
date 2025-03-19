using System.Collections.Generic;

public class MultiColumnTooltipPanel : ResizableTooltipPanel
{
	public List<UberText> m_textColumns = new List<UberText>();

	public override void Initialize(string keywordName, string keywordText)
	{
		base.Initialize(keywordName, keywordText);
		float maxColumnHeight = 0f;
		foreach (UberText uberText in m_textColumns)
		{
			if (uberText != null && uberText.Height > maxColumnHeight)
			{
				maxColumnHeight = uberText.Height;
			}
		}
		float totalHeight = (m_name.Height + m_bodyTextHeight + maxColumnHeight) * m_heightPadding;
		SetBackgroundSize(totalHeight);
	}
}
