using UnityEngine;

internal class PlayerLeaderboardDuosTeam : PlayerLeaderboardTeam
{
	[Header("Duos Team Items")]
	[SerializeField]
	private GameObject m_TopTileBone;

	[SerializeField]
	private GameObject m_BottomTileBone;

	protected override void OnMemberAdded(PlayerData newMember)
	{
		if (m_teamMembers.Count == m_TeamSize)
		{
			UpdatePlayerOrder(animate: false);
		}
	}

	public override void UpdatePlayerOrder(bool animate = true)
	{
		if (m_teamMembers.Count == m_TeamSize)
		{
			bool changed = true;
			if (m_teamMembers[0].Entry.Entity.GetRealTimePlayerFightsFirst() == 1 || (m_teamMembers[1].Entry.Entity.GetRealTimePlayerFightsFirst() == 0 && m_teamMembers[0].Entry.Entity.GetController().GetRealTimePlayerFightsFirst() == 1))
			{
				changed = false;
			}
			else
			{
				PlayerData temp = m_teamMembers[0];
				m_teamMembers[0] = m_teamMembers[1];
				m_teamMembers[1] = temp;
			}
			if (animate && changed)
			{
				AnimatePositionSwap();
			}
			else
			{
				SetPlayerPositions();
			}
		}
	}

	private void AnimatePositionSwap()
	{
		if (m_teamMembers.Count == m_TeamSize)
		{
			iTween.MoveTo(m_teamMembers[0].Entry.gameObject, iTween.Hash("position", m_TopTileBone.transform.localPosition, "time", 0.25f, "islocal", true));
			iTween.MoveTo(m_teamMembers[1].Entry.gameObject, iTween.Hash("position", m_BottomTileBone.transform.localPosition, "time", 0.25f, "islocal", true));
		}
	}

	private void SetPlayerPositions()
	{
		if (m_teamMembers.Count == m_TeamSize)
		{
			m_teamMembers[0].Entry.gameObject.transform.localPosition = m_TopTileBone.transform.localPosition;
			m_teamMembers[1].Entry.gameObject.transform.localPosition = m_BottomTileBone.transform.localPosition;
		}
	}

	public override void SetPlaceIcon(int currentPlace)
	{
		m_IconFirst.SetActive(value: false);
		m_IconSecond.SetActive(value: false);
		switch (currentPlace)
		{
		case 1:
			m_IconFirst.SetActive(value: true);
			break;
		case 2:
			m_IconSecond.SetActive(value: true);
			break;
		}
	}

	public override void SetCurrentHealth(float healthPercent)
	{
		SetHealthBarActive(active: true);
		SetSkullIconActive(healthPercent == 0f);
		if (healthPercent == 0f)
		{
			m_HealthBar.m_barIntensity = 0f;
		}
		m_HealthBar.SetProgressBar(healthPercent);
	}
}
