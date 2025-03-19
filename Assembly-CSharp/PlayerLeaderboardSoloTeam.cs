using UnityEngine;

internal class PlayerLeaderboardSoloTeam : PlayerLeaderboardTeam
{
	[Header("Solo Team Items")]
	[SerializeField]
	private GameObject m_IconThird;

	[SerializeField]
	private GameObject m_IconFourth;

	public override void SetPlaceIcon(int currentPlace)
	{
		m_IconFirst.SetActive(value: false);
		m_IconSecond.SetActive(value: false);
		m_IconThird.SetActive(value: false);
		m_IconFourth.SetActive(value: false);
		switch (currentPlace)
		{
		case 1:
			m_IconFirst.SetActive(value: true);
			break;
		case 2:
			m_IconSecond.SetActive(value: true);
			break;
		case 3:
			m_IconThird.SetActive(value: true);
			break;
		case 4:
			m_IconFourth.SetActive(value: true);
			break;
		}
	}
}
