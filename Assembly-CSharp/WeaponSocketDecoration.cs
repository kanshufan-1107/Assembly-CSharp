using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class WeaponSocketDecoration : MonoBehaviour
{
	public List<WeaponSocketRequirement> m_VisibilityRequirements;

	public bool IsShown()
	{
		return GetComponent<Renderer>().enabled;
	}

	public void UpdateVisibility()
	{
		if (AreVisibilityRequirementsMet())
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public bool AreVisibilityRequirementsMet()
	{
		Map<int, Player> playerMap = GameState.Get().GetPlayerMap();
		if (playerMap == null)
		{
			return false;
		}
		if (m_VisibilityRequirements == null)
		{
			return false;
		}
		foreach (WeaponSocketRequirement requirement in m_VisibilityRequirements)
		{
			bool foundPlayer = false;
			foreach (Player player in playerMap.Values)
			{
				if (requirement.m_Side == player.GetSide())
				{
					Entity heroEntity = player.GetHero();
					if (heroEntity == null)
					{
						Debug.LogWarning($"WeaponSocketDecoration.AreVisibilityRequirementsMet() - player {player} has no hero");
						return false;
					}
					if (requirement.m_HasWeapon != WeaponSocketMgr.ShouldSeeWeaponSocket(heroEntity.GetClass()))
					{
						return false;
					}
					foundPlayer = true;
				}
			}
			if (!foundPlayer)
			{
				return false;
			}
		}
		return true;
	}

	public void Show()
	{
		RenderUtils.EnableRenderers(base.gameObject, enable: true);
	}

	public void Hide()
	{
		RenderUtils.EnableRenderers(base.gameObject, enable: false);
	}
}
