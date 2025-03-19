using UnityEngine;

public class SocialToast : MonoBehaviour
{
	[SerializeField]
	private UberText m_text;

	public void SetText(string text)
	{
		m_text.Text = text;
		ThreeSliceElement threeSlice = GetComponent<ThreeSliceElement>();
		if (threeSlice != null)
		{
			float textWidth = m_text.GetTextWorldSpaceBounds().size.x;
			float sideWidth = (threeSlice.GetLeftSize().x + threeSlice.GetRightSize().x) * 0.5f;
			threeSlice.SetWidth(textWidth + sideWidth);
		}
	}
}
