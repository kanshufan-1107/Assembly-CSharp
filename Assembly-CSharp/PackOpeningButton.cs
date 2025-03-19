using UnityEngine;

public class PackOpeningButton : BoxMenuButton
{
	public UberText m_count;

	public GameObject m_countFrame;

	public string GetGetPackCount()
	{
		return m_count.Text;
	}

	public void SetPackCount(int packs)
	{
		if (!(m_countFrame == null) && !(m_count == null))
		{
			if (packs <= 0)
			{
				m_count.Text = "";
				m_countFrame.SetActive(value: false);
			}
			else
			{
				m_countFrame.SetActive(value: true);
				m_count.Text = GameStrings.Format("GLUE_PACK_OPENING_BOOSTER_COUNT", packs);
			}
		}
	}
}
