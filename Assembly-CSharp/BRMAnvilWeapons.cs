using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class BRMAnvilWeapons : MonoBehaviour
{
	[Serializable]
	public class AnvilWeapon
	{
		public PlayMakerFSM m_FSM;

		public List<string> m_Events;

		[HideInInspector]
		public int m_CurrentWeaponIndex;
	}

	public List<AnvilWeapon> m_Weapons;

	private int m_LastWeaponIndex;

	public void RandomWeaponEvent()
	{
		List<int> possibleWeapons = new List<int>();
		for (int idx = 0; idx < m_Weapons.Count; idx++)
		{
			if (idx != m_LastWeaponIndex)
			{
				possibleWeapons.Add(idx);
			}
		}
		if (m_Weapons.Count > 0 && possibleWeapons.Count > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, possibleWeapons.Count);
			AnvilWeapon weapon = m_Weapons[possibleWeapons[randomIndex]];
			m_LastWeaponIndex = possibleWeapons[randomIndex];
			weapon.m_FSM.SendEvent(weapon.m_Events[RandomSubWeapon(weapon)]);
		}
	}

	public int RandomSubWeapon(AnvilWeapon weapon)
	{
		List<int> possibleSubWeapons = new List<int>();
		for (int idx = 0; idx < weapon.m_Events.Count; idx++)
		{
			if (idx != weapon.m_CurrentWeaponIndex)
			{
				possibleSubWeapons.Add(idx);
			}
		}
		int randomIndex = UnityEngine.Random.Range(0, possibleSubWeapons.Count);
		weapon.m_CurrentWeaponIndex = possibleSubWeapons[randomIndex];
		return possibleSubWeapons[randomIndex];
	}
}
